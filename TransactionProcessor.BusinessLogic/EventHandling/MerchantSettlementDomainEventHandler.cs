using MediatR;
using Polly;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.EventHandling;
using SimpleResults;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Prometheus;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.BusinessLogic.EventHandling;

public class MerchantSettlementDomainEventHandler : IDomainEventHandler {
    private readonly IMediator Mediator;

    public MerchantSettlementDomainEventHandler(IMediator mediator) {
        this.Mediator = mediator;
    }

    public async Task<Result> Handle(IDomainEvent domainEvent,
                                     CancellationToken cancellationToken) {

        Stopwatch sw = Stopwatch.StartNew();
        String g = domainEvent.GetType().Name;
        String m = this.GetType().Name;
        Counter counterCalls = AggregateService.GetCounterMetric($"{m}_{g}_events_processed");
        Histogram histogramMetric = AggregateService.GetHistogramMetric($"{m}_{g}");
        counterCalls.Inc();
        
        Task<Result> t = domainEvent switch {
            SettlementDomainEvents.MerchantFeeSettledEvent mfse => this.HandleSpecificDomainEvent(mfse, cancellationToken),
            _ => null
        };
        
        Result result = Result.Success();
        if (t != null)
            result = await t;
        sw.Stop();
        histogramMetric.Observe(sw.Elapsed.TotalSeconds);
        return result;
    }

    private async Task<Result> HandleSpecificDomainEvent(SettlementDomainEvents.MerchantFeeSettledEvent domainEvent,
                                                         CancellationToken cancellationToken) {
        MerchantStatementCommands.AddSettledFeeToMerchantStatementCommand command = new(domainEvent.EstateId, domainEvent.MerchantId, domainEvent.FeeCalculatedDateTime, domainEvent.CalculatedValue, domainEvent.TransactionId, domainEvent.FeeId);

        return await this.Mediator.Send(command, cancellationToken);
    }
}