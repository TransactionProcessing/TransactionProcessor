using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.BusinessLogic.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProjectionEngine.State;
using Shared.EventStore.EventStore;

public interface ITransactionValidationService
{
    #region Methods

    Task<Result<TransactionValidationResult>> ValidateLogonTransaction(Guid estateId,
                                                                       Guid merchantId,
                                                                       String deviceIdentifier,
                                                                       CancellationToken cancellationToken);
    Task<Result<TransactionValidationResult>> ValidateReconciliationTransaction(Guid estateId,
                                                                                Guid merchantId,
                                                                                String deviceIdentifier,
                                                                                CancellationToken cancellationToken);

    Task<Result<TransactionValidationResult>> ValidateSaleTransaction(Guid estateId,
                                                                      Guid merchantId,
                                                                      Guid contractId,
                                                                      Guid productId,
                                                                      String deviceIdentifier,
                                                                      Guid operatorId,
                                                                      Decimal? transactionAmount,
                                                                      CancellationToken cancellationToken);

    #endregion
}

public class TransactionValidationService : ITransactionValidationService{
    #region Fields
    private readonly IEventStoreContext EventStoreContext;
    private readonly IAggregateService AggregateService;
    #endregion

    public TransactionValidationService(IEventStoreContext eventStoreContext,
                                        IAggregateService aggregateService)
    {
        this.EventStoreContext = eventStoreContext;
        this.AggregateService = aggregateService;
    }

    #region Methods
    
    internal static Result<T> CreateFailedResult<T>(T resultData) {
        return new Result<T>
        {
            IsSuccess = false,
            Data = resultData
        };
    }


    public async Task<Result<TransactionValidationResult>> ValidateLogonTransaction(Guid estateId,
                                                                                     Guid merchantId,
                                                                                     String deviceIdentifier,
                                                                                     CancellationToken cancellationToken) {
        // Validate Estate
        Result<TransactionValidationResult<EstateAggregate>> estateValidationResult = await ValidateEstate(estateId, cancellationToken);
        if (estateValidationResult.IsFailed) return CreateFailedResult(estateValidationResult.Data.validationResult);

        // Validate Merchant
        var merchantValidationResult = await ValidateMerchant(estateId, estateValidationResult.Data.additionalData.EstateName, merchantId, cancellationToken);
        if (merchantValidationResult.IsFailed) return CreateFailedResult(merchantValidationResult.Data.validationResult); ;

        Models.Merchant.Merchant merchant = merchantValidationResult.Data.additionalData.GetMerchant();

        // Validate Device
        Result<TransactionValidationResult> deviceValidationResult = ValidateDeviceForLogon(merchant, deviceIdentifier);
        if (deviceValidationResult.IsFailed) return deviceValidationResult;
        
        // Validate the merchant device
        return deviceValidationResult;
    }

    public async Task<Result<TransactionValidationResult>> ValidateReconciliationTransaction(Guid estateId,
        Guid merchantId,
        String deviceIdentifier,
        CancellationToken cancellationToken)
    {
        // Validate Estate
        Result<TransactionValidationResult<EstateAggregate>> estateValidationResult = await ValidateEstate(estateId, cancellationToken);
        if (estateValidationResult.IsFailed) return CreateFailedResult(estateValidationResult.Data.validationResult);

        EstateAggregate estate = estateValidationResult.Data.additionalData;

        // Validate Merchant
        Result<TransactionValidationResult<MerchantAggregate>> merchantValidationResult = await ValidateMerchant(estateId, estate.EstateName, merchantId, cancellationToken);
        if (merchantValidationResult.IsFailed) return CreateFailedResult(merchantValidationResult.Data.validationResult); ;

        var merchant = merchantValidationResult.Data.additionalData.GetMerchant();

        // Validate Device
        Result<TransactionValidationResult> deviceValidationResult = ValidateDevice(merchant, deviceIdentifier);
        if (deviceValidationResult.IsFailed) return deviceValidationResult;

        // Validate the merchant device
        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }

    #endregion

