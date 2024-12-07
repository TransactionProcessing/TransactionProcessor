using Microsoft.AspNetCore.Mvc;
using Shared.Results;
using SimpleResults;

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
            FloatCommands.CreateFloatForContractProductCommand command = new(estateId, createFloatRequest.ContractId, createFloatRequest.ProductId,
                                                                                                       createFloatRequest.CreateDateTime);

            Result result= await this.Mediator.Send(command, cancellationToken);

            return result.ToActionResultX();
        }

        [HttpPut]
        public async Task<IActionResult> RecordFloatCreditPurchase([FromRoute] Guid estateId, [FromBody] DataTransferObjects.RecordFloatCreditPurchaseRequest recordFloatCreditPurchaseRequest, CancellationToken cancellationToken){
            FloatCommands.RecordCreditPurchaseForFloatCommand command = new(estateId, recordFloatCreditPurchaseRequest.FloatId, recordFloatCreditPurchaseRequest.CreditAmount, recordFloatCreditPurchaseRequest.CostPrice, recordFloatCreditPurchaseRequest.PurchaseDateTime);

            Result result = await this.Mediator.Send(command, cancellationToken);

            return result.ToActionResultX();
        }
    }
}
