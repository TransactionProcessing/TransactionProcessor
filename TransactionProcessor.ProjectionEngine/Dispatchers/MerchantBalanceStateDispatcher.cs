﻿namespace TransactionProcessor.ProjectionEngine.Dispatchers;

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
                   ChangeAmount = @event.CalculatedValue,
                   DateTime = @event.FeeCalculatedDateTime.AddSeconds(2),
                   Reference = "Transaction Fee Processed",
                   AggregateId = @event.TransactionId,
                   OriginalEventId = @event.EventId,
                   DebitOrCredit = "C"
               };
    }

    private MerchantBalanceChangedEntry CreateTransactionBalanceEntry(MerchantBalanceState state, TransactionHasBeenCompletedEvent @event) {
        if (@event.IsAuthorised == false)
            return null;
            
        Decimal transactionAmount = @event.TransactionAmount.GetValueOrDefault(0);

        // Skip logons
        if (transactionAmount == 0)
            return null;

        if (transactionAmount < 0)
            transactionAmount = transactionAmount * -1;

        return new MerchantBalanceChangedEntry
               {
                   MerchantId = @event.MerchantId,
                   EstateId = @event.EstateId,
                   ChangeAmount = transactionAmount,
                   DateTime = @event.CompletedDateTime,
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
                                                   ChangeAmount = 0,
                                                   DateTime = @event.DateCreated,
                                                   Reference = "Opening Balance",
                                                   AggregateId = @event.MerchantId,
                                                   OriginalEventId = @event.EventId,
                                                   DebitOrCredit = "C"
                                               };
    }
}