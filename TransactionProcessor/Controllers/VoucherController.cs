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
            BusinessLogic.Requests.RedeemVoucherRequest request = BusinessLogic.Requests.RedeemVoucherRequest.Create(redeemVoucherRequest.EstateId, redeemVoucherRequest.VoucherCode, redeemedDateTime);

            RedeemVoucherResponse response = await this.Mediator.Send(request, cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(response));
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

            Voucher voucherModel = null;
            if (String.IsNullOrEmpty(voucherCode) == false) {
                {
                    // By code is priority
                    voucherModel = await this.VoucherManagementManager.GetVoucherByCode(estateId, voucherCode, cancellationToken);
                    return this.Ok(this.ModelFactory.ConvertFrom(voucherModel));
                }
            }

            if (transactionId != Guid.Empty) {
                // By transaction id is an additional filter
                voucherModel = await this.VoucherManagementManager.GetVoucherByTransactionId(estateId, transactionId, cancellationToken);
                return this.Ok(this.ModelFactory.ConvertFrom(voucherModel));
            }

            return this.NoContent();
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
