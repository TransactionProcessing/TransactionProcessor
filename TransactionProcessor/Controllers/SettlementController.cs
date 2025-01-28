using EstateManagement.Client;
using EstateManagement.DataTransferObjects.Responses.Settlement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecurityService.Client;
using SecurityService.DataTransferObjects.Responses;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Aggregates;

namespace TransactionProcessor.Controllers
{
    using System;
    using System.Collections.Generic;
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
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Logger;
    using TransactionProcessor.Database.Entities;

    [ExcludeFromCodeCoverage]
    [Route(SettlementController.ControllerRoute)]
    [ApiController]
    [Authorize]
    public class SettlementController : ControllerBase
    {
        private readonly IAggregateRepository<SettlementAggregate, DomainEvent> SettlmentAggregateRepository;

        private readonly IMediator Mediator;
        private readonly IEstateClient EstateClient;
        private readonly ISecurityServiceClient SecurityServiceClient;

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "settlements";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + SettlementController.ControllerName;

        public SettlementController(IAggregateRepository<SettlementAggregate, DomainEvent> settlementAggregateRepository,
                                    IMediator mediator,
                                    IEstateClient estateClient,
                                    ISecurityServiceClient securityServiceClient)
        {
            this.SettlmentAggregateRepository = settlementAggregateRepository;
            this.Mediator = mediator;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
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
        private TokenResponse TokenResponse;

        [Route("{settlementId}")]
        [HttpGet]
        public async Task<IActionResult> GetSettlement([FromRoute] Guid estateId,
                                                       [FromQuery] Guid merchantId,
                                                       [FromRoute] Guid settlementId,
                                                       CancellationToken cancellationToken)
        {
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            var result = await this.EstateClient.GetSettlement(this.TokenResponse.AccessToken, estateId, merchantId, settlementId, cancellationToken);

            DataTransferObjects.Responses.Settlement.SettlementResponse settlementResponse = new DataTransferObjects.Responses.Settlement.SettlementResponse {
                IsCompleted = result.Data.IsCompleted,
                NumberOfFeesSettled = result.Data.NumberOfFeesSettled,
                SettlementDate = result.Data.SettlementDate,
                SettlementFees = new(),
                SettlementId = result.Data.SettlementId,
                ValueOfFeesSettled = result.Data.ValueOfFeesSettled
            };

            foreach (SettlementFeeResponse settlementFeeResponse in result.Data.SettlementFees) {
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
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            var result = await this.EstateClient.GetSettlements(this.TokenResponse.AccessToken, estateId, merchantId, startDate, endDate, cancellationToken);
            if (result.IsFailed)
                return result.ToActionResultX();

            var responses = new List<DataTransferObjects.Responses.Settlement.SettlementResponse>();
            foreach (EstateManagement.DataTransferObjects.Responses.Settlement.SettlementResponse settlementResponses in result.Data) {
                DataTransferObjects.Responses.Settlement.SettlementResponse sr = new DataTransferObjects.Responses.Settlement.SettlementResponse
                {
                    IsCompleted = settlementResponses.IsCompleted,
                    NumberOfFeesSettled = settlementResponses.NumberOfFeesSettled,
                    SettlementDate = settlementResponses.SettlementDate,
                    SettlementFees = new(),
                    SettlementId = settlementResponses.SettlementId,
                    ValueOfFeesSettled = settlementResponses.ValueOfFeesSettled
                };

                foreach (SettlementFeeResponse settlementFeeResponse in settlementResponses.SettlementFees)
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
