using MediatR;
using Prometheus;
using Shared.EventStore.Aggregate;
using SimpleResults;
using System;
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
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using TransactionProcessor.BusinessLogic.Common;
    using TransactionProcessor.BusinessLogic.Services;
    using TransactionProcessor.DataTransferObjects;
    using TransactionProcessor.Models;
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
            Stopwatch sw = Stopwatch.StartNew();
            String g = domainEvent.GetType().Name;
            String m = this.GetType().Name;
            Counter counterCalls = AggregateService.GetCounterMetric($"{m}_{g}_events_processed");
            Histogram histogramMetric = AggregateService.GetHistogramMetric($"{m}_{g}");
            counterCalls.Inc();

            Task<Result> t = domainEvent switch
            {
                FloatDomainEvents.FloatCreditPurchasedEvent fcpe => this.HandleSpecificDomainEvent(fcpe, cancellationToken),
                TransactionDomainEvents.TransactionCostInformationRecordedEvent tcire => this.HandleSpecificDomainEvent(tcire, cancellationToken),
                TransactionDomainEvents.TransactionHasBeenCompletedEvent thbce => this.HandleSpecificDomainEvent(thbce, cancellationToken),
                TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent mfpse => this.HandleSpecificDomainEvent(mfpse, cancellationToken),
                TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent smfate => this.HandleSpecificDomainEvent(smfate, cancellationToken),
                TransactionDomainEvents.CustomerEmailReceiptRequestedEvent ce => this.HandleSpecificDomainEvent(ce, cancellationToken),
                TransactionDomainEvents.CustomerEmailReceiptResendRequestedEvent cer => this.HandleSpecificDomainEvent(cer, cancellationToken),
                _ => null
            };

            Result result = Result.Success();
            if (t != null)
                result = await t;
            sw.Stop();
            histogramMetric.Observe(sw.Elapsed.TotalSeconds);
            return result;
        }

        private async Task<Result> HandleSpecificDomainEvent(FloatDomainEvents.FloatCreditPurchasedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            FloatActivityCommands.RecordCreditPurchaseCommand command = new(domainEvent.EstateId, domainEvent.FloatId, domainEvent.CreditPurchasedDateTime, domainEvent.Amount, domainEvent.EventId);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.TransactionCostInformationRecordedEvent domainEvent,
                                                             CancellationToken cancellationToken) {

            FloatActivityCommands.RecordTransactionCommand command = new(domainEvent.EstateId, domainEvent.TransactionId);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent,
                                                             CancellationToken cancellationToken) {

            CalculateFeesForTransactionCommand command = new(domainEvent.TransactionId, domainEvent.CompletedDateTime, domainEvent.EstateId, domainEvent.MerchantId);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            SettlementCommands.AddMerchantFeePendingSettlementCommand command = new SettlementCommands.AddMerchantFeePendingSettlementCommand(domainEvent.TransactionId, domainEvent.CalculatedValue, domainEvent.FeeCalculatedDateTime, (CalculationType)domainEvent.FeeCalculationType, domainEvent.FeeId, domainEvent.FeeValue, domainEvent.SettlementDueDate, domainEvent.MerchantId, domainEvent.EstateId);
            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            AddSettledFeeToSettlementCommand command = new AddSettledFeeToSettlementCommand(domainEvent.SettledDateTime.Date, domainEvent.MerchantId, domainEvent.EstateId, domainEvent.FeeId, domainEvent.TransactionId);
            var result = await this.Mediator.Send(command, cancellationToken);
            if (result.IsFailed) {
                return result;
            }

            // Kick off a background command to do the balance processing
            // TODO: maybe add in some retry logic here or offload to another process to retry?
            FireAndForgetHelper.Run(
                async () => {
                    MerchantBalanceCommands.RecordSettledFeeCommand balanceCommand = new MerchantBalanceCommands.RecordSettledFeeCommand(domainEvent.EstateId, domainEvent.MerchantId, domainEvent.FeeId, domainEvent.CalculatedValue, domainEvent.SettledDateTime);
                    Result balanceResult = await this.Mediator.Send(balanceCommand, cancellationToken);
                    if (balanceResult.IsFailed)
                    {
                        throw new Exception($"Failed to record balance for transaction {domainEvent.TransactionId} fee {domainEvent.FeeId} with error: {balanceResult.Message}");
                    }
                },
                $"Balance Recording for Transaction {domainEvent.TransactionId} Fee {domainEvent.FeeId}"
            );

            return result;
        }

        private async Task<Result> HandleSpecificDomainEvent(SettlementDomainEvents.MerchantFeeSettledEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            AddSettledMerchantFeeCommand command = new(domainEvent.TransactionId, domainEvent.CalculatedValue, domainEvent.FeeCalculatedDateTime, (CalculationType)domainEvent.FeeCalculationType, domainEvent.FeeId, domainEvent.FeeValue, domainEvent.SettledDateTime, domainEvent.SettlementId);
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

    [ExcludeFromCodeCoverage]
    public static class FireAndForgetHelper
    {
        public static void Run(Func<Task> action, string contextDescription = "background task")
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    Exception e = new Exception($"Unhandled exception in {contextDescription}", ex);
                    Logger.LogError(e);
                }
            });
        }
    }
}