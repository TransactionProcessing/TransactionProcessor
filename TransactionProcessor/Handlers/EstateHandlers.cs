using MediatR;
using Microsoft.AspNetCore.Http;
using TransactionProcessing.UnionResults;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.Factories;
using TransactionProcessor.DataTransferObjects.Responses.Estate;

namespace TransactionProcessor.Handlers;

public static class EstateHandlers
{
    public static async Task<Result<Guid>> CreateEstate(IMediator mediator, HttpContext context, CreateEstateRequest createEstateRequest, CancellationToken cancellationToken)
    {
        EstateCommands.CreateEstateCommand command = new(createEstateRequest);
        Result<Guid> result = await mediator.Send(command, cancellationToken);

        return result;
    }

    public static async Task<Result<EstateResponse>> GetEstate(IMediator mediator, HttpContext context, Guid estateId, CancellationToken cancellationToken)
    {
        EstateQueries.GetEstateQuery query = new(estateId);
        Result<Models.Estate.Estate> result = await mediator.Send(query, cancellationToken);

        return result.Map(ModelFactory.ConvertFrom);
    }

    public static async Task<Result<List<EstateResponse>>> GetEstates(IMediator mediator, HttpContext context, Guid estateId, CancellationToken cancellationToken)
    {
        EstateQueries.GetEstatesQuery query = new(estateId);
        Result<List<Models.Estate.Estate>> result = await mediator.Send(query, cancellationToken);

        return result.Map(ModelFactory.ConvertFrom);
    }

    public static async Task<Result> CreateEstateUser(IMediator mediator, HttpContext context, Guid estateId, CreateEstateUserRequest createEstateUserRequest, CancellationToken cancellationToken)
    {
        EstateCommands.CreateEstateUserCommand command = new(estateId, createEstateUserRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return result;
    }

    public static async Task<Result> AssignOperator(IMediator mediator, HttpContext context, Guid estateId, AssignOperatorRequest assignOperatorRequest, CancellationToken cancellationToken)
    {
        EstateCommands.AddOperatorToEstateCommand command = new(estateId, assignOperatorRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return result;
    }

    public static async Task<Result> RemoveOperator(IMediator mediator, HttpContext context, Guid estateId, Guid operatorId, CancellationToken cancellationToken)
    {
        EstateCommands.RemoveOperatorFromEstateCommand command = new(estateId, operatorId);
        Result result = await mediator.Send(command, cancellationToken);

        return result;
    }
}