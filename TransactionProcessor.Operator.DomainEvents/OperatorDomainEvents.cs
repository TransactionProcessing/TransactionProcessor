using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.Operator.DomainEvents
{
    [ExcludeFromCodeCoverage]
    public record OperatorCreatedEvent(Guid OperatorId,
                                       Guid EstateId,
                                       String Name,
                                       Boolean RequireCustomMerchantNumber,
                                       Boolean RequireCustomTerminalNumber) : DomainEvent(OperatorId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record OperatorNameUpdatedEvent(Guid OperatorId,
                                           Guid EstateId,
                                           String Name) : DomainEvent(OperatorId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record OperatorRequireCustomMerchantNumberChangedEvent(Guid OperatorId,
                                                                  Guid EstateId,
                                                                  Boolean RequireCustomMerchantNumber) : DomainEvent(OperatorId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record OperatorRequireCustomTerminalNumberChangedEvent(Guid OperatorId,
                                                                  Guid EstateId,
                                                                  Boolean RequireCustomTerminalNumber) : DomainEvent(OperatorId, Guid.NewGuid());
}
