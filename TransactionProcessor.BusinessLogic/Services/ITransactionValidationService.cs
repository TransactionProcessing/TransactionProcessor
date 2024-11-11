using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface ITransactionValidationService{
    #region Methods

    Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateLogonTransaction(Guid estateId,
                                                                                                  Guid merchantId,
                                                                                                  String deviceIdentifier,
                                                                                                  CancellationToken cancellationToken);

    Task<Result<TransactionValidationResult>> ValidateLogonTransactionX(Guid estateId,
                                                                        Guid merchantId,
                                                                        String deviceIdentifier,
                                                                        CancellationToken cancellationToken);

    Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateReconciliationTransaction(Guid estateId,
                                                                                                           Guid merchantId,
                                                                                                           String deviceIdentifier,
                                                                                                           CancellationToken cancellationToken);

    Task<Result<TransactionValidationResult>> ValidateReconciliationTransactionX(Guid estateId,
        Guid merchantId,
        String deviceIdentifier,
        CancellationToken cancellationToken);

    Task<(String responseMessage, TransactionResponseCode responseCode)> ValidateSaleTransaction(Guid estateId,
                                                                                                 Guid merchantId,
                                                                                                 Guid contractId,
                                                                                                 Guid productId,
                                                                                                 String deviceIdentifier,
                                                                                                 Guid operatorId,
                                                                                                 Decimal? transactionAmount,
                                                                                                 CancellationToken cancellationToken);

    Task<Result<TransactionValidationResult>> ValidateSaleTransactionX(Guid estateId,
                                                                       Guid merchantId,
                                                                       Guid contractId,
                                                                       Guid productId,
                                                                       String deviceIdentifier,
                                                                       Guid operatorId,
                                                                       Decimal? transactionAmount,
                                                                       CancellationToken cancellationToken);

    #endregion
}

public record TransactionValidationResult(TransactionResponseCode ResponseCode, String ResponseMessage);