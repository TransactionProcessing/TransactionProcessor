using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record FloatCommands {
    public record CreateFloatCommand(Guid EstateId,
                                     Guid FloatId,
                                     DateTimeOffset CreateDateTime) : IRequest<Result>;

    public record RecordCreditPurchaseForFloatCommand(Guid EstateId,
                                                      Guid FloatId,
                                                      Decimal CreditAmount,
                                                      Decimal CostPrice,
                                                      DateTimeOffset PurchaseDateTime) : IRequest<Result>;
}