namespace MessagingService.BusinessLogic.EventHandling
{
    using System.Threading;
    using System.Threading.Tasks;
    using Shared.DomainDrivenDesign.EventSourcing;

    public interface IDomainEventHandler
    {
        #region Methods

        /// <summary>
        /// Handles the specified domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task Handle(DomainEvent domainEvent,
                    CancellationToken cancellationToken);

        #endregion
    }
}