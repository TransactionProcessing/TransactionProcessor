using MediatR;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.Controllers;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Common;
using DataTransferObjects;
using EstateManagement.Database.Entities;
using EventStore.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectionEngine.Models;
using ProjectionEngine.Repository;
using ProjectionEngine.State;
using Shared.EventStore.EventStore;
using Shared.Exceptions;
using Shared.General;
using Swashbuckle.AspNetCore.Annotations;

[ExcludeFromCodeCoverage]
[Route(MerchantController.ControllerRoute)]
[ApiController]
[Authorize]
public class MerchantController : ControllerBase
{
    private readonly IProjectionStateRepository<MerchantBalanceState> MerchantBalanceStateRepository;

    private readonly IEventStoreContext EventStoreContext;
    private readonly IMediator Mediator;

    private readonly ITransactionProcessorReadRepository TransactionProcessorReadRepository;

    public MerchantController(IProjectionStateRepository<MerchantBalanceState> merchantBalanceStateRepository,
                              ITransactionProcessorReadRepository transactionProcessorReadRepository,
                              IEventStoreContext eventStoreContext,
                              IMediator mediator) {
        this.MerchantBalanceStateRepository = merchantBalanceStateRepository;
        this.TransactionProcessorReadRepository = transactionProcessorReadRepository;
        this.EventStoreContext = eventStoreContext;
        this.Mediator = mediator;
    }

    #region Others

    /// <summary>
    /// The controller name
    /// </summary>
    public const String ControllerName = "merchants";

    /// <summary>
    /// The controller route
    /// </summary>
    private const String ControllerRoute = "api/estates/{estateId}/" + MerchantController.ControllerName;

    #endregion

    private Result PerformSecurityChecks(Guid estateId,Guid merchantId) {
        String estateRoleName = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("EstateRoleName"))
            ? "Estate"
            : Environment.GetEnvironmentVariable("EstateRoleName");
        String merchantRoleName = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MerchantRoleName"))
            ? "Merchant"
            : Environment.GetEnvironmentVariable("MerchantRoleName");

        if (ClaimsHelper.IsUserRolesValid(this.User, new[] { estateRoleName, merchantRoleName }) == false) {
            return Result.Forbidden();
        }

        Claim estateIdClaim = null;
        Claim merchantIdClaim = null;

        // Determine the users role
        if (this.User.IsInRole(estateRoleName))
        {
            // Estate user
            // Get the Estate Id claim from the user
            estateIdClaim = ClaimsHelper.GetUserClaim(this.User, "EstateId");
        }

        if (this.User.IsInRole(merchantRoleName))
        {
            // Get the merchant Id claim from the user
            estateIdClaim = ClaimsHelper.GetUserClaim(this.User, "EstateId");
            merchantIdClaim = ClaimsHelper.GetUserClaim(this.User, "MerchantId");
        }

        if (ClaimsHelper.ValidateRouteParameter(estateId, estateIdClaim) == false) {
            return Result.Forbidden();
        }

        if (ClaimsHelper.ValidateRouteParameter(merchantId, merchantIdClaim) == false) {
            return Result.Forbidden();
        }

