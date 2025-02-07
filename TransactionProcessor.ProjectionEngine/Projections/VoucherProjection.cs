using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.ProjectionEngine.Projections
{
    using Shared.DomainDrivenDesign.EventSourcing;
    using State;

    public class VoucherProjection : IProjection<VoucherState>
    {
        public async Task<VoucherState> Handle(VoucherState state, IDomainEvent domainEvent, CancellationToken cancellationToken){
            VoucherState newState = domainEvent switch
            {
                VoucherDomainEvents.VoucherGeneratedEvent vge => state.HandleVoucherGeneratedEvent(vge),
                VoucherDomainEvents.BarcodeAddedEvent bae => state.HandleBarcodeAddedEvent(bae),
                VoucherDomainEvents.VoucherIssuedEvent vie => state.HandleVoucherIssuedEvent(vie),
                VoucherDomainEvents.VoucherFullyRedeemedEvent vfre=> state.HandleVoucherFullyRedeemedEvent(vfre),
                _ => state
            };

            return newState;
        }

        public Boolean ShouldIHandleEvent(IDomainEvent domainEvent){
            return domainEvent switch
            {
                VoucherDomainEvents.VoucherGeneratedEvent _ => true,
                VoucherDomainEvents.BarcodeAddedEvent _ => true,
                VoucherDomainEvents.VoucherIssuedEvent _ => true,
                VoucherDomainEvents.VoucherFullyRedeemedEvent _ => true,
                _ => false
            };
        }
    }
}
