using SimpleResults;

namespace TransactionProcessor.Client{
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

    public class TransactionProcessorClient : ClientProxyBase, ITransactionProcessorClient{
        #region Fields

        private readonly String BaseAddress;

        #endregion

        #region Constructors

        public TransactionProcessorClient(Func<String, String> baseAddressResolver,
                                          HttpClient httpClient) : base(httpClient){
            this.BaseAddress = baseAddressResolver("TransactionProcessorApi");

            // Add the API version header
            this.HttpClient.DefaultRequestHeaders.Add("api-version", "1.0");
        }

        #endregion

        public async Task<Result> CreateFloatForContractProduct(String accessToken, Guid estateId, CreateFloatForContractProductRequest createFloatForContractProductRequest, CancellationToken cancellationToken){
            String requestUri = $"{this.BaseAddress}/api/estates/{estateId}/floats";

            try
            {
                String requestSerialised = JsonConvert.SerializeObject(createFloatForContractProductRequest);

                StringContent httpContent = new StringContent(requestSerialised, Encoding.UTF8, "application/json");

                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                Result<StringResult> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error creating contract product float.", ex);

                throw exception;
            }
        }

        public async Task<Result<MerchantBalanceResponse>> GetMerchantBalance(String accessToken,
                                                                      Guid estateId,
                                                                      Guid merchantId,
                                                                      CancellationToken cancellationToken,
                                                                      Boolean liveBalance = true){
            String requestUri = $"{this.BaseAddress}/api/estates/{estateId}/merchants/{merchantId}/balance";

            if (liveBalance){
                requestUri = $"{this.BaseAddress}/api/estates/{estateId}/merchants/{merchantId}/livebalance";
            }
            
            try
            {
                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<StringResult> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<MerchantBalanceResponse> responseData =
                    JsonConvert.DeserializeObject<ResponseData<MerchantBalanceResponse>>(result.Data.StringData);

                return Result.Success(responseData.Data);
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting merchant balance.", ex);

                throw exception;
            }
        }

        public async Task<Result<List<MerchantBalanceChangedEntryResponse>>> GetMerchantBalanceHistory(String accessToken,
                                                                                               Guid estateId,
                                                                                               Guid merchantId,
                                                                                               DateTime startDate,
                                                                                               DateTime endDate,
                                                                                               CancellationToken cancellationToken){
            String requestUri = $"{this.BaseAddress}/api/estates/{estateId}/merchants/{merchantId}/balancehistory?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";

            try
            {
                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<StringResult> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<List<MerchantBalanceChangedEntryResponse>> responseData =
                    JsonConvert.DeserializeObject<ResponseData<List<MerchantBalanceChangedEntryResponse>>>(result.Data.StringData);

                return Result.Success(responseData.Data);
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting merchant balance history.", ex);

                throw exception;
            }
        }

        public async Task<Result<SettlementResponse>> GetSettlementByDate(String accessToken,
                                                                          DateTime settlementDate,
                                                                          Guid estateId,
                                                                          Guid merchantId,
                                                                          CancellationToken cancellationToken){
            String requestUri = $"{this.BaseAddress}/api/settlements/{settlementDate.Date:yyyy-MM-dd}/estates/{estateId}/merchants/{merchantId}/pending";
            try {
                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<StringResult> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<SettlementResponse> responseData =
                    JsonConvert.DeserializeObject<ResponseData<SettlementResponse>>(result.Data.StringData);

                return Result.Success(responseData.Data);
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting settlement.", ex);

                throw exception;
            }
        }

        public async Task<Result<GetVoucherResponse>> GetVoucherByCode(String accessToken,
                                                                       Guid estateId,
                                                                       String voucherCode,
                                                                       CancellationToken cancellationToken){
            GetVoucherResponse response = null;

            String requestUri = $"{this.BaseAddress}/api/vouchers?estateId={estateId}&voucherCode={voucherCode}";

            try
            {
                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<StringResult> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<GetVoucherResponse> responseData =
                    JsonConvert.DeserializeObject<ResponseData<GetVoucherResponse>>(result.Data.StringData);

                return Result.Success(responseData.Data);
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting voucher by code.", ex); ;

                throw exception;
            }
        }

