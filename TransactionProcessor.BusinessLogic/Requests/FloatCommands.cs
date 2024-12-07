using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record FloatCommands {
    public record CreateFloatForContractProductCommand(Guid EstateId,
                                                       Guid ContractId,
                                                       Guid ProductId,
                                                       DateTime CreateDateTime) : IRequest<Result>;

    public record RecordCreditPurchaseForFloatCommand(Guid EstateId,
                                                      Guid FloatId,
                                                      Decimal CreditAmount,
                                                      Decimal CostPrice,
                                                      DateTime PurchaseDateTime) : IRequest<Result>;
}