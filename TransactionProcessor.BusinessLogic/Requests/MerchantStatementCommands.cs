using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record MerchantStatementCommands {
    public record AddTransactionToMerchantStatementCommand(Guid EstateId,
                                                           Guid MerchantId,
                                                           DateTime TransactionDateTime,
                                                           Decimal? TransactionAmount,
                                                           Boolean IsAuthorised,
                                                           Guid TransactionId) : IRequest<Result>;

    public record EmailMerchantStatementCommand(Guid EstateId,
                                                Guid MerchantId,
                                                Guid MerchantStatementId) : IRequest<Result>;

    public record AddSettledFeeToMerchantStatementCommand(Guid EstateId,
                                                          Guid MerchantId,
                                                          DateTime SettledDateTime,
                                                          Decimal SettledAmount,
                                                          Guid TransactionId,
                                                          Guid SettledFeeId) : IRequest<Result>;
}