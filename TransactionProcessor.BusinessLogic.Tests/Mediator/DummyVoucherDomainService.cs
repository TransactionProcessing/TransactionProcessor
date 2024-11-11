using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using Models;

public class DummyVoucherDomainService : IVoucherDomainService
{
    public async Task<Result<IssueVoucherResponse>> IssueVoucher(Guid voucherId,
                                                                 Guid operatorId,
                                                                 Guid estateId,
                                                                 Guid transactionId,
                                                                 DateTime issuedDateTime,
                                                                 Decimal value,
                                                                 String recipientEmail,
                                                                 String recipientMobile,
                                                                 CancellationToken cancellationToken) {
        return Result.Success(new IssueVoucherResponse());
    }

    public async Task<Result<RedeemVoucherResponse>> RedeemVoucher(Guid estateId,
                                                                   String voucherCode,
                                                                   DateTime redeemedDateTime,
                                                                   CancellationToken cancellationToken) {
        return Result.Success(new RedeemVoucherResponse());
    }
}

