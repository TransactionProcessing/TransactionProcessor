using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents;

[ExcludeFromCodeCoverage]
public class MerchantScheduleDomainEvents {
    public record MerchantScheduleCreatedEvent(Guid MerchantScheduleId,
                                               Guid EstateId,
                                               Guid MerchantId,
                                               Int32 Year) : DomainEvent(MerchantScheduleId, Guid.NewGuid());

    public record MerchantScheduleMonthUpdatedEvent(Guid MerchantScheduleId,
                                                    Guid EstateId,
                                                    Guid MerchantId,
                                                    Int32 Year,
                                                    Int32 Month,
                                                    Int32[] ClosedDays,
                                                    Int32[] OpenDays) : DomainEvent(MerchantScheduleId, Guid.NewGuid());
}
