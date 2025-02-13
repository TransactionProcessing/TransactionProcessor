using Shared.DomainDrivenDesign.EventSourcing;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DomainEvents {
    [ExcludeFromCodeCoverage]
    public class MerchantStatementDomainEvents {
        public record SettledFeeAddedToStatementEvent(Guid MerchantStatementId, Guid EventId, Guid EstateId, Guid MerchantId, Guid SettledFeeId, Guid TransactionId, DateTime SettledDateTime, Decimal SettledValue) : DomainEvent(MerchantStatementId, EventId);

        public record TransactionAddedToStatementEvent(Guid MerchantStatementId, Guid EventId, Guid EstateId, Guid MerchantId, Guid TransactionId, DateTime TransactionDateTime, Decimal TransactionValue) : DomainEvent(MerchantStatementId, EventId);

        public record StatementGeneratedEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTime DateGenerated) : DomainEvent(MerchantStatementId, Guid.NewGuid());

        public record StatementEmailedEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTime DateEmailed, Guid MessageId) : DomainEvent(MerchantStatementId, Guid.NewGuid());

        public record StatementCreatedEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTime DateCreated) : DomainEvent(MerchantStatementId, Guid.NewGuid());
    }
}