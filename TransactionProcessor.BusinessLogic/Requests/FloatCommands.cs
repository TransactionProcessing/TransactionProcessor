using MediatR;
using SimpleResults;
using System;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.DataTransferObjects.Requests.Float;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record FloatCommands {
    public record CreateFloatCommand(Guid EstateId,
                                     Guid OperatorId,
                                     DateTimeOffset CreateDateTime) : IRequest<Result<Guid>>;

    public record RecordCreditPurchaseForFloatCommand(Guid EstateId,
                                                      RecordFloatCreditPurchaseRequest RecordFloatCreditPurchaseRequest) : IRequest<Result>;
}