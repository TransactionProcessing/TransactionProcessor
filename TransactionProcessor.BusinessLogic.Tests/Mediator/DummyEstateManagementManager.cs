using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessing.UnionResults;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;
using MerchantScheduleModel = TransactionProcessor.Models.MerchantSchedule.MerchantSchedule;
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
        return Results.Ok(new List<Contract>());
    }

    public async Task<Result<Contract>> GetContract(Guid estateId,
                                                    Guid contractId,
                                                    CancellationToken cancellationToken) {
        return Results.Ok(new Contract());
    }

    public async Task<Result<Models.Estate.Estate>> GetEstate(Guid estateId,
                                                              CancellationToken cancellationToken) {
        return Results.Ok(new Models.Estate.Estate());
    }

    public async Task<Result<List<Models.Estate.Estate>>> GetEstates(Guid estateId,
                                                                     CancellationToken cancellationToken) {
        return Results.Ok(new List<Models.Estate.Estate>());
    }

    public async Task<Result<Merchant>> GetMerchant(Guid estateId,
                                                    Guid merchantId,
                                                    CancellationToken cancellationToken) {
        return new Result<Merchant>(new Error(ErrorType.Failure, "Merchant was not found."));
    }

    public async Task<Result<MerchantScheduleModel>> GetMerchantSchedule(Guid estateId,
                                                                          Guid merchantId,
                                                                          Int32 year,
                                                                          CancellationToken cancellationToken) {
        return Results.Ok(new MerchantScheduleModel());
    }

    public async Task<Result<MerchantScheduleModel>> GetMerchantScheduleFromReadModel(Guid estateId,
                                                                                       Guid merchantId,
                                                                                       Int32 year,
                                                                                       CancellationToken cancellationToken) {
        return Results.Ok(new MerchantScheduleModel());
    }

    public async Task<Result<List<Merchant>>> GetMerchants(Guid estateId,
                                                           CancellationToken cancellationToken) {
        return Results.Ok(new List<Merchant>());
    }

    public async Task<Result<List<ContractProductTransactionFee>>> GetTransactionFeesForProduct(Guid estateId,
                                                                                                Guid merchantId,
                                                                                                Guid contractId,
                                                                                                Guid productId,
                                                                                                CancellationToken cancellationToken) {
        return Results.Ok(new List<ContractProductTransactionFee>());
    }

    public async Task<Result<Models.Operator.Operator>> GetOperator(Guid estateId,
                                                                    Guid operatorId,
                                                                    CancellationToken cancellationToken) {
        return Results.Ok(new Models.Operator.Operator());
    }

    public async Task<Result<List<Models.Operator.Operator>>> GetOperators(Guid estateId,
                                                                           CancellationToken cancellationToken) {
        return Results.Ok(new List<Models.Operator.Operator>());
    }

    public async Task<Result<SettlementModel>> GetSettlement(Guid estateId,
                                                             Guid merchantId,
                                                             Guid settlementId,
                                                             CancellationToken cancellationToken) {
        return Results.Ok(new SettlementModel());
    }

    public async Task<Result<List<SettlementModel>>> GetSettlements(Guid estateId,
                                                                    Guid? merchantId,
                                                                    String startDate,
                                                                    String endDate,
                                                                    CancellationToken cancellationToken) {
        return Results.Ok(new List<SettlementModel>());
    }

    public async Task<Result<PendingSettlementModel>> GetPendingSettlement(Guid estateId,
                                                                           Guid merchantId,
                                                                           DateTimeOffset settlementDate,
                                                                           CancellationToken cancellationToken) {
        return Results.Ok(new PendingSettlementModel());
    }
}
