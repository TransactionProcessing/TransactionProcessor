using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.Models;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record VoucherCommands {
    public record IssueVoucherCommand(Guid VoucherId,
                                      Guid OperatorId,
                                      Guid EstateId,
                                      Guid TransactionId,
                                      DateTime IssuedDateTime,
                                      Decimal Value,
                                      String RecipientEmail,
                                      String RecipientMobile) : IRequest<Result<IssueVoucherResponse>>;

    public record RedeemVoucherCommand(Guid EstateId, String VoucherCode, DateTime RedeemedDateTime)
        : IRequest<Result<RedeemVoucherResponse>>;
}