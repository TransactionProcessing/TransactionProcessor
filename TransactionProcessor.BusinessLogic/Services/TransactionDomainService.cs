namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;
    using Shared.DomainDrivenDesign.EventStore;
    using Shared.EventStore.EventStore;
    using TransactionAggregate;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.BusinessLogic.Services.ITransactionDomainService" />
    public class TransactionDomainService : ITransactionDomainService
    {
        private readonly IAggregateRepositoryManager AggregateRepositoryManager;

        public TransactionDomainService(IAggregateRepositoryManager aggregateRepositoryManager)
        {
            this.AggregateRepositoryManager = aggregateRepositoryManager;
        }

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
        public async Task<ProcessLogonTransactionResponse> ProcessLogonTransaction(Guid transactionId, Guid estateId, Guid merchantId, DateTime transactionDateTime,
                                                                                   String transactionNumber, String deviceIdentifier, CancellationToken cancellationToken)
        {
            IAggregateRepository<TransactionAggregate> transactionAggregateRepository = this.AggregateRepositoryManager.GetAggregateRepository<TransactionAggregate>(estateId);
            
            TransactionAggregate transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
            transactionAggregate.StartTransaction(transactionDateTime, transactionNumber, "Logon", estateId, merchantId, deviceIdentifier);
            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);

            transactionAggregate = await transactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
            transactionAggregate.AuthoriseTransactionLocally("ABCD1234", "0000", "SUCCESS");
            await transactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);

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


        #endregion
    }
}