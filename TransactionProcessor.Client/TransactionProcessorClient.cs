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

        #region Methods

        public async Task<CreateFloatForContractProductResponse> CreateFloatForContractProduct(String accessToken, Guid estateId, CreateFloatForContractProductRequest createFloatForContractProductRequest, CancellationToken cancellationToken){
            CreateFloatForContractProductResponse response = null;

            String requestUri = $"{this.BaseAddress}/api/estates/{estateId}/floats";

            try{
                response = await this.Post<CreateFloatForContractProductRequest, CreateFloatForContractProductResponse>(requestUri, createFloatForContractProductRequest, accessToken, cancellationToken);
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error creating contract product float.", ex);

                throw exception;
            }

            return response;
        }

        public async Task<MerchantBalanceResponse> GetMerchantBalance(String accessToken,
                                                                      Guid estateId,
                                                                      Guid merchantId,
                                                                      CancellationToken cancellationToken,
                                                                      Boolean liveBalance = true){
            String requestUri = $"{this.BaseAddress}/api/estates/{estateId}/merchants/{merchantId}/balance";

            if (liveBalance){
                requestUri = $"{this.BaseAddress}/api/estates/{estateId}/merchants/{merchantId}/livebalance";
            }

            MerchantBalanceResponse response = null;
            try{
                response = await this.Get<MerchantBalanceResponse>(requestUri, accessToken, cancellationToken);
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting merchant balance.", ex);

                throw exception;
            }

            return response;
        }

        public async Task<List<MerchantBalanceChangedEntryResponse>> GetMerchantBalanceHistory(String accessToken,
                                                                                               Guid estateId,
                                                                                               Guid merchantId,
                                                                                               DateTime startDate,
                                                                                               DateTime endDate,
                                                                                               CancellationToken cancellationToken){
            String requestUri = $"{this.BaseAddress}/api/estates/{estateId}/merchants/{merchantId}/balancehistory?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            List<MerchantBalanceChangedEntryResponse> response = null;
            try{
                response = await this.Get<List<MerchantBalanceChangedEntryResponse>>(requestUri, accessToken, cancellationToken);
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting merchant balance.", ex);

                throw exception;
            }

            return response;
        }

        public async Task<SettlementResponse> GetSettlementByDate(String accessToken,
                                                                  DateTime settlementDate,
                                                                  Guid estateId,
                                                                  Guid merchantId,
                                                                  CancellationToken cancellationToken){
            SettlementResponse response = null;

            String requestUri = $"{this.BaseAddress}/api/settlements/{settlementDate.Date:yyyy-MM-dd}/estates/{estateId}/merchants/{merchantId}/pending";

            try{
                response = await this.Get<SettlementResponse>(requestUri, accessToken, cancellationToken);
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting settlment.", ex);

                throw exception;
            }

            return response;
        }

        public async Task<GetVoucherResponse> GetVoucherByCode(String accessToken,
                                                               Guid estateId,
                                                               String voucherCode,
                                                               CancellationToken cancellationToken){
            GetVoucherResponse response = null;

            String requestUri = $"{this.BaseAddress}/api/vouchers?estateId={estateId}&voucherCode={voucherCode}";

            try{
                response = await this.Get<GetVoucherResponse>(requestUri, accessToken, cancellationToken);
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting voucher.", ex);

                throw exception;
            }

            return response;
        }

        public async Task<GetVoucherResponse> GetVoucherByTransactionId(String accessToken,
                                                                        Guid estateId,
                                                                        Guid transactionId,
                                                                        CancellationToken cancellationToken){
            GetVoucherResponse response = null;

            String requestUri = $"{this.BaseAddress}/api/vouchers?estateId={estateId}&transactionId={transactionId}";

            try{
                response = await this.Get<GetVoucherResponse>(requestUri, accessToken, cancellationToken);
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting voucher.", ex);

                throw exception;
            }

            return response;
        }

        public async Task<SerialisedMessage> PerformTransaction(String accessToken,
                                                                SerialisedMessage transactionRequest,
                                                                CancellationToken cancellationToken){
            SerialisedMessage response = null;

            String requestUri = $"{this.BaseAddress}/api/transactions";

            try{
                response = await this.Post<SerialisedMessage, SerialisedMessage>(requestUri, transactionRequest, accessToken, cancellationToken);
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error posting transaction.", ex);

                throw exception;
            }

            return response;
        }

        public async Task ProcessSettlement(String accessToken,
                                            DateTime settlementDate,
                                            Guid estateId,
                                            Guid merchantId,
                                            CancellationToken cancellationToken){
            String requestUri = $"{this.BaseAddress}/api/settlements/{settlementDate.Date:yyyy-MM-dd}/estates/{estateId}/merchants/{merchantId}";

            try{
                await this.Post(requestUri, accessToken, cancellationToken);
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error processing settlment.", ex);

                throw exception;
            }
        }

        public async Task RecordFloatCreditPurchase(String accessToken, Guid estateId, RecordFloatCreditPurchaseRequest recordFloatCreditPurchaseRequest, CancellationToken cancellationToken){
            String requestUri = $"{this.BaseAddress}/api/estates/{estateId}/floats";

            try{
                await this.Put(requestUri, recordFloatCreditPurchaseRequest, accessToken, cancellationToken);
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error crediting contract product float.", ex);

                throw exception;
            }
        }

        public async Task<RedeemVoucherResponse> RedeemVoucher(String accessToken,
                                                               RedeemVoucherRequest redeemVoucherRequest,
                                                               CancellationToken cancellationToken){
            RedeemVoucherResponse response = null;

            String requestUri = $"{this.BaseAddress}/api/vouchers";

            try{
                response = await this.Post<RedeemVoucherRequest, RedeemVoucherResponse>(requestUri, redeemVoucherRequest, accessToken, cancellationToken);
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error redeeming voucher.", ex);

                throw exception;
            }

            return response;
        }

        public async Task ResendEmailReceipt(String accessToken,
                                             Guid estateId,
                                             Guid transactionId,
                                             CancellationToken cancellationToken){
            String requestUri = $"{this.BaseAddress}/api/{estateId}/transactions/{transactionId}/resendreceipt";

            try{
                await this.Post(requestUri, accessToken, cancellationToken);
            }
            catch(Exception ex){
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error requesting receipt resend.", ex);

                throw exception;
            }
        }

        private async Task<TResponse> Get<TResponse>(String requestUri, String accessToken, CancellationToken cancellationToken){
            // Add the access token to the client headers
            this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Make the Http Call here
            HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

            // Process the response
            String content = await this.HandleResponse(httpResponse, cancellationToken);

            // call was successful so now deserialise the body to the response object
            return JsonConvert.DeserializeObject<TResponse>(content);
        }

        private async Task Post(String requestUri, String accessToken, CancellationToken cancellationToken){
            StringContent httpContent = new("");

            // Add the access token to the client headers
            this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Make the Http Call here
            HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

            // Process the response
            await this.HandleResponse(httpResponse, cancellationToken);
        }

        private async Task<TResponse> Post<TRequest, TResponse>(String requestUri, TRequest requestObject, String accessToken, CancellationToken cancellationToken){
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);

            String requestSerialised = JsonConvert.SerializeObject(requestObject);

            StringContent httpContent = new(requestSerialised, Encoding.UTF8, "application/json");

            // Add the access token to the client headers
            this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Make the Http Call here
            HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

            // Process the response
            String content = await this.HandleResponse(httpResponse, cancellationToken);

            // call was successful so now deserialise the body to the response object
            return JsonConvert.DeserializeObject<TResponse>(content);
        }

        private async Task Put<TRequest>(String requestUri, TRequest requestObject, String accessToken, CancellationToken cancellationToken){
            String requestSerialised = JsonConvert.SerializeObject(requestObject);

            StringContent httpContent = new(requestSerialised, Encoding.UTF8, "application/json");

            // Add the access token to the client headers
            this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Make the Http Call here
            HttpResponseMessage httpResponse = await this.HttpClient.PutAsync(requestUri, httpContent, cancellationToken);

            // Process the response
            await this.HandleResponse(httpResponse, cancellationToken);
        }

        #endregion
    }
}