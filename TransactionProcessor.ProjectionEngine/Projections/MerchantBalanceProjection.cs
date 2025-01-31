using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.ProjectionEngine.Projections;

using EstateManagement.Merchant.DomainEvents;
using Shared.DomainDrivenDesign.EventSourcing;
using State;

public class MerchantBalanceProjection : IProjection<MerchantBalanceState>
{
    public async Task<MerchantBalanceState> Handle(MerchantBalanceState state,
                                                   IDomainEvent domainEvent,
                                                   CancellationToken cancellationToken)
    {
        MerchantBalanceState newState = domainEvent switch {
            MerchantCreatedEvent mce => state.HandleMerchantCreated(mce),
            ManualDepositMadeEvent mdme => state.HandleManualDepositMadeEvent(mdme),
            WithdrawalMadeEvent wme => state.HandleWithdrawalMadeEvent(wme),
            AutomaticDepositMadeEvent adme => state.HandleAutomaticDepositMadeEvent(adme),
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
            MerchantCreatedEvent _ => true,
            ManualDepositMadeEvent _ => true,
            AutomaticDepositMadeEvent _ => true,
            TransactionDomainEvents.TransactionHasStartedEvent _ => true,
            TransactionDomainEvents.TransactionHasBeenCompletedEvent _ => true,
            TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent _ => true,
            _ => false
        };
    }
}