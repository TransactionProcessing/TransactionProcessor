using System.Diagnostics.CodeAnalysis;
using TransactionProcessing.UnionResults;

namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPostPay
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using global::PataPawaPostPay;
    using Microsoft.Extensions.Caching.Memory;

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

        public async Task<Result<OperatorResponse>> ProcessLogonMessage(CancellationToken cancellationToken) {
            try {
                // Check if we need to do a logon with the operator
                OperatorResponse operatorResponse = this.MemoryCache.Get<OperatorResponse>("PataPawaPostPayLogon");
                if (operatorResponse != null) {
                    return Results.Ok(operatorResponse);
                }

                IPataPawaPostPayService channel = this.ChannelResolver(this.ServiceClient, "PataPawaPostPay", this.Configuration.Url);
                login logonResponse = await channel.getLoginRequestAsync(this.Configuration.Username, this.Configuration.Password);
                if (logonResponse.status != 0) {
                    return new Error(ErrorType.Failure, $"Error logging on with PataPawa Post Paid API, Response is {logonResponse.status}");
                }

                operatorResponse = new OperatorResponse { IsSuccessful = true, ResponseCode = "0000", ResponseMessage = logonResponse.message, AdditionalTransactionResponseMetadata = new Dictionary<String, String>() };

                operatorResponse.AdditionalTransactionResponseMetadata.Add("PataPawaPostPaidAPIKey", logonResponse.api_key);
                operatorResponse.AdditionalTransactionResponseMetadata.Add("PataPawaPostPaidBalance", logonResponse.balance.ToString());

                this.MemoryCache.Set("PataPawaPostPayLogon", operatorResponse, MemoryCacheEntryOptions);

                return Results.Ok(operatorResponse);
            }
            catch (Exception ex) {
                return new Error(ErrorType.Failure, $"Error processing logon message for PataPawaPostPay [{ex.Message}]");
            }
        }

        private MemoryCacheEntryOptions MemoryCacheEntryOptions =>
            new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove).SetSlidingExpiration(TimeSpan.FromHours(1))
                                         .RegisterPostEvictionCallback(PostEvictionCallback);

        [ExcludeFromCodeCoverage]
        private void PostEvictionCallback(Object key,
                                          Object value,
                                          EvictionReason reason,
                                          Object state) {
            if (key.ToString().Contains("Logon")) {
                ProcessLogonMessage(CancellationToken.None).Wait();
            }
        }

        public async Task<Result<OperatorResponse>> ProcessSaleMessage(Guid transactionId,
                                                                       Guid operatorId,
                                                                       Models.Merchant.Merchant merchant,
                                                                       DateTimeOffset transactionDateTime,
                                                                       String transactionReference,
                                                                       Dictionary<String, String> additionalTransactionMetadata,
                                                                       CancellationToken cancellationToken) {
            // Get the logon response for the operator
            OperatorResponse logonResponse = this.MemoryCache.Get<OperatorResponse>("PataPawaPostPayLogon");
            if (logonResponse == null) {
                return new Error(ErrorType.Validation, $"PataPawaPostPaidAPIKey - logonResponse is null");
            }
            String apiKey = logonResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<String>("PataPawaPostPaidAPIKey");

            if (String.IsNullOrEmpty(apiKey)) {
                return new Error(ErrorType.Validation, $"PataPawaPostPaidAPIKey - APIKey is a required field for this transaction type");
            }

            // Check the meta data for the sale message type
            String messageType = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("PataPawaPostPaidMessageType");

            if (String.IsNullOrEmpty(messageType)) {
                return new Error(ErrorType.Validation, "PataPawaPostPaidMessageType - Message Type is a required field for this transaction type");
            }

            String accountNumber = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("CustomerAccountNumber");

            if (String.IsNullOrEmpty(accountNumber)) {
                return new Error(ErrorType.Validation, "CustomerAccountNumber - Customer Account Number is a required field for this transaction type");
            }

            return messageType switch {
                "VerifyAccount" => await this.PerformVerifyAccountTransaction(accountNumber, apiKey, cancellationToken),
                "ProcessBill" => await this.PerformProcessBillTransaction(accountNumber, apiKey, additionalTransactionMetadata, cancellationToken),
                _ => new Error(ErrorType.Validation, $"PataPawaPostPaidMessageType - Unsupported Message Type {messageType}")
            };
        }

        private async Task<Result<OperatorResponse>> PerformProcessBillTransaction(String accountNumber,
                                                                           String apiKey,
                                                                           Dictionary<String, String> additionalTransactionMetadata,
                                                                           CancellationToken cancellationToken) {
            try {
                cancellationToken.ThrowIfCancellationRequested();

                String mobileNumber = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("MobileNumber");
                String customerName = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("CustomerName");
                String transactionAmount = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("Amount");

                if (String.IsNullOrEmpty(mobileNumber)) {
                    return new Error(ErrorType.Validation, "MobileNumber - MobileNumber is a required field for this transaction type");
                }

                if (String.IsNullOrEmpty(customerName)) {
                    return new Error(ErrorType.Validation, "CustomerName - CustomerName is a required field for this transaction type");
                }

                if (String.IsNullOrEmpty(transactionAmount)) {
                    return new Error(ErrorType.Validation, "Amount - Amount is a required field for this transaction type");
                }

                // Multiply amount before sending
                // Covert the transaction amount to Decimal and remove decimal places
                if (Decimal.TryParse(transactionAmount, out Decimal amountAsDecimal) == false) {
                    return new Error(ErrorType.Validation, "Amount - Transaction Amount is not a valid decimal value");
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
                    return new Error(ErrorType.Failure, $"Error paying bill for account number {accountNumber} api status {payBillResponse.status}");
                }

                return  Results.Ok(new OperatorResponse {
                                                ResponseMessage = payBillResponse.msg,
                                                IsSuccessful = true,
                                                ResponseCode = payBillResponse.rescode,
                                                TransactionId = payBillResponse.receipt_no,
                                            });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                throw;
            }
            catch (Exception ex) {
                return new Error(ErrorType.Failure, $"Error paying bill for account number {accountNumber} [{ex.Message}]");
            }
        }

        private async Task<Result<OperatorResponse>> PerformVerifyAccountTransaction(String accountNumber,
                                                                             String apiKey,
                                                                             CancellationToken cancellationToken) {
            try {
                cancellationToken.ThrowIfCancellationRequested();

                IPataPawaPostPayService channel = this.ChannelResolver(this.ServiceClient, "PataPawaPostPay", this.Configuration.Url);
                verify verifyResponse = await channel.getVerifyRequestAsync(this.Configuration.Username, apiKey, accountNumber);

                if (String.IsNullOrEmpty(verifyResponse.account_name)) {
                    return new Error(ErrorType.NotFound, $"Error verifying account number {accountNumber}");
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

                return Results.Ok(operatorResponse);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                throw;
            }
            catch (Exception ex) {
                return new Error(ErrorType.Failure, $"Error verifying account number {accountNumber} [{ex.Message}]");
            }
        }

        #endregion
    }
}
