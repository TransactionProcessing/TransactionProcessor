using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models.Settlement;

namespace TransactionProcessor.BusinessLogic.Requests;
[ExcludeFromCodeCoverage]
public record SettlementQueries {
    public record GetPendingSettlementQuery(DateTime SettlementDate, Guid MerchantId, Guid EstateId)
        : IRequest<Result<PendingSettlementModel>>;
    public record GetSettlementQuery(Guid EstateId, Guid MerchantId, Guid SettlementId)
        : IRequest<Result<SettlementModel>>;

    public record GetSettlementsQuery(Guid EstateId, Guid? MerchantId, String StartDate, String EndDate)
        : IRequest<Result<List<SettlementModel>>>;
}