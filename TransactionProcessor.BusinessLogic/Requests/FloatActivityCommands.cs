using System;
using MediatR;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Requests;

public record FloatActivityCommands {
    public record RecordCreditPurchaseCommand(Guid EstateId, Guid FloatId, DateTime CreditPurchasedDateTime, Decimal Amount) : IRequest<Result>;

    public record RecordTransactionCommand(Guid EstateId, Guid TransactionId) : IRequest<Result>;
}