using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using System.Diagnostics.CodeAnalysis;
using SimpleResults;
using TransactionProcessor.Aggregates.Models;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Merchant;
using Deposit = TransactionProcessor.Models.Merchant.Deposit;
using Withdrawal = TransactionProcessor.Models.Merchant.Withdrawal;

namespace TransactionProcessor.Aggregates;

public static class MerchantStatementForDateAggregateExtensions
{
    public static Result AddSettledFeeToStatement(this MerchantStatementForDateAggregate aggregate,
                                                Guid merchantStatementId,
                                                DateTime statementDate,
                                                Guid eventId,
                                                Guid estateId,
                                                Guid merchantId,
                                                SettledFee settledFee) {
        // Create statement id required
        Result result = aggregate.CreateStatementForDate(merchantStatementId, settledFee.DateTime.Date, statementDate, estateId, merchantId);
        if (result.IsFailed)
            return result;

        MerchantStatementForDateDomainEvents.SettledFeeAddedToStatementForDateEvent settledFeeAddedToStatementEvent = new(aggregate.AggregateId, eventId, aggregate.EstateId, aggregate.MerchantId, merchantStatementId, settledFee.SettledFeeId, settledFee.TransactionId, settledFee.DateTime, settledFee.Amount);

        aggregate.ApplyAndAppend(settledFeeAddedToStatementEvent);

        return Result.Success();
    }

    public static Result AddDepositToStatement(this MerchantStatementForDateAggregate aggregate,
                                               Guid merchantStatementId,
                                               DateTime statementDate,
                                               Guid eventId,
                                               Guid estateId,
                                               Guid merchantId,
                                               Deposit deposit)
    {
        // Create statement id required
        Result result = aggregate.CreateStatementForDate(merchantStatementId, deposit.DepositDateTime.Date, statementDate, estateId, merchantId);
        if (result.IsFailed)
            return result;

        MerchantStatementForDateDomainEvents.DepositAddedToStatementForDateEvent depositAddedToStatementEvent = new(aggregate.AggregateId, eventId, aggregate.EstateId, aggregate.MerchantId, merchantStatementId, deposit.DepositId, deposit.DepositDateTime, deposit.Amount);

        aggregate.ApplyAndAppend(depositAddedToStatementEvent);

        return Result.Success();
    }

    public static Result AddWithdrawalToStatement(this MerchantStatementForDateAggregate aggregate,
                                                  Guid merchantStatementId,
                                                  DateTime statementDate,
                                                  Guid eventId,
                                                  Guid estateId,
                                                  Guid merchantId,
                                                  Withdrawal withdrawal)
    {
        // Create statement id required
        Result result = aggregate.CreateStatementForDate(merchantStatementId, withdrawal.WithdrawalDateTime.Date, statementDate, estateId, merchantId);
        if (result.IsFailed)
            return result;

        MerchantStatementForDateDomainEvents.WithdrawalAddedToStatementForDateEvent withdrawalAddedToStatementEvent = new(aggregate.AggregateId, eventId, aggregate.EstateId, aggregate.MerchantId, merchantStatementId, withdrawal.WithdrawalId, withdrawal.WithdrawalDateTime, withdrawal.Amount);

        aggregate.ApplyAndAppend(withdrawalAddedToStatementEvent);

        return Result.Success();
    }

    public static Result AddTransactionToStatement(this MerchantStatementForDateAggregate aggregate,
                                                   Guid merchantStatementId,
                                                   DateTime statementDate,
                                                   Guid eventId,
                                                   Guid estateId,
                                                   Guid merchantId,
                                                   Transaction transaction)
    {
        // Create statement if required
        Result result = aggregate.CreateStatementForDate(merchantStatementId, transaction.DateTime.Date, statementDate, estateId, merchantId);
        if (result.IsFailed)
            return result;

        MerchantStatementForDateDomainEvents.TransactionAddedToStatementForDateEvent transactionAddedToStatementEvent = new(aggregate.AggregateId,
                                                                                                                 eventId,
                                                                                                                 aggregate.EstateId,
                                                                                                                 aggregate.MerchantId,
                                                                                                                 merchantStatementId,
                                                                                                                 transaction.TransactionId,
                                                                                                                 transaction.DateTime,
                                                                                                                 transaction.Amount);

        aggregate.ApplyAndAppend(transactionAddedToStatementEvent);

        return Result.Success();
    }

    private static Result CreateStatementForDate(this MerchantStatementForDateAggregate aggregate,
                                        Guid merchantStatementId,
                                        DateTime activityDate,
                                        DateTime statementDate,
                                        Guid estateId,
                                        Guid merchantId)
    {
        if (aggregate.IsCreated)
            return Result.Success();
        
        if (merchantStatementId == Guid.Empty)
            return Result.Invalid("Merchant Statement Id cannot be an empty Guid");
        if (estateId == Guid.Empty)
            return Result.Invalid("Estate Id cannot be an empty Guid");
        if (merchantId == Guid.Empty)
            return Result.Invalid("Merchant Id cannot be an empty Guid");

        MerchantStatementForDateDomainEvents.StatementCreatedForDateEvent statementCreatedForDateEvent = new(aggregate.AggregateId,
            activityDate, statementDate, merchantStatementId, estateId, merchantId);

        aggregate.ApplyAndAppend(statementCreatedForDateEvent);
        return Result.Success();
    }
    
