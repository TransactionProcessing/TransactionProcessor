using MediatR;
using SecurityService.Client;
using Shared.Results;
using Shared.Results.Web;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;
using TransactionProcessor.Factories;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.Controllers;

using DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectionEngine.Models;
using ProjectionEngine.Repository;
using ProjectionEngine.State;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.EventStore;
using Shared.Exceptions;
using Shared.General;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

[ExcludeFromCodeCoverage]
[Route(MerchantController.ControllerRoute)]
[ApiController]
[Authorize]
public class MerchantController : ControllerBase
{
    private readonly IMediator Mediator;

    public MerchantController(IMediator mediator) {
        this.Mediator = mediator;
    }

    #region Others

    /// <summary>
    /// The controller name
    /// </summary>
    private const String ControllerName = "merchants";

    /// <summary>
    /// The controller route
    /// </summary>
    private const String ControllerRoute = "api/estates/{estateId}/" + MerchantController.ControllerName;

    #endregion

    private const String MerchantRoleNameKeyName = "MerchantRoleName";
    private const String EstateRoleNameKeyName = "EstateRoleName";
    private Result PerformSecurityChecks(Guid estateId,Guid merchantId) {
        String estateRoleName = string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EstateRoleNameKeyName))
            ? "Estate"
            : Environment.GetEnvironmentVariable(EstateRoleNameKeyName);
        String merchantRoleName = string.IsNullOrEmpty(Environment.GetEnvironmentVariable(MerchantRoleNameKeyName))
            ? "Merchant"
            : Environment.GetEnvironmentVariable(MerchantRoleNameKeyName);

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
            Result<Claim> estateIdClaimResult = ClaimsHelper.GetUserClaim(GetUser(), "EstateId");
            if (estateIdClaimResult.IsFailed)
                return Result.Forbidden("User estate id claim is not valid");
            estateIdClaim = estateIdClaimResult.Data;
        }

        if (this.User.IsInRole(merchantRoleName))
        {
            // Get the merchant Id claim from the user
            Result<Claim> estateIdClaimResult = ClaimsHelper.GetUserClaim(GetUser(), "EstateId");
            if (estateIdClaimResult.IsFailed)
                return Result.Forbidden("User estate id claim is not valid");
            estateIdClaim = estateIdClaimResult.Data;
            Result<Claim> merchantIdClaimResult = ClaimsHelper.GetUserClaim(GetUser(), "MerchantId");
            if (estateIdClaimResult.IsFailed)
                return Result.Forbidden("User merchant id claim is not valid");
            merchantIdClaim = merchantIdClaimResult.Data;
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
    public async Task<IResult> GetMerchantBalance([FromRoute] Guid estateId,
                                                        [FromRoute] Guid merchantId,
                                                        CancellationToken cancellationToken) {

        Result securityChecksResult = PerformSecurityChecks(estateId, merchantId);
        if (securityChecksResult.Status == ResultStatus.Forbidden) {
            return ResponseFactory.FromResult(securityChecksResult);
        }

        MerchantQueries.GetMerchantBalanceQuery query = new(estateId, merchantId);
        Result<MerchantBalanceState> getMerchantBalanceResult = await this.Mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(getMerchantBalanceResult, r => {
            MerchantBalanceResponse response = new() {
                Balance = r.Balance, 
                MerchantId = r.MerchantId, 
                AvailableBalance = r.AvailableBalance, 
                EstateId = r.EstateId
            };
            return response;
        });
    }

    [HttpGet]
    [Route("{merchantId}/livebalance")]
    [SwaggerResponse(200, "OK", typeof(MerchantBalanceResponse))]
    public async Task<IResult> GetMerchantBalanceLive([FromRoute] Guid estateId,
                                                      [FromRoute] Guid merchantId,
                                                      CancellationToken cancellationToken)
    {
        Result securityChecksResult = PerformSecurityChecks(estateId, merchantId);
        if (securityChecksResult.Status == ResultStatus.Forbidden) {
            return ResponseFactory.FromResult(securityChecksResult);
        }

        MerchantQueries.GetMerchantLiveBalanceQuery query = new MerchantQueries.GetMerchantLiveBalanceQuery(merchantId);

        Result<MerchantBalanceProjectionState1> getLiveMerchantBalanceResult = await this.Mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(getLiveMerchantBalanceResult, r => {
            MerchantBalanceResponse response = new() { 
                Balance = r.merchant.balance, 
                MerchantId = Guid.Parse(r.merchant.Id),
                AvailableBalance = r.merchant.balance, 
                EstateId = estateId};
            return response;
        });
    }

    [HttpGet]
    [Route("{merchantId}/balancehistory")]
    public async Task<IResult> GetMerchantBalanceHistory([FromRoute] Guid estateId,
                                                         [FromRoute] Guid merchantId,
                                                         [FromQuery] DateTime startDate,
                                                         [FromQuery] DateTime endDate,
                                                         CancellationToken cancellationToken) {
        Result securityChecksResult = PerformSecurityChecks(estateId, merchantId);
        if (securityChecksResult.Status == ResultStatus.Forbidden) {
            return ResponseFactory.FromResult(securityChecksResult);
        }

        MerchantQueries.GetMerchantBalanceHistoryQuery query = new(estateId, merchantId, startDate, endDate); 

        Result<List<MerchantBalanceChangedEntry>> getMerchantBalanceHistoryResult = await this.Mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(getMerchantBalanceHistoryResult, r => {
            List<MerchantBalanceChangedEntryResponse> response = new();
            r.ForEach(h => response.Add(new MerchantBalanceChangedEntryResponse
            {
                Balance = h.Balance,
                MerchantId = h.MerchantId,
                EstateId = h.EstateId,
                DateTime = h.DateTime,
                ChangeAmount = h.ChangeAmount,
                DebitOrCredit = h.DebitOrCredit,
                OriginalEventId = h.OriginalEventId,
                Reference = h.Reference,
            }));
            return response;
        });
    }

    private ClaimsPrincipal UserOverride;
    internal void SetContextOverride(HttpContext ctx)
    {
        UserOverride = ctx.User;
    }

    internal ClaimsPrincipal GetUser()
    {
        return UserOverride switch
        {
            null => HttpContext.User,
            _ => UserOverride
        };
    }

    private Result PerformStandardChecks(Guid estateId)
    {
        // Get the Estate Id claim from the user
        Result<Claim> estateIdClaimResult = ClaimsHelper.GetUserClaim(GetUser(), "EstateId",estateId.ToString());
        if (estateIdClaimResult.IsFailed)
            return ResultHelpers.CreateFailure(estateIdClaimResult);
        Claim estateIdClaim = estateIdClaimResult.Data;

        String estateRoleName = Environment.GetEnvironmentVariable(EstateRoleNameKeyName);
        if (ClaimsHelper.IsUserRolesValid(GetUser(), new[] { string.IsNullOrEmpty(estateRoleName) ? "Estate" : estateRoleName }) == false)
        {
            return Result.Invalid("User Roles not valid");
        }

        if (ClaimsHelper.ValidateRouteParameter(estateId, estateIdClaim) == false)
        {
            return Result.Invalid("Route parameter not valid");
        }

        return Result.Success();
    }

    [HttpPost]
    [Route("")]
    public async Task<IResult> CreateMerchant([FromRoute] Guid estateId,
                                              [FromBody] CreateMerchantRequest createMerchantRequest,
                                              CancellationToken cancellationToken)
    {

        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.CreateMerchantCommand command = new(estateId, createMerchantRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);

    }

    [HttpPatch]
    [Route("{merchantId}/operators")]
    [ProducesResponseType(typeof(AssignOperatorResponse), 201)]
    public async Task<IResult> AssignOperator([FromRoute] Guid estateId,
                                              [FromRoute] Guid merchantId,
                                              AssignOperatorRequest assignOperatorRequest,
                                              CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.AssignOperatorToMerchantCommand command = new(estateId, merchantId, assignOperatorRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [HttpDelete]
    [Route("{merchantId}/operators/{operatorId}")]
    public async Task<IResult> RemoveOperator([FromRoute] Guid estateId,
                                              [FromRoute] Guid merchantId,
                                              [FromRoute] Guid operatorId,
                                              CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.RemoveOperatorFromMerchantCommand command = new(estateId, merchantId, operatorId);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [HttpPatch]
    [Route("{merchantId}/devices")]
    public async Task<IResult> AddDevice([FromRoute] Guid estateId,
                                         [FromRoute] Guid merchantId,
                                         [FromBody] AddMerchantDeviceRequest addMerchantDeviceRequest,
                                         CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.AddMerchantDeviceCommand command = new(estateId, merchantId, addMerchantDeviceRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [HttpPatch]
    [Route("{merchantId}/contracts")]
    public async Task<IResult> AddContract([FromRoute] Guid estateId,
                                           [FromRoute] Guid merchantId,
                                           [FromBody] AddMerchantContractRequest addMerchantContractRequest,
                                           CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.AddMerchantContractCommand command = new(estateId, merchantId, addMerchantContractRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [HttpDelete]
    [Route("{merchantId}/contracts/{contractId}")]
    public async Task<IResult> RemoveContract([FromRoute] Guid estateId,
                                              [FromRoute] Guid merchantId,
                                              [FromRoute] Guid contractId,
                                              CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.RemoveMerchantContractCommand command = new(estateId, merchantId, contractId);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [HttpPatch]
    [Route("{merchantId}/users")]
    public async Task<IResult> CreateMerchantUser([FromRoute] Guid estateId,
                                                  [FromRoute] Guid merchantId,
                                                  [FromBody] CreateMerchantUserRequest createMerchantUserRequest,
                                                  CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.CreateMerchantUserCommand command = new(estateId, merchantId, createMerchantUserRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [HttpPost]
    [Route("{merchantId}/deposits")]
    public async Task<IResult> MakeDeposit([FromRoute] Guid estateId,
                                           [FromRoute] Guid merchantId,
                                           [FromBody] MakeMerchantDepositRequest makeMerchantDepositRequest,
                                           CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        // This will always be a manual deposit as auto ones come in via another route
        MerchantCommands.MakeMerchantDepositCommand command = new(estateId, merchantId, DataTransferObjects.Requests.Merchant.MerchantDepositSource.Manual, makeMerchantDepositRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [HttpPost]
    [Route("{merchantId}/withdrawals")]
    //[SwaggerResponse(201, "Created", typeof(MakeMerchantDepositResponse))]
    //[SwaggerResponseExample(201, typeof(MakeMerchantDepositResponseExample))]
    public async Task<IResult> MakeWithdrawal([FromRoute] Guid estateId,
                                              [FromRoute] Guid merchantId,
                                              [FromBody] MakeMerchantWithdrawalRequest makeMerchantWithdrawalRequest,
                                              CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.MakeMerchantWithdrawalCommand command = new(estateId, merchantId, makeMerchantWithdrawalRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [HttpPatch]
    [Route("{merchantId}/devices/{deviceIdentifier}")]
    public async Task<IResult> SwapMerchantDevice([FromRoute] Guid estateId,
                                                  [FromRoute] Guid merchantId,
                                                  [FromRoute] string deviceIdentifier,
                                                  [FromBody] SwapMerchantDeviceRequest swapMerchantDeviceRequest,
                                                  CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.SwapMerchantDeviceCommand command = new(estateId, merchantId, deviceIdentifier, swapMerchantDeviceRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    private Result PerformMerchantUserChecks(Guid estateId, Guid merchantId)
    {

        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResultHelpers.CreateFailure(isRequestAllowedResult);
        }

        string merchantRoleName = string.IsNullOrEmpty(Environment.GetEnvironmentVariable(MerchantRoleNameKeyName))
            ? "Merchant"
            : Environment.GetEnvironmentVariable(MerchantRoleNameKeyName);

        if (GetUser().IsInRole(merchantRoleName) == false)
            return Result.Success();

        if (ClaimsHelper.IsUserRolesValid(GetUser(), new[] { merchantRoleName }) == false) {
            return Result.Invalid("User Roles not valid");
        }

        Result<Claim> getMerchantIdClaimResult = ClaimsHelper.GetUserClaim(GetUser(), "MerchantId");
        if (getMerchantIdClaimResult.IsFailed)
            return ResultHelpers.CreateFailure(getMerchantIdClaimResult);

        Claim merchantIdClaim = getMerchantIdClaimResult.Data;
        if (ClaimsHelper.ValidateRouteParameter(merchantId, merchantIdClaim) == false) {
            return Result.Invalid("Route parameter not valid");
        }

        return Result.Success();
    }

    [HttpGet]
    [Route("{merchantId}")]
    public async Task<IResult> GetMerchant([FromRoute] Guid estateId,
                                           [FromRoute] Guid merchantId,
                                           CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformMerchantUserChecks(estateId, merchantId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantQueries.GetMerchantQuery query = new(estateId, merchantId);

        // Route the query
        Result<Models.Merchant.Merchant> result = await Mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
    
    [Route("{merchantId}/contracts")]
    [HttpGet]
    public async Task<IResult> GetMerchantContracts([FromRoute] Guid estateId,
                                                    [FromRoute] Guid merchantId,
                                                    CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformMerchantUserChecks(estateId, merchantId);
        if (isRequestAllowedResult.IsFailed)
        {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantQueries.GetMerchantContractsQuery query = new(estateId, merchantId);

        Result<List<Models.Contract.Contract>> result = await Mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    [HttpGet]
    [Route("")]
    public async Task<IResult> GetMerchants([FromRoute] Guid estateId,
                                            CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantQueries.GetMerchantsQuery query = new(estateId);

        Result<List<Models.Merchant.Merchant>> result = await Mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    [Route("{merchantId}/contracts/{contractId}/products/{productId}/transactionFees")]
    [HttpGet]
    public async Task<IResult> GetTransactionFeesForProduct([FromRoute] Guid estateId,
                                                            [FromRoute] Guid merchantId,
                                                            [FromRoute] Guid contractId,
                                                            [FromRoute] Guid productId,
                                                            CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformMerchantUserChecks(estateId, merchantId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantQueries.GetTransactionFeesForProductQuery query = new(estateId, merchantId, contractId, productId);

        Result<List<ContractProductTransactionFee>> transactionFeesResult = await Mediator.Send(query, cancellationToken);
        
        return ResponseFactory.FromResult(transactionFeesResult, ModelFactory.ConvertFrom);
    }

    [HttpPatch]
    [Route("{merchantId}")]
    public async Task<IResult> UpdateMerchant([FromRoute] Guid estateId,
                                              [FromRoute] Guid merchantId,
                                              [FromBody] UpdateMerchantRequest updateMerchantRequest,
                                              CancellationToken cancellationToken)
    {

        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.UpdateMerchantCommand command = new(estateId, merchantId, updateMerchantRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [Route("{merchantId}/addresses")]
    [HttpPatch]
    public async Task<IResult> AddMerchantAddress([FromRoute] Guid estateId,
                                                  [FromRoute] Guid merchantId,
                                                  [FromBody] DataTransferObjects.Requests.Merchant.Address addAddressRequest,
                                                  CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.AddMerchantAddressCommand command = new(estateId, merchantId, addAddressRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [Route("{merchantId}/addresses/{addressId}")]
    [HttpPatch]
    public async Task<IResult> UpdateMerchantAddress([FromRoute] Guid estateId,
                                                     [FromRoute] Guid merchantId,
                                                     [FromRoute] Guid addressId,
                                                     [FromBody] DataTransferObjects.Requests.Merchant.Address updateAddressRequest,
                                                     CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.UpdateMerchantAddressCommand command = new(estateId, merchantId, addressId, updateAddressRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [Route("{merchantId}/contacts")]
    [HttpPatch]
    public async Task<IResult> AddMerchantContact([FromRoute] Guid estateId,
                                                  [FromRoute] Guid merchantId,
                                                  [FromBody] DataTransferObjects.Requests.Merchant.Contact addContactRequest,
                                                  CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.AddMerchantContactCommand command = new(estateId, merchantId, addContactRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [Route("{merchantId}/contacts/{contactId}")]
    [HttpPatch]
    public async Task<IResult> UpdateMerchantContact([FromRoute] Guid estateId,
                                                     [FromRoute] Guid merchantId,
                                                     [FromRoute] Guid contactId,
                                                     [FromBody] DataTransferObjects.Requests.Merchant.Contact updateContactRequest,
                                                     CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.UpdateMerchantContactCommand command = new(estateId, merchantId, contactId, updateContactRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }

    [HttpPost]
    [Route("{merchantId}/statements")]
    public async Task<IResult> GenerateMerchantStatement([FromRoute] Guid estateId,
                                                         [FromRoute] Guid merchantId,
                                                         [FromBody] GenerateMerchantStatementRequest generateMerchantStatementRequest,
                                                         CancellationToken cancellationToken)
    {
        Result isRequestAllowedResult = PerformStandardChecks(estateId);
        if (isRequestAllowedResult.IsFailed) {
            return ResponseFactory.FromResult(isRequestAllowedResult);
        }

        MerchantCommands.GenerateMerchantStatementCommand command = new(estateId, merchantId, generateMerchantStatementRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return ResponseFactory.FromResult(result);
    }
}