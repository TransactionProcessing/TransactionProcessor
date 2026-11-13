using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.UnionResults;

namespace TransactionProcessor.ProjectionEngine.ProjectionHandler
{
    public interface IProjectionHandler
    {
        Task<Result> Handle(IDomainEvent @event, CancellationToken cancellationToken);
    }
}
