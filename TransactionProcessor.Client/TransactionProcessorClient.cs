namespace TransactionProcessor.Client
{
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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClientProxyBase" />
    /// <seealso cref="TransactionProcessor.Client.ITransactionProcessorClient" />
    public class TransactionProcessorClient : ClientProxyBase, ITransactionProcessorClient
    {
        #region Fields

        /// <summary>
        /// The base address
        /// </summary>
        private readonly String BaseAddress;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionProcessorClient" /> class.
        /// </summary>
        /// <param name="baseAddressResolver">The base address resolver.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public TransactionProcessorClient(Func<String, String> baseAddressResolver,
                                          HttpClient httpClient) : base(httpClient)
        {
            this.BaseAddress = baseAddressResolver("TransactionProcessorApi");

            // Add the API version header
            this.HttpClient.DefaultRequestHeaders.Add("api-version", "1.0");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs the transaction.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="transactionRequest">The transaction request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<SerialisedMessage> PerformTransaction(String accessToken,
                                                                SerialisedMessage transactionRequest,
                                                                CancellationToken cancellationToken)
        {
            SerialisedMessage response = null;

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
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<SerialisedMessage>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error posting transaction.", ex);

                throw exception;
            }

            return response;
        }

        public async Task<SettlementResponse> GetSettlementByDate(String accessToken,
                                                                                DateTime settlementDate,
                                                                                Guid estateId,
                                                                                CancellationToken cancellationToken)
        {
            SettlementResponse response = null;

            String requestUri = $"{this.BaseAddress}/api/settlements/{settlementDate.Date:yyyy-MM-dd}/estates/{estateId}/pending";

            try
            {
                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<SettlementResponse>(content);
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting settlment.", ex);

                throw exception;
            }

            return response;
        }

        public async Task ProcessSettlement(String accessToken,
                                            DateTime settlementDate,
                                            Guid estateId,
                                            CancellationToken cancellationToken)
        {
            String requestUri = $"{this.BaseAddress}/api/settlements/{settlementDate.Date:yyyy-MM-dd}/estates/{estateId}";

            try
            {
                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                StringContent requestContent = new StringContent(String.Empty);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, requestContent, cancellationToken);

                // Process the response
                await this.HandleResponse(httpResponse, cancellationToken);

            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error processing settlment.", ex);

                throw exception;
            }
        }
        
        public async Task ResendEmailReceipt(String accessToken,
                                       Guid estateId,
                                       Guid transactionId,
                                       CancellationToken cancellationToken) {
            String requestUri = $"{this.BaseAddress}/api/{estateId}/transactions/{transactionId}/resendreceipt";

            try
            {
                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                StringContent requestContent = new StringContent(String.Empty);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, requestContent, cancellationToken);

                // Process the response
                await this.HandleResponse(httpResponse, cancellationToken);

            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error requesting receipt resend.", ex);

                throw exception;
            }
        }

        public async Task<MerchantBalanceResponse> GetMerchantBalance(String accessToken,
                                                                      Guid estateId,
                                                                      Guid merchantId,
                                                                      CancellationToken cancellationToken) {
            String requestUri = $"{this.BaseAddress}/api/estates/{estateId}/merchants/{merchantId}/balance";
            MerchantBalanceResponse response = null;
            try
            {
                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                
                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<MerchantBalanceResponse>(content);

            }
            catch (Exception ex)
            {
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
                                                                                         CancellationToken cancellationToken) {
            String requestUri = $"{this.BaseAddress}/api/estates/{estateId}/merchants/{merchantId}/balancehistory?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            List<MerchantBalanceChangedEntryResponse> response = null;
            try
            {
                // Add the access token to the client headers
                this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<List<MerchantBalanceChangedEntryResponse>>(content);
            }
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting merchant balance.", ex);

                throw exception;
            }

            return response;
        }

        #endregion
    }
}