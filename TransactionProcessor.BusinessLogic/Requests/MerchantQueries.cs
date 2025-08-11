using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.ProjectionEngine.Models;
using TransactionProcessor.ProjectionEngine.State;

namespace TransactionProcessor.BusinessLogic.Requests;
[ExcludeFromCodeCoverage]
public record MerchantQueries {
    public record GetMerchantBalanceQuery(Guid EstateId, Guid MerchantId) : IRequest<Result<MerchantBalanceState>>;

    public record GetMerchantLiveBalanceQuery(Guid EstateId, Guid MerchantId) : IRequest<Result<Decimal>>;

    public record GetMerchantBalanceHistoryQuery(Guid EstateId, Guid MerchantId, DateTime StartDate, DateTime EndDate)
        : IRequest<Result<List<MerchantBalanceChangedEntry>>>;

    public record GetMerchantQuery(Guid EstateId, Guid MerchantId) : IRequest<Result<Models.Merchant.Merchant>>;

    public record GetMerchantContractsQuery(Guid EstateId, Guid MerchantId) : IRequest<Result<List<Models.Contract.Contract>>>;

    public record GetMerchantsQuery(Guid EstateId) : IRequest<Result<List<Models.Merchant.Merchant>>>;

    public record GetTransactionFeesForProductQuery(Guid EstateId, Guid MerchantId, Guid ContractId, Guid ProductId) : IRequest<Result<List<ContractProductTransactionFee>>>;

}