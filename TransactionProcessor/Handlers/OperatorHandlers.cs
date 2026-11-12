using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using TransactionProcessing.UnionResults;
using TransactionProcessing.UnionResults.AspNetCore;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects.Requests.Operator;
using TransactionProcessor.Factories;
using TransactionProcessor.Models.Operator;

namespace TransactionProcessor.Handlers
{
    public static class OperatorHandlers
    {
        public static async Task<IResultOutcome> CreateOperator(IMediator mediator, HttpContext ctx, Guid estateId, CreateOperatorRequest createOperatorRequest, CancellationToken cancellationToken)
        {
            OperatorCommands.CreateOperatorCommand command = new(estateId, createOperatorRequest);
            Result<Guid> result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult<Guid>(result, (operatorId) => result.GetValueOrThrow());
        }

        public static async Task<IResultOutcome> UpdateOperator(IMediator mediator, HttpContext ctx, Guid estateId, Guid operatorId, UpdateOperatorRequest updateOperatorRequest, CancellationToken cancellationToken)
        {
            OperatorCommands.UpdateOperatorCommand command = new(estateId, operatorId, updateOperatorRequest);
            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResultOutcome> GetOperator(IMediator mediator, HttpContext ctx, Guid estateId, Guid operatorId, CancellationToken cancellationToken)
        {
            OperatorQueries.GetOperatorQuery query = new(estateId, operatorId);
            Result<Operator> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }

        public static async Task<IResultOutcome> GetOperators(IMediator mediator, HttpContext ctx, Guid estateId, CancellationToken cancellationToken)
        {
            OperatorQueries.GetOperatorsQuery query = new(estateId);
            Result<List<Operator>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }
    }
}