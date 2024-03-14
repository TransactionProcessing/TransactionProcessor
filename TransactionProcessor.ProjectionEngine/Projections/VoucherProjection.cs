using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.ProjectionEngine.Projections
{
    using EstateManagement.Merchant.DomainEvents;
    using Shared.DomainDrivenDesign.EventSourcing;
    using State;
    using TransactionProcessor.Transaction.DomainEvents;
    using Voucher.DomainEvents;

    public class VoucherProjection : IProjection<VoucherState>
    {
        public async Task<VoucherState> Handle(VoucherState state, IDomainEvent domainEvent, CancellationToken cancellationToken){
            VoucherState newState = domainEvent switch
            {
                VoucherGeneratedEvent vge => state.HandleVoucherGeneratedEvent(vge),
                BarcodeAddedEvent bae => state.HandleBarcodeAddedEvent(bae),
                VoucherIssuedEvent vie => state.HandleVoucherIssuedEvent(vie),
                VoucherFullyRedeemedEvent vfre=> state.HandleVoucherFullyRedeemedEvent(vfre),
                _ => state
            };

            return newState;
        }

        public Boolean ShouldIHandleEvent(IDomainEvent domainEvent){
            return domainEvent switch
            {
                VoucherGeneratedEvent _ => true,
                BarcodeAddedEvent _ => true,
                VoucherIssuedEvent _ => true,
                VoucherFullyRedeemedEvent _ => true,
                _ => false
            };
        }
    }
}
