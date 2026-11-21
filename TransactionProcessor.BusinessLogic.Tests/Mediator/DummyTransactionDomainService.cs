using KurrentDB.Client;
using TransactionProcessing.Shared.Serialisation;
using TransactionProcessing.UnionResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.ProjectionEngine.Models;
using TransactionProcessor.ProjectionEngine.Repository;
using TransactionProcessor.ProjectionEngine.State;

namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using Models;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.EventStore;

public class DummyTransactionDomainService : ITransactionDomainService
{
    public async Task<Result<ProcessLogonTransactionResponse>> ProcessLogonTransaction(TransactionCommands.ProcessLogonTransactionCommand command,
                                                                                       CancellationToken cancellationToken) {
        return Results.Ok(new ProcessLogonTransactionResponse());
    }

    public async Task<Result<ProcessSaleTransactionResponse>> ProcessSaleTransaction(TransactionCommands.ProcessSaleTransactionCommand command,
                                                                                     CancellationToken cancellationToken) {
        return Results.Ok(new ProcessSaleTransactionResponse());
    }

    public async Task<Result<ProcessReconciliationTransactionResponse>> ProcessReconciliationTransaction(TransactionCommands.ProcessReconciliationCommand command,
                                                                                                         CancellationToken cancellationToken) {
        return Results.Ok(new ProcessReconciliationTransactionResponse());
    }

    public async Task<Result> ResendTransactionReceipt(TransactionCommands.ResendTransactionReceiptCommand command,
                                                           CancellationToken cancellationToken) =>
        Results.Ok();

    public async Task<Result> CalculateFeesForTransaction(TransactionCommands.CalculateFeesForTransactionCommand command,
                                                          CancellationToken cancellationToken) =>
        Results.Ok();

    public async Task<Result> AddSettledMerchantFee(TransactionCommands.AddSettledMerchantFeeCommand command,
                                                    CancellationToken cancellationToken) =>
        Results.Ok();

    public async Task<Result> SendCustomerEmailReceipt(TransactionCommands.SendCustomerEmailReceiptCommand command,
                                                       CancellationToken cancellationToken) => Results.Ok();

    public async Task<Result> ResendCustomerEmailReceipt(TransactionCommands.ResendCustomerEmailReceiptCommand command,
                                                         CancellationToken cancellationToken) => Results.Ok();
}

public class DummyMerchantBalanceStateRepository : IProjectionStateRepository<MerchantBalanceState>
{
    public async Task<Result<MerchantBalanceState>> Load(IDomainEvent @event, CancellationToken cancellationToken) {
        return Results.Ok(new MerchantBalanceState());
    }

    public async Task<Result<MerchantBalanceState>> Load(Guid estateId, Guid stateId, CancellationToken cancellationToken)
    {
        return Results.Ok(new MerchantBalanceState());
    }

    public async Task<Result<MerchantBalanceState>> Save(MerchantBalanceState state, IDomainEvent @event, CancellationToken cancellationToken) {
        return Results.Ok(state);
    }
}

public class DummyTransactionProcessorReadRepository : ITransactionProcessorReadRepository {
    public async Task<Result> AddMerchantBalanceChangedEntry(MerchantBalanceChangedEntry entry,
                                                             CancellationToken cancellationToken) {
        return Results.Ok();
    }

    public async Task<Result<List<MerchantBalanceChangedEntry>>> GetMerchantBalanceHistory(Guid estateId,
                                                                                           Guid merchantId,
                                                                                           DateTimeOffset startDate,
                                                                                           DateTimeOffset endDate,
                                                                                           CancellationToken cancellationToken) {
        return Results.Ok(new List<MerchantBalanceChangedEntry>());
    }
}

public class DummyEventStoreContext : IEventStoreContext {
    public async Task<Result<List<ResolvedEvent>>> GetEventsBackward(String streamName,
                                                                     Int32 maxNumberOfEventsToRetrieve,
                                                                     CancellationToken cancellationToken) {
        return Results.Ok(new List<ResolvedEvent>());
    }

    public async Task<Result<String>> GetPartitionResultFromProjection(String projectionName,
                                                                       String partitionId,
                                                                       CancellationToken cancellationToken) {
        return Results.Ok(String.Empty);
    }

    public async Task<Result<String>> GetPartitionStateFromProjection(String projectionName,
                                                                      String partitionId,
                                                                      CancellationToken cancellationToken) {
        MerchantBalanceProjectionState1 state = new MerchantBalanceProjectionState1(new Merchant("", "", 0, 0));
        return Results.Ok<String>(StringSerialiser.Serialise(state));
    }

    public async Task<Result<String>> GetResultFromProjection(String projectionName,
                                                              CancellationToken cancellationToken) {
        return Results.Ok(String.Empty);
    }

    public async Task<Result<String>> GetStateFromProjection(String projectionName,
                                                             CancellationToken cancellationToken) {
        return Results.Ok(String.Empty);
    }

    public async Task<Result> InsertEvents(String streamName,
                                           Int64 expectedVersion,
                                           List<EventData> aggregateEvents,
                                           CancellationToken cancellationToken) {
        return Results.Ok();
    }

    public async Task<Result> InsertEvents(String streamName,
                                           Int64 expectedVersion,
                                           List<EventData> aggregateEvents,
                                           Object metadata,
                                           CancellationToken cancellationToken) {
        return Results.Ok();
    }

    public async Task<Result<List<ResolvedEvent>>> ReadEvents(String streamName,
                                                              Int64 fromVersion,
                                                              CancellationToken cancellationToken) {
        return Results.Ok(new List<ResolvedEvent>());
    }

    public async Task<Result<List<ResolvedEvent>>> ReadLastEventsFromAll(Int64 numberEvents,
                                                                         CancellationToken cancellationToken) {
        return Results.Ok(new List<ResolvedEvent>());
    }

    public async Task<Result<String>> RunTransientQuery(String query,
                                                        CancellationToken cancellationToken) {
        return Results.Ok(String.Empty);
    }

    public event TraceHandler TraceGenerated;
}
