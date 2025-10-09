using Newtonsoft.Json;
using Shared.Exceptions;
using Shared.General;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.BusinessLogic.Services{
    using Common;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Models;
    using OperatorInterfaces;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.EventStore.Aggregate;
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

        public TransactionDomainService(Func<IAggregateService> aggregateService,
                                        Func<String, IOperatorProxy> operatorProxyResolver,
                                        ITransactionValidationService transactionValidationService,
                                        ISecurityServiceClient securityServiceClient,
                                        IMemoryCacheWrapper memoryCache,
                                        IFeeCalculationManager feeCalculationManager,
                                        ITransactionReceiptBuilder transactionReceiptBuilder,
                                        IMessagingServiceClient messagingServiceClient) {
            this.AggregateService = aggregateService();
            this.OperatorProxyResolver = operatorProxyResolver;
            this.TransactionValidationService = transactionValidationService;
            this.SecurityServiceClient = securityServiceClient;
            this.MemoryCache = memoryCache;
            this.FeeCalculationManager = feeCalculationManager;
            this.TransactionReceiptBuilder = transactionReceiptBuilder;
            this.MessagingServiceClient = messagingServiceClient;
        }

        #endregion

        public async Task<Result<ProcessLogonTransactionResponse>> ProcessLogonTransaction(TransactionCommands.ProcessLogonTransactionCommand command,
                                                                                           CancellationToken cancellationToken) {
            try {
                Result<TransactionAggregate> transactionResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, ct), command.TransactionId, cancellationToken, false);
                if (transactionResult.IsFailed)
                    return ResultHelpers.CreateFailure(transactionResult);
                TransactionAggregate transactionAggregate = transactionResult.Data;

                TransactionType transactionType = TransactionType.Logon;

                // Generate a transaction reference
                String transactionReference = TransactionHelpers.GenerateTransactionReference();

                Result stateResult = transactionAggregate.StartTransaction(command.TransactionDateTime, command.TransactionNumber, transactionType, transactionReference, command.EstateId, command.MerchantId, command.DeviceIdentifier, null); // Logon transaction has no amount
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result<TransactionValidationResult> validationResult = await this.TransactionValidationService.ValidateLogonTransaction(command.EstateId, command.MerchantId, command.DeviceIdentifier, cancellationToken);

                if (validationResult.IsSuccess && (validationResult.Data.ResponseCode == TransactionResponseCode.Success || validationResult.Data.ResponseCode == TransactionResponseCode.SuccessNeedToAddDevice)) {
                    if (validationResult.Data.ResponseCode == TransactionResponseCode.SuccessNeedToAddDevice) {
                        await this.AddDeviceToMerchant(command.MerchantId, command.DeviceIdentifier, cancellationToken);
                    }

                    // Record the successful validation
                    stateResult = transactionAggregate.AuthoriseTransactionLocally(TransactionHelpers.GenerateAuthCode(), validationResult.Data.ResponseCode, validationResult.Data.ResponseMessage);
                    if (stateResult.IsFailed)
                        return ResultHelpers.CreateFailure(stateResult);
                }
                else {
                    // Record the failure
                    stateResult = transactionAggregate.DeclineTransactionLocally(validationResult.Data.ResponseCode, validationResult.Data.ResponseMessage);
                    if (stateResult.IsFailed)
                        return ResultHelpers.CreateFailure(stateResult);
                }

                stateResult = transactionAggregate.CompleteTransaction();
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                stateResult = transactionAggregate.RecordTransactionTimings(command.TransactionReceivedDateTime, null, null, DateTime.Now);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(transactionAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);
                return Result.Success(new ProcessLogonTransactionResponse {
                    ResponseMessage = transactionAggregate.ResponseMessage,
                    ResponseCode = transactionAggregate.ResponseCode,
                    EstateId = command.EstateId,
                    MerchantId = command.MerchantId,
                    TransactionId = command.TransactionId
                });
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }
        
        public async Task<Result<ProcessReconciliationTransactionResponse>> ProcessReconciliationTransaction(TransactionCommands.ProcessReconciliationCommand command,
                                                                                                             CancellationToken cancellationToken) {
            try{

            Result<ReconciliationAggregate> reconciliationResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<ReconciliationAggregate>(command.TransactionId, ct), command.TransactionId, cancellationToken, false);
            if (reconciliationResult.IsFailed)
                return ResultHelpers.CreateFailure(reconciliationResult);

            ReconciliationAggregate reconciliationAggregate = reconciliationResult.Data;
            Result<TransactionValidationResult> validationResult = await this.TransactionValidationService.ValidateReconciliationTransaction(command.EstateId, command.MerchantId, command.DeviceIdentifier, cancellationToken);

                Result stateResult = reconciliationAggregate.StartReconciliation(command.TransactionDateTime, command.EstateId, command.MerchantId);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                stateResult = reconciliationAggregate.RecordOverallTotals(command.TransactionCount, command.TransactionValue);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                if (validationResult.IsSuccess && validationResult.Data.ResponseCode == TransactionResponseCode.Success) {
                    // Record the successful validation
                    stateResult = reconciliationAggregate.Authorise(validationResult.Data.ResponseCode, validationResult.Data.ResponseMessage);
                    if (stateResult.IsFailed)
                        return ResultHelpers.CreateFailure(stateResult);
                }
                else {
                    // Record the failure
                    stateResult = reconciliationAggregate.Decline(validationResult.Data.ResponseCode, validationResult.Data.ResponseMessage);
                    if (stateResult.IsFailed)
                        return ResultHelpers.CreateFailure(stateResult);
                }
                
                stateResult = reconciliationAggregate.CompleteReconciliation();
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(reconciliationAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success(new ProcessReconciliationTransactionResponse
                {
                    EstateId = reconciliationAggregate.EstateId,
                    MerchantId = reconciliationAggregate.MerchantId,
                    ResponseCode = reconciliationAggregate.ResponseCode,
                    ResponseMessage = reconciliationAggregate.ResponseMessage,
                    TransactionId = command.TransactionId
                });
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result<ProcessSaleTransactionResponse>> ProcessSaleTransactionX(TransactionCommands.ProcessSaleTransactionCommand command,
                                                                                         CancellationToken cancellationToken) {
            try
            {
                Result<TransactionAggregate> transactionResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, ct), command.TransactionId, cancellationToken, false);
                if (transactionResult.IsFailed)
                    return ResultHelpers.CreateFailure(transactionResult);

                TransactionAggregate transactionAggregate = transactionResult.Data;

                TransactionType transactionType = TransactionType.Sale;
                TransactionSource transactionSourceValue = (TransactionSource)command.TransactionSource;

                // Generate a transaction reference
                String transactionReference = TransactionHelpers.GenerateTransactionReference();

                // Extract the transaction amount from the metadata
                Decimal? transactionAmount = command.AdditionalTransactionMetadata.ExtractFieldFromMetadata<Decimal?>("Amount");

                Result<TransactionValidationResult> validationResult = await this.TransactionValidationService.ValidateSaleTransaction(command.EstateId, command.MerchantId, command.ContractId, command.ProductId, command.DeviceIdentifier, command.OperatorId, transactionAmount, cancellationToken);

                Logger.LogInformation($"Validation response is [{JsonConvert.SerializeObject(validationResult)}]");

                Guid floatAggregateId = IdGenerationService.GenerateFloatAggregateId(command.EstateId, command.ContractId, command.ProductId);
                Result<FloatAggregate> floatAggregateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatAggregate>(floatAggregateId, ct), floatAggregateId, cancellationToken, false);
                Decimal unitCost = 0;
                Decimal totalCost = 0;
                if (floatAggregateResult.IsSuccess) {
                    FloatAggregate floatAggregate = floatAggregateResult.Data;
                    unitCost = floatAggregate.GetUnitCostPrice();
                    totalCost = floatAggregate.GetTotalCostPrice(transactionAmount.GetValueOrDefault());
                }

                Result stateResult = transactionAggregate.StartTransaction(command.TransactionDateTime, command.TransactionNumber, transactionType, transactionReference, command.EstateId, command.MerchantId, command.DeviceIdentifier, transactionAmount);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                // Add the product details (unless invalid estate)
                if (validationResult.Data.ResponseCode != TransactionResponseCode.InvalidEstateId && validationResult.Data.ResponseCode != TransactionResponseCode.InvalidContractIdValue && validationResult.Data.ResponseCode != TransactionResponseCode.InvalidProductIdValue && validationResult.Data.ResponseCode != TransactionResponseCode.ContractNotValidForMerchant && validationResult.Data.ResponseCode != TransactionResponseCode.ProductNotValidForMerchant) {
                    stateResult = transactionAggregate.AddProductDetails(command.ContractId, command.ProductId);
                    if (stateResult.IsFailed)
                        return ResultHelpers.CreateFailure(stateResult);
                }

                stateResult = transactionAggregate.RecordCostPrice(unitCost, totalCost);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                // Add the transaction source
                stateResult = transactionAggregate.AddTransactionSource(transactionSourceValue);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);
                DateTime? operatorStartDateTime = null;
                DateTime? operatorEndDateTime = null;
                if (validationResult.Data.ResponseCode == TransactionResponseCode.Success) {
                    // Record any additional request metadata
                    stateResult = transactionAggregate.RecordAdditionalRequestData(command.OperatorId, command.AdditionalTransactionMetadata);
                    if (stateResult.IsFailed)
                        return ResultHelpers.CreateFailure(stateResult);

                    // Do the online processing with the operator here
                    Result<Merchant> merchantResult = await this.GetMerchant(command.MerchantId, cancellationToken);
                    if (merchantResult.IsFailed)
                        return ResultHelpers.CreateFailure(merchantResult);
                    operatorStartDateTime =DateTime.Now;
                    Result<OperatorResponse> operatorResult = await this.ProcessMessageWithOperator(merchantResult.Data, command.TransactionId, command.TransactionDateTime, command.OperatorId, command.AdditionalTransactionMetadata, transactionReference, cancellationToken);
                    operatorEndDateTime = DateTime.Now;
                    
                    if (operatorResult.IsSuccess) {
                        TransactionResponseCode transactionResponseCode = TransactionResponseCode.Success;
                        String responseMessage = "SUCCESS";

                        stateResult= transactionAggregate.AuthoriseTransaction(command.OperatorId, operatorResult.Data.AuthorisationCode, operatorResult.Data.ResponseCode, operatorResult.Data.ResponseMessage, operatorResult.Data.TransactionId, transactionResponseCode, responseMessage);
                        if (stateResult.IsFailed)
                            return ResultHelpers.CreateFailure(stateResult);
                    }
                    else {
                        TransactionResponseCode transactionResponseCode = TransactionResponseCode.TransactionDeclinedByOperator;
                        String responseMessage = "DECLINED BY OPERATOR";

                        stateResult = transactionAggregate.DeclineTransaction(command.OperatorId, operatorResult.Data.ResponseCode, operatorResult.Data.ResponseMessage, transactionResponseCode, responseMessage);
                        if (stateResult.IsFailed)
                            return ResultHelpers.CreateFailure(stateResult);
                    }

                    // Record any additional operator response metadata
                    stateResult = transactionAggregate.RecordAdditionalResponseData(command.OperatorId, operatorResult.Data.AdditionalTransactionResponseMetadata);
                    if (stateResult.IsFailed)
                        return ResultHelpers.CreateFailure(stateResult);
                }
                else {
                    // Record the failure
                    stateResult = transactionAggregate.DeclineTransactionLocally(validationResult.Data.ResponseCode, validationResult.Data.ResponseMessage);
                    if (stateResult.IsFailed)
                        return ResultHelpers.CreateFailure(stateResult);
                }

                stateResult = transactionAggregate.RecordTransactionTimings(command.TransactionReceivedDateTime, operatorStartDateTime, operatorEndDateTime, DateTime.Now);;
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                stateResult= transactionAggregate.CompleteTransaction();
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                // Determine if the email receipt is required
                if (String.IsNullOrEmpty(command.CustomerEmailAddress) == false) {
                    stateResult = transactionAggregate.RequestEmailReceipt(command.CustomerEmailAddress);
                    if (stateResult.IsFailed)
                        return ResultHelpers.CreateFailure(stateResult);
                }

                // Get the model from the aggregate
                Models.Transaction transaction = transactionAggregate.GetTransaction();
                
                Result saveResult = await this.AggregateService.Save(transactionAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success(new ProcessSaleTransactionResponse
                {
                    ResponseMessage = transaction.ResponseMessage,
                    ResponseCode = transaction.ResponseCode,
                    EstateId = command.EstateId,
                    MerchantId = command.MerchantId,
                    AdditionalTransactionMetadata = transaction.AdditionalResponseMetadata,
                    TransactionId = command.TransactionId
                });
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> ResendTransactionReceipt(TransactionCommands.ResendTransactionReceiptCommand command,
                                                           CancellationToken cancellationToken) {

            try
            {
                Result<TransactionAggregate> transactionResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, ct), command.TransactionId, cancellationToken, false);
                if (transactionResult.IsFailed)
                    return ResultHelpers.CreateFailure(transactionResult);

                TransactionAggregate transactionAggregate = transactionResult.Data;

                Result stateResult = transactionAggregate.RequestEmailReceiptResend();
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(transactionAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        

        public async Task<Result> CalculateFeesForTransaction(TransactionCommands.CalculateFeesForTransactionCommand command,
                                                              CancellationToken cancellationToken) {
            try
            {
                Result<TransactionAggregate> transactionResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, ct), command.TransactionId, cancellationToken, false);
                if (transactionResult.IsFailed)
                    return ResultHelpers.CreateFailure(transactionResult);

                TransactionAggregate transactionAggregate = transactionResult.Data;

                if (TransactionHelpers.RequireFeeCalculation(transactionAggregate) == false)
                    return Result.Success();

                Result<List<TransactionFeeToCalculate>> feesForCalculationResult = await this.GetTransactionFeesForCalculation(transactionAggregate, cancellationToken);

                if (feesForCalculationResult.IsFailed)
                    return ResultHelpers.CreateFailure(feesForCalculationResult);

                List<CalculatedFee> resultFees = this.FeeCalculationManager.CalculateFees(feesForCalculationResult.Data, transactionAggregate.TransactionAmount.Value, command.CompletedDateTime);

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
                        return Result.Failure($"Merchant {merchantResult.Data.MerchantId} does not have a settlement schedule configured");
                    }

                    foreach (CalculatedFee calculatedFee in merchantFees) {
                        // Determine when the fee should be applied
                        DateTime settlementDate = TransactionHelpers.CalculateSettlementDate(merchantResult.Data.SettlementSchedule, command.CompletedDateTime);

                        Result stateResult = transactionAggregate.AddFeePendingSettlement(calculatedFee, settlementDate);
                        if (stateResult.IsFailed)
                            return ResultHelpers.CreateFailure(stateResult);
                        
                        if (merchantResult.Data.SettlementSchedule == Models.Merchant.SettlementSchedule.Immediate) {
                            Guid settlementId = Helpers.CalculateSettlementAggregateId(settlementDate, command.MerchantId, command.EstateId);

                            // Add fees to transaction now if settlement is immediate
                            stateResult =transactionAggregate.AddSettledFee(calculatedFee, settlementDate, settlementId);
                            if (stateResult.IsFailed)
                                return ResultHelpers.CreateFailure(stateResult);
                        }
                    }
                }

                Result saveResult = await this.AggregateService.Save(transactionAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddSettledMerchantFee(TransactionCommands.AddSettledMerchantFeeCommand command,
                                                        CancellationToken cancellationToken) {
            try {
                Result<TransactionAggregate> transactionResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, ct), command.TransactionId, cancellationToken, false);
                if (transactionResult.IsFailed)
                    return ResultHelpers.CreateFailure(transactionResult);

                TransactionAggregate transactionAggregate = transactionResult.Data;

                CalculatedFee calculatedFee = new CalculatedFee {
                    CalculatedValue = command.CalculatedValue,
                    FeeCalculatedDateTime = command.FeeCalculatedDateTime,
                    FeeCalculationType = command.FeeCalculationType,
                    FeeId = command.FeeId,
                    FeeType = TransactionProcessor.Models.Contract.FeeType.Merchant,
                    FeeValue = command.FeeValue,
                    IsSettled = true
                };

                Result stateResult = transactionAggregate.AddSettledFee(calculatedFee, command.SettledDateTime, command.SettlementId);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(transactionAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> SendCustomerEmailReceipt(TransactionCommands.SendCustomerEmailReceiptCommand command,
                                                           CancellationToken cancellationToken) {
            Result<TokenResponse> getTokenResult = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
            if (getTokenResult.IsFailed)
                return ResultHelpers.CreateFailure(getTokenResult);
            this.TokenResponse = getTokenResult.Data;
            Result<TransactionAggregate> transactionResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, ct), command.TransactionId, cancellationToken, false);
            if (transactionResult.IsFailed)
                return ResultHelpers.CreateFailure(transactionResult);

            TransactionAggregate transactionAggregate = transactionResult.Data;

            Models.Transaction transaction = transactionAggregate.GetTransaction();

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

            Result<TransactionAggregate> transactionResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, ct), command.TransactionId, cancellationToken, false);
            if (transactionResult.IsFailed)
                return ResultHelpers.CreateFailure(transactionResult);

            TransactionAggregate transactionAggregate = transactionResult.Data;

            return await this.ResendEmailMessage(this.TokenResponse.AccessToken, transactionAggregate.ReceiptMessageId, command.EstateId, cancellationToken);
        }
        
        private async Task<Result<List<TransactionFeeToCalculate>>> GetTransactionFeesForCalculation(TransactionAggregate transactionAggregate,
                                                                                             CancellationToken cancellationToken) {
            // Get the fees to be calculated
            Result<List<ContractProductTransactionFee>> feesForProduct = await this.GetTransactionFeesForProduct(transactionAggregate.ContractId, transactionAggregate.ProductId, cancellationToken);
        
            if (feesForProduct.IsFailed) {
                Logger.LogWarning($"Failed to get fees {feesForProduct.Message}");
                return ResultHelpers.CreateFailure(feesForProduct);
            }

            Logger.LogInformation($"After getting Fees {feesForProduct.Data.Count} returned");

            List<TransactionFeeToCalculate> feesForCalculation = new();

            foreach (Models.Contract.ContractProductTransactionFee contractProductTransactionFee in feesForProduct.Data) {
                TransactionFeeToCalculate transactionFeeToCalculate = new TransactionFeeToCalculate { FeeId = contractProductTransactionFee.TransactionFeeId, Value = contractProductTransactionFee.Value, FeeType = (TransactionProcessor.Models.Contract.FeeType)contractProductTransactionFee.FeeType, CalculationType = (TransactionProcessor.Models.Contract.CalculationType)contractProductTransactionFee.CalculationType };

                feesForCalculation.Add(transactionFeeToCalculate);
            }

            return Result.Success(feesForCalculation);
        }

        private async Task<Result> AddDeviceToMerchant(Guid merchantId,
                                               String deviceIdentifier,
                                               CancellationToken cancellationToken) {
            // Add the device to the merchant
            Result<MerchantAggregate> merchantAggregate = await this.AggregateService.GetLatest<MerchantAggregate>(merchantId, cancellationToken);
            if (merchantAggregate.IsFailed)
                return ResultHelpers.CreateFailure(merchantAggregate);
            Result stateResult = merchantAggregate.Data.AddDevice(Guid.NewGuid(), deviceIdentifier);
            if (stateResult.IsFailed)
                return ResultHelpers.CreateFailure(stateResult);
            return await this.AggregateService.Save(merchantAggregate.Data, cancellationToken);
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

            Result<OperatorAggregate> getOperatorResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<OperatorAggregate>(operatorId, ct), operatorId, cancellationToken, false);
            if (getOperatorResult.IsFailed)
                return ResultHelpers.CreateFailure(getOperatorResult);
            OperatorAggregate operatorAggregate = getOperatorResult.Data;
            
            Models.Operator.Operator operatorResult = operatorAggregate.GetOperator();
            IOperatorProxy operatorProxy = this.OperatorProxyResolver(operatorResult.Name.Replace(" ", ""));
            try {
                Result<OperatorResponse> saleResult = await operatorProxy.ProcessSaleMessage(transactionId, operatorId, merchant, transactionDateTime, transactionReference, additionalTransactionMetadata, cancellationToken);
                if (saleResult.IsFailed) {
                    return ResultHelpers.CreateFailedResult(new OperatorResponse { IsSuccessful = false, ResponseCode = "9999", ResponseMessage = saleResult.Message });
                }

                return saleResult;
            }
            catch (Exception e) {
                // Log out the error
                Logger.LogError(e);
                return ResultHelpers.CreateFailedResult(new OperatorResponse { IsSuccessful = false, ResponseCode = "9999", ResponseMessage = e.GetCombinedExceptionMessages() });
            }
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
                FromAddress = ConfigurationReader.GetValueOrDefault("AppSettings", "FromEmailAddress", "golfhandicapping@btinternet.com"),
                IsHtml = true,
                Subject = subject,
                ToAddresses = new List<String> { emailAddress }
            };

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
            Result<ContractAggregate> contractAggregateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<ContractAggregate>(contractId, ct), contractId, cancellationToken, false);
            if (contractAggregateResult.IsFailed)
                return ResultHelpers.CreateFailure(contractAggregateResult);
            ContractAggregate contractAggregate = contractAggregateResult.Data;


            Models.Contract.Contract contract = contractAggregate.GetContract();

            Product product = contract.Products.SingleOrDefault(p => p.ContractProductId == productId);
            if (product == null)
                return Result.Failure("Product not found");

            List<Models.Contract.ContractProductTransactionFee> transactionFees = product.TransactionFees.ToList();
            return Result.Success(transactionFees);
        }

        public async Task<Result<ProcessSaleTransactionResponse>> ProcessSaleTransaction(TransactionCommands.ProcessSaleTransactionCommand command,
                                                                                         CancellationToken cancellationToken) {
            Logger.LogInformation($"Starting ProcessSaleTransaction for TransactionId {command.TransactionId}");

            try {
                Result<TransactionAggregate> transactionResult = await GetTransactionAggregate(command, cancellationToken);
                if (transactionResult.IsFailed)
                    return ResultHelpers.CreateFailure(transactionResult);

                TransactionAggregate transactionAggregate = transactionResult.Data;

                Result<TransactionValidationResult> validationResult = await ValidateTransaction(command, cancellationToken);
                (Decimal unitCost, Decimal totalCost) = await GetFloatCost(command, cancellationToken);

                Result startResult = StartTransaction(transactionAggregate, command, unitCost, totalCost, validationResult);
                if (startResult.IsFailed)
                    return ResultHelpers.CreateFailure(startResult);

                (DateTime? Start, DateTime? End) operatorTiming = await HandleOperatorProcessing(transactionAggregate, command, validationResult, cancellationToken);

                transactionAggregate.RecordTransactionTimings(command.TransactionReceivedDateTime, operatorTiming.Start, operatorTiming.End, DateTime.Now);

                transactionAggregate.CompleteTransaction();

                if (!string.IsNullOrEmpty(command.CustomerEmailAddress))
                    transactionAggregate.RequestEmailReceipt(command.CustomerEmailAddress);

                Result saveResult = await AggregateService.Save(transactionAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                Transaction transaction = transactionAggregate.GetTransaction();
                Logger.LogInformation($"Transaction {command.TransactionId} completed successfully");

                return Result.Success(new ProcessSaleTransactionResponse {
                    ResponseMessage = transaction.ResponseMessage,
                    ResponseCode = transaction.ResponseCode,
                    EstateId = command.EstateId,
                    MerchantId = command.MerchantId,
                    AdditionalTransactionMetadata = transaction.AdditionalResponseMetadata,
                    TransactionId = command.TransactionId
                });
            }
            catch (Exception ex) {
                Logger.LogError($"Unhandled exception during ProcessSaleTransaction for {command.TransactionId}", ex);
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        private async Task<Result<TransactionAggregate>> GetTransactionAggregate(TransactionCommands.ProcessSaleTransactionCommand command,
                                                                                      CancellationToken cancellationToken) {
            Logger.LogDebug($"Fetching TransactionAggregate for TransactionId {command.TransactionId}");

            Result<TransactionAggregate> result = await DomainServiceHelper.GetAggregateOrFailure(ct => AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, ct), command.TransactionId, cancellationToken, false);

            if (result.IsFailed)
                Logger.LogWarning($"TransactionAggregate not found for TransactionId {command.TransactionId}");

            return result;
        }


        private async Task<Result<TransactionValidationResult>> ValidateTransaction(TransactionCommands.ProcessSaleTransactionCommand command,
                                                                                         CancellationToken cancellationToken) {
            Decimal? amount = command.AdditionalTransactionMetadata.ExtractFieldFromMetadata<decimal?>("Amount");
            Logger.LogInformation($"Validating Sale Transaction for Merchant {command.MerchantId}, Product {command.ProductId}");

            Result<TransactionValidationResult> result = await TransactionValidationService.ValidateSaleTransaction(command.EstateId, command.MerchantId, command.ContractId, command.ProductId, command.DeviceIdentifier, command.OperatorId, amount, cancellationToken);

            Logger.LogInformation($"Validation completed with ResponseCode {result.Data?.ResponseCode}, Message {result.Data?.ResponseMessage}");

            return result;
        }


        private async Task<(decimal UnitCost, decimal TotalCost)> GetFloatCost(TransactionCommands.ProcessSaleTransactionCommand command,
                                                                                    CancellationToken cancellationToken) {
            Guid floatAggregateId = IdGenerationService.GenerateFloatAggregateId(command.EstateId, command.ContractId, command.ProductId);

            Logger.LogDebug($"Fetching FloatAggregate {floatAggregateId}");

            Result<FloatAggregate> floatAggregateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => AggregateService.GetLatest<FloatAggregate>(floatAggregateId, ct), floatAggregateId, cancellationToken, false);

            if (floatAggregateResult.IsFailed)
                return (0, 0);

            FloatAggregate floatAggregate = floatAggregateResult.Data;
            Decimal amount = command.AdditionalTransactionMetadata.ExtractFieldFromMetadata<decimal?>("Amount") ?? 0;
            Decimal unitCost = floatAggregate.GetUnitCostPrice();
            Decimal totalCost = floatAggregate.GetTotalCostPrice(amount);

            Logger.LogInformation($"Float cost calculated: UnitCost={unitCost}, TotalCost={totalCost}");
            return (unitCost, totalCost);
        }


        private Result StartTransaction(TransactionAggregate transactionAggregate,
                                        TransactionCommands.ProcessSaleTransactionCommand command,
                                        decimal unitCost,
                                        decimal totalCost,
                                        Result<TransactionValidationResult> validationResult) {
            TransactionType transactionType = TransactionType.Sale;
            TransactionSource transactionSourceValue = (TransactionSource)command.TransactionSource;
            String transactionReference = TransactionHelpers.GenerateTransactionReference();
            Decimal? amount = command.AdditionalTransactionMetadata.ExtractFieldFromMetadata<decimal?>("Amount");

            Result result = transactionAggregate.StartTransaction(command.TransactionDateTime, command.TransactionNumber, transactionType, transactionReference, command.EstateId, command.MerchantId, command.DeviceIdentifier, amount);

            if (result.IsFailed)
                return result;

            if (validationResult.Data.ResponseCode is not (TransactionResponseCode.InvalidEstateId or TransactionResponseCode.InvalidContractIdValue or TransactionResponseCode.InvalidProductIdValue or TransactionResponseCode.ContractNotValidForMerchant or TransactionResponseCode.ProductNotValidForMerchant)) {
                transactionAggregate.AddProductDetails(command.ContractId, command.ProductId);
            }

            transactionAggregate.RecordCostPrice(unitCost, totalCost);
            transactionAggregate.AddTransactionSource(transactionSourceValue);
            return Result.Success();
        }

        private async Task<(DateTime? Start, DateTime? End)> HandleOperatorProcessing(TransactionAggregate transactionAggregate,
                                                                                           TransactionCommands.ProcessSaleTransactionCommand command,
                                                                                           Result<TransactionValidationResult> validationResult,
                                                                                           CancellationToken cancellationToken) {
            DateTime? start = null, end = null;

            if (validationResult.Data.ResponseCode != TransactionResponseCode.Success) {
                Logger.LogWarning($"Validation failed with ResponseCode {validationResult.Data.ResponseCode}: {validationResult.Data.ResponseMessage}");
                transactionAggregate.DeclineTransactionLocally(validationResult.Data.ResponseCode, validationResult.Data.ResponseMessage);
                return (start, end);
            }

            Logger.LogInformation($"Validation success, processing with Operator {command.OperatorId}");

            transactionAggregate.RecordAdditionalRequestData(command.OperatorId, command.AdditionalTransactionMetadata);
            Result<Merchant> merchantResult = await GetMerchant(command.MerchantId, cancellationToken);
            if (merchantResult.IsFailed)
                return (start, end);

            start = DateTime.Now;
            Result<OperatorResponse> operatorResult = await ProcessMessageWithOperator(merchantResult.Data, command.TransactionId, command.TransactionDateTime, command.OperatorId, command.AdditionalTransactionMetadata, TransactionHelpers.GenerateTransactionReference(), cancellationToken);
            end = DateTime.Now;

            if (operatorResult.IsSuccess) {
                Logger.LogInformation("Operator authorised transaction successfully");
                transactionAggregate.AuthoriseTransaction(command.OperatorId, operatorResult.Data.AuthorisationCode, operatorResult.Data.ResponseCode, operatorResult.Data.ResponseMessage, operatorResult.Data.TransactionId, TransactionResponseCode.Success, "SUCCESS");
            }
            else {
                Logger.LogWarning($"Operator declined transaction: {operatorResult.Data.ResponseMessage}");
                transactionAggregate.DeclineTransaction(command.OperatorId, operatorResult.Data.ResponseCode, operatorResult.Data.ResponseMessage, TransactionResponseCode.TransactionDeclinedByOperator, "DECLINED BY OPERATOR");
            }

            transactionAggregate.RecordAdditionalResponseData(command.OperatorId, operatorResult.Data.AdditionalTransactionResponseMetadata);
            return (start, end);
        }
    }
}