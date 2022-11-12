using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    using Models;
    using Requests;
    using Services;

    public class VoucherManagementRequestHandler : IRequestHandler<IssueVoucherRequest, IssueVoucherResponse>,
                                                   IRequestHandler<RedeemVoucherRequest, RedeemVoucherResponse>
    {
        #region Fields

        /// <summary>
        /// The voucher domain service
        /// </summary>
        private readonly IVoucherDomainService VoucherDomainService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VoucherManagementRequestHandler" /> class.
        /// </summary>
        /// <param name="voucherDomainService">The voucher domain service.</param>
        public VoucherManagementRequestHandler(IVoucherDomainService voucherDomainService)
        {
            this.VoucherDomainService = voucherDomainService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// Response from the request
        /// </returns>
        public async Task<IssueVoucherResponse> Handle(IssueVoucherRequest request,
                                                       CancellationToken cancellationToken)
        {
            return await this.VoucherDomainService.IssueVoucher(request.VoucherId,
                                                                request.OperatorIdentifier,
                                                                request.EstateId,
                                                                request.TransactionId,
                                                                request.IssuedDateTime,
                                                                request.Value,
                                                                request.RecipientEmail,
                                                                request.RecipientMobile,
                                                                cancellationToken);
        }

        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// Response from the request
        /// </returns>
        public async Task<RedeemVoucherResponse> Handle(RedeemVoucherRequest request,
                                                        CancellationToken cancellationToken)
        {
            return await this.VoucherDomainService.RedeemVoucher(request.EstateId, request.VoucherCode, request.RedeemedDateTime, cancellationToken);
        }

        #endregion
    }
}
