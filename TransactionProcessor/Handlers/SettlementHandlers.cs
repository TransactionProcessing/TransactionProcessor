using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using SimpleResults;
using Shared.Results.Web;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Factories;
using TransactionProcessor.Models.Settlement;

namespace TransactionProcessor.Handlers
{
    public static class SettlementHandlers
    {
        public static async Task<IResult> GetPendingSettlement(IMediator mediator, HttpContext ctx, DateTime settlementDate, Guid estateId, Guid merchantId, CancellationToken cancellationToken)
        {
            SettlementQueries.GetPendingSettlementQuery query = new(settlementDate, merchantId, estateId);
            Result<SettlementAggregate> getPendingSettlementResult = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(getPendingSettlementResult, r => new TransactionProcessor.DataTransferObjects.SettlementResponse
            {
                EstateId = r.EstateId,
                MerchantId = r.MerchantId,
                NumberOfFeesPendingSettlement = r.GetNumberOfFeesPendingSettlement(),
                NumberOfFeesSettled = r.GetNumberOfFeesSettled(),
                SettlementDate = r.SettlementDate,
                SettlementCompleted = r.SettlementComplete
            });
        }

        public static async Task<IResult> ProcessSettlement(IMediator mediator, HttpContext ctx, DateTime settlementDate, Guid estateId, Guid merchantId, CancellationToken cancellationToken)
        {
            SettlementCommands.ProcessSettlementCommand command = new(settlementDate, merchantId, estateId);
            Result<Guid> result = await mediator.Send(command, cancellationToken);
            return ResponseFactory.FromResult(result, guid => guid);
        }

        public static async Task<IResult> GetSettlement(IMediator mediator, HttpContext ctx, Guid estateId, Guid settlementId, Guid merchantId, CancellationToken cancellationToken)
        {
            SettlementQueries.GetSettlementQuery query = new(estateId, merchantId, settlementId);
            var result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, r =>
            {
                var settlementResponse = new TransactionProcessor.DataTransferObjects.Responses.Settlement.SettlementResponse
                {
                    IsCompleted = r.IsCompleted,
                    NumberOfFeesSettled = r.NumberOfFeesSettled,
                    SettlementDate = r.SettlementDate,
                    SettlementFees = new(),
                    SettlementId = r.SettlementId,
                    ValueOfFeesSettled = r.ValueOfFeesSettled
                };

                foreach (var settlementFeeResponse in r.SettlementFees)
                {
                    settlementResponse.SettlementFees.Add(new TransactionProcessor.DataTransferObjects.Responses.Settlement.SettlementFeeResponse
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

                return settlementResponse;
            });
        }

        public static async Task<IResult> GetSettlements(IMediator mediator, HttpContext ctx, Guid estateId, Guid? merchantId, string startDate, string endDate, CancellationToken cancellationToken)
        {
            SettlementQueries.GetSettlementsQuery query = new(estateId, merchantId, startDate, endDate);
            Result<List<SettlementModel>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, r =>
            {
                var responses = new List<TransactionProcessor.DataTransferObjects.Responses.Settlement.SettlementResponse>();
                foreach (var settlementResponses in r)
                {
                    var sr = new TransactionProcessor.DataTransferObjects.Responses.Settlement.SettlementResponse
                    {
                        IsCompleted = settlementResponses.IsCompleted,
                        NumberOfFeesSettled = settlementResponses.NumberOfFeesSettled,
                        SettlementDate = settlementResponses.SettlementDate,
                        SettlementFees = new(),
                        SettlementId = settlementResponses.SettlementId,
                        ValueOfFeesSettled = settlementResponses.ValueOfFeesSettled
                    };

                    foreach (var settlementFeeResponse in settlementResponses.SettlementFees)
                    {
                        sr.SettlementFees.Add(new TransactionProcessor.DataTransferObjects.Responses.Settlement.SettlementFeeResponse
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

                return responses;
            });
        }
    }
}