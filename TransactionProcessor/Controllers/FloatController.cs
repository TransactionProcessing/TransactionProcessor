using Microsoft.AspNetCore.Mvc;

namespace TransactionProcessor.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Requests;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Models;

    [ExcludeFromCodeCoverage]
    [Route(FloatController.ControllerRoute)]
    [ApiController]
    [Authorize]
    public class FloatController : ControllerBase
    {
        private readonly IMediator Mediator;

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "floats";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/estates/{estateId}/" + FloatController.ControllerName;

        #endregion

        public FloatController(IMediator mediator){
            this.Mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFloatForContractProduct([FromRoute] Guid estateId, [FromBody] DataTransferObjects.CreateFloatForContractProductRequest createFloatRequest, CancellationToken cancellationToken){
            CreateFloatForContractProductRequest request = CreateFloatForContractProductRequest.Create(estateId, createFloatRequest.ContractId, createFloatRequest.ProductId,
                                                                                                       createFloatRequest.CreateDateTime);

            CreateFloatForContractProductResponse response = await this.Mediator.Send(request, cancellationToken);

            return this.Created("",
                                new DataTransferObjects.CreateFloatForContractProductResponse{
                                                                                                 FloatId = response.FloatId,
                                                                                             });
        }

        [HttpPut]
        public async Task<IActionResult> RecordFloatCreditPurchase([FromRoute] Guid estateId, [FromBody] DataTransferObjects.RecordFloatCreditPurchaseRequest recordFloatCreditPurchaseRequest, CancellationToken cancellationToken){
            RecordCreditPurchaseForFloatRequest request = RecordCreditPurchaseForFloatRequest.Create(estateId, recordFloatCreditPurchaseRequest.FloatId, recordFloatCreditPurchaseRequest.CreditAmount, recordFloatCreditPurchaseRequest.CostPrice, recordFloatCreditPurchaseRequest.PurchaseDateTime);

            await this.Mediator.Send(request, cancellationToken);

            return this.Ok();
        }
    }
}
