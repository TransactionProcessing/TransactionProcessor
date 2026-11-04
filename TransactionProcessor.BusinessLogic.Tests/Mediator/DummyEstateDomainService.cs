using System.Threading;
using System.Threading.Tasks;
using SimpleResults;
using System;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;

namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

public class DummyEstateDomainService : IEstateDomainService
{
    public async Task<Result<Guid>> CreateEstate(EstateCommands.CreateEstateCommand command, CancellationToken cancellationToken) => Result.Success(Guid.NewGuid());
        
    public async Task<Result> AddOperatorToEstate(EstateCommands.AddOperatorToEstateCommand command, CancellationToken cancellationToken) => Result.Success();

    public async Task<Result> CreateEstateUser(EstateCommands.CreateEstateUserCommand command, CancellationToken cancellationToken) => Result.Success();

    public async Task<Result> RemoveOperatorFromEstate(EstateCommands.RemoveOperatorFromEstateCommand command, CancellationToken cancellationToken) => Result.Success();
}