using Shared.EventStore.Aggregate;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Manager;
    using Common.Examples;
    using DataTransferObjects;
    using Factories;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Shared.General;
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.Filters;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
    using IssueVoucherResponse = Models.IssueVoucherResponse;
    using RedeemVoucherResponse = Models.RedeemVoucherResponse;

    [ExcludeFromCodeCoverage]
    [Route(VoucherController.ControllerRoute)]
    [ApiController]
    [Authorize]
    public class VoucherController : ControllerBase
    {
        #region Fields

        private readonly IMediator Mediator;

        private readonly IVoucherManagementManager VoucherManagementManager;

        private readonly IModelFactory ModelFactory;

        #endregion

        #region Constructors

        public VoucherController(IMediator mediator,
                                 IVoucherManagementManager voucherManagementManager,
                                 IModelFactory modelFactory)
        {
            this.Mediator = mediator;
            this.VoucherManagementManager = voucherManagementManager;
            this.ModelFactory = modelFactory;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Redeems the voucher.
        /// </summary>
        /// <param name="redeemVoucherRequest">The redeem voucher request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerResponse(200, "OK", typeof(RedeemVoucherResponse))]
        [SwaggerResponseExample(200, typeof(RedeemVoucherResponseExample))]
        public async Task<IActionResult> RedeemVoucher(RedeemVoucherRequest redeemVoucherRequest,
                                                       CancellationToken cancellationToken)
        {
            // Reject password tokens
            if (ClaimsHelper.IsPasswordToken(this.User))
            {
                return this.Forbid();
            }

            DateTime redeemedDateTime = redeemVoucherRequest.RedeemedDateTime.HasValue ? redeemVoucherRequest.RedeemedDateTime.Value : DateTime.Now;
            VoucherCommands.RedeemVoucherCommand command = new(redeemVoucherRequest.EstateId, redeemVoucherRequest.VoucherCode, redeemedDateTime);

            Result<RedeemVoucherResponse> result = await this.Mediator.Send(command, cancellationToken);
            if (result.IsFailed)
                ResultHelpers.CreateFailure(result).ToActionResultX();

            return this.ModelFactory.ConvertFrom(result.Data).ToActionResultX();
        }

        /// <summary>
        /// Gets the voucher by code.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="voucherCode">The voucher code.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse(200, "OK", typeof(GetVoucherResponse))]
        [SwaggerResponseExample(200, typeof(GetVoucherResponseExample))]
        public async Task<IActionResult> GetVoucher([FromQuery] Guid estateId,
                                                    [FromQuery] String voucherCode,
                                                    [FromQuery] Guid transactionId,
                                                    CancellationToken cancellationToken) {
            // Reject password tokens
            if (ClaimsHelper.IsPasswordToken(this.User)) {
                return this.Forbid();
            }

            if (String.IsNullOrEmpty(voucherCode) == false) {
                VoucherQueries.GetVoucherByVoucherCodeQuery queryByVoucherCode = new(estateId, voucherCode);
                // By code is priority
                Result<Voucher> getVoucherByCodeResult = await this.Mediator.Send(queryByVoucherCode, cancellationToken);
                if (getVoucherByCodeResult.IsFailed)
                    ResultHelpers.CreateFailure(getVoucherByCodeResult).ToActionResultX();

                return this.ModelFactory.ConvertFrom(getVoucherByCodeResult.Data).ToActionResultX();
            }

            if (transactionId != Guid.Empty) {
                // By transaction id is an additional filter
                VoucherQueries.GetVoucherByTransactionIdQuery queryByTransactionId = new(estateId, transactionId);
                Result<Voucher> getVoucherByTransactionIdResult = await this.Mediator.Send(queryByTransactionId, cancellationToken);
                if (getVoucherByTransactionIdResult.IsFailed)
                    ResultHelpers.CreateFailure(getVoucherByTransactionIdResult).ToActionResultX();

                return this.ModelFactory.ConvertFrom(getVoucherByTransactionIdResult.Data).ToActionResultX();
            }

            return Result.Invalid().ToActionResultX();
        }
        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "vouchers";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + VoucherController.ControllerName;

        #endregion
    }
}
