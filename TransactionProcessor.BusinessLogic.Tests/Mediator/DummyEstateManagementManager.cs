using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

public class DummyEstateManagementManager : IEstateManagementManager {
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

    public async Task<Result<Models.Operator.Operator>> GetOperator(Guid estateId,
                                                                    Guid operatorId,
                                                                    CancellationToken cancellationToken) {
        return Result.Success(new Models.Operator.Operator());
    }

    public async Task<Result<List<Models.Operator.Operator>>> GetOperators(Guid estateId,
                                                                           CancellationToken cancellationToken) {
        return Result.Success(new List<Models.Operator.Operator>());
    }
}