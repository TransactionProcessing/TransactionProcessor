namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>Sources from which merchant deposits can be recorded.</summary>
    /// <example>0</example>
    public enum MerchantDepositSource
    {
        /// <summary>The source has not been specified.</summary>
        NotSet,
        /// <summary>A deposit entered manually.</summary>
        Manual,
        /// <summary>A deposit created by an automated process.</summary>
        Automatic
    }
}
