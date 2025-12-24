using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Models.Settlement;
using Contract = TransactionProcessor.Models.Contract.Contract;

namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

public class DummyTransactionProcessorManager : ITransactionProcessorManager {
    public async Task<Result<List<Contract>>> GetMerchantContracts(Guid estateId,
                                                                   Guid merchantId,
                                                                   CancellationToken cancellationToken) {
        return new Result<List<Contract>>();
    }

    public async Task<Result<List<Contract>>> GetContracts(Guid estateId,
                                                           CancellationToken cancellationToken) {
        return Result.Success(new List<Contract>());
    }

    public async Task<Result<Contract>> GetContract(Guid estateId,
                                                    Guid contractId,
                                                    CancellationToken cancellationToken) {
        return Result.Success(new Contract());
    }

    public async Task<Result<Models.Estate.Estate>> GetEstate(Guid estateId,
                                                              CancellationToken cancellationToken) {
        return Result.Success(new Models.Estate.Estate());
    }

    public async Task<Result<List<Models.Estate.Estate>>> GetEstates(Guid estateId,
                                                                     CancellationToken cancellationToken) {
        return Result.Success(new List<Models.Estate.Estate>());
    }

    public async Task<Result<Merchant>> GetMerchant(Guid estateId,
                                                    Guid merchantId,
                                                    CancellationToken cancellationToken) {
        return new Result<Merchant>();
    }

    public async Task<Result<List<Merchant>>> GetMerchants(Guid estateId,
                                                           CancellationToken cancellationToken) {
        return Result.Success(new List<Merchant>());
    }

    public async Task<Result<List<ContractProductTransactionFee>>> GetTransactionFeesForProduct(Guid estateId,
                                                                                                Guid merchantId,
                                                                                                Guid contractId,
                                                                                                Guid productId,
                                                                                                CancellationToken cancellationToken) {
        return Result.Success(new List<ContractProductTransactionFee>());
    }

    public async Task<Result<Models.Operator.Operator>> GetOperator(Guid estateId,
                                                                    Guid operatorId,
                                                                    CancellationToken cancellationToken) {
        return Result.Success(new Models.Operator.Operator());
    }

    public async Task<Result<List<Models.Operator.Operator>>> GetOperators(Guid estateId,
                                                                           CancellationToken cancellationToken) {
        return Result.Success(new List<Models.Operator.Operator>());
    }

    public async Task<Result<SettlementModel>> GetSettlement(Guid estateId,
                                                             Guid merchantId,
                                                             Guid settlementId,
                                                             CancellationToken cancellationToken) {
        return Result.Success(new SettlementModel());
    }

    public async Task<Result<List<SettlementModel>>> GetSettlements(Guid estateId,
                                                                    Guid? merchantId,
                                                                    String startDate,
                                                                    String endDate,
                                                                    CancellationToken cancellationToken) {
        return Result.Success(new List<SettlementModel>());
    }

    public async Task<Result<PendingSettlementModel>> GetPendingSettlement(Guid estateId,
                                                                           Guid merchantId,
                                                                           DateTime settlementDate,
                                                                           CancellationToken cancellationToken) {
        return Result.Success(new PendingSettlementModel());
    }
}