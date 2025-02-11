using Shared.ValueObjects;
using Shouldly;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
    public class MerchantDepositListAggregateTests
    {
        #region Methods

        [Fact]
        public void MerchantDepositListAggregate_Create_IsCreated() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.AggregateId.ShouldBe(TestData.MerchantId);
            merchantDepositListAggregate.EstateId.ShouldBe(TestData.EstateId);
            merchantDepositListAggregate.DateCreated.ShouldBe(TestData.DateMerchantCreated);
        }

        [Fact]
        public void MerchantDepositListAggregate_Create_AlreadyCreated_IsCreated()
        {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.AggregateId.ShouldBe(TestData.MerchantId);
            merchantDepositListAggregate.EstateId.ShouldBe(TestData.EstateId);
            merchantDepositListAggregate.DateCreated.ShouldBe(TestData.DateMerchantCreated);
        }

        [Fact]
        public void MerchantDepositListAggregate_Create_MerchantNotCreated_ErrorThrown() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            Should.Throw<InvalidOperationException>(() => { merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated); });
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeDeposit_AutomaticDepositSource_DepositMade() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.MakeDeposit(TestData.MerchantDepositSourceAutomatic,
                                                     TestData.DepositReference,
                                                     TestData.DepositDateTime,
                                                     TestData.DepositAmount);

            List<Deposit> depositListModel = merchantDepositListAggregate.GetDeposits();
            depositListModel.ShouldHaveSingleItem();
            depositListModel.Single().Source.ShouldBe(TestData.MerchantDepositSourceAutomatic);
            depositListModel.Single().DepositDateTime.ShouldBe(TestData.DepositDateTime);
            depositListModel.Single().Reference.ShouldBe(TestData.DepositReference);
            depositListModel.Single().Amount.ShouldBe(TestData.DepositAmount.Value);
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeDeposit_DepositMade() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.MakeDeposit(TestData.MerchantDepositSourceManual, TestData.DepositReference, TestData.DepositDateTime, TestData.DepositAmount);

            List<Deposit> depositListModel = merchantDepositListAggregate.GetDeposits();
            depositListModel.ShouldHaveSingleItem();
            depositListModel.Single().Source.ShouldBe(TestData.MerchantDepositSourceManual);

            depositListModel.Single().DepositDateTime.ShouldBe(TestData.DepositDateTime);
            depositListModel.Single().Reference.ShouldBe(TestData.DepositReference);
            depositListModel.Single().Amount.ShouldBe(TestData.DepositAmount.Value);
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeDeposit_DepositSourceNotSet_ErrorThrown() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            Should.Throw<InvalidOperationException>(() => {
                                                        merchantDepositListAggregate.MakeDeposit(MerchantDepositSource.NotSet,
                                                                                                 TestData.DepositReference,
                                                                                                 TestData.DepositDateTime,
                                                                                                 TestData.DepositAmount);
                                                    });
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeDeposit_DuplicateDeposit_ErrorThrown() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.MakeDeposit(TestData.MerchantDepositSourceManual, TestData.DepositReference, TestData.DepositDateTime, TestData.DepositAmount);
            List<Deposit> depositListModel = merchantDepositListAggregate.GetDeposits();
            depositListModel.ShouldHaveSingleItem();

            Should.Throw<InvalidOperationException>(() => {
                                                        merchantDepositListAggregate.MakeDeposit(TestData.MerchantDepositSourceManual,
                                                                                                 TestData.DepositReference,
                                                                                                 TestData.DepositDateTime,
                                                                                                 TestData.DepositAmount);
                                                    });
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeDeposit_MerchantDepositListNotCreated_ErrorThrown() {
            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);

            Should.Throw<InvalidOperationException>(() => {
                                                        merchantDepositListAggregate.MakeDeposit(TestData.MerchantDepositSourceManual,
                                                                                                 TestData.DepositReference,
                                                                                                 TestData.DepositDateTime,
                                                                                                 TestData.DepositAmount);
                                                    });
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeDeposit_TwoDeposits_BothDepositsMade() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.MakeDeposit(TestData.MerchantDepositSourceManual, TestData.DepositReference, TestData.DepositDateTime, TestData.DepositAmount);
            merchantDepositListAggregate.MakeDeposit(TestData.MerchantDepositSourceManual,
                                                     TestData.DepositReference2,
                                                     TestData.DepositDateTime2,
                                                     TestData.DepositAmount2);

            List<Deposit> depositListModel = merchantDepositListAggregate.GetDeposits();
            depositListModel.Count.ShouldBe(2);
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeDeposit_TwoDepositsOneMonthApartSameDetails_BothDepositsMade() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.MakeDeposit(TestData.MerchantDepositSourceManual,
                                                     "Test Data Gen Deposit",
                                                     new DateTime(2021, 1, 1, 0, 0, 0),
                                                     PositiveMoney.Create(Money.Create(650.00m)));
            merchantDepositListAggregate.MakeDeposit(TestData.MerchantDepositSourceManual,
                                                     "Test Data Gen Deposit",
                                                     new DateTime(2021, 2, 1, 0, 0, 0),
                                                     PositiveMoney.Create(Money.Create(650.00m)));

            List<Deposit> depositListModel = merchantDepositListAggregate.GetDeposits();
            depositListModel.Count.ShouldBe(2);
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeDeposit_TwoDepositsSameDetailsApartFromAmounts_BothDepositsMade() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.MakeDeposit(TestData.MerchantDepositSourceManual,
                                                     "Test Data Gen Deposit",
                                                     new DateTime(2021, 1, 1, 0, 0, 0),
                                                     PositiveMoney.Create(Money.Create(650.00m)));
            merchantDepositListAggregate.MakeDeposit(TestData.MerchantDepositSourceManual,
                                                     "Test Data Gen Deposit",
                                                     new DateTime(2021, 1, 1, 0, 0, 0),
                                                     PositiveMoney.Create(Money.Create(934.00m)));

            List<Deposit> depositListModel = merchantDepositListAggregate.GetDeposits();
            depositListModel.Count.ShouldBe(2);
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeWithdrawal_DuplicateWithdrawal_ErrorThrown() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.MakeWithdrawal(TestData.WithdrawalDateTime, TestData.WithdrawalAmount);
            List<Withdrawal> withdrawalListModel = merchantDepositListAggregate.GetWithdrawals();
            withdrawalListModel.ShouldHaveSingleItem();

            Should.Throw<InvalidOperationException>(() => { merchantDepositListAggregate.MakeWithdrawal(TestData.WithdrawalDateTime, TestData.WithdrawalAmount); });
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeWithdrawal_MerchantDepositListNotCreated_ErrorThrown() {
            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);

            Should.Throw<InvalidOperationException>(() => { merchantDepositListAggregate.MakeWithdrawal(TestData.WithdrawalDateTime, TestData.WithdrawalAmount); });
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeWithdrawal_TwoWithdrawals_BothWithdrawalsMade() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.MakeWithdrawal(TestData.WithdrawalDateTime, TestData.WithdrawalAmount);
            merchantDepositListAggregate.MakeWithdrawal(TestData.WithdrawalDateTime2, TestData.WithdrawalAmount2);

            List<Withdrawal> withdrawalListModel = merchantDepositListAggregate.GetWithdrawals();
            withdrawalListModel.Count.ShouldBe(2);
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeWithdrawal_TwoWithdrawalsOneMonthApartSameDetails_BothWithdrawalsMade() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.MakeWithdrawal(new DateTime(2021, 1, 1, 0, 0, 0), PositiveMoney.Create(Money.Create(650.00m)));
            merchantDepositListAggregate.MakeWithdrawal(new DateTime(2021, 2, 1, 0, 0, 0), PositiveMoney.Create(Money.Create(650.00m)));

            List<Withdrawal> withdrawalListModel = merchantDepositListAggregate.GetWithdrawals();
            withdrawalListModel.Count.ShouldBe(2);
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeWithdrawal_TwoWithdrawalsSameDetailsApartFromAmounts_BothWithdrawalsMade() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.MakeWithdrawal(new DateTime(2021, 1, 1, 0, 0, 0), PositiveMoney.Create(Money.Create(650.00m)));
            merchantDepositListAggregate.MakeWithdrawal(new DateTime(2021, 1, 1, 0, 0, 0), PositiveMoney.Create(Money.Create(934.00m)));

            List<Withdrawal> withdrawalListModel = merchantDepositListAggregate.GetWithdrawals();
            withdrawalListModel.Count.ShouldBe(2);
        }

        [Fact]
        public void MerchantDepositListAggregate_MakeWithdrawal_WithdrawalMade() {
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
            merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

            merchantDepositListAggregate.MakeWithdrawal(TestData.WithdrawalDateTime, TestData.WithdrawalAmount);

            var withdrawalListModel = merchantDepositListAggregate.GetWithdrawals();
            withdrawalListModel.ShouldHaveSingleItem();

            withdrawalListModel.Single().WithdrawalDateTime.ShouldBe(TestData.WithdrawalDateTime);
            withdrawalListModel.Single().Amount.ShouldBe(TestData.WithdrawalAmount.Value);
        }

        #endregion
    }
}