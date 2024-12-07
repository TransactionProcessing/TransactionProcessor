using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Results;
using SimpleResults;

namespace TransactionProcessor.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Common;
    using BusinessLogic.Requests;
    using DataTransferObjects;
    using Factories;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Models;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Logger;

    [ExcludeFromCodeCoverage]
    [Route(SettlementController.ControllerRoute)]
    [ApiController]
    [Authorize]
    public class SettlementController : ControllerBase
    {
        private readonly IAggregateRepository<SettlementAggregate, DomainEvent> SettlmentAggregateRepository;

        private readonly IMediator Mediator;

        private readonly IModelFactory ModelFactory;

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "settlements";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + SettlementController.ControllerName;

        public SettlementController(IAggregateRepository<SettlementAggregate, DomainEvent> settlmentAggregateRepository,
                                    IMediator mediator,
                                    IModelFactory modelFactory)
        {
            this.SettlmentAggregateRepository = settlmentAggregateRepository;
            this.Mediator = mediator;
            this.ModelFactory = modelFactory;
        }

        [HttpGet]
        [Route("{settlementDate}/estates/{estateId}/merchants/{merchantId}/pending")]
        public async Task<IActionResult> GetPendingSettlement([FromRoute] DateTime settlementDate,
                                                              [FromRoute] Guid estateId,
                                                              [FromRoute] Guid merchantId,
                                                              CancellationToken cancellationToken)
        {
            //// TODO: Convert to using a manager/model/factory
            //// Convert the date passed in to a guid
            //Guid aggregateId = Helpers.CalculateSettlementAggregateId(settlementDate, merchantId, estateId);

            //Logger.LogInformation($"Settlement Aggregate Id {aggregateId}");

            //var getSettlementResult = await this.SettlmentAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);
            //if (getSettlementResult.IsFailed)
            //    return ResultHelpers.CreateFailure(getSettlementResult).ToActionResultX();

            //var settlementAggregate = getSettlementResult.Data;
            SettlementQueries.GetPendingSettlementQuery query = new(settlementDate, merchantId, estateId);

            Result<SettlementAggregate> getPendingSettlementResult = await this.Mediator.Send(query, cancellationToken);
            if (getPendingSettlementResult.IsFailed)
                return getPendingSettlementResult.ToActionResultX();

            var settlementResponse = new SettlementResponse
                                            {
                                                EstateId = getPendingSettlementResult.Data.EstateId,
                                                MerchantId = getPendingSettlementResult.Data.MerchantId,
                                                NumberOfFeesPendingSettlement = getPendingSettlementResult.Data.GetNumberOfFeesPendingSettlement(),
                                                NumberOfFeesSettled = getPendingSettlementResult.Data.GetNumberOfFeesSettled(),
                                                SettlementDate = getPendingSettlementResult.Data.SettlementDate,
                                                SettlementCompleted = getPendingSettlementResult.Data.SettlementComplete
                                            };

            return Result.Success(settlementResponse).ToActionResultX();

        }

        [HttpPost]
        [Route("{settlementDate}/estates/{estateId}/merchants/{merchantId}")]
        public async Task<IActionResult> ProcessSettlement([FromRoute] DateTime settlementDate,
                                                           [FromRoute] Guid estateId,
                                                           [FromRoute] Guid merchantId,
                                                           CancellationToken cancellationToken)
        {
            SettlementCommands.ProcessSettlementCommand command = new(settlementDate, merchantId, estateId);

            Result<Guid> result = await this.Mediator.Send(command, cancellationToken);

            return result.ToActionResultX();
        }

        #endregion

    }
}