    public async Task<Result<TransactionValidationResult>> ValidateSaleTransaction(Guid estateId,
                                                                               Guid merchantId,
                                                                               Guid contractId,
                                                                               Guid productId,
                                                                               String deviceIdentifier,
                                                                               Guid operatorId,
                                                                               Decimal? transactionAmount,
                                                                               CancellationToken cancellationToken)
    {
        // Validate Estate
        Result<TransactionValidationResult<EstateAggregate>> estateValidationResult = await ValidateEstate(estateId, cancellationToken);
        if (estateValidationResult.IsFailed) return CreateFailedResult(estateValidationResult.Data.validationResult);

        EstateAggregate estate = estateValidationResult.Data.additionalData;

        // Validate Operator for Estate
        Result<TransactionValidationResult> estateOperatorValidationResult = ValidateEstateOperator(estate, operatorId);
        if (estateOperatorValidationResult.IsFailed) return estateOperatorValidationResult;

        // Validate Merchant
        Result<TransactionValidationResult<MerchantAggregate>> merchantValidationResult = await ValidateMerchant(estateId, estate.EstateName, merchantId, cancellationToken);
        if (merchantValidationResult.IsFailed) return CreateFailedResult(merchantValidationResult.Data.validationResult); ;

        Models.Merchant.Merchant merchant = merchantValidationResult.Data.additionalData.GetMerchant();

        // Validate Device
        Result<TransactionValidationResult> deviceValidationResult = ValidateDevice(merchant, deviceIdentifier);
        if (deviceValidationResult.IsFailed) return deviceValidationResult;

        // Validate Operator for Merchant
        Result<TransactionValidationResult> merchantOperatorValidationResult = ValidateMerchantOperator(merchant, operatorId);
        if (merchantOperatorValidationResult.IsFailed) return merchantOperatorValidationResult;

        // Validate Contract and Product
        Result<TransactionValidationResult> contractProductValidationResult = await ValidateContractAndProduct(merchant, contractId, productId, cancellationToken);
        if (contractProductValidationResult.IsFailed) return contractProductValidationResult;

        // Validate Transaction Amount
        Result<TransactionValidationResult> transactionAmountValidationResult = await ValidateTransactionAmount(merchantId, merchant.MerchantName, transactionAmount, cancellationToken);
        if (transactionAmountValidationResult.IsFailed) return transactionAmountValidationResult;

        // If we get here everything is good
        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }

    private async Task<Result<TransactionValidationResult<EstateAggregate>>> ValidateEstate(Guid estateId, CancellationToken cancellationToken)
    {
        var getEstateResult= await this.AggregateService.Get<EstateAggregate>(estateId, cancellationToken);
        if (getEstateResult.IsFailed) {
            TransactionValidationResult transactionValidationResult = getEstateResult.Status switch
            {
                ResultStatus.NotFound => new TransactionValidationResult(TransactionResponseCode.InvalidEstateId, $"Estate Id [{estateId}] is not a valid estate"),
                _ => new TransactionValidationResult(TransactionResponseCode.UnknownFailure, $"An error occurred while getting Estate Id [{estateId}] Message: [{getEstateResult.Message}]")
            };
            return CreateFailedResult(new TransactionValidationResult<EstateAggregate>(transactionValidationResult));
        }
        return Result.Success(new TransactionValidationResult<EstateAggregate>(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"), getEstateResult.Data));
    }

    private Result<TransactionValidationResult> ValidateEstateOperator(EstateAggregate estate, Guid operatorId) {
        List<Models.Estate.Operator> estateOperators = estate.GetEstate().Operators;

        if (estateOperators == null || estateOperators.Any() == false)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.NoEstateOperators, $"Estate {estate.EstateName} has no operators defined"));
        }

        Models.Estate.Operator estateOperatorRecord = estateOperators.SingleOrDefault(o => o.OperatorId == operatorId);

