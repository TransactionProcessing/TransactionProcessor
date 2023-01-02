namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using Models;

public class DummyVoucherDomainService : IVoucherDomainService
{
    public async Task<IssueVoucherResponse> IssueVoucher(Guid voucherId,
                                                         String operatorId,
                                                         Guid estateId,
                                                         Guid transactionId,
                                                         DateTime issuedDateTime,
                                                         Decimal value,
                                                         String recipientEmail,
                                                         String recipientMobile,
                                                         CancellationToken cancellationToken) {
        return new IssueVoucherResponse();
    }

    public async Task<RedeemVoucherResponse> RedeemVoucher(Guid estateId,
                                                           String voucherCode,
                                                           DateTime redeemedDateTime,
                                                           CancellationToken cancellationToken) {
        return new RedeemVoucherResponse();
    }
}