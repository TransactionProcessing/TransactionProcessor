using EstateManagement.Client;
using EstateManagement.DataTransferObjects.Responses.Contract;
using EstateManagement.DataTransferObjects.Responses.Merchant;
using MediatR;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using SecurityService.Client;
using SecurityService.DataTransferObjects.Responses;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using Address = EstateManagement.DataTransferObjects.Requests.Merchant.Address;
using CalculationType = TransactionProcessor.DataTransferObjects.Responses.Contract.CalculationType;
using Contact = EstateManagement.DataTransferObjects.Requests.Merchant.Contact;
using FeeType = TransactionProcessor.DataTransferObjects.Responses.Contract.FeeType;
using ProductType = TransactionProcessor.DataTransferObjects.Responses.Contract.ProductType;

namespace TransactionProcessor.Controllers;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Common;
using DataTransferObjects;
using EventStore.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectionEngine.Models;
using ProjectionEngine.Repository;
using ProjectionEngine.State;
using Shared.EventStore.EventStore;
using Shared.Exceptions;
using Shared.General;
using Swashbuckle.AspNetCore.Annotations;
using TransactionProcessor.Database.Entities;

[ExcludeFromCodeCoverage]
[Route(MerchantController.ControllerRoute)]
[ApiController]
[Authorize]
public class MerchantController : ControllerBase
{
    private readonly IProjectionStateRepository<MerchantBalanceState> MerchantBalanceStateRepository;

    private readonly IEventStoreContext EventStoreContext;
    private readonly IMediator Mediator;
    private readonly IEstateClient EstateClient;
    private readonly ISecurityServiceClient SecurityServiceClient;

    private readonly ITransactionProcessorReadRepository TransactionProcessorReadRepository;

