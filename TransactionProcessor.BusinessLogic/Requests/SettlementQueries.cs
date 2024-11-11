using System;
using MediatR;
using SimpleResults;
using TransactionProcessor.SettlementAggregates;

namespace TransactionProcessor.BusinessLogic.Requests;

public record SettlementQueries {
    public record GetPendingSettlementQuery(DateTime SettlementDate, Guid MerchantId, Guid EstateId)
        : IRequest<Result<SettlementAggregate>>;
}