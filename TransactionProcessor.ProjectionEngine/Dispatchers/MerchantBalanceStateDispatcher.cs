using Shared.Logger;
using SimpleResults;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.ProjectionEngine.Dispatchers;

using EstateManagement.Merchant.DomainEvents;
using Models;
using Repository;
using Shared.DomainDrivenDesign.EventSourcing;
using State;

public class MerchantBalanceStateDispatcher : IStateDispatcher<MerchantBalanceState>
{
    private readonly ITransactionProcessorReadRepository TransactionProcessorReadRepository;

    public MerchantBalanceStateDispatcher(ITransactionProcessorReadRepository transactionProcessorReadRepository) {
        this.TransactionProcessorReadRepository = transactionProcessorReadRepository;
    }
    public async Task<Result> Dispatch(MerchantBalanceState state,
                                       IDomainEvent @event,
                                       CancellationToken cancellationToken) {

        MerchantBalanceChangedEntry entry = @event switch {
            MerchantCreatedEvent e => this.CreateOpeningBalanceEntry(e),
            ManualDepositMadeEvent e => this.CreateManualDepositBalanceEntry(state, e),
            AutomaticDepositMadeEvent e => this.CreateAutomaticDepositBalanceEntry(state, e),
            WithdrawalMadeEvent e => this.CreateWithdrawalBalanceEntry(state, e),
            TransactionDomainEvents.TransactionHasBeenCompletedEvent e => this.CreateTransactionBalanceEntry(state, e),
            TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent e => this.CreateTransactionFeeBalanceEntry(state, e),
            _ => null
        };

        if (entry == null) {
            Logger.LogInformation($"Entry is null for event {@event.EventType}");
            return Result.Success();
        }

        return await this.TransactionProcessorReadRepository.AddMerchantBalanceChangedEntry(entry, cancellationToken);
    }

    private MerchantBalanceChangedEntry CreateTransactionFeeBalanceEntry(MerchantBalanceState state,
                                                                         TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent @event) =>
        new MerchantBalanceChangedEntry {
                                            MerchantId = @event.MerchantId,
                                            EstateId = @event.EstateId,
                                            ChangeAmount = @event.CalculatedValue,
                                            DateTime = @event.FeeCalculatedDateTime.AddSeconds(2),
                                            Reference = "Transaction Fee Processed",
                                            AggregateId = @event.TransactionId,
                                            OriginalEventId = @event.EventId,
                                            DebitOrCredit = "C"
                                        };

    private MerchantBalanceChangedEntry CreateWithdrawalBalanceEntry(MerchantBalanceState state,
                                                                         WithdrawalMadeEvent @event) =>
        new MerchantBalanceChangedEntry {
                                            MerchantId = @event.MerchantId,
                                            EstateId = @event.EstateId,
                                            ChangeAmount = @event.Amount,
                                            DateTime = @event.WithdrawalDateTime,
                                            Reference = "Merchant Withdrawal",
                                            AggregateId = @event.MerchantId,
                                            OriginalEventId = @event.EventId,
                                            DebitOrCredit = "D"
                                        };

    private MerchantBalanceChangedEntry CreateTransactionBalanceEntry(MerchantBalanceState state, TransactionDomainEvents.TransactionHasBeenCompletedEvent @event) {
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

    private MerchantBalanceChangedEntry CreateManualDepositBalanceEntry(MerchantBalanceState state,
                                                                        ManualDepositMadeEvent @event) =>
        new MerchantBalanceChangedEntry {
                                            MerchantId = @event.MerchantId,
                                            EstateId = @event.EstateId,
                                            ChangeAmount = @event.Amount,
                                            DateTime = @event.DepositDateTime,
                                            Reference = "Merchant Deposit",
                                            AggregateId = @event.MerchantId,
                                            OriginalEventId = @event.EventId,
                                            DebitOrCredit = "C"
                                        };

    private MerchantBalanceChangedEntry CreateAutomaticDepositBalanceEntry(MerchantBalanceState state,
                                                                           AutomaticDepositMadeEvent @event) =>
        new MerchantBalanceChangedEntry {
                                            MerchantId = @event.MerchantId,
                                            EstateId = @event.EstateId,
                                            ChangeAmount = @event.Amount,
                                            DateTime = @event.DepositDateTime,
                                            Reference = "Merchant Deposit",
                                            AggregateId = @event.MerchantId,
                                            OriginalEventId = @event.EventId,
                                            DebitOrCredit = "C"
                                        };

    private MerchantBalanceChangedEntry CreateOpeningBalanceEntry(MerchantCreatedEvent @event) => 
        new MerchantBalanceChangedEntry {
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