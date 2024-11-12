using Newtonsoft.Json;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleResults;

namespace TransactionProcessor.ProjectionEngine.ProjectionHandler
{
    public interface IProjectionHandler
    {
        Task<Result> Handle(IDomainEvent @event, CancellationToken cancellationToken);
    }
}
