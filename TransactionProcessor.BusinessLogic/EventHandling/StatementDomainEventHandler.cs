/*using System.Threading;
using System.Threading.Tasks;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.EventHandling;
using SimpleResults;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.ProjectionEngine.Repository;
using TransactionProcessor.Repository;

namespace TransactionProcessor.BusinessLogic.EventHandling
{
    public class StatementDomainEventHandler : IDomainEventHandler
    {
        #region Fields

        /// <summary>
        /// The estate reporting repository
        /// </summary>
        private readonly ITransactionProcessorReadModelRepository EstateReportingRepository;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDomainEventHandler" /> class.
        /// </summary>
        /// <param name="estateReportingRepository">The estate reporting repository.</param>
        public StatementDomainEventHandler(ITransactionProcessorReadModelRepository estateReportingRepository)
        {
            this.EstateReportingRepository = estateReportingRepository;
        }

        #endregion

        #region Methods

        public async Task<Result> Handle(IDomainEvent domainEvent,
                                         CancellationToken cancellationToken)
        {
            return await this.HandleSpecificDomainEvent((dynamic)domainEvent, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(MerchantStatementDomainEvents.StatementCreatedEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            return await this.EstateReportingRepository.CreateStatement(domainEvent, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(MerchantStatementDomainEvents.TransactionAddedToStatementEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            return await this.EstateReportingRepository.AddTransactionToStatement(domainEvent, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(MerchantStatementDomainEvents.SettledFeeAddedToStatementEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            return await this.EstateReportingRepository.AddSettledFeeToStatement(domainEvent, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(MerchantStatementDomainEvents.StatementGeneratedEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            return await this.EstateReportingRepository.MarkStatementAsGenerated(domainEvent, cancellationToken);
        }
        #endregion
    }
}
*/