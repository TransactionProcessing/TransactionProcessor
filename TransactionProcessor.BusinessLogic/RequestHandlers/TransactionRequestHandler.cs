using SimpleResults;

namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Models;
    using Requests;
    using Services;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

    public class TransactionRequestHandler : IRequestHandler<TransactionCommands.ProcessLogonTransactionCommand, Result<ProcessLogonTransactionResponse>>,
                                             IRequestHandler<TransactionCommands.ProcessSaleTransactionCommand, Result<ProcessSaleTransactionResponse>>,
                                             IRequestHandler<TransactionCommands.ProcessReconciliationCommand, Result<ProcessReconciliationTransactionResponse>>,
                                             IRequestHandler<TransactionCommands.ResendTransactionReceiptCommand,Result>,
                                             IRequestHandler<TransactionCommands.CalculateFeesForTransactionCommand, Result>,
                                             IRequestHandler<TransactionCommands.AddSettledMerchantFeeCommand, Result>,
                                             IRequestHandler<TransactionCommands.SendCustomerEmailReceiptCommand, Result> {
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

        public async Task<Result<ProcessLogonTransactionResponse>> Handle(TransactionCommands.ProcessLogonTransactionCommand command,
                                                                  CancellationToken cancellationToken) {
            return await this.TransactionDomainService.ProcessLogonTransaction(
                command, cancellationToken);
        }

        public async Task<Result<ProcessSaleTransactionResponse>> Handle(TransactionCommands.ProcessSaleTransactionCommand command,
                                                                 CancellationToken cancellationToken)
        {
            return await this.TransactionDomainService.ProcessSaleTransaction(command, cancellationToken);
        }

        public async Task<Result<ProcessReconciliationTransactionResponse>> Handle(TransactionCommands.ProcessReconciliationCommand command,
                                                                           CancellationToken cancellationToken)
        {
            return await this.TransactionDomainService.ProcessReconciliationTransaction(command, cancellationToken);
        }

        #endregion

        public async Task<Result> Handle(TransactionCommands.ResendTransactionReceiptCommand command,
                                 CancellationToken cancellationToken) {
            return await this.TransactionDomainService.ResendTransactionReceipt(command, cancellationToken);
        }

        public async Task<Result> Handle(TransactionCommands.CalculateFeesForTransactionCommand command,
                                         CancellationToken cancellationToken) {
            return await this.TransactionDomainService.CalculateFeesForTransaction(command, cancellationToken);
        }

        public async Task<Result> Handle(TransactionCommands.AddSettledMerchantFeeCommand command,
                                         CancellationToken cancellationToken) {
            return await this.TransactionDomainService.AddSettledMerchantFee(command, cancellationToken);
        }

        public async Task<Result> Handle(TransactionCommands.SendCustomerEmailReceiptCommand command,
                                         CancellationToken cancellationToken) {
            return await this.TransactionDomainService.SendCustomerEmailReceipt(command, cancellationToken);
        }
    }
}