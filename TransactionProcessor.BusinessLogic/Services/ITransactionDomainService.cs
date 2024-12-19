using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    public interface ITransactionDomainService
    {
        #region Methods

        Task<Result<ProcessLogonTransactionResponse>> ProcessLogonTransaction(TransactionCommands.ProcessLogonTransactionCommand command,
                                                                                  CancellationToken cancellationToken);
        
        Task<Result<ProcessSaleTransactionResponse>> ProcessSaleTransaction(TransactionCommands.ProcessSaleTransactionCommand command,
                                                                            CancellationToken cancellationToken);

        Task<Result<ProcessReconciliationTransactionResponse>> ProcessReconciliationTransaction(TransactionCommands.ProcessReconciliationCommand command,
                                                                                        CancellationToken cancellationToken);

        Task<Result> ResendTransactionReceipt(TransactionCommands.ResendTransactionReceiptCommand command,
                                              CancellationToken cancellationToken);

        Task<Result> CalculateFeesForTransaction(TransactionCommands.CalculateFeesForTransactionCommand command,
                                                 CancellationToken cancellationToken);

        #endregion

        Task<Result> AddSettledMerchantFee(TransactionCommands.AddSettledMerchantFeeCommand command,
                                           CancellationToken cancellationToken);

        Task<Result> SendCustomerEmailReceipt(TransactionCommands.SendCustomerEmailReceiptCommand command,
                                              CancellationToken cancellationToken);
        Task<Result> ResendCustomerEmailReceipt(TransactionCommands.ResendCustomerEmailReceiptCommand command,
                                              CancellationToken cancellationToken);
    }
}