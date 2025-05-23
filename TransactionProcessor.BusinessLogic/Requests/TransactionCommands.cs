﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.Models;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record TransactionCommands {
    public record ProcessLogonTransactionCommand(Guid TransactionId,
                                                 Guid EstateId,
                                                 Guid MerchantId,
                                                 String DeviceIdentifier,
                                                 String TransactionType,
                                                 DateTime TransactionDateTime,
                                                 String TransactionNumber,
                                                 DateTime TransactionReceivedDateTime)
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
                                                Int32 TransactionSource,
                                                DateTime TransactionReceivedDateTime)
        : IRequest<Result<ProcessSaleTransactionResponse>>;

    public record ResendTransactionReceiptCommand(Guid TransactionId, Guid EstateId) : IRequest<Result>;

    public record CalculateFeesForTransactionCommand(Guid TransactionId, DateTime CompletedDateTime, Guid EstateId, Guid MerchantId) : IRequest<Result>;

    public record AddSettledMerchantFeeCommand(Guid TransactionId, Decimal CalculatedValue, DateTime FeeCalculatedDateTime, CalculationType FeeCalculationType, Guid FeeId, Decimal FeeValue, DateTime SettledDateTime, Guid SettlementId) : IRequest<Result>;

    public record SendCustomerEmailReceiptCommand(Guid EstateId, Guid TransactionId, Guid EventId, String CustomerEmailAddress) : IRequest<Result>;
    public record ResendCustomerEmailReceiptCommand(Guid EstateId, Guid TransactionId) : IRequest<Result>;
}