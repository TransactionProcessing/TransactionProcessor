namespace TransactionProcessor.BusinessLogic.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.BusinessLogic.Services.ITransactionDomainService" />
    public class TransactionDomainService : ITransactionDomainService
    {
        #region Methods

        /// <summary>
        /// Processes the logon transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<ProcessLogonTransactionResponse> ProcessLogonTransaction(CancellationToken cancellationToken)
        {
            return new ProcessLogonTransactionResponse
                   {
                       ResponseMessage = "SUCCESS",
                       ResponseCode = 0
                   };
        }

        #endregion
    }
}