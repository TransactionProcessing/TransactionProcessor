using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.Models.Operator;

[ExcludeFromCodeCoverage]
public class OperatorQueries
{
    public record GetOperatorQuery(Guid EstateId, Guid OperatorId) : IRequest<Result<Operator>>;

    public record GetOperatorsQuery(Guid EstateId) : IRequest<Result<List<Operator>>>;
}