namespace TransactionProcessor.BusinessLogic.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EstateManagement.Client;
using EstateManagement.DataTransferObjects.Responses;
using ProjectionEngine.Repository;
using ProjectionEngine.State;
using SecurityService.Client;
using SecurityService.DataTransferObjects.Responses;
using Shared.General;
using Shared.Logger;

public class TransactionValidationService : ITransactionValidationService{
    #region Fields

    /// <summary>
    /// The estate client
    /// </summary>
    private readonly IEstateClient EstateClient;

    private readonly IProjectionStateRepository<MerchantBalanceState> MerchantBalanceStateRepository;

    /// <summary>
    /// The security service client
    /// </summary>
    private readonly ISecurityServiceClient SecurityServiceClient;

    private TokenResponse TokenResponse;

    #endregion

    public TransactionValidationService(IEstateClient estateClient,
                                        ISecurityServiceClient securityServiceClient,
                                        IProjectionStateRepository<MerchantBalanceState> merchantBalanceStateRepository){
        this.EstateClient = estateClient;
        this.SecurityServiceClient = securityServiceClient;
        this.MerchantBalanceStateRepository = merchantBalanceStateRepository;
    }

    #region Methods

    public async Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateLogonTransaction(Guid estateId,
                                                                                                               Guid merchantId,
                                                                                                               String deviceIdentifier,
                                                                                                               CancellationToken cancellationToken){
        try{
            (EstateResponse estate, MerchantResponse merchant) validateTransactionResponse = await this.ValidateMerchant(estateId, merchantId, cancellationToken);
            MerchantResponse merchant = validateTransactionResponse.merchant;

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
            (EstateResponse estate, MerchantResponse merchant) validateTransactionResponse = await this.ValidateMerchant(estateId, merchantId, cancellationToken);
            MerchantResponse merchant = validateTransactionResponse.merchant;

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
                                                                                                              String operatorIdentifier,
                                                                                                              Decimal? transactionAmount,
                                                                                                              CancellationToken cancellationToken){
        try{
            (EstateResponse estate, MerchantResponse merchant) validateTransactionResponse = await this.ValidateMerchant(estateId, merchantId, cancellationToken);
            EstateResponse estate = validateTransactionResponse.estate;
            MerchantResponse merchant = validateTransactionResponse.merchant;

            // Operator Validation (Estate)
            if (estate.Operators == null || estate.Operators.Any() == false){
                throw new TransactionValidationException($"Estate {estate.EstateName} has no operators defined", TransactionResponseCode.NoEstateOperators);
            }

            // Operators have been configured for the estate
            EstateOperatorResponse estateOperatorRecord = estate.Operators.SingleOrDefault(o => o.Name == operatorIdentifier);
            if (estateOperatorRecord == null){
                throw new TransactionValidationException($"Operator {operatorIdentifier} not configured for Estate [{estate.EstateName}]",
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
                MerchantOperatorResponse merchantOperatorRecord = merchant.Operators.SingleOrDefault(o => o.Name == operatorIdentifier);
                if (merchantOperatorRecord == null){
                    throw new TransactionValidationException($"Operator {operatorIdentifier} not configured for Merchant [{merchant.MerchantName}]",
                                                             TransactionResponseCode.OperatorNotValidForMerchant);
                }
            }

            // Contract and Product Validation
            if (contractId == Guid.Empty){
                throw new TransactionValidationException($"Contract Id [{contractId}] must be set for a sale transaction",
                                                         TransactionResponseCode.InvalidContractIdValue);
            }
            
            List<ContractResponse> merchantContracts = null;
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
            ContractResponse contract = merchantContracts.SingleOrDefault(c => c.ContractId == contractId);

            if (contract == null){
                throw new TransactionValidationException($"Contract Id [{contractId}] not valid for Merchant [{merchant.MerchantName}]",
                                                         TransactionResponseCode.ContractNotValidForMerchant);
            }

            if (productId == Guid.Empty){
                throw new TransactionValidationException($"Product Id [{productId}] must be set for a sale transaction",
                                                         TransactionResponseCode.InvalidProductIdValue);
            }

            ContractProduct contractProduct = contract.Products.SingleOrDefault(p => p.ProductId == productId);

            if (contractProduct == null){
                throw new TransactionValidationException($"Product Id [{productId}] not valid for Merchant [{merchant.MerchantName}]",
                                                         TransactionResponseCode.ProductNotValidForMerchant);
            }

            // Check the amount
            if (transactionAmount.HasValue){
                if (transactionAmount <= 0){
                    throw new TransactionValidationException("Transaction Amount must be greater than 0", TransactionResponseCode.InvalidSaleTransactionAmount);
                }

                MerchantBalanceState merchantBalanceState = await this.MerchantBalanceStateRepository.Load(estateId, merchantId, cancellationToken);

                // Check the merchant has enough balance to perform the sale
                if (merchantBalanceState.AvailableBalance < transactionAmount){
                    throw new
                        TransactionValidationException($"Merchant [{merchant.MerchantName}] does not have enough credit available [{merchantBalanceState.AvailableBalance}] to perform transaction amount [{transactionAmount}]",
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
        this.TokenResponse = await this.GetToken(cancellationToken);

        EstateResponse estate = await this.EstateClient.GetEstate(this.TokenResponse.AccessToken, estateId, cancellationToken);

        return estate;
    }

    private async Task<MerchantResponse> GetMerchant(Guid estateId,
                                                     Guid merchantId,
                                                     CancellationToken cancellationToken){
        this.TokenResponse = await this.GetToken(cancellationToken);

        MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);

        return merchant;
    }

    private async Task<List<ContractResponse>> GetMerchantContracts(Guid estateId,
                                                                    Guid merchantId,
                                                                    CancellationToken cancellationToken){
        this.TokenResponse = await this.GetToken(cancellationToken);

        List<ContractResponse> merchantContracts = await this.EstateClient.GetMerchantContracts(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);

        return merchantContracts;
    }

    [ExcludeFromCodeCoverage]
    private async Task<TokenResponse> GetToken(CancellationToken cancellationToken){
        // Get a token to talk to the estate service
        String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
        String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");
        Logger.LogInformation($"Client Id is {clientId}");
        Logger.LogInformation($"Client Secret is {clientSecret}");

        if (this.TokenResponse == null){
            TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
            Logger.LogInformation($"Token is {token.AccessToken}");
            return token;
        }

        if (this.TokenResponse.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2)){
            Logger.LogInformation($"Token is about to expire at {this.TokenResponse.Expires.DateTime:O}");
            TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
            Logger.LogInformation($"Token is {token.AccessToken}");
            return token;
        }

        return this.TokenResponse;
    }

    private async Task<(EstateResponse estate, MerchantResponse merchant)> ValidateMerchant(Guid estateId,
                                                                                            Guid merchantId,
                                                                                            CancellationToken cancellationToken){
        EstateResponse estate = null;
        MerchantResponse merchant = null;

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