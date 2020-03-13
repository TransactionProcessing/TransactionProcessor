namespace TransactionProcessor.Factories
{
    using DataTransferObjects;
    using Models;

    /// <summary>
    /// 
    /// </summary>
    public interface IModelFactory
    {
        #region Methods

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="processLogonTransactionResponse">The process logon transaction response.</param>
        /// <returns></returns>
        SerialisedMessage ConvertFrom(ProcessLogonTransactionResponse processLogonTransactionResponse);

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="processSaleTransactionResponse">The process sale transaction response.</param>
        /// <returns></returns>
        SerialisedMessage ConvertFrom(ProcessSaleTransactionResponse processSaleTransactionResponse);

        #endregion
    }
}