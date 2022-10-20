namespace TransactionProcessor.ProjectionEngine.Dispatchers;

using EstateManagement.Merchant.DomainEvents;
using Models;
using Repository;
using Shared.DomainDrivenDesign.EventSourcing;
using State;
using Transaction.DomainEvents;

public class MerchantBalanceStateDispatcher : IStateDispatcher<MerchantBalanceState>
{
    private readonly ITransactionProcessorReadRepository TransactionProcessorReadRepository;

    public MerchantBalanceStateDispatcher(ITransactionProcessorReadRepository transactionProcessorReadRepository) {
        this.TransactionProcessorReadRepository = transactionProcessorReadRepository;
    }
    public async Task Dispatch(MerchantBalanceState state,
                               IDomainEvent @event,
                               CancellationToken cancellationToken) {

        MerchantBalanceChangedEntry entry = @event switch {
            MerchantCreatedEvent e => this.CreateOpeningBalanceEntry(e),
            ManualDepositMadeEvent e => this.CreateManualDepositBalanceEntry(state, e),
            AutomaticDepositMadeEvent e => this.CreateAutomaticDepositBalanceEntry(state, e),
            TransactionHasBeenCompletedEvent e => this.CreateTransactionBalanceEntry(state, e),
            MerchantFeeAddedToTransactionEvent e => this.CreateTransactionFeeBalanceEntry(state, e),
            _ => null
        };

        if (entry == null) 
            return;

        await this.TransactionProcessorReadRepository.AddMerchantBalanceChangedEntry(entry, cancellationToken);
    }

    private MerchantBalanceChangedEntry CreateTransactionFeeBalanceEntry(MerchantBalanceState state, MerchantFeeAddedToTransactionEvent @event)
    {
        return new MerchantBalanceChangedEntry
               {
                   MerchantId = @event.MerchantId,
                   EstateId = @event.EstateId,
                   Balance = state.Balance,
                   ChangeAmount = @event.CalculatedValue,
                   DateTime = @event.FeeCalculatedDateTime,
                   Reference = "Transaction Fee Processed",
                   AggregateId = @event.TransactionId,
                   OriginalEventId = @event.EventId,
                   DebitOrCredit = "C"
               };
    }

    private MerchantBalanceChangedEntry CreateTransactionBalanceEntry(MerchantBalanceState state, TransactionHasBeenCompletedEvent @event) {
        if (@event.IsAuthorised == false)
            return null;
            
        var transactionAmount = @event.TransactionAmount.GetValueOrDefault(0);

        // Skip logons
        if (transactionAmount == 0)
            return null;

        if (transactionAmount < 0)
            transactionAmount = transactionAmount * -1;

        return new MerchantBalanceChangedEntry
               {
                   MerchantId = @event.MerchantId,
                   EstateId = @event.EstateId,
                   Balance = state.Balance,
                   ChangeAmount = transactionAmount,
                   DateTime = @event.CompletedDateTime.AddSeconds(2),
                   Reference = "Transaction Completed",
                   AggregateId = @event.TransactionId,
                   OriginalEventId = @event.EventId,
                   DebitOrCredit = "D"
               };
    }

    private MerchantBalanceChangedEntry CreateManualDepositBalanceEntry(MerchantBalanceState state, ManualDepositMadeEvent @event) {
        return new MerchantBalanceChangedEntry {
                                                   MerchantId = @event.MerchantId,
                                                   EstateId = @event.EstateId,
                                                   Balance = state.Balance,
                                                   ChangeAmount = @event.Amount,
                                                   DateTime = @event.DepositDateTime,
                                                   Reference = "Merchant Deposit",
                                                   AggregateId = @event.MerchantId,
                                                   OriginalEventId = @event.EventId,
                                                   DebitOrCredit = "C"
                                               };
    }

    private MerchantBalanceChangedEntry CreateAutomaticDepositBalanceEntry(MerchantBalanceState state, AutomaticDepositMadeEvent @event)
    {
        return new MerchantBalanceChangedEntry
               {
                   MerchantId = @event.MerchantId,
                   EstateId = @event.EstateId,
                   Balance = state.Balance,
                   ChangeAmount = @event.Amount,
                   DateTime = @event.DepositDateTime,
                   Reference = "Merchant Deposit",
                   AggregateId = @event.MerchantId,
                   OriginalEventId = @event.EventId,
                   DebitOrCredit = "C"
               };
    }

    private MerchantBalanceChangedEntry CreateOpeningBalanceEntry(MerchantCreatedEvent @event) {
        return new MerchantBalanceChangedEntry {
                                                   MerchantId = @event.MerchantId,
                                                   EstateId = @event.EstateId,
                                                   CauseOfChangeId = @event.MerchantId,
                                                   Balance = 0,
                                                   ChangeAmount = 0,
                                                   DateTime = @event.DateCreated.AddMonths(-1),
                                                   Reference = "Opening Balance",
                                                   AggregateId = @event.MerchantId,
                                                   OriginalEventId = @event.EventId,
                                                   DebitOrCredit = "C"
                                               };
    }
}