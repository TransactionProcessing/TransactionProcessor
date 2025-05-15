using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents {

    [ExcludeFromCodeCoverage]
    public class MerchantStatementDomainEvents {
        public record StatementGeneratedEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTime DateGenerated) : DomainEvent(MerchantStatementId, Guid.NewGuid());

        public record StatementBuiltEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTime DateBuilt, String statementData) : DomainEvent(MerchantStatementId, Guid.NewGuid());
        
        public record StatementEmailedEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTime DateEmailed, Guid MessageId) : DomainEvent(MerchantStatementId, Guid.NewGuid());

        public record StatementCreatedEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTime StatementDate) : DomainEvent(MerchantStatementId, Guid.NewGuid());

        public record ActivityDateAddedToStatementEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, Guid MerchantStatementForDateId, DateTime ActivityDate) : DomainEvent(MerchantStatementId, Guid.NewGuid());

        public record StatementSummaryForDateEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTime ActivityDate, Int32 LineNumber,
                                                   Int32 NumberOfTransactions, Decimal ValueOfTransactions, Int32 NumberOfSettledFees, Decimal ValueOfSettledFees) : DomainEvent(MerchantStatementId, Guid.NewGuid());
    }

    [ExcludeFromCodeCoverage]
    public class MerchantStatementForDateDomainEvents {
        public record StatementCreatedForDateEvent(Guid MerchantStatementForDateId, DateTime ActivityDate, DateTime MerchantStatementDate, Guid MerchantStatementId, Guid EstateId, Guid MerchantId) : DomainEvent(MerchantStatementForDateId, Guid.NewGuid());

        public record SettledFeeAddedToStatementForDateEvent(Guid MerchantStatementForDateId, Guid EventId, Guid EstateId, Guid MerchantId, Guid SettledFeeId, Guid TransactionId, DateTime SettledDateTime, Decimal SettledValue) : DomainEvent(MerchantStatementForDateId, EventId);

        public record TransactionAddedToStatementForDateEvent(Guid MerchantStatementForDateId, Guid EventId, Guid EstateId, Guid MerchantId, Guid TransactionId, DateTime TransactionDateTime, Decimal TransactionValue) : DomainEvent(MerchantStatementForDateId, EventId);

    }
}