using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using SimpleResults;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.DomainEvents;
using MerchantScheduleModel = TransactionProcessor.Models.MerchantSchedule.MerchantSchedule;
using MerchantScheduleMonthModel = TransactionProcessor.Models.MerchantSchedule.MerchantScheduleMonth;

namespace TransactionProcessor.Aggregates
{
    public static class MerchantScheduleAggregateExtensions
    {
        public static Result Create(this MerchantScheduleAggregate aggregate,
                                    Guid estateId,
                                    Guid merchantId,
                                    Int32 year,
                                    IEnumerable<MerchantScheduleMonthModel>? months)
        {
            Result result = ValidateCreateArguments(aggregate, estateId, merchantId, year);
            if (result.IsFailed)
                return result;

            if (aggregate.IsCreated)
                return Result.Success();

            MerchantScheduleDomainEvents.MerchantScheduleCreatedEvent merchantScheduleCreatedEvent =
                new(aggregate.AggregateId, estateId, merchantId, year);

            aggregate.ApplyAndAppend(merchantScheduleCreatedEvent);

            return aggregate.UpdateSchedule(months ?? []);
        }

        public static Result UpdateSchedule(this MerchantScheduleAggregate aggregate,
                                            IEnumerable<MerchantScheduleMonthModel> months)
        {
            Result result = aggregate.EnsureScheduleHasBeenCreated();
            if (result.IsFailed)
                return result;

            List<MerchantScheduleMonthModel> monthList = months?.ToList() ?? [];
            if (monthList.GroupBy(m => m.Month).Any(g => g.Count() > 1))
                return Result.Invalid("Each month can only be provided once when updating a merchant schedule");

            foreach (MerchantScheduleMonthModel month in monthList.OrderBy(m => m.Month))
            {
                Result monthResult = aggregate.SetMonthSchedule(month.Month, month.ClosedDays, month.OpenDays);
                if (monthResult.IsFailed)
                    return monthResult;
            }

            return Result.Success();
        }

        public static Result SetMonthSchedule(this MerchantScheduleAggregate aggregate,
                                              Int32 month,
                                              IEnumerable<Int32>? closedDays,
                                              IEnumerable<Int32>? openDays)
        {
            Result result = aggregate.EnsureScheduleHasBeenCreated();
            if (result.IsFailed)
                return result;

            result = ValidateMonth(aggregate.Year, month, closedDays, openDays);
            if (result.IsFailed)
                return result;

            Int32[] normalisedClosedDays = NormaliseDays(closedDays);
            Int32[] normalisedOpenDays = NormaliseDays(openDays);

            if (aggregate.Months.TryGetValue(month, out MerchantScheduleMonthModel? existingMonth))
            {
                if (existingMonth.ClosedDays.SequenceEqual(normalisedClosedDays) &&
                    existingMonth.OpenDays.SequenceEqual(normalisedOpenDays))
                    return Result.Success();
            }

            MerchantScheduleDomainEvents.MerchantScheduleMonthUpdatedEvent merchantScheduleMonthUpdatedEvent =
                new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, aggregate.Year, month, normalisedClosedDays, normalisedOpenDays);

            aggregate.ApplyAndAppend(merchantScheduleMonthUpdatedEvent);

            return Result.Success();
        }

        public static MerchantScheduleModel GetSchedule(this MerchantScheduleAggregate aggregate)
        {
            MerchantScheduleModel model = new()
            {
                MerchantScheduleId = aggregate.AggregateId,
                EstateId = aggregate.EstateId,
                MerchantId = aggregate.MerchantId,
                Year = aggregate.Year,
                Months = []
            };

            foreach (MerchantScheduleMonthModel month in aggregate.Months.Values.OrderBy(m => m.Month))
            {
                model.Months.Add(new MerchantScheduleMonthModel
                {
                    Month = month.Month,
                    ClosedDays = [.. month.ClosedDays],
                    OpenDays = [.. month.OpenDays]
                });
            }

            return model;
        }

