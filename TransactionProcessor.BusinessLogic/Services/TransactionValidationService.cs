using System.Runtime.CompilerServices;
using EstateManagement.DataTransferObjects.Responses.Merchant;
using Shared.Results;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using EstateManagement.Client;
using EstateManagement.Database.Entities;
using EstateManagement.DataTransferObjects.Responses;
using EstateManagement.DataTransferObjects.Responses.Estate;
using EventStore.Client;
using Newtonsoft.Json;
using ProjectionEngine.Repository;
using ProjectionEngine.State;
using SecurityService.Client;
using SecurityService.DataTransferObjects.Responses;
using Shared.EventStore.EventStore;
using Shared.EventStore.ProjectionEngine;
using Shared.General;
using Shared.Logger;

public class TransactionValidationService : ITransactionValidationService{
    #region Fields

    /// <summary>
    /// The estate client
    /// </summary>
    private readonly IEstateClient EstateClient;

    private readonly ProjectionEngine.Repository.IProjectionStateRepository<MerchantBalanceState> MerchantBalanceStateRepository;

    private readonly IEventStoreContext EventStoreContext;


    /// <summary>
    /// The security service client
    /// </summary>
    private readonly ISecurityServiceClient SecurityServiceClient;

    private TokenResponse TokenResponse;

    #endregion

    public TransactionValidationService(IEstateClient estateClient,
                                        ISecurityServiceClient securityServiceClient,
                                        ProjectionEngine.Repository.IProjectionStateRepository<MerchantBalanceState> merchantBalanceStateRepository,
                                        IEventStoreContext eventStoreContext)
    {
        this.EstateClient = estateClient;
        this.SecurityServiceClient = securityServiceClient;
        this.MerchantBalanceStateRepository = merchantBalanceStateRepository;
        this.EventStoreContext = eventStoreContext;
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
        Result<TransactionValidationResult<EstateResponse>> estateValidationResult = await ValidateEstate(estateId, cancellationToken);
        if (estateValidationResult.IsFailed) return CreateFailedResult(estateValidationResult.Data.validationResult);

        // Validate Merchant
        Result<TransactionValidationResult<MerchantResponse>> merchantValidationResult = await ValidateMerchant(estateId, merchantId, cancellationToken);
        if (merchantValidationResult.IsFailed) return CreateFailedResult(merchantValidationResult.Data.validationResult); ;

        MerchantResponse merchant = merchantValidationResult.Data.additionalData;

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
        Result<TransactionValidationResult<EstateResponse>> estateValidationResult = await ValidateEstate(estateId, cancellationToken);
        if (estateValidationResult.IsFailed) return CreateFailedResult(estateValidationResult.Data.validationResult);

        EstateResponse estate = estateValidationResult.Data.additionalData;

        // Validate Merchant
        Result<TransactionValidationResult<MerchantResponse>> merchantValidationResult = await ValidateMerchant(estateId, merchantId, cancellationToken);
        if (merchantValidationResult.IsFailed) return CreateFailedResult(merchantValidationResult.Data.validationResult); ;

        MerchantResponse merchant = merchantValidationResult.Data.additionalData;

        // Validate Device
        Result<TransactionValidationResult> deviceValidationResult = ValidateDevice(merchant, deviceIdentifier);
        if (deviceValidationResult.IsFailed) return deviceValidationResult;

        // Validate the merchant device
        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }
    
    private async Task<Result<EstateResponse>> GetEstate(Guid estateId,
                                                         CancellationToken cancellationToken){
        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        return await this.EstateClient.GetEstate(this.TokenResponse.AccessToken, estateId, cancellationToken);
    }