    public MerchantController(IProjectionStateRepository<MerchantBalanceState> merchantBalanceStateRepository,
                              ITransactionProcessorReadRepository transactionProcessorReadRepository,
                              IEventStoreContext eventStoreContext,
                              IMediator mediator,
                              IEstateClient estateClient,
                              ISecurityServiceClient securityServiceClient) {
        this.MerchantBalanceStateRepository = merchantBalanceStateRepository;
        this.TransactionProcessorReadRepository = transactionProcessorReadRepository;
        this.EventStoreContext = eventStoreContext;
        this.Mediator = mediator;
        this.EstateClient = estateClient;
        this.SecurityServiceClient = securityServiceClient;
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
        Claim estateIdClaim = ClaimsHelper.GetUserClaim(GetUser(), "EstateId", estateId.ToString());

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
    private TokenResponse TokenResponse;
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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.CreateMerchantRequest {
            Name = createMerchantRequest.Name,
            Address = new Address {
                AddressLine1 = createMerchantRequest.Address.AddressLine1,
                AddressLine2 = createMerchantRequest.Address.AddressLine2,
                AddressLine3 = createMerchantRequest.Address.AddressLine3,
                AddressLine4 = createMerchantRequest.Address.AddressLine4,
                Country = createMerchantRequest.Address.Country,
                PostalCode = createMerchantRequest.Address.PostalCode,
                Region = createMerchantRequest.Address.Region,
                Town = createMerchantRequest.Address.Town
            },
            Contact = new Contact { ContactName = createMerchantRequest.Contact.ContactName, EmailAddress = createMerchantRequest.Contact.EmailAddress, PhoneNumber = createMerchantRequest.Contact.PhoneNumber },
            CreatedDateTime = createMerchantRequest.CreatedDateTime,
            MerchantId = createMerchantRequest.MerchantId,
            SettlementSchedule = (SettlementSchedule)createMerchantRequest.SettlementSchedule
        };
        var result = await this.EstateClient.CreateMerchant(this.TokenResponse.AccessToken, estateId, estateClientRequest, cancellationToken);

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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.AssignOperatorRequest
        {
            OperatorId = assignOperatorRequest.OperatorId,
            MerchantNumber = assignOperatorRequest.MerchantNumber,
            TerminalNumber = assignOperatorRequest.TerminalNumber
        };
        var result = await this.EstateClient.AssignOperatorToMerchant(this.TokenResponse.AccessToken, estateId, merchantId, estateClientRequest, cancellationToken);

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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var result = await this.EstateClient.RemoveOperatorFromMerchant(this.TokenResponse.AccessToken, estateId, merchantId, operatorId, cancellationToken);

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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.AddMerchantDeviceRequest
        {
            DeviceIdentifier = addMerchantDeviceRequest.DeviceIdentifier
        };

        var result = await this.EstateClient.AddDeviceToMerchant(this.TokenResponse.AccessToken, estateId, merchantId, estateClientRequest, cancellationToken);


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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.AddMerchantContractRequest
        {
            ContractId = addMerchantContractRequest.ContractId
        };
        var result = await this.EstateClient.AddContractToMerchant(this.TokenResponse.AccessToken, estateId, merchantId, estateClientRequest, cancellationToken);
        
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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var result = await this.EstateClient.RemoveContractFromMerchant(this.TokenResponse.AccessToken, estateId, merchantId, contractId, cancellationToken);

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

        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.CreateMerchantUserRequest
        {
            EmailAddress = createMerchantUserRequest.EmailAddress,
            FamilyName = createMerchantUserRequest.FamilyName,
            GivenName = createMerchantUserRequest.GivenName,
            Password = createMerchantUserRequest.Password,
            MiddleName = createMerchantUserRequest.MiddleName,
        };

        var result = await this.EstateClient.CreateMerchantUser(this.TokenResponse.AccessToken, estateId, merchantId, estateClientRequest, cancellationToken);

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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.MakeMerchantDepositRequest
        {
            Amount = makeMerchantDepositRequest.Amount,
            DepositDateTime = makeMerchantDepositRequest.DepositDateTime,
            Reference = makeMerchantDepositRequest.Reference
        };

        var result = await this.EstateClient.MakeMerchantDeposit(this.TokenResponse.AccessToken, estateId, merchantId, estateClientRequest, cancellationToken);


        // return the result
        return result.ToActionResultX();
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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var estateClientRequest= new EstateManagement.DataTransferObjects.Requests.Merchant.MakeMerchantWithdrawalRequest
        {
            Amount = makeMerchantWithdrawalRequest.Amount,
            Reference = makeMerchantWithdrawalRequest.Reference,
            WithdrawalDateTime = makeMerchantWithdrawalRequest.WithdrawalDateTime
        };

        var result = await this.EstateClient.MakeMerchantWithdrawal(this.TokenResponse.AccessToken, estateId, merchantId, estateClientRequest, cancellationToken);

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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.SwapMerchantDeviceRequest
        {
            NewDeviceIdentifier = swapMerchantDeviceRequest.NewDeviceIdentifier
        };
        var result = await this.EstateClient.SwapDeviceForMerchant(this.TokenResponse.AccessToken, estateId, merchantId, deviceIdentifier, estateClientRequest, cancellationToken);


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

        Claim merchantIdClaim = ClaimsHelper.GetUserClaim(GetUser(), "MerchantId");

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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        Result<MerchantResponse> result = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);

        if (result.IsFailed)
            return result.ToActionResultX();

        var m = ConvertMerchant(result.Data);

