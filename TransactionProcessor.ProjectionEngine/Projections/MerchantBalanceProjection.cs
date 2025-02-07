using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.ProjectionEngine.Projections;

using Shared.DomainDrivenDesign.EventSourcing;
using State;

public class MerchantBalanceProjection : IProjection<MerchantBalanceState>
{
    public async Task<MerchantBalanceState> Handle(MerchantBalanceState state,
                                                   IDomainEvent domainEvent,
                                                   CancellationToken cancellationToken)
    {
        MerchantBalanceState newState = domainEvent switch {
            MerchantDomainEvents.MerchantCreatedEvent mce => state.HandleMerchantCreated(mce),
            MerchantDomainEvents.ManualDepositMadeEvent mdme => state.HandleManualDepositMadeEvent(mdme),
            MerchantDomainEvents.WithdrawalMadeEvent wme => state.HandleWithdrawalMadeEvent(wme),
            MerchantDomainEvents.AutomaticDepositMadeEvent adme => state.HandleAutomaticDepositMadeEvent(adme),
            TransactionDomainEvents.TransactionHasStartedEvent thse => state.HandleTransactionHasStartedEvent(thse),
            TransactionDomainEvents.TransactionHasBeenCompletedEvent thbce => state.HandleTransactionHasBeenCompletedEvent(thbce),
            TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent mfatte => state.HandleSettledMerchantFeeAddedToTransactionEvent(mfatte),
            _ => state
        };

        return newState;
    }

    public bool ShouldIHandleEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            MerchantDomainEvents.MerchantCreatedEvent _ => true,
            MerchantDomainEvents.ManualDepositMadeEvent _ => true,
            MerchantDomainEvents.AutomaticDepositMadeEvent _ => true,
            TransactionDomainEvents.TransactionHasStartedEvent _ => true,
            TransactionDomainEvents.TransactionHasBeenCompletedEvent _ => true,
            TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent _ => true,
            _ => false
        };
    }
}