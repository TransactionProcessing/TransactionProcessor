using System;
using MediatR;
using Polly;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.EventHandling;
using SimpleResults;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Prometheus;
using Shared.Logger;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.DomainEvents;
using Shared.EventStore.Aggregate;

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

        public MerchantStatementDomainEventHandler(IMediator mediator) {
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
            Stopwatch sw = Stopwatch.StartNew();
            String g = domainEvent.GetType().Name;
            String m = this.GetType().Name;
            Counter counterCalls = AggregateService.GetCounterMetric($"{m}_{g}_events_processed");
            Histogram histogramMetric = AggregateService.GetHistogramMetric($"{m}_{g}");
            counterCalls.Inc();

            Task<Result> t = domainEvent switch
            {
                SettlementDomainEvents.MerchantFeeSettledEvent de => this.HandleSpecificDomainEvent(de, cancellationToken),
                MerchantStatementDomainEvents.StatementGeneratedEvent de => this.HandleSpecificDomainEvent(de, cancellationToken),
                TransactionDomainEvents.TransactionHasBeenCompletedEvent de => this.HandleSpecificDomainEvent(de, cancellationToken),
                MerchantStatementForDateDomainEvents.StatementCreatedForDateEvent de => this.HandleSpecificDomainEvent(de, cancellationToken),
                MerchantStatementDomainEvents.StatementBuiltEvent de => this.HandleSpecificDomainEvent(de, cancellationToken),
                _ => null
            };

            Result result = Result.Success();
            if (t != null)
                result = await t;
            sw.Stop();
            histogramMetric.Observe(sw.Elapsed.TotalSeconds);
            return result;
        }

        private async Task<Result> HandleSpecificDomainEvent(MerchantStatementDomainEvents.StatementBuiltEvent domainEvent,
                                                       CancellationToken cancellationToken) {
            var statementData = domainEvent.statementData;
            // Decode thebase64 string to a byte array
            byte[] data = Convert.FromBase64String(statementData);
            // Convert the byte array to a string
            string statementDataHtml = System.Text.Encoding.UTF8.GetString(data);
            await File.WriteAllTextAsync($"C:\\Temp\\statements\\inbound\\{domainEvent.MerchantStatementId}.html", statementDataHtml, cancellationToken);
            return Result.Success();
        }

        private async Task<Result> HandleSpecificDomainEvent(MerchantStatementForDateDomainEvents.StatementCreatedForDateEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            MerchantStatementCommands.RecordActivityDateOnMerchantStatementCommand command = new(domainEvent.EstateId, domainEvent.MerchantId, domainEvent.MerchantStatementId, domainEvent.MerchantStatementDate,
                domainEvent.MerchantStatementForDateId, domainEvent.ActivityDate);
            return await this.Mediator.Send(command, cancellationToken);
        }

        /// <summary>
        /// Handles the specific domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task<Result> HandleSpecificDomainEvent(MerchantStatementDomainEvents.StatementGeneratedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            MerchantStatementCommands.BuildMerchantStatementCommand command = new(domainEvent.EstateId, domainEvent.MerchantId, domainEvent.MerchantStatementId);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            MerchantStatementCommands.AddTransactionToMerchantStatementCommand command = new(domainEvent.EstateId, domainEvent.MerchantId, domainEvent.CompletedDateTime, domainEvent.TransactionAmount, domainEvent.IsAuthorised, domainEvent.TransactionId);

            Result result = await this.Mediator.Send(command, cancellationToken);
            return result;
        }

        private async Task<Result> HandleSpecificDomainEvent(SettlementDomainEvents.MerchantFeeSettledEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            MerchantStatementCommands.AddSettledFeeToMerchantStatementCommand command = new(domainEvent.EstateId, domainEvent.MerchantId, domainEvent.FeeCalculatedDateTime, domainEvent.CalculatedValue, domainEvent.TransactionId, domainEvent.FeeId);

            //return await this.Mediator.Send(command, cancellationToken);
            Result result = await this.Mediator.Send(command, cancellationToken);
            return result;
        }

        #endregion
    }
}