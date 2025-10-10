using SimpleResults;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
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

public class TransactionProcessorClient : ClientProxyBase, ITransactionProcessorClient {
    #region Fields
    
    private readonly Func<String, String> BaseAddressResolver;

    #endregion

    #region Constructors

    public TransactionProcessorClient(Func<String, String> baseAddressResolver,
                                      HttpClient httpClient) : base(httpClient) {
        this.BaseAddressResolver = baseAddressResolver;

        // Add the API version header
        this.HttpClient.DefaultRequestHeaders.Add("api-version", "1.0");
    }

    #endregion

    public async Task<Result<EstateResponse>> GetEstate(String accessToken,
                                                        Guid estateId,
                                                        CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}");

        try {
            Result<String> result =  await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            // call was successful so now deserialise the body to the response object
            ResponseData<EstateResponse> responseData = this.HandleResponseContent<EstateResponse>(result.Data);

            return Result.Success(responseData.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting estate Id {estateId}.", ex);

            throw exception;
        }
    }

    private async Task<Result<String>> SendGetRequest(String uri, String accessToken, CancellationToken cancellationToken) {

        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.Bearer, accessToken);

        // Make the Http Call here
        HttpResponseMessage httpResponse = await this.HttpClient.SendAsync(requestMessage, cancellationToken);

        // Process the response
        Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

        if (result.IsFailed)
            return ResultHelpers.CreateFailure(result);

