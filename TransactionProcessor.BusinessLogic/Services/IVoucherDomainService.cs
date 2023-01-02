using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Services
{
    using Models;

    public interface IVoucherDomainService
    {
        #region Methods

        /// <summary>
        /// Issues the voucher.
        /// </summary>
        /// <param name="voucherId">The voucher identifier.</param>
        /// <param name="operatorId">The operator identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="issuedDateTime">The issued date time.</param>
        /// <param name="value">The value.</param>
        /// <param name="recipientEmail">The recipient email.</param>
        /// <param name="recipientMobile">The recipient mobile.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IssueVoucherResponse> IssueVoucher(Guid voucherId,
                                                String operatorId,
                                                Guid estateId,
                                                Guid transactionId,
                                                DateTime issuedDateTime,
                                                Decimal value,
                                                String recipientEmail,
                                                String recipientMobile,
                                                CancellationToken cancellationToken);

        /// <summary>
        /// Redeems the voucher.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="voucherCode">The voucher code.</param>
        /// <param name="redeemedDateTime">The redeemed date time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<RedeemVoucherResponse> RedeemVoucher(Guid estateId,
                                                  String voucherCode,
                                                  DateTime redeemedDateTime,
                                                  CancellationToken cancellationToken);

        #endregion
    }
}
