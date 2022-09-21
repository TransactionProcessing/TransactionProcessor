namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPostPay
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EstateManagement.DataTransferObjects.Responses;
    using global::PataPawaPostPay;
    using Microsoft.Extensions.Caching.Memory;
    using Shared.Logger;

    public class PataPawaPostPayProxy : IOperatorProxy
    {
        #region Fields

        private readonly PataPawaPostPaidConfiguration Configuration;

        private readonly IMemoryCache MemoryCache;

        private readonly PataPawaPostPayServiceClient ServiceClient;

        private readonly Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> ChannelResolver;

        #endregion

        #region Constructors

        public PataPawaPostPayProxy(PataPawaPostPayServiceClient serviceClient,
                                    Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver,
                                    PataPawaPostPaidConfiguration configuration,
                                    IMemoryCache memoryCache) {
            this.ServiceClient = serviceClient;
            this.ChannelResolver = channelResolver;
            this.Configuration = configuration;
            this.MemoryCache = memoryCache;
        }

        #endregion

        #region Methods

        public async Task<OperatorResponse> ProcessLogonMessage(String accessToken,
                                                                CancellationToken cancellationToken) {
            IPataPawaPostPayService channel = this.ChannelResolver(this.ServiceClient, this.Configuration.Url);
            login logonResponse = await channel.getLoginRequestAsync(this.Configuration.Username, this.Configuration.Password);
            if (logonResponse.status != 0) {
                return new OperatorResponse {
                                                IsSuccessful = false,
                                                ResponseCode = "-1",
                                                ResponseMessage = "Error logging on with PataPawa Post Paid API",
                                                AdditionalTransactionResponseMetadata = new Dictionary<String, String>()
                };
            }

            OperatorResponse operatorResponse = new OperatorResponse {
                                                                         IsSuccessful = true,
                                                                         ResponseCode = "0000",
                                                                         ResponseMessage = logonResponse.message,
                                                                         AdditionalTransactionResponseMetadata = new Dictionary<String, String>()
                                                                     };

            operatorResponse.AdditionalTransactionResponseMetadata.Add("PataPawaPostPaidAPIKey", logonResponse.api_key);
            operatorResponse.AdditionalTransactionResponseMetadata.Add("PataPawaPostPaidBalance", logonResponse.balance.ToString());

            this.MemoryCache.Set("PataPawaPostPayLogon", operatorResponse, MemoryCacheEntryOptions);

            return operatorResponse;
        }

        private MemoryCacheEntryOptions MemoryCacheEntryOptions =>
            new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove).SetSlidingExpiration(TimeSpan.FromHours(1))
                                         .RegisterPostEvictionCallback(PostEvictionCallback);

        private void PostEvictionCallback(Object key,
                                          Object value,
                                          EvictionReason reason,
                                          Object state) {
            if (key.ToString().Contains("Logon")) {
                ProcessLogonMessage(String.Empty, CancellationToken.None).Wait();
            }
        }

        public async Task<OperatorResponse> ProcessSaleMessage(String accessToken,
                                                               Guid transactionId,
                                                               String operatorIdentifier,
                                                               MerchantResponse merchant,
                                                               DateTime transactionDateTime,
                                                               String transactionReference,
                                                               Dictionary<String, String> additionalTransactionMetadata,
                                                               CancellationToken cancellationToken) {
            // Get the logon response for the operator
            OperatorResponse logonResponse = this.MemoryCache.Get<OperatorResponse>("PataPawaPostPayLogon");
            if (logonResponse == null) {
                throw new ArgumentNullException("PataPawaPostPaidAPIKey", "logonResponse is null");
            }
            String apiKey = logonResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<String>("PataPawaPostPaidAPIKey");

            if (String.IsNullOrEmpty(apiKey)) {
                throw new ArgumentNullException("PataPawaPostPaidAPIKey", "APIKey is a required field for this transaction type");
            }

            // Check the meta data for the sale message type
            String messageType = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("PataPawaPostPaidMessageType");

            if (String.IsNullOrEmpty(messageType)) {
                throw new ArgumentNullException("PataPawaPostPaidMessageType", "Message Type is a required field for this transaction type");
            }

            String accountNumber = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("CustomerAccountNumber");

            if (String.IsNullOrEmpty(accountNumber)) {
                throw new ArgumentNullException("CustomerAccountNumber","Customer Account Number is a required field for this transaction type");
            }

            return messageType switch {
                "VerifyAccount" => await this.PerformVerifyAccountTransaction(accountNumber, apiKey),
                "ProcessBill" => await this.PerformProcessBillTransaction(accountNumber, apiKey, additionalTransactionMetadata),
                _ => throw new ArgumentOutOfRangeException("PataPawaPostPaidMessageType", $"Unsupported Message Type {messageType}")
            };
        }

        private async Task<OperatorResponse> PerformProcessBillTransaction(String accountNumber,
                                                                           String apiKey,
                                                                           Dictionary<String, String> additionalTransactionMetadata) {
            String mobileNumber = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("MobileNumber");
            String customerName = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("CustomerName");
            String transactionAmount = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("Amount");

            if (String.IsNullOrEmpty(mobileNumber)) {
                throw new ArgumentNullException("MobileNumber", "MobileNumber is a required field for this transaction type");
            }

            if (String.IsNullOrEmpty(customerName)) {
                throw new ArgumentNullException("CustomerName","CustomerName is a required field for this transaction type");
            }

            if (String.IsNullOrEmpty(transactionAmount)) {
                throw new ArgumentNullException("Amount","Amount is a required field for this transaction type");
            }

            // Multiply amount before sending
            // Covert the transaction amount to Decimal and remove decimal places
            if (Decimal.TryParse(transactionAmount, out Decimal amountAsDecimal) == false) {
                throw new ArgumentOutOfRangeException("Amount","Transaction Amount is not a valid decimal value");
            }

            Decimal operatorTransactionAmount = amountAsDecimal * 100;
            IPataPawaPostPayService channel = this.ChannelResolver(this.ServiceClient, this.Configuration.Url);
            paybill payBillResponse = await channel.getPayBillRequestAsync(this.Configuration.Username,
                                                                                      apiKey,
                                                                                      accountNumber,
                                                                                      mobileNumber,
                                                                                      customerName,
                                                                                      operatorTransactionAmount);

            if (payBillResponse.status != 0) {
                throw new Exception($"Error paying bill for account number {accountNumber}");
            }

            return new OperatorResponse {
                                            ResponseMessage = payBillResponse.msg,
                                            IsSuccessful = true,
                                            ResponseCode = payBillResponse.rescode,
                                            TransactionId = payBillResponse.receipt_no,
                                        };
        }

        private async Task<OperatorResponse> PerformVerifyAccountTransaction(String accountNumber,
                                                                             String apiKey) {

            IPataPawaPostPayService channel = this.ChannelResolver(this.ServiceClient, this.Configuration.Url);
            verify verifyResponse = await channel.getVerifyRequestAsync(this.Configuration.Username, apiKey, accountNumber);

            if (String.IsNullOrEmpty(verifyResponse.account_name)) {
                throw new Exception($"Error verifying account number {accountNumber}");
            }

            OperatorResponse operatorResponse = new OperatorResponse {
                                                                         IsSuccessful = true,
                                                                         ResponseMessage = "SUCCESS",
                                                                         AdditionalTransactionResponseMetadata = new Dictionary<String, String>()
                                                                     };

            operatorResponse.AdditionalTransactionResponseMetadata.Add("CustomerBillBalance", verifyResponse.account_balance.ToString());
            operatorResponse.AdditionalTransactionResponseMetadata.Add("CustomerAccountNumber", verifyResponse.account_no);
            operatorResponse.AdditionalTransactionResponseMetadata.Add("CustomerAccountName", verifyResponse.account_name);
            operatorResponse.AdditionalTransactionResponseMetadata.Add("CustomerBillDueDate", verifyResponse.due_date.ToString("yyyy-MM-dd"));

            return operatorResponse;
        }

        #endregion
    }
}