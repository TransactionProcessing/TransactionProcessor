using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TransactionProcessor.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Common;
    using DataTransferObjects;
    using Microsoft.AspNetCore.Authorization;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;

    [ExcludeFromCodeCoverage]
    [Route(SettlementController.ControllerRoute)]
    [ApiController]
    [Authorize]
    public class SettlementController : ControllerBase
    {
        private readonly IAggregateRepository<PendingSettlementAggregate, DomainEventRecord.DomainEvent> PendingSettlmentAggregateRepository;

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "settlements";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + SettlementController.ControllerName;

        public SettlementController(IAggregateRepository<PendingSettlementAggregate, DomainEventRecord.DomainEvent> pendingSettlmentAggregateRepository)
        {
            this.PendingSettlmentAggregateRepository = pendingSettlmentAggregateRepository;
        }

        [HttpGet]
        [Route("{pendingSettlementDate}/estates/{estateId}/pending")]
        public async Task<IActionResult> GetPendingSettlement([FromRoute] DateTime pendingSettlementDate,
                                                        [FromRoute] Guid estateId,
                                                        CancellationToken cancellationToken)
        {
            // TODO: Convert to using a manager/model/factory
            // Convert the date passed in to a guid
            var aggregateId = pendingSettlementDate.Date.ToGuid();

            var pendingSettlementAggregate = await this.PendingSettlmentAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);

            var pendingSettlementResponse = new PendingSettlementResponse
                                            {
                                                EstateId = pendingSettlementAggregate.EstateId,
                                                NumberOfFeesPendingSettlement = pendingSettlementAggregate.GetNumberOfFeesPendingSettlement(),
                                                NumberOfFeesSettled = pendingSettlementAggregate.GetNumberOfFeesSettled(),
                                                SettlementDate = pendingSettlementAggregate.SettlmentDate,
                                            };

            return this.Ok(pendingSettlementResponse);

        }
        #endregion

    }
}
