namespace TransactionProcessor.Factories
{
    using DataTransferObjects;
    using Models;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.Factories.IModelFactory" />
    public class ModelFactory : IModelFactory
    {
        #region Methods

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="processLogonTransactionResponse">The process logon transaction response.</param>
        /// <returns></returns>
        public LogonTransactionResponse ConvertFrom(ProcessLogonTransactionResponse processLogonTransactionResponse)
        {
            return new LogonTransactionResponse
                   {
                       ResponseMessage = processLogonTransactionResponse.ResponseMessage,
                       ResponseCode = processLogonTransactionResponse.ResponseCode
                   };
        }

        #endregion
    }
}