using System.Collections.Generic;
using System.Text;

namespace MessagingService.BusinessLogic.EventHandling
{
    using Shared.DomainDrivenDesign.EventSourcing;

    public interface IDomainEventHandlerResolver
    {
        #region Methods

        /// <summary>
        /// Gets the domain event handlers.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <returns></returns>
        List<IDomainEventHandler> GetDomainEventHandlers(DomainEvent domainEvent);

        #endregion
    }
}
