using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using Models;

public class DummyTransactionDomainService : ITransactionDomainService
{
    public async Task<Result<ProcessLogonTransactionResponse>> ProcessLogonTransaction(TransactionCommands.ProcessLogonTransactionCommand command,
                                                                                       CancellationToken cancellationToken) {
        return Result.Success(new ProcessLogonTransactionResponse());
    }

    public async Task<Result<ProcessSaleTransactionResponse>> ProcessSaleTransaction(TransactionCommands.ProcessSaleTransactionCommand command,
                                                                                     CancellationToken cancellationToken) {
        return Result.Success(new ProcessSaleTransactionResponse());
    }

    public async Task<Result<ProcessReconciliationTransactionResponse>> ProcessReconciliationTransaction(TransactionCommands.ProcessReconciliationCommand command,
                                                                                                         CancellationToken cancellationToken) {
        return Result.Success(new ProcessReconciliationTransactionResponse());
    }

    public async Task<Result> ResendTransactionReceipt(TransactionCommands.ResendTransactionReceiptCommand command,
                                                           CancellationToken cancellationToken) =>
        Result.Success();

    public async Task<Result> CalculateFeesForTransaction(TransactionCommands.CalculateFeesForTransactionCommand command,
                                                          CancellationToken cancellationToken) =>
        Result.Success();

    public async Task<Result> AddSettledMerchantFee(TransactionCommands.AddSettledMerchantFeeCommand command,
                                                    CancellationToken cancellationToken) =>
        Result.Success();
}
