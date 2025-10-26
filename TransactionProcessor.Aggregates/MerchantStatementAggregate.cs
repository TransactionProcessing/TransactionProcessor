using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using SimpleResults;
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
            if (aggregateId == Guid.Empty)
                throw new ArgumentNullException(nameof(aggregateId));

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

        public static Result RecordActivityDateOnStatement(this MerchantStatementAggregate aggregate,
                                                         Guid statementId,
                                                         DateTime statementDate,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         Guid merchantStatementForDateId,
                                                         DateTime activityDate)
        {
            if (aggregate.IsCreated == false) {
                if (statementId == Guid.Empty)
                    return Result.Invalid("Statement id cannot be an empty guid");
                if (estateId == Guid.Empty)
                    return Result.Invalid("Estate id cannot be an empty guid");
                if (merchantId == Guid.Empty)
                    return Result.Invalid("Merchant id cannot be an empty guid");

                StatementCreatedEvent statementCreatedEvent = new(statementId, estateId, merchantId, statementDate);
                aggregate.ApplyAndAppend(statementCreatedEvent);
            }

            if (aggregate.ActivityDates.SingleOrDefault(a => a.activityDate == activityDate) != default) {
                // Activity date already exists
                return Result.Success();
            }

            ActivityDateAddedToStatementEvent activityDateAddedToStatementEvent = new(statementId, estateId, merchantId, merchantStatementForDateId, activityDate);
            aggregate.ApplyAndAppend(activityDateAddedToStatementEvent);

            return Result.Success();
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

        public static Result BuildStatement(this MerchantStatementAggregate aggregate,
                                          DateTime builtDateTime,
                                          String statementData)
        {
            if (aggregate.IsBuilt)
                return Result.Success();
            
            Result result = aggregate.EnsureStatementHasBeenGenerated();
            if (result.IsFailed)
                return result;

            StatementBuiltEvent statementBuiltEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, builtDateTime, statementData);

            aggregate.ApplyAndAppend(statementBuiltEvent);

            return Result.Success();
        }

        public static Result EmailStatement(this MerchantStatementAggregate aggregate,
                                          DateTime emailedDateTime,
                                          Guid messageId) {
            if (aggregate.HasBeenEmailed) {
                return Result.Success();
            }

            Result result = aggregate.EnsureStatementHasBeenBuilt();
            if (result.IsFailed)
                return result;

            StatementEmailedEvent statementEmailedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, emailedDateTime, messageId);

            aggregate.ApplyAndAppend(statementEmailedEvent);

            return Result.Success();
        }

        public static Result GenerateStatement(this MerchantStatementAggregate aggregate,
                                             DateTime generatedDateTime)
        {
            if (aggregate.IsGenerated)
                return Result.Success();

            // Validate days have been added
            if (aggregate.MerchantStatementSummaries.Any() == false)
            {
                return Result.Invalid("Statement has no transactions or settled fees");
            }

            StatementGeneratedEvent statementGeneratedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, generatedDateTime);

            aggregate.ApplyAndAppend(statementGeneratedEvent);

            return Result.Success();
        }

        private static Result EnsureStatementHasBeenGenerated(this MerchantStatementAggregate aggregate) {
            if (aggregate.IsGenerated == false) {
                return Result.Invalid("Statement header has not been generated");
            }
            return Result.Success();
        }

        private static Result EnsureStatementHasBeenBuilt(this MerchantStatementAggregate aggregate)
        {
            if (aggregate.IsBuilt == false) {
                return Result.Invalid("Statement header has not been built");
            }
            return Result.Success();
        }

        public static Result AddDailySummaryRecord(this MerchantStatementAggregate aggregate, DateTime activityDate, 
                                                 Int32 numberOfTransactions, Decimal valueOfTransactions, 
                                                 Int32 numberOfSettledFees, Decimal valueOfSettledFees,
                                                 Int32 numberOfDepoits, Decimal valueOfDepoits,
                                                 Int32 numberOfWithdrawals, Decimal valueOfWithdrawals) {
            if (aggregate.MerchantStatementSummaries.Any(s => s.ActivityDate == activityDate)) {
                return Result.Success();
            }

            MerchantStatementDomainEvents.StatementSummaryForDateEvent statementSummaryForDateEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, activityDate,aggregate.MerchantStatementSummaries.Count +1
                ,numberOfTransactions, valueOfTransactions, numberOfSettledFees, valueOfSettledFees, numberOfDepoits, valueOfDepoits, numberOfWithdrawals, valueOfWithdrawals);
            aggregate.ApplyAndAppend(statementSummaryForDateEvent);

            return Result.Success();
        }
    }

    public record MerchantStatementSummary(DateTime ActivityDate, Int32 NumberOfTransactions, Decimal ValueOfTransactions, Int32 NumberOfSettledFees, Decimal ValueOfSettledFees);
}