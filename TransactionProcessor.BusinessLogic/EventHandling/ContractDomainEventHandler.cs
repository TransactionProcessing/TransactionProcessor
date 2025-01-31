using System.Threading;
using System.Threading.Tasks;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.EventHandling;
using SimpleResults;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Repository;

namespace TransactionProcessor.BusinessLogic.EventHandling;

public class ContractDomainEventHandler : IDomainEventHandler
{
    #region Fields

    /// <summary>
    /// The estate reporting repository
    /// </summary>
    private readonly ITransactionProcessorReadModelRepository EstateReportingRepository;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="EstateDomainEventHandler" /> class.
    /// </summary>
    /// <param name="estateReportingRepository">The estate reporting repository.</param>
    public ContractDomainEventHandler(ITransactionProcessorReadModelRepository estateReportingRepository)
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
    private async Task<Result> HandleSpecificDomainEvent(ContractDomainEvents.ContractCreatedEvent domainEvent,
                                                         CancellationToken cancellationToken)
    {
        return await this.EstateReportingRepository.AddContract(domainEvent, cancellationToken);
    }

    /// <summary>
    /// Handles the specific domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task<Result> HandleSpecificDomainEvent(ContractDomainEvents.FixedValueProductAddedToContractEvent domainEvent,
                                                         CancellationToken cancellationToken)
    {
        return await this.EstateReportingRepository.AddContractProduct(domainEvent, cancellationToken);
    }

    /// <summary>
    /// Handles the specific domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task<Result> HandleSpecificDomainEvent(ContractDomainEvents.VariableValueProductAddedToContractEvent domainEvent,
                                                         CancellationToken cancellationToken)
    {
        return await this.EstateReportingRepository.AddContractProduct(domainEvent, cancellationToken);
    }

    /// <summary>
    /// Handles the specific domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task<Result> HandleSpecificDomainEvent(ContractDomainEvents.TransactionFeeForProductAddedToContractEvent domainEvent,
                                                         CancellationToken cancellationToken)
    {
        return await this.EstateReportingRepository.AddContractProductTransactionFee(domainEvent, cancellationToken);
    }

    /// <summary>
    /// Handles the specific domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task<Result> HandleSpecificDomainEvent(ContractDomainEvents.TransactionFeeForProductDisabledEvent domainEvent,
                                                         CancellationToken cancellationToken)
    {
        return await this.EstateReportingRepository.DisableContractProductTransactionFee(domainEvent, cancellationToken);
    }

    #endregion
}