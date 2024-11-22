using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json;
using Shared.EventStore.EventStore;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.ProjectionEngine.Models;
using TransactionProcessor.ProjectionEngine.Repository;
using TransactionProcessor.ProjectionEngine.State;

namespace TransactionProcessor.BusinessLogic.RequestHandlers;

public class MerchantRequestHandler :
    IRequestHandler<MerchantQueries.GetMerchantBalanceQuery, Result<MerchantBalanceState>>,
    IRequestHandler<MerchantQueries.GetMerchantLiveBalanceQuery, Result<MerchantBalanceProjectionState1>>,
    IRequestHandler<MerchantQueries.GetMerchantBalanceHistoryQuery, Result<List<MerchantBalanceChangedEntry>>> {
    private readonly IProjectionStateRepository<MerchantBalanceState> MerchantBalanceStateRepository;
    private readonly IEventStoreContext EventStoreContext;
    private readonly ITransactionProcessorReadRepository TransactionProcessorReadRepository;

    public MerchantRequestHandler(IProjectionStateRepository<MerchantBalanceState> merchantBalanceStateRepository,
                                  IEventStoreContext eventStoreContext,
                                  ITransactionProcessorReadRepository transactionProcessorReadRepository) {
        this.MerchantBalanceStateRepository = merchantBalanceStateRepository;
        this.EventStoreContext = eventStoreContext;
        this.TransactionProcessorReadRepository = transactionProcessorReadRepository;
    }

    public async Task<Result<MerchantBalanceState>> Handle(MerchantQueries.GetMerchantBalanceQuery query,
                                                           CancellationToken cancellationToken) {
        return await this.MerchantBalanceStateRepository.Load(query.EstateId, query.MerchantId, cancellationToken);
    }

    public async Task<Result<MerchantBalanceProjectionState1>> Handle(MerchantQueries.GetMerchantLiveBalanceQuery query,
                                                                      CancellationToken cancellationToken) {

        Result<String> result = await this.EventStoreContext.GetPartitionStateFromProjection("MerchantBalanceProjection", $"MerchantBalance-{query.MerchantId:N}", cancellationToken);
        if (result.IsFailed)
            return Result.NotFound(
                $"Merchant Balance not found for Merchant {query.MerchantId} on MerchantBalanceProjection");
        
        MerchantBalanceProjectionState1 projectionState = JsonConvert.DeserializeObject<MerchantBalanceProjectionState1>(result.Data);

        return Result.Success(projectionState);
    }

    public async Task<Result<List<MerchantBalanceChangedEntry>>> Handle(MerchantQueries.GetMerchantBalanceHistoryQuery query,
                                                                        CancellationToken cancellationToken) {
        return await this.TransactionProcessorReadRepository.GetMerchantBalanceHistory(query.EstateId, query.MerchantId, query.StartDate, query.EndDate, cancellationToken);
    }
}