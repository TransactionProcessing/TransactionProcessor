using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using SimpleResults;
using Shared.Results.Web;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects.Requests.Float;

namespace TransactionProcessor.Handlers
{
    public static class FloatHandlers
    {
        public static async Task<IResult> CreateFloat(IMediator mediator, HttpContext ctx, Guid estateId, CreateFloatRequest createFloatRequest, CancellationToken cancellationToken)
        {
            FloatCommands.CreateFloatCommand command = new(estateId, createFloatRequest.FloatId, createFloatRequest.CreateDateTime);
            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResult> RecordFloatCreditPurchase(IMediator mediator, HttpContext ctx, Guid estateId, RecordFloatCreditPurchaseRequest recordFloatCreditPurchaseRequest, CancellationToken cancellationToken) {
            
            FloatCommands.RecordCreditPurchaseForFloatCommand command = new(estateId, recordFloatCreditPurchaseRequest);
            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }
    }
}