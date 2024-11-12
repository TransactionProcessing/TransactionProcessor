using SimpleResults;

namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPostPay
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using global::PataPawaPostPay;
    using Microsoft.Extensions.Caching.Memory;
    using Shared.Logger;

    public class PataPawaPostPayProxy : IOperatorProxy
    {
        #region Fields

        private readonly PataPawaPostPaidConfiguration Configuration;

        private readonly IMemoryCache MemoryCache;

        private readonly PataPawaPostPayServiceClient ServiceClient;

        private readonly Func<PataPawaPostPayServiceClient, String,String, IPataPawaPostPayService> ChannelResolver;

        #endregion

        #region Constructors

        public PataPawaPostPayProxy(PataPawaPostPayServiceClient serviceClient,
                                    Func<PataPawaPostPayServiceClient, String, String, IPataPawaPostPayService> channelResolver,
                                    PataPawaPostPaidConfiguration configuration,
                                    IMemoryCache memoryCache) {
            this.ServiceClient = serviceClient;
            this.ChannelResolver = channelResolver;
            this.Configuration = configuration;
            this.MemoryCache = memoryCache;
        }

        #endregion

        #region Methods

        public async Task<Result<OperatorResponse>> ProcessLogonMessage(String accessToken,
                                                                        CancellationToken cancellationToken) {

            // Check if we need to do a logon with the operator
            OperatorResponse operatorResponse = this.MemoryCache.Get<OperatorResponse>("PataPawaPostPayLogon");
            if (operatorResponse != null){
                return operatorResponse;
            }

            IPataPawaPostPayService channel = this.ChannelResolver(this.ServiceClient, "PataPawaPostPay", this.Configuration.Url);
            login logonResponse = await channel.getLoginRequestAsync(this.Configuration.Username, this.Configuration.Password);
            if (logonResponse.status != 0) {
                return Result.Failure($"Error logging on with PataPawa Post Paid API, Response is {logonResponse.status}");
            }

            operatorResponse = new OperatorResponse {
                                                                         IsSuccessful = true,
                                                                         ResponseCode = "0000",
                                                                         ResponseMessage = logonResponse.message,
                                                                         AdditionalTransactionResponseMetadata = new Dictionary<String, String>()
                                                                     };

            operatorResponse.AdditionalTransactionResponseMetadata.Add("PataPawaPostPaidAPIKey", logonResponse.api_key);
            operatorResponse.AdditionalTransactionResponseMetadata.Add("PataPawaPostPaidBalance", logonResponse.balance.ToString());

            this.MemoryCache.Set("PataPawaPostPayLogon", operatorResponse, MemoryCacheEntryOptions);

            return Result.Success(operatorResponse);
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

        public async Task<Result<OperatorResponse>> ProcessSaleMessage(String accessToken,
                                                                       Guid transactionId,
                                                                       Guid operatorId,
                                                                       EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant,
                                                                       DateTime transactionDateTime,
                                                                       String transactionReference,
                                                                       Dictionary<String, String> additionalTransactionMetadata,
                                                                       CancellationToken cancellationToken) {
            // Get the logon response for the operator
            OperatorResponse logonResponse = this.MemoryCache.Get<OperatorResponse>("PataPawaPostPayLogon");
            if (logonResponse == null) {
                return Result.Invalid($"PataPawaPostPaidAPIKey - logonResponse is null");
            }
            String apiKey = logonResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<String>("PataPawaPostPaidAPIKey");

            if (String.IsNullOrEmpty(apiKey)) {
                return Result.Invalid($"PataPawaPostPaidAPIKey - APIKey is a required field for this transaction type");
            }

            // Check the meta data for the sale message type
            String messageType = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("PataPawaPostPaidMessageType");

            if (String.IsNullOrEmpty(messageType)) {
                return Result.Invalid("PataPawaPostPaidMessageType - Message Type is a required field for this transaction type");
            }

            String accountNumber = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("CustomerAccountNumber");

            if (String.IsNullOrEmpty(accountNumber)) {
                return Result.Invalid("CustomerAccountNumber - Customer Account Number is a required field for this transaction type");
            }

            return messageType switch {
                "VerifyAccount" => await this.PerformVerifyAccountTransaction(accountNumber, apiKey),
                "ProcessBill" => await this.PerformProcessBillTransaction(accountNumber, apiKey, additionalTransactionMetadata),
                _ => Result.Invalid($"PataPawaPostPaidMessageType - Unsupported Message Type {messageType}")
            };
        }

        private async Task<Result<OperatorResponse>> PerformProcessBillTransaction(String accountNumber,
                                                                           String apiKey,
                                                                           Dictionary<String, String> additionalTransactionMetadata) {
            String mobileNumber = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("MobileNumber");
            String customerName = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("CustomerName");
            String transactionAmount = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("Amount");

            if (String.IsNullOrEmpty(mobileNumber)) {
                return Result.Invalid("MobileNumber - MobileNumber is a required field for this transaction type");
            }

            if (String.IsNullOrEmpty(customerName)) {
                return Result.Invalid("CustomerName - CustomerName is a required field for this transaction type");
            }

            if (String.IsNullOrEmpty(transactionAmount)) {
                return Result.Invalid("Amount - Amount is a required field for this transaction type");
            }

            // Multiply amount before sending
            // Covert the transaction amount to Decimal and remove decimal places
            if (Decimal.TryParse(transactionAmount, out Decimal amountAsDecimal) == false) {
                return Result.Invalid("Amount - Transaction Amount is not a valid decimal value");
            }

            Decimal operatorTransactionAmount = amountAsDecimal * 100;
            IPataPawaPostPayService channel = this.ChannelResolver(this.ServiceClient, "PataPawaPostPay", this.Configuration.Url);
            paybill payBillResponse = await channel.getPayBillRequestAsync(this.Configuration.Username,
                                                                           apiKey,
                                                                           accountNumber,
                                                                           mobileNumber,
                                                                           customerName,
                                                                           operatorTransactionAmount);

            if (payBillResponse.status != 0) {
                return Result.Failure($"Error paying bill for account number {accountNumber} api status {payBillResponse.status}");
            }

            return  Result.Success(new OperatorResponse {
                                            ResponseMessage = payBillResponse.msg,
                                            IsSuccessful = true,
                                            ResponseCode = payBillResponse.rescode,
                                            TransactionId = payBillResponse.receipt_no,
                                        });
        }

        private async Task<Result<OperatorResponse>> PerformVerifyAccountTransaction(String accountNumber,
                                                                             String apiKey) {

            IPataPawaPostPayService channel = this.ChannelResolver(this.ServiceClient, "PataPawaPostPay", this.Configuration.Url);
            verify verifyResponse = await channel.getVerifyRequestAsync(this.Configuration.Username, apiKey, accountNumber);

            if (String.IsNullOrEmpty(verifyResponse.account_name)) {
                return Result.NotFound($"Error verifying account number {accountNumber}");
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

            return Result.Success(operatorResponse);
        }

        #endregion
    }
}