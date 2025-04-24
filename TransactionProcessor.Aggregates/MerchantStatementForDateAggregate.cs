using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.Aggregates.Models;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.Aggregates;

public static class MerchantStatementForDateAggregateExtensions
{
    public static void AddSettledFeeToStatement(this MerchantStatementForDateAggregate aggregate,
                                                Guid merchantStatementId,
                                                DateTime statementDate,
                                                Guid eventId,
                                                Guid estateId,
                                                Guid merchantId,
                                                SettledFee settledFee) {
        // Create statement id required
        aggregate.CreateStatementForDate(merchantStatementId, settledFee.DateTime.Date, statementDate, estateId, merchantId);

        MerchantStatementForDateDomainEvents.SettledFeeAddedToStatementForDateEvent settledFeeAddedToStatementEvent = new(aggregate.AggregateId, eventId, aggregate.EstateId, aggregate.MerchantId, settledFee.SettledFeeId, settledFee.TransactionId, settledFee.DateTime, settledFee.Amount);

        aggregate.ApplyAndAppend(settledFeeAddedToStatementEvent);
    }

    public static void AddTransactionToStatement(this MerchantStatementForDateAggregate aggregate,
                                                 Guid merchantStatementId,
                                                 DateTime statementDate,
                                                 Guid eventId,
                                                 Guid estateId,
                                                 Guid merchantId,
                                                 Transaction transaction)
    {
        // Create statement if required
        aggregate.CreateStatementForDate(merchantStatementId, transaction.DateTime.Date, statementDate, estateId, merchantId);

        MerchantStatementForDateDomainEvents.TransactionAddedToStatementForDateEvent transactionAddedToStatementEvent = new(aggregate.AggregateId,
                                                                                                                 eventId,
                                                                                                                 aggregate.EstateId,
                                                                                                                 aggregate.MerchantId,
                                                                                                                 transaction.TransactionId,
                                                                                                                 transaction.DateTime,
                                                                                                                 transaction.Amount);

        aggregate.ApplyAndAppend(transactionAddedToStatementEvent);
    }

    private static void CreateStatementForDate(this MerchantStatementForDateAggregate aggregate,
                                        Guid merchantStatementId,
                                        DateTime activityDate,
                                        DateTime statementDate,
                                        Guid estateId,
                                        Guid merchantId)
    {
        if (aggregate.IsCreated == false)
        {
            Guard.ThrowIfInvalidGuid(merchantStatementId, nameof(merchantStatementId));
            Guard.ThrowIfInvalidGuid(estateId, nameof(estateId));
            Guard.ThrowIfInvalidGuid(merchantId, nameof(merchantId));

            MerchantStatementForDateDomainEvents.StatementCreatedForDateEvent statementCreatedForDateEvent = new(aggregate.AggregateId,
                activityDate, statementDate, merchantStatementId, estateId, merchantId);

            aggregate.ApplyAndAppend(statementCreatedForDateEvent);
        }
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
    }

    private MerchantStatementForDateAggregate(Guid aggregateId)
    {
        Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

        this.AggregateId = aggregateId;
        this.Transactions = new List<Transaction>();
        this.SettledFees = new List<SettledFee>();
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