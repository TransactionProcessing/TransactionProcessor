using Newtonsoft.Json;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransactionProcessor.ProjectionEngine.ProjectionHandler
{
    public interface IProjectionHandler
    {
        Task Handle(IDomainEvent @event, CancellationToken cancellationToken);
    }
}
