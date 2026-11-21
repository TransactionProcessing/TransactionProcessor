using System.Threading;
using System.Threading.Tasks;
using TransactionProcessing.UnionResults;
using System;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;

namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

public class DummyOperatorDomainService : IOperatorDomainService
{
    public async Task<Result<Guid>> CreateOperator(OperatorCommands.CreateOperatorCommand command, CancellationToken cancellationToken) => Results.Ok(Guid.NewGuid());

    public async Task<Result> UpdateOperator(OperatorCommands.UpdateOperatorCommand command, CancellationToken cancellationToken) => Results.Ok();
}