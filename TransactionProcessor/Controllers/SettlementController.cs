using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityService.DataTransferObjects.Responses;
using Shared.Results;
using Shared.Results.Web;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.DataTransferObjects.Responses.Settlement;
using TransactionProcessor.Models.Settlement;

namespace TransactionProcessor.Controllers
{
    using BusinessLogic.Common;
    using BusinessLogic.Requests;
    using DataTransferObjects;
    using MediatR;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using TransactionProcessor.Database.Entities;

    [ExcludeFromCodeCoverage]
    [Route(SettlementController.ControllerRoute)]
    [ApiController]
    [Authorize]
    public class SettlementController : ControllerBase
    {
        private readonly IMediator Mediator;
        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        private const String ControllerName = "settlements";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/estates/{estateId}/" + SettlementController.ControllerName;

        public SettlementController(IMediator mediator)
        {
            this.Mediator = mediator;
        }

        [HttpGet]
        [Route("{settlementDate}/merchants/{merchantId}/pending")]
        public async Task<IActionResult> GetPendingSettlement([FromRoute] DateTime settlementDate,
                                                              [FromRoute] Guid estateId,
                                                              [FromRoute] Guid merchantId,
                                                              CancellationToken cancellationToken)
        {
            SettlementQueries.GetPendingSettlementQuery query = new(settlementDate, merchantId, estateId);

            Result<SettlementAggregate> getPendingSettlementResult = await this.Mediator.Send(query, cancellationToken);
            if (getPendingSettlementResult.IsFailed)
                return getPendingSettlementResult.ToActionResultX();

            SettlementResponse settlementResponse = new SettlementResponse
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
        [Route("{settlementDate}/merchants/{merchantId}")]
        public async Task<IActionResult> ProcessSettlement([FromRoute] DateTime settlementDate,
                                                           [FromRoute] Guid estateId,
                                                           [FromRoute] Guid merchantId,
                                                           CancellationToken cancellationToken)
        {
            SettlementCommands.ProcessSettlementCommand command = new(settlementDate, merchantId, estateId);

            Result<Guid> result = await this.Mediator.Send(command, cancellationToken);

            return result.ToActionResultX();
        }

        [Route("{settlementId}")]
        [HttpGet]
        public async Task<IActionResult> GetSettlement([FromRoute] Guid estateId,
                                                       [FromQuery] Guid merchantId,
                                                       [FromRoute] Guid settlementId,
                                                       CancellationToken cancellationToken)
        {
            SettlementQueries.GetSettlementQuery query = new(estateId, merchantId, settlementId);
            var result = await this.Mediator.Send(query, cancellationToken);
            if (result.IsFailed)
                return result.ToActionResultX();

            DataTransferObjects.Responses.Settlement.SettlementResponse settlementResponse = new() {
                IsCompleted = result.Data.IsCompleted,
                NumberOfFeesSettled = result.Data.NumberOfFeesSettled,
                SettlementDate = result.Data.SettlementDate,
                SettlementFees = new(),
                SettlementId = result.Data.SettlementId,
                ValueOfFeesSettled = result.Data.ValueOfFeesSettled
            };

            foreach (SettlementFeeModel settlementFeeResponse in result.Data.SettlementFees)
            {
                settlementResponse.SettlementFees.Add(new()
                {
                    TransactionId = settlementFeeResponse.TransactionId,
                    MerchantId = settlementFeeResponse.MerchantId,
                    MerchantName = settlementFeeResponse.MerchantName,
                    SettlementDate = settlementFeeResponse.SettlementDate,
                    SettlementId = settlementFeeResponse.SettlementId,
                    CalculatedValue = settlementFeeResponse.CalculatedValue,
                    FeeDescription = settlementFeeResponse.FeeDescription,
                    IsSettled = settlementFeeResponse.IsSettled,
                    OperatorIdentifier = settlementFeeResponse.OperatorIdentifier
                });
            }

            return Result.Success(settlementResponse).ToActionResultX();
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> GetSettlements([FromRoute] Guid estateId,
                                                        [FromQuery] Guid? merchantId,
                                                        [FromQuery(Name = "start_date")] string startDate,
                                                        [FromQuery(Name = "end_date")] string endDate,
                                                        CancellationToken cancellationToken)
        {
            SettlementQueries.GetSettlementsQuery query = new(estateId, merchantId, startDate, endDate);
            var result = await this.Mediator.Send(query, cancellationToken);
            if (result.IsFailed)
                return result.ToActionResultX();

            var responses = new List<DataTransferObjects.Responses.Settlement.SettlementResponse>();
            foreach (SettlementModel settlementResponses in result.Data)
            {
                DataTransferObjects.Responses.Settlement.SettlementResponse sr = new DataTransferObjects.Responses.Settlement.SettlementResponse
                {
                    IsCompleted = settlementResponses.IsCompleted,
                    NumberOfFeesSettled = settlementResponses.NumberOfFeesSettled,
                    SettlementDate = settlementResponses.SettlementDate,
                    SettlementFees = new(),
                    SettlementId = settlementResponses.SettlementId,
                    ValueOfFeesSettled = settlementResponses.ValueOfFeesSettled
                };

                foreach (SettlementFeeModel settlementFeeResponse in settlementResponses.SettlementFees)
                {
                    sr.SettlementFees.Add(new()
                    {
                        TransactionId = settlementFeeResponse.TransactionId,
                        MerchantId = settlementFeeResponse.MerchantId,
                        MerchantName = settlementFeeResponse.MerchantName,
                        SettlementDate = settlementFeeResponse.SettlementDate,
                        SettlementId = settlementFeeResponse.SettlementId,
                        CalculatedValue = settlementFeeResponse.CalculatedValue,
                        FeeDescription = settlementFeeResponse.FeeDescription,
                        IsSettled = settlementFeeResponse.IsSettled,
                        OperatorIdentifier = settlementFeeResponse.OperatorIdentifier
                    });
                }
                responses.Add(sr);
            }


            return Result.Success(responses).ToActionResultX();
        }

        #endregion

    }
}
