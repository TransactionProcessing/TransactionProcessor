using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.BusinessLogic.Requests;
[ExcludeFromCodeCoverage]
public record SettlementCommands {
    public record ProcessSettlementCommand(DateTimeOffset SettlementDate, Guid MerchantId, Guid EstateId)
        : IRequest<Result<Guid>>;

    public record AddMerchantFeePendingSettlementCommand(Guid TransactionId,
                                                         Decimal CalculatedValue,
                                                         DateTimeOffset FeeCalculatedDateTime,
                                                         CalculationType FeeCalculationType,
                                                         Guid FeeId,
                                                         Decimal FeeValue,
                                                         DateTimeOffset SettlementDueDate,
                                                         Guid MerchantId,
                                                         Guid EstateId) : IRequest<Result>;

    public record AddSettledFeeToSettlementCommand(DateTimeOffset SettledDate, Guid MerchantId, Guid EstateId, Guid FeeId, Guid TransactionId) : IRequest<Result>;
}