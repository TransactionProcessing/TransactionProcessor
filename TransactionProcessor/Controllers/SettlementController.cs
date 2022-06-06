using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        [Route("{settlementDate}/estates/{estateId}/pending")]
        public async Task<IActionResult> GetPendingSettlement([FromRoute] DateTime settlementDate,
                                                        [FromRoute] Guid estateId,
                                                        CancellationToken cancellationToken)
        {
            // TODO: Convert to using a manager/model/factory
            // Convert the date passed in to a guid
            Guid aggregateId = Helpers.CalculateSettlementAggregateId(settlementDate, estateId);

            Logger.LogInformation($"Settlement Aggregate Id {aggregateId}");

            var settlementAggregate = await this.SettlmentAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);

            var settlementResponse = new SettlementResponse
                                            {
                                                EstateId = settlementAggregate.EstateId,
                                                NumberOfFeesPendingSettlement = settlementAggregate.GetNumberOfFeesPendingSettlement(),
                                                NumberOfFeesSettled = settlementAggregate.GetNumberOfFeesSettled(),
                                                SettlementDate = settlementAggregate.SettlementDate,
                                                SettlementCompleted = settlementAggregate.SettlementComplete
                                            };

            return this.Ok(settlementResponse);

        }

        [HttpPost]
        [Route("{settlementDate}/estates/{estateId}")]
        public async Task<IActionResult> ProcessSettlement([FromRoute] DateTime settlementDate,
                                                           [FromRoute] Guid estateId,
                                                           CancellationToken cancellationToken)
        {
            ProcessSettlementRequest command = ProcessSettlementRequest.Create(settlementDate, estateId);

            var processSettlementResponse = await this.Mediator.Send(command, cancellationToken);

            return this.Ok();
        }

        #endregion

    }
}
