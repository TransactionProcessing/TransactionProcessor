using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.SettlementAggregates;

namespace TransactionProcessor.BusinessLogic.Requests;
[ExcludeFromCodeCoverage]
public record SettlementQueries {
    public record GetPendingSettlementQuery(DateTime SettlementDate, Guid MerchantId, Guid EstateId)
        : IRequest<Result<SettlementAggregate>>;
}