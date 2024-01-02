namespace TransactionProcessor.BusinessLogic.Services{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Requests;
    using EstateManagement.DataTransferObjects.Responses;
    using FloatAggregate;
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
                                        IAggregateRepository<FloatAggregate, DomainEvent> floatAggregateRepository){
            this.TransactionAggregateRepository = transactionAggregateRepository;
            this.EstateClient = estateClient;
            this.OperatorProxyResolver = operatorProxyResolver;
            this.ReconciliationAggregateRepository = reconciliationAggregateRepository;
            this.TransactionValidationService = transactionValidationService;
            this.SecurityServiceClient = securityServiceClient;
            this.FloatAggregateRepository = floatAggregateRepository;
        }

        #endregion

        #region Methods

        public async Task<ProcessLogonTransactionResponse> ProcessLogonTransaction(Guid transactionId,
                                                                                   Guid estateId,
                                                                                   Guid merchantId,
                                                                                   DateTime transactionDateTime,
                                                                                   String transactionNumber,
                                                                                   String deviceIdentifier,
                                                                                   CancellationToken cancellationToken){
            TransactionType transactionType = TransactionType.Logon;

            // Generate a transaction reference
            String transactionReference = this.GenerateTransactionReference();

            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.StartTransaction(transactionDateTime,
                                                  transactionNumber,
                                                  transactionType,
                                                  transactionReference,
                                                  estateId,
                                                  merchantId,
                                                  deviceIdentifier,
                                                  null); // Logon transaction has no amount

            (String responseMessage, TransactionResponseCode responseCode) validationResult =
                await this.TransactionValidationService.ValidateLogonTransaction(estateId, merchantId, deviceIdentifier, cancellationToken);

            if (validationResult.responseCode == TransactionResponseCode.Success ||
                validationResult.responseCode == TransactionResponseCode.SuccessNeedToAddDevice)
            {
                if (validationResult.responseCode == TransactionResponseCode.SuccessNeedToAddDevice)
                {
                    await this.AddDeviceToMerchant(estateId, merchantId, deviceIdentifier, cancellationToken);
                }

                // Record the successful validation
                // TODO: Generate local authcode
                transactionAggregate.AuthoriseTransactionLocally("ABCD1234",
                                                                 ((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'),
                                                                 validationResult.responseMessage);
            }
            else{
                // Record the failure
                transactionAggregate.DeclineTransactionLocally(((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'),
                                                               validationResult.responseMessage);
            }

            transactionAggregate.CompleteTransaction();

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);

            return new ProcessLogonTransactionResponse{
                                                          ResponseMessage = transactionAggregate.ResponseMessage,
                                                          ResponseCode = transactionAggregate.ResponseCode,
                                                          EstateId = estateId,
                                                          MerchantId = merchantId,
                                                          TransactionId = transactionId
                                                      };
        }

        public async Task<ProcessReconciliationTransactionResponse> ProcessReconciliationTransaction(Guid transactionId,
                                                                                                     Guid estateId,
                                                                                                     Guid merchantId,
                                                                                                     String deviceIdentifier,
                                                                                                     DateTime transactionDateTime,
                                                                                                     Int32 transactionCount,
                                                                                                     Decimal transactionValue,
                                                                                                     CancellationToken cancellationToken){
            (String responseMessage, TransactionResponseCode responseCode) validationResult =
                await this.TransactionValidationService.ValidateReconciliationTransaction(estateId, merchantId, deviceIdentifier, cancellationToken);

            ReconciliationAggregate reconciliationAggregate = await this.ReconciliationAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            reconciliationAggregate.StartReconciliation(transactionDateTime, estateId, merchantId);

            reconciliationAggregate.RecordOverallTotals(transactionCount, transactionValue);

            if (validationResult.responseCode == TransactionResponseCode.Success){
                // Record the successful validation
                reconciliationAggregate.Authorise(((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'), validationResult.responseMessage);
            }
            else{
                // Record the failure
                reconciliationAggregate.Decline(((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'), validationResult.responseMessage);
            }

            reconciliationAggregate.CompleteReconciliation();

            await this.ReconciliationAggregateRepository.SaveChanges(reconciliationAggregate, cancellationToken);

            return new ProcessReconciliationTransactionResponse{
                                                                   EstateId = reconciliationAggregate.EstateId,
                                                                   MerchantId = reconciliationAggregate.MerchantId,
                                                                   ResponseCode = reconciliationAggregate.ResponseCode,
                                                                   ResponseMessage = reconciliationAggregate.ResponseMessage,
                                                                   TransactionId = transactionId
                                                               };
        }

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
                                                                                 CancellationToken cancellationToken){
            TransactionType transactionType = TransactionType.Sale;
            TransactionSource transactionSourceValue = (TransactionSource)transactionSource;

            // Generate a transaction reference
            String transactionReference = this.GenerateTransactionReference();

            // Extract the transaction amount from the metadata
            Decimal? transactionAmount = additionalTransactionMetadata.ExtractFieldFromMetadata<Decimal?>("Amount");

            (String responseMessage, TransactionResponseCode responseCode) validationResult =
                await this.TransactionValidationService.ValidateSaleTransaction(estateId,
                                                                                merchantId,
                                                                                contractId,
                                                                                productId,
                                                                                deviceIdentifier,
                                                                                operatorIdentifier,
                                                                                transactionAmount,
                                                                                cancellationToken);

            Logger.LogInformation($"Validation response is [{validationResult.responseCode}]");

            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            Guid floatAggregateId = IdGenerationService.GenerateFloatAggregateId(estateId, contractId, productId);
            FloatAggregate floatAggregate= await this.FloatAggregateRepository.GetLatestVersion(floatAggregateId, cancellationToken);
            
            // TODO: Move calculation to float
            Decimal unitCost = floatAggregate.GetUnitCostPrice();
            Decimal totalCost = transactionAmount.GetValueOrDefault() * unitCost;

            transactionAggregate.StartTransaction(transactionDateTime,
                                                  transactionNumber,
                                                  transactionType,
                                                  transactionReference,
                                                  estateId,
                                                  merchantId,
                                                  deviceIdentifier,
                                                  transactionAmount);

            // Add the product details (unless invalid estate)
            if (validationResult.responseCode != TransactionResponseCode.InvalidEstateId &&
                validationResult.responseCode != TransactionResponseCode.InvalidContractIdValue &&
                validationResult.responseCode != TransactionResponseCode.InvalidProductIdValue &&
                validationResult.responseCode != TransactionResponseCode.ContractNotValidForMerchant &&
                validationResult.responseCode != TransactionResponseCode.ProductNotValidForMerchant){
                transactionAggregate.AddProductDetails(contractId, productId);
            }

            transactionAggregate.RecordCostPrice(unitCost, totalCost);

            // Add the transaction source
            transactionAggregate.AddTransactionSource(transactionSourceValue);

            if (validationResult.responseCode == TransactionResponseCode.Success){
                // Record any additional request metadata
                transactionAggregate.RecordAdditionalRequestData(operatorIdentifier, additionalTransactionMetadata);

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
                if (operatorResponse == null){
                    // Failed to perform sed/receive with the operator
                    TransactionResponseCode transactionResponseCode = TransactionResponseCode.OperatorCommsError;
                    String responseMessage = "OPERATOR COMMS ERROR";

                    transactionAggregate.DeclineTransactionLocally(((Int32)transactionResponseCode).ToString().PadLeft(4, '0'), responseMessage);
                }
                else{
                    if (operatorResponse.IsSuccessful){
                        TransactionResponseCode transactionResponseCode = TransactionResponseCode.Success;
                        String responseMessage = "SUCCESS";

                        transactionAggregate.AuthoriseTransaction(operatorIdentifier,
                                                                  operatorResponse.AuthorisationCode,
                                                                  operatorResponse.ResponseCode,
                                                                  operatorResponse.ResponseMessage,
                                                                  operatorResponse.TransactionId,
                                                                  ((Int32)transactionResponseCode).ToString().PadLeft(4, '0'),
                                                                  responseMessage);
                    }
                    else{
                        TransactionResponseCode transactionResponseCode = TransactionResponseCode.TransactionDeclinedByOperator;
                        String responseMessage = "DECLINED BY OPERATOR";

                        transactionAggregate.DeclineTransaction(operatorIdentifier,
                                                                operatorResponse.ResponseCode,
                                                                operatorResponse.ResponseMessage,
                                                                ((Int32)transactionResponseCode).ToString().PadLeft(4, '0'),
                                                                responseMessage);
                    }

                    // Record any additional operator response metadata
                    transactionAggregate.RecordAdditionalResponseData(operatorIdentifier, operatorResponse.AdditionalTransactionResponseMetadata);
                }
            }
            else{
                // Record the failure
                transactionAggregate.DeclineTransactionLocally(((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'), validationResult.responseMessage);
            }

            transactionAggregate.CompleteTransaction();

            // Determine if the email receipt is required
            if (String.IsNullOrEmpty(customerEmailAddress) == false){
                transactionAggregate.RequestEmailReceipt(customerEmailAddress);
            }

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);

            // Get the model from the aggregate
            Transaction transaction = transactionAggregate.GetTransaction();

            return new ProcessSaleTransactionResponse{
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
                                                   CancellationToken cancellationToken){
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.RequestEmailReceiptResend();

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
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

        private async Task<MerchantResponse> GetMerchant(Guid estateId,
                                                         Guid merchantId,
                                                         CancellationToken cancellationToken){
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);

            return merchant;
        }
        
        private async Task<OperatorResponse> ProcessMessageWithOperator(MerchantResponse merchant,
                                                                        Guid transactionId,
                                                                        DateTime transactionDateTime,
                                                                        String operatorIdentifier,
                                                                        Dictionary<String, String> additionalTransactionMetadata,
                                                                        String transactionReference,
                                                                        CancellationToken cancellationToken){
            IOperatorProxy operatorProxy = this.OperatorProxyResolver(operatorIdentifier.Replace(" ", ""));
            OperatorResponse operatorResponse = null;
            try{
                operatorResponse = await operatorProxy.ProcessSaleMessage(this.TokenResponse.AccessToken,
                                                                          transactionId,
                                                                          operatorIdentifier,
                                                                          merchant,
                                                                          transactionDateTime,
                                                                          transactionReference,
                                                                          additionalTransactionMetadata,
                                                                          cancellationToken);
            }
            catch(Exception e){
                // Log out the error
                Logger.LogError(e);
            }

            return operatorResponse;
        }

        #endregion
    }
}