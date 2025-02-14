using System.Diagnostics;
using MediatR;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.BusinessLogic.EventHandling
{
    using System.Threading;
    using System.Threading.Tasks;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.EventHandling;
    using Shared.Logger;
    using static TransactionProcessor.BusinessLogic.Requests.SettlementCommands;
    using static TransactionProcessor.BusinessLogic.Requests.TransactionCommands;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.EventStore.EventHandling.IDomainEventHandler" />
    /// <seealso cref="IDomainEventHandler" />
    public class TransactionDomainEventHandler : IDomainEventHandler
    {
        #region Fields
        
        private readonly IMediator Mediator;

        private TokenResponse TokenResponse;


        #endregion

        #region Constructors

        public TransactionDomainEventHandler(IMediator mediator) {
            this.Mediator = mediator;
        }

        #endregion

        #region Methods

        public async Task<Result> Handle(IDomainEvent domainEvent,
                                         CancellationToken cancellationToken) {

            Logger.LogWarning($"|{domainEvent.EventId}|Transaction Domain Event Handler - Inside Handle {domainEvent.EventType}");
            Stopwatch sw = Stopwatch.StartNew();
            var result = await this.HandleSpecificDomainEvent((dynamic)domainEvent, cancellationToken);
            sw.Stop();
            Logger.LogWarning($"|{domainEvent.EventId}|Transaction Domain Event Handler - after HandleSpecificDomainEvent {domainEvent.EventType} time {sw.ElapsedMilliseconds}ms");

            return result;
        }

        private async Task<Result> HandleSpecificDomainEvent(FloatDomainEvents.FloatCreditPurchasedEvent domainEvent,
                                                     CancellationToken cancellationToken) {
            FloatActivityCommands.RecordCreditPurchaseCommand command =
                new(domainEvent.EstateId, domainEvent.FloatId,
                    domainEvent.CreditPurchasedDateTime, domainEvent.Amount, domainEvent.EventId);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.TransactionCostInformationRecordedEvent domainEvent, CancellationToken cancellationToken){
            FloatActivityCommands.RecordTransactionCommand command = new(domainEvent.EstateId,
                domainEvent.TransactionId);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            CalculateFeesForTransactionCommand command = new(domainEvent.TransactionId, domainEvent.CompletedDateTime, domainEvent.EstateId, domainEvent.MerchantId);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent domainEvent,
                                                             CancellationToken cancellationToken) {

            SettlementCommands.AddMerchantFeePendingSettlementCommand command =
                new SettlementCommands.AddMerchantFeePendingSettlementCommand(domainEvent.TransactionId,
                    domainEvent.CalculatedValue, domainEvent.FeeCalculatedDateTime,
                    (CalculationType)domainEvent.FeeCalculationType, domainEvent.FeeId, domainEvent.FeeValue,
                    domainEvent.SettlementDueDate, domainEvent.MerchantId, domainEvent.EstateId);
            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            AddSettledFeeToSettlementCommand command = new AddSettledFeeToSettlementCommand(
                domainEvent.SettledDateTime.Date, domainEvent.MerchantId, domainEvent.EstateId, domainEvent.FeeId, domainEvent.TransactionId);
            return await this.Mediator.Send(command, cancellationToken);

        }

        private async Task<Result> HandleSpecificDomainEvent(SettlementDomainEvents.MerchantFeeSettledEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            AddSettledMerchantFeeCommand command = new(domainEvent.TransactionId, domainEvent.CalculatedValue,
                domainEvent.FeeCalculatedDateTime, (CalculationType)domainEvent.FeeCalculationType, domainEvent.FeeId,
                domainEvent.FeeValue, domainEvent.SettledDateTime, domainEvent.SettlementId);
            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.CustomerEmailReceiptRequestedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            SendCustomerEmailReceiptCommand command = new(domainEvent.EstateId, domainEvent.TransactionId, domainEvent.EventId, domainEvent.CustomerEmailAddress);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.CustomerEmailReceiptResendRequestedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            TransactionCommands.ResendCustomerEmailReceiptCommand command = new(domainEvent.EstateId, domainEvent.TransactionId);

            return await this.Mediator.Send(command, cancellationToken);
        }
        
        #endregion
    }
}