        return Result.Success();
    }

    /// <summary>
    /// Gets the merchant balance.
    /// </summary>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="merchantId">The merchant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="Shared.Exceptions.NotFoundException">Merchant Balance details not found with estate Id {estateId} and merchant Id {merchantId}</exception>
    /// <exception cref="NotFoundException">Merchant Balance details not found with estate Id {estateId} and merchant Id {merchantId}</exception>
    [HttpGet]
    [Route("{merchantId}/balance")]
    [SwaggerResponse(200, "OK", typeof(MerchantBalanceResponse))]
    public async Task<IActionResult> GetMerchantBalance([FromRoute] Guid estateId,
                                                        [FromRoute] Guid merchantId,
                                                        CancellationToken cancellationToken) {

        Result securityChecksResult = PerformSecurityChecks(estateId, merchantId);
        if (securityChecksResult.Status == ResultStatus.Forbidden){
            return this.Forbid();
        }

        MerchantQueries.GetMerchantBalanceQuery query = new(estateId, merchantId);
        Result<MerchantBalanceState> getMerchantBalanceResult = await this.Mediator.Send(query, cancellationToken);

        if (getMerchantBalanceResult.IsFailed) {
            return getMerchantBalanceResult.ToActionResultX();
        }
        
        Result<MerchantBalanceResponse> result = Result.Success(new MerchantBalanceResponse
        {
            Balance = getMerchantBalanceResult.Data.Balance,
            MerchantId = merchantId,
            AvailableBalance = getMerchantBalanceResult.Data.AvailableBalance,
            EstateId = estateId
        });

        return result.ToActionResultX();
    }

    [HttpGet]
    [Route("{merchantId}/livebalance")]
    [SwaggerResponse(200, "OK", typeof(MerchantBalanceResponse))]
    public async Task<IActionResult> GetMerchantBalanceLive([FromRoute] Guid estateId,
                                                        [FromRoute] Guid merchantId,
                                                        CancellationToken cancellationToken)
    {
        Result securityChecksResult = PerformSecurityChecks(estateId, merchantId);
        if (securityChecksResult.Status == ResultStatus.Forbidden) {
            return this.Forbid();
        }

        MerchantQueries.GetMerchantLiveBalanceQuery query = new MerchantQueries.GetMerchantLiveBalanceQuery(merchantId);

        Result<MerchantBalanceProjectionState1> getLiveMerchantBalanceResult = await this.Mediator.Send(query, cancellationToken);

        if (getLiveMerchantBalanceResult.IsFailed)
        {
            return getLiveMerchantBalanceResult.ToActionResultX();
        }

        Result<MerchantBalanceResponse> result = Result.Success(new MerchantBalanceResponse
        {
            Balance = getLiveMerchantBalanceResult.Data.merchant.balance,
            MerchantId = merchantId,
            AvailableBalance = getLiveMerchantBalanceResult.Data.merchant.balance,
            EstateId = estateId
        });

        return result.ToActionResultX();
    }

    [HttpGet]
    [Route("{merchantId}/balancehistory")]
    public async Task<IActionResult> GetMerchantBalanceHistory([FromRoute] Guid estateId,
                                                               [FromRoute] Guid merchantId,
                                                               [FromQuery] DateTime startDate,
                                                               [FromQuery] DateTime endDate,
                                                               CancellationToken cancellationToken) {
        Result securityChecksResult = PerformSecurityChecks(estateId, merchantId);
        if (securityChecksResult.Status == ResultStatus.Forbidden)
        {
            return this.Forbid();
        }

        MerchantQueries.GetMerchantBalanceHistoryQuery query =
            new MerchantQueries.GetMerchantBalanceHistoryQuery(estateId, merchantId, startDate, endDate); 

        Result<List<MerchantBalanceChangedEntry>> getMerchantBalanceHistoryResult = await this.Mediator.Send(query, cancellationToken);
        if (getMerchantBalanceHistoryResult.IsFailed)
        {
            return getMerchantBalanceHistoryResult.ToActionResultX();
        }


        List<MerchantBalanceChangedEntryResponse> response = new List<MerchantBalanceChangedEntryResponse>();
        getMerchantBalanceHistoryResult.Data.ForEach(h => response.Add(new MerchantBalanceChangedEntryResponse {
                                                                                             Balance = h.Balance,
                                                                                             MerchantId = h.MerchantId,
                                                                                             EstateId = h.EstateId,
                                                                                             DateTime = h.DateTime,
                                                                                             ChangeAmount = h.ChangeAmount,
                                                                                             DebitOrCredit = h.DebitOrCredit,
                                                                                             OriginalEventId = h.OriginalEventId,
                                                                                             Reference = h.Reference,
                                                                                         }));

        Result<List<MerchantBalanceChangedEntryResponse>> result = Result.Success(response);

        return result.ToActionResultX();
    }
}