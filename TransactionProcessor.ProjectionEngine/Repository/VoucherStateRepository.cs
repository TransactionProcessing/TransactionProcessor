using EventStore.Client;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;

namespace TransactionProcessor.ProjectionEngine.Repository;

using Database.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.ProjectionEngine;
using State;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class VoucherStateRepository : IProjectionStateRepository<VoucherState>
{
    #region Fields

    private readonly Shared.EntityFramework.IDbContextFactory<EstateManagementContext> ContextFactory;

    #endregion

    #region Constructors

    public VoucherStateRepository(Shared.EntityFramework.IDbContextFactory<EstateManagementContext> contextFactory)
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

    public async Task<Result<VoucherState>> Load(IDomainEvent @event,
                                                 CancellationToken cancellationToken)
    {
        Guid estateId = VoucherStateRepository.GetEstateId(@event);
        Guid voucherId = GetVoucherId(@event);

        return await this.LoadHelper(estateId, voucherId, cancellationToken);
    }

    public async Task<Result<VoucherState>> Load(Guid estateId,
                                                 Guid stateId,
                                                 CancellationToken cancellationToken)
    {
        return await this.LoadHelper(estateId, stateId, cancellationToken);
    }

    public async Task<Result<VoucherState>> Save(VoucherState state,
                                                 IDomainEvent domainEvent,
                                                 CancellationToken cancellationToken)
    {

        await using EstateManagementContext context =
            await this.ContextFactory.GetContext(state.EstateId, VoucherStateRepository.ConnectionStringIdentifier, cancellationToken);
        // Note: we don't want to select the state again here....
        VoucherProjectionState entity = VoucherStateRepository.CreateVoucherProjectionState(state);

        if (state.IsInitialised)
        {
            // handle updates here
            context.VoucherProjectionStates.Update(entity);
        }
        else
        {
            await context.VoucherProjectionStates.AddAsync(entity, cancellationToken);
        }

        var @event = VoucherStateRepository.Create(state.GetType().Name, domainEvent);

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
                                                 Value = state.Value,
                                                 OperatorIdentifier = state.OperatorIdentifier,
                                                 ExpiryDate = state.ExpiryDate,
                                                 ExpiryDateTime = state.ExpiryDateTime,
                                                 GenerateDate = state.GenerateDate,
                                                 GenerateDateTime = state.GenerateDateTime,
                                                 IsGenerated = state.IsGenerated,
                                                 IsIssued = state.IsIssued,
                                                 IsRedeemed = state.IsRedeemed,
                                                 IssuedDate = state.IssuedDate,
                                                 IssuedDateTime = state.IssuedDateTime,
                                                 RecipientEmail = state.RecipientEmail,
                                                 RecipientMobile = state.RecipientMobile,
                                                 RedeemedDate = state.RedeemedDate,
                                                 RedeemedDateTime = state.RedeemedDateTime
        };
        return entity;
    }

    private async Task<VoucherState> LoadHelper(Guid estateId,
                                                        Guid voucherId,
                                                        CancellationToken cancellationToken)
    {
        await using EstateManagementContext? context =
            await this.ContextFactory.GetContext(estateId, VoucherStateRepository.ConnectionStringIdentifier, cancellationToken);

        VoucherProjectionState? entity = await context.VoucherProjectionStates.Where(m => m.VoucherId == voucherId).SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (entity == null)
        {
            return new VoucherState();
        }

        // We have located a state record so we need to translate to the Model type
        return new VoucherState
        {
            EstateId = entity.EstateId,
            Version = entity.Timestamp,
            VoucherCode = entity.VoucherCode,
            TransactionId = entity.TransactionId,
            Barcode = entity.Barcode,
            VoucherId = entity.VoucherId,
            Value = entity.Value,
            OperatorIdentifier = entity.OperatorIdentifier,
            ExpiryDate = entity.ExpiryDate,
            ExpiryDateTime = entity.ExpiryDateTime,
            GenerateDate = entity.GenerateDate,
            GenerateDateTime = entity.GenerateDateTime,
            IsGenerated = entity.IsGenerated,
            IsIssued = entity.IsIssued,
            IsRedeemed = entity.IsRedeemed,
            IssuedDate = entity.IssuedDate,
            IssuedDateTime = entity.IssuedDateTime,
            RecipientEmail = entity.RecipientEmail,
            RecipientMobile = entity.RecipientMobile,
            RedeemedDate = entity.RedeemedDate,
            RedeemedDateTime = entity.RedeemedDateTime
        };
    }

    #endregion

    #region Others

    private const String ConnectionStringIdentifier = "EstateReportingReadModel";

    #endregion
}