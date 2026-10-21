namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>Methods used to calculate a transaction fee.</summary>
    /// <example>0</example>
    public enum CalculationType
    {
        /// <summary>
        /// A fixed fee amount.
        /// </summary>
        Fixed,

        /// <summary>
        /// A fee calculated as a percentage of the transaction value.
        /// </summary>
        Percentage
    }
}