    private async Task<Result<EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse>> GetMerchant(Guid estateId,
                                                                                                             Guid merchantId,
                                                                                                             CancellationToken cancellationToken){
        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        return await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);
    }

    private async Task<Result<List<EstateManagement.DataTransferObjects.Responses.Merchant.MerchantContractResponse>>> GetMerchantContracts(Guid estateId,
                                                                                                                                    Guid merchantId,
                                                                                                                                    CancellationToken cancellationToken)
    {
        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        var result = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);
        if (result.IsFailed)
            return ResultHelpers.CreateFailure(result);
        return result.Data.Contracts;
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
        Result<TransactionValidationResult<EstateResponse>> estateValidationResult = await ValidateEstate(estateId, cancellationToken);
        if (estateValidationResult.IsFailed) return CreateFailedResult(estateValidationResult.Data.validationResult);

        EstateResponse estate = estateValidationResult.Data.additionalData;

        // Validate Operator for Estate
        Result<TransactionValidationResult> estateOperatorValidationResult = ValidateEstateOperator(estate, operatorId);
        if (estateOperatorValidationResult.IsFailed) return estateOperatorValidationResult;

        // Validate Merchant
        Result<TransactionValidationResult<MerchantResponse>> merchantValidationResult = await ValidateMerchant(estateId, merchantId, cancellationToken);
        if (merchantValidationResult.IsFailed) return CreateFailedResult(merchantValidationResult.Data.validationResult); ;

        MerchantResponse merchant = merchantValidationResult.Data.additionalData;

        // Validate Device
        Result<TransactionValidationResult> deviceValidationResult = ValidateDevice(merchant, deviceIdentifier);
        if (deviceValidationResult.IsFailed) return deviceValidationResult;

        // Validate Operator for Merchant
        Result<TransactionValidationResult> merchantOperatorValidationResult = ValidateMerchantOperator(merchant, operatorId);
        if (merchantOperatorValidationResult.IsFailed) return merchantOperatorValidationResult;

        // Validate Contract and Product
        Result<TransactionValidationResult> contractProductValidationResult = await ValidateContractAndProduct(estateId, merchantId, contractId, productId, cancellationToken);
        if (contractProductValidationResult.IsFailed) return contractProductValidationResult;

        // Validate Transaction Amount
        Result<TransactionValidationResult> transactionAmountValidationResult = await ValidateTransactionAmount(merchantId, merchant.MerchantName, transactionAmount, cancellationToken);
        if (transactionAmountValidationResult.IsFailed) return transactionAmountValidationResult;

        // If we get here everything is good
        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }

    private async Task<Result<TransactionValidationResult<EstateResponse>>> ValidateEstate(Guid estateId, CancellationToken cancellationToken)
    {
        Result<EstateResponse> getEstateResult = await this.GetEstate(estateId, cancellationToken);
        if (getEstateResult.IsFailed) {
            TransactionValidationResult transactionValidationResult = getEstateResult.Status switch {
                ResultStatus.NotFound => new TransactionValidationResult(TransactionResponseCode.InvalidEstateId, $"Estate Id [{estateId}] is not a valid estate"),
                _ => new TransactionValidationResult(TransactionResponseCode.UnknownFailure, $"An error occurred while getting Estate Id [{estateId}] Message: [{getEstateResult.Message}]")
            };

            return CreateFailedResult(new TransactionValidationResult<EstateResponse>(transactionValidationResult));
        }
        return Result.Success(new TransactionValidationResult<EstateResponse>(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"), getEstateResult.Data));
    }

    private Result<TransactionValidationResult> ValidateEstateOperator(EstateResponse estate, Guid operatorId)
    {
        if (estate.Operators == null || !estate.Operators.Any())
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.NoEstateOperators, $"Estate {estate.EstateName} has no operators defined"));
        }

        EstateOperatorResponse estateOperatorRecord = estate.Operators.SingleOrDefault(o => o.OperatorId == operatorId);

        Result<TransactionValidationResult> result = estateOperatorRecord switch {
            null => CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotValidForEstate, $"Operator {operatorId} not configured for Estate [{estate.EstateName}]")),
            _ when estateOperatorRecord.IsDeleted => CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotEnabledForEstate, $"Operator {operatorId} not enabled for Estate [{estate.EstateName}]")),
            _ => Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"))
        };
        return result;
    }

    private async Task<Result<TransactionValidationResult<MerchantResponse>>> ValidateMerchant(Guid estateId, Guid merchantId, CancellationToken cancellationToken)
    {
        Result<MerchantResponse> getMerchantResult = await this.GetMerchant(estateId, merchantId, cancellationToken);
        if (getMerchantResult.IsFailed)
        {
            TransactionValidationResult transactionValidationResult = getMerchantResult.Status switch
            {
                ResultStatus.NotFound => new TransactionValidationResult(TransactionResponseCode.InvalidMerchantId, $"Merchant Id [{merchantId}] is not a valid merchant for estate [{estateId}]"),
                _ => new TransactionValidationResult(TransactionResponseCode.UnknownFailure, $"An error occurred while getting Merchant Id [{merchantId}] Message: [{getMerchantResult.Message}]")
            };

            return CreateFailedResult(new TransactionValidationResult<MerchantResponse>(transactionValidationResult));
        }
        return Result.Success(new TransactionValidationResult<MerchantResponse>(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"), getMerchantResult.Data));
    }

    private Result<TransactionValidationResult> ValidateDevice(MerchantResponse merchant, String deviceIdentifier)
    {
        if (merchant.Devices == null || !merchant.Devices.Any())
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.NoValidDevices, $"Merchant {merchant.MerchantName} has no valid Devices for this transaction."));
        }

        KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);
        if (device.Key == Guid.Empty)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidDeviceIdentifier, $"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}"));
        }

        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }

    private Result<TransactionValidationResult> ValidateDeviceForLogon(MerchantResponse merchant, String deviceIdentifier)
    {
        if (merchant.Devices == null || !merchant.Devices.Any())
        {
            return Result.Success(new TransactionValidationResult(TransactionResponseCode.SuccessNeedToAddDevice,"SUCCESS"));
        }

        KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);
        if (device.Key == Guid.Empty)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidDeviceIdentifier, $"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}"));
        }

        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }

    private Result<TransactionValidationResult> ValidateMerchantOperator(MerchantResponse merchant, Guid operatorId)
    {
        if (merchant.Operators == null || !merchant.Operators.Any())
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.NoMerchantOperators, $"Merchant {merchant.MerchantName} has no operators defined"));
        }

        MerchantOperatorResponse merchantOperatorRecord = merchant.Operators.SingleOrDefault(o => o.OperatorId == operatorId);

        Result<TransactionValidationResult> result = merchantOperatorRecord switch
        {
            null => CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotValidForMerchant, $"Operator {operatorId} not configured for Merchant [{merchant.MerchantName}]")),
            _ when merchantOperatorRecord.IsDeleted => CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotEnabledForMerchant, $"Operator {operatorId} not enabled for Merchant [{merchant.MerchantName}]")),
            _ => Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"))
        };
        return result;
    }

    private async Task<Result<TransactionValidationResult>> ValidateContractAndProduct(Guid estateId, Guid merchantId, Guid contractId, Guid productId, CancellationToken cancellationToken)
    {
        if (contractId == Guid.Empty)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidContractIdValue, $"Contract Id [{contractId}] must be set for a sale transaction"));
        }

        var getMerchantContractsResult = await this.GetMerchantContracts(estateId, merchantId, cancellationToken);
        if (getMerchantContractsResult.IsFailed)
        {
            return getMerchantContractsResult.Status == ResultStatus.NotFound
                ? CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidMerchantId, $"Merchant Id [{merchantId}] is not a valid merchant for estate [{estateId}]"))
                : CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.UnknownFailure, $"An error occurred while getting Merchant Id [{merchantId}] Contracts Message: [{getMerchantContractsResult.Message}]"));
        }

        var merchantContracts = getMerchantContractsResult.Data;
        if (merchantContracts == null || !merchantContracts.Any())
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.MerchantHasNoContractsConfigured, $"Merchant {merchantId} has no contracts configured"));
        }

        var contract = merchantContracts.SingleOrDefault(c => c.ContractId == contractId);
        if (contract == null)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.ContractNotValidForMerchant, $"Contract Id [{contractId}] not valid for Merchant [{merchantId}]"));
        }

        if (productId == Guid.Empty)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidProductIdValue, $"Product Id [{productId}] must be set for a sale transaction"));
        }

        var contractProduct = contract.ContractProducts.SingleOrDefault(p => p == productId);
        if (contractProduct == Guid.Empty)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.ProductNotValidForMerchant, $"Product Id [{productId}] not valid for Merchant [{merchantId}]"));
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