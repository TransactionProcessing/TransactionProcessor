using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    using Models;
    using Requests;
    using Services;
    using TransactionProcessor.BusinessLogic.Manager;

    public class VoucherManagementRequestHandler : IRequestHandler<VoucherCommands.IssueVoucherCommand, Result<IssueVoucherResponse>>,
                                                   IRequestHandler<VoucherCommands.RedeemVoucherCommand, Result<RedeemVoucherResponse>>,
                                                   IRequestHandler<VoucherQueries.GetVoucherByVoucherCodeQuery, Result<Voucher>>,
                                                   IRequestHandler<VoucherQueries.GetVoucherByTransactionIdQuery, Result<Voucher>>
    {
        #region Fields

        /// <summary>
        /// The voucher domain service
        /// </summary>
        private readonly IVoucherDomainService VoucherDomainService;

        private readonly IVoucherManagementManager VoucherManagementManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VoucherManagementRequestHandler" /> class.
        /// </summary>
        /// <param name="voucherDomainService">The voucher domain service.</param>
        public VoucherManagementRequestHandler(IVoucherDomainService voucherDomainService,
                                               IVoucherManagementManager voucherManagementManager) {
            this.VoucherDomainService = voucherDomainService;
            this.VoucherManagementManager = voucherManagementManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="command">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>
        /// Response from the request
        /// </returns>
        public async Task<Result<IssueVoucherResponse>> Handle(VoucherCommands.IssueVoucherCommand command,
                                                       CancellationToken cancellationToken)
        {
            return await this.VoucherDomainService.IssueVoucher(command.VoucherId,
                                                                command.OperatorId,
                                                                command.EstateId,
                                                                command.TransactionId,
                                                                command.IssuedDateTime,
                                                                command.Value,
                                                                command.RecipientEmail,
                                                                command.RecipientMobile,
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
        public async Task<Result<RedeemVoucherResponse>> Handle(VoucherCommands.RedeemVoucherCommand command,
                                                        CancellationToken cancellationToken)
        {
            return await this.VoucherDomainService.RedeemVoucher(command.EstateId, command.VoucherCode, command.RedeemedDateTime, cancellationToken);
        }

        #endregion

        public async Task<Result<Voucher>> Handle(VoucherQueries.GetVoucherByVoucherCodeQuery query,
                                                  CancellationToken cancellationToken) {
            return await this.VoucherManagementManager.GetVoucherByCode(query.EstateId, query.VoucherCode,
                cancellationToken);
        }

        public async Task<Result<Voucher>> Handle(VoucherQueries.GetVoucherByTransactionIdQuery query,
                                                  CancellationToken cancellationToken) {
            return await this.VoucherManagementManager.GetVoucherByTransactionId(query.EstateId, query.TransactionId,
                cancellationToken);
        }
    }
}
