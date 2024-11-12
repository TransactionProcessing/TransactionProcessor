using System;
using MediatR;
using SimpleResults;
using TransactionProcessor.Models;

namespace TransactionProcessor.BusinessLogic.Requests;

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

public record VoucherQueries {
    public record GetVoucherByVoucherCodeQuery(Guid EstateId,String VoucherCode) : IRequest<Result<Models.Voucher>>;
    public record GetVoucherByTransactionIdQuery(Guid EstateId, Guid TransactionId) : IRequest<Result<Models.Voucher>>;
}