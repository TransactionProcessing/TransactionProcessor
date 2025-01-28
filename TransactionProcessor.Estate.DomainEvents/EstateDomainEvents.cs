using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.Estate.DomainEvents{
    public record EstateCreatedEvent(Guid EstateId,
                                     String EstateName) : DomainEvent(EstateId, Guid.NewGuid());

    public record EstateReferenceAllocatedEvent(Guid EstateId, String EstateReference) : DomainEvent(EstateId, Guid.NewGuid());

    public record OperatorAddedToEstateEvent(Guid EstateId,
                                             Guid OperatorId) : DomainEvent(EstateId, Guid.NewGuid());

    public record OperatorRemovedFromEstateEvent(Guid EstateId,
                                             Guid OperatorId) : DomainEvent(EstateId, Guid.NewGuid());

    public record SecurityUserAddedToEstateEvent(Guid EstateId,
                                                 Guid SecurityUserId,
                                                 String EmailAddress) : DomainEvent(EstateId, Guid.NewGuid());
}