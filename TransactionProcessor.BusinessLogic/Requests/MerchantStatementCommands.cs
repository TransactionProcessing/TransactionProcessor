using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using TransactionProcessing.Shared.ValueObjects;
using TransactionProcessing.UnionResults;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record MerchantStatementCommands {
    public record AddTransactionToMerchantStatementCommand(Guid EstateId,
                                                           Guid MerchantId,
                                                           DateTimeOffset TransactionDateTime,
                                                           Money? TransactionAmount,
                                                           Boolean IsAuthorised,
                                                           Guid TransactionId) : IRequest<Result>;

    public record AddDepositToMerchantStatementCommand(Guid EstateId,
                                                       Guid MerchantId,
                                                       Guid DepositId,
                                                       String Reference,
                                                       DateTimeOffset DepositDateTime, 
                                                       PositiveMoney Amount) : IRequest<Result>;

    public record AddWithdrawalToMerchantStatementCommand(Guid EstateId,
                                                          Guid MerchantId,
                                                          Guid WithdrawalId,
                                                          DateTimeOffset WithdrawalDateTime,
                                                          PositiveMoney Amount) : IRequest<Result>;

    public record BuildMerchantStatementCommand(Guid EstateId,
                                                Guid MerchantId,
                                                Guid MerchantStatementId) : IRequest<Result>;

    public record EmailMerchantStatementCommand(Guid EstateId,
                                                Guid MerchantStatementId,
        String pdfData) : IRequest<Result>;

    public record AddSettledFeeToMerchantStatementCommand(Guid EstateId,
                                                          Guid MerchantId,
                                                          DateTimeOffset SettledDateTime,
                                                          PositiveMoney SettledAmount,
                                                          Guid TransactionId,
                                                          Guid SettledFeeId) : IRequest<Result>;

    public record RecordActivityDateOnMerchantStatementCommand(Guid EstateId,
                                                 Guid MerchantId,
                                                 Guid MerchantStatementId,
                                                 DateTimeOffset StatementDate,
                                                 Guid MerchantStatementForDateId,
                                                 DateTimeOffset StatementActivityDate) : IRequest<Result>;
}