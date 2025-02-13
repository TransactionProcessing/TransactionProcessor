/*using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.EventHandling;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Repository;

namespace TransactionProcessor.BusinessLogic.EventHandling
{
    public class OperatorDomainEventHandler : IDomainEventHandler
    {
        #region Fields

        private readonly ITransactionProcessorReadModelRepository EstateReportingRepository;

        #endregion

        #region Constructors

        public OperatorDomainEventHandler(ITransactionProcessorReadModelRepository estateReportingRepository)
        {
            this.EstateReportingRepository = estateReportingRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the specified domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<Result> Handle(IDomainEvent domainEvent,
                                         CancellationToken cancellationToken)
        {
            Task<Result> t = domainEvent switch
            {
                OperatorDomainEvents.OperatorCreatedEvent oce => this.EstateReportingRepository.AddOperator(oce, cancellationToken),
                OperatorDomainEvents.OperatorNameUpdatedEvent onue => this.EstateReportingRepository.UpdateOperator(onue, cancellationToken),
                OperatorDomainEvents.OperatorRequireCustomMerchantNumberChangedEvent oprcmnce => this.EstateReportingRepository.UpdateOperator(oprcmnce, cancellationToken),
                OperatorDomainEvents.OperatorRequireCustomTerminalNumberChangedEvent oprctnce => this.EstateReportingRepository.UpdateOperator(oprctnce, cancellationToken),
                _ => null
            };
            if (t != null)
                return await t;

            return Result.Success();
        }

        #endregion
    }
}*/
