using Newtonsoft.Json;
using Shared.Exceptions;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Estate;
using TransactionProcessor.Models.Merchant;
using Contract = TransactionProcessor.Models.Contract.Contract;
using Operator = TransactionProcessor.Models.Estate.Operator;

namespace TransactionProcessor.BusinessLogic.Services{
    using Common;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Microsoft.Extensions.Caching.Memory;
    using Models;
    using OperatorInterfaces;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TransactionProcessor.BusinessLogic.Manager;
    using TransactionProcessor.BusinessLogic.Requests;

    public interface ITransactionDomainService
    {
        #region Methods

        Task<Result<ProcessLogonTransactionResponse>> ProcessLogonTransaction(TransactionCommands.ProcessLogonTransactionCommand command,
                                                                              CancellationToken cancellationToken);

        Task<Result<ProcessSaleTransactionResponse>> ProcessSaleTransaction(TransactionCommands.ProcessSaleTransactionCommand command,
                                                                            CancellationToken cancellationToken);

        Task<Result<ProcessReconciliationTransactionResponse>> ProcessReconciliationTransaction(TransactionCommands.ProcessReconciliationCommand command,
                                                                                                CancellationToken cancellationToken);

        Task<Result> ResendTransactionReceipt(TransactionCommands.ResendTransactionReceiptCommand command,
                                              CancellationToken cancellationToken);

        Task<Result> CalculateFeesForTransaction(TransactionCommands.CalculateFeesForTransactionCommand command,
                                                 CancellationToken cancellationToken);

        #endregion

        Task<Result> AddSettledMerchantFee(TransactionCommands.AddSettledMerchantFeeCommand command,
                                           CancellationToken cancellationToken);

        Task<Result> SendCustomerEmailReceipt(TransactionCommands.SendCustomerEmailReceiptCommand command,
                                              CancellationToken cancellationToken);
        Task<Result> ResendCustomerEmailReceipt(TransactionCommands.ResendCustomerEmailReceiptCommand command,
                                                CancellationToken cancellationToken);
    }

    public class TransactionDomainService : ITransactionDomainService {
        #region Fields

        private readonly IAggregateService AggregateService;
        private readonly Func<String, IOperatorProxy> OperatorProxyResolver;
        private readonly ISecurityServiceClient SecurityServiceClient;
        private readonly IMemoryCacheWrapper MemoryCache;
        private readonly IFeeCalculationManager FeeCalculationManager;
        private readonly ITransactionReceiptBuilder TransactionReceiptBuilder;
        private readonly IMessagingServiceClient MessagingServiceClient;
        private TokenResponse TokenResponse;
        private readonly ITransactionValidationService TransactionValidationService;
        
        #endregion

        #region Constructors

        public TransactionDomainService(IAggregateService aggregateService,
                                        Func<String, IOperatorProxy> operatorProxyResolver,
                                        ITransactionValidationService transactionValidationService,
                                        ISecurityServiceClient securityServiceClient,
                                        IMemoryCacheWrapper memoryCache,
                                        IFeeCalculationManager feeCalculationManager,
                                        ITransactionReceiptBuilder transactionReceiptBuilder,
                                        IMessagingServiceClient messagingServiceClient) {
            this.AggregateService = aggregateService;
            this.OperatorProxyResolver = operatorProxyResolver;
            this.TransactionValidationService = transactionValidationService;
            this.SecurityServiceClient = securityServiceClient;
            this.MemoryCache = memoryCache;
            this.FeeCalculationManager = feeCalculationManager;
            this.TransactionReceiptBuilder = transactionReceiptBuilder;
            this.MessagingServiceClient = messagingServiceClient;
        }

        #endregion

