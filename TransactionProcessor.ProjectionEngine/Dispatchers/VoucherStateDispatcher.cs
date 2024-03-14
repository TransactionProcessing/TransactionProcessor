namespace TransactionProcessor.ProjectionEngine.Dispatchers;

using Shared.DomainDrivenDesign.EventSourcing;
using State;

public class VoucherStateDispatcher : IStateDispatcher<VoucherState>{
    public async Task Dispatch(VoucherState state, IDomainEvent @event, CancellationToken cancellationToken){
        
    }
}