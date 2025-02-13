/*using System.Threading;
using System.Threading.Tasks;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.EventHandling;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Events;
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
        //return await this.HandleSpecificDomainEvent((dynamic)domainEvent, cancellationToken);
        Task<Result> t = domainEvent switch
        {
            //MerchantDomainEvents.MerchantCreatedEvent de => this.EstateReportingRepository.AddMerchant(de, cancellationToken),
            ContractDomainEvents.ContractCreatedEvent de => this.EstateReportingRepository.AddContract(de, cancellationToken),
            ContractDomainEvents.FixedValueProductAddedToContractEvent de  => this.EstateReportingRepository.AddContractProduct(de, cancellationToken),
            ContractDomainEvents.VariableValueProductAddedToContractEvent de => this.EstateReportingRepository.AddContractProduct(de, cancellationToken),
            ContractDomainEvents.TransactionFeeForProductAddedToContractEvent de => this.EstateReportingRepository.AddContractProductTransactionFee(de, cancellationToken),
            ContractDomainEvents.TransactionFeeForProductDisabledEvent de => this.EstateReportingRepository.DisableContractProductTransactionFee(de, cancellationToken),
            _ => null
        };
        if (t != null)
            return await t;

        return Result.Success();
    }

    #endregion
}*/