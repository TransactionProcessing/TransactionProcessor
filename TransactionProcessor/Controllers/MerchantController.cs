using MediatR;
using SecurityService.Client;
using Shared.Results;
using Shared.Results.Web;
using SimpleResults;
using System.Security.Cryptography;
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
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventStore;
using Shared.Exceptions;
using Shared.General;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
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

    private bool PerformStandardChecks(Guid estateId)
    {
        // Get the Estate Id claim from the user
        Result<Claim> estateIdClaimResult = ClaimsHelper.GetUserClaim(GetUser(), "EstateId",estateId.ToString());
        if (estateIdClaimResult.IsFailed)
            return false; // TODO: Shoudl this be a result?
        Claim estateIdClaim = estateIdClaimResult.Data;

        string estateRoleName = Environment.GetEnvironmentVariable("EstateRoleName");
        if (ClaimsHelper.IsUserRolesValid(GetUser(), new[] { string.IsNullOrEmpty(estateRoleName) ? "Estate" : estateRoleName }) == false)
        {
            return false;
        }

        if (ClaimsHelper.ValidateRouteParameter(estateId, estateIdClaim) == false)
        {
            return false;
        }

        return true;
    }
    //private TokenResponse TokenResponse;
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> CreateMerchant([FromRoute] Guid estateId,
                                                    [FromBody] CreateMerchantRequest createMerchantRequest,
                                                    CancellationToken cancellationToken)
    {

        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.CreateMerchantCommand command = new(estateId, createMerchantRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();

    }

    [HttpPatch]
    [Route("{merchantId}/operators")]
    [ProducesResponseType(typeof(AssignOperatorResponse), 201)]
    public async Task<IActionResult> AssignOperator([FromRoute] Guid estateId,
                                                    [FromRoute] Guid merchantId,
                                                    AssignOperatorRequest assignOperatorRequest,
                                                    CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.AssignOperatorToMerchantCommand command = new(estateId, merchantId, assignOperatorRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [HttpDelete]
    [Route("{merchantId}/operators/{operatorId}")]
    public async Task<IActionResult> RemoveOperator([FromRoute] Guid estateId,
                                                    [FromRoute] Guid merchantId,
                                                    [FromRoute] Guid operatorId,
                                                    CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.RemoveOperatorFromMerchantCommand command = new(estateId, merchantId, operatorId);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [HttpPatch]
    [Route("{merchantId}/devices")]
    public async Task<IActionResult> AddDevice([FromRoute] Guid estateId,
                                               [FromRoute] Guid merchantId,
                                               [FromBody] AddMerchantDeviceRequest addMerchantDeviceRequest,
                                               CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.AddMerchantDeviceCommand command = new(estateId, merchantId, addMerchantDeviceRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [HttpPatch]
    [Route("{merchantId}/contracts")]
    public async Task<IActionResult> AddContract([FromRoute] Guid estateId,
                                                 [FromRoute] Guid merchantId,
                                                 [FromBody] AddMerchantContractRequest addMerchantContractRequest,
                                                 CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.AddMerchantContractCommand command = new(estateId, merchantId, addMerchantContractRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [HttpDelete]
    [Route("{merchantId}/contracts/{contractId}")]
    public async Task<IActionResult> RemoveContract([FromRoute] Guid estateId,
                                                    [FromRoute] Guid merchantId,
                                                    [FromRoute] Guid contractId,
                                                    CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.RemoveMerchantContractCommand command = new(estateId, merchantId, contractId);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [HttpPatch]
    [Route("{merchantId}/users")]
    public async Task<IActionResult> CreateMerchantUser([FromRoute] Guid estateId,
                                                        [FromRoute] Guid merchantId,
                                                        [FromBody] CreateMerchantUserRequest createMerchantUserRequest,
                                                        CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.CreateMerchantUserCommand command = new(estateId, merchantId, createMerchantUserRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [HttpPost]
    [Route("{merchantId}/deposits")]
    public async Task<IActionResult> MakeDeposit([FromRoute] Guid estateId,
                                                 [FromRoute] Guid merchantId,
                                                 [FromBody] MakeMerchantDepositRequest makeMerchantDepositRequest,
                                                 CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        // This will always be a manual deposit as auto ones come in via another route
        MerchantCommands.MakeMerchantDepositCommand command = new(estateId, merchantId, MerchantDepositSource.Manual, makeMerchantDepositRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToActionResultX();
        }

        // Now we need to record the deposit against the balance
        String depositData = $"{makeMerchantDepositRequest.DepositDateTime:yyyyMMdd hh:mm:ss.fff}-{makeMerchantDepositRequest.Reference}-{makeMerchantDepositRequest.Amount:N2}-{MerchantDepositSource.Manual}";
        Guid depositId = GenerateGuidFromString(depositData);
        MerchantBalanceCommands.RecordDepositCommand recordDepositCommand = new(estateId, merchantId, depositId, makeMerchantDepositRequest.Amount, makeMerchantDepositRequest.DepositDateTime);

        // Route the command
        result = await Mediator.Send(recordDepositCommand, cancellationToken);
        
        // return the result
        return result.ToActionResultX();
    }

    public static Guid GenerateGuidFromString(String input)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            //Generate hash from the key
            Byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            Byte[] j = bytes.Skip(Math.Max(0, bytes.Count() - 16)).ToArray(); //Take last 16

            //Create our Guid.
            return new Guid(j);
        }
    }

    [HttpPost]
    [Route("{merchantId}/withdrawals")]
    //[SwaggerResponse(201, "Created", typeof(MakeMerchantDepositResponse))]
    //[SwaggerResponseExample(201, typeof(MakeMerchantDepositResponseExample))]
    public async Task<IActionResult> MakeWithdrawal([FromRoute] Guid estateId,
                                                    [FromRoute] Guid merchantId,
                                                    [FromBody] MakeMerchantWithdrawalRequest makeMerchantWithdrawalRequest,
                                                    CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.MakeMerchantWithdrawalCommand command = new(estateId, merchantId, makeMerchantWithdrawalRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToActionResultX();
        }

        // Now we need to record the deposit against the balance
        String depositData = $"{makeMerchantWithdrawalRequest.WithdrawalDateTime:yyyyMMdd hh:mm:ss.fff}-{makeMerchantWithdrawalRequest.Amount:N2}";
        Guid withdrawalId = GenerateGuidFromString(depositData);
        MerchantBalanceCommands.RecordWithdrawalCommand recordWithdrawalCommand = new(estateId, merchantId, withdrawalId, makeMerchantWithdrawalRequest.Amount, makeMerchantWithdrawalRequest.WithdrawalDateTime);

        // Route the command
        result = await Mediator.Send(recordWithdrawalCommand, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [HttpPatch]
    [Route("{merchantId}/devices/{deviceIdentifier}")]
    public async Task<IActionResult> SwapMerchantDevice([FromRoute] Guid estateId,
                                                        [FromRoute] Guid merchantId,
                                                        [FromRoute] string deviceIdentifier,
                                                        [FromBody] SwapMerchantDeviceRequest swapMerchantDeviceRequest,
                                                        CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.SwapMerchantDeviceCommand command = new(estateId, merchantId, deviceIdentifier, swapMerchantDeviceRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    private bool PerformMerchantUserChecks(Guid estateId, Guid merchantId)
    {

        if (PerformStandardChecks(estateId) == false)
        {
            return false;
        }

        string merchantRoleName = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MerchantRoleName"))
            ? "Merchant"
            : Environment.GetEnvironmentVariable("MerchantRoleName");

        if (GetUser().IsInRole(merchantRoleName) == false)
            return true;

        if (ClaimsHelper.IsUserRolesValid(GetUser(), new[] { merchantRoleName }) == false)
        {
            return false;
        }

        Result<Claim> getMerchantIdClaimResult = ClaimsHelper.GetUserClaim(GetUser(), "MerchantId");
        if (getMerchantIdClaimResult.IsFailed)
            return false; // TODO: Shoudl this be a result?
        Claim merchantIdClaim = getMerchantIdClaimResult.Data;
        if (ClaimsHelper.ValidateRouteParameter(merchantId, merchantIdClaim) == false)
        {
            return false;
        }

        return true;
    }

    [HttpGet]
    [Route("{merchantId}")]
    public async Task<IActionResult> GetMerchant([FromRoute] Guid estateId,
                                                 [FromRoute] Guid merchantId,
                                                 CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformMerchantUserChecks(estateId, merchantId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantQueries.GetMerchantQuery query = new(estateId, merchantId);

        // Route the query
        Result<Models.Merchant.Merchant> result = await Mediator.Send(query, cancellationToken);
        if (result.IsFailed)
            return result.ToActionResultX();
        return ModelFactory.ConvertFrom(result.Data).ToActionResultX();
    }

    //private static DataTransferObjects.Responses.Merchant.MerchantResponse ConvertMerchant(MerchantResponse merchant) {
    //    var m = new DataTransferObjects.Responses.Merchant.MerchantResponse()
    //    {
    //        Operators = new List<DataTransferObjects.Responses.Merchant.MerchantOperatorResponse>(),
    //        MerchantId = merchant.MerchantId,
    //        SettlementSchedule = (DataTransferObjects.Responses.Merchant.SettlementSchedule)merchant.SettlementSchedule,
    //        EstateId = merchant.EstateId,
    //        EstateReportingId = merchant.EstateReportingId,
    //        Addresses = new(),
    //        Contacts = new(),
    //        Contracts = new(),
    //        Devices = new Dictionary<Guid, String>(),
    //        MerchantName = merchant.MerchantName,
    //        MerchantReference = merchant.MerchantReference,
    //        MerchantReportingId = merchant.MerchantReportingId,
    //        NextStatementDate = merchant.NextStatementDate
    //    };

    //    if (merchant.Addresses != null) {
    //        foreach (AddressResponse addressResponse in merchant.Addresses) {
    //            m.Addresses.Add(new DataTransferObjects.Responses.Merchant.AddressResponse {
    //                AddressLine3 = addressResponse.AddressLine3,
    //                AddressLine4 = addressResponse.AddressLine4,
    //                AddressLine2 = addressResponse.AddressLine2,
    //                Country = addressResponse.Country,
    //                PostalCode = addressResponse.PostalCode,
    //                Region = addressResponse.Region,
    //                Town = addressResponse.Town,
    //                AddressLine1 = addressResponse.AddressLine1,
    //                AddressId = addressResponse.AddressId
    //            });
    //        }
    //    }

    //    if (merchant.Contacts != null)
    //    {
    //        foreach (ContactResponse contactResponse in merchant.Contacts)
    //        {
    //            m.Contacts.Add(new DataTransferObjects.Responses.Contract.ContactResponse
    //            {
    //                ContactId = contactResponse.ContactId,
    //                ContactName = contactResponse.ContactName,
    //                ContactEmailAddress = contactResponse.ContactEmailAddress,
    //                ContactPhoneNumber = contactResponse.ContactPhoneNumber
    //            });
    //        }
    //    }

    //    if (merchant.Contracts != null) {
    //        foreach (MerchantContractResponse merchantContractResponse in merchant.Contracts) {
    //            var mcr = new DataTransferObjects.Responses.Merchant.MerchantContractResponse { ContractId = merchantContractResponse.ContractId, ContractProducts = new List<Guid>(), IsDeleted = merchantContractResponse.IsDeleted, };
    //            foreach (Guid contractProduct in merchantContractResponse.ContractProducts) {
    //                mcr.ContractProducts.Add(contractProduct);
    //            }

    //            m.Contracts.Add(mcr);
    //        }
    //    }

    //    if (merchant.Devices != null) {
    //        foreach (KeyValuePair<Guid, String> keyValuePair in merchant.Devices) {
    //            m.Devices.Add(keyValuePair.Key, keyValuePair.Value);
    //        }
    //    }

    //    if (merchant.Operators != null) {
    //        foreach (MerchantOperatorResponse merchantOperatorResponse in merchant.Operators) {
    //            m.Operators.Add(new() {
    //                OperatorId = merchantOperatorResponse.OperatorId, 
    //                MerchantNumber = merchantOperatorResponse.MerchantNumber, 
    //                TerminalNumber = merchantOperatorResponse.TerminalNumber,
    //                IsDeleted = merchantOperatorResponse.IsDeleted,
    //                Name = merchantOperatorResponse.Name

    //            });
    //        }
    //    }

    //    return m;
    //}

    [Route("{merchantId}/contracts")]
    [HttpGet]
    public async Task<IActionResult> GetMerchantContracts([FromRoute] Guid estateId,
                                                          [FromRoute] Guid merchantId,
                                                          CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformMerchantUserChecks(estateId, merchantId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantQueries.GetMerchantContractsQuery query = new(estateId, merchantId);

        Result<List<Models.Contract.Contract>> result = await Mediator.Send(query, cancellationToken);

        if (result.IsFailed)
            return result.ToActionResultX();

        return ModelFactory.ConvertFrom(result.Data).ToActionResultX();
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetMerchants([FromRoute] Guid estateId,
                                                  CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantQueries.GetMerchantsQuery query = new(estateId);

        Result<List<Models.Merchant.Merchant>> result = await Mediator.Send(query, cancellationToken);

        return ModelFactory.ConvertFrom(result.Data).ToActionResultX();
    }

    [Route("{merchantId}/contracts/{contractId}/products/{productId}/transactionFees")]
    [HttpGet]
    public async Task<IActionResult> GetTransactionFeesForProduct([FromRoute] Guid estateId,
                                                                  [FromRoute] Guid merchantId,
                                                                  [FromRoute] Guid contractId,
                                                                  [FromRoute] Guid productId,
                                                                  CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformMerchantUserChecks(estateId, merchantId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantQueries.GetTransactionFeesForProductQuery query = new(estateId, merchantId, contractId, productId);

        Result<List<ContractProductTransactionFee>> transactionFeesResult = await Mediator.Send(query, cancellationToken);
        if (transactionFeesResult.IsFailed)
            return transactionFeesResult.ToActionResultX();
        List<ContractProductTransactionFee> transactionFees = transactionFeesResult.Data;
        return ModelFactory.ConvertFrom(transactionFees).ToActionResultX();
    }

    [HttpPatch]
    [Route("{merchantId}")]
    public async Task<IActionResult> UpdateMerchant([FromRoute] Guid estateId,
                                                    [FromRoute] Guid merchantId,
                                                    [FromBody] UpdateMerchantRequest updateMerchantRequest,
                                                    CancellationToken cancellationToken)
    {

        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.UpdateMerchantCommand command = new(estateId, merchantId, updateMerchantRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [Route("{merchantId}/addresses")]
    [HttpPatch]
    public async Task<IActionResult> AddMerchantAddress([FromRoute] Guid estateId,
                                                        [FromRoute] Guid merchantId,
                                                        [FromBody] DataTransferObjects.Requests.Merchant.Address addAddressRequest,
                                                        CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.AddMerchantAddressCommand command = new(estateId, merchantId, addAddressRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [Route("{merchantId}/addresses/{addressId}")]
    [HttpPatch]
    public async Task<IActionResult> UpdateMerchantAddress([FromRoute] Guid estateId,
                                                           [FromRoute] Guid merchantId,
                                                           [FromRoute] Guid addressId,
                                                           [FromBody] DataTransferObjects.Requests.Merchant.Address updateAddressRequest,
                                                           CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.UpdateMerchantAddressCommand command = new(estateId, merchantId, addressId, updateAddressRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [Route("{merchantId}/contacts")]
    [HttpPatch]
    public async Task<IActionResult> AddMerchantContact([FromRoute] Guid estateId,
                                                            [FromRoute] Guid merchantId,
                                                            [FromBody] DataTransferObjects.Requests.Merchant.Contact addContactRequest,
                                                            CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.AddMerchantContactCommand command = new(estateId, merchantId, addContactRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [Route("{merchantId}/contacts/{contactId}")]
    [HttpPatch]
    public async Task<IActionResult> UpdateMerchantContact([FromRoute] Guid estateId,
                                                           [FromRoute] Guid merchantId,
                                                           [FromRoute] Guid contactId,
                                                           [FromBody] DataTransferObjects.Requests.Merchant.Contact updateContactRequest,
                                                           CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.UpdateMerchantContactCommand command = new(estateId, merchantId, contactId, updateContactRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }

    [HttpPost]
    [Route("{merchantId}/statements")]
    public async Task<IActionResult> GenerateMerchantStatement([FromRoute] Guid estateId,
                                                               [FromRoute] Guid merchantId,
                                                               [FromBody] GenerateMerchantStatementRequest generateMerchantStatementRequest,
                                                               CancellationToken cancellationToken)
    {
        bool isRequestAllowed = PerformStandardChecks(estateId);
        if (isRequestAllowed == false)
        {
            return Forbid();
        }

        MerchantCommands.GenerateMerchantStatementCommand command = new(estateId, merchantId, generateMerchantStatementRequest);

        // Route the command
        Result result = await Mediator.Send(command, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }
}