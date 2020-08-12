namespace TransactionProcessor.Models
{
    /// <summary>
    /// 
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// The logon
        /// </summary>
        Logon,

        /// <summary>
        /// The sale
        /// </summary>
        Sale,

        /// <summary>
        /// The refund
        /// </summary>
        Refund,

        /// <summary>
        /// The reversal
        /// </summary>
        Reversal
    }
}