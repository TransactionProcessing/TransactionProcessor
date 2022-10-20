namespace TransactionProcessor.ProjectionEngine.State
{
    using System.Diagnostics.Contracts;
    using Transaction.DomainEvents;

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
        public static MerchantBalanceState SetEstateId(this MerchantBalanceState state,
                                                       Guid estateId) =>
            state with {
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
        public static MerchantBalanceState StartTransaction(this MerchantBalanceState state,
                                                            TransactionHasStartedEvent domainEvent) {

            if (domainEvent.TransactionType == "Logon")
                return state;

            return state with {
                           StartedTransactionCount = state.StartedTransactionCount + 1
                       };
        }


        [Pure]
        public static MerchantBalanceState CompleteTransaction(this MerchantBalanceState state,
                                                               TransactionHasBeenCompletedEvent domainEvent) {

            if (domainEvent.TransactionAmount.HasValue == false)
                return state;

            return state with {
                           CompletedTransactionCount = state.CompletedTransactionCount + 1
                       };
        }

        [Pure]
        public static MerchantBalanceState SetMerchantName(this MerchantBalanceState state,
                                                         String merchantName) =>
            state with
            {
                MerchantName = merchantName
            };

        [Pure]
        public static MerchantBalanceState IncrementBalance(this MerchantBalanceState state,
                                                            Decimal amount) =>
            state with {
                           Balance = state.Balance + amount
                       };

        [Pure]
        public static MerchantBalanceState IncrementAvailableBalance(this MerchantBalanceState state,
                                                            Decimal amount) =>
            state with
            {
                AvailableBalance = state.AvailableBalance + amount
            };

        [Pure]
        public static MerchantBalanceState RecordDeposit(this MerchantBalanceState state,
                                                               Decimal depositAmount) =>
            state with
            {
                DepositCount = state.DepositCount + 1,
                TotalDeposited = state.TotalDeposited+depositAmount
            };

        [Pure]
        public static MerchantBalanceState RecordAuthorisedSale(this MerchantBalanceState state,
                                                         Decimal saleAmount) =>
            state with
            {
                SaleCount = state.SaleCount+1,
                AuthorisedSales = state.AuthorisedSales + saleAmount
            };

        [Pure]
        public static MerchantBalanceState RecordDeclinedSale(this MerchantBalanceState state,
                                                                Decimal saleAmount) =>
            state with
            {
                SaleCount = state.SaleCount+1,
                DeclinedSales = state.DeclinedSales + saleAmount
            };

        [Pure]
        public static MerchantBalanceState RecordMerchantFee(this MerchantBalanceState state,
                                                             Decimal feeAmount) =>
            state with
            {
                FeeCount = state.FeeCount+1,
                ValueOfFees = state.ValueOfFees+ feeAmount
            };

        [Pure]
        public static MerchantBalanceState DecrementAvailableBalance(this MerchantBalanceState state,
                                                                     Decimal amount) =>
            state with
            {
                AvailableBalance = state.AvailableBalance - amount
            };

        [Pure]
        public static MerchantBalanceState DecrementBalance(this MerchantBalanceState state,
                                                                     Decimal amount) =>
            state with
            {
                Balance = state.Balance - amount
            };
    }
}