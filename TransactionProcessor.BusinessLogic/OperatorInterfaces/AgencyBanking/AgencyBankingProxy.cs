using Shared.Logger;
using Shared.Serialisation;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private static SerialiserOptions SerialiserOptions = new(SerialiserPropertyFormat.CamelCase);
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
            try {
                Logger.LogInformation("AgencyBanking sale request received. TransactionId=[{TransactionId}], OperatorId=[{OperatorId}], MerchantId=[{MerchantId}], TransactionDateTime=[{TransactionDateTime}], TransactionReference=[{TransactionReference}]",
                    transactionId, operatorId, merchant.MerchantId, transactionDateTime.ToString("O"), transactionReference);

                // Check the meta data for the message type
                String messageType = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("AgencyBankingMessageType");

                if (String.IsNullOrEmpty(messageType))
                {
                    Logger.LogWarning("AgencyBanking sale request rejected. TransactionId=[{TransactionId}] missing AgencyBankingMessageType", transactionId);
                    return Result.Invalid("AgencyBankingMessageType - Message Type is a required field for this transaction type");
                }
            
                // Check the meta data for the account number
                String accountNumber = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("AgencyBankingAccountNumber");

                if (String.IsNullOrEmpty(accountNumber))
                {
                    Logger.LogWarning("AgencyBanking sale request rejected. TransactionId=[{TransactionId}] missing AgencyBankingAccountNumber", transactionId);
                    return Result.Invalid("AgencyBankingAccountNumber - Account Number is a required field for this transaction type");
                }

                Logger.LogInformation("AgencyBanking sale request metadata resolved. TransactionId=[{TransactionId}], MessageType=[{MessageType}], AccountNumber=[{AccountNumber}]", transactionId, messageType, accountNumber);

                Result<OperatorResponse> result = messageType switch {
                    "balanceenquiry" => await ProcessBalanceEnquiry(transactionId, merchant.MerchantId.ToString(), accountNumber, cancellationToken),
                    "deposit" => await ProcessDeposit(transactionId, merchant.MerchantId.ToString(), accountNumber, additionalTransactionMetadata, cancellationToken),
                    "withdrawal" => await ProcessWithdrawal(transactionId, merchant.MerchantId.ToString(), accountNumber, additionalTransactionMetadata, cancellationToken),
                    "ministatement" => await ProcessMiniStatement(merchant.MerchantId.ToString(), accountNumber, cancellationToken),
                    _ => Result.Invalid($"AgencyBankingMessageType - Message Type {messageType} is not supported for this transaction type")
                };
            
                if (result.Status == ResultStatus.Invalid) {
                    Logger.LogWarning("AgencyBanking sale request rejected. TransactionId=[{TransactionId}] unsupported message type [{MessageType}]", transactionId, messageType);
                }

                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                throw;
            }
            catch (Exception ex) {
                Logger.LogError(ex,"AgencyBanking sale request failed. TransactionId=[{TransactionId}]", transactionId);
                return Result.Failure($"AgencyBanking request failed [{ex.Message}]");
            }
        }

        private static StringContent CreateContent<T>(T body) {
            String requestBody = StringSerialiser.Serialise(body, SerialiserOptions);
            return new StringContent(requestBody, Encoding.UTF8, "application/json");
        }
        
        private async Task<Result<OperatorResponse>> ProcessWithdrawal(Guid transactionId,
                                                                       String agentId,
                                                                       String accountNumber,
                                                                       Dictionary<String, String> additionalTransactionMetadata,
                                                                       CancellationToken cancellationToken) {
            HttpRequestMessage request = new(HttpMethod.Post, $"{this.Configuration.Url}/transactions/withdrawal");
            // Extract required data from metadata
            String amountValue = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("Amount");

            if (String.IsNullOrWhiteSpace(amountValue))
            {
                Logger.LogWarning("AgencyBanking withdrawal request rejected. TransactionId=[{TransactionId}] missing Amount", transactionId);
                return Result.Invalid("Amount - Amount is a required field for this transaction type");
            }

            if (Decimal.TryParse(amountValue, out Decimal amount) == false)
            {
                Logger.LogWarning("AgencyBanking withdrawal request rejected. TransactionId=[{TransactionId}] invalid Amount [{Amount}]", amountValue);
                return Result.Invalid("Amount - Amount is not a valid decimal value");
            }

            WithdrawalRequest body = new() { AgentId = agentId, AccountNumber = accountNumber, Amount = amount, TransactionId = transactionId.ToString() };

            request.Content = CreateContent(body);
            string requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            Logger.LogWarning("AgencyBanking withdrawal request starting. TransactionId=[{TransactionId}], AgentId=[{AgentId}], AccountNumber=[{AccountNumber}], RequestUri=[{Uri}], RequestBody=[{requestBody}]",
                transactionId, agentId, accountNumber, request.RequestUri, requestBody);

            HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);
            String responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            Logger.LogWarning("AgencyBanking withdrawal response received. TransactionId=[{TransactionId}], StatusCode=[{StatusCode}], ContentLength=[{ContentLength}]",
                transactionId, response.StatusCode, responseContent?.Length ?? 0);

            if (response.IsSuccessStatusCode == false)
            {
                Logger.LogWarning("AgencyBanking withdrawal failed. TransactionId=[{TransactionId}], StatusCode=[{StatusCode}], ResponseBody=[{ResponseBody}]",transactionId, response.StatusCode, responseContent);
                return Result.Invalid($"Failed to process withdrawal {responseContent}");
            }

            TransactionResult withdrawalResponse = StringSerialiser.Deserialise<TransactionResult>(responseContent, SerialiserOptions);
            Logger.LogWarning("AgencyBanking withdrawal completed. TransactionId=[{TransactionId}], ResponseCode=[{ResponseCode}]", transactionId, withdrawalResponse.ResponseCode);

            OperatorResponse operatorResponse = new()
            {
                IsSuccessful = true,
                ResponseCode = "0000",
                ResponseMessage = "SUCCESS",
                AdditionalTransactionResponseMetadata = new Dictionary<String, String>()
            };

            return Result.Success(operatorResponse);
        }

        private async Task<Result<OperatorResponse>> ProcessMiniStatement(String agentId,
                                                                          String accountNumber,
                                                                          CancellationToken cancellationToken) {
            HttpRequestMessage request = new(HttpMethod.Post, $"{this.Configuration.Url}/transactions/mini-statement");

            MiniStatementRequest body = new() { AgentId = agentId, AccountNumber = accountNumber };

            request.Content = CreateContent(body);
            string requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            Logger.LogWarning("AgencyBanking mini-statement request starting. AgentId=[{AgentId}], AccountNumber=[{AccountNumber}], RequestUri=[{RequestUri}], RequestBody=[{RequestBody}]", agentId, accountNumber, request.RequestUri, requestBody);

            HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);
            String responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            Logger.LogWarning("AgencyBanking mini-statement response received. AgentId=[{AgentId}], AccountNumber=[{AccountNumber}], StatusCode=[{StatusCode}], ContentLength=[{ContentLength}]",
                agentId, accountNumber, response.StatusCode, responseContent?.Length ?? 0);

            if (response.IsSuccessStatusCode == false) {
                Logger.LogWarning("AgencyBanking mini-statement failed. AgentId=[{AgentId}], AccountNumber=[{AccountNumber}], StatusCode=[{StatusCode}], ResponseBody=[{ResponseContent}]",
                    agentId, accountNumber, response.StatusCode, responseContent);
                return Result.Invalid($"Failed to process mini-statement {responseContent}");
            }

            MiniStatementResponse miniStatementResponse = StringSerialiser.Deserialise<MiniStatementResponse>(responseContent, SerialiserOptions);
            Logger.LogWarning("AgencyBanking mini-statement completed. AgentId=[{AgentId}], AccountNumber=[{AccountNumber}], ResponseCode=[{ResponseCode}]", agentId, accountNumber, miniStatementResponse.ResponseCode);

            // Transform the mini-statement response into an operator response
            List<OperatorStatementLine> statementLines = miniStatementResponse.Transactions.Select(line => new OperatorStatementLine { Amount = line.Amount, TransactionDate = line.TransactionDate, TransactionType = line.TransactionType }).ToList();


            OperatorResponse operatorResponse = new() { IsSuccessful = true, ResponseCode = "0000", ResponseMessage = "SUCCESS", AdditionalTransactionResponseMetadata = new Dictionary<String, String>() };

            operatorResponse.AdditionalTransactionResponseMetadata.Add("StatementLines", StringSerialiser.Serialise(statementLines));

            return Result.Success(operatorResponse);
        }

        private async Task<Result<OperatorResponse>> ProcessDeposit(Guid transactionId,
                                                                    String agentId,
                                                                    String accountNumber,
                                                                    Dictionary<String, String> additionalTransactionMetadata,
                                                                    CancellationToken cancellationToken) {

            HttpRequestMessage request = new(HttpMethod.Post, $"{this.Configuration.Url}/transactions/deposit");
            // Extract required data from metadata
            String amountValue = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("Amount");

            if (String.IsNullOrWhiteSpace(amountValue))
            {
                Logger.LogWarning("AgencyBanking deposit request rejected. TransactionId=[{TransactionId}] missing Amount", transactionId);
                return Result.Invalid("Amount - Amount is a required field for this transaction type");
            }

            if (Decimal.TryParse(amountValue, out Decimal amount) == false)
            {
                Logger.LogWarning("AgencyBanking deposit request rejected. TransactionId=[{TransactionId}] invalid Amount [{Amount}]", transactionId, amountValue);
                return Result.Invalid("Amount - Amount is not a valid decimal value");
            }

            DepositRequest body = new() { AgentId = agentId, AccountNumber = accountNumber, Amount = amount, TransactionId = transactionId.ToString() };
            request.Content = CreateContent(body);
            string requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            Logger.LogWarning("AgencyBanking deposit request starting. TransactionId=[{TransactionId}], AgentId=[{AgentId}], AccountNumber=[{AccountNumber}], RequestUri=[{RequestUri}], RequestBody=[{RequestBody}]",
                transactionId, agentId, accountNumber, request.RequestUri, requestBody);

            HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);
            String responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            Logger.LogWarning("AgencyBanking deposit response received. TransactionId=[{TransactionId}], StatusCode=[{StatusCode}], ContentLength=[{ContentLength}]",
                transactionId, response.StatusCode, responseContent?.Length ?? 0);

            if (response.IsSuccessStatusCode == false)
            {
                Logger.LogWarning("AgencyBanking deposit failed. TransactionId=[{TransactionId}], StatusCode=[{StatusCode}], ResponseBody=[{ResponseContent}]", transactionId, response.StatusCode, responseContent);
                return Result.Invalid($"Failed to process deposit {responseContent}");
            }

            TransactionResult depositResponse = StringSerialiser.Deserialise<TransactionResult>(responseContent, SerialiserOptions);
            Logger.LogWarning("AgencyBanking deposit completed. TransactionId=[{TransactionId}], ResponseCode=[{ResponseCode}]", transactionId, depositResponse.ResponseCode);

            OperatorResponse operatorResponse = new()
            {
                IsSuccessful = true,
                ResponseCode = "0000",
                ResponseMessage = "SUCCESS",
                AdditionalTransactionResponseMetadata = new Dictionary<String, String>()
            };
            
            return Result.Success(operatorResponse);
        }

        private async Task<Result<OperatorResponse>> ProcessBalanceEnquiry(Guid transactionId,
                                                                           String agentId,
                                                                           String accountNumber,
                                                                           CancellationToken cancellationToken) {
            HttpRequestMessage request = new(HttpMethod.Post, $"{this.Configuration.Url}/transactions/balance-enquiry");

            BalanceEnquiryRequest body = new(){
                AgentId = agentId,
                AccountNumber = accountNumber
            };

            request.Content = CreateContent(body);
            string requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            Logger.LogWarning("AgencyBanking balance enquiry request starting. TransactionId=[{TransactionId}], AgentId=[{AgentId}], AccountNumber=[{AccountNumber}], RequestUri=[{Uri}], RequestBody=[{RequestBody}]",
                transactionId, agentId, accountNumber, request.RequestUri, requestBody);

            HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);
            String responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            Logger.LogWarning("AgencyBanking balance enquiry response received. TransactionId=[{TransactionId}], StatusCode=[{StatusCode}], ContentLength=[{ContentLength}]",
                transactionId, response.StatusCode, responseContent?.Length ?? 0);

            if (response.IsSuccessStatusCode == false) {
                Logger.LogWarning("AgencyBanking balance enquiry failed. TransactionId=[{TransactionId}], StatusCode=[{StatusCode}], ResponseBody=[{ResponseContent}]",
                    transactionId, response.StatusCode, responseContent);
                return Result.Invalid($"Failed to process balance enquiry {responseContent}");
            }

            BalanceEnquiryResponse balanceEnquiryResponse = StringSerialiser.Deserialise<BalanceEnquiryResponse>(responseContent, SerialiserOptions);
            Logger.LogWarning("AgencyBanking balance enquiry completed. TransactionId=[{TransactionId}], ResponseMessage=[{ResponseMessage}], AvailableBalance=[{AvailableBalance}]",
                transactionId, balanceEnquiryResponse.ResponseMessage, balanceEnquiryResponse.AvailableBalance);

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
