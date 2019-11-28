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
        LogonTransactionResponse ConvertFrom(ProcessLogonTransactionResponse processLogonTransactionResponse);

        #endregion
    }
}