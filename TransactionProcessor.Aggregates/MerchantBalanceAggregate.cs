using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.Aggregates
{
    public static class MerchantBalanceAggregateExtensions
    {
        public static void PlayEvent(this MerchantBalanceAggregate aggregate, MerchantBalanceDomainEvents.AuthorisedTransactionRecordedEvent domainEvent)
        {
            aggregate.Balance -= domainEvent.Amount;
            aggregate.AuthorisedSales = aggregate.AuthorisedSales with { Count = aggregate.AuthorisedSales.Count + 1, Value = aggregate.AuthorisedSales.Value + domainEvent.Amount, LastActivity = domainEvent.DateTime };
        }

        public static void PlayEvent(this MerchantBalanceAggregate aggregate, MerchantBalanceDomainEvents.DeclinedTransactionRecordedEvent domainEvent)
        {
            aggregate.DeclinedSales= aggregate.AuthorisedSales with { Count = aggregate.DeclinedSales.Count + 1, Value = aggregate.DeclinedSales.Value + domainEvent.Amount, LastActivity = domainEvent.DateTime };
        }

        public static void PlayEvent(this MerchantBalanceAggregate aggregate, MerchantBalanceDomainEvents.MerchantDepositRecordedEvent domainEvent)
        {
            aggregate.Balance += domainEvent.Amount;
            aggregate.Deposits = aggregate.Deposits with { Count = aggregate.Deposits.Count + 1, Value = aggregate.Deposits.Value + domainEvent.Amount, LastActivity = domainEvent.DateTime };
        }

        public static void PlayEvent(this MerchantBalanceAggregate aggregate, MerchantBalanceDomainEvents.MerchantWithdrawalRecordedEvent domainEvent)
        {
            aggregate.Balance -= domainEvent.Amount;
            aggregate.Withdrawals = aggregate.Withdrawals with { Count = aggregate.Withdrawals.Count + 1, Value = aggregate.Withdrawals.Value + domainEvent.Amount, LastActivity = domainEvent.DateTime };
        }

        public static void PlayEvent(this MerchantBalanceAggregate aggregate, MerchantBalanceDomainEvents.SettledFeeRecordedEvent domainEvent)
        {
            aggregate.Balance += domainEvent.Amount;
            aggregate.Fees = aggregate.Fees with { Count = aggregate.Fees.Count + 1, Value = aggregate.Fees.Value + domainEvent.Amount, LastActivity = domainEvent.DateTime };
        }

        public static void PlayEvent(this MerchantBalanceAggregate aggregate, MerchantBalanceDomainEvents.MerchantBalanceInitialisedEvent domainEvent)
        {
            aggregate.IsInitialised = true;
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.Balance = 0;
        }

        private static void EnsureMerchantHasBeenCreated(this MerchantBalanceAggregate aggregate,
                                                         MerchantAggregate merchantAggregate) {
            if (merchantAggregate.IsCreated == false) {
                throw new InvalidOperationException("Merchant has not been created");
            }
        }

        private static void EnsureMerchantBalanceHasBeenInitialised(this MerchantBalanceAggregate aggregate,
                                                                    MerchantAggregate merchantAggregate, 
                                                                    DateTime dateTime)
        {
            if (aggregate.IsInitialised == false) {
                Merchant merchant = merchantAggregate.GetMerchant();
                MerchantBalanceDomainEvents.MerchantBalanceInitialisedEvent merchantBalanceInitialisedEvent = new MerchantBalanceDomainEvents.MerchantBalanceInitialisedEvent(merchant.MerchantId, merchant.EstateId,dateTime);
                aggregate.ApplyAndAppend(merchantBalanceInitialisedEvent);
            }
        }


        public static void RecordCompletedTransaction(this MerchantBalanceAggregate aggregate,
                                                      MerchantAggregate merchantAggregate,
                                                      Guid transactionId,
                                                      Decimal transactionAmount,
                                                      DateTime transactionDateTime,
                                                      Boolean isAuthorised) {
            aggregate.EnsureMerchantHasBeenCreated(merchantAggregate);
            aggregate.EnsureMerchantBalanceHasBeenInitialised(merchantAggregate, transactionDateTime);
            DomainEvent domainEvent = isAuthorised switch {
                true => new MerchantBalanceDomainEvents.AuthorisedTransactionRecordedEvent(aggregate.AggregateId, aggregate.EstateId, transactionId, transactionAmount, transactionDateTime),
                _ => new MerchantBalanceDomainEvents.DeclinedTransactionRecordedEvent(aggregate.AggregateId, aggregate.EstateId, transactionId, transactionAmount, transactionDateTime)
            };
            aggregate.ApplyAndAppend(domainEvent);
        }

        public static void RecordMerchantDeposit(this MerchantBalanceAggregate aggregate,
                                                 MerchantAggregate merchantAggregate,
                                                 Guid depositId,
                                                 Decimal depositAmount,
                                                 DateTime depositDateTime) {
            aggregate.EnsureMerchantHasBeenCreated(merchantAggregate);
            aggregate.EnsureMerchantBalanceHasBeenInitialised(merchantAggregate, depositDateTime);

            MerchantBalanceDomainEvents.MerchantDepositRecordedEvent domainEvent = new MerchantBalanceDomainEvents.MerchantDepositRecordedEvent(aggregate.AggregateId, aggregate.EstateId, depositId, depositAmount, depositDateTime);
            aggregate.ApplyAndAppend(domainEvent);
        }

        public static void RecordMerchantWithdrawal(this MerchantBalanceAggregate aggregate,
                                                    MerchantAggregate merchantAggregate,
                                                    Guid withdrawalId,
                                                    Decimal withdrawalAmount,
                                                    DateTime withdrawalDateTime) {
            aggregate.EnsureMerchantHasBeenCreated(merchantAggregate);
            aggregate.EnsureMerchantBalanceHasBeenInitialised(merchantAggregate, withdrawalDateTime);

            MerchantBalanceDomainEvents.MerchantWithdrawalRecordedEvent domainEvent = new MerchantBalanceDomainEvents.MerchantWithdrawalRecordedEvent(aggregate.AggregateId, aggregate.EstateId, withdrawalId, withdrawalAmount, withdrawalDateTime);
            aggregate.ApplyAndAppend(domainEvent);
        }

        public static void RecordSettledFee(this MerchantBalanceAggregate aggregate,
                                            MerchantAggregate merchantAggregate,
                                            Guid feeId,
                                            Decimal feeAmount,
                                            DateTime feeDateTime) {
            aggregate.EnsureMerchantHasBeenCreated(merchantAggregate);
            aggregate.EnsureMerchantBalanceHasBeenInitialised(merchantAggregate, feeDateTime);

            MerchantBalanceDomainEvents.SettledFeeRecordedEvent domainEvent = new MerchantBalanceDomainEvents.SettledFeeRecordedEvent(aggregate.AggregateId, aggregate.EstateId, feeId, feeAmount, feeDateTime);
            aggregate.ApplyAndAppend(domainEvent);
        }
    }

    public record ActivityType(Int32 Count, Decimal Value, DateTime LastActivity);
    
    public record MerchantBalanceAggregate : Aggregate {

        public Boolean IsInitialised { get; internal set; }
        public Guid EstateId { get; internal set; }
        public Decimal Balance { get; internal set; }
        public ActivityType Deposits { get; internal set; }
        public ActivityType Withdrawals { get; internal set; }
        public ActivityType AuthorisedSales { get; internal set; }
        public ActivityType DeclinedSales { get; internal set; }
        public ActivityType Fees { get; internal set; }

        [ExcludeFromCodeCoverage]
        public MerchantBalanceAggregate()
        {
            // Nothing here
            this.AuthorisedSales = new ActivityType(0, 0, DateTime.MinValue);
            this.DeclinedSales = new ActivityType(0, 0, DateTime.MinValue);
            this.Deposits = new ActivityType(0, 0, DateTime.MinValue);
            this.Withdrawals = new ActivityType(0, 0, DateTime.MinValue);
            this.Fees = new ActivityType(0, 0, DateTime.MinValue);
        }

        private MerchantBalanceAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.AuthorisedSales = new ActivityType(0, 0, DateTime.MinValue);
            this.DeclinedSales = new ActivityType(0, 0, DateTime.MinValue);
            this.Deposits = new ActivityType(0, 0, DateTime.MinValue);
            this.Withdrawals = new ActivityType(0, 0, DateTime.MinValue);
            this.Fees = new ActivityType(0, 0, DateTime.MinValue);
        }

        public static MerchantBalanceAggregate Create(Guid aggregateId)
        {
            return new MerchantBalanceAggregate(aggregateId);
        }

        public override void PlayEvent(IDomainEvent domainEvent) => MerchantBalanceAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return new
            {
                EstateId = Guid.NewGuid() // TODO: Populate
            };
        }
    }
}
