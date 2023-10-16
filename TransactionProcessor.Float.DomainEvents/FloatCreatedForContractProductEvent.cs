namespace TransactionProcessor.Float.DomainEvents;

using Shared.DomainDrivenDesign.EventSourcing;

public record FloatCreatedForContractProductEvent(Guid FloatId,
                                                  Guid EstateId,
                                                  Guid ContractId,
                                                  Guid ProductId,
                                                  DateTime CreatedDateTime)
    : DomainEvent(FloatId, Guid.NewGuid());