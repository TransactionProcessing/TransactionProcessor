using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.ProjectionEngine.State;

namespace TransactionProcessor.ProjectionEngine.Tests
{
    using Shouldly;
    using Transaction.DomainEvents;

    public class MerchantBalanceStateExtensionsTests{
        [Fact]
        public void MerchantBalanceStateExtensions_InitialiseBalances_BalancesAreInitialised(){
            MerchantBalanceState state = new MerchantBalanceState();

            state = state.InitialiseBalances();

            state.AvailableBalance.ShouldBe(0);
            state.Balance.ShouldBe(0);
        }

        [Fact]
        public void MerchantBalanceStateExtensions_HandleMerchantCreated_EventHandled(){
            MerchantBalanceState state = new MerchantBalanceState();
            state = state.HandleMerchantCreated(TestData.MerchantCreatedEvent);

            state.EstateId.ShouldBe(TestData.MerchantCreatedEvent.EstateId);
            state.MerchantId.ShouldBe(TestData.MerchantCreatedEvent.MerchantId);
            state.MerchantName.ShouldBe(TestData.MerchantCreatedEvent.MerchantName);
            state.AvailableBalance.ShouldBe(0);
            state.Balance.ShouldBe(0);
        }

        [Fact]
        public void MerchantBalanceStateExtensions_HandleManualDepositMadeEvent_EventHandled()
        {
            MerchantBalanceState state = new MerchantBalanceState();
            state = state.InitialiseBalances();

            state.AvailableBalance.ShouldBe(0);
            state.Balance.ShouldBe(0);

            state = state.HandleManualDepositMadeEvent(TestData.ManualDepositMadeEvent);

            state.AvailableBalance.ShouldBe(TestData.ManualDepositMadeEvent.Amount);
            state.Balance.ShouldBe(TestData.ManualDepositMadeEvent.Amount);
            state.DepositCount.ShouldBe(1);
            state.LastDeposit.ShouldBe(TestData.ManualDepositMadeEvent.DepositDateTime);
        }