    public static void PlayEvent(this MerchantStatementForDateAggregate aggregate, MerchantStatementForDateDomainEvents.StatementCreatedForDateEvent domainEvent)
    {
        aggregate.IsCreated = true;
        aggregate.EstateId = domainEvent.EstateId;
        aggregate.MerchantId = domainEvent.MerchantId;
        aggregate.ActivityDate = domainEvent.ActivityDate;
        aggregate.MerchantStatementId = domainEvent.MerchantStatementId;
        aggregate.StatementDate = domainEvent.MerchantStatementDate;
    }
    public static void PlayEvent(this MerchantStatementForDateAggregate aggregate, MerchantStatementForDateDomainEvents.TransactionAddedToStatementForDateEvent domainEvent)
    {
        aggregate.Transactions.Add(new Transaction(domainEvent.TransactionId, domainEvent.TransactionDateTime, domainEvent.TransactionValue));
    }

    public static void PlayEvent(this MerchantStatementForDateAggregate aggregate, MerchantStatementForDateDomainEvents.SettledFeeAddedToStatementForDateEvent domainEvent)
    {
        aggregate.SettledFees.Add(new SettledFee(domainEvent.SettledFeeId, domainEvent.TransactionId, domainEvent.SettledDateTime, domainEvent.SettledValue));
    }

    public static void PlayEvent(this MerchantStatementForDateAggregate aggregate, MerchantStatementForDateDomainEvents.DepositAddedToStatementForDateEvent domainEvent)
    {
        aggregate.Deposits.Add(new Deposit {
            DepositDateTime = domainEvent.DepositDateTime,
            Amount = domainEvent.DepositAmount,
            DepositId = domainEvent.DepositId,
        });
    }

    public static void PlayEvent(this MerchantStatementForDateAggregate aggregate, MerchantStatementForDateDomainEvents.WithdrawalAddedToStatementForDateEvent domainEvent)
    {
        aggregate.Withdrawals.Add(new Withdrawal()
        {
            WithdrawalDateTime = domainEvent.WithdrawalDateTime,
            Amount = domainEvent.WithdrawalAmount,
            WithdrawalId = domainEvent.WithdrawalId,
        });
    }

    public static MerchantStatementForDate GetStatement(this MerchantStatementForDateAggregate aggregate, Boolean includeStatementLines = false)
    {
        MerchantStatementForDate merchantStatement = new MerchantStatementForDate
        {
            EstateId = aggregate.EstateId,
            MerchantId = aggregate.MerchantId,
            MerchantStatementId = aggregate.MerchantStatementId,
            IsCreated = aggregate.IsCreated,
            ActivityDate = aggregate.ActivityDate,
            MerchantStatementForDateId = aggregate.AggregateId,
            StatementDate = aggregate.StatementDate
        };

        if (includeStatementLines)
        {
            foreach (Transaction transaction in aggregate.Transactions)
            {
                merchantStatement.AddStatementLine(new MerchantStatementLine
                {
                    Amount = transaction.Amount,
                    DateTime = transaction.DateTime,
                    Description = string.Empty,
                    LineType = 1 // Transaction
                });
            }

            foreach (SettledFee settledFee in aggregate.SettledFees)
            {
                merchantStatement.AddStatementLine(new MerchantStatementLine
                {
                    Amount = settledFee.Amount,
                    DateTime = settledFee.DateTime,
                    Description = string.Empty,
                    LineType = 2 // Settled Fee
                });
            }

            foreach (Deposit deposit in aggregate.Deposits)
            {
                merchantStatement.AddStatementLine(new MerchantStatementLine
                {
                    Amount = deposit.Amount,
                    DateTime = deposit.DepositDateTime,
                    Description = string.Empty,
                    LineType = 3 // Deposit
                });
            }

            foreach (Withdrawal withdrawal in aggregate.Withdrawals)
            {
                merchantStatement.AddStatementLine(new MerchantStatementLine
                {
                    Amount = withdrawal.Amount,
                    DateTime = withdrawal.WithdrawalDateTime,
                    Description = string.Empty,
                    LineType = 4 // Withdrawal
                });
            }
        }

        return merchantStatement;
    }
}

public record MerchantStatementForDateAggregate : Aggregate
{
    #region Fields

    internal DateTime ActivityDate;
    internal Guid EstateId;
    internal Boolean IsCreated;
    internal Guid MerchantId;
    internal readonly List<SettledFee> SettledFees;
    internal readonly List<Transaction> Transactions;
    internal readonly List<Deposit> Deposits;
    internal readonly List<Withdrawal> Withdrawals;
    internal Guid MerchantStatementId;
    internal DateTime StatementDate;
    #endregion

    #region Constructors

    [ExcludeFromCodeCoverage]
    public MerchantStatementForDateAggregate()
    {
        // Nothing here
        this.Transactions = new List<Transaction>();
        this.SettledFees = new List<SettledFee>();
        this.Deposits = new List<Deposit>();
        this.Withdrawals= new List<Withdrawal>();
    }

    private MerchantStatementForDateAggregate(Guid aggregateId)
    {
        if (aggregateId == Guid.Empty)
            throw new ArgumentNullException(nameof(aggregateId));

        this.AggregateId = aggregateId;
        this.Transactions = new List<Transaction>();
        this.SettledFees = new List<SettledFee>();
        this.Deposits = new List<Deposit>(); 
        this.Withdrawals = new List<Withdrawal>();
    }

    #endregion

    #region Methods

    public static MerchantStatementForDateAggregate Create(Guid aggregateId)
    {
        return new MerchantStatementForDateAggregate(aggregateId);
    }

    public override void PlayEvent(IDomainEvent domainEvent) => MerchantStatementForDateAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

    [ExcludeFromCodeCoverage]
    protected override Object GetMetadata()
    {
        return null;
    }

    #endregion
}