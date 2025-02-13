using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.EventHandling;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.BusinessLogic.EventHandling;

public class MerchantSettlementDomainEventHandler : IDomainEventHandler
{
    private readonly IMediator Mediator;

    public MerchantSettlementDomainEventHandler(IMediator mediator) {
        this.Mediator = mediator;
    }

    public async Task<Result>  Handle(IDomainEvent domainEvent,
                             CancellationToken cancellationToken)
    {
        Task<Result> t = domainEvent switch
        {
            SettlementDomainEvents.MerchantFeeSettledEvent mfse => this.HandleSpecificDomainEvent(mfse, cancellationToken),
            _ => null
        };
        if (t != null)
            return await t;

        return Result.Success();
    }

    private async Task<Result> HandleSpecificDomainEvent(SettlementDomainEvents.MerchantFeeSettledEvent domainEvent,
                                                         CancellationToken cancellationToken)
    {
        MerchantStatementCommands.AddSettledFeeToMerchantStatementCommand  command = new(domainEvent.EstateId,
            domainEvent.MerchantId,
            domainEvent.FeeCalculatedDateTime,
            domainEvent.CalculatedValue,
            domainEvent.TransactionId,
            domainEvent.FeeId);

        return await this.Mediator.Send(command, cancellationToken);
    }
}