        Result<TransactionValidationResult> result = estateOperatorRecord switch {
            null => CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotValidForEstate, $"Operator {operatorId} not configured for Estate [{estate.EstateName}]")),
            _ when estateOperatorRecord.IsDeleted => CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotEnabledForEstate, $"Operator {operatorId} not enabled for Estate [{estate.EstateName}]")),
            _ => Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"))
        };
        return result;
    }

    private async Task<Result<TransactionValidationResult<MerchantAggregate>>> ValidateMerchant(Guid estateId, String estateName, Guid merchantId, CancellationToken cancellationToken)
    {
        Result<MerchantAggregate> getMerchantResult = await this.AggregateService.Get<MerchantAggregate>(merchantId, cancellationToken);
        if (getMerchantResult.IsFailed)
        {
            TransactionValidationResult transactionValidationResult = getMerchantResult.Status switch
            {
                ResultStatus.NotFound => new TransactionValidationResult(TransactionResponseCode.InvalidMerchantId, $"Merchant Id [{merchantId}] is not a valid merchant for estate [{estateName}]"),
                _ => new TransactionValidationResult(TransactionResponseCode.UnknownFailure, $"An error occurred while getting Merchant Id [{merchantId}] Message: [{getMerchantResult.Message}]")
            };
            return CreateFailedResult(new TransactionValidationResult<MerchantAggregate>(transactionValidationResult));
        }
        return Result.Success(new TransactionValidationResult<MerchantAggregate>(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"), getMerchantResult.Data));

    }

    private Result<TransactionValidationResult> ValidateDevice(Models.Merchant.Merchant merchant, String deviceIdentifier)
    {
        if (merchant.Devices == null || !merchant.Devices.Any())
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.NoValidDevices, $"Merchant {merchant.MerchantName} has no valid Devices for this transaction."));
        }

        Device device = merchant.Devices.SingleOrDefault(d => d.DeviceIdentifier == deviceIdentifier);
        if (device == null)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidDeviceIdentifier, $"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}"));
        }

        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }

    private Result<TransactionValidationResult> ValidateDeviceForLogon(Models.Merchant.Merchant merchant, String deviceIdentifier)
    {
        if (merchant.Devices == null || !merchant.Devices.Any())
        {
            return Result.Success(new TransactionValidationResult(TransactionResponseCode.SuccessNeedToAddDevice,"SUCCESS"));
        }

        Device device = merchant.Devices.SingleOrDefault(d => d.DeviceIdentifier == deviceIdentifier);
        if (device == null)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidDeviceIdentifier, $"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}"));
        }

        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }

    private Result<TransactionValidationResult> ValidateMerchantOperator(Models.Merchant.Merchant merchant, Guid operatorId)
    {
        if (merchant.Operators == null || !merchant.Operators.Any())
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.NoMerchantOperators, $"Merchant {merchant.MerchantName} has no operators defined"));
        }

        Operator merchantOperatorRecord = merchant.Operators.SingleOrDefault(o => o.OperatorId == operatorId);

        Result<TransactionValidationResult> result = merchantOperatorRecord switch
        {
            null => CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotValidForMerchant, $"Operator {operatorId} not configured for Merchant [{merchant.MerchantName}]")),
            _ when merchantOperatorRecord.IsDeleted => CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotEnabledForMerchant, $"Operator {operatorId} not enabled for Merchant [{merchant.MerchantName}]")),
            _ => Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"))
        };
        return result;
    }

    private async Task<Result<TransactionValidationResult>> ValidateContractAndProduct(Models.Merchant.Merchant merchant, Guid contractId, Guid productId, CancellationToken cancellationToken)
    {
        if (contractId == Guid.Empty)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidContractIdValue, $"Contract Id [{contractId}] must be set for a sale transaction"));
        }

        var merchantContracts = merchant.Contracts;
        if (merchantContracts == null || !merchantContracts.Any())
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.MerchantHasNoContractsConfigured, $"Merchant {merchant.MerchantId} has no contracts configured"));
        }

        var contract = merchantContracts.SingleOrDefault(c => c.ContractId == contractId);
        if (contract == null)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.ContractNotValidForMerchant, $"Contract Id [{contractId}] not valid for Merchant [{merchant.MerchantName}]"));
        }

        if (productId == Guid.Empty)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidProductIdValue, $"Product Id [{productId}] must be set for a sale transaction"));
        }

        var contractProduct = contract.ContractProducts.SingleOrDefault(p => p == productId);
        if (contractProduct == Guid.Empty)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.ProductNotValidForMerchant, $"Product Id [{productId}] not valid for Merchant [{merchant.MerchantName}]"));
        }

        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }

    private async Task<Result<TransactionValidationResult>> ValidateTransactionAmount(Guid merchantId, String merchantName, Decimal? transactionAmount, CancellationToken cancellationToken)
    {
        if (transactionAmount is <= 0)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidSaleTransactionAmount, "Transaction Amount must be greater than 0"));
        }

        Result<String> getBalanceResult = await this.EventStoreContext.GetPartitionStateFromProjection("MerchantBalanceProjection", $"MerchantBalance-{merchantId:N}", cancellationToken);
        if (getBalanceResult.IsFailed)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.UnknownFailure, $"Error getting balance for Merchant [{merchantName}]"));
        }

        MerchantBalanceProjectionState1 projectionState = JsonConvert.DeserializeObject<MerchantBalanceProjectionState1>(getBalanceResult.Data);
        if (projectionState.merchant.balance < transactionAmount)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.MerchantDoesNotHaveEnoughCredit, $"Merchant [{merchantName}] does not have enough credit available [{projectionState.merchant.balance:0.00}] to perform transaction amount [{transactionAmount}]"));
        }

        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }
}

[ExcludeFromCodeCoverage]
public record TransactionValidationResult(TransactionResponseCode ResponseCode, String ResponseMessage);

[ExcludeFromCodeCoverage]
public record TransactionValidationResult<T>(TransactionValidationResult validationResult, T additionalData = default);