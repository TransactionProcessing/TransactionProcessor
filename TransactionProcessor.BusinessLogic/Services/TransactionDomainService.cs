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
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.DomainDrivenDesign.EventStore;
    using Shared.EventStore.EventStore;
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDomainService" /> class.
        /// </summary>
        /// <param name="aggregateRepositoryManager">The aggregate repository manager.</param>
        /// <param name="estateClient">The estate client.</param>
        /// <param name="securityServiceClient">The security service client.</param>
        public TransactionDomainService(IAggregateRepositoryManager aggregateRepositoryManager,
                                        IEstateClient estateClient,
                                        ISecurityServiceClient securityServiceClient)
        {
            this.AggregateRepositoryManager = aggregateRepositoryManager;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
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
            IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
            transactionAggregate.StartTransaction(transactionDateTime, transactionNumber, "Logon", estateId, merchantId, deviceIdentifier);
            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);

            (String responseMessage, TransactionResponseCode responseCode) validationResult = await this.ValidateTransaction(estateId, merchantId, deviceIdentifier, cancellationToken);

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
        /// Validates the transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="TransactionProcessor.BusinessLogic.Services.TransactionValidationException">Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}</exception>
        private async Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateTransaction(Guid estateId,
                                                    Guid merchantId,
                                                    String deviceIdentifier,
                                                    CancellationToken cancellationToken)
        {
            try
            {

                // Get a token to talk to the estate service
                String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
                String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");

                Logger.LogInformation($"Client Id is {clientId}");
                Logger.LogInformation($"Client Secret is {clientSecret}");

                TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                Logger.LogInformation($"Token is {token.AccessToken}");

                // Validate the Estate Record is a valid estate
                var estate = await this.EstateClient.GetEstate(token.AccessToken, estateId, cancellationToken);

                // TODO: Remove this once GetEstate returns correct response when estate not found
                if (estate.EstateName == null)
                {






                    throw new TransactionValidationException($"Estate Id [{estateId}] is not a valid estate", TransactionResponseCode.InvalidEstateId);
                }

                // get the merchant record and validate the device
                // TODO: Token
                MerchantResponse merchant = await this.EstateClient.GetMerchant(token.AccessToken, estateId, merchantId, cancellationToken);

                if (merchant.Devices == null || merchant.Devices.Any() == false)
                {
                    // Add the device to the merchant
                    await this.EstateClient.AddDeviceToMerchant(token.AccessToken,
                                                                estateId,
                                                                merchantId,
                                                                new AddMerchantDeviceRequest
                                                                {
                                                                    DeviceIdentifier = deviceIdentifier
                                                                },
                                                                cancellationToken);
                }
                else
                {
                    // Validate the device
                    KeyValuePair<Guid, String> device = merchant.Devices.SingleOrDefault(d => d.Value == deviceIdentifier);

                    if (device.Key == Guid.Empty)
                    {
                        // Device not found,throw error
                        throw new TransactionValidationException($"Device Identifier {deviceIdentifier} not valid for Merchant {merchant.MerchantName}", TransactionResponseCode.InvalidDeviceIdentifier);
                    }
                }

                // If we get here everything is good
                return ("SUCCESS", TransactionResponseCode.Success);
            }
            catch (TransactionValidationException tvex)
            {
                return (tvex.Message, tvex.ResponseCode);
            }

        }

        #endregion
    }
}