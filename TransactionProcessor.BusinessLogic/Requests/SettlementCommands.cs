using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.Models;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.BusinessLogic.Requests;
[ExcludeFromCodeCoverage]
public record SettlementCommands {
    public record ProcessSettlementCommand(DateTime SettlementDate, Guid MerchantId, Guid EstateId)
        : IRequest<Result<Guid>>;

    public record AddMerchantFeePendingSettlementCommand(Guid TransactionId,
                                                         Decimal CalculatedValue,
                                                         DateTime FeeCalculatedDateTime,
                                                         CalculationType FeeCalculationType,
                                                         Guid FeeId,
                                                         Decimal FeeValue,
                                                         DateTime SettlementDueDate,
                                                         Guid MerchantId,
                                                         Guid EstateId) : IRequest<Result>;

    public record AddSettledFeeToSettlementCommand(DateTime SettledDate, Guid MerchantId, Guid EstateId, Guid FeeId, Guid TransactionId) : IRequest<Result>;
}