        public async Task<Result<GetVoucherResponse>> GetVoucherByTransactionId(String accessToken,
                                                                                Guid estateId,
                                                                                Guid transactionId,
                                                                                CancellationToken cancellationToken){
            GetVoucherResponse response = null;

            String requestUri = $"{this.BaseAddress}/api/vouchers?estateId={estateId}&transactionId={transactionId}";

            try
            {
                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<StringResult> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<GetVoucherResponse> responseData =
                    JsonConvert.DeserializeObject<ResponseData<GetVoucherResponse>>(result.Data.StringData);

                return Result.Success(responseData.Data);
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting voucher by transaction Id.", ex); ;

                throw exception;
            }
        }

        public async Task<Result<SerialisedMessage>> PerformTransaction(String accessToken,
                                                                        SerialisedMessage transactionRequest,
                                                                        CancellationToken cancellationToken){
            String requestUri = $"{this.BaseAddress}/api/transactions";
            try
            {
                String requestSerialised = JsonConvert.SerializeObject(transactionRequest);

                StringContent httpContent = new StringContent(requestSerialised, Encoding.UTF8, "application/json");

                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                Result<StringResult> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<SerialisedMessage> responseData =
                    JsonConvert.DeserializeObject<ResponseData<SerialisedMessage>>(result.Data.StringData);

                return Result.Success(responseData.Data);
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error posting transaction.", ex);

                throw exception;
            }
        }

        public async Task<Result> ProcessSettlement(String accessToken,
                                                    DateTime settlementDate,
                                                    Guid estateId,
                                                    Guid merchantId,
                                                    CancellationToken cancellationToken){
            String requestUri = $"{this.BaseAddress}/api/settlements/{settlementDate.Date:yyyy-MM-dd}/estates/{estateId}/merchants/{merchantId}";

            try
            {
                StringContent httpContent = new StringContent(String.Empty, Encoding.UTF8, "application/json");

                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                Result<StringResult> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error processing settlement.", ex);

                throw exception;
            }
        }

        public async Task<Result> RecordFloatCreditPurchase(String accessToken, Guid estateId, RecordFloatCreditPurchaseRequest recordFloatCreditPurchaseRequest, CancellationToken cancellationToken){
            String requestUri = $"{this.BaseAddress}/api/estates/{estateId}/floats";

            try
            {
                String requestSerialised = JsonConvert.SerializeObject(recordFloatCreditPurchaseRequest);

                StringContent httpContent = new StringContent(requestSerialised, Encoding.UTF8, "application/json");
                
                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PutAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                Result<StringResult> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error crediting contract product float.", ex);

                throw exception;
            }
        }

        public async Task<Result<RedeemVoucherResponse>> RedeemVoucher(String accessToken,
                                                                       RedeemVoucherRequest redeemVoucherRequest,
                                                                       CancellationToken cancellationToken){
            RedeemVoucherResponse response = null;

            String requestUri = $"{this.BaseAddress}/api/vouchers";

            try
            {
                String requestSerialised = JsonConvert.SerializeObject(redeemVoucherRequest);

                StringContent httpContent = new StringContent(requestSerialised, Encoding.UTF8, "application/json");

                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PutAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                Result<StringResult> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<RedeemVoucherResponse> responseData =
                    JsonConvert.DeserializeObject<ResponseData<RedeemVoucherResponse>>(result.Data.StringData);

                return Result.Success(responseData.Data);
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error redeeming voucher.", ex);

                throw exception;
            }
        }

