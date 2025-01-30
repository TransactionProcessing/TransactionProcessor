using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Requests
{
    [ExcludeFromCodeCoverage]
    public class EstateQueries
    {
        public record GetEstateQuery(Guid EstateId) : IRequest<Result<Models.Estate.Estate>>;
        public record GetEstatesQuery(Guid EstateId) : IRequest<Result<List<Models.Estate.Estate>>>;
    }
}