        private async Task<Result<T>> ApplyUpdates<T>(Func<TransactionAggregate, Task<Result<T>>> action,
                                                      Guid transactionId,
                                                      CancellationToken cancellationToken,
                                                      Boolean isNotFoundError = true) {
            try {

                Result<TransactionAggregate> getTransactionResult = await this.AggregateService.GetLatest<TransactionAggregate>(transactionId, cancellationToken);
                Result<TransactionAggregate> transactionAggregateResult = DomainServiceHelper.HandleGetAggregateResult(getTransactionResult, transactionId, isNotFoundError);

                if (transactionAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(transactionAggregateResult);

                TransactionAggregate transactionAggregate = transactionAggregateResult.Data;
                Result<T> result = await action(transactionAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.AggregateService.Save(transactionAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);
                return Result.Success(result.Data);
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        private async Task<Result> ApplyUpdates(Func<TransactionAggregate, Task<Result>> action,
                                                Guid transactionId,
                                                CancellationToken cancellationToken,
                                                Boolean isNotFoundError = true) {
            try {

                Result<TransactionAggregate> getTransactionResult = await this.AggregateService.GetLatest<TransactionAggregate>(transactionId, cancellationToken);
                Result<TransactionAggregate> transactionAggregateResult = DomainServiceHelper.HandleGetAggregateResult(getTransactionResult, transactionId, isNotFoundError);

                if (transactionAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(transactionAggregateResult);

                TransactionAggregate transactionAggregate = transactionAggregateResult.Data;
                Result result = await action(transactionAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.AggregateService.Save(transactionAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);
                return Result.Success();
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        private async Task<Result<T>> ApplyUpdates<T>(Func<ReconciliationAggregate, Task<Result<T>>> action,
                                                      Guid transactionId,
                                                      CancellationToken cancellationToken,
                                                      Boolean isNotFoundError = true) {
            try {

                Result<ReconciliationAggregate> getTransactionResult = await this.AggregateService.GetLatest<ReconciliationAggregate>(transactionId, cancellationToken);
                Result<ReconciliationAggregate> reconciliationAggregateResult = DomainServiceHelper.HandleGetAggregateResult(getTransactionResult, transactionId, isNotFoundError);

                ReconciliationAggregate reconciliationAggregate = reconciliationAggregateResult.Data;
                Result<T> result = await action(reconciliationAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.AggregateService.Save(reconciliationAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);
                return Result.Success(result.Data);
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        #region Methods

        public async Task<Result<ProcessLogonTransactionResponse>> ProcessLogonTransaction(TransactionCommands.ProcessLogonTransactionCommand command,
                                                                                           CancellationToken cancellationToken) {
            Result<ProcessLogonTransactionResponse> result = await ApplyUpdates<ProcessLogonTransactionResponse>(async (TransactionAggregate transactionAggregate) => {
                TransactionType transactionType = TransactionType.Logon;

                // Generate a transaction reference
                String transactionReference = this.GenerateTransactionReference();

                transactionAggregate.StartTransaction(command.TransactionDateTime, command.TransactionNumber, transactionType, transactionReference, command.EstateId, command.MerchantId, command.DeviceIdentifier, null); // Logon transaction has no amount

                Result<TransactionValidationResult> validationResult = await this.TransactionValidationService.ValidateLogonTransaction(command.EstateId, command.MerchantId, command.DeviceIdentifier, cancellationToken);

                if (validationResult.IsSuccess && (validationResult.Data.ResponseCode == TransactionResponseCode.Success || validationResult.Data.ResponseCode == TransactionResponseCode.SuccessNeedToAddDevice)) {
                    if (validationResult.Data.ResponseCode == TransactionResponseCode.SuccessNeedToAddDevice) {
                        await this.AddDeviceToMerchant(command.MerchantId, command.DeviceIdentifier, cancellationToken);
                    }

                    // Record the successful validation
                    // TODO: Generate local authcode
                    transactionAggregate.AuthoriseTransactionLocally("ABCD1234", ((Int32)validationResult.Data.ResponseCode).ToString().PadLeft(4, '0'), validationResult.Data.ResponseMessage);
                }
                else {
                    // Record the failure
                    transactionAggregate.DeclineTransactionLocally(((Int32)validationResult.Data.ResponseCode).ToString().PadLeft(4, '0'), validationResult.Data.ResponseMessage);
                }

                transactionAggregate.CompleteTransaction();

                return Result.Success(new ProcessLogonTransactionResponse {
                    ResponseMessage = transactionAggregate.ResponseMessage,
                    ResponseCode = transactionAggregate.ResponseCode,
                    EstateId = command.EstateId,
                    MerchantId = command.MerchantId,
                    TransactionId = command.TransactionId
                });

            }, command.TransactionId, cancellationToken, false);

            return result;
        }

        public async Task<Result<ProcessReconciliationTransactionResponse>> ProcessReconciliationTransaction(TransactionCommands.ProcessReconciliationCommand command,
                                                                                                             CancellationToken cancellationToken) {

            Result<ProcessReconciliationTransactionResponse> result = await ApplyUpdates<ProcessReconciliationTransactionResponse>(async (ReconciliationAggregate reconciliationAggregate) => {
                Result<TransactionValidationResult> validationResult = await this.TransactionValidationService.ValidateReconciliationTransaction(command.EstateId, command.MerchantId, command.DeviceIdentifier, cancellationToken);

                reconciliationAggregate.StartReconciliation(command.TransactionDateTime, command.EstateId, command.MerchantId);

                reconciliationAggregate.RecordOverallTotals(command.TransactionCount, command.TransactionValue);

                if (validationResult.IsSuccess && validationResult.Data.ResponseCode == TransactionResponseCode.Success) {
                    // Record the successful validation
                    reconciliationAggregate.Authorise(((Int32)validationResult.Data.ResponseCode).ToString().PadLeft(4, '0'), validationResult.Data.ResponseMessage);
                }
                else {
                    // Record the failure
                    reconciliationAggregate.Decline(((Int32)validationResult.Data.ResponseCode).ToString().PadLeft(4, '0'), validationResult.Data.ResponseMessage);
                }

                reconciliationAggregate.CompleteReconciliation();

                return Result.Success(new ProcessReconciliationTransactionResponse {
                    EstateId = reconciliationAggregate.EstateId,
                    MerchantId = reconciliationAggregate.MerchantId,
                    ResponseCode = reconciliationAggregate.ResponseCode,
                    ResponseMessage = reconciliationAggregate.ResponseMessage,
                    TransactionId = command.TransactionId
                });

            }, command.TransactionId, cancellationToken, false);
            return result;
        }

        public async Task<Result<ProcessSaleTransactionResponse>> ProcessSaleTransaction(TransactionCommands.ProcessSaleTransactionCommand command,
                                                                                         CancellationToken cancellationToken) {

            Result<ProcessSaleTransactionResponse> result = await ApplyUpdates<ProcessSaleTransactionResponse>(async (TransactionAggregate transactionAggregate) => {

                TransactionType transactionType = TransactionType.Sale;
                TransactionSource transactionSourceValue = (TransactionSource)command.TransactionSource;

                // Generate a transaction reference
                String transactionReference = this.GenerateTransactionReference();

                // Extract the transaction amount from the metadata
                Decimal? transactionAmount = command.AdditionalTransactionMetadata.ExtractFieldFromMetadata<Decimal?>("Amount");

                Result<TransactionValidationResult> validationResult = await this.TransactionValidationService.ValidateSaleTransaction(command.EstateId, command.MerchantId, command.ContractId, command.ProductId, command.DeviceIdentifier, command.OperatorId, transactionAmount, cancellationToken);

                Logger.LogInformation($"Validation response is [{JsonConvert.SerializeObject(validationResult)}]");

                Guid floatAggregateId = IdGenerationService.GenerateFloatAggregateId(command.EstateId, command.ContractId, command.ProductId);
                Result<FloatAggregate> floatAggregateResult = await this.AggregateService.GetLatest<FloatAggregate>(floatAggregateId, cancellationToken);
                Decimal unitCost = 0;
                Decimal totalCost = 0;
                if (floatAggregateResult.IsSuccess) {
                    // TODO: Move calculation to float
                    FloatAggregate floatAggregate = floatAggregateResult.Data;
                    unitCost = floatAggregate.GetUnitCostPrice();
                    totalCost = transactionAmount.GetValueOrDefault() * unitCost;
                }

                transactionAggregate.StartTransaction(command.TransactionDateTime, command.TransactionNumber, transactionType, transactionReference, command.EstateId, command.MerchantId, command.DeviceIdentifier, transactionAmount);

                // Add the product details (unless invalid estate)
                if (validationResult.Data.ResponseCode != TransactionResponseCode.InvalidEstateId && validationResult.Data.ResponseCode != TransactionResponseCode.InvalidContractIdValue && validationResult.Data.ResponseCode != TransactionResponseCode.InvalidProductIdValue && validationResult.Data.ResponseCode != TransactionResponseCode.ContractNotValidForMerchant && validationResult.Data.ResponseCode != TransactionResponseCode.ProductNotValidForMerchant) {
                    transactionAggregate.AddProductDetails(command.ContractId, command.ProductId);
                }

                transactionAggregate.RecordCostPrice(unitCost, totalCost);

                // Add the transaction source
                transactionAggregate.AddTransactionSource(transactionSourceValue);

                if (validationResult.Data.ResponseCode == TransactionResponseCode.Success) {
                    // Record any additional request metadata
                    transactionAggregate.RecordAdditionalRequestData(command.OperatorId, command.AdditionalTransactionMetadata);

                    // Do the online processing with the operator here
                    Result<Merchant> merchantResult = await this.GetMerchant(command.MerchantId, cancellationToken);
                    if (merchantResult.IsFailed)
                        return ResultHelpers.CreateFailure(merchantResult);
                    Result<OperatorResponse> operatorResult = await this.ProcessMessageWithOperator(merchantResult.Data, command.TransactionId, command.TransactionDateTime, command.OperatorId, command.AdditionalTransactionMetadata, transactionReference, cancellationToken);

                    // Act on the operator response
                    // TODO: see if we still need this case...
                    //if (operatorResult.IsFailed) {
                    //    // Failed to perform sed/receive with the operator
                    //    TransactionResponseCode transactionResponseCode =
                    //        TransactionResponseCode.OperatorCommsError;
                    //    String responseMessage = "OPERATOR COMMS ERROR";

                    //    transactionAggregate.DeclineTransactionLocally(
                    //        ((Int32)transactionResponseCode).ToString().PadLeft(4, '0'), responseMessage);
                    //}
                    //else {

                    if (operatorResult.IsSuccess) {
                        TransactionResponseCode transactionResponseCode = TransactionResponseCode.Success;
                        String responseMessage = "SUCCESS";

                        transactionAggregate.AuthoriseTransaction(command.OperatorId, operatorResult.Data.AuthorisationCode, operatorResult.Data.ResponseCode, operatorResult.Data.ResponseMessage, operatorResult.Data.TransactionId, ((Int32)transactionResponseCode).ToString().PadLeft(4, '0'), responseMessage);
                    }
                    else {
                        TransactionResponseCode transactionResponseCode = TransactionResponseCode.TransactionDeclinedByOperator;
                        String responseMessage = "DECLINED BY OPERATOR";

                        transactionAggregate.DeclineTransaction(command.OperatorId, operatorResult.Data.ResponseCode, operatorResult.Data.ResponseMessage, ((Int32)transactionResponseCode).ToString().PadLeft(4, '0'), responseMessage);
                    }

                    // Record any additional operator response metadata
                    transactionAggregate.RecordAdditionalResponseData(command.OperatorId, operatorResult.Data.AdditionalTransactionResponseMetadata);
                    //}
                }
                else {
                    // Record the failure
                    transactionAggregate.DeclineTransactionLocally(((Int32)validationResult.Data.ResponseCode).ToString().PadLeft(4, '0'), validationResult.Data.ResponseMessage);
                }

                transactionAggregate.CompleteTransaction();

                // Determine if the email receipt is required
                if (String.IsNullOrEmpty(command.CustomerEmailAddress) == false) {
                    transactionAggregate.RequestEmailReceipt(command.CustomerEmailAddress);
                }

                // Get the model from the aggregate
                Models.Transaction transaction = transactionAggregate.GetTransaction();

                return Result.Success(new ProcessSaleTransactionResponse {
                    ResponseMessage = transaction.ResponseMessage,
                    ResponseCode = transaction.ResponseCode,
                    EstateId = command.EstateId,
                    MerchantId = command.MerchantId,
                    AdditionalTransactionMetadata = transaction.AdditionalResponseMetadata,
                    TransactionId = command.TransactionId
                });
            }, command.TransactionId, cancellationToken, false);

            return result;
        }

        public async Task<Result> ResendTransactionReceipt(TransactionCommands.ResendTransactionReceiptCommand command,
                                                           CancellationToken cancellationToken) {

            Result result = await ApplyUpdates(async (TransactionAggregate transactionAggregate) => {
                transactionAggregate.RequestEmailReceiptResend();

                return Result.Success();
            }, command.TransactionId, cancellationToken);
            return result;
        }

        internal static Boolean RequireFeeCalculation(TransactionAggregate transactionAggregate) {
            return transactionAggregate switch {
                _ when transactionAggregate.TransactionType == TransactionType.Logon => false,
                _ when transactionAggregate.IsAuthorised == false => false,
                _ when transactionAggregate.IsCompleted == false => false,
                _ when transactionAggregate.ContractId == Guid.Empty => false,
                _ when transactionAggregate.TransactionAmount == null => false,
                _ => true
            };
        }

        public async Task<Result> CalculateFeesForTransaction(TransactionCommands.CalculateFeesForTransactionCommand command,
                                                              CancellationToken cancellationToken) {
            Result result = await ApplyUpdates(async (TransactionAggregate transactionAggregate) => {

                if (RequireFeeCalculation(transactionAggregate) == false)
                    return Result.Success();

                List<TransactionFeeToCalculate> feesForCalculation = await this.GetTransactionFeesForCalculation(transactionAggregate, cancellationToken);

                if (feesForCalculation == null)
                    return Result.Failure("Error getting transaction fees");

                List<CalculatedFee> resultFees = this.FeeCalculationManager.CalculateFees(feesForCalculation, transactionAggregate.TransactionAmount.Value, command.CompletedDateTime);

                IEnumerable<CalculatedFee> nonMerchantFees = resultFees.Where(f => f.FeeType == TransactionProcessor.Models.Contract.FeeType.ServiceProvider);
                foreach (CalculatedFee calculatedFee in nonMerchantFees) {
                    // Add Fee to the Transaction 
                    transactionAggregate.AddFee(calculatedFee);
                }

                // Now deal with merchant fees 
                List<CalculatedFee> merchantFees = resultFees.Where(f => f.FeeType == TransactionProcessor.Models.Contract.FeeType.Merchant).ToList();

                if (merchantFees.Any()) {
                    Result<Merchant> merchantResult = await this.GetMerchant(command.MerchantId, cancellationToken);
                    if (merchantResult.IsFailed)
                        return ResultHelpers.CreateFailure(merchantResult);
                    if (merchantResult.Data.SettlementSchedule == Models.Merchant.SettlementSchedule.NotSet) {
                        // TODO: Result
                        //throw new NotSupportedException($"Merchant {merchant.MerchantId} does not have a settlement schedule configured");
                    }

                    foreach (CalculatedFee calculatedFee in merchantFees) {
                        // Determine when the fee should be applied
                        DateTime settlementDate = CalculateSettlementDate(merchantResult.Data.SettlementSchedule, command.CompletedDateTime);

                        transactionAggregate.AddFeePendingSettlement(calculatedFee, settlementDate);


                        if (merchantResult.Data.SettlementSchedule == Models.Merchant.SettlementSchedule.Immediate) {
                            Guid settlementId = Helpers.CalculateSettlementAggregateId(settlementDate, command.MerchantId, command.EstateId);

                            // Add fees to transaction now if settlement is immediate
                            transactionAggregate.AddSettledFee(calculatedFee, settlementDate, settlementId);
                        }
                    }
                }

                return Result.Success();
            }, command.TransactionId, cancellationToken);
            return result;
        }

        public async Task<Result> AddSettledMerchantFee(TransactionCommands.AddSettledMerchantFeeCommand command,
                                                        CancellationToken cancellationToken) {
            Result result = await ApplyUpdates(async (TransactionAggregate transactionAggregate) => {
                CalculatedFee calculatedFee = new CalculatedFee {
                    CalculatedValue = command.CalculatedValue,
                    FeeCalculatedDateTime = command.FeeCalculatedDateTime,
                    FeeCalculationType = command.FeeCalculationType,
                    FeeId = command.FeeId,
                    FeeType = TransactionProcessor.Models.Contract.FeeType.Merchant,
                    FeeValue = command.FeeValue,
                    IsSettled = true
                };

                transactionAggregate.AddSettledFee(calculatedFee, command.SettledDateTime, command.SettlementId);

                return Result.Success();
            }, command.TransactionId, cancellationToken);
            return result;
        }

        public async Task<Result> SendCustomerEmailReceipt(TransactionCommands.SendCustomerEmailReceiptCommand command,
                                                           CancellationToken cancellationToken) {
            Result<TokenResponse> getTokenResult = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
            if (getTokenResult.IsFailed)
                return ResultHelpers.CreateFailure(getTokenResult);
            this.TokenResponse = getTokenResult.Data;
            Result<TransactionAggregate> transactionAggregateResult = await this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, cancellationToken);

            if (transactionAggregateResult.IsFailed)
                return ResultHelpers.CreateFailure(transactionAggregateResult);

            Models.Transaction transaction = transactionAggregateResult.Data.GetTransaction();

            Result<Merchant> merchantResult = await this.GetMerchant(transaction.MerchantId, cancellationToken);
            if (merchantResult.IsFailed)
                return ResultHelpers.CreateFailure(merchantResult);
            
            Result<EstateAggregate> getEstateResult = await this.AggregateService.Get<EstateAggregate>(command.EstateId, cancellationToken);
            if (getEstateResult.IsFailed)
            {
                return ResultHelpers.CreateFailure(getEstateResult);
            }
            EstateAggregate estateAggregate = getEstateResult.Data;
            Models.Estate.Estate estate = estateAggregate.GetEstate();
            Models.Estate.Operator @operator = estate.Operators.Single(o => o.OperatorId == transaction.OperatorId);

            // Determine the body of the email
            String receiptMessage = await this.TransactionReceiptBuilder.GetEmailReceiptMessage(transaction, merchantResult.Data, @operator.Name, cancellationToken);

            // Send the message
            return await this.SendEmailMessage(this.TokenResponse.AccessToken, command.EventId, command.EstateId, "Transaction Successful", receiptMessage, command.CustomerEmailAddress, cancellationToken);
        }

        public async Task<Result> ResendCustomerEmailReceipt(TransactionCommands.ResendCustomerEmailReceiptCommand command,
                                                             CancellationToken cancellationToken) {
            Result<TokenResponse> getTokenResult = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
            if (getTokenResult.IsFailed)
                return ResultHelpers.CreateFailure(getTokenResult);
            this.TokenResponse = getTokenResult.Data;

            Result<TransactionAggregate> transactionAggregateResult = await this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, cancellationToken);

            if (transactionAggregateResult.IsFailed)
                return ResultHelpers.CreateFailure(transactionAggregateResult);

            return await this.ResendEmailMessage(this.TokenResponse.AccessToken, transactionAggregateResult.Data.ReceiptMessageId, command.EstateId, cancellationToken);
        }

        internal static DateTime CalculateSettlementDate(Models.Merchant.SettlementSchedule merchantSettlementSchedule,
                                                         DateTime completeDateTime) {
            if (merchantSettlementSchedule == Models.Merchant.SettlementSchedule.Weekly) {
                return completeDateTime.Date.AddDays(7).Date;
            }

            if (merchantSettlementSchedule == Models.Merchant.SettlementSchedule.Monthly) {
                return completeDateTime.Date.AddMonths(1).Date;
            }

            return completeDateTime.Date;
        }

        private async Task<List<TransactionFeeToCalculate>> GetTransactionFeesForCalculation(TransactionAggregate transactionAggregate,
                                                                                             CancellationToken cancellationToken) {
            // TODO: convert to result??
            // Get the fees to be calculated
            Result<List<ContractProductTransactionFee>> feesForProduct = await this.GetTransactionFeesForProduct(transactionAggregate.ContractId, transactionAggregate.ProductId, cancellationToken);
        
            if (feesForProduct.IsFailed) {
                Logger.LogWarning($"Failed to get fees {feesForProduct.Message}");
                return null;
            }

            Logger.LogInformation($"After getting Fees {feesForProduct.Data.Count} returned");

            List<TransactionFeeToCalculate> feesForCalculation = new();

            foreach (Models.Contract.ContractProductTransactionFee contractProductTransactionFee in feesForProduct.Data) {
                TransactionFeeToCalculate transactionFeeToCalculate = new TransactionFeeToCalculate { FeeId = contractProductTransactionFee.TransactionFeeId, Value = contractProductTransactionFee.Value, FeeType = (TransactionProcessor.Models.Contract.FeeType)contractProductTransactionFee.FeeType, CalculationType = (TransactionProcessor.Models.Contract.CalculationType)contractProductTransactionFee.CalculationType };

                feesForCalculation.Add(transactionFeeToCalculate);
            }

            return feesForCalculation;
        }

        /// <summary>
        /// Adds the device to merchant.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task AddDeviceToMerchant(Guid merchantId,
                                               String deviceIdentifier,
                                               CancellationToken cancellationToken) {
            // TODO: Should this be firing a command to add the device??
            // Add the device to the merchant
            Result<MerchantAggregate> merchantAggregate = await this.AggregateService.GetLatest<MerchantAggregate>(merchantId, cancellationToken);
            merchantAggregate.Data.AddDevice(Guid.NewGuid(), deviceIdentifier);
            await this.AggregateService.Save(merchantAggregate.Data, cancellationToken);
        }

        /// <summary>
        /// Generates the transaction reference.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        private String GenerateTransactionReference() {
            Int64 i = 1;
            foreach (Byte b in Guid.NewGuid().ToByteArray()) {
                i *= (b + 1);
            }

            return $"{i - DateTime.Now.Ticks:x}";
        }

        private async Task<Result<Merchant>> GetMerchant(Guid merchantId,
                                                         CancellationToken cancellationToken) {
            Result<MerchantAggregate> getMerchantResult= await this.AggregateService.Get<MerchantAggregate>(merchantId, cancellationToken);

            if (getMerchantResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantResult);
            Models.Merchant.Merchant merchant = getMerchantResult.Data.GetMerchant();

            return merchant;
        }

        private async Task<Result<OperatorResponse>> ProcessMessageWithOperator(Models.Merchant.Merchant merchant,
                                                                                Guid transactionId,
                                                                                DateTime transactionDateTime,
                                                                                Guid operatorId,
                                                                                Dictionary<String, String> additionalTransactionMetadata,
                                                                                String transactionReference,
                                                                                CancellationToken cancellationToken) {
            
            Result<OperatorAggregate> getOperatorResult= await this.AggregateService.Get<OperatorAggregate>(operatorId, cancellationToken);
            if (getOperatorResult.IsFailed)
                return ResultHelpers.CreateFailure(getOperatorResult);
            Models.Operator.Operator operatorResult = getOperatorResult.Data.GetOperator();
            IOperatorProxy operatorProxy = this.OperatorProxyResolver(operatorResult.Name.Replace(" ", ""));
            try {
                Result<OperatorResponse> saleResult = await operatorProxy.ProcessSaleMessage(transactionId, operatorId, merchant, transactionDateTime, transactionReference, additionalTransactionMetadata, cancellationToken);
                if (saleResult.IsFailed) {
                    return CreateFailedResult(new OperatorResponse { IsSuccessful = false, ResponseCode = "9999", ResponseMessage = saleResult.Message });
                }

                return saleResult;
            }
            catch (Exception e) {
                // Log out the error
                Logger.LogError(e);
                return CreateFailedResult(new OperatorResponse { IsSuccessful = false, ResponseCode = "9999", ResponseMessage = e.GetCombinedExceptionMessages() });
            }
        }

        #endregion

        internal static Result<T> CreateFailedResult<T>(T resultData) {
            return new Result<T> { IsSuccess = false, Data = resultData };
        }

        [ExcludeFromCodeCoverage]
        private async Task<Result> SendEmailMessage(String accessToken,
                                                    Guid messageId,
                                                    Guid estateId,
                                                    String subject,
                                                    String body,
                                                    String emailAddress,
                                                    CancellationToken cancellationToken) {
            SendEmailRequest sendEmailRequest = new SendEmailRequest {
                MessageId = messageId,
                Body = body,
                ConnectionIdentifier = estateId,
                FromAddress = "golfhandicapping@btinternet.com", // TODO: lookup from config
                IsHtml = true,
                Subject = subject,
                ToAddresses = new List<String> { emailAddress }
            };

            // TODO: may decide to record the message Id againsts the Transaction Aggregate in future, but for now
            // we wont do this...
            try {
                return await this.MessagingServiceClient.SendEmail(accessToken, sendEmailRequest, cancellationToken);
            }
            catch (Exception ex) when (ex.InnerException != null && ex.InnerException.GetType() == typeof(InvalidOperationException)) {
                if (ex.InnerException.Message.Contains("Cannot send a message to provider that has already been sent", StringComparison.InvariantCultureIgnoreCase) == false) {
                    return Result.Failure(ex.GetExceptionMessages());
                }

                return Result.Success("Duplicate message send");
            }
        }

        [ExcludeFromCodeCoverage]
        private async Task<Result> ResendEmailMessage(String accessToken,
                                                      Guid messageId,
                                                      Guid estateId,
                                                      CancellationToken cancellationToken) {
            ResendEmailRequest resendEmailRequest = new ResendEmailRequest { ConnectionIdentifier = estateId, MessageId = messageId };
            try {
                return await this.MessagingServiceClient.ResendEmail(accessToken, resendEmailRequest, cancellationToken);
            }
            catch (Exception ex) when (ex.InnerException != null && ex.InnerException.GetType() == typeof(InvalidOperationException)) {
                // Only bubble up if not a duplicate message
                if (ex.InnerException.Message.Contains("Cannot send a message to provider that has already been sent", StringComparison.InvariantCultureIgnoreCase) == false) {
                    return Result.Failure(ex.GetExceptionMessages());
                }

                return Result.Success("Duplicate message send");
            }

        }

        private async Task<Result<List<Models.Contract.ContractProductTransactionFee>>> GetTransactionFeesForProduct(Guid contractId,
                                                                                                                     Guid productId,
                                                                                                                     CancellationToken cancellationToken) {
            Result<ContractAggregate> contractAggregateResult = await this.AggregateService.Get<ContractAggregate>(contractId, CancellationToken.None);
            if (contractAggregateResult.IsFailed)
                return ResultHelpers.CreateFailure(contractAggregateResult);

            Models.Contract.Contract contract = contractAggregateResult.Data.GetContract();

            Product product = contract.Products.SingleOrDefault(p => p.ContractProductId == productId);
            if (product == null)
                return Result.Failure("Product not found");

            List<Models.Contract.ContractProductTransactionFee> transactionFees = product.TransactionFees.ToList();
            return Result.Success(transactionFees);
        }
    }
}