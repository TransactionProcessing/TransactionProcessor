using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record VoucherQueries {
    public record GetVoucherByVoucherCodeQuery(Guid EstateId,String VoucherCode) : IRequest<Result<Models.Voucher>>;
    public record GetVoucherByTransactionIdQuery(Guid EstateId, Guid TransactionId) : IRequest<Result<Models.Voucher>>;
}