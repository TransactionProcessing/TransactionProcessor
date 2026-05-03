using SecurityService.Client;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Requests.MerchantSchedule;
using TransactionProcessor.DataTransferObjects.Requests.Operator;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;
using TransactionProcessor.DataTransferObjects.Responses.Operator;
using AssignOperatorRequest = TransactionProcessor.DataTransferObjects.Requests.Merchant.AssignOperatorRequest;

namespace TransactionProcessor.Client;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientProxyBase;
using DataTransferObjects;
using Newtonsoft.Json;
using Shared.Results;

public class TransactionProcessorClient : ClientBase, ITransactionProcessorClient {
    private static String Serialise(Object arg)
    {
        return JsonConvert.SerializeObject(arg);
    }

    private static Object Deserialise(String arg, Type type)
    {
        return JsonConvert.DeserializeObject(arg, type);
    }

    #region Fields

    private readonly Func<String, String> BaseAddressResolver;

    #endregion

    #region Constructors

    public TransactionProcessorClient(Func<String, String> baseAddressResolver,
                                      HttpClient httpClient) : base(httpClient, Serialise, Deserialise)
    {
        this.BaseAddressResolver = baseAddressResolver;

        // Add the API version header
        //this.HttpClient.DefaultRequestHeaders.Add("api-version", "1.0");
    }

    #endregion

    public async Task<Result<EstateResponse>> GetEstate(String accessToken,
                                                        Guid estateId,
                                                        CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}");

