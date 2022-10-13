namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Requests;
    using EstateManagement.DataTransferObjects.Responses;
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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.BusinessLogic.Services.ITransactionDomainService" />
    public class TransactionDomainService : ITransactionDomainService
    {
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

        /// <summary>
        /// The security service client
        /// </summary>
        private readonly ISecurityServiceClient SecurityServiceClient;

        /// <summary>
        /// The token response
        /// </summary>
        private TokenResponse TokenResponse;

        /// <summary>
        /// The transaction aggregate manager
        /// </summary>
        private readonly ITransactionAggregateManager TransactionAggregateManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDomainService" /> class.
        /// </summary>
        /// <param name="transactionAggregateManager">The transaction aggregate manager.</param>
        /// <param name="estateClient">The estate client.</param>
        /// <param name="securityServiceClient">The security service client.</param>
        /// <param name="operatorProxyResolver">The operator proxy resolver.</param>
        /// <param name="reconciliationAggregateRepository">The reconciliation aggregate repository.</param>
        public TransactionDomainService(ITransactionAggregateManager transactionAggregateManager,
                                        IEstateClient estateClient,
                                        ISecurityServiceClient securityServiceClient,
                                        Func<String, IOperatorProxy> operatorProxyResolver,
                                        IAggregateRepository<ReconciliationAggregate, DomainEvent> reconciliationAggregateRepository) {
            this.TransactionAggregateManager = transactionAggregateManager;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
            this.OperatorProxyResolver = operatorProxyResolver;
            this.ReconciliationAggregateRepository = reconciliationAggregateRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes the logon transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<ProcessLogonTransactionResponse> ProcessLogonTransaction(Guid transactionId,
                                                                                   Guid estateId,
                                                                                   Guid merchantId,
                                                                                   DateTime transactionDateTime,
                                                                                   String transactionNumber,
                                                                                   String deviceIdentifier,
                                                                                   CancellationToken cancellationToken) {
            TransactionType transactionType = TransactionType.Logon;

            // Generate a transaction reference
            String transactionReference = this.GenerateTransactionReference();

            await this.TransactionAggregateManager.StartTransaction(transactionId,
                                                                    transactionDateTime,
                                                                    transactionNumber,
                                                                    transactionType,
                                                                    transactionReference,
                                                                    estateId,
                                                                    merchantId,
                                                                    deviceIdentifier,
                                                                    null, // Logon transaction has no amount
                                                                    cancellationToken);

            (String responseMessage, TransactionResponseCode responseCode) validationResult =
                await this.ValidateLogonTransaction(estateId, merchantId, deviceIdentifier, cancellationToken);

            if (validationResult.responseCode == TransactionResponseCode.Success) {
                // Record the successful validation
                // TODO: Generate local authcode
                await this.TransactionAggregateManager.AuthoriseTransactionLocally(estateId, transactionId, "ABCD1234", validationResult, cancellationToken);
            }
            else {
                // Record the failure
                await this.TransactionAggregateManager.DeclineTransactionLocally(estateId, transactionId, validationResult, cancellationToken);
            }

            await this.TransactionAggregateManager.CompleteTransaction(estateId, transactionId, cancellationToken);

            TransactionAggregate transactionAggregate = await this.TransactionAggregateManager.GetAggregate(estateId, transactionId, cancellationToken);

            return new ProcessLogonTransactionResponse {
                                                           ResponseMessage = transactionAggregate.ResponseMessage,
                                                           ResponseCode = transactionAggregate.ResponseCode,
                                                           EstateId = estateId,
                                                           MerchantId = merchantId,
                                                           TransactionId = transactionId
                                                       };
        }

        /// <summary>
        /// Processes the reconciliation transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionCount">The transaction count.</param>
        /// <param name="transactionValue">The transaction value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<ProcessReconciliationTransactionResponse> ProcessReconciliationTransaction(Guid transactionId,
                                                                                                     Guid estateId,
                                                                                                     Guid merchantId,
                                                                                                     String deviceIdentifier,
                                                                                                     DateTime transactionDateTime,
                                                                                                     Int32 transactionCount,
                                                                                                     Decimal transactionValue,
                                                                                                     CancellationToken cancellationToken) {
            (String responseMessage, TransactionResponseCode responseCode) validationResult =
                await this.ValidateReconciliationTransaction(estateId, merchantId, deviceIdentifier, cancellationToken);

            ReconciliationAggregate reconciliationAggregate = await this.ReconciliationAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            reconciliationAggregate.StartReconciliation(transactionDateTime, estateId, merchantId);

            reconciliationAggregate.RecordOverallTotals(transactionCount, transactionValue);

            if (validationResult.responseCode == TransactionResponseCode.Success) {
                // Record the successful validation
                reconciliationAggregate.Authorise(((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'), validationResult.responseMessage);
            }
            else {
                // Record the failure
                reconciliationAggregate.Decline(((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'), validationResult.responseMessage);
            }

            reconciliationAggregate.CompleteReconciliation();

            await this.ReconciliationAggregateRepository.SaveChanges(reconciliationAggregate, cancellationToken);

            return new ProcessReconciliationTransactionResponse {
                                                                    EstateId = reconciliationAggregate.EstateId,
                                                                    MerchantId = reconciliationAggregate.MerchantId,
                                                                    ResponseCode = reconciliationAggregate.ResponseCode,
                                                                    ResponseMessage = reconciliationAggregate.ResponseMessage,
                                                                    TransactionId = transactionId
                                                                };
        }

        /// <summary>
        /// Processes the sale transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="customerEmailAddress">The customer email address.</param>
        /// <param name="additionalTransactionMetadata">The additional transaction metadata.</param>
        /// <param name="contractId">The contract identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<ProcessSaleTransactionResponse> ProcessSaleTransaction(Guid transactionId,
                                                                                 Guid estateId,
                                                                                 Guid merchantId,
                                                                                 DateTime transactionDateTime,
                                                                                 String transactionNumber,
                                                                                 String deviceIdentifier,
                                                                                 String operatorIdentifier,
                                                                                 String customerEmailAddress,
                                                                                 Dictionary<String, String> additionalTransactionMetadata,
                                                                                 Guid contractId,
                                                                                 Guid productId,
                                                                                 Int32 transactionSource,
                                                                                 CancellationToken cancellationToken) {
            TransactionType transactionType = TransactionType.Sale;
            TransactionSource transactionSourceValue = (TransactionSource)transactionSource;

            // Generate a transaction reference
            String transactionReference = this.GenerateTransactionReference();

            // Extract the transaction amount from the metadata
            Decimal? transactionAmount = additionalTransactionMetadata.ExtractFieldFromMetadata<Decimal?>("Amount");

            (String responseMessage, TransactionResponseCode responseCode) validationResult =
                await this.ValidateSaleTransaction(estateId, merchantId,contractId, productId, deviceIdentifier, operatorIdentifier, transactionAmount, cancellationToken);

            Logger.LogInformation($"Validation response is [{validationResult.responseCode}]");

            await this.TransactionAggregateManager.StartTransaction(transactionId,
                                                                    transactionDateTime,
                                                                    transactionNumber,
                                                                    transactionType,
                                                                    transactionReference,
                                                                    estateId,
                                                                    merchantId,
                                                                    deviceIdentifier,
                                                                    transactionAmount,
                                                                    cancellationToken);

                // Add the product details (unless invalid estate)
                if (validationResult.responseCode != TransactionResponseCode.InvalidEstateId &&
                    validationResult.responseCode != TransactionResponseCode.InvalidContractIdValue &&
                    validationResult.responseCode != TransactionResponseCode.InvalidProductIdValue &&
                    validationResult.responseCode != TransactionResponseCode.ContractNotValidForMerchant &&
                    validationResult.responseCode != TransactionResponseCode.ProductNotValidForMerchant)
                {
                    await this.TransactionAggregateManager.AddProductDetails(estateId, transactionId, contractId, productId, cancellationToken);
                }

                // Add the transaction source
                await this.TransactionAggregateManager.AddTransactionSource(estateId, transactionId, transactionSourceValue, cancellationToken);

                if (validationResult.responseCode == TransactionResponseCode.Success)
                {
                    // Record any additional request metadata
                    await this.TransactionAggregateManager.RecordAdditionalRequestData(estateId,
                                                                                       transactionId,
                                                                                       operatorIdentifier,
                                                                                       additionalTransactionMetadata,
                                                                                       cancellationToken);

                    // Do the online processing with the operator here
                    MerchantResponse merchant = await this.GetMerchant(estateId, merchantId, cancellationToken);
                    OperatorResponse operatorResponse = await this.ProcessMessageWithOperator(merchant,
                                                                                              transactionId,
                                                                                              transactionDateTime,
                                                                                              operatorIdentifier,
                                                                                              additionalTransactionMetadata,
                                                                                              transactionReference,
                                                                                              cancellationToken);

                    // Act on the operator response
                    if (operatorResponse == null)
                    {
                        // Failed to perform sed/receive with the operator
                        TransactionResponseCode transactionResponseCode = TransactionResponseCode.OperatorCommsError;
                        String responseMessage = "OPERATOR COMMS ERROR";

                        await this.TransactionAggregateManager.DeclineTransactionLocally(estateId,
                                                                                         transactionId,
                                                                                         (responseMessage, transactionResponseCode),
                                                                                         cancellationToken);
                    }
                    else
                    {
                        if (operatorResponse.IsSuccessful)
                        {
                            TransactionResponseCode transactionResponseCode = TransactionResponseCode.Success;
                            String responseMessage = "SUCCESS";

                            await this.TransactionAggregateManager.AuthoriseTransaction(estateId,
                                                                                        transactionId,
                                                                                        operatorIdentifier,
                                                                                        operatorResponse,
                                                                                        transactionResponseCode,
                                                                                        responseMessage,
                                                                                        cancellationToken);
                        }
                        else
                        {
                            TransactionResponseCode transactionResponseCode = TransactionResponseCode.TransactionDeclinedByOperator;
                            String responseMessage = "DECLINED BY OPERATOR";

                            await this.TransactionAggregateManager.DeclineTransaction(estateId,
                                                                                      transactionId,
                                                                                      operatorIdentifier,
                                                                                      operatorResponse,
                                                                                      transactionResponseCode,
                                                                                      responseMessage,
                                                                                      cancellationToken);
                        }

                        // Record any additional operator response metadata
                        await this.TransactionAggregateManager.RecordAdditionalResponseData(estateId,
                                                                                            transactionId,
                                                                                            operatorIdentifier,
                                                                                            operatorResponse.AdditionalTransactionResponseMetadata,
                                                                                            cancellationToken);
                    }
                }
                else
                {
                    // Record the failure
                    await this.TransactionAggregateManager.DeclineTransactionLocally(estateId, transactionId, validationResult, cancellationToken);
                }

                await this.TransactionAggregateManager.CompleteTransaction(estateId, transactionId, cancellationToken);

                // Determine if the email receipt is required
                if (String.IsNullOrEmpty(customerEmailAddress) == false)
                {
                    await this.TransactionAggregateManager.RequestEmailReceipt(estateId, transactionId, customerEmailAddress, cancellationToken);
                }

                TransactionAggregate transactionAggregate = await this.TransactionAggregateManager.GetAggregate(estateId, transactionId, cancellationToken);

            // Get the model from the aggregate
            Transaction transaction = transactionAggregate.GetTransaction();

            return new ProcessSaleTransactionResponse {
                                                          ResponseMessage = transaction.ResponseMessage,
                                                          ResponseCode = transaction.ResponseCode,
                                                          EstateId = estateId,
                                                          MerchantId = merchantId,
                                                          AdditionalTransactionMetadata = transaction.AdditionalResponseMetadata,
                                                          TransactionId = transactionId
                                                      };
        }

        public async Task ResendTransactionReceipt(Guid transactionId,
                                                   Guid estateId,
                                                   CancellationToken cancellationToken) {
            await this.TransactionAggregateManager.ResendReceipt(estateId, transactionId, cancellationToken);
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
                                               CancellationToken cancellationToken) {
            this.TokenResponse = await this.GetToken(cancellationToken);

            // Add the device to the merchant
            await this.EstateClient.AddDeviceToMerchant(this.TokenResponse.AccessToken,
                                                        estateId,
                                                        merchantId,
                                                        new AddMerchantDeviceRequest {
                                                                                         DeviceIdentifier = deviceIdentifier
                                                                                     },
                                                        cancellationToken);
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

        /// <summary>
        /// Gets the estate.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<EstateResponse> GetEstate(Guid estateId,
                                                     CancellationToken cancellationToken) {
            this.TokenResponse = await this.GetToken(cancellationToken);

            EstateResponse estate = await this.EstateClient.GetEstate(this.TokenResponse.AccessToken, estateId, cancellationToken);

            return estate;
        }

        /// <summary>
        /// Gets the merchant.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<MerchantResponse> GetMerchant(Guid estateId,
                                                         Guid merchantId,
                                                         CancellationToken cancellationToken) {
            this.TokenResponse = await this.GetToken(cancellationToken);

            MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);

            return merchant;
        }

        private async Task<List<ContractResponse>> GetMerchantContracts(Guid estateId,
                                                         Guid merchantId,
                                                         CancellationToken cancellationToken)
        {
            this.TokenResponse = await this.GetToken(cancellationToken);

            List<ContractResponse> merchantContracts = await this.EstateClient.GetMerchantContracts(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);

            return merchantContracts;
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<TokenResponse> GetToken(CancellationToken cancellationToken) {
            // Get a token to talk to the estate service
            String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
            String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");
            Logger.LogInformation($"Client Id is {clientId}");
            Logger.LogInformation($"Client Secret is {clientSecret}");

            if (this.TokenResponse == null) {
                TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                Logger.LogInformation($"Token is {token.AccessToken}");
                return token;
            }

            if (this.TokenResponse.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2)) {
                Logger.LogInformation($"Token is about to expire at {this.TokenResponse.Expires.DateTime:O}");
                TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                Logger.LogInformation($"Token is {token.AccessToken}");
                return token;
            }

            return this.TokenResponse;
        }

        private async Task<OperatorResponse> ProcessMessageWithOperator(MerchantResponse merchant,
                                                                        Guid transactionId,
                                                                        DateTime transactionDateTime,
                                                                        String operatorIdentifier,
                                                                        Dictionary<String, String> additionalTransactionMetadata,
                                                                        String transactionReference,
                                                                        CancellationToken cancellationToken) {
            IOperatorProxy operatorProxy = this.OperatorProxyResolver(operatorIdentifier.Replace(" ", ""));
            OperatorResponse operatorResponse = null;
            try {
                operatorResponse = await operatorProxy.ProcessSaleMessage(this.TokenResponse.AccessToken,
                                                                          transactionId,
                                                                          operatorIdentifier,
                                                                          merchant,
                                                                          transactionDateTime,
                                                                          transactionReference,
                                                                          additionalTransactionMetadata,
                                                                          cancellationToken);
            }
            catch(Exception e) {
                // Log out the error
                Logger.LogError(e);
            }

            return operatorResponse;
        }

        /// <summary>
        /// Validates the transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="TransactionValidationException">Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}</exception>
        /// <exception cref="TransactionProcessor.BusinessLogic.Services.TransactionValidationException">Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}</exception>
        private async Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateLogonTransaction(Guid estateId,
            Guid merchantId,
            String deviceIdentifier,
            CancellationToken cancellationToken) {
            try {
                (EstateResponse estate, MerchantResponse merchant, List<ContractResponse> merchantContracts) validateTransactionResponse = await this.ValidateTransaction(estateId, merchantId, cancellationToken);
                MerchantResponse merchant = validateTransactionResponse.merchant;

                // Device Validation
                if (merchant.Devices == null || merchant.Devices.Any() == false) {
                    await this.AddDeviceToMerchant(estateId, merchantId, deviceIdentifier, cancellationToken);
                }
                else {
                    // Validate the device
                    KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);

                    if (device.Key == Guid.Empty) {
                        // Device not found,throw error
                        throw new TransactionValidationException($"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}",
                                                                 TransactionResponseCode.InvalidDeviceIdentifier);
                    }
                }

                // If we get here everything is good
                return ("SUCCESS", TransactionResponseCode.Success);
            }
            catch(TransactionValidationException tvex) {
                return (tvex.Message, tvex.ResponseCode);
            }
        }

        /// <summary>
        /// Validates the reconciliation transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="TransactionValidationException">Merchant {merchant.MerchantName} has no valid Devices for this transaction.
        /// or
        /// Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}</exception>
        private async Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateReconciliationTransaction(Guid estateId,
            Guid merchantId,
            String deviceIdentifier,
            CancellationToken cancellationToken) {
            try {
                (EstateResponse estate, MerchantResponse merchant, List<ContractResponse> merchantContracts) validateTransactionResponse = await this.ValidateTransaction(estateId, merchantId, cancellationToken);
                MerchantResponse merchant = validateTransactionResponse.merchant;

                // Device Validation
                if (merchant.Devices == null || merchant.Devices.Any() == false) {
                    throw new TransactionValidationException($"Merchant {merchant.MerchantName} has no valid Devices for this transaction.",
                                                             TransactionResponseCode.NoValidDevices);
                }

                // Validate the device
                KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);

                if (device.Key == Guid.Empty) {
                    // Device not found,throw error
                    throw new TransactionValidationException($"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}",
                                                             TransactionResponseCode.InvalidDeviceIdentifier);
                }

                // If we get here everything is good
                return ("SUCCESS", TransactionResponseCode.Success);
            }
            catch(TransactionValidationException tvex) {
                return (tvex.Message, tvex.ResponseCode);
            }
        }

        /// <summary>
        /// Validates the sale transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="transactionAmount">The transaction amount.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="TransactionValidationException">Merchant {merchant.MerchantName} has no valid Devices for this transaction.
        /// or
        /// Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}
        /// or
        /// Estate {estate.EstateName} has no operators defined
        /// or
        /// Operator {operatorIdentifier} not configured for Estate [{estate.EstateName}]
        /// or
        /// Merchant {merchant.MerchantName} has no operators defined
        /// or
        /// Operator {operatorIdentifier} not configured for Merchant [{merchant.MerchantName}]</exception>
        private async Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateSaleTransaction(Guid estateId,
            Guid merchantId,
            Guid contractId,
            Guid productId,
            String deviceIdentifier,
            String operatorIdentifier,
            Decimal? transactionAmount,
            CancellationToken cancellationToken) {
            try {
                (EstateResponse estate, MerchantResponse merchant, List<ContractResponse> merchantContracts) validateTransactionResponse = await this.ValidateTransaction(estateId, merchantId, cancellationToken);
                EstateResponse estate = validateTransactionResponse.estate;
                MerchantResponse merchant = validateTransactionResponse.merchant;
                List<ContractResponse> contracts = validateTransactionResponse.merchantContracts;

                // Device Validation
                if (merchant.Devices == null || merchant.Devices.Any() == false) {
                    throw new TransactionValidationException($"Merchant {merchant.MerchantName} has no valid Devices for this transaction.",
                                                             TransactionResponseCode.NoValidDevices);
                }

                // Validate the device
                KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);

                if (device.Key == Guid.Empty) {
                    // Device not found,throw error
                    throw new TransactionValidationException($"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}",
                                                             TransactionResponseCode.InvalidDeviceIdentifier);
                }

                // Operator Validation (Estate)
                if (estate.Operators == null || estate.Operators.Any() == false) {
                    throw new TransactionValidationException($"Estate {estate.EstateName} has no operators defined", TransactionResponseCode.NoEstateOperators);
                }

                {
                    // Operators have been configured for the estate
                    EstateOperatorResponse operatorRecord = estate.Operators.SingleOrDefault(o => o.Name == operatorIdentifier);
                    if (operatorRecord == null) {
                        throw new TransactionValidationException($"Operator {operatorIdentifier} not configured for Estate [{estate.EstateName}]",
                                                                 TransactionResponseCode.OperatorNotValidForEstate);
                    }
                }

                // Operator Validation (Merchant)
                if (merchant.Operators == null || merchant.Operators.Any() == false) {
                    throw new TransactionValidationException($"Merchant {merchant.MerchantName} has no operators defined", TransactionResponseCode.NoEstateOperators);
                }

                {
                    // Operators have been configured for the estate
                    MerchantOperatorResponse operatorRecord = merchant.Operators.SingleOrDefault(o => o.Name == operatorIdentifier);
                    if (operatorRecord == null) {
                        throw new TransactionValidationException($"Operator {operatorIdentifier} not configured for Merchant [{merchant.MerchantName}]",
                                                                 TransactionResponseCode.OperatorNotValidForMerchant);
                    }
                }

                // Check the amount
                if (transactionAmount.HasValue) {
                    if (transactionAmount <= 0) {
                        throw new TransactionValidationException("Transaction Amount must be greater than 0", TransactionResponseCode.InvalidSaleTransactionAmount);
                    }

                    // Check the merchant has enough balance to perform the sale
                    if (merchant.AvailableBalance < transactionAmount) {
                        throw new
                            TransactionValidationException($"Merchant [{merchant.MerchantName}] does not have enough credit available [{merchant.AvailableBalance}] to perform transaction amount [{transactionAmount}]",
                                                           TransactionResponseCode.MerchantDoesNotHaveEnoughCredit);
                    }
                }

                // Contract and Product Validation
                if (contractId == Guid.Empty)
                {
                    throw new TransactionValidationException($"Contract Id [{contractId}] must be set for a sale transaction",
                        TransactionResponseCode.InvalidContractIdValue);
                }

                // Check the contract and product id against the merchant
                ContractResponse contract = contracts.SingleOrDefault(c => c.ContractId == contractId);

                if (contract == null)
                {
                    throw new TransactionValidationException($"Contract Id [{contractId}] not valid for Merchant [{merchant.MerchantName}]",
                                                             TransactionResponseCode.ContractNotValidForMerchant);
                }

                if (productId == Guid.Empty)
                {
                    throw new TransactionValidationException($"Product Id [{productId}] must be set for a sale transaction",
                        TransactionResponseCode.InvalidProductIdValue);
                }
                
                ContractProduct contractProduct = contract.Products.SingleOrDefault(p => p.ProductId == productId);

                if (contractProduct == null)
                {
                    throw new TransactionValidationException($"Product Id [{productId}] not valid for Merchant [{merchant.MerchantName}]",
                                                             TransactionResponseCode.ProductNotValidForMerchant);
                }

                // If we get here everything is good
                return ("SUCCESS", TransactionResponseCode.Success);
            }
            catch(TransactionValidationException tvex) {
                return (tvex.Message, tvex.ResponseCode);
            }
        }

        /// <summary>
        /// Validates the transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="TransactionValidationException">Estate Id [{estateId}] is not a valid estate
        /// or
        /// Merchant Id [{merchantId}] is not a valid merchant for estate [{estate.EstateName}]</exception>
        private async Task<(EstateResponse estate, MerchantResponse merchant, List<ContractResponse> merchantContracts)> ValidateTransaction(Guid estateId,
                                                                                                   Guid merchantId,
                                                                                                   CancellationToken cancellationToken) {
            EstateResponse estate = null;
            MerchantResponse merchant = null;
            List<ContractResponse> merchantContracts = null;

            // Validate the Estate Record is a valid estate
            try {
                estate = await this.GetEstate(estateId, cancellationToken);
            }
            catch(Exception ex) when(ex.InnerException != null && ex.InnerException.GetType() == typeof(KeyNotFoundException)) {
                throw new TransactionValidationException($"Estate Id [{estateId}] is not a valid estate", TransactionResponseCode.InvalidEstateId);
            }

            // get the merchant record and validate the device
            try {
                merchant = await this.GetMerchant(estateId, merchantId, cancellationToken);
            }
            catch(Exception ex) when(ex.InnerException != null && ex.InnerException.GetType() == typeof(KeyNotFoundException)) {
                throw new TransactionValidationException($"Merchant Id [{merchantId}] is not a valid merchant for estate [{estate.EstateName}]",
                                                         TransactionResponseCode.InvalidMerchantId);
            }

            try
            {
                merchantContracts = await this.GetMerchantContracts(estateId, merchantId, cancellationToken);
            }
            catch (Exception ex) when (ex.InnerException != null && ex.InnerException.GetType() == typeof(KeyNotFoundException))
            {
                //throw new TransactionValidationException($"Merchant Id [{merchantId}] is not a valid merchant for estate [{estate.EstateName}]",
                //                                         TransactionResponseCode.InvalidMerchantId);
            }

            return (estate, merchant, merchantContracts);
        }

        #endregion
    }
}