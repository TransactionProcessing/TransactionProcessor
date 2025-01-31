using Shared.DomainDrivenDesign.EventSourcing;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DomainEvents;

public class EstateDomainEvents {
    [ExcludeFromCodeCoverage]
    public record EstateCreatedEvent(Guid EstateId, String EstateName) : DomainEvent(EstateId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record EstateReferenceAllocatedEvent(Guid EstateId, String EstateReference) : DomainEvent(EstateId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record OperatorAddedToEstateEvent(Guid EstateId, Guid OperatorId) : DomainEvent(EstateId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record OperatorRemovedFromEstateEvent(Guid EstateId, Guid OperatorId) : DomainEvent(EstateId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record SecurityUserAddedToEstateEvent(Guid EstateId, Guid SecurityUserId, String EmailAddress) : DomainEvent(EstateId, Guid.NewGuid());
}