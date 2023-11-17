namespace TransactionProcessor.ProjectionEngine.Repository;

using Common;
using Microsoft.EntityFrameworkCore;
using Shared.DomainDrivenDesign.EventSourcing;
using State;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.ProjectionEngine.Database.Database;
using TransactionProcessor.ProjectionEngine.Database.Database.Entities;

[ExcludeFromCodeCoverage]
public class MerchantBalanceStateRepository : IProjectionStateRepository<MerchantBalanceState>
{
    #region Fields

    private readonly Shared.EntityFramework.IDbContextFactory<TransactionProcessorGenericContext> ContextFactory;

    #endregion

    #region Constructors

    public MerchantBalanceStateRepository(Shared.EntityFramework.IDbContextFactory<TransactionProcessorGenericContext> contextFactory) {
        this.ContextFactory = contextFactory;
    }

    #endregion

    #region Methods

    public static Event Create(String type,
                               IDomainEvent domainEvent) {
        return new() {
                         EventId = domainEvent.EventId,
                         Date = domainEvent.EventTimestamp.Date.Date,
                         Type = type
                     };
    }

    public static Guid GetEstateId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "EstateId");

    public static Guid GetMerchantId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "MerchantId");

    public async Task<MerchantBalanceState> Load(IDomainEvent @event,
                                                 CancellationToken cancellationToken) {
        Guid estateId = GetEstateId(@event);
        Guid merchantId = GetMerchantId(@event);

        return await this.LoadHelper(estateId, merchantId, cancellationToken);
    }

    public async Task<MerchantBalanceState> Load(Guid estateId,
                                                 Guid stateId,
                                                 CancellationToken cancellationToken) {
        return await this.LoadHelper(estateId, stateId, cancellationToken);
    }

    public async Task<MerchantBalanceState> Save(MerchantBalanceState state,
                                                 IDomainEvent domainEvent,
                                                 CancellationToken cancellationToken) {
        await using TransactionProcessorGenericContext context =
            await this.ContextFactory.GetContext(state.EstateId, MerchantBalanceStateRepository.ConnectionStringIdentifier, cancellationToken);
        // Note: we don't want to select the state again here....
        MerchantBalanceProjectionState entity = MerchantBalanceStateRepository.CreateMerchantBalanceProjectionState(state);

        if (state.IsInitialised) {
            // handle updates here
            context.MerchantBalanceProjectionState.Update(entity);
        }
        else {
            await context.MerchantBalanceProjectionState.AddAsync(entity, cancellationToken);
        }

        Event @event = MerchantBalanceStateRepository.Create(state.GetType().Name, domainEvent);

        await context.Events.AddAsync(@event, cancellationToken);

        try {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch(DbUpdateException) {
            //This lets the next component know no changes were persisted.
            state = state with {
                                   ChangesApplied = false
                               };
        }

        return state;
    }

    private static MerchantBalanceProjectionState CreateMerchantBalanceProjectionState(MerchantBalanceState state) {
        MerchantBalanceProjectionState entity = new() {
                                                          Balance = state.Balance,
                                                          EstateId = state.EstateId,
                                                          MerchantId = state.MerchantId,
                                                          AvailableBalance = state.AvailableBalance,
                                                          MerchantName = state.MerchantName,
                                                          Timestamp = state.Version,
                                                          DeclinedSales = state.DeclinedSales,
                                                          ValueOfFees = state.ValueOfFees,
                                                          StartedTransactionCount = state.StartedTransactionCount,
                                                          FeeCount = state.FeeCount,
                                                          AuthorisedSales = state.AuthorisedSales,
                                                          CompletedTransactionCount = state.CompletedTransactionCount,
                                                          DepositCount = state.DepositCount,
                                                          LastDeposit = state.LastDeposit,
                                                          LastFee = state.LastFee,
                                                          LastSale = state.LastSale,
                                                          SaleCount = state.SaleCount,
                                                          TotalDeposited = state.TotalDeposited,
                                                          LastWithdrawal = state.LastWithdrawal,
                                                          TotalWithdrawn = state.TotalWithdrawn,
                                                          WithdrawalCount = state.WithdrawalCount,
                                                      };
        return entity;
    }

    private async Task<MerchantBalanceState> LoadHelper(Guid estateId,
                                                        Guid merchantId,
                                                        CancellationToken cancellationToken) {
        await using TransactionProcessorGenericContext context =
            await this.ContextFactory.GetContext(estateId, MerchantBalanceStateRepository.ConnectionStringIdentifier, cancellationToken);

        MerchantBalanceProjectionState? entity = await context.MerchantBalanceProjectionState.Where(m => m.MerchantId == merchantId).SingleOrDefaultAsync();

        if (entity == null) {
            return new MerchantBalanceState();
        }

        // We have located a state record so we need to translate to the Model type
        return new MerchantBalanceState {
                                            Version = entity.Timestamp,
                                            Balance = entity.Balance,
                                            MerchantId = merchantId,
                                            AvailableBalance = entity.AvailableBalance,
                                            MerchantName = entity.MerchantName,
                                            EstateId = entity.EstateId,
                                            DeclinedSales = entity.DeclinedSales,
                                            ValueOfFees = entity.ValueOfFees,
                                            StartedTransactionCount = entity.StartedTransactionCount,
                                            FeeCount = entity.FeeCount,
                                            AuthorisedSales = entity.AuthorisedSales,
                                            CompletedTransactionCount = entity.CompletedTransactionCount,
                                            DepositCount = entity.DepositCount,
                                            LastDeposit = entity.LastDeposit,
                                            LastFee = entity.LastFee,
                                            LastSale = entity.LastSale,
                                            SaleCount = entity.SaleCount,
                                            TotalDeposited = entity.TotalDeposited,
                                            LastWithdrawal = entity.LastWithdrawal,
                                            TotalWithdrawn = entity.TotalWithdrawn,
                                            WithdrawalCount = entity.WithdrawalCount,
                                        };
    }

    #endregion

    #region Others

    private const String ConnectionStringIdentifier = "TransactionProcessorReadModel";

    #endregion
}