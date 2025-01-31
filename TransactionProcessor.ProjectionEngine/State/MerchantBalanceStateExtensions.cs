using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.ProjectionEngine.State
{
    using System.Diagnostics.Contracts;
    using EstateManagement.Merchant.DomainEvents;
    public static class MerchantBalanceStateExtensions
    {
        [Pure]
        public static MerchantBalanceState InitialiseBalances(this MerchantBalanceState state) =>
            state with
            {
                Balance = 0,
                AvailableBalance = 0
            };

        [Pure]
        public static MerchantBalanceState HandleMerchantCreated(this MerchantBalanceState state,
                                                                MerchantCreatedEvent mce) =>
            state.SetEstateId(mce.EstateId).SetMerchantId(mce.MerchantId).SetMerchantName(mce.MerchantName).InitialiseBalances();


        [Pure]
        public static MerchantBalanceState HandleManualDepositMadeEvent(this MerchantBalanceState state,
                                                                        ManualDepositMadeEvent mdme) =>
            state.IncrementAvailableBalance(mdme.Amount).IncrementBalance(mdme.Amount).RecordDeposit(mdme);

        [Pure]
        public static MerchantBalanceState HandleWithdrawalMadeEvent(this MerchantBalanceState state,
                                                                     WithdrawalMadeEvent wme) =>
            state.DecrementAvailableBalance(wme.Amount).DecrementBalance(wme.Amount).RecordWithdrawal(wme);

        [Pure]
        public static MerchantBalanceState HandleAutomaticDepositMadeEvent(this MerchantBalanceState state,
                                                                           AutomaticDepositMadeEvent adme) =>
            state.IncrementAvailableBalance(adme.Amount).IncrementBalance(adme.Amount).RecordDeposit(adme);

        [Pure]
        public static MerchantBalanceState HandleTransactionHasStartedEvent(this MerchantBalanceState state,
                                                                            TransactionDomainEvents.TransactionHasStartedEvent thse){

            if (thse.TransactionType == "Logon"){
                return state;
            }
            state = state.DecrementAvailableBalance(thse.TransactionAmount.GetValueOrDefault(0)).StartTransaction();

            return state;
        }

        [Pure]
        public static MerchantBalanceState HandleTransactionHasBeenCompletedEvent(this MerchantBalanceState state,
                                                                                  TransactionDomainEvents.TransactionHasBeenCompletedEvent thbce)
        {

            if (thbce.IsAuthorised)
            {
                state = state.DecrementBalance(thbce.TransactionAmount.GetValueOrDefault(0)).RecordAuthorisedSale(thbce.TransactionAmount.GetValueOrDefault(0));
            }
            else
            {
                state = state.IncrementAvailableBalance(thbce.TransactionAmount.GetValueOrDefault(0)).RecordDeclinedSale(thbce.TransactionAmount.GetValueOrDefault(0));
            }
            return state.CompleteTransaction(thbce);
        }

        [Pure]
        public static MerchantBalanceState HandleSettledMerchantFeeAddedToTransactionEvent(this MerchantBalanceState state,
                                                                                           TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent mfatte) =>
            state.IncrementAvailableBalance(mfatte.CalculatedValue).IncrementBalance(mfatte.CalculatedValue).RecordMerchantFee(mfatte);

        [Pure]
        public static MerchantBalanceState SetEstateId(this MerchantBalanceState state,
                                                       Guid estateId) =>
            state with
            {
                EstateId = estateId
            };

        [Pure]
        public static MerchantBalanceState SetMerchantId(this MerchantBalanceState state,
                                                       Guid merchantId) =>
            state with
            {
                MerchantId = merchantId
            };

        [Pure]
        public static MerchantBalanceState StartTransaction(this MerchantBalanceState state) =>
        state with
            {
                StartedTransactionCount = state.StartedTransactionCount + 1
            };

        [Pure]
        public static MerchantBalanceState CompleteTransaction(this MerchantBalanceState state,
                                                               TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent)
        {
            return state with
            {
                CompletedTransactionCount = state.CompletedTransactionCount + 1,
                LastSale = domainEvent.CompletedDateTime > state.LastSale ? domainEvent.CompletedDateTime : state.LastSale,
            };
        }

        [Pure]
        public static MerchantBalanceState SetMerchantName(this MerchantBalanceState state,
                                                         string merchantName) =>
            state with
            {
                MerchantName = merchantName
            };

        [Pure]
        public static MerchantBalanceState IncrementBalance(this MerchantBalanceState state,
                                                            decimal amount) =>
            state with
            {
                Balance = state.Balance + amount
            };

        [Pure]
        public static MerchantBalanceState IncrementAvailableBalance(this MerchantBalanceState state,
                                                            decimal amount) =>
            state with
            {
                AvailableBalance = state.AvailableBalance + amount
            };

        [Pure]
        public static MerchantBalanceState RecordDeposit(this MerchantBalanceState state,
                                                               ManualDepositMadeEvent mdme) =>
            state with
            {
                DepositCount = state.DepositCount + 1,
                TotalDeposited = state.TotalDeposited + mdme.Amount,
                LastDeposit = mdme.DepositDateTime > state.LastDeposit ? mdme.DepositDateTime : state.LastDeposit,
            };

        [Pure]
        public static MerchantBalanceState RecordDeposit(this MerchantBalanceState state,
                                                         AutomaticDepositMadeEvent adme) =>
            state with
            {
                DepositCount = state.DepositCount + 1,
                TotalDeposited = state.TotalDeposited + adme.Amount,
                LastDeposit = adme.DepositDateTime > state.LastDeposit ? adme.DepositDateTime : state.LastDeposit,
            };

        [Pure]
        public static MerchantBalanceState RecordWithdrawal(this MerchantBalanceState state,
                                                         WithdrawalMadeEvent wme) =>
            state with
            {
                WithdrawalCount = state.WithdrawalCount + 1,
                TotalWithdrawn = state.TotalWithdrawn + wme.Amount,
                LastWithdrawal = wme.WithdrawalDateTime > state.LastWithdrawal ? wme.WithdrawalDateTime : state.LastWithdrawal
            };

        [Pure]
        public static MerchantBalanceState RecordAuthorisedSale(this MerchantBalanceState state,
                                                         decimal saleAmount) =>
            state with
            {
                SaleCount = state.SaleCount + 1,
                AuthorisedSales = state.AuthorisedSales + saleAmount
            };

        [Pure]
        public static MerchantBalanceState RecordDeclinedSale(this MerchantBalanceState state,
                                                                decimal saleAmount) =>
            state with
            {
                SaleCount = state.SaleCount + 1,
                DeclinedSales = state.DeclinedSales + saleAmount
            };

        [Pure]
        public static MerchantBalanceState RecordMerchantFee(this MerchantBalanceState state,
                                                             TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent mfatte) =>
            state with
            {
                FeeCount = state.FeeCount + 1,
                ValueOfFees = state.ValueOfFees + mfatte.CalculatedValue,
                LastFee = mfatte.FeeCalculatedDateTime > state.LastFee ? mfatte.FeeCalculatedDateTime : state.LastFee,
            };

        [Pure]
        public static MerchantBalanceState DecrementAvailableBalance(this MerchantBalanceState state,
                                                                     decimal amount) =>
            state with
            {
                AvailableBalance = state.AvailableBalance - amount
            };

        [Pure]
        public static MerchantBalanceState DecrementBalance(this MerchantBalanceState state,
                                                                     decimal amount) =>

            state with
            {
                Balance = state.Balance - amount
            };
    }
}