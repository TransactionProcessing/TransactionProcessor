namespace TransactionProcessor.ProjectionEngine.Repository;

using System.Diagnostics.CodeAnalysis;
using Database.Database;
using Database.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.DomainDrivenDesign.EventSourcing;
using State;

[ExcludeFromCodeCoverage]
public class VoucherStateRepository : IProjectionStateRepository<VoucherState>
{
    #region Fields

    private readonly Shared.EntityFramework.IDbContextFactory<TransactionProcessorGenericContext> ContextFactory;

    #endregion

    #region Constructors

    public VoucherStateRepository(Shared.EntityFramework.IDbContextFactory<TransactionProcessorGenericContext> contextFactory)
    {
        this.ContextFactory = contextFactory;
    }

    #endregion

    #region Methods

    public static Event Create(String type,
                               IDomainEvent domainEvent)
    {
        return new()
               {
                   EventId = domainEvent.EventId,
                   Date = domainEvent.EventTimestamp.Date.Date,
                   Type = type
               };
    }

    public static Guid GetEstateId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "EstateId");

    public static Guid GetVoucherId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "VoucherId");

    public async Task<VoucherState> Load(IDomainEvent @event,
                                         CancellationToken cancellationToken)
    {
        Guid estateId = VoucherStateRepository.GetEstateId(@event);
        Guid voucherId = GetVoucherId(@event);

        return await this.LoadHelper(estateId, voucherId, cancellationToken);
    }

    public async Task<VoucherState> Load(Guid estateId,
                                         Guid stateId,
                                         CancellationToken cancellationToken)
    {
        return await this.LoadHelper(estateId, stateId, cancellationToken);
    }

    public async Task<VoucherState> Save(VoucherState state,
                                         IDomainEvent domainEvent,
                                         CancellationToken cancellationToken)
    {

        await using TransactionProcessorGenericContext context =
            await this.ContextFactory.GetContext(state.EstateId, VoucherStateRepository.ConnectionStringIdentifier, cancellationToken);
        // Note: we don't want to select the state again here....
        VoucherProjectionState entity = VoucherStateRepository.CreateVoucherProjectionState(state);

        if (state.IsInitialised)
        {
            // handle updates here
            context.VoucherProjectionState.Update(entity);
        }
        else
        {
            await context.VoucherProjectionState.AddAsync(entity, cancellationToken);
        }

        Event @event = MerchantBalanceStateRepository.Create(state.GetType().Name, domainEvent);

        await context.Events.AddAsync(@event, cancellationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            //This lets the next component know no changes were persisted.
            state = state with
                    {
                        ChangesApplied = false
                    };
        }

        return state;
    }

    private static VoucherProjectionState CreateVoucherProjectionState(VoucherState state){
        VoucherProjectionState entity = new(){
                                                 EstateId = state.EstateId,
                                                 Timestamp = state.Version,
                                                 VoucherCode = state.VoucherCode,
                                                 TransactionId = state.TransactionId,
                                                 Barcode = String.IsNullOrEmpty(state.Barcode) ? "" : state.Barcode,
                                                 VoucherId = state.VoucherId,
                                             };
        return entity;
    }

    private async Task<VoucherState> LoadHelper(Guid estateId,
                                                        Guid voucherId,
                                                        CancellationToken cancellationToken)
    {
        await using TransactionProcessorGenericContext context =
            await this.ContextFactory.GetContext(estateId, VoucherStateRepository.ConnectionStringIdentifier, cancellationToken);

        VoucherProjectionState? entity = await context.VoucherProjectionState.Where(m => m.VoucherId == voucherId).SingleOrDefaultAsync();

        if (entity == null)
        {
            return new VoucherState();
        }

        // We have located a state record so we need to translate to the Model type
        return new VoucherState
        {
                   Version = entity.Timestamp,
                   VoucherCode = entity.VoucherCode,
                   TransactionId = entity.TransactionId,
                   Barcode = entity.Barcode,
                   VoucherId = entity.VoucherId,
                   EstateId = entity.EstateId,
               };
    }

    #endregion

    #region Others

    private const String ConnectionStringIdentifier = "TransactionProcessorReadModel";

    #endregion
}