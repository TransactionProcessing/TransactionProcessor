namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore.Internal;
    using Models;
    using OperatorInterfaces;
    using Shared.DomainDrivenDesign.EventStore;
    using Shared.EventStore.EventStore;
    using TransactionAggregate;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.BusinessLogic.Services.ITransactionAggregateManager" />
    public class TransactionAggregateManager : ITransactionAggregateManager
    {
        #region Fields

        /// <summary>
        /// The aggregate repository manager
        /// </summary>
        private readonly IAggregateRepositoryManager AggregateRepositoryManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAggregateManager"/> class.
        /// </summary>
        /// <param name="aggregateRepositoryManager">The aggregate repository manager.</param>
        public TransactionAggregateManager(IAggregateRepositoryManager aggregateRepositoryManager)
        {
            this.AggregateRepositoryManager = aggregateRepositoryManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Authorises the transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="operatorResponse">The operator response.</param>
        /// <param name="transactionResponseCode">The transaction response code.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task AuthoriseTransaction(Guid estateId,
                                               Guid transactionId,
                                               String operatorIdentifier,
                                               OperatorResponse operatorResponse,
                                               TransactionResponseCode transactionResponseCode,
                                               String responseMessage,
                                               CancellationToken cancellationToken)
        {
            IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.AuthoriseTransaction(operatorIdentifier,
                                                      operatorResponse.AuthorisationCode,
                                                      operatorResponse.ResponseCode,
                                                      operatorResponse.ResponseMessage,
                                                      operatorResponse.TransactionId,
                                                      ((Int32)transactionResponseCode).ToString().PadLeft(4, '0'),
                                                      responseMessage);

            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        /// <summary>
        /// Authorises the transaction locally.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="authorisationCode">The authorisation code.</param>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task AuthoriseTransactionLocally(Guid estateId,
                                                      Guid transactionId,
                                                      String authorisationCode,
                                                      (String responseMessage, TransactionResponseCode responseCode) validationResult,
                                                      CancellationToken cancellationToken)
        {
            IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.AuthoriseTransactionLocally(authorisationCode,
                                                             ((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'),
                                                             validationResult.responseMessage);

            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        /// <summary>
        /// Completes the transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task CompleteTransaction(Guid estateId,
                                              Guid transactionId,
                                              CancellationToken cancellationToken)
        {
            IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.CompleteTransaction();

            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        /// <summary>
        /// Declines the transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="operatorResponse">The operator response.</param>
        /// <param name="transactionResponseCode">The transaction response code.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task DeclineTransaction(Guid estateId,
                                             Guid transactionId,
                                             String operatorIdentifier,
                                             OperatorResponse operatorResponse,
                                             TransactionResponseCode transactionResponseCode,
                                             String responseMessage,
                                             CancellationToken cancellationToken)
        {
            IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.DeclineTransaction(operatorIdentifier,
                                                    operatorResponse.ResponseCode,
                                                    operatorResponse.ResponseMessage,
                                                    ((Int32)transactionResponseCode).ToString().PadLeft(4, '0'),
                                                    responseMessage);

            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        /// <summary>
        /// Declines the transaction locally.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task DeclineTransactionLocally(Guid estateId,
                                                    Guid transactionId,
                                                    (String responseMessage, TransactionResponseCode responseCode) validationResult,
                                                    CancellationToken cancellationToken)
        {
            IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.DeclineTransactionLocally(((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'), validationResult.responseMessage);

            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        /// <summary>
        /// Gets the aggregate.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<TransactionAggregate> GetAggregate(Guid estateId,
                                                             Guid transactionId,
                                                             CancellationToken cancellationToken)
        {
            IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
            return transactionAggregate;
        }

        /// <summary>
        /// Records the additional request data.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="additionalTransactionRequestMetadata">The additional transaction request metadata.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task RecordAdditionalRequestData(Guid estateId,
                                                      Guid transactionId,
                                                      String operatorIdentifier,
                                                      Dictionary<String, String> additionalTransactionRequestMetadata,
                                                      CancellationToken cancellationToken)
        {
            if (additionalTransactionRequestMetadata != null && additionalTransactionRequestMetadata.Any())
            {
                IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                    this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

                TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

                transactionAggregate.RecordAdditionalRequestData(operatorIdentifier, additionalTransactionRequestMetadata);

                await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
            }
        }

        /// <summary>
        /// Records the additional response data.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="additionalTransactionResponseMetadata">The additional transaction response metadata.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task RecordAdditionalResponseData(Guid estateId,
                                                       Guid transactionId,
                                                       String operatorIdentifier,
                                                       Dictionary<String, String> additionalTransactionResponseMetadata,
                                                       CancellationToken cancellationToken)
        {
            if (additionalTransactionResponseMetadata != null && additionalTransactionResponseMetadata.Any())
            {
                IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                    this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

                TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

                transactionAggregate.RecordAdditionalResponseData(operatorIdentifier, additionalTransactionResponseMetadata);

                await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
            }
        }

        /// <summary>
        /// Requests the email receipt.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task RequestEmailReceipt(Guid estateId, Guid transactionId, String customerEmailAddress, CancellationToken cancellationToken)
        {
            IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.RequestEmailReceipt(customerEmailAddress);

            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        /// <summary>
        /// Starts the transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task StartTransaction(Guid transactionId,
                                           DateTime transactionDateTime,
                                           String transactionNumber,
                                           TransactionType transactionType,
                                           String transactionReference,
                                           Guid estateId,
                                           Guid merchantId,
                                           String deviceIdentifier,
                                           CancellationToken cancellationToken)
        {
            IAggregateRepository<TransactionAggregate> transactionAggregateRepository =
                this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);

            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.StartTransaction(transactionDateTime, transactionNumber, transactionType, transactionReference, estateId, merchantId, deviceIdentifier);

            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        #endregion
    }
}