namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using Models;

public class DummyTransactionDomainService : ITransactionDomainService
{
    public async Task<ProcessLogonTransactionResponse> ProcessLogonTransaction(Guid transactionId,
                                                                               Guid estateId,
                                                                               Guid merchantId,
                                                                               DateTime transactionDateTime,
                                                                               String transactionNumber,
                                                                               String deviceIdentifier,
                                                                               CancellationToken cancellationToken) {
        return new ProcessLogonTransactionResponse();
    }

    public async Task<ProcessSaleTransactionResponse> ProcessSaleTransaction(Guid transactionId,
                                                                             Guid estateId,
                                                                             Guid merchantId,
                                                                             DateTime transactionDateTime,
                                                                             String transactionNumber,
                                                                             String deviceIdentifier,
                                                                             Guid operatorId,
                                                                             String customerEmailAddress,
                                                                             Dictionary<String, String> additionalTransactionMetadata,
                                                                             Guid contractId,
                                                                             Guid productId,
                                                                             Int32 transactionSource,
                                                                             CancellationToken cancellationToken) {
        return new ProcessSaleTransactionResponse();
    }

    public async Task<ProcessReconciliationTransactionResponse> ProcessReconciliationTransaction(Guid transactionId,
                                                                                                 Guid estateId,
                                                                                                 Guid merchantId,
                                                                                                 String deviceIdentifier,
                                                                                                 DateTime transactionDateTime,
                                                                                                 Int32 transactionCount,
                                                                                                 Decimal transactionValue,
                                                                                                 CancellationToken cancellationToken) {
        return new ProcessReconciliationTransactionResponse();
    }

    public async Task ResendTransactionReceipt(Guid transactionId,
                                               Guid estateId,
                                               CancellationToken cancellationToken) {            
    }
}
