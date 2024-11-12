using System;
using System.Collections.Generic;
using MediatR;
using SimpleResults;
using TransactionProcessor.Models;

namespace TransactionProcessor.BusinessLogic.Requests;

public record TransactionCommands {
    public record ProcessLogonTransactionCommand(Guid TransactionId,
                                                 Guid EstateId,
                                                 Guid MerchantId,
                                                 String DeviceIdentifier,
                                                 String TransactionType,
                                                 DateTime TransactionDateTime,
                                                 String TransactionNumber)
        : IRequest<Result<ProcessLogonTransactionResponse>>;

    public record ProcessReconciliationCommand(Guid TransactionId,
                                               Guid EstateId,
                                               Guid MerchantId,
                                               String DeviceIdentifier,
                                               DateTime TransactionDateTime,
                                               Int32 TransactionCount,
                                               Decimal TransactionValue)
        : IRequest<Result<ProcessReconciliationTransactionResponse>>;

    public record ProcessSaleTransactionCommand(Guid TransactionId,
                                                Guid EstateId,
                                                Guid MerchantId,
                                                String DeviceIdentifier,
                                                String TransactionType,
                                                DateTime TransactionDateTime,
                                                String TransactionNumber,
                                                Guid OperatorId,
                                                String CustomerEmailAddress,
                                                Dictionary<String, String> AdditionalTransactionMetadata,
                                                Guid ContractId,
                                                Guid ProductId,
                                                Int32 TransactionSource)
        : IRequest<Result<ProcessSaleTransactionResponse>>;

    public record ResendTransactionReceiptCommand(Guid TransactionId, Guid EstateId) : IRequest<Result>;

    public record CalculateFeesForTransactionCommand(Guid TransactionId, DateTime CompletedDateTime, Guid EstateId, Guid MerchantId) : IRequest<Result>;

    public record AddSettledMerchantFeeCommand(Guid TransactionId, Decimal CalculatedValue, DateTime FeeCalculatedDateTime, CalculationType FeeCalculationType, Guid FeeId, Decimal FeeValue, DateTime SettledDateTime, Guid SettlementId) : IRequest<Result>;
}