namespace TransactionProcessor.BusinessLogic.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using EstateManagement.Client;
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

    private async Task<EstateResponse> GetEstate(Guid estateId,
                                                 CancellationToken cancellationToken){
        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        EstateResponse estate = await this.EstateClient.GetEstate(this.TokenResponse.AccessToken, estateId, cancellationToken);

        return estate;
    }

    private async Task<EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse> GetMerchant(Guid estateId,
                                                                                                             Guid merchantId,
                                                                                                             CancellationToken cancellationToken){
        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);
        
        return merchant;
    }

    private async Task<List<EstateManagement.DataTransferObjects.Responses.Merchant.MerchantContractResponse>> GetMerchantContracts(Guid estateId,
                                                                                                                                    Guid merchantId,
                                                                                                                                    CancellationToken cancellationToken)
    {
        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);

        return merchant.Contracts;
    }
    
    private async Task<(EstateResponse estate, EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant)> ValidateMerchant(Guid estateId,
                                                                                                                                                    Guid merchantId,
                                                                                                                                                    CancellationToken cancellationToken){
        EstateResponse estate = null;
        EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant = null;

        // Validate the Estate Record is a valid estate
        try{
            estate = await this.GetEstate(estateId, cancellationToken);
        }
        catch(Exception ex) when(ex.InnerException != null && ex.InnerException.GetType() == typeof(KeyNotFoundException)){
            throw new TransactionValidationException($"Estate Id [{estateId}] is not a valid estate", TransactionResponseCode.InvalidEstateId);
        }
        catch (Exception e){
            throw new TransactionValidationException($"Exception occurred while getting Estate Id [{estateId}] Exception [{e.Message}]", TransactionResponseCode.UnknownFailure);
        }

        // get the merchant record and validate the device
        try{
            merchant = await this.GetMerchant(estateId, merchantId, cancellationToken);
        }
        catch(Exception ex) when(ex.InnerException != null && ex.InnerException.GetType() == typeof(KeyNotFoundException)){
            throw new TransactionValidationException($"Merchant Id [{merchantId}] is not a valid merchant for estate [{estate.EstateName}]",
                                                     TransactionResponseCode.InvalidMerchantId);
        }
        catch(Exception e){
            throw new TransactionValidationException($"Exception occurred while getting Merchant Id [{merchantId}] Exception [{e.Message}]", TransactionResponseCode.UnknownFailure);
        }

        return (estate, merchant);
    }

    #endregion
}