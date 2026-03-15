using Shared.DomainDrivenDesign.EventSourcing;
using Shouldly;
using SimpleResults;
using System.Reflection;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.MerchantSchedule;

namespace TransactionProcessor.Aggregates.Tests
{
    public class MerchantScheduleAggregateTests
    {
        private static readonly Guid MerchantScheduleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid EstateId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid MerchantId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        [Fact]
        public void MerchantScheduleAggregate_CanBeCreated_IsCreated()
        {
            MerchantScheduleAggregate aggregate = MerchantScheduleAggregate.Create(MerchantScheduleId);

            aggregate.AggregateId.ShouldBe(MerchantScheduleId);
            aggregate.IsCreated.ShouldBeFalse();
        }

        [Fact]
        public void MerchantScheduleAggregate_Create_WithInitialMonths_IsCreated()
        {
            MerchantScheduleAggregate aggregate = MerchantScheduleAggregate.Create(MerchantScheduleId);

            Result result = aggregate.Create(EstateId,
                                             MerchantId,
                                             2026,
                                             [
                                                 new MerchantScheduleMonth
                                                 {
                                                     Month = 1,
                                                     ClosedDays = [1, 26],
                                                     OpenDays = [4]
                                                 }
                                             ]);

            result.IsSuccess.ShouldBeTrue();
            aggregate.IsCreated.ShouldBeTrue();
            aggregate.EstateId.ShouldBe(EstateId);
            aggregate.MerchantId.ShouldBe(MerchantId);
            aggregate.Year.ShouldBe(2026);

            MerchantSchedule model = aggregate.GetSchedule();
            model.Months.ShouldHaveSingleItem();
            model.Months.Single().Month.ShouldBe(1);
            model.Months.Single().ClosedDays.ShouldBe([1, 26]);
            model.Months.Single().OpenDays.ShouldBe([4]);
        }

        [Fact]
        public void MerchantScheduleAggregate_Create_InvalidYear_ErrorReturned()
        {
            MerchantScheduleAggregate aggregate = MerchantScheduleAggregate.Create(MerchantScheduleId);

            Result result = aggregate.Create(EstateId, MerchantId, 1899, []);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("A valid year must be provided when creating a merchant schedule");
        }

        [Fact]
        public void MerchantScheduleAggregate_SetMonthSchedule_WhenChanged_EmitsMonthEvent()
        {
            MerchantScheduleAggregate aggregate = MerchantScheduleAggregate.Create(MerchantScheduleId);
            aggregate.Create(EstateId, MerchantId, 2026, []);

            Result result = aggregate.SetMonthSchedule(12, [25, 26], [24]);

            result.IsSuccess.ShouldBeTrue();

            List<IDomainEvent> pendingEvents = GetPendingEvents(aggregate);
            pendingEvents.Last().ShouldBeOfType<MerchantScheduleDomainEvents.MerchantScheduleMonthUpdatedEvent>();

            MerchantScheduleDomainEvents.MerchantScheduleMonthUpdatedEvent updatedEvent =
                (MerchantScheduleDomainEvents.MerchantScheduleMonthUpdatedEvent)pendingEvents.Last();
            updatedEvent.Month.ShouldBe(12);
            updatedEvent.ClosedDays.ShouldBe([25, 26]);
            updatedEvent.OpenDays.ShouldBe([24]);
        }

        [Fact]
        public void MerchantScheduleAggregate_SetMonthSchedule_SameValue_NoAdditionalEventRaised()
        {
            MerchantScheduleAggregate aggregate = MerchantScheduleAggregate.Create(MerchantScheduleId);
            aggregate.Create(EstateId, MerchantId, 2026, []);
            aggregate.SetMonthSchedule(5, [1, 2], [3]);

            List<IDomainEvent> pendingEvents = GetPendingEvents(aggregate);
            Int32 originalEventCount = pendingEvents.Count;

            Result result = aggregate.SetMonthSchedule(5, [2, 1], [3, 3]);

            result.IsSuccess.ShouldBeTrue();
            pendingEvents.Count.ShouldBe(originalEventCount);
        }

        [Fact]
        public void MerchantScheduleAggregate_SetMonthSchedule_InvalidDay_ErrorReturned()
        {
            MerchantScheduleAggregate aggregate = MerchantScheduleAggregate.Create(MerchantScheduleId);
            aggregate.Create(EstateId, MerchantId, 2025, []);

            Result result = aggregate.SetMonthSchedule(2, [29], []);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("Only days between 1 and 28 can be supplied for 2025-02");
        }

        [Fact]
        public void MerchantScheduleAggregate_UpdateSchedule_DuplicateMonth_ErrorReturned()
        {
            MerchantScheduleAggregate aggregate = MerchantScheduleAggregate.Create(MerchantScheduleId);
            aggregate.Create(EstateId, MerchantId, 2026, []);

            Result result = aggregate.UpdateSchedule([
                new MerchantScheduleMonth { Month = 6, ClosedDays = [1] },
                new MerchantScheduleMonth { Month = 6, OpenDays = [2] }
            ]);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("Each month can only be provided once when updating a merchant schedule");
        }

        private static List<IDomainEvent> GetPendingEvents(MerchantScheduleAggregate aggregate)
        {
            PropertyInfo property = aggregate.GetType().GetProperty("PendingEvents", BindingFlags.Instance | BindingFlags.NonPublic);
            property.ShouldNotBeNull();

            Object value = property.GetValue(aggregate);
            value.ShouldNotBeNull();

            return (List<IDomainEvent>)value;
        }
    }
}
