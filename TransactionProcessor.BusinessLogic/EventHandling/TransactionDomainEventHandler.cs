using MediatR;
using SimpleResults;
using System.Diagnostics;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.BusinessLogic.EventHandling
{
    using Polly;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.EventHandling;
    using Shared.Logger;
    using System.Threading;
    using System.Threading.Tasks;
    using TransactionProcessor.BusinessLogic.Common;
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
           
            IAsyncPolicy<Result> retryPolicy = PolicyFactory.CreatePolicy(2, policyTag: "TransactionDomainEventHandler - FloatCreditPurchasedEvent");

            return await PolicyFactory.ExecuteWithPolicyAsync(async () => {
                FloatActivityCommands.RecordCreditPurchaseCommand command =
                    new(domainEvent.EstateId, domainEvent.FloatId,
                        domainEvent.CreditPurchasedDateTime, domainEvent.Amount, domainEvent.EventId);

                return await this.Mediator.Send(command, cancellationToken);
            },retryPolicy, "TransactionDomainEventHandler - FloatCreditPurchasedEvent");
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.TransactionCostInformationRecordedEvent domainEvent, CancellationToken cancellationToken){
            
            IAsyncPolicy<Result> retryPolicy = PolicyFactory.CreatePolicy(2, policyTag: "TransactionDomainEventHandler - TransactionCostInformationRecordedEvent");

            return await PolicyFactory.ExecuteWithPolicyAsync(async () => {
                FloatActivityCommands.RecordTransactionCommand command = new(domainEvent.EstateId,
                    domainEvent.TransactionId);

                return await this.Mediator.Send(command, cancellationToken);
            }, retryPolicy, "TransactionDomainEventHandler - TransactionCostInformationRecordedEvent");
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            CalculateFeesForTransactionCommand command = new(domainEvent.TransactionId, domainEvent.CompletedDateTime, domainEvent.EstateId, domainEvent.MerchantId);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            IAsyncPolicy<Result> retryPolicy = PolicyFactory.CreatePolicy(2, policyTag: "TransactionDomainEventHandler - MerchantFeePendingSettlementAddedToTransactionEvent");

            return await PolicyFactory.ExecuteWithPolicyAsync(async () => {
                SettlementCommands.AddMerchantFeePendingSettlementCommand command =
                    new SettlementCommands.AddMerchantFeePendingSettlementCommand(domainEvent.TransactionId,
                        domainEvent.CalculatedValue, domainEvent.FeeCalculatedDateTime,
                        (CalculationType)domainEvent.FeeCalculationType, domainEvent.FeeId, domainEvent.FeeValue,
                        domainEvent.SettlementDueDate, domainEvent.MerchantId, domainEvent.EstateId);
                return await this.Mediator.Send(command, cancellationToken);
            }, retryPolicy, "TransactionDomainEventHandler - MerchantFeePendingSettlementAddedToTransactionEvent");
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            IAsyncPolicy<Result> retryPolicy = PolicyFactory.CreatePolicy(2, policyTag: "TransactionDomainEventHandler - SettledMerchantFeeAddedToTransactionEvent");

            return await PolicyFactory.ExecuteWithPolicyAsync(async () => {
                AddSettledFeeToSettlementCommand command = new AddSettledFeeToSettlementCommand(
                    domainEvent.SettledDateTime.Date, domainEvent.MerchantId, domainEvent.EstateId, domainEvent.FeeId, domainEvent.TransactionId);
                return await this.Mediator.Send(command, cancellationToken);
            },retryPolicy, "TransactionDomainEventHandler - SettledMerchantFeeAddedToTransactionEvent");
        }

        private async Task<Result> HandleSpecificDomainEvent(SettlementDomainEvents.MerchantFeeSettledEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            IAsyncPolicy<Result> retryPolicy = PolicyFactory.CreatePolicy(2, policyTag: "TransactionDomainEventHandler - MerchantFeeSettledEvent");

            return await PolicyFactory.ExecuteWithPolicyAsync(async () => {
                AddSettledMerchantFeeCommand command = new(domainEvent.TransactionId, domainEvent.CalculatedValue,
                    domainEvent.FeeCalculatedDateTime, (CalculationType)domainEvent.FeeCalculationType, domainEvent.FeeId,
                    domainEvent.FeeValue, domainEvent.SettledDateTime, domainEvent.SettlementId);
                return await this.Mediator.Send(command, cancellationToken);
            }, retryPolicy, "TransactionDomainEventHandler - MerchantFeeSettledEvent");
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