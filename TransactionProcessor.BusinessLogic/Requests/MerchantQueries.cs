using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.ProjectionEngine.Models;
using TransactionProcessor.ProjectionEngine.State;

namespace TransactionProcessor.BusinessLogic.Requests;
[ExcludeFromCodeCoverage]
public record MerchantQueries {
    public record GetMerchantBalanceQuery(Guid EstateId, Guid MerchantId) : IRequest<Result<MerchantBalanceState>>;

    public record GetMerchantLiveBalanceQuery(Guid MerchantId) : IRequest<Result<MerchantBalanceProjectionState1>>;

    public record GetMerchantBalanceHistoryQuery(Guid EstateId, Guid MerchantId, DateTime StartDate, DateTime EndDate)
        : IRequest<Result<List<MerchantBalanceChangedEntry>>>;
}