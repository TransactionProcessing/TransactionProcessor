using Shared.DomainDrivenDesign.EventSourcing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.DomainEvents
{
    [ExcludeFromCodeCoverage]
    public class MerchantBalanceDomainEvents
    {
        public record MerchantBalanceInitialisedEvent(Guid MerchantId, Guid EstateId,DateTime DateTime) : DomainEvent(MerchantId, Guid.NewGuid());
        public record AuthorisedTransactionRecordedEvent(Guid MerchantId, Guid EstateId, Guid TransactionId, Decimal Amount, DateTime DateTime) : DomainEvent(MerchantId, TransactionId);
        public record DeclinedTransactionRecordedEvent(Guid MerchantId, Guid EstateId, Guid TransactionId, Decimal Amount, DateTime DateTime) : DomainEvent(MerchantId, TransactionId);
        public record MerchantDepositRecordedEvent(Guid MerchantId, Guid EstateId, Guid DepositId, Decimal Amount, DateTime DateTime) : DomainEvent(MerchantId, DepositId);
        public record MerchantWithdrawalRecordedEvent(Guid MerchantId, Guid EstateId, Guid WithdrawalId, Decimal Amount,DateTime DateTime) : DomainEvent(MerchantId, WithdrawalId);
        public record SettledFeeRecordedEvent(Guid MerchantId, Guid EstateId, Guid FeeId, Decimal Amount, DateTime DateTime) : DomainEvent(MerchantId, FeeId);
    }
}
