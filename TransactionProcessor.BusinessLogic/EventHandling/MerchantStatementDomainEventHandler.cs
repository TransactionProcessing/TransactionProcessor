using MediatR;
using Polly;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.EventHandling;
using SimpleResults;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.BusinessLogic.EventHandling
{
    public class MerchantStatementDomainEventHandler : IDomainEventHandler
    {
        #region Fields

        /// <summary>
        /// The mediator
        /// </summary>
        private readonly IMediator Mediator;

        #endregion

        #region Constructors

        public MerchantStatementDomainEventHandler(IMediator mediator)
        {
            this.Mediator = mediator;
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
            return await this.HandleSpecificDomainEvent((dynamic)domainEvent, cancellationToken);
        }

        /// <summary>
        /// Handles the specific domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task<Result> HandleSpecificDomainEvent(MerchantStatementDomainEvents.StatementGeneratedEvent domainEvent,
                                                             CancellationToken cancellationToken) {

            IAsyncPolicy<Result> retryPolicy = PolicyFactory.CreatePolicy(2, policyTag: "MerchantStatementDomainEventHandler - StatementGeneratedEvent");

            return await PolicyFactory.ExecuteWithPolicyAsync(async () => {
                MerchantStatementCommands.EmailMerchantStatementCommand command = new(domainEvent.EstateId, domainEvent.MerchantId, domainEvent.MerchantStatementId);

                return await this.Mediator.Send(command, cancellationToken);
            }, retryPolicy, "MerchantStatementDomainEventHandler - StatementGeneratedEvent");
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            IAsyncPolicy<Result> retryPolicy = PolicyFactory.CreatePolicy(2, policyTag: "MerchantStatementDomainEventHandler - TransactionHasBeenCompletedEvent");

            return await PolicyFactory.ExecuteWithPolicyAsync(async () => {
                MerchantStatementCommands.AddTransactionToMerchantStatementCommand command = new(domainEvent.EstateId, domainEvent.MerchantId, domainEvent.CompletedDateTime, domainEvent.TransactionAmount, domainEvent.IsAuthorised, domainEvent.TransactionId);

                return await this.Mediator.Send(command, cancellationToken);
            },retryPolicy, "MerchantStatementDomainEventHandler - TransactionHasBeenCompletedEvent");
        }

        #endregion
    }
}