using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TransactionProcessor.Aggregates.Models;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Merchant;
using static TransactionProcessor.DomainEvents.MerchantStatementDomainEvents;

namespace TransactionProcessor.Aggregates
{
    public record MerchantStatementAggregate : Aggregate
    {
        #region Fields

        internal DateTime StatementDate;

        internal DateTime EmailedDateTime;
        internal DateTime BuiltDateTime;

        internal Guid EmailMessageId;

        internal Guid EstateId;

        internal DateTime GeneratedDateTime;

        internal Boolean HasBeenEmailed;

        internal Boolean IsCreated;

        internal Boolean IsGenerated;
        internal Boolean IsBuilt;

        internal Guid MerchantId;

        internal List<(Guid merchantStatementForDateId, DateTime activityDate)> ActivityDates;

        internal List<MerchantStatementSummary> MerchantStatementSummaries;
        #endregion

        #region Constructors

        [ExcludeFromCodeCoverage]
        public MerchantStatementAggregate()
        {
            // Nothing here
            this.ActivityDates = new();
            this.MerchantStatementSummaries = new();
        }

        private MerchantStatementAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.ActivityDates = new();
            this.MerchantStatementSummaries = new();
        }

        #endregion

        #region Methods

        public static MerchantStatementAggregate Create(Guid aggregateId)
        {
            return new MerchantStatementAggregate(aggregateId);
        }