        public static void PlayEvent(this MerchantScheduleAggregate aggregate,
                                     MerchantScheduleDomainEvents.MerchantScheduleCreatedEvent domainEvent)
        {
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.MerchantId = domainEvent.MerchantId;
            aggregate.Year = domainEvent.Year;
            aggregate.IsCreated = true;
        }

        public static void PlayEvent(this MerchantScheduleAggregate aggregate,
                                     MerchantScheduleDomainEvents.MerchantScheduleMonthUpdatedEvent domainEvent)
        {
            aggregate.Months[domainEvent.Month] = new MerchantScheduleMonthModel
            {
                Month = domainEvent.Month,
                ClosedDays = [.. domainEvent.ClosedDays],
                OpenDays = [.. domainEvent.OpenDays]
            };
        }

        private static Result EnsureScheduleHasBeenCreated(this MerchantScheduleAggregate aggregate)
        {
            if (aggregate.IsCreated == false)
                return Result.Invalid("Merchant schedule has not been created");

            return Result.Success();
        }

        private static Result ValidateCreateArguments(MerchantScheduleAggregate aggregate,
                                                      Guid estateId,
                                                      Guid merchantId,
                                                      Int32 year)
        {
            if (aggregate.AggregateId == Guid.Empty)
                return Result.Invalid("Merchant schedule id must be provided");

            if (estateId == Guid.Empty)
                return Result.Invalid("Estate id must be provided when creating a merchant schedule");

            if (merchantId == Guid.Empty)
                return Result.Invalid("Merchant id must be provided when creating a merchant schedule");

            if (year < 1)
                return Result.Invalid("A valid year must be provided when creating a merchant schedule");

            return Result.Success();
        }

        private static Result ValidateMonth(Int32 year,
                                            Int32 month,
                                            IEnumerable<Int32>? closedDays,
                                            IEnumerable<Int32>? openDays)
        {
            if (month is < 1 or > 12)
                return Result.Invalid("A valid month must be provided when updating a merchant schedule");

            Int32 daysInMonth = DateTime.DaysInMonth(year, month);
            Int32[] normalisedClosedDays = NormaliseDays(closedDays);
            Int32[] normalisedOpenDays = NormaliseDays(openDays);

            if (normalisedClosedDays.Any(day => day < 1 || day > daysInMonth) ||
                normalisedOpenDays.Any(day => day < 1 || day > daysInMonth))
                return Result.Invalid($"Only days between 1 and {daysInMonth} can be supplied for {year}-{month:D2}");

            if (normalisedClosedDays.Intersect(normalisedOpenDays).Any())
                return Result.Invalid("A merchant schedule day cannot be marked as both open and closed");

            return Result.Success();
        }

        private static Int32[] NormaliseDays(IEnumerable<Int32>? days) =>
            (days ?? []).Distinct().OrderBy(day => day).ToArray();
    }

    public record MerchantScheduleAggregate : Aggregate
    {
        internal readonly Dictionary<Int32, MerchantScheduleMonthModel> Months;

        [ExcludeFromCodeCoverage]
        public MerchantScheduleAggregate()
        {
            this.Months = new Dictionary<Int32, MerchantScheduleMonthModel>();
        }

        private MerchantScheduleAggregate(Guid aggregateId)
        {
            if (aggregateId == Guid.Empty)
                throw new ArgumentNullException(nameof(aggregateId));

            this.AggregateId = aggregateId;
            this.Months = new Dictionary<Int32, MerchantScheduleMonthModel>();
        }

        public Guid EstateId { get; internal set; }

        public Guid MerchantId { get; internal set; }

        public Int32 Year { get; internal set; }

        public Boolean IsCreated { get; internal set; }

        public static MerchantScheduleAggregate Create(Guid aggregateId)
        {
            return new MerchantScheduleAggregate(aggregateId);
        }

        public override void PlayEvent(IDomainEvent domainEvent) => MerchantScheduleAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return new
            {
                MerchantScheduleId = this.AggregateId,
                this.EstateId,
                this.MerchantId,
                this.Year
            };
        }
    }
}
