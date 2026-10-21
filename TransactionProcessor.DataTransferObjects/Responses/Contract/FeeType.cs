namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>Parties that can receive a transaction fee.</summary>
    /// <example>0</example>
    public enum FeeType
    {
        /// <summary>
        /// The fee is payable to the merchant.
        /// </summary>
        Merchant,

        /// <summary>
        /// The fee is payable to the service provider.
        /// </summary>
        ServiceProvider
    }
}