        public override void PlayEvent(IDomainEvent domainEvent) => MerchantStatementAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return null;
        }

        #endregion

    }

    public static class MerchantStatementAggregateExtensions
    {
        public static void PlayEvent(this MerchantStatementAggregate aggregate, MerchantStatementDomainEvents.StatementCreatedEvent domainEvent)
        {
            aggregate.IsCreated = true;
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.MerchantId = domainEvent.MerchantId;
            aggregate.StatementDate = domainEvent.StatementDate;
        }

        public static void PlayEvent(this MerchantStatementAggregate aggregate, MerchantStatementDomainEvents.StatementGeneratedEvent domainEvent)
        {
            aggregate.IsGenerated = true;
            aggregate.GeneratedDateTime = domainEvent.DateGenerated;
        }

        public static void PlayEvent(this MerchantStatementAggregate aggregate, MerchantStatementDomainEvents.StatementEmailedEvent domainEvent)
        {
            aggregate.HasBeenEmailed = true;
            aggregate.EmailedDateTime = domainEvent.DateEmailed;
            aggregate.EmailMessageId = domainEvent.MessageId;
        }

        public static void PlayEvent(this MerchantStatementAggregate aggregate,
                                     MerchantStatementDomainEvents.ActivityDateAddedToStatementEvent domainEvent) {
            aggregate.ActivityDates.Add((domainEvent.MerchantStatementForDateId, domainEvent.ActivityDate));
        }

        public static void PlayEvent(this MerchantStatementAggregate aggregate,
                                     MerchantStatementDomainEvents.StatementBuiltEvent domainEvent) {
            aggregate.BuiltDateTime = domainEvent.DateBuilt;
            aggregate.IsBuilt = true;
        }

        public static void PlayEvent(this MerchantStatementAggregate aggregate,
                                     MerchantStatementDomainEvents.StatementSummaryForDateEvent domainEvent)
        {
            aggregate.MerchantStatementSummaries.Add(new MerchantStatementSummary(domainEvent.ActivityDate,domainEvent.NumberOfTransactions,domainEvent.ValueOfTransactions,
                domainEvent.NumberOfSettledFees,domainEvent.ValueOfTransactions));
        }

        public static void RecordActivityDateOnStatement(this MerchantStatementAggregate aggregate,
                                                         Guid statementId,
                                                         DateTime statementDate,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         Guid merchantStatementForDateId,
                                                         DateTime activityDate)
        {
            if (aggregate.IsCreated == false)
            {
                Guard.ThrowIfInvalidGuid(statementId, nameof(statementId));
                Guard.ThrowIfInvalidGuid(estateId, nameof(estateId));
                Guard.ThrowIfInvalidGuid(merchantId, nameof(merchantId));

                MerchantStatementDomainEvents.StatementCreatedEvent statementCreatedEvent = new(statementId, estateId, merchantId, statementDate);

                aggregate.ApplyAndAppend(statementCreatedEvent);
            }

            if (aggregate.ActivityDates.SingleOrDefault(a => a.activityDate == activityDate) != default)
            {
                // Activity date already exists
                return;
            }

            MerchantStatementDomainEvents.ActivityDateAddedToStatementEvent activityDateAddedToStatementEvent = new(statementId, estateId, merchantId, merchantStatementForDateId, activityDate);
            aggregate.ApplyAndAppend(activityDateAddedToStatementEvent);
        }

        public static MerchantStatement GetStatement(this MerchantStatementAggregate aggregate)
        {
            MerchantStatement merchantStatement = new()
            {
                EstateId = aggregate.EstateId,
                MerchantId = aggregate.MerchantId,
                IsCreated = aggregate.IsCreated,
                StatementDate = aggregate.StatementDate,
                MerchantStatementId = aggregate.AggregateId,
                IsGenerated = aggregate.IsGenerated,
                BuiltDateTime = aggregate.BuiltDateTime,
                HasBeenEmailed = aggregate.HasBeenEmailed,
                IsBuilt = aggregate.IsBuilt
            };

            foreach ((Guid merchantStatementForDateId, DateTime activityDate) aggregateActivityDate in aggregate.ActivityDates) {
                merchantStatement.AddStatementActivityDate(aggregateActivityDate.merchantStatementForDateId, aggregateActivityDate.activityDate);
            }

            foreach (MerchantStatementSummary aggregateMerchantStatementSummary in aggregate.MerchantStatementSummaries) {
                merchantStatement.AddStatementDailySummary(aggregateMerchantStatementSummary.ActivityDate, aggregateMerchantStatementSummary.ValueOfTransactions, aggregateMerchantStatementSummary.ValueOfSettledFees);
            }

            return merchantStatement;
        }

        public static void BuildStatement(this MerchantStatementAggregate aggregate,
                                          DateTime builtDateTime,
                                          String statementData)
        {
            aggregate.EnsureStatementHasBeenGenerated();
            aggregate.EnsureStatementHasNotAlreadyBeenBuilt();

            MerchantStatementDomainEvents.StatementBuiltEvent statementBuiltEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, builtDateTime, statementData);

            aggregate.ApplyAndAppend(statementBuiltEvent);
        }

        public static void EmailStatement(this MerchantStatementAggregate aggregate,
                                          DateTime emailedDateTime,
                                          Guid messageId) {
            aggregate.EnsureStatementHasBeenBuilt();
            aggregate.EnsureStatementHasNotAlreadyBeenEmailed();
            StatementEmailedEvent statementEmailedEvent = new StatementEmailedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, emailedDateTime, messageId);

            aggregate.ApplyAndAppend(statementEmailedEvent);
        }

        public static void GenerateStatement(this MerchantStatementAggregate aggregate,
                                             DateTime generatedDateTime)
        {
            aggregate.EnsureStatementHasNotAlreadyBeenGenerated();

            // Validate days have been added
            if (aggregate.MerchantStatementSummaries.Any() == false)
            {
                throw new InvalidOperationException("Statement has no transactions or settled fees");
            }

            MerchantStatementDomainEvents.StatementGeneratedEvent statementGeneratedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, generatedDateTime);

            aggregate.ApplyAndAppend(statementGeneratedEvent);
        }

        private static void EnsureStatementHasBeenGenerated(this MerchantStatementAggregate aggregate)
        {
            if (aggregate.IsGenerated == false)
            {
                throw new InvalidOperationException("Statement header has not been generated");
            }
        }

        private static void EnsureStatementHasBeenBuilt(this MerchantStatementAggregate aggregate)
        {
            if (aggregate.IsBuilt == false)
            {
                throw new InvalidOperationException("Statement header has not been built");
            }
        }

        private static void EnsureStatementHasNotAlreadyBeenGenerated(this MerchantStatementAggregate aggregate)
        {
            if (aggregate.IsGenerated)
            {
                throw new InvalidOperationException("Statement header has already been generated");
            }
        }

        private static void EnsureStatementHasNotAlreadyBeenBuilt(this MerchantStatementAggregate aggregate)
        {
            if (aggregate.IsBuilt)
            {
                throw new InvalidOperationException("Statement header has already been built");
            }
        }

        private static void EnsureStatementHasNotAlreadyBeenEmailed(this MerchantStatementAggregate aggregate)
        {
            if (aggregate.HasBeenEmailed)
            {
                throw new InvalidOperationException("Statement header has already been emailed");
            }
        }

        public static void AddDailySummaryRecord(this MerchantStatementAggregate aggregate, DateTime activityDate, 
                                                 Int32 numberOfTransactions, Decimal valueOfTransactions, 
                                                 Int32 numberOfSettledFees, Decimal valueOfSettledFees,
                                                 Int32 numberOfDepoits, Decimal valueOfDepoits,
                                                 Int32 numberOfWithdrawals, Decimal valueOfWithdrawals) {
            if (aggregate.MerchantStatementSummaries.Any(s => s.ActivityDate == activityDate)) {
                throw new InvalidOperationException($"Summary Data for Activity Date {activityDate:yyyy-MM-dd} already exists");
            }

            // TODO: should this check the date has been added to the statement, before allowing the summary?

            MerchantStatementDomainEvents.StatementSummaryForDateEvent statementSummaryForDateEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, activityDate,aggregate.MerchantStatementSummaries.Count +1
                ,numberOfTransactions, valueOfTransactions, numberOfSettledFees, valueOfSettledFees, numberOfDepoits, valueOfDepoits, numberOfWithdrawals, valueOfWithdrawals);
            aggregate.ApplyAndAppend(statementSummaryForDateEvent);
        }
    }

    public record MerchantStatementSummary(DateTime ActivityDate, Int32 NumberOfTransactions, Decimal ValueOfTransactions, Int32 NumberOfSettledFees, Decimal ValueOfSettledFees);
}