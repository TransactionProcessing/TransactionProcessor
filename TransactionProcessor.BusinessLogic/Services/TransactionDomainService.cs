﻿using EstateManagement.DataTransferObjects.Responses.Contract;
using Newtonsoft.Json;
using Shared.Exceptions;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Services{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Requests;
    using EstateManagement.DataTransferObjects.Requests.Merchant;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.DataTransferObjects.Responses.Estate;
    using FloatAggregate;
    using Microsoft.Extensions.Caching.Memory;
    using Models;
    using OperatorInterfaces;
    using ReconciliationAggregate;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using TransactionAggregate;
    using TransactionProcessor.BusinessLogic.Manager;
    using TransactionProcessor.BusinessLogic.Requests;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.BusinessLogic.Services.ITransactionDomainService" />
    public class TransactionDomainService : ITransactionDomainService{
        #region Fields

        /// <summary>
        /// The estate client
        /// </summary>
        private readonly IEstateClient EstateClient;

        /// <summary>
        /// The operator proxy resolver
        /// </summary>
        private readonly Func<String, IOperatorProxy> OperatorProxyResolver;

        /// <summary>
        /// The reconciliation aggregate repository
        /// </summary>
        private readonly IAggregateRepository<ReconciliationAggregate, DomainEvent> ReconciliationAggregateRepository;

        private readonly ISecurityServiceClient SecurityServiceClient;

        private readonly IAggregateRepository<FloatAggregate, DomainEvent> FloatAggregateRepository;
        private readonly IMemoryCacheWrapper MemoryCache;
        private readonly IFeeCalculationManager FeeCalculationManager;

        private TokenResponse TokenResponse;

        private readonly IAggregateRepository<TransactionAggregate, DomainEvent> TransactionAggregateRepository;

        private readonly ITransactionValidationService TransactionValidationService;

        #endregion

        #region Constructors

        public TransactionDomainService(IAggregateRepository<TransactionAggregate, DomainEvent> transactionAggregateRepository,
                                        IEstateClient estateClient,
                                        Func<String, IOperatorProxy> operatorProxyResolver,
                                        IAggregateRepository<ReconciliationAggregate, DomainEvent> reconciliationAggregateRepository,
                                        ITransactionValidationService transactionValidationService,
                                        ISecurityServiceClient securityServiceClient,
                                        IAggregateRepository<FloatAggregate, DomainEvent> floatAggregateRepository,
                                        IMemoryCacheWrapper memoryCache,
                                        IFeeCalculationManager feeCalculationManager)
        {
            this.TransactionAggregateRepository = transactionAggregateRepository;
            this.EstateClient = estateClient;
            this.OperatorProxyResolver = operatorProxyResolver;
            this.ReconciliationAggregateRepository = reconciliationAggregateRepository;
            this.TransactionValidationService = transactionValidationService;
            this.SecurityServiceClient = securityServiceClient;
            this.FloatAggregateRepository = floatAggregateRepository;
            this.MemoryCache = memoryCache;
            this.FeeCalculationManager = feeCalculationManager;
        }

        #endregion

        private async Task<Result<T>> ApplyUpdates<T>(Func<TransactionAggregate, Task<Result<T>>> action,
                                                      Guid transactionId,
                                                      CancellationToken cancellationToken,
                                                      Boolean isNotFoundError = true)
        {
            try
            {

                Result<TransactionAggregate> getTransactionResult = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
                Result<TransactionAggregate> transactionAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getTransactionResult, transactionId, isNotFoundError);

                TransactionAggregate transactionAggregate = transactionAggregateResult.Data;
                Result<T> result = await action(transactionAggregate);
                if (result.IsFailed)
                    return Shared.EventStore.Aggregate.ResultHelpers.CreateFailure(result);

                Result saveResult = await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return Shared.EventStore.Aggregate.ResultHelpers.CreateFailure(saveResult);
                return Result.Success(result.Data);
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        private async Task<Result> ApplyUpdates(Func<TransactionAggregate, Task<Result>> action,
                                                      Guid transactionId,
                                                      CancellationToken cancellationToken,
                                                      Boolean isNotFoundError = true)
        {
            try
            {

                Result<TransactionAggregate> getTransactionResult = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
                Result<TransactionAggregate> transactionAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getTransactionResult, transactionId, isNotFoundError);

                TransactionAggregate transactionAggregate = transactionAggregateResult.Data;
                Result result = await action(transactionAggregate);
                if (result.IsFailed)
                    return Shared.EventStore.Aggregate.ResultHelpers.CreateFailure(result);

                Result saveResult = await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return Shared.EventStore.Aggregate.ResultHelpers.CreateFailure(saveResult);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        private async Task<Result<T>> ApplyUpdates<T>(Func<ReconciliationAggregate, Task<Result<T>>> action,
                                                      Guid transactionId,
                                                      CancellationToken cancellationToken,
                                                      Boolean isNotFoundError = true)
        {
            try
            {

                Result<ReconciliationAggregate> getTransactionResult = await this.ReconciliationAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
                Result<ReconciliationAggregate> reconciliationAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getTransactionResult, transactionId, isNotFoundError);

                ReconciliationAggregate reconciliationAggregate = reconciliationAggregateResult.Data;
                Result<T> result = await action(reconciliationAggregate);
                if (result.IsFailed)
                    return Shared.EventStore.Aggregate.ResultHelpers.CreateFailure(result);

                Result saveResult = await this.ReconciliationAggregateRepository.SaveChanges(reconciliationAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return Shared.EventStore.Aggregate.ResultHelpers.CreateFailure(saveResult);
                return Result.Success(result.Data);
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        #region Methods

        public async Task<Result<ProcessLogonTransactionResponse>> ProcessLogonTransaction(TransactionCommands.ProcessLogonTransactionCommand command,
                                                                                   CancellationToken cancellationToken) {
            Result<ProcessLogonTransactionResponse> result = await ApplyUpdates<ProcessLogonTransactionResponse>(
                async (TransactionAggregate transactionAggregate) => {
                    TransactionType transactionType = TransactionType.Logon;

                    // Generate a transaction reference
                    String transactionReference = this.GenerateTransactionReference();

                    transactionAggregate.StartTransaction(command.TransactionDateTime, command.TransactionNumber, transactionType,
                        transactionReference, command.EstateId, command.MerchantId, command.DeviceIdentifier,
                        null); // Logon transaction has no amount

                    Result<TransactionValidationResult> validationResult =
                        await this.TransactionValidationService.ValidateLogonTransactionX(command.EstateId, command.MerchantId, command.DeviceIdentifier, cancellationToken);

                    if (validationResult.IsSuccess) {
                        if (validationResult.Data.ResponseCode == TransactionResponseCode.SuccessNeedToAddDevice) {
                            await this.AddDeviceToMerchant(command.EstateId, command.MerchantId, command.DeviceIdentifier, cancellationToken);
                        }

                        // Record the successful validation
                        // TODO: Generate local authcode
                        transactionAggregate.AuthoriseTransactionLocally("ABCD1234",
                            ((Int32)validationResult.Data.ResponseCode).ToString().PadLeft(4, '0'),
                            validationResult.Data.ResponseMessage);
                    }
                    else {
                        // Record the failure
                        transactionAggregate.DeclineTransactionLocally(
                            ((Int32)validationResult.Data.ResponseCode).ToString().PadLeft(4, '0'),
                            validationResult.Data.ResponseMessage);
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
                                                                                                     CancellationToken cancellationToken){

            Result<ProcessReconciliationTransactionResponse> result = await ApplyUpdates<ProcessReconciliationTransactionResponse>(
                async (ReconciliationAggregate reconciliationAggregate) => {
                    Result<TransactionValidationResult> validationResult =
                        await this.TransactionValidationService.ValidateReconciliationTransactionX(command.EstateId, command.MerchantId, command.DeviceIdentifier, cancellationToken);

                    reconciliationAggregate.StartReconciliation(command.TransactionDateTime, command.EstateId, command.MerchantId);

                    reconciliationAggregate.RecordOverallTotals(command.TransactionCount, command.TransactionValue);

                    if (validationResult.IsSuccess)
                    {
                        // Record the successful validation
                        reconciliationAggregate.Authorise(((Int32)validationResult.Data.ResponseCode).ToString().PadLeft(4, '0'), validationResult.Data.ResponseMessage);
                    }
                    else
                    {
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

            Result<ProcessSaleTransactionResponse> result = await ApplyUpdates<ProcessSaleTransactionResponse>(
                async (TransactionAggregate transactionAggregate) => {
                    TransactionType transactionType = TransactionType.Sale;
                    TransactionSource transactionSourceValue = (TransactionSource)command.TransactionSource;

                    // Generate a transaction reference
                    String transactionReference = this.GenerateTransactionReference();

                    // Extract the transaction amount from the metadata
                    Decimal? transactionAmount =
                        command.AdditionalTransactionMetadata.ExtractFieldFromMetadata<Decimal?>("Amount");

                    Result<TransactionValidationResult> validationResult =
                        await this.TransactionValidationService.ValidateSaleTransactionX(command.EstateId, command.MerchantId,
                            command.ContractId, command.ProductId, command.DeviceIdentifier, command.OperatorId, transactionAmount, cancellationToken);

                    Logger.LogInformation($"Validation response is [{JsonConvert.SerializeObject(validationResult)}]");

                    Guid floatAggregateId =
                        IdGenerationService.GenerateFloatAggregateId(command.EstateId, command.ContractId, command.ProductId);
                    var floatAggregateResult =
                        await this.FloatAggregateRepository.GetLatestVersion(floatAggregateId, cancellationToken);
                    Decimal unitCost = 0;
                    Decimal totalCost = 0;
                    if (floatAggregateResult.IsSuccess) {
                        // TODO: Move calculation to float
                        var floatAggregate = floatAggregateResult.Data;
                        unitCost = floatAggregate.GetUnitCostPrice();
                        totalCost = transactionAmount.GetValueOrDefault() * unitCost;
                    }

                    transactionAggregate.StartTransaction(command.TransactionDateTime, command.TransactionNumber, transactionType,
                        transactionReference, command.EstateId, command.MerchantId, command.DeviceIdentifier, transactionAmount);

                    // Add the product details (unless invalid estate)
                    if (validationResult.Data.ResponseCode != TransactionResponseCode.InvalidEstateId &&
                        validationResult.Data.ResponseCode != TransactionResponseCode.InvalidContractIdValue &&
                        validationResult.Data.ResponseCode != TransactionResponseCode.InvalidProductIdValue &&
                        validationResult.Data.ResponseCode != TransactionResponseCode.ContractNotValidForMerchant &&
                        validationResult.Data.ResponseCode != TransactionResponseCode.ProductNotValidForMerchant) {
                        transactionAggregate.AddProductDetails(command.ContractId, command.ProductId);
                    }

                    transactionAggregate.RecordCostPrice(unitCost, totalCost);

                    // Add the transaction source
                    transactionAggregate.AddTransactionSource(transactionSourceValue);

                    if (validationResult.Data.ResponseCode == TransactionResponseCode.Success) {
                        // Record any additional request metadata
                        transactionAggregate.RecordAdditionalRequestData(command.OperatorId, command.AdditionalTransactionMetadata);

                        // Do the online processing with the operator here
                        EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant =
                            await this.GetMerchant(command.EstateId, command.MerchantId, cancellationToken);
                        Result<OperatorResponse> operatorResult = await this.ProcessMessageWithOperator(merchant,
                            command.TransactionId, command.TransactionDateTime, command.OperatorId, command.AdditionalTransactionMetadata,
                            transactionReference, cancellationToken);

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

                                transactionAggregate.AuthoriseTransaction(command.OperatorId,
                                    operatorResult.Data.AuthorisationCode, operatorResult.Data.ResponseCode,
                                    operatorResult.Data.ResponseMessage, operatorResult.Data.TransactionId,
                                    ((Int32)transactionResponseCode).ToString().PadLeft(4, '0'), responseMessage);
                            }
                            else {
                                TransactionResponseCode transactionResponseCode =
                                    TransactionResponseCode.TransactionDeclinedByOperator;
                                String responseMessage = "DECLINED BY OPERATOR";

                                transactionAggregate.DeclineTransaction(command.OperatorId, operatorResult.Data.ResponseCode,
                                    operatorResult.Data.ResponseMessage,
                                    ((Int32)transactionResponseCode).ToString().PadLeft(4, '0'), responseMessage);
                            }

                            // Record any additional operator response metadata
                            transactionAggregate.RecordAdditionalResponseData(command.OperatorId,
                                operatorResult.Data.AdditionalTransactionResponseMetadata);
                        //}
                    }
                    else {
                        // Record the failure
                        transactionAggregate.DeclineTransactionLocally(
                            ((Int32)validationResult.Data.ResponseCode).ToString().PadLeft(4, '0'),
                            validationResult.Data.ResponseMessage);
                    }

                    transactionAggregate.CompleteTransaction();

                    // Determine if the email receipt is required
                    if (String.IsNullOrEmpty(command.CustomerEmailAddress) == false) {
                        transactionAggregate.RequestEmailReceipt(command.CustomerEmailAddress);
                    }

                    // Get the model from the aggregate
                    Transaction transaction = transactionAggregate.GetTransaction();

                    return Result.Success(new ProcessSaleTransactionResponse {
                        ResponseMessage = transaction.ResponseMessage,
                        ResponseCode = transaction.ResponseCode,
                        EstateId = command.EstateId,
                        MerchantId = command.MerchantId,
                        AdditionalTransactionMetadata = transaction.AdditionalResponseMetadata,
                        TransactionId = command.TransactionId
                    });
                }, command.TransactionId, cancellationToken,false);

            return result;
        }

        public async Task<Result> ResendTransactionReceipt(TransactionCommands.ResendTransactionReceiptCommand command,
                                                           CancellationToken cancellationToken) {

            Result result = await ApplyUpdates(
                async (TransactionAggregate transactionAggregate) => {
                    transactionAggregate.RequestEmailReceiptResend();

                    return Result.Success();
                }, command.TransactionId, cancellationToken);
            return result;
        }

        internal static Boolean RequireFeeCalculation(TransactionAggregate transactionAggregate)
        {
            return transactionAggregate switch
            {
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
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            Result result = await ApplyUpdates(
                async (TransactionAggregate transactionAggregate) => {

                    if (RequireFeeCalculation(transactionAggregate) == false)
                        return Result.Success();

                    List<TransactionFeeToCalculate> feesForCalculation = await this.GetTransactionFeesForCalculation(transactionAggregate, cancellationToken);

                    if (feesForCalculation == null)
                        return Result.Failure("Error getting transaction fees");

                    List<CalculatedFee> resultFees =
                        this.FeeCalculationManager.CalculateFees(feesForCalculation, transactionAggregate.TransactionAmount.Value, command.CompletedDateTime);

                    IEnumerable<CalculatedFee> nonMerchantFees = resultFees.Where(f => f.FeeType == FeeType.ServiceProvider);
                    foreach (CalculatedFee calculatedFee in nonMerchantFees){
                        // Add Fee to the Transaction 
                        transactionAggregate.AddFee(calculatedFee);
                    }

                    // Now deal with merchant fees 
                    List<CalculatedFee> merchantFees = resultFees.Where(f => f.FeeType == FeeType.Merchant).ToList();

                    if (merchantFees.Any())
                    {
                        var merchant = await this.GetMerchant(command.EstateId, command.MerchantId, cancellationToken);
                        if (merchant.SettlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.NotSet)
                        {
                            // TODO: Result
                            //throw new NotSupportedException($"Merchant {merchant.MerchantId} does not have a settlement schedule configured");
                        }

                        foreach (CalculatedFee calculatedFee in merchantFees)
                        {
                            // Determine when the fee should be applied
                            DateTime settlementDate = CalculateSettlementDate(merchant.SettlementSchedule, command.CompletedDateTime);

                            transactionAggregate.AddFeePendingSettlement(calculatedFee, settlementDate);


                            if (merchant.SettlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate)
                            {
                                Guid settlementId = Helpers.CalculateSettlementAggregateId(settlementDate, command.MerchantId, command.EstateId);

                                // Add fees to transaction now if settlement is immediate
                                transactionAggregate.AddSettledFee(calculatedFee,
                                                                   settlementDate,
                                                                   settlementId);
                            }
                        }
                    }

                    return Result.Success();
                }, command.TransactionId, cancellationToken);
            return result;
        }

        public async Task<Result> AddSettledMerchantFee(TransactionCommands.AddSettledMerchantFeeCommand command,
                                                        CancellationToken cancellationToken) {
            Result result = await ApplyUpdates(
                async (TransactionAggregate transactionAggregate) => {
                    CalculatedFee calculatedFee = new CalculatedFee
                    {
                        CalculatedValue = command.CalculatedValue,
                        FeeCalculatedDateTime = command.FeeCalculatedDateTime,
                        FeeCalculationType = command.FeeCalculationType,
                        FeeId = command.FeeId,
                        FeeType = FeeType.Merchant,
                        FeeValue = command.FeeValue,
                        IsSettled = true
                    };

                    transactionAggregate.AddSettledFee(calculatedFee, command.SettledDateTime, command.SettlementId);

                    return Result.Success();
                }, command.TransactionId, cancellationToken);
            return result;
        }

        internal static DateTime CalculateSettlementDate(EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule merchantSettlementSchedule,
                                                         DateTime completeDateTime)
        {
            if (merchantSettlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly)
            {
                return completeDateTime.Date.AddDays(7).Date;
            }

            if (merchantSettlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly)
            {
                return completeDateTime.Date.AddMonths(1).Date;
            }

            return completeDateTime.Date;
        }

        private async Task<List<TransactionFeeToCalculate>> GetTransactionFeesForCalculation(TransactionAggregate transactionAggregate, CancellationToken cancellationToken)
        {
            // TODO: convert to result??

            Boolean contractProductFeeCacheEnabled;
            String contractProductFeeCacheEnabledValue = ConfigurationReader.GetValue("ContractProductFeeCacheEnabled");
            if (String.IsNullOrEmpty(contractProductFeeCacheEnabledValue))
            {
                contractProductFeeCacheEnabled = false;
            }
            else
            {
                contractProductFeeCacheEnabled = Boolean.Parse(contractProductFeeCacheEnabledValue);
            }

            Boolean feesInCache;
            Result<List<ContractProductTransactionFee>> feesForProduct = null;
            if (contractProductFeeCacheEnabled == false)
            {
                feesInCache = false;
            }
            else
            {
                // Ok we should have filtered out the not applicable transactions
                // Check if we have fees for this product in the cache
                feesInCache = this.MemoryCache.TryGetValue((transactionAggregate.EstateId, transactionAggregate.ContractId, transactionAggregate.ProductId),
                                                                   out feesForProduct);
            }

            if (feesInCache == false)
            {
                Logger.LogInformation($"Fees for Key: Estate Id {transactionAggregate.EstateId} Contract Id {transactionAggregate.ContractId} ProductId {transactionAggregate.ProductId} not found in the cache");

                // Nothing in cache so we need to make a remote call
                // Get the fees to be calculated
                feesForProduct = await this.EstateClient.GetTransactionFeesForProduct(this.TokenResponse.AccessToken,
                                                                                      transactionAggregate.EstateId,
                                                                                      transactionAggregate.MerchantId,
                                                                                      transactionAggregate.ContractId,
                                                                                      transactionAggregate.ProductId,
                                                                                      cancellationToken);


                if (feesForProduct.IsFailed) {
                    Logger.LogWarning($"Failed to get fees {feesForProduct.Message}");
                    return null;
                }

                Logger.LogInformation($"After getting Fees {feesForProduct.Data.Count} returned");

                if (contractProductFeeCacheEnabled == true)
                {
                    // Now add this the result to the cache
                    String contractProductFeeCacheExpiryInHours = ConfigurationReader.GetValue("ContractProductFeeCacheExpiryInHours");
                    if (String.IsNullOrEmpty(contractProductFeeCacheExpiryInHours))
                    {
                        contractProductFeeCacheExpiryInHours = "168"; // 7 Days default
                    }

                    this.MemoryCache.Set((transactionAggregate.EstateId, transactionAggregate.ContractId, transactionAggregate.ProductId),
                                         feesForProduct.Data,
                                         new MemoryCacheEntryOptions()
                                         {
                                             AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(Int32.Parse(contractProductFeeCacheExpiryInHours))
                                         });
                    Logger.LogInformation($"Fees for Key: Estate Id {transactionAggregate.EstateId} Contract Id {transactionAggregate.ContractId} ProductId {transactionAggregate.ProductId} added to cache");
                }
            }
            else
            {
                Logger.LogInformation($"Fees for Key: Estate Id {transactionAggregate.EstateId} Contract Id {transactionAggregate.ContractId} ProductId {transactionAggregate.ProductId} found in the cache");
            }

            List<TransactionFeeToCalculate> feesForCalculation = new List<TransactionFeeToCalculate>();

            foreach (ContractProductTransactionFee contractProductTransactionFee in feesForProduct.Data)
            {
                TransactionFeeToCalculate transactionFeeToCalculate = new TransactionFeeToCalculate
                {
                    FeeId = contractProductTransactionFee.TransactionFeeId,
                    Value = contractProductTransactionFee.Value,
                    FeeType = (FeeType)contractProductTransactionFee.FeeType,
                    CalculationType =
                                                                              (CalculationType)contractProductTransactionFee.CalculationType
                };

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
        private async Task AddDeviceToMerchant(Guid estateId,
                                               Guid merchantId,
                                               String deviceIdentifier,
                                               CancellationToken cancellationToken){
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            // Add the device to the merchant
            await this.EstateClient.AddDeviceToMerchant(this.TokenResponse.AccessToken,
                                                        estateId,
                                                        merchantId,
                                                        new AddMerchantDeviceRequest{
                                                                                        DeviceIdentifier = deviceIdentifier
                                                                                    },
                                                        cancellationToken);
        }

        /// <summary>
        /// Generates the transaction reference.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        private String GenerateTransactionReference(){
            Int64 i = 1;
            foreach (Byte b in Guid.NewGuid().ToByteArray()){
                i *= (b + 1);
            }

            return $"{i - DateTime.Now.Ticks:x}";
        }

        private async Task<EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse> GetMerchant(Guid estateId,
                                                                                                                                   Guid merchantId,
                                                                                                                                   CancellationToken cancellationToken){
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);

            return merchant;
        }
        
        private async Task<Result<OperatorResponse>> ProcessMessageWithOperator(EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant,
                                                                        Guid transactionId,
                                                                        DateTime transactionDateTime,
                                                                        Guid operatorId,
                                                                        Dictionary<String, String> additionalTransactionMetadata,
                                                                        String transactionReference,
                                                                        CancellationToken cancellationToken){

            // TODO: introduce some kind of mapping in here to link operator id to the name
            // Get Operators from the Estate Management API
            Result<EstateResponse> getEstateResult = await this.EstateClient.GetEstate(this.TokenResponse.AccessToken, merchant.EstateId, cancellationToken);
            EstateOperatorResponse @operator = getEstateResult.Data.Operators.Single(e => e.OperatorId == operatorId);
            
            IOperatorProxy operatorProxy = this.OperatorProxyResolver(@operator.Name.Replace(" ", ""));
            try{
                Result<OperatorResponse> saleResult = await operatorProxy.ProcessSaleMessage(this.TokenResponse.AccessToken,
                                                                          transactionId,
                                                                          operatorId,
                                                                          merchant,
                                                                          transactionDateTime,
                                                                          transactionReference,
                                                                          additionalTransactionMetadata,
                                                                          cancellationToken);
                if (saleResult.IsFailed) {
                    return CreateFailedResult(new OperatorResponse {
                        IsSuccessful = false, ResponseCode = "9999", ResponseMessage = saleResult.Message
                    });
                }

                return saleResult;
            }
            catch(Exception e){
                // Log out the error
                Logger.LogError(e);
                return CreateFailedResult(new OperatorResponse
                {
                    IsSuccessful = false,
                    ResponseCode = "9999",
                    ResponseMessage =e.GetCombinedExceptionMessages()
                });
            }
        }

        #endregion

        internal static Result<T> CreateFailedResult<T>(T resultData)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Data = resultData
            };
        }
    }
}