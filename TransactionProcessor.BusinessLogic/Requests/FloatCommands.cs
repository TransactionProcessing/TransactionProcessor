using MediatR;
using SimpleResults;
using System;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.DataTransferObjects.Requests.Float;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record FloatCommands {
    public record CreateFloatCommand(Guid EstateId,
                                     Guid FloatId,
                                     DateTimeOffset CreateDateTime) : IRequest<Result>;

    public record RecordCreditPurchaseForFloatCommand(Guid EstateId,
                                                      RecordFloatCreditPurchaseRequest RecordFloatCreditPurchaseRequest) : IRequest<Result>;
}