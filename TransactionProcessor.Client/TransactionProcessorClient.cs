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
    using Shared.Results;

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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<MerchantBalanceResponse> responseData = HandleResponseContent<MerchantBalanceResponse>(result.Data);
                
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<List<MerchantBalanceChangedEntryResponse>> responseData = HandleResponseContent<List<MerchantBalanceChangedEntryResponse>>(result.Data);

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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<SettlementResponse> responseData = HandleResponseContent<SettlementResponse>(result.Data);
                
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<GetVoucherResponse> responseData = HandleResponseContent<GetVoucherResponse>(result.Data);
                
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<GetVoucherResponse> responseData = HandleResponseContent<GetVoucherResponse>(result.Data);

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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<SerialisedMessage> responseData = HandleResponseContent<SerialisedMessage>(result.Data);

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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                ResponseData<RedeemVoucherResponse> responseData = HandleResponseContent<RedeemVoucherResponse>(result.Data);

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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

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
}