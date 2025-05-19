using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using Shared.ValueObjects;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record MerchantStatementCommands {
    public record AddTransactionToMerchantStatementCommand(Guid EstateId,
                                                           Guid MerchantId,
                                                           DateTime TransactionDateTime,
                                                           Money? TransactionAmount,
                                                           Boolean IsAuthorised,
                                                           Guid TransactionId) : IRequest<Result>;

    public record AddDepositToMerchantStatementCommand(Guid EstateId,
                                                       Guid MerchantId,
                                                       Guid DepositId,
                                                       String Reference, 
                                                       DateTime DepositDateTime, 
                                                       PositiveMoney Amount) : IRequest<Result>;

    public record AddWithdrawalToMerchantStatementCommand(Guid EstateId,
                                                          Guid MerchantId,
                                                          Guid WithdrawalId, 
                                                          DateTime WithdrawalDateTime,
                                                          PositiveMoney Amount) : IRequest<Result>;

    public record BuildMerchantStatementCommand(Guid EstateId,
                                                Guid MerchantId,
                                                Guid MerchantStatementId) : IRequest<Result>;

    public record EmailMerchantStatementCommand(Guid EstateId,
                                                Guid MerchantStatementId,
        String pdfData) : IRequest<Result>;

    public record AddSettledFeeToMerchantStatementCommand(Guid EstateId,
                                                          Guid MerchantId,
                                                          DateTime SettledDateTime,
                                                          PositiveMoney SettledAmount,
                                                          Guid TransactionId,
                                                          Guid SettledFeeId) : IRequest<Result>;

    public record RecordActivityDateOnMerchantStatementCommand(Guid EstateId,
                                                 Guid MerchantId,
                                                 Guid MerchantStatementId,
                                                 DateTime StatementDate,
                                                 Guid MerchantStatementForDateId,
                                                 DateTime StatementActivityDate) : IRequest<Result>;
}