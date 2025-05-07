using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TransactionProcessor.Aggregates.Models;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.Aggregates
{
    public record MerchantStatementAggregate : Aggregate
    {
        #region Fields

        internal DateTime StatementDate;

        internal DateTime EmailedDateTime;

        internal Guid EmailMessageId;

        internal Guid EstateId;

        internal DateTime GeneratedDateTime;

        internal Boolean HasBeenEmailed;

        internal Boolean IsCreated;

        internal Boolean IsGenerated;

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

        //public static void PlayEvent(this MerchantStatementAggregate aggregate, MerchantStatementDomainEvents.StatementEmailedEvent domainEvent)
        //{
        //    aggregate.HasBeenEmailed = true;
        //    aggregate.EmailedDateTime = domainEvent.DateEmailed;
        //    aggregate.EmailMessageId = domainEvent.MessageId;
        //}

        public static void PlayEvent(this MerchantStatementAggregate aggregate,
                                     MerchantStatementDomainEvents.ActivityDateAddedToStatementEvent domainEvent) {
            aggregate.ActivityDates.Add((domainEvent.MerchantStatementForDateId, domainEvent.ActivityDate));
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
            };

            foreach ((Guid merchantStatementForDateId, DateTime activityDate) aggregateActivityDate in aggregate.ActivityDates) {
                merchantStatement.AddStatementActivityDate(aggregateActivityDate.merchantStatementForDateId, aggregateActivityDate.activityDate);
            }

            return merchantStatement;
        }

        //public static void EmailStatement(this MerchantStatementAggregate aggregate,
        //                                  DateTime emailedDateTime,
        //                                  Guid messageId)
        //{
        //    aggregate.EnsureStatementHasBeenCreated();
        //    aggregate.EnsureStatementHasBeenGenerated();

        //    MerchantStatementDomainEvents.StatementEmailedEvent statementEmailedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, emailedDateTime, messageId);

        //    aggregate.ApplyAndAppend(statementEmailedEvent);
        //}

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

        //private static void EnsureStatementHasBeenCreated(this MerchantStatementAggregate aggregate)
        //{
        //    if (aggregate.IsCreated == false)
        //    {
        //        throw new InvalidOperationException("Statement header has not been created");
        //    }
        //}

        //private static void EnsureStatementHasBeenGenerated(this MerchantStatementAggregate aggregate)
        //{
        //    if (aggregate.IsGenerated == false)
        //    {
        //        throw new InvalidOperationException("Statement header has not been generated");
        //    }
        //}

        private static void EnsureStatementHasNotAlreadyBeenGenerated(this MerchantStatementAggregate aggregate)
        {
            if (aggregate.IsGenerated)
            {
                throw new InvalidOperationException("Statement header has already been generated");
            }
        }

        public static void AddDailySummaryRecord(this MerchantStatementAggregate aggregate, DateTime activityDate, Int32 numberOfTransactions, Decimal valueOfTransactions, Int32 numberOfSettledFees, Decimal valueOfSettledFees) {
            if (aggregate.MerchantStatementSummaries.Any(s => s.ActivityDate == activityDate)) {
                throw new InvalidOperationException($"Summary Data for Activity Date {activityDate:yyyy-MM-dd} already exists");
            }
            MerchantStatementDomainEvents.StatementSummaryForDateEvent statementSummaryForDateEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, activityDate,aggregate.MerchantStatementSummaries.Count +1
                ,numberOfTransactions, valueOfTransactions, numberOfSettledFees, valueOfSettledFees);
            aggregate.ApplyAndAppend(statementSummaryForDateEvent);
        }
    }

    public record MerchantStatementSummary(DateTime ActivityDate, Int32 NumberOfTransactions, Decimal ValueOfTransactions, Int32 NumberOfSettledFees, Decimal ValueOfSettledFees);
}