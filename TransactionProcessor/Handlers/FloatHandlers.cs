using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using TransactionProcessing.UnionResults;
using TransactionProcessing.UnionResults.AspNetCore;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects.Requests.Float;

namespace TransactionProcessor.Handlers
{
    public static class FloatHandlers
    {
        public static async Task<IResultOutcome> CreateFloat(IMediator mediator, HttpContext ctx, Guid estateId, CreateFloatRequest createFloatRequest, CancellationToken cancellationToken)
        {
            FloatCommands.CreateFloatCommand command = new(estateId, createFloatRequest.OperatorId, createFloatRequest.CreateDateTime);
            Result<Guid> result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult<Guid>(result, (floatId) => result.GetValueOrThrow());
        }

        public static async Task<IResultOutcome> RecordFloatCreditPurchase(IMediator mediator, HttpContext ctx, Guid estateId, RecordFloatCreditPurchaseRequest recordFloatCreditPurchaseRequest, CancellationToken cancellationToken) {
            
            FloatCommands.RecordCreditPurchaseForFloatCommand command = new(estateId, recordFloatCreditPurchaseRequest);
            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }
    }
}