        public async Task<Result> ResendEmailReceipt(String accessToken,
                                                     Guid estateId,
                                                     Guid transactionId,
                                                     CancellationToken cancellationToken){
            String requestUri = $"{this.BaseAddress}/api/{estateId}/transactions/{transactionId}/resendreceipt";

            try {
                StringContent httpContent = new StringContent(String.Empty, Encoding.UTF8, "application/json");

                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse =
                    await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                Result<StringResult> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error requesting receipt resend.", ex);

                throw exception;
            }
        }
    }

    internal class ResponseData<T>
    {
        public T Data { get; set; }
    }

    public static class ResultHelpers
    {
        public static Result CreateFailure(Result result)
        {
            if (result.IsFailed)
            {
                return BuildResult(result.Status, result.Message, result.Errors);
            }
            return Result.Failure("Unknown Failure");
        }

        public static Result CreateFailure<T>(Result<T> result)
        {
            if (result.IsFailed)
            {
                return BuildResult(result.Status, result.Message, result.Errors);
            }
            return Result.Failure("Unknown Failure");
        }

        private static Result BuildResult(ResultStatus status, String messageValue, IEnumerable<String> errorList)
        {
            return (status, messageValue, errorList) switch
            {
                // If the status is NotFound and there are errors, return the errors
                (ResultStatus.NotFound, _, List<string> errors) when errors is { Count: > 0 } =>
                    Result.NotFound(errors),

                // If the status is NotFound and the message is not null or empty, return the message
                (ResultStatus.NotFound, string message, _) when !string.IsNullOrEmpty(message) =>
                    Result.NotFound(message),

                // If the status is Failure and there are errors, return the errors
                (ResultStatus.Failure, _, List<string> errors) when errors is { Count: > 0 } =>
                    Result.Failure(errors),

                // If the status is Failure and the message is not null or empty, return the message
                (ResultStatus.Failure, string message, _) when !string.IsNullOrEmpty(message) =>
                    Result.Failure(message),

                // If the status is Forbidden and there are errors, return the errors
                (ResultStatus.Forbidden, _, List<string> errors) when errors is { Count: > 0 } =>
                    Result.Forbidden(errors),

                // If the status is Forbidden and the message is not null or empty, return the message
                (ResultStatus.Forbidden, string message, _) when !string.IsNullOrEmpty(message) =>
                    Result.NotFound(message),
                //###
                // If the status is Invalid and there are errors, return the errors
                (ResultStatus.Invalid, _, List<string> errors) when errors is { Count: > 0 } =>
                    Result.Invalid(errors),

                // If the status is Invalid and the message is not null or empty, return the message
                (ResultStatus.Invalid, string message, _) when !string.IsNullOrEmpty(message) =>
                    Result.Invalid(message),

                // If the status is Unauthorized and there are errors, return the errors
                (ResultStatus.Unauthorized, _, List<string> errors) when errors is { Count: > 0 } =>
                    Result.Unauthorized(errors),

                // If the status is Unauthorized and the message is not null or empty, return the message
                (ResultStatus.Unauthorized, string message, _) when !string.IsNullOrEmpty(message) =>
                    Result.Unauthorized(message),

                // If the status is Conflict and there are errors, return the errors
                (ResultStatus.Conflict, _, List<string> errors) when errors is { Count: > 0 } =>
                    Result.Conflict(errors),

                // If the status is Conflict and the message is not null or empty, return the message
                (ResultStatus.Conflict, string message, _) when !string.IsNullOrEmpty(message) =>
                    Result.Conflict(message),

                // If the status is CriticalError and there are errors, return the errors
                (ResultStatus.CriticalError, _, List<string> errors) when errors is { Count: > 0 } =>
                    Result.CriticalError(errors),

                // If the status is CriticalError and the message is not null or empty, return the message
                (ResultStatus.CriticalError, string message, _) when !string.IsNullOrEmpty(message) =>
                    Result.CriticalError(message),

                // Default case, return a generic failure message
                _ => Result.Failure("An unexpected error occurred.")
            };
        }
    }
}