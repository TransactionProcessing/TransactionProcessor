using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using TransactionProcessing.UnionResults;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record ContractQueries {
    public record GetContractQuery(Guid EstateId, Guid ContractId) : IRequest<Result<Models.Contract.Contract>>;
    public record GetContractsQuery(Guid EstateId) : IRequest<Result<List<Models.Contract.Contract>>>;
}