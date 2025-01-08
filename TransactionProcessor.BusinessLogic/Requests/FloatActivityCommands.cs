using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record FloatActivityCommands {
    public record RecordCreditPurchaseCommand(Guid EstateId, Guid FloatId, DateTime CreditPurchasedDateTime, Decimal Amount, Guid CreditId) : IRequest<Result>;

    public record RecordTransactionCommand(Guid EstateId, Guid TransactionId) : IRequest<Result>;
}