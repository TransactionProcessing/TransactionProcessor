using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents;

public class ContractDomainEvents {
    [ExcludeFromCodeCoverage]
    public record ContractCreatedEvent(Guid ContractId, Guid EstateId, Guid OperatorId, String Description) : DomainEvent(ContractId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record VariableValueProductAddedToContractEvent(Guid ContractId, Guid EstateId, Guid ProductId, String ProductName, String DisplayText, Int32 ProductType) : DomainEvent(ContractId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record FixedValueProductAddedToContractEvent(Guid ContractId, Guid EstateId, Guid ProductId, String ProductName, String DisplayText, Decimal Value, Int32 ProductType) : DomainEvent(ContractId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record TransactionFeeForProductAddedToContractEvent(Guid ContractId, Guid EstateId, Guid ProductId, Guid TransactionFeeId, String Description, Int32 CalculationType, Int32 FeeType, Decimal Value) : DomainEvent(ContractId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record TransactionFeeForProductDisabledEvent(Guid ContractId, Guid EstateId, Guid ProductId, Guid TransactionFeeId) : DomainEvent(ContractId, Guid.NewGuid());
}