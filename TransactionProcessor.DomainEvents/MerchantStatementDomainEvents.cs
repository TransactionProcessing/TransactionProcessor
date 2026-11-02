using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents {

    [ExcludeFromCodeCoverage]
    public class MerchantStatementDomainEvents {
        public record StatementGeneratedEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTimeOffset DateGenerated) : DomainEvent(MerchantStatementId, Guid.NewGuid());

        public record StatementBuiltEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTimeOffset DateBuilt, String statementData) : DomainEvent(MerchantStatementId, Guid.NewGuid());
        
        public record StatementEmailedEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTimeOffset DateEmailed, Guid MessageId) : DomainEvent(MerchantStatementId, Guid.NewGuid());

        public record StatementCreatedEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTimeOffset StatementDate) : DomainEvent(MerchantStatementId, Guid.NewGuid());

        public record ActivityDateAddedToStatementEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, Guid MerchantStatementForDateId, DateTimeOffset ActivityDate) : DomainEvent(MerchantStatementId, Guid.NewGuid());

        public record StatementSummaryForDateEvent(Guid MerchantStatementId, Guid EstateId, Guid MerchantId, DateTimeOffset ActivityDate, Int32 LineNumber,
                                                   Int32 NumberOfTransactions, Decimal ValueOfTransactions, 
                                                   Int32 NumberOfSettledFees, Decimal ValueOfSettledFees,
                                                   Int32 NumberOfDeposits, Decimal ValueOfDeposits,
                                                   Int32 NumberOfWithdrawals, Decimal ValueOfWithdrawals) : DomainEvent(MerchantStatementId, Guid.NewGuid());
    }

    [ExcludeFromCodeCoverage]
    public class MerchantStatementForDateDomainEvents {
        public record StatementCreatedForDateEvent(Guid MerchantStatementForDateId, DateTimeOffset ActivityDate, DateTimeOffset MerchantStatementDate, Guid MerchantStatementId, Guid EstateId, Guid MerchantId) : DomainEvent(MerchantStatementForDateId, Guid.NewGuid());

        public record SettledFeeAddedToStatementForDateEvent(Guid MerchantStatementForDateId, Guid EventId, Guid EstateId, Guid MerchantId, Guid MerchantStatementId, Guid SettledFeeId, Guid TransactionId, DateTimeOffset SettledDateTime, Decimal SettledValue) : DomainEvent(MerchantStatementForDateId, EventId);

        public record TransactionAddedToStatementForDateEvent(Guid MerchantStatementForDateId, Guid EventId, Guid EstateId, Guid MerchantId, Guid MerchantStatementId, Guid TransactionId, DateTimeOffset TransactionDateTime, Decimal TransactionValue) : DomainEvent(MerchantStatementForDateId, EventId);

        public record DepositAddedToStatementForDateEvent(Guid MerchantStatementForDateId, Guid EventId, Guid EstateId, Guid MerchantId, Guid MerchantStatementId, Guid DepositId, DateTimeOffset DepositDateTime, Decimal DepositAmount) : DomainEvent(MerchantStatementForDateId, EventId);
        public record WithdrawalAddedToStatementForDateEvent(Guid MerchantStatementForDateId, Guid EventId, Guid EstateId, Guid MerchantId, Guid MerchantStatementId, Guid WithdrawalId, DateTimeOffset WithdrawalDateTime, Decimal WithdrawalAmount) : DomainEvent(MerchantStatementForDateId, EventId);

    }
}