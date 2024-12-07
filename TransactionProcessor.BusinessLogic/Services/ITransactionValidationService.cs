using System.Diagnostics.CodeAnalysis;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface ITransactionValidationService{
    #region Methods
    
    Task<Result<TransactionValidationResult>> ValidateLogonTransaction(Guid estateId,
                                                                        Guid merchantId,
                                                                        String deviceIdentifier,
                                                                        CancellationToken cancellationToken);
    Task<Result<TransactionValidationResult>> ValidateReconciliationTransaction(Guid estateId,
        Guid merchantId,
        String deviceIdentifier,
        CancellationToken cancellationToken);

    Task<Result<TransactionValidationResult>> ValidateSaleTransaction(Guid estateId,
                                                                       Guid merchantId,
                                                                       Guid contractId,
                                                                       Guid productId,
                                                                       String deviceIdentifier,
                                                                       Guid operatorId,
                                                                       Decimal? transactionAmount,
                                                                       CancellationToken cancellationToken);

    #endregion
}

[ExcludeFromCodeCoverage]
public record TransactionValidationResult(TransactionResponseCode ResponseCode, String ResponseMessage);