using TransactionProcessing.UnionResults;

namespace TransactionProcessor.ProjectionEngine.Dispatchers;

using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using State;

public class VoucherStateDispatcher : IStateDispatcher<VoucherState>{
    public async Task<Result> Dispatch(VoucherState state, IDomainEvent @event, CancellationToken cancellationToken){   
        return Result.Success();
    }
}