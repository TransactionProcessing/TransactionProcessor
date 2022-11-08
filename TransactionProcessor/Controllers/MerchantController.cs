namespace TransactionProcessor.Controllers;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Common;
using DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectionEngine.Models;
using ProjectionEngine.Repository;
using ProjectionEngine.State;
using Shared.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

[ExcludeFromCodeCoverage]
[Route(MerchantController.ControllerRoute)]
[ApiController]
[Authorize]
public class MerchantController : ControllerBase
{
    private readonly IProjectionStateRepository<MerchantBalanceState> MerchantBalanceStateRepository;

    private readonly ITransactionProcessorReadRepository TransactionProcessorReadRepository;

    public MerchantController(IProjectionStateRepository<MerchantBalanceState> merchantBalanceStateRepository,
                              ITransactionProcessorReadRepository transactionProcessorReadRepository) {
        this.MerchantBalanceStateRepository = merchantBalanceStateRepository;
        this.TransactionProcessorReadRepository = transactionProcessorReadRepository;
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
                                                        CancellationToken cancellationToken)
    {
        // Reject password tokens
        if (ClaimsHelper.IsPasswordToken(this.User))
        {
            return this.Forbid();
        }

        MerchantBalanceState merchantBalance = await this.MerchantBalanceStateRepository.Load(estateId, merchantId, cancellationToken);

        if (merchantBalance == null)
        {
            throw new NotFoundException($"Merchant Balance details not found with estate Id {estateId} and merchant Id {merchantId}");
        }

        MerchantBalanceResponse response= new MerchantBalanceResponse {
                                                                          Balance = merchantBalance.Balance,
                                                                          MerchantId = merchantId,
                                                                          AvailableBalance = merchantBalance.AvailableBalance,
                                                                          EstateId = estateId
                                                                      };

        return this.Ok(response);
    }

    [HttpGet]
    [Route("{merchantId}/balancehistory")]
    public async Task<IActionResult> GetMerchantBalanceHistory([FromRoute] Guid estateId,
                                                               [FromRoute] Guid merchantId,
                                                               [FromQuery] DateTime startDate,
                                                               [FromQuery] DateTime enddate,
                                                               CancellationToken cancellationToken) {

        // Reject password tokens
        if (ClaimsHelper.IsPasswordToken(this.User))
        {
            return this.Forbid();
        }

        List<MerchantBalanceChangedEntry> historyEntries = await this.TransactionProcessorReadRepository.GetMerchantBalanceHistory(estateId, merchantId, startDate, enddate, cancellationToken);
        List<MerchantBalanceChangedEntryResponse> response = new List<MerchantBalanceChangedEntryResponse>();
        historyEntries.ForEach(h => response.Add(new MerchantBalanceChangedEntryResponse {
                                                                                             Balance = h.Balance,
                                                                                             MerchantId = h.MerchantId,
                                                                                             EstateId = h.EstateId,
                                                                                             DateTime = h.DateTime,
                                                                                             ChangeAmount = h.ChangeAmount,
                                                                                             DebitOrCredit = h.DebitOrCredit,
                                                                                             OriginalEventId = h.OriginalEventId,
                                                                                             Reference = h.Reference,
                                                                                         }));

        return this.Ok(response);

    }
}