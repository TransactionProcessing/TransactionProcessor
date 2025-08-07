using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
    public class MerchantBalanceAggregateTests
    {
        [Fact]
        public void MerchantBalanceAggregate_RecordCompletedTransaction_MerchantNotCreated_ErrorThrown()
        {
            MerchantBalanceAggregate aggregate = MerchantBalanceAggregate.Create(TestData.MerchantId);
            MerchantAggregate merchantAggregate = TestData.Aggregates.EmptyMerchantAggregate();
            Should.Throw<InvalidOperationException>(() => {
                aggregate.RecordCompletedTransaction(merchantAggregate, TestData.TransactionId, TestData.TransactionAmount, TestData.TransactionDateTime, true);
            });
        }

        [Fact]
        public void MerchantBalanceAggregate_RecordDeposit_MerchantNotCreated_ErrorThrown()
        {
            MerchantBalanceAggregate aggregate = MerchantBalanceAggregate.Create(TestData.MerchantId);
            MerchantAggregate merchantAggregate = TestData.Aggregates.EmptyMerchantAggregate();
            Should.Throw<InvalidOperationException>(() => {
                aggregate.RecordMerchantDeposit(merchantAggregate, TestData.DepositId, TestData.DepositAmount.Value, TestData.DepositDateTime);
            });
        }

        [Fact]
        public void MerchantBalanceAggregate_RecordMerchantWithdrawal_MerchantNotCreated_ErrorThrown()
        {
            MerchantBalanceAggregate aggregate = MerchantBalanceAggregate.Create(TestData.MerchantId);
            MerchantAggregate merchantAggregate = TestData.Aggregates.EmptyMerchantAggregate();
            Should.Throw<InvalidOperationException>(() => {
                aggregate.RecordMerchantWithdrawal(merchantAggregate, TestData.WithdrawalId, TestData.WithdrawalAmount.Value, TestData.WithdrawalDateTime);
            });
        }

        [Fact]
        public void MerchantBalanceAggregate_RecordSettledFee_MerchantNotCreated_ErrorThrown()
        {
            MerchantBalanceAggregate aggregate = MerchantBalanceAggregate.Create(TestData.MerchantId);
            MerchantAggregate merchantAggregate = TestData.Aggregates.EmptyMerchantAggregate();
            Should.Throw<InvalidOperationException>(() => {
                aggregate.RecordSettledFee(merchantAggregate, TestData.SettledFeeId1, TestData.SettledFeeAmount1, TestData.SettledFeeDateTime1);
            });
        }

        [Fact]
        public void MerchantBalanceAggregate_RecordCompletedTransaction_TransactionIsRecorded() {
            MerchantBalanceAggregate aggregate = MerchantBalanceAggregate.Create(TestData.MerchantId);
            MerchantAggregate merchantAggregate = TestData.Aggregates.CreatedMerchantAggregate();
            aggregate.RecordCompletedTransaction(merchantAggregate, TestData.TransactionId, TestData.TransactionAmount, TestData.TransactionDateTime, true);
            aggregate.Balance.ShouldBe(TestData.TransactionAmount * -1);
            aggregate.AuthorisedSales.Count.ShouldBe(1);
            aggregate.AuthorisedSales.Value.ShouldBe(TestData.TransactionAmount);
            aggregate.AuthorisedSales.LastActivity.ShouldBe(TestData.TransactionDateTime);
        }

        [Fact]
        public void MerchantBalanceAggregate_RecordCompletedTransaction_MultipleTransactions_TransactionIsRecorded()
        {
            MerchantBalanceAggregate aggregate = MerchantBalanceAggregate.Create(TestData.MerchantId);
            MerchantAggregate merchantAggregate = TestData.Aggregates.CreatedMerchantAggregate();
            aggregate.RecordCompletedTransaction(merchantAggregate, TestData.TransactionId, TestData.TransactionAmount, TestData.TransactionDateTime, true);
            aggregate.RecordCompletedTransaction(merchantAggregate, TestData.TransactionId1, TestData.TransactionAmount, TestData.TransactionDateTime, true);
            aggregate.Balance.ShouldBe((TestData.TransactionAmount * 2) * -1);
            aggregate.AuthorisedSales.Count.ShouldBe(2);
            aggregate.AuthorisedSales.Value.ShouldBe(TestData.TransactionAmount * 2);
            aggregate.AuthorisedSales.LastActivity.ShouldBe(TestData.TransactionDateTime);
        }

        [Fact]
        public void MerchantBalanceAggregate_RecordCompletedTransaction_DuplicateTransactionIsIgnored()
        {
            MerchantBalanceAggregate aggregate = MerchantBalanceAggregate.Create(TestData.MerchantId);
            MerchantAggregate merchantAggregate = TestData.Aggregates.CreatedMerchantAggregate();
            aggregate.RecordCompletedTransaction(merchantAggregate, TestData.TransactionId, TestData.TransactionAmount, TestData.TransactionDateTime, true);
            aggregate.RecordCompletedTransaction(merchantAggregate, TestData.TransactionId, TestData.TransactionAmount, TestData.TransactionDateTime, true);
            aggregate.Balance.ShouldBe(TestData.TransactionAmount * -1);
            aggregate.AuthorisedSales.Count.ShouldBe(1);
            aggregate.AuthorisedSales.Value.ShouldBe(TestData.TransactionAmount);
            aggregate.AuthorisedSales.LastActivity.ShouldBe(TestData.TransactionDateTime);
        }

        [Fact]
        public void MerchantBalanceAggregate_RecordCompletedTransaction_TransactionDeclined_TransactionIsRecorded()
        {
            MerchantBalanceAggregate aggregate = MerchantBalanceAggregate.Create(TestData.MerchantId);
            MerchantAggregate merchantAggregate = TestData.Aggregates.CreatedMerchantAggregate();
            aggregate.RecordCompletedTransaction(merchantAggregate, TestData.TransactionId, TestData.TransactionAmount, TestData.TransactionDateTime, false);
            aggregate.Balance.ShouldBe(0);
            aggregate.DeclinedSales.Count.ShouldBe(1);
            aggregate.DeclinedSales.Value.ShouldBe(TestData.TransactionAmount);
            aggregate.DeclinedSales.LastActivity.ShouldBe(TestData.TransactionDateTime);
        }

        [Fact]
        public void MerchantBalanceAggregate_RecordDeposit_DepositIsRecorded()
        {
            MerchantBalanceAggregate aggregate = MerchantBalanceAggregate.Create(TestData.MerchantId);
            MerchantAggregate merchantAggregate = TestData.Aggregates.CreatedMerchantAggregate();
            aggregate.RecordMerchantDeposit(merchantAggregate, TestData.DepositId, TestData.DepositAmount.Value, TestData.DepositDateTime);
            aggregate.Balance.ShouldBe(TestData.DepositAmount.Value);
            aggregate.Deposits.Count.ShouldBe(1);
            aggregate.Deposits.Value.ShouldBe(TestData.DepositAmount.Value);
            aggregate.Deposits.LastActivity.ShouldBe(TestData.DepositDateTime);
        }

        [Fact]
        public void MerchantBalanceAggregate_RecordWithdrawal_WithdrawalIsRecorded()
        {
            MerchantBalanceAggregate aggregate = MerchantBalanceAggregate.Create(TestData.MerchantId);
            MerchantAggregate merchantAggregate = TestData.Aggregates.CreatedMerchantAggregate();
            aggregate.RecordMerchantWithdrawal(merchantAggregate, TestData.WithdrawalId, TestData.WithdrawalAmount.Value, TestData.WithdrawalDateTime);
            aggregate.Balance.ShouldBe(TestData.WithdrawalAmount.Value * -1);
            aggregate.Withdrawals.Count.ShouldBe(1);
            aggregate.Withdrawals.Value.ShouldBe(TestData.WithdrawalAmount.Value);
            aggregate.Withdrawals.LastActivity.ShouldBe(TestData.WithdrawalDateTime);
        }

        [Fact]
        public void MerchantBalanceAggregate_RecordSettledFee_SettledFeeIsRecorded()
        {
            MerchantBalanceAggregate aggregate = MerchantBalanceAggregate.Create(TestData.MerchantId);
            MerchantAggregate merchantAggregate = TestData.Aggregates.CreatedMerchantAggregate();
            aggregate.RecordSettledFee(merchantAggregate, TestData.SettledFeeId1, TestData.SettledFeeAmount1, TestData.SettledFeeDateTime1);
            aggregate.Balance.ShouldBe(TestData.SettledFeeAmount1);
            aggregate.Fees.Count.ShouldBe(1);
            aggregate.Fees.Value.ShouldBe(TestData.SettledFeeAmount1);
            aggregate.Fees.LastActivity.ShouldBe(TestData.SettledFeeDateTime1);
        }
    }
}
