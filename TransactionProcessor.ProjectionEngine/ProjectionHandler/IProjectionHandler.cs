using Shared.DomainDrivenDesign.EventSourcing;
using SimpleResults;

namespace TransactionProcessor.ProjectionEngine.ProjectionHandler
{
    public interface IProjectionHandler
    {
        Task<Result> Handle(IDomainEvent @event, CancellationToken cancellationToken);
    }
}
