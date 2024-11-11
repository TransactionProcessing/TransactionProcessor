using System.Runtime.CompilerServices;
using EstateManagement.DataTransferObjects.Responses.Merchant;
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
using Shared.General;
using Shared.Logger;

public class TransactionValidationService : ITransactionValidationService{
    #region Fields

    /// <summary>
    /// The estate client
    /// </summary>
    private readonly IEstateClient EstateClient;

    private readonly IProjectionStateRepository<MerchantBalanceState> MerchantBalanceStateRepository;

    private readonly IEventStoreContext EventStoreContext;


    /// <summary>
    /// The security service client
    /// </summary>
    private readonly ISecurityServiceClient SecurityServiceClient;

    private TokenResponse TokenResponse;

    #endregion

    public TransactionValidationService(IEstateClient estateClient,
                                        ISecurityServiceClient securityServiceClient,
                                        IProjectionStateRepository<MerchantBalanceState> merchantBalanceStateRepository,
                                        IEventStoreContext eventStoreContext)
    {
        this.EstateClient = estateClient;
        this.SecurityServiceClient = securityServiceClient;
        this.MerchantBalanceStateRepository = merchantBalanceStateRepository;
        this.EventStoreContext = eventStoreContext;
    }

    #region Methods

    public async Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateLogonTransaction(Guid estateId,
                                                                                                               Guid merchantId,
                                                                                                               String deviceIdentifier,
                                                                                                               CancellationToken cancellationToken){
        try{
            (EstateResponse estate, EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant) validateTransactionResponse = await this.ValidateMerchant(estateId, merchantId, cancellationToken);
            EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant = validateTransactionResponse.merchant;

            // Device Validation
            if (merchant.Devices == null || merchant.Devices.Any() == false){
                return ("SUCCESS", TransactionResponseCode.SuccessNeedToAddDevice);
            }

            // Validate the device
            KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);

            if (device.Key == Guid.Empty){
                // Device not found,throw error
                throw new TransactionValidationException($"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}",
                                                         TransactionResponseCode.InvalidDeviceIdentifier);
            }

            // If we get here everything is good
            return ("SUCCESS", TransactionResponseCode.Success);
        }
        catch(TransactionValidationException tvex){
            return (tvex.Message, tvex.ResponseCode);
        }
    }

    internal static Result<T> CreateFailedResult<T>(T resultData) {
        return new Result<T>
        {
            IsSuccess = false,
            Data = resultData
        };
    }

    public async Task<Result<TransactionValidationResult>> ValidateLogonTransactionX(Guid estateId,
                                                                                     Guid merchantId,
                                                                                     String deviceIdentifier,
                                                                                     CancellationToken cancellationToken) {
        // Validate Estate
        Result<EstateResponse> getEstateResult = await this.GetEstate(estateId, cancellationToken);
        if (getEstateResult.IsFailed && getEstateResult.Status == ResultStatus.NotFound) {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidEstateId, $"Estate Id [{estateId}] is not a valid estate"));
        }
        if (getEstateResult.IsFailed && getEstateResult.Status != ResultStatus.NotFound) {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.UnknownFailure, $"An error occurred while getting Estate Id [{estateId}] Message: [{getEstateResult.Message}]"));
        }

        EstateResponse estate = getEstateResult.Data;
        // Validate Merchant
        Result<MerchantResponse> getMerchantResult = await this.GetMerchant(estateId, merchantId, cancellationToken);
        if (getMerchantResult.IsFailed && getMerchantResult.Status == ResultStatus.NotFound) {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidMerchantId,
                $"Merchant Id [{merchantId}] is not a valid merchant for estate [{estate.EstateName}]"));
        }

        if (getMerchantResult.IsFailed && getMerchantResult.Status != ResultStatus.NotFound) {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.UnknownFailure,
                $"An error occurred while getting Merchant Id [{merchantId}] Message: [{getMerchantResult.Message}]"));
        }

        MerchantResponse merchant = getMerchantResult.Data;
        // Device Validation
        if (merchant.Devices == null || merchant.Devices.Any() == false) {
            return Result.Success(new TransactionValidationResult(TransactionResponseCode.SuccessNeedToAddDevice,
                "SUCCESS"));
        }

        // Validate the device
        KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);

        if (device.Key == Guid.Empty) {
            // Device not found,throw error
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidDeviceIdentifier,
                $"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}"));
        }

        // Validate the merchant device
        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }

    public async Task<Result<TransactionValidationResult>> ValidateReconciliationTransactionX(Guid estateId,
        Guid merchantId,
        String deviceIdentifier,
        CancellationToken cancellationToken)
    {
        // Validate Estate
        Result<EstateResponse> getEstateResult = await this.GetEstate(estateId, cancellationToken);
        if (getEstateResult.IsFailed && getEstateResult.Status == ResultStatus.NotFound)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidEstateId, $"Estate Id [{estateId}] is not a valid estate"));
        }
        if (getEstateResult.IsFailed && getEstateResult.Status != ResultStatus.NotFound)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.UnknownFailure, $"An error occurred while getting Estate Id [{estateId}] Message: [{getEstateResult.Message}]"));
        }

        EstateResponse estate = getEstateResult.Data;
        // Validate Merchant
        Result<MerchantResponse> getMerchantResult = await this.GetMerchant(estateId, merchantId, cancellationToken);
        if (getMerchantResult.IsFailed && getMerchantResult.Status == ResultStatus.NotFound)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidMerchantId,
                $"Merchant Id [{merchantId}] is not a valid merchant for estate [{estate.EstateName}]"));
        }

        if (getMerchantResult.IsFailed && getMerchantResult.Status != ResultStatus.NotFound)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.UnknownFailure,
                $"An error occurred while getting Merchant Id [{merchantId}] Message: [{getMerchantResult.Message}]"));
        }

        MerchantResponse merchant = getMerchantResult.Data;
        // Device Validation
        if (merchant.Devices == null || merchant.Devices.Any() == false)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.NoValidDevices,
                $"Merchant {merchant.MerchantName} has no valid Devices for this transaction."));
        }

        // Validate the device
        KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);

        if (device.Key == Guid.Empty)
        {
            // Device not found,throw error
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidDeviceIdentifier,
                $"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}"));
        }

        // Validate the merchant device
        return Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS"));
    }

    public async Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateReconciliationTransaction(Guid estateId,
                                                                                                                        Guid merchantId,
                                                                                                                        String deviceIdentifier,
                                                                                                                        CancellationToken cancellationToken){
        try{
            (EstateResponse estate, EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant) validateTransactionResponse = await this.ValidateMerchant(estateId, merchantId, cancellationToken);
            EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant = validateTransactionResponse.merchant;

            // Device Validation
            if (merchant.Devices == null || merchant.Devices.Any() == false){
                throw new TransactionValidationException($"Merchant {merchant.MerchantName} has no valid Devices for this transaction.",
                                                         TransactionResponseCode.NoValidDevices);
            }

            // Validate the device
            KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);

            if (device.Key == Guid.Empty){
                // Device not found,throw error
                throw new TransactionValidationException($"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}",
                                                         TransactionResponseCode.InvalidDeviceIdentifier);
            }

            // If we get here everything is good
            return ("SUCCESS", TransactionResponseCode.Success);
        }
        catch(TransactionValidationException tvex){
            return (tvex.Message, tvex.ResponseCode);
        }
    }

    public async Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateSaleTransaction(Guid estateId,
                                                                                                              Guid merchantId,
                                                                                                              Guid contractId,
                                                                                                              Guid productId,
                                                                                                              String deviceIdentifier,
                                                                                                              Guid operatorId,
                                                                                                              Decimal? transactionAmount,
                                                                                                              CancellationToken cancellationToken){
        try{
            (EstateResponse estate, EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant) validateTransactionResponse = await this.ValidateMerchant(estateId, merchantId, cancellationToken);
            EstateResponse estate = validateTransactionResponse.estate;
            EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant = validateTransactionResponse.merchant;

            // Operator Validation (Estate)
            if (estate.Operators == null || estate.Operators.Any() == false){
                throw new TransactionValidationException($"Estate {estate.EstateName} has no operators defined", TransactionResponseCode.NoEstateOperators);
            }

            // Operators have been configured for the estate
            EstateOperatorResponse estateOperatorRecord = estate.Operators.SingleOrDefault(o => o.OperatorId == operatorId);
            if (estateOperatorRecord == null){
                throw new TransactionValidationException($"Operator {operatorId} not configured for Estate [{estate.EstateName}]",
                                                         TransactionResponseCode.OperatorNotValidForEstate);
            }

            // Check the operator is enabled and not deleted
            if (estateOperatorRecord.IsDeleted){
                throw new TransactionValidationException($"Operator {operatorId} not enabled for Estate [{estate.EstateName}]",
                                                         TransactionResponseCode.OperatorNotEnabledForEstate);
            }

            // Device Validation
            if (merchant.Devices == null || merchant.Devices.Any() == false){
                throw new TransactionValidationException($"Merchant {merchant.MerchantName} has no valid Devices for this transaction.",
                                                         TransactionResponseCode.NoValidDevices);
            }

            // Validate the device
            KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);

            if (device.Key == Guid.Empty){
                // Device not found,throw error
                throw new TransactionValidationException($"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}",
                                                         TransactionResponseCode.InvalidDeviceIdentifier);
            }

            // Operator Validation (Merchant)
            if (merchant.Operators == null || merchant.Operators.Any() == false){
                throw new TransactionValidationException($"Merchant {merchant.MerchantName} has no operators defined", TransactionResponseCode.NoMerchantOperators);
            }

            {
                // Operators have been configured for the estate
                EstateManagement.DataTransferObjects.Responses.Merchant.MerchantOperatorResponse merchantOperatorRecord = merchant.Operators.SingleOrDefault(o => o.OperatorId == operatorId);
                if (merchantOperatorRecord == null){
                    throw new TransactionValidationException($"Operator {operatorId} not configured for Merchant [{merchant.MerchantName}]",
                                                             TransactionResponseCode.OperatorNotValidForMerchant);
                }

                // Check the operator is enabled and not deleted
                if (merchantOperatorRecord.IsDeleted)
                {
                    throw new TransactionValidationException($"Operator {operatorId} not enabled for Merchant [{merchant.MerchantName}]",
                                                             TransactionResponseCode.OperatorNotEnabledForMerchant);
                }
            }
            
            // Contract and Product Validation
            if (contractId == Guid.Empty){
                throw new TransactionValidationException($"Contract Id [{contractId}] must be set for a sale transaction",
                                                         TransactionResponseCode.InvalidContractIdValue);
            }

            List<EstateManagement.DataTransferObjects.Responses.Merchant.MerchantContractResponse> merchantContracts = null;
            try{
                merchantContracts = await this.GetMerchantContracts(estateId, merchantId, cancellationToken);
            }
            catch(Exception ex) when(ex.InnerException != null && ex.InnerException.GetType() == typeof(KeyNotFoundException)){
                throw new TransactionValidationException($"Merchant Id [{merchantId}] is not a valid merchant for estate [{estate.EstateName}]",
                                                         TransactionResponseCode.InvalidMerchantId);
            }
            catch(Exception e){
                throw new TransactionValidationException($"Exception occurred while getting Merchant Id [{merchantId}] Contracts Exception [{e.Message}]", TransactionResponseCode.UnknownFailure);
            }

            if (merchantContracts == null || merchantContracts.Any() == false){
                throw new TransactionValidationException($"Merchant {merchant.MerchantName} has no contracts configured", TransactionResponseCode.MerchantHasNoContractsConfigured);
            }

            // Check the contract and product id against the merchant
            EstateManagement.DataTransferObjects.Responses.Merchant.MerchantContractResponse contract = merchantContracts.SingleOrDefault(c => c.ContractId == contractId);

            if (contract == null){
                throw new TransactionValidationException($"Contract Id [{contractId}] not valid for Merchant [{merchant.MerchantName}]",
                                                         TransactionResponseCode.ContractNotValidForMerchant);
            }

            if (productId == Guid.Empty){
                throw new TransactionValidationException($"Product Id [{productId}] must be set for a sale transaction",
                                                         TransactionResponseCode.InvalidProductIdValue);
            }

            Guid contractProduct = contract.ContractProducts.SingleOrDefault(p => p == productId);

            if (contractProduct == Guid.Empty){
                throw new TransactionValidationException($"Product Id [{productId}] not valid for Merchant [{merchant.MerchantName}]",
                                                         TransactionResponseCode.ProductNotValidForMerchant);
            }

            // Check the amount
            if (transactionAmount.HasValue){
                if (transactionAmount <= 0){
                    throw new TransactionValidationException("Transaction Amount must be greater than 0", TransactionResponseCode.InvalidSaleTransactionAmount);
                }

                String state = await this.EventStoreContext.GetPartitionStateFromProjection("MerchantBalanceProjection", $"MerchantBalance-{merchantId:N}", cancellationToken);
                MerchantBalanceProjectionState1 projectionState = JsonConvert.DeserializeObject<MerchantBalanceProjectionState1>(state);
                
                // Check the merchant has enough balance to perform the sale
                if (projectionState.merchant.balance < transactionAmount){
                    throw new
                        TransactionValidationException($"Merchant [{merchant.MerchantName}] does not have enough credit available [{projectionState.merchant.balance:0.00}] to perform transaction amount [{transactionAmount}]",
                                                       TransactionResponseCode.MerchantDoesNotHaveEnoughCredit);
                }
            }

            // If we get here everything is good
            return ("SUCCESS", TransactionResponseCode.Success);
        }
        catch(TransactionValidationException tvex){
            return (tvex.Message, tvex.ResponseCode);
        }
    }

    public async Task<Result<TransactionValidationResult>> ValidateSaleTransactionX(Guid estateId,
                                                                                    Guid merchantId,
                                                                                    Guid contractId,
                                                                                    Guid productId,
                                                                                    String deviceIdentifier,
                                                                                    Guid operatorId,
                                                                                    Decimal? transactionAmount,
                                                                                    CancellationToken cancellationToken) {
        // Validate Estate
        Result<EstateResponse> getEstateResult = await this.GetEstate(estateId, cancellationToken);
        if (getEstateResult.IsFailed && getEstateResult.Status == ResultStatus.NotFound)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidEstateId, $"Estate Id [{estateId}] is not a valid estate"));
        }
        if (getEstateResult.IsFailed && getEstateResult.Status != ResultStatus.NotFound)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.UnknownFailure, $"An error occurred while getting Estate Id [{estateId}] Message: [{getEstateResult.Message}]"));
        }

        EstateResponse estate = getEstateResult.Data;

        // Operator Validation (Estate)
        if (estate.Operators == null || estate.Operators.Any() == false) {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.NoEstateOperators,
                $"Estate {estate.EstateName} has no operators defined"));
        }

        // Operators have been configured for the estate
        EstateOperatorResponse estateOperatorRecord = estate.Operators.SingleOrDefault(o => o.OperatorId == operatorId);
        if (estateOperatorRecord == null)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotValidForEstate,
                $"Operator {operatorId} not configured for Estate [{estate.EstateName}]"));
        }

        // Check the operator is enabled and not deleted
        if (estateOperatorRecord.IsDeleted)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotEnabledForEstate,
                $"Operator {operatorId} not enabled for Estate [{estate.EstateName}]"));
        }
        
        // Validate Merchant
        Result<MerchantResponse> getMerchantResult = await this.GetMerchant(estateId, merchantId, cancellationToken);
        if (getMerchantResult.IsFailed && getMerchantResult.Status == ResultStatus.NotFound)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidMerchantId,
                $"Merchant Id [{merchantId}] is not a valid merchant for estate [{estate.EstateName}]"));
        }

        if (getMerchantResult.IsFailed && getMerchantResult.Status != ResultStatus.NotFound)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.UnknownFailure,
                $"An error occurred while getting Merchant Id [{merchantId}] Message: [{getMerchantResult.Message}]"));
        }
        
        MerchantResponse merchant = getMerchantResult.Data;
        // Device Validation
        if (merchant.Devices == null || merchant.Devices.Any() == false)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.NoValidDevices,
                $"Merchant {merchant.MerchantName} has no valid Devices for this transaction."));
        }

        // Validate the device
        KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);

        if (device.Key == Guid.Empty)
        {
            // Device not found,throw error
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidDeviceIdentifier,
                $"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}"));
        }

        // Operator Validation (Merchant)
        if (merchant.Operators == null || merchant.Operators.Any() == false)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.NoMerchantOperators,
                $"Merchant {merchant.MerchantName} has no operators defined"));
        }

        // Operators have been configured for the merchant
        EstateManagement.DataTransferObjects.Responses.Merchant.MerchantOperatorResponse merchantOperatorRecord = merchant.Operators.SingleOrDefault(o => o.OperatorId == operatorId);
        if (merchantOperatorRecord == null)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotValidForMerchant,
                $"Operator {operatorId} not configured for Merchant [{merchant.MerchantName}]"));
        }

        // Check the operator is enabled and not deleted
        if (merchantOperatorRecord.IsDeleted)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.OperatorNotEnabledForMerchant,
                $"Operator {operatorId} not enabled for Merchant [{merchant.MerchantName}]"));
        }

        // Contract and Product Validation
        if (contractId == Guid.Empty) {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidContractIdValue,
                $"Contract Id [{contractId}] must be set for a sale transaction"));
        }
        Result<List<MerchantContractResponse>> getMerchantContractsResult = await this.GetMerchantContracts(estateId, merchantId, cancellationToken);

        if (getMerchantContractsResult.IsFailed && getMerchantContractsResult.Status == ResultStatus.NotFound)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidMerchantId, $"Merchant Id [{merchantId}] is not a valid merchant for estate [{estate.EstateName}]"));
        }
        if (getMerchantContractsResult.IsFailed && getMerchantContractsResult.Status != ResultStatus.NotFound)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.UnknownFailure, $"An error occurred while getting Merchant Id [{{merchantId}}] Contracts Message: [{getMerchantContractsResult.Message}]"));
        }

        List<MerchantContractResponse> merchantContracts = getMerchantContractsResult.Data;
        if (merchantContracts == null || merchantContracts.Any() == false)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.MerchantHasNoContractsConfigured, $"Merchant {merchant.MerchantName} has no contracts configured"));
        }

        // Check the contract and product id against the merchant
        EstateManagement.DataTransferObjects.Responses.Merchant.MerchantContractResponse contract = merchantContracts.SingleOrDefault(c => c.ContractId == contractId);

        if (contract == null)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.ContractNotValidForMerchant, $"Contract Id [{contractId}] not valid for Merchant [{merchant.MerchantName}]"));
        }

        if (productId == Guid.Empty)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidProductIdValue, $"Product Id [{productId}] must be set for a sale transaction"));
        }

        Guid contractProduct = contract.ContractProducts.SingleOrDefault(p => p == productId);

        if (contractProduct == Guid.Empty)
        {
            return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.ProductNotValidForMerchant, $"Product Id [{productId}] not valid for Merchant [{merchant.MerchantName}]"));
        }

        // Check the amount
        if (transactionAmount.HasValue)
        {
            if (transactionAmount <= 0)
            {
                return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.InvalidSaleTransactionAmount, "Transaction Amount must be greater than 0"));
            }

            Result<String> getBalanceResult = await this.EventStoreContext.GetPartitionStateFromProjection("MerchantBalanceProjection", $"MerchantBalance-{merchantId:N}", cancellationToken);
            if (getBalanceResult.IsFailed) {
                return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.UnknownFailure, $"Error getting balance for Merchant [{merchant.MerchantName}]"));
            }
            MerchantBalanceProjectionState1 projectionState = JsonConvert.DeserializeObject<MerchantBalanceProjectionState1>(getBalanceResult.Message);

            // Check the merchant has enough balance to perform the sale
            if (projectionState.merchant.balance < transactionAmount)
            {
                return CreateFailedResult(new TransactionValidationResult(TransactionResponseCode.MerchantDoesNotHaveEnoughCredit, $"Merchant [{merchant.MerchantName}] does not have enough credit available [{projectionState.merchant.balance:0.00}] to perform transaction amount [{transactionAmount}]"));
            }
        }

        // If we get here everything is good
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
    
    // TODO: refactor thit to a result as well
    private async Task<(EstateResponse estate, EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant)> ValidateMerchant(Guid estateId,
                                                                                                                                                    Guid merchantId,
                                                                                                                                                    CancellationToken cancellationToken){

        // Validate the Estate Record is a valid estate
        var getEstateResult = await this.GetEstate(estateId, cancellationToken);
        if (getEstateResult.IsFailed && getEstateResult.Status == ResultStatus.NotFound)
            throw new TransactionValidationException(getEstateResult.Message, TransactionResponseCode.InvalidEstateId);
        if (getEstateResult.IsFailed && getEstateResult.Status != ResultStatus.NotFound) {
            throw new TransactionValidationException(getEstateResult.Message, TransactionResponseCode.UnknownFailure);
        }

        // get the merchant record and validate the device
        var getMerchantResult = await this.GetMerchant(estateId, merchantId, cancellationToken);
        // TODO: shud we use the result message here...
        if (getMerchantResult.IsFailed && getMerchantResult.Status == ResultStatus.NotFound)
            throw new TransactionValidationException($"Merchant Id [{merchantId}] is not a valid merchant for estate [{getEstateResult.Data.EstateName}]",
                                                     TransactionResponseCode.InvalidMerchantId);
        if (getMerchantResult.IsFailed && getMerchantResult.Status != ResultStatus.NotFound)
        {
            throw new TransactionValidationException(getMerchantResult.Message, TransactionResponseCode.UnknownFailure);
        }

        return (getEstateResult.Data, getMerchantResult.Data);
    }

    #endregion
}