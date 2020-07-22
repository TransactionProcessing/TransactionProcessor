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
    /// <seealso cref="MediatR.IRequestHandler{ProcessLogonTransactionRequest, ProcessLogonTransactionResponse}" />
    /// <seealso cref="" />
    public class TransactionRequestHandler : IRequestHandler<ProcessLogonTransactionRequest, ProcessLogonTransactionResponse>,
                                             IRequestHandler<ProcessSaleTransactionRequest, ProcessSaleTransactionResponse>
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

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<ProcessSaleTransactionResponse> Handle(ProcessSaleTransactionRequest request,
                                                                 CancellationToken cancellationToken)
        {
            ProcessSaleTransactionResponse saleResponse = await this.TransactionDomainService.ProcessSaleTransaction(request.TransactionId,
                                                                                                                     request.EstateId,
                                                                                                                     request.MerchantId,
                                                                                                                     request.TransactionDateTime,
                                                                                                                     request.TransactionNumber,
                                                                                                                     request.DeviceIdentifier,
                                                                                                                     request.OperatorIdentifier,
                                                                                                                     request.CustomerEmailAddress,
                                                                                                                     request.AdditionalTransactionMetadata,
                                                                                                                     request.ContractId,
                                                                                                                     request.ProductId,
                                                                                                                     cancellationToken);

            return saleResponse;
        }
    }
}