        try {
            Result<EstateResponse> result =  await this.Get<EstateResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting estate Id {estateId}.", ex);

            throw exception;
        }
    }
    
    public async Task<Result> CreateEstate(String accessToken,
                                           CreateEstateRequest createEstateRequest,
                                           CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/estates/");

        try {
            var result = await this.Post(requestUri, createEstateRequest, accessToken, cancellationToken);
            
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating new estate {createEstateRequest.EstateName}.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<EstateResponse>>> GetEstates(String accessToken,
                                                               Guid estateId,
                                                               CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/all");

        try {
            Result<List<EstateResponse>> result = await this.Get<List<EstateResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting all estates for estate Id {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateFloatForContractProduct(String accessToken,
                                                            Guid estateId,
                                                            CreateFloatForContractProductRequest createFloatForContractProductRequest,
                                                            CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/floats");

        try {
            var result = await this.Post<CreateFloatForContractProductRequest>(requestUri, createFloatForContractProductRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error creating contract product float.", ex);

            throw exception;
        }
    }

    public async Task<Result> UpdateMerchantOpeningHours(String accessToken,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         MerchantOpeningRequest merchantOpeningRequest,
                                                         CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/opening");

        try
        {
            var result = await this.Patch<MerchantOpeningRequest>(requestUri, merchantOpeningRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex)
        {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error updating merchant opening hours.", ex);

            throw exception;
        }
    }

    public async Task<Result<MerchantBalanceResponse>> GetMerchantBalance(String accessToken,
                                                                          Guid estateId,
                                                                          Guid merchantId,
                                                                          CancellationToken cancellationToken,
                                                                          Boolean liveBalance = true) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/balance");

        if (liveBalance) requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/livebalance");

        try {
            Result<MerchantBalanceResponse> result = await this.Get<MerchantBalanceResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error getting merchant balance.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<MerchantBalanceChangedEntryResponse>>> GetMerchantBalanceHistory(String accessToken,
                                                                                                   Guid estateId,
                                                                                                   Guid merchantId,
                                                                                                   DateTime startDate,
                                                                                                   DateTime endDate,
                                                                                                   CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/balancehistory?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        try {
            Result<List<MerchantBalanceChangedEntryResponse>> result = await this.Get<List<MerchantBalanceChangedEntryResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error getting merchant balance history.", ex);

            throw exception;
        }
    }

    public async Task<Result<SettlementResponse>> GetSettlementByDate(String accessToken,
                                                                      DateTime settlementDate,
                                                                      Guid estateId,
                                                                      Guid merchantId,
                                                                      CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/settlements/{settlementDate.Date:yyyy-MM-dd}/merchants/{merchantId}/pending");
        try {
            var result = await this.Get<SettlementResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error getting settlement.", ex);

            throw exception;
        }
    }

    public async Task<Result<GetVoucherResponse>> GetVoucherByCode(String accessToken,
                                                                   Guid estateId,
                                                                   String voucherCode,
                                                                   CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/vouchers?estateId={estateId}&voucherCode={voucherCode}");

        try {
            var result = await this.Get<GetVoucherResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error getting voucher by code.", ex);
            
            throw exception;
        }
    }

    public async Task<Result<GetVoucherResponse>> GetVoucherByTransactionId(String accessToken,
                                                                            Guid estateId,
                                                                            Guid transactionId,
                                                                            CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/vouchers?estateId={estateId}&transactionId={transactionId}");

        try {
            var result = await this.Get<GetVoucherResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error getting voucher by transaction Id.", ex);
            
            throw exception;
        }
    }

    public async Task<Result<SerialisedMessage>> PerformTransaction(String accessToken,
                                                                    SerialisedMessage transactionRequest,
                                                                    CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/transactions");
        try {
            var result = await this.Post<SerialisedMessage, SerialisedMessage>(requestUri, transactionRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success(result.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error posting transaction.", ex);

            throw exception;
        }
    }

    public async Task<Result> ProcessSettlement(String accessToken,
                                                DateTime settlementDate,
                                                Guid estateId,
                                                Guid merchantId,
                                                CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/settlements/{settlementDate.Date:yyyy-MM-dd}/merchants/{merchantId}");

        try {
            Result result = await this.Post(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error processing settlement.", ex);

            throw exception;
        }
    }

    private String BuildRequestUrl(String route) {
        String baseAddress = this.BaseAddressResolver("TransactionProcessorApi");

        return $"{baseAddress}{route}";
    }

    public async Task<Result> RecordFloatCreditPurchase(String accessToken,
                                                        Guid estateId,
                                                        RecordFloatCreditPurchaseRequest recordFloatCreditPurchaseRequest,
                                                        CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/floats");

        try {
            var result = await this.Post<RecordFloatCreditPurchaseRequest>(requestUri, recordFloatCreditPurchaseRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error crediting contract product float.", ex);

            throw exception;
        }
    }

    public async Task<Result> RedeemVoucher(String accessToken,
                                            RedeemVoucherRequest redeemVoucherRequest,
                                            CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/vouchers");

        try {

            Result result = await this.Put(requestUri, redeemVoucherRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);


            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error redeeming voucher.", ex);

            throw exception;
        }
    }

    public async Task<Result> ResendEmailReceipt(String accessToken,
                                                 Guid estateId,
                                                 Guid transactionId,
                                                 CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/{estateId}/transactions/{transactionId}/resendreceipt");

        try {
            var result = await this.Post(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error requesting receipt resend.", ex);

            throw exception;
        }
    }

    public async Task<Result> AddContractToMerchant(String accessToken,
                                                    Guid estateId,
                                                    Guid merchantId,
                                                    AddMerchantContractRequest addMerchantContractRequest,
                                                    CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/contracts");

        try {
            Result result = await this.Patch(requestUri, addMerchantContractRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result; }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error adding contract {addMerchantContractRequest.ContractId} to merchant Id {merchantId} in estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> AddDeviceToMerchant(String accessToken,
                                                  Guid estateId,
                                                  Guid merchantId,
                                                  AddMerchantDeviceRequest addMerchantDeviceRequest,
                                                  CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/devices");

        try {
            
            Result result = await this.Patch(requestUri, addMerchantDeviceRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error adding device to merchant Id {merchantId} in estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> AddMerchantAddress(String accessToken,
                                                 Guid estateId,
                                                 Guid merchantId,
                                                 Address newAddressRequest,
                                                 CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/addresses");

        try {
            Result result = await this.Patch(requestUri, newAddressRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error adding address for merchant {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> AddMerchantContact(String accessToken,
                                                 Guid estateId,
                                                 Guid merchantId,
                                                 Contact newContactRequest,
                                                 CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/contacts");

        try {
            Result result = await this.Patch(requestUri, newContactRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error adding contact for merchant {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> AddProductToContract(String accessToken,
                                                   Guid estateId,
                                                   Guid contractId,
                                                   AddProductToContractRequest addProductToContractRequest,
                                                   CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/contracts/{contractId}/products");

        try {
            Result result = await this.Patch(requestUri, addProductToContractRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error adding product [{addProductToContractRequest.ProductName}] to contract [{contractId}] for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> AddTransactionFeeForProductToContract(String accessToken,
                                                                    Guid estateId,
                                                                    Guid contractId,
                                                                    Guid productId,
                                                                    AddTransactionFeeForProductToContractRequest addTransactionFeeForProductToContractRequest,
                                                                    CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/contracts/{contractId}/products/{productId}/transactionFees");

        try {
            Result result = await this.Patch(requestUri, addTransactionFeeForProductToContractRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error adding transaction fee [{addTransactionFeeForProductToContractRequest.Description}] for product [{productId}] to contract [{contractId}] for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> AssignOperatorToMerchant(String accessToken,
                                                       Guid estateId,
                                                       Guid merchantId,
                                                       AssignOperatorRequest assignOperatorRequest,
                                                       CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/operators");

        try {
            Result result = await this.Patch(requestUri, assignOperatorRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error assigning operator Id {assignOperatorRequest.OperatorId} to merchant Id {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> RemoveOperatorFromMerchant(String accessToken,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         Guid operatorId,
                                                         CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/operators/{operatorId}");

        try {
            Result result = await this.Delete(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error removing operator Id {operatorId} from merchant Id {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> RemoveOperatorFromEstate(String accessToken,
                                                       Guid estateId,
                                                       Guid operatorId,
                                                       CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/operators/{operatorId}");

        try {
            Result result = await this.Delete(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error removing operator Id {operatorId} from estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> RemoveContractFromMerchant(String accessToken,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         Guid contractId,
                                                         CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/contracts/{contractId}");

        try {
            Result result = await this.Delete(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error removing contract Id {contractId} from merchant Id {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateContract(String accessToken,
                                             Guid estateId,
                                             CreateContractRequest createContractRequest,
                                             CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/contracts/");

        try {
            var result = await this.Post<CreateContractRequest>(requestUri, createContractRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating contract [{createContractRequest.Description}] for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateOperator(String accessToken,
                                             Guid estateId,
                                             CreateOperatorRequest createOperatorRequest,
                                             CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/operators");

        try {
            var result = await this.Post<CreateOperatorRequest>(requestUri, createOperatorRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating new operator {createOperatorRequest.Name}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateEstateUser(String accessToken,
                                               Guid estateId,
                                               CreateEstateUserRequest createEstateUserRequest,
                                               CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/users");

        try {
            Result result = await this.Patch(requestUri, createEstateUserRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
            ;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating new estate user Estate Id {estateId} Email Address {createEstateUserRequest.EmailAddress}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateMerchant(String accessToken,
                                             Guid estateId,
                                             CreateMerchantRequest createMerchantRequest,
                                             CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants");

        try {
            var result = await this.Post<CreateMerchantRequest>(requestUri, createMerchantRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating new merchant {createMerchantRequest.Name} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateMerchantSchedule(String accessToken,
                                                     Guid estateId,
                                                     Guid merchantId,
                                                     CreateMerchantScheduleRequest createMerchantScheduleRequest,
                                                     CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/schedules");

        try {
            var result = await this.Post<CreateMerchantScheduleRequest>(requestUri, createMerchantScheduleRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex) {
            Exception exception = new($"Error creating schedule for merchant {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> UpdateMerchantSchedule(String accessToken,
                                                     Guid estateId,
                                                     Guid merchantId,
                                                     Int32 year,
                                                     UpdateMerchantScheduleRequest updateMerchantScheduleRequest,
                                                     CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/schedules/{year}");

        try
        {
            var result = await this.Patch(requestUri, updateMerchantScheduleRequest, accessToken, cancellationToken);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex)
        {
            Exception exception = new($"Error updating {year} schedule for merchant {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result<MerchantScheduleResponse>> GetMerchantSchedule(String accessToken,
                                                                            Guid estateId,
                                                                            Guid merchantId,
                                                                            Int32 year,
                                                                            CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/schedules/{year}");

        try {
            Result<MerchantScheduleResponse> result = await this.Get<MerchantScheduleResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            Exception exception = new($"Error getting merchant schedule for merchant {merchantId} for estate {estateId} in year {year}.", ex);

            throw exception;
        }
    }

    public async Task<Result<MerchantScheduleResponse>> GetMerchantScheduleFromReadModel(String accessToken,
                                                                                         Guid estateId,
                                                                                         Guid merchantId,
                                                                                         Int32 year,
                                                                                         CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/schedules/{year}/readmodel");

        try {
            Result<MerchantScheduleResponse> result = await this.Get<MerchantScheduleResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            Exception exception = new($"Error getting merchant schedule from read model for merchant {merchantId} for estate {estateId} in year {year}.", ex);

            throw exception;
        }
    }

    public async Task<Result> CreateMerchantUser(String accessToken,
                                                 Guid estateId,
                                                 Guid merchantId,
                                                 CreateMerchantUserRequest createMerchantUserRequest,
                                                 CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/users");

        try {
            Result result = await this.Patch(requestUri, createMerchantUserRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error creating new merchant user Merchant Id {estateId} Email Address {createMerchantUserRequest.EmailAddress}.", ex);

            throw exception;
        }
    }

    public async Task<Result> AssignOperatorToEstate(String accessToken,
                                                     Guid estateId,
                                                     DataTransferObjects.Requests.Estate.AssignOperatorRequest assignOperatorRequest,
                                                     CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/operators");

        try {
            Result result = await this.Patch(requestUri, assignOperatorRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error adding new operator {assignOperatorRequest.OperatorId} to estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> DisableTransactionFeeForProduct(String accessToken,
                                                              Guid estateId,
                                                              Guid contractId,
                                                              Guid productId,
                                                              Guid transactionFeeId,
                                                              CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/contracts/{contractId}/products/{productId}/transactionFees/{transactionFeeId}");

        try {
            Result result = await this.Delete(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error disabling transaction fee Id [{transactionFeeId}] for product [{productId}] on contract [{contractId}] for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result<ContractResponse>> GetContract(String accessToken,
                                                            Guid estateId,
                                                            Guid contractId,
                                                            CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/contracts/{contractId}");

        try {
            var result = await this.Get<ContractResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting contract {contractId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<ContractResponse>>> GetContracts(String accessToken,
                                                                   Guid estateId,
                                                                   CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/contracts");

        try {
            var result = await this.Get<List<ContractResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting contracts for estate {estateId}.", ex);

            throw exception;
        }
    }
    
    public async Task<Result<MerchantResponse>> GetMerchant(String accessToken,
                                                            Guid estateId,
                                                            Guid merchantId,
                                                            CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}");

        try {
            var result = await this.Get<MerchantResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting merchant Id {merchantId} in estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<ContractResponse>>> GetMerchantContracts(String accessToken,
                                                                           Guid estateId,
                                                                           Guid merchantId,
                                                                           CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/contracts");

        try {
            var result = await this.Get<List<ContractResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting merchant contracts for merchant Id {merchantId} in estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<MerchantResponse>>> GetMerchants(String accessToken,
                                                                   Guid estateId,
                                                                   CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants");

        try {
            var result = await this.Get<List<MerchantResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting merchant list for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result<DataTransferObjects.Responses.Settlement.SettlementResponse>> GetSettlement(String accessToken,
                                                                                                         Guid estateId,
                                                                                                         Guid? merchantId,
                                                                                                         Guid settlementId,
                                                                                                         CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/settlements/{settlementId}?merchantId={merchantId}");

        try {
            var result = await this.Get<DataTransferObjects.Responses.Settlement.SettlementResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting settlement id {settlementId} for estate [{estateId}]");

            throw exception;
        }
    }

    public async Task<Result<List<DataTransferObjects.Responses.Settlement.SettlementResponse>>> GetSettlements(String accessToken,
                                                                                                                Guid estateId,
                                                                                                                Guid? merchantId,
                                                                                                                String startDate,
                                                                                                                String endDate,
                                                                                                                CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/settlements?merchantId={merchantId}&start_date={startDate}&end_date={endDate}");

        try {
            var result = await this.Get<List<DataTransferObjects.Responses.Settlement.SettlementResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting settlements for estate [{estateId}]");

            throw exception;
        }
    }

    public async Task<Result<List<ContractProductTransactionFee>>> GetTransactionFeesForProduct(String accessToken,
                                                                                                Guid estateId,
                                                                                                Guid merchantId,
                                                                                                Guid contractId,
                                                                                                Guid productId,
                                                                                                CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/contracts/{contractId}/products/{productId}/transactionFees");

        try {
            var result = await this.Get<List<ContractProductTransactionFee>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error transaction fees for product {productId} on contract {contractId} for merchant Id {merchantId} in estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> MakeMerchantDeposit(String accessToken,
                                                  Guid estateId,
                                                  Guid merchantId,
                                                  MakeMerchantDepositRequest makeMerchantDepositRequest,
                                                  CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/deposits");

        try {
            var result = await this.Post<MakeMerchantDepositRequest>(requestUri, makeMerchantDepositRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error making merchant deposit for merchant {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> MakeMerchantWithdrawal(String accessToken,
                                                     Guid estateId,
                                                     Guid merchantId,
                                                     MakeMerchantWithdrawalRequest makeMerchantWithdrawalRequest,
                                                     CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/withdrawals");

        try {
            var result = await this.Post<MakeMerchantWithdrawalRequest>(requestUri, makeMerchantWithdrawalRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error making merchant withdrawal for merchant {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> SetMerchantSettlementSchedule(String accessToken,
                                                            Guid estateId,
                                                            Guid merchantId,
                                                            SetSettlementScheduleRequest setSettlementScheduleRequest,
                                                            CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}");

        try {
            Result result = await this.Patch(requestUri, setSettlementScheduleRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error setting settlement interval for merchant {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> SwapDeviceForMerchant(String accessToken,
                                                    Guid estateId,
                                                    Guid merchantId,
                                                    String deviceIdentifier,
                                                    SwapMerchantDeviceRequest swapMerchantDeviceRequest,
                                                    CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/devices/{deviceIdentifier}");

        try {
            Result result = await this.Patch<SwapMerchantDeviceRequest>(requestUri, swapMerchantDeviceRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error swapping device for merchant Id {merchantId} in estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> UpdateMerchant(String accessToken,
                                             Guid estateId,
                                             Guid merchantId,
                                             UpdateMerchantRequest updateMerchantRequest,
                                             CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}");

        try {
            Result result = await this.Patch(requestUri, updateMerchantRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error updating merchant {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> UpdateMerchantAddress(String accessToken,
                                                    Guid estateId,
                                                    Guid merchantId,
                                                    Guid addressId,
                                                    Address updatedAddressRequest,
                                                    CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/addresses/{addressId}");

        try {
            Result result = await this.Patch(requestUri, updatedAddressRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error updating address {addressId} for merchant {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> UpdateMerchantContact(String accessToken,
                                                    Guid estateId,
                                                    Guid merchantId,
                                                    Guid contactId,
                                                    Contact updatedContactRequest,
                                                    CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/contacts/{contactId}");

        try {
            Result result = await this.Patch(requestUri, updatedContactRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error updating contact {contactId} for merchant {merchantId} for estate {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result> UpdateOperator(String accessToken,
                                             Guid estateId,
                                             Guid operatorId,
                                             UpdateOperatorRequest updateOperatorRequest,
                                             CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/operators/{operatorId}");

        try {
            var result = await this.Post<UpdateOperatorRequest>(requestUri, updateOperatorRequest, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error updating operator id {operatorId}.", ex);

            throw exception;
        }
    }

    public async Task<Result<OperatorResponse>> GetOperator(String accessToken,
                                                            Guid estateId,
                                                            Guid operatorId,
                                                            CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/operators/{operatorId}");

        try {
            var result = await this.Get<OperatorResponse>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting operator id {operatorId} for estate Id {estateId}.", ex);

            throw exception;
        }
    }

    public async Task<Result<List<OperatorResponse>>> GetOperators(String accessToken,
                                                                   Guid estateId,
                                                                   CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/operators");

        try {
            var result = await this.Get<List<OperatorResponse>>(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return result;
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting all operators for estate Id {estateId}.", ex);

            throw exception;
        }
    }
}
