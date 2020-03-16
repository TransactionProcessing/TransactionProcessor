namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Requests;
    using EstateManagement.DataTransferObjects.Responses;
    using Models;
    using OperatorInterfaces;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.DomainDrivenDesign.EventStore;
    using Shared.EventStore.EventStore;
    using Shared.Exceptions;
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
        /// The aggregate repository manager
        /// </summary>
        private readonly IAggregateRepositoryManager AggregateRepositoryManager;

        /// <summary>
        /// The estate client
        /// </summary>
        private readonly IEstateClient EstateClient;

        /// <summary>
        /// The security service client
        /// </summary>
        private readonly ISecurityServiceClient SecurityServiceClient;

        private readonly Func<String, IOperatorProxy> OperatorProxyResolver;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDomainService" /> class.
        /// </summary>
        /// <param name="aggregateRepositoryManager">The aggregate repository manager.</param>
        /// <param name="estateClient">The estate client.</param>
        /// <param name="securityServiceClient">The security service client.</param>
        /// <param name="operatorProxyResolver">The operator proxy resolver.</param>
        public TransactionDomainService(IAggregateRepositoryManager aggregateRepositoryManager,
                                        IEstateClient estateClient,
                                        ISecurityServiceClient securityServiceClient,
                                        Func<String, IOperatorProxy> operatorProxyResolver)
        {
            this.AggregateRepositoryManager = aggregateRepositoryManager;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
            this.OperatorProxyResolver = operatorProxyResolver;
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
                                                                                   CancellationToken cancellationToken)
        {
            TransactionType transactionType = TransactionType.Logon;

            IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
            transactionAggregate.StartTransaction(transactionDateTime, transactionNumber, transactionType, estateId, merchantId, deviceIdentifier);
            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);

            (String responseMessage, TransactionResponseCode responseCode) validationResult = await this.ValidateTransaction(estateId, merchantId, deviceIdentifier, transactionType, cancellationToken);

            if (validationResult.responseCode == TransactionResponseCode.Success)
            {
                // Record the successful validation
                transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
                // TODO: Generate local authcode
                transactionAggregate.AuthoriseTransactionLocally("ABCD1234", ((Int32)validationResult.responseCode).ToString().PadLeft(4,'0'), validationResult.responseMessage);
                await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
            }
            else
            {
                // Record the failure
                transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
                transactionAggregate.DeclineTransactionLocally(((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'), validationResult.responseMessage);
                await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
            }

            transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
            transactionAggregate.CompleteTransaction();
            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);

            return new ProcessLogonTransactionResponse
                   {
                       ResponseMessage = transactionAggregate.ResponseMessage,
                       ResponseCode = transactionAggregate.ResponseCode,
                       EstateId = estateId,
                       MerchantId = merchantId
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
        /// <param name="operatorId">The operator identifier.</param>
        /// <param name="additionalTransactionMetadata">The additional transaction metadata.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<ProcessSaleTransactionResponse> ProcessSaleTransaction(Guid transactionId,
                                                                                 Guid estateId,
                                                                                 Guid merchantId,
                                                                                 DateTime transactionDateTime,
                                                                                 String transactionNumber,
                                                                                 String deviceIdentifier,
                                                                                 String operatorId,
                                                                                 Dictionary<String, String> additionalTransactionMetadata,
                                                                                 CancellationToken cancellationToken)
        {
            TransactionType transactionType = TransactionType.Sale;

            IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
            transactionAggregate.StartTransaction(transactionDateTime, transactionNumber, transactionType, estateId, merchantId, deviceIdentifier);
            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);

            (String responseMessage, TransactionResponseCode responseCode) validationResult = await this.ValidateTransaction(estateId, merchantId, deviceIdentifier, transactionType, cancellationToken);

            if (validationResult.responseCode == TransactionResponseCode.Success)
            {
                // TODO: Do the online processing with the operator here
                MerchantResponse merchant = await this.GetMerchant(estateId, merchantId, cancellationToken);
                IOperatorProxy operatorProxy = OperatorProxyResolver(operatorId);
                await operatorProxy.ProcessSaleMessage(transactionId, merchant, transactionDateTime, additionalTransactionMetadata, cancellationToken);

                // Record the successful validation
                transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
                // TODO: Generate local authcode
                transactionAggregate.AuthoriseTransactionLocally("ABCD1234", ((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'), validationResult.responseMessage);
                await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
            }
            else
            {
                // Record the failure
                transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
                transactionAggregate.DeclineTransactionLocally(((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'), validationResult.responseMessage);
                await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
            }

            transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
            transactionAggregate.CompleteTransaction();
            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);

            return new ProcessSaleTransactionResponse
                   {
                       ResponseMessage = transactionAggregate.ResponseMessage,
                       ResponseCode = transactionAggregate.ResponseCode,
                       EstateId = estateId,
                       MerchantId = merchantId,
                       AdditionalTransactionMetadata = new Dictionary<String, String>()
                   };
        }

        /// <summary>
        /// Validates the transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="TransactionProcessor.BusinessLogic.Services.TransactionValidationException">Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}</exception>
        private async Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateTransaction(Guid estateId,
                                                    Guid merchantId,
                                                    String deviceIdentifier,
                                                    TransactionType transactionType,
                                                    CancellationToken cancellationToken)
        {
            try
            {
                EstateResponse estate = null;
                // Validate the Estate Record is a valid estate
                try
                {
                    estate = await this.GetEstate(estateId, cancellationToken);
                }
                catch (Exception ex) when (ex.InnerException != null && ex.InnerException.GetType() == typeof(KeyNotFoundException))
                {
                    throw new TransactionValidationException($"Estate Id [{estateId}] is not a valid estate", TransactionResponseCode.InvalidEstateId);
                }

                // get the merchant record and validate the device
                // TODO: Token
                MerchantResponse merchant = await this.GetMerchant(estateId, merchantId, cancellationToken);

                // TODO: Remove this once GetMerchant returns correct response when merchant not found
                if (merchant.MerchantName == null)
                {
                    throw new TransactionValidationException($"Merchant Id [{merchantId}] is not a valid merchant for estate [{estate.EstateName}]",
                                                             TransactionResponseCode.InvalidMerchantId);
                }

                if (merchant.Devices == null || merchant.Devices.Any() == false)
                {
                    if (transactionType == TransactionType.Logon)
                    {
                        await this.AddDeviceToMerchant(estateId, merchantId, deviceIdentifier, cancellationToken);
                    }
                    else
                    {
                        throw new TransactionValidationException($"Merchant {merchant.MerchantName} has no valid Devices for this transaction.",
                                                                 TransactionResponseCode.NoValidDevices);
                    }
                }
                else
                {
                    // Validate the device
                    KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);

                    if (device.Key == Guid.Empty)
                    {
                        // Device not found,throw error
                        throw new TransactionValidationException($"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}",
                                                                 TransactionResponseCode.InvalidDeviceIdentifier);
                    }
                }

                // If we get here everything is good
                return ("SUCCESS", TransactionResponseCode.Success);
            }
            catch (TransactionValidationException tvex)
            {
                return (tvex.Message, tvex.ResponseCode);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return ("Unspecified Processing Error", TransactionResponseCode.UnknownFailure);
            }

        }

        private TokenResponse TokenResponse;

        private async Task<EstateResponse> GetEstate(Guid estateId, CancellationToken cancellationToken)
        {
            await this.GetToken(cancellationToken);

            EstateResponse estate = await this.EstateClient.GetEstate(this.TokenResponse.AccessToken, estateId, cancellationToken);
            
            return estate;
        }

        private async Task<MerchantResponse> GetMerchant(Guid estateId,
                                                         Guid merchantId,
                                                         CancellationToken cancellationToken)
        {
            await this.GetToken(cancellationToken);

            MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, estateId, merchantId, cancellationToken);

            return merchant;
        }

        private async Task GetToken(CancellationToken cancellationToken)
        {
            if (this.TokenResponse == null)
            {
                // Get a token to talk to the estate service
                String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
                String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");

                Logger.LogInformation($"Client Id is {clientId}");
                Logger.LogInformation($"Client Secret is {clientSecret}");

                TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                Logger.LogInformation($"Token is {token.AccessToken}");
                this.TokenResponse = token;
            }
        }

        private async Task AddDeviceToMerchant(Guid estateId,
                                               Guid merchantId, 
                                               String deviceIdentifier,
                                               CancellationToken cancellationToken)
        {
            await this.GetToken(cancellationToken);

            // Add the device to the merchant
            await this.EstateClient.AddDeviceToMerchant(this.TokenResponse.AccessToken,
                                                        estateId,
                                                        merchantId,
                                                        new AddMerchantDeviceRequest
                                                        {
                                                            DeviceIdentifier = deviceIdentifier
                                                        },
                                                        cancellationToken);
        }

        #endregion
    }
}