        [Fact]
        public void MerchantBalanceStateExtensions_HandleAutomaticDepositMadeEvent_EventHandled()
        {
            MerchantBalanceState state = new MerchantBalanceState();
            state = state.InitialiseBalances();

            state.AvailableBalance.ShouldBe(0);
            state.Balance.ShouldBe(0);

            state = state.HandleAutomaticDepositMadeEvent(TestData.AutomaticDepositMadeEvent);

            state.AvailableBalance.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount);
            state.Balance.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount);
            state.DepositCount.ShouldBe(1);
            state.LastDeposit.ShouldBe(TestData.AutomaticDepositMadeEvent.DepositDateTime);
        }

        [Fact]
        public void MerchantBalanceStateExtensions_HandleWithdrawalMadeEvent_EventHandled()
        {
            MerchantBalanceState state = new MerchantBalanceState();
            state = state.InitialiseBalances();

            state.AvailableBalance.ShouldBe(0);
            state.Balance.ShouldBe(0);

            state = state.HandleWithdrawalMadeEvent(TestData.WithdrawalMadeEvent);

            state.AvailableBalance.ShouldBe(TestData.WithdrawalMadeEvent.Amount * -1);
            state.Balance.ShouldBe(TestData.WithdrawalMadeEvent.Amount * -1);
            state.WithdrawalCount.ShouldBe(1);
            state.LastWithdrawal.ShouldBe(TestData.WithdrawalMadeEvent.WithdrawalDateTime);
        }

        [Fact]
        public void MerchantBalanceStateExtensions_HandleWithdrawalMadeEvent_WithdrawalOutOfOrder_EventHandled()
        {
            MerchantBalanceState state = new MerchantBalanceState();
            state = state.InitialiseBalances().HandleWithdrawalMadeEvent(TestData.WithdrawalMadeEvent1);

            state.AvailableBalance.ShouldBe(TestData.WithdrawalMadeEvent1.Amount * -1);
            state.Balance.ShouldBe(TestData.WithdrawalMadeEvent1.Amount * -1);
            state.WithdrawalCount.ShouldBe(1);
            state.TotalWithdrawn.ShouldBe(TestData.WithdrawalMadeEvent1.Amount);
            state.LastWithdrawal.ShouldBe(TestData.WithdrawalMadeEvent1.WithdrawalDateTime);

            state = state.HandleWithdrawalMadeEvent(TestData.WithdrawalMadeEvent);

            state.AvailableBalance.ShouldBe((TestData.WithdrawalMadeEvent1.Amount + TestData.WithdrawalMadeEvent.Amount) * -1);
            state.Balance.ShouldBe((TestData.WithdrawalMadeEvent1.Amount + TestData.WithdrawalMadeEvent.Amount) * -1);
            state.WithdrawalCount.ShouldBe(2);
            state.TotalWithdrawn.ShouldBe(TestData.WithdrawalMadeEvent1.Amount + TestData.WithdrawalMadeEvent.Amount);
            state.LastWithdrawal.ShouldBe(TestData.WithdrawalMadeEvent1.WithdrawalDateTime);
        }

        [Fact]
        public void MerchantBalanceStateExtensions_HandleTransactionHasStartedEvent_EventHandled()
        {
            MerchantBalanceState state = new MerchantBalanceState();
            state = state.InitialiseBalances();

            state.AvailableBalance.ShouldBe(0);
            state.Balance.ShouldBe(0);

            TransactionHasStartedEvent startedEvent = TestData.GetTransactionHasStartedEvent(TestData.TransactionAmount);
            state = state.HandleTransactionHasStartedEvent(startedEvent);

            state.AvailableBalance.ShouldBe(startedEvent.TransactionAmount.Value * -1);
            state.Balance.ShouldBe(0);
            state.StartedTransactionCount.ShouldBe(1);
        }

        [Fact]
        public void MerchantBalanceStateExtensions_HandleTransactionHasStartedEvent_Logon_EventHandled()
        {
            MerchantBalanceState state = new MerchantBalanceState();
            state = state.InitialiseBalances();

            state.AvailableBalance.ShouldBe(0);
            state.Balance.ShouldBe(0);

            TransactionHasStartedEvent startedEvent = TestData.GetTransactionHasStartedEvent(TestData.TransactionAmount, "Logon");

            state = state.HandleTransactionHasStartedEvent(startedEvent);

            state.AvailableBalance.ShouldBe(0);
            state.Balance.ShouldBe(0);
            state.StartedTransactionCount.ShouldBe(0);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void MerchantBalanceStateExtensions_HandleTransactionHasBeenCompletedEvent_EventHandled(Boolean isAuthorised, Boolean hasValue)
        {
            MerchantBalanceState state = new MerchantBalanceState();
            state = state.InitialiseBalances();

            state.AvailableBalance.ShouldBe(0);
            state.Balance.ShouldBe(0);

            TransactionHasBeenCompletedEvent completedEvent = TestData.GetTransactionHasBeenCompletedEvent(isAuthorised, 
                                                                                                           hasValue == true ? 
                                                                                                           TestData.TransactionAmount : null);
            state = state.HandleTransactionHasBeenCompletedEvent(completedEvent);
            
            state.SaleCount.ShouldBe(1);
            if (hasValue)
                state.LastSale.ShouldBe(completedEvent.CompletedDateTime);

            if (isAuthorised){
                state.AvailableBalance.ShouldBe(0);
                state.AuthorisedSales.ShouldBe(completedEvent.TransactionAmount.GetValueOrDefault(0));
                state.Balance.ShouldBe(completedEvent.TransactionAmount.GetValueOrDefault(0) * -1);
            }
            else{
                state.AvailableBalance.ShouldBe(completedEvent.TransactionAmount.GetValueOrDefault(0));
                state.DeclinedSales.ShouldBe(completedEvent.TransactionAmount.GetValueOrDefault(0));
                state.Balance.ShouldBe(0);
            }
        }

        [Fact]
        public void MerchantBalanceStateExtensions_HandleMerchantFeeAddedToTransactionEvent_EventHandled(){
            MerchantBalanceState state = new MerchantBalanceState();
            state = state.InitialiseBalances();

            state.AvailableBalance.ShouldBe(0);
            state.Balance.ShouldBe(0);

            MerchantFeeAddedToTransactionEvent merchantFeeAddedToTransactionEvent = TestData.GetMerchantFeeAddedToTransactionEvent(1.00m);
            state = state.HandleMerchantFeeAddedToTransactionEvent(merchantFeeAddedToTransactionEvent);

            state.Balance.ShouldBe(merchantFeeAddedToTransactionEvent.CalculatedValue);
            state.AvailableBalance.ShouldBe(merchantFeeAddedToTransactionEvent.CalculatedValue);
            state.FeeCount.ShouldBe(1);
            state.ValueOfFees.ShouldBe(merchantFeeAddedToTransactionEvent.CalculatedValue);
            state.LastFee.ShouldBe(merchantFeeAddedToTransactionEvent.FeeCalculatedDateTime);
        }
    }
}
