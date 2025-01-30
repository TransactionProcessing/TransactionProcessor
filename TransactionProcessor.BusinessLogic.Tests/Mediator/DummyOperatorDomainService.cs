using System.Threading;
using System.Threading.Tasks;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;

namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

public class DummyOperatorDomainService : IOperatorDomainService
{
    public async Task<Result> CreateOperator(OperatorCommands.CreateOperatorCommand command, CancellationToken cancellationToken) => Result.Success();

    public async Task<Result> UpdateOperator(OperatorCommands.UpdateOperatorCommand command, CancellationToken cancellationToken) => Result.Success();
}