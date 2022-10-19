namespace TransactionProcessor.ProjectionEngine.Projections;

using EstateManagement.Merchant.DomainEvents;
using Shared.DomainDrivenDesign.EventSourcing;
using State;
using Transaction.DomainEvents;

public class MerchantBalanceProjection : IProjection<MerchantBalanceState>
{
    public async Task<MerchantBalanceState> Handle(MerchantBalanceState state,
                                                   IDomainEvent domainEvent,
                                                   CancellationToken cancellationToken)
    {
        Func<TransactionHasBeenCompletedEvent, MerchantBalanceState> ProcessTransactionHasBeenCompletedEvent = (thbce) =>
                                                                                                               {
                                                                                                                   if (thbce.IsAuthorised)
                                                                                                                   {
                                                                                                                       return state.DecrementBalance(thbce.TransactionAmount.GetValueOrDefault(0)).RecordAuthorisedSale(thbce.TransactionAmount.GetValueOrDefault(0));
                                                                                                                   }
                                                                                                                   else
                                                                                                                   {
                                                                                                                       return state.IncrementAvailableBalance(thbce.TransactionAmount.GetValueOrDefault(0)).RecordDeclinedSale(thbce.TransactionAmount.GetValueOrDefault(0));
                                                                                                                   }
                                                                                                               };

        MerchantBalanceState newState = domainEvent switch {
            MerchantCreatedEvent mce => state.SetEstateId(mce.EstateId).SetMerchantId(mce.MerchantId).SetMerchantName(mce.MerchantName).InitialiseBalances(),
            ManualDepositMadeEvent mdme => state.IncrementAvailableBalance(mdme.Amount).IncrementBalance(mdme.Amount).RecordDeposit(mdme.Amount),
            AutomaticDepositMadeEvent adme => state.IncrementAvailableBalance(adme.Amount).IncrementBalance(adme.Amount).RecordDeposit(adme.Amount),
            TransactionHasStartedEvent thse => state.DecrementAvailableBalance(thse.TransactionAmount.GetValueOrDefault(0)).StartTransaction(thse),
            TransactionHasBeenCompletedEvent thbce => ProcessTransactionHasBeenCompletedEvent(thbce).CompleteTransaction(thbce),
            MerchantFeeAddedToTransactionEvent mfatte => state.IncrementAvailableBalance(mfatte.CalculatedValue).IncrementBalance(mfatte.CalculatedValue)
                                                              .RecordMerchantFee(mfatte.CalculatedValue),
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
            TransactionHasStartedEvent _ => true,
            TransactionHasBeenCompletedEvent _ => true,
            MerchantFeeAddedToTransactionEvent _ => true,
            _ => false
        };
    }
}