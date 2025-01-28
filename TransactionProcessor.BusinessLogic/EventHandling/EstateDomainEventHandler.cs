using System.Threading;
using System.Threading.Tasks;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.EventHandling;
using SimpleResults;
using TransactionProcessor.Estate.DomainEvents;
using TransactionProcessor.Repository;

namespace TransactionProcessor.BusinessLogic.EventHandling;

public class EstateDomainEventHandler : IDomainEventHandler
{
    #region Fields

    /// <summary>
    /// The estate reporting repository
    /// </summary>
    private readonly ITransactionProcessorReadModelRepository EstateReportingRepository;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="EstateDomainEventHandler"/> class.
    /// </summary>
    /// <param name="estateReportingRepository">The estate reporting repository.</param>
    public EstateDomainEventHandler(ITransactionProcessorReadModelRepository estateReportingRepository)
    {
        this.EstateReportingRepository = estateReportingRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handles the specified domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<Result> Handle(IDomainEvent domainEvent,
                                     CancellationToken cancellationToken)
    {
        return await this.HandleSpecificDomainEvent((dynamic)domainEvent, cancellationToken);
    }

    /// <summary>
    /// Handles the specific domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task<Result> HandleSpecificDomainEvent(EstateCreatedEvent domainEvent,
                                                         CancellationToken cancellationToken)
    {
        Result createResult = await this.EstateReportingRepository.CreateReadModel(domainEvent, cancellationToken);
        if (createResult.IsFailed)
            return createResult;

        return await this.EstateReportingRepository.AddEstate(domainEvent, cancellationToken);
    }

    /// <summary>
    /// Handles the specific domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task<Result> HandleSpecificDomainEvent(SecurityUserAddedToEstateEvent domainEvent,
                                                         CancellationToken cancellationToken)
    {
        return await this.EstateReportingRepository.AddEstateSecurityUser(domainEvent, cancellationToken);
    }
    
    private async Task<Result> HandleSpecificDomainEvent(EstateReferenceAllocatedEvent domainEvent,
                                                         CancellationToken cancellationToken)
    {
        return await this.EstateReportingRepository.UpdateEstate(domainEvent, cancellationToken);
    }

    #endregion
}