        return Result.Success(m).ToActionResultX();
    }

    private static DataTransferObjects.Responses.Merchant.MerchantResponse ConvertMerchant(MerchantResponse merchant) {
        var m = new DataTransferObjects.Responses.Merchant.MerchantResponse()
        {
            Operators = new List<DataTransferObjects.Responses.Merchant.MerchantOperatorResponse>(),
            MerchantId = merchant.MerchantId,
            SettlementSchedule = (DataTransferObjects.Responses.Merchant.SettlementSchedule)merchant.SettlementSchedule,
            EstateId = merchant.EstateId,
            EstateReportingId = merchant.EstateReportingId,
            Addresses = new(),
            Contacts = new(),
            Contracts = new(),
            Devices = new Dictionary<Guid, String>(),
            MerchantName = merchant.MerchantName,
            MerchantReference = merchant.MerchantReference,
            MerchantReportingId = merchant.MerchantReportingId,
            NextStatementDate = merchant.NextStatementDate
        };

        if (merchant.Addresses != null) {
            foreach (AddressResponse addressResponse in merchant.Addresses) {
                m.Addresses.Add(new DataTransferObjects.Responses.Merchant.AddressResponse {
                    AddressLine3 = addressResponse.AddressLine3,
                    AddressLine4 = addressResponse.AddressLine4,
                    AddressLine2 = addressResponse.AddressLine2,
                    Country = addressResponse.Country,
                    PostalCode = addressResponse.PostalCode,
                    Region = addressResponse.Region,
                    Town = addressResponse.Town,
                    AddressLine1 = addressResponse.AddressLine1,
                    AddressId = addressResponse.AddressId
                });
            }
        }

        if (merchant.Contacts != null)
        {
            foreach (ContactResponse contactResponse in merchant.Contacts)
            {
                m.Contacts.Add(new DataTransferObjects.Responses.Contract.ContactResponse
                {
                    ContactId = contactResponse.ContactId,
                    ContactName = contactResponse.ContactName,
                    ContactEmailAddress = contactResponse.ContactEmailAddress,
                    ContactPhoneNumber = contactResponse.ContactPhoneNumber
                });
            }
        }

        if (merchant.Contracts != null) {
            foreach (MerchantContractResponse merchantContractResponse in merchant.Contracts) {
                var mcr = new DataTransferObjects.Responses.Merchant.MerchantContractResponse { ContractId = merchantContractResponse.ContractId, ContractProducts = new List<Guid>(), IsDeleted = merchantContractResponse.IsDeleted, };
                foreach (Guid contractProduct in merchantContractResponse.ContractProducts) {
                    mcr.ContractProducts.Add(contractProduct);
                }

                m.Contracts.Add(mcr);
            }
        }

        if (merchant.Devices != null) {
            foreach (KeyValuePair<Guid, String> keyValuePair in merchant.Devices) {
                m.Devices.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        if (merchant.Operators != null) {
            foreach (MerchantOperatorResponse merchantOperatorResponse in merchant.Operators) {
                m.Operators.Add(new() { OperatorId = merchantOperatorResponse.OperatorId, MerchantNumber = merchantOperatorResponse.MerchantNumber, TerminalNumber = merchantOperatorResponse.TerminalNumber });
            }
        }

        return m;
    }

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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        Result<List<ContractResponse>> result = await this.EstateClient.GetMerchantContracts(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);

        if (result.IsFailed)
            return result.ToActionResultX();

        List<DataTransferObjects.Responses.Contract.ContractResponse> responses = new();

        foreach (ContractResponse contractResponse in result.Data) {
            var cr = new DataTransferObjects.Responses.Contract.ContractResponse {
                Description = contractResponse.Description,
                EstateId = contractResponse.EstateId,
                EstateReportingId = contractResponse.EstateReportingId,
                ContractId = contractResponse.ContractId,
                Products = new(),
                ContractReportingId = contractResponse.ContractReportingId,
                OperatorId = contractResponse.OperatorId,
                OperatorName = contractResponse.OperatorName
            };

            foreach (EstateManagement.DataTransferObjects.Responses.Contract.ContractProduct contractResponseProduct in contractResponse.Products) {
                var p = new DataTransferObjects.Responses.Contract.ContractProduct
                {
                    ProductId = contractResponseProduct.ProductId,
                    Value = contractResponseProduct.Value,
                    ProductType = (ProductType)contractResponseProduct.ProductType,
                    DisplayText = contractResponseProduct.DisplayText,
                    Name = contractResponseProduct.Name,
                    ProductReportingId = contractResponseProduct.ProductReportingId,
                    TransactionFees = new()
                };
                foreach (EstateManagement.DataTransferObjects.Responses.Contract.ContractProductTransactionFee contractProductTransactionFee in contractResponseProduct.TransactionFees) {
                    p.TransactionFees.Add(new DataTransferObjects.Responses.Contract.ContractProductTransactionFee
                    {
                        FeeType = (DataTransferObjects.Responses.Contract.FeeType)contractProductTransactionFee.FeeType,
                        CalculationType = (DataTransferObjects.Responses.Contract.CalculationType)contractProductTransactionFee.CalculationType,
                        Value = contractProductTransactionFee.Value,
                        Description = contractProductTransactionFee.Description,
                        TransactionFeeReportingId = contractProductTransactionFee.TransactionFeeReportingId,
                        TransactionFeeId = contractProductTransactionFee.TransactionFeeId
                    });
                }
                cr.Products.Add(p);

            }

            responses.Add(cr);
        }

        return Result.Success(responses).ToActionResultX();
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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var result = await this.EstateClient.GetMerchants(this.TokenResponse.AccessToken, estateId, cancellationToken);

        if (result.IsFailed)
            return result.ToActionResultX();
        List<DataTransferObjects.Responses.Merchant.MerchantResponse> responses = new();
        foreach (MerchantResponse merchantResponse in result.Data) {
            responses.Add(ConvertMerchant(merchantResponse));
        }
        
        return Result.Success(responses).ToActionResultX();
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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var result = await this.EstateClient.GetTransactionFeesForProduct(this.TokenResponse.AccessToken, estateId, merchantId, contractId, productId, cancellationToken);

        if (result.IsFailed)
            return result.ToActionResultX();

        List<DataTransferObjects.Responses.Contract.ContractProductTransactionFee> responses = new();

        foreach (EstateManagement.DataTransferObjects.Responses.Contract.ContractProductTransactionFee contractProductTransactionFee in result.Data) {
            responses.Add(new DataTransferObjects.Responses.Contract.ContractProductTransactionFee {
                CalculationType = (CalculationType)contractProductTransactionFee.CalculationType,
                Value = contractProductTransactionFee.Value,
                Description = contractProductTransactionFee.Description,
                FeeType = (FeeType)contractProductTransactionFee.FeeType,
                TransactionFeeReportingId = contractProductTransactionFee.TransactionFeeReportingId,
                TransactionFeeId = contractProductTransactionFee.TransactionFeeId
            });
        }
        
        return Result.Success(responses).ToActionResultX();
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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.UpdateMerchantRequest
        {
            Name = updateMerchantRequest.Name,
            SettlementSchedule = (SettlementSchedule)updateMerchantRequest.SettlementSchedule
        };
        
        var result = await this.EstateClient.UpdateMerchant(this.TokenResponse.AccessToken, estateId, merchantId, estateClientRequest, cancellationToken);
        
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

        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.Address
        {
            AddressLine1 = addAddressRequest.AddressLine1,
            AddressLine2 = addAddressRequest.AddressLine2,
            AddressLine3 = addAddressRequest.AddressLine3,
            AddressLine4 = addAddressRequest.AddressLine4,
            Country = addAddressRequest.Country,
            PostalCode = addAddressRequest.PostalCode,
            Region = addAddressRequest.Region,
            Town = addAddressRequest.Town
        };

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var result = await this.EstateClient.AddMerchantAddress(this.TokenResponse.AccessToken, estateId, merchantId, estateClientRequest, cancellationToken);

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

        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.Address
        {
            AddressLine1 = updateAddressRequest.AddressLine1,
            AddressLine2 = updateAddressRequest.AddressLine2,
            AddressLine3 = updateAddressRequest.AddressLine3,
            AddressLine4 = updateAddressRequest.AddressLine4,
            Country = updateAddressRequest.Country,
            PostalCode = updateAddressRequest.PostalCode,
            Region = updateAddressRequest.Region,
            Town = updateAddressRequest.Town
        };

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var result = await this.EstateClient.UpdateMerchantAddress(this.TokenResponse.AccessToken, estateId, merchantId,addressId, estateClientRequest, cancellationToken);

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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.Contact
        {
            ContactName = addContactRequest.ContactName,
            EmailAddress = addContactRequest.EmailAddress,
            PhoneNumber = addContactRequest.PhoneNumber
        };

        var result = await this.EstateClient.AddMerchantContact(this.TokenResponse.AccessToken, estateId, merchantId, estateClientRequest, cancellationToken);

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

        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
        var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Merchant.Contact
        {
            ContactName = updateContactRequest.ContactName,
            EmailAddress = updateContactRequest.EmailAddress,
            PhoneNumber = updateContactRequest.PhoneNumber
        };

        var result = await this.EstateClient.UpdateMerchantContact(this.TokenResponse.AccessToken, estateId, merchantId, contactId,estateClientRequest, cancellationToken);

        // return the result
        return result.ToActionResultX();
    }
}