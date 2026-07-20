using Shared.Logger;
using Shared.Serialisation;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPostPay;
using TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPrePay;
using TransactionProcessor.BusinessLogic.OperatorInterfaces.SafaricomPinless;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Models.Merchant;
using Merchant = TransactionProcessor.Models.Merchant.Merchant;

namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.AgencyBanking
{
    public class AgencyBankingProxy:IOperatorProxy

    {
        private readonly HttpClient HttpClient;
        private readonly AgencyBankingConfiguration Configuration;
        private static SerialiserOptions SerialiserOptions = new SerialiserOptions(SerialiserPropertyFormat.CamelCase);
        public AgencyBankingProxy(HttpClient httpClient, AgencyBankingConfiguration configuration) {
            HttpClient = httpClient;
            this.Configuration = configuration;
        }

        public async Task<Result<OperatorResponse>> ProcessLogonMessage(CancellationToken cancellationToken) {
            Logger.LogInformation("AgencyBanking logon requested");
            return Result.Success();
        }

        public async Task<Result<OperatorResponse>> ProcessSaleMessage(Guid transactionId,
                                                                       Guid operatorId,
                                                                       Merchant merchant,
                                                                       DateTime transactionDateTime,
                                                                       String transactionReference,
                                                                       Dictionary<String, String> additionalTransactionMetadata,
                                                                       CancellationToken cancellationToken) {
            Logger.LogInformation($"AgencyBanking sale request received. TransactionId=[{transactionId}], OperatorId=[{operatorId}], MerchantId=[{merchant.MerchantId}], TransactionDateTime=[{transactionDateTime:o}], TransactionReference=[{transactionReference}]");

            // Check the meta data for the message type
            String messageType = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("AgencyBankingMessageType");

            if (String.IsNullOrEmpty(messageType))
            {
                Logger.LogWarning($"AgencyBanking sale request rejected. TransactionId=[{transactionId}] missing AgencyBankingMessageType");
                return Result.Invalid("AgencyBankingMessageType - Message Type is a required field for this transaction type");
            }

            //// Check the meta data for the agent id
            //String agentId = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("AgencyBankingAgentId");

            //if (String.IsNullOrEmpty(agentId))
            //{
            //    return Result.Invalid("AgencyBankingAgentId - Agent Id is a required field for this transaction type");
            //}

            // Check the meta data for the account number
            String accountNumber = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("AgencyBankingAccountNumber");

            if (String.IsNullOrEmpty(accountNumber))
            {
                Logger.LogWarning($"AgencyBanking sale request rejected. TransactionId=[{transactionId}] missing AgencyBankingAccountNumber");
                return Result.Invalid("AgencyBankingAccountNumber - Account Number is a required field for this transaction type");
            }

            Logger.LogInformation($"AgencyBanking sale request metadata resolved. TransactionId=[{transactionId}], MessageType=[{messageType}], AccountNumber=[{accountNumber}]");

            if (messageType == "balanceenquiry")
            {
                return await ProcessBalanceEnquiry(transactionId, merchant.MerchantId.ToString(), accountNumber, cancellationToken);
            }

            Logger.LogWarning($"AgencyBanking sale request rejected. TransactionId=[{transactionId}] unsupported message type [{messageType}]");
            return Result.Invalid($"AgencyBankingMessageType - Message Type {messageType} is not supported for this transaction type");
        }

        private async Task<Result<OperatorResponse>> ProcessBalanceEnquiry(Guid transactionId,
                                                                           String agentId,
                                                                           String accountNumber,
                                                                           CancellationToken cancellationToken) {
            HttpRequestMessage request = new(HttpMethod.Post, $"{this.Configuration.Url}/transactions/balance-enquiry");

            BalanceEnquiryRequest body = new BalanceEnquiryRequest(){
                AgentId = agentId,
                AccountNumber = accountNumber
            };

            String requestBody = StringSerialiser.Serialise(body, SerialiserOptions);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            Logger.LogWarning($"AgencyBanking balance enquiry request starting. TransactionId=[{transactionId}], AgentId=[{agentId}], AccountNumber=[{accountNumber}], RequestUri=[{request.RequestUri}], RequestBody=[{requestBody}]");

            HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);
            String responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            Logger.LogWarning($"AgencyBanking balance enquiry response received. TransactionId=[{transactionId}], StatusCode=[{response.StatusCode}], ContentLength=[{responseContent?.Length ?? 0}]");

            if (response.IsSuccessStatusCode == false) {
                Logger.LogWarning($"AgencyBanking balance enquiry failed. TransactionId=[{transactionId}], StatusCode=[{response.StatusCode}], ResponseBody=[{responseContent}]");
                return Result.Invalid($"Failed to process balance enquiry {responseContent}");
            }

            BalanceEnquiryResponse balanceEnquiryResponse = StringSerialiser.Deserialise<BalanceEnquiryResponse>(responseContent, SerialiserOptions);
            Logger.LogWarning($"AgencyBanking balance enquiry completed. TransactionId=[{transactionId}], ResponseMessage=[{balanceEnquiryResponse.ResponseMessage}], AvailableBalance=[{balanceEnquiryResponse.AvailableBalance}]");

            OperatorResponse operatorResponse = new() {
                IsSuccessful = true,
                ResponseCode = "0000",
                ResponseMessage = balanceEnquiryResponse.ResponseMessage,
                AdditionalTransactionResponseMetadata = new Dictionary<String, String>()
            };

            operatorResponse.AdditionalTransactionResponseMetadata.Add("AgencyBankingBalance", balanceEnquiryResponse.AvailableBalance.ToString());

            return Result.Success(operatorResponse);
        }
    }
}
