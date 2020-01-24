namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Models;
    using Requests;
    using Services;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler{TransactionProcessor.BusinessLogic.Requests.ProcessLogonTransactionRequest, TransactionProcessor.Models.ProcessLogonTransactionResponse}" />
    /// <seealso cref="" />
    public class TransactionRequestHandler : IRequestHandler<ProcessLogonTransactionRequest, ProcessLogonTransactionResponse> 
    {
        #region Fields

        /// <summary>
        /// The transaction domain service
        /// </summary>
        private readonly ITransactionDomainService TransactionDomainService;

        #endregion

        #region Constructors

        public TransactionRequestHandler(ITransactionDomainService transactionDomainService)
        {
            this.TransactionDomainService = transactionDomainService;
        }

        #endregion

        #region Methods

        public async Task<ProcessLogonTransactionResponse> Handle(ProcessLogonTransactionRequest request,
                                                                  CancellationToken cancellationToken)
        {
            ProcessLogonTransactionResponse logonResponse =
                await this.TransactionDomainService.ProcessLogonTransaction(request.TransactionId,
                                                                            request.EstateId,
                                                                            request.MerchantId,
                                                                            request.TransactionDateTime,
                                                                            request.TransactionNumber,
                                                                            request.DeviceIdentifier,
                                                                            cancellationToken);

            return logonResponse;
        }

        #endregion
    }
}