        return Result.Success<String>(result.Data);
    }

    private async Task<Result<String>> SendPostRequest(String uri, String accessToken, String content, CancellationToken cancellationToken)
    {

        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.Bearer, accessToken);
        requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");

        // Make the Http Call here
        HttpResponseMessage httpResponse = await this.HttpClient.SendAsync(requestMessage, cancellationToken);

        // Process the response
        Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

        if (result.IsFailed)
            return ResultHelpers.CreateFailure(result);

        return Result.Success<String>(result.Data);
    }

    private async Task<Result<String>> SendPutRequest(String uri, String accessToken, String content, CancellationToken cancellationToken)
    {

        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, uri);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.Bearer, accessToken);
        requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");

        // Make the Http Call here
        HttpResponseMessage httpResponse = await this.HttpClient.SendAsync(requestMessage, cancellationToken);

        // Process the response
        Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

        if (result.IsFailed)
            return ResultHelpers.CreateFailure(result);

        return Result.Success<String>(result.Data);
    }

    public static class AuthenticationSchemes
    {
        public static readonly String Bearer = "Bearer";
    }

    private async Task<Result<String>> SendPatchRequest(String uri, String accessToken, String content, CancellationToken cancellationToken)
    {

        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Patch, uri);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.Bearer, accessToken);
        requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");

        // Make the Http Call here
        HttpResponseMessage httpResponse = await this.HttpClient.SendAsync(requestMessage, cancellationToken);

        // Process the response
        Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

        if (result.IsFailed)
            return ResultHelpers.CreateFailure(result);

        return Result.Success<String>(result.Data);
    }

    private async Task<Result<String>> SendDeleteRequest(String uri, String accessToken, CancellationToken cancellationToken)
    {

        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.Bearer, accessToken);

        // Make the Http Call here
        HttpResponseMessage httpResponse = await this.HttpClient.SendAsync(requestMessage, cancellationToken);

        // Process the response
        Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

        if (result.IsFailed)
            return ResultHelpers.CreateFailure(result);

        return Result.Success<String>(result.Data);
    }

    public async Task<Result> CreateEstate(String accessToken,
                                           CreateEstateRequest createEstateRequest,
                                           CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl("/api/estates/");

        try {
            String requestSerialised = JsonConvert.SerializeObject(createEstateRequest);
            Result<String> result = await this.SendPostRequest(requestUri, accessToken, requestSerialised, cancellationToken);
            
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            ResponseData<List<EstateResponse>> responseData = this.HandleResponseContent<List<EstateResponse>>(result.Data);

            return Result.Success(responseData.Data);
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
            String requestSerialised = JsonConvert.SerializeObject(createFloatForContractProductRequest);

            Result<String> result = await this.SendPostRequest(requestUri, accessToken, requestSerialised, cancellationToken);

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

    public async Task<Result<MerchantBalanceResponse>> GetMerchantBalance(String accessToken,
                                                                          Guid estateId,
                                                                          Guid merchantId,
                                                                          CancellationToken cancellationToken,
                                                                          Boolean liveBalance = true) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/balance");

        if (liveBalance) requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/livebalance");

        try {
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<MerchantBalanceResponse> responseData = this.HandleResponseContent<MerchantBalanceResponse>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<List<MerchantBalanceChangedEntryResponse>> responseData = this.HandleResponseContent<List<MerchantBalanceChangedEntryResponse>>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<SettlementResponse> responseData = this.HandleResponseContent<SettlementResponse>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<GetVoucherResponse> responseData = this.HandleResponseContent<GetVoucherResponse>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<GetVoucherResponse> responseData = this.HandleResponseContent<GetVoucherResponse>(result.Data);

            return Result.Success(responseData.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new("Error getting voucher by transaction Id.", ex);
            ;

            throw exception;
        }
    }

    public async Task<Result<SerialisedMessage>> PerformTransaction(String accessToken,
                                                                    SerialisedMessage transactionRequest,
                                                                    CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/transactions");
        try {
            String requestSerialised = JsonConvert.SerializeObject(transactionRequest);

            Result<String> result = await this.SendPostRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<SerialisedMessage> responseData = this.HandleResponseContent<SerialisedMessage>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendPostRequest(requestUri, accessToken, String.Empty, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);
            
            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(recordFloatCreditPurchaseRequest);

            Result<String> result = await this.SendPostRequest(requestUri, accessToken, requestSerialised, cancellationToken);

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

    public async Task<Result<RedeemVoucherResponse>> RedeemVoucher(String accessToken,
                                                                   RedeemVoucherRequest redeemVoucherRequest,
                                                                   CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/vouchers");

        try {
            String requestSerialised = JsonConvert.SerializeObject(redeemVoucherRequest);

            Result<String> result = await this.SendPutRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<RedeemVoucherResponse> responseData = this.HandleResponseContent<RedeemVoucherResponse>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendPostRequest(requestUri, accessToken, String.Empty, cancellationToken);

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
            String requestSerialised = JsonConvert.SerializeObject(addMerchantContractRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }
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
            String requestSerialised = JsonConvert.SerializeObject(addMerchantDeviceRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(newAddressRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(newContactRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(addProductToContractRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(addTransactionFeeForProductToContractRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(assignOperatorRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            Result<String> result = await this.SendDeleteRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            Result<String> result = await this.SendDeleteRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            Result<String> result = await this.SendDeleteRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(createContractRequest);

            Result<String> result = await this.SendPostRequest(requestUri, accessToken, requestSerialised, cancellationToken);

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
            String requestSerialised = JsonConvert.SerializeObject(createOperatorRequest);

            Result<String> result = await this.SendPostRequest(requestUri, accessToken, requestSerialised, cancellationToken);

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
            String requestSerialised = JsonConvert.SerializeObject(createEstateUserRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(createMerchantRequest);

            Result<String> result = await this.SendPostRequest(requestUri, accessToken, requestSerialised, cancellationToken);

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

    public async Task<Result> CreateMerchantUser(String accessToken,
                                                 Guid estateId,
                                                 Guid merchantId,
                                                 CreateMerchantUserRequest createMerchantUserRequest,
                                                 CancellationToken cancellationToken) {
        String requestUri = this.BuildRequestUrl($"/api/estates/{estateId}/merchants/{merchantId}/users");

        try {
            String requestSerialised = JsonConvert.SerializeObject(createMerchantUserRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(assignOperatorRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            Result<String> result = await this.SendDeleteRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            // call was successful so now deserialise the body to the response object
            ResponseData<ContractResponse> responseData = this.HandleResponseContent<ContractResponse>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<List<ContractResponse>> responseData = this.HandleResponseContent<List<ContractResponse>>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<MerchantResponse> responseData = this.HandleResponseContent<MerchantResponse>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<List<ContractResponse>> responseData = this.HandleResponseContent<List<ContractResponse>>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<List<MerchantResponse>> responseData = this.HandleResponseContent<List<MerchantResponse>>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<DataTransferObjects.Responses.Settlement.SettlementResponse> responseData = this.HandleResponseContent<DataTransferObjects.Responses.Settlement.SettlementResponse>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<List<DataTransferObjects.Responses.Settlement.SettlementResponse>> responseData = this.HandleResponseContent<List<DataTransferObjects.Responses.Settlement.SettlementResponse>>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            // call was successful so now deserialise the body to the response object
            ResponseData<List<ContractProductTransactionFee>> responseData = this.HandleResponseContent<List<ContractProductTransactionFee>>(result.Data);

            return Result.Success(responseData.Data);
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
            String requestSerialised = JsonConvert.SerializeObject(makeMerchantDepositRequest);

            Result<String> result = await this.SendPostRequest(requestUri, accessToken, requestSerialised, cancellationToken);

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
            String requestSerialised = JsonConvert.SerializeObject(makeMerchantWithdrawalRequest);

            Result<String> result = await this.SendPostRequest(requestUri, accessToken, requestSerialised, cancellationToken);

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
            String requestSerialised = JsonConvert.SerializeObject(setSettlementScheduleRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(swapMerchantDeviceRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(updateMerchantRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(updatedAddressRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(updatedContactRequest);

            Result<String> result = await this.SendPatchRequest(requestUri, accessToken, requestSerialised, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
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
            String requestSerialised = JsonConvert.SerializeObject(updateOperatorRequest);

            Result<String> result = await this.SendPostRequest(requestUri, accessToken, requestSerialised, cancellationToken);

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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<OperatorResponse> responseData = this.HandleResponseContent<OperatorResponse>(result.Data);

            return Result.Success(responseData.Data);
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
            Result<String> result = await this.SendGetRequest(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            ResponseData<List<OperatorResponse>> responseData = this.HandleResponseContent<List<OperatorResponse>>(result.Data);

            return Result.Success(responseData.Data);
        }
        catch (Exception ex) {
            // An exception has occurred, add some additional information to the message
            Exception exception = new($"Error getting all operators for estate Id {estateId}.", ex);

            throw exception;
        }
    }
}