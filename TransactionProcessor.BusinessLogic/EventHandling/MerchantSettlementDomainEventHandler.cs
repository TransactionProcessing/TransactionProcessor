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

namespace TransactionProcessor.BusinessLogic.EventHandling;

public class MerchantSettlementDomainEventHandler : IDomainEventHandler {
    private readonly IMediator Mediator;

    public MerchantSettlementDomainEventHandler(IMediator mediator) {
        this.Mediator = mediator;
    }

    public async Task<Result> Handle(IDomainEvent domainEvent,
                                     CancellationToken cancellationToken) {
        Task<Result> t = domainEvent switch {
            SettlementDomainEvents.MerchantFeeSettledEvent mfse => this.HandleSpecificDomainEvent(mfse, cancellationToken),
            _ => null
        };
        if (t != null)
            return await t;

        return Result.Success();
    }

    private async Task<Result> HandleSpecificDomainEvent(SettlementDomainEvents.MerchantFeeSettledEvent domainEvent,
                                                         CancellationToken cancellationToken) {
        IAsyncPolicy<Result> retryPolicy = PolicyFactory.CreatePolicy(2, policyTag: "MerchantSettlementDomainEventHandler - MerchantFeeSettledEvent");

        return await retryPolicy.ExecuteAsync(async () => {
            MerchantStatementCommands.AddSettledFeeToMerchantStatementCommand command = new(domainEvent.EstateId, domainEvent.MerchantId, domainEvent.FeeCalculatedDateTime, domainEvent.CalculatedValue, domainEvent.TransactionId, domainEvent.FeeId);

            return await this.Mediator.Send(command, cancellationToken);
        });
    }
}