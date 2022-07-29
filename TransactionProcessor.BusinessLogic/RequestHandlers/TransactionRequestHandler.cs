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
    /// <seealso cref="MediatR.IRequestHandler{TransactionProcessor.BusinessLogic.Requests.ProcessSaleTransactionRequest, TransactionProcessor.Models.ProcessSaleTransactionResponse}" />
    /// <seealso cref="MediatR.IRequestHandler{TransactionProcessor.BusinessLogic.Requests.ProcessReconciliationRequest, TransactionProcessor.BusinessLogic.Requests.ProcessReconciliationResponse}" />
    /// <seealso cref="MediatR.IRequestHandler{ProcessLogonTransactionRequest, ProcessLogonTransactionResponse}" />
    /// <seealso cref="" />
    public class TransactionRequestHandler : IRequestHandler<ProcessLogonTransactionRequest, ProcessLogonTransactionResponse>,
                                             IRequestHandler<ProcessSaleTransactionRequest, ProcessSaleTransactionResponse>,
                                             IRequestHandler<ProcessReconciliationRequest, ProcessReconciliationTransactionResponse>
    {
        #region Fields

        /// <summary>
        /// The transaction domain service
        /// </summary>
        private readonly ITransactionDomainService TransactionDomainService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRequestHandler"/> class.
        /// </summary>
        /// <param name="transactionDomainService">The transaction domain service.</param>
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
        /// <returns>
        /// Response from the request
        /// </returns>
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

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// Response from the request
        /// </returns>
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
                request.TransactionSource,
                cancellationToken);

            return saleResponse;
        }

        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// Response from the request
        /// </returns>
        public async Task<ProcessReconciliationTransactionResponse> Handle(ProcessReconciliationRequest request,
                                                                           CancellationToken cancellationToken)
        {
            ProcessReconciliationTransactionResponse reconciliationResponse= await this.TransactionDomainService.ProcessReconciliationTransaction(request.TransactionId,
                                                                                         request.EstateId,
                                                                                         request.MerchantId,
                                                                                         request.DeviceIdentifier,
                                                                                         request.TransactionDateTime,
                                                                                         request.TransactionCount,
                                                                                         request.TransactionValue,
                                                                                         cancellationToken);

            return reconciliationResponse;
        }

        #endregion
    }
}