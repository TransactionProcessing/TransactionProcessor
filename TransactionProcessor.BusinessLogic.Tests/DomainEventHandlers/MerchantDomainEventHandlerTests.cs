using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.BusinessLogic.Events;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Repository;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers
{
    public class MerchantDomainEventHandlerTests
    {
        private Mock<IAggregateRepository<MerchantAggregate, DomainEvent>> MerchantAggregateRepository;
        private Mock<ITransactionProcessorReadModelRepository> TransactionProcessorReadModelRepository;
        private Mock<IMediator> Mediator;

        private MerchantDomainEventHandler DomainEventHandler;

        public MerchantDomainEventHandlerTests() {
            Logger.Initialise(NullLogger.Instance);

            this.Mediator = new Mock<IMediator>();
            this.MerchantAggregateRepository = new Mock<IAggregateRepository<MerchantAggregate, DomainEvent>>();
            this.TransactionProcessorReadModelRepository = new Mock<ITransactionProcessorReadModelRepository>();

            this.DomainEventHandler = new MerchantDomainEventHandler(this.MerchantAggregateRepository.Object,
                                                                                           this.TransactionProcessorReadModelRepository.Object,
                                                                                           this.Mediator.Object);
        }

        [Fact]
        public async Task MerchantDomainEventHandler_Handle_CallbackReceivedEnrichedEvent_Deposit_EventIsHandled()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchantFromReference(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(Result.Success(TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts()));
            this.Mediator.Setup(m => m.Send(It.IsAny<MerchantCommands.MakeMerchantDepositCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            CallbackReceivedEnrichedEvent domainEvent = TestData.DomainEvents.CallbackReceivedEnrichedEventDeposit;

            var result = await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

        }

        [Fact]
        public async Task MerchantDomainEventHandler_Handle_CallbackReceivedEnrichedEvent_OtherType_EventIsHandled()
        {
            CallbackReceivedEnrichedEvent domainEvent = TestData.DomainEvents.CallbackReceivedEnrichedEventOtherType;

            var result = await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task MerchantDomainEventHandler_Handle_CallbackReceivedEnrichedEvent_Deposit_GetMerchantFailed_ResultIsFailure()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchantFromReference(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            CallbackReceivedEnrichedEvent domainEvent = TestData.DomainEvents.CallbackReceivedEnrichedEventDeposit;

            var result = await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();

        }

        #region Methods

        [Fact]
        public void MerchantDomainEventHandler_AddressAddedEvent_EventIsHandled()
        {
            MerchantDomainEvents.AddressAddedEvent addressAddedEvent = TestData.DomainEvents.AddressAddedEvent;
            
            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(addressAddedEvent, CancellationToken.None); });
        }
        
        [Fact]
        public void MerchantDomainEventHandler_ContactAddedEvent_EventIsHandled()
        {
            MerchantDomainEvents.ContactAddedEvent contactAddedEvent = TestData.DomainEvents.ContactAddedEvent;
            
            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(contactAddedEvent, CancellationToken.None); });
        }

        [Fact]
        public void MerchantDomainEventHandler_MerchantReferenceAllocatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantReferenceAllocatedEvent merchantReferenceAllocatedEvent = TestData.DomainEvents.MerchantReferenceAllocatedEvent;
            
            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(merchantReferenceAllocatedEvent, CancellationToken.None); });
        }

        [Fact]
        public void MerchantDomainEventHandler_DeviceAddedToMerchantEvent_EventIsHandled()
        {
            MerchantDomainEvents.DeviceAddedToMerchantEvent deviceAddedToMerchantEvent = TestData.DomainEvents.DeviceAddedToMerchantEvent;
            
            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(deviceAddedToMerchantEvent, CancellationToken.None); });
        }
        
        [Fact]
        public void MerchantDomainEventHandler_MerchantCreatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantCreatedEvent merchantCreatedEvent = TestData.DomainEvents.MerchantCreatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(merchantCreatedEvent, CancellationToken.None); });
        }

        [Fact]
        public void MerchantDomainEventHandler_OperatorAssignedToMerchantEvent_EventIsHandled()
        {
            MerchantDomainEvents.OperatorAssignedToMerchantEvent operatorAssignedToMerchantEvent = TestData.DomainEvents.OperatorAssignedToMerchantEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(operatorAssignedToMerchantEvent, CancellationToken.None); });
        }

        [Fact]
        public void MerchantDomainEventHandler_SecurityUserAddedEvent_EventIsHandled()
        {
            MerchantDomainEvents.SecurityUserAddedToMerchantEvent merchantSecurityUserAddedEvent = TestData.DomainEvents.MerchantSecurityUserAddedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(merchantSecurityUserAddedEvent, CancellationToken.None); });
        }

        [Fact]
        public void MerchantDomainEventHandler_SettlementScheduleChangedEvent_EventIsHandled()
        {
            MerchantDomainEvents.SettlementScheduleChangedEvent settlementScheduleChangedEvent = TestData.DomainEvents.SettlementScheduleChangedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(settlementScheduleChangedEvent, CancellationToken.None); });
        }

        [Fact(Skip = "No event yet")]
        public void MerchantDomainEventHandler_SettlementGeneratedEvent_EventIsHandled()
        {
            //StatementGeneratedEvent statementGeneratedEvent = TestData.StatementGeneratedEvent;

            //Should.NotThrow(async () => { await this.DomainEventHandler.Handle(statementGeneratedEvent, CancellationToken.None); });
        }

        [Fact]
        public void MerchantDomainEventHandler_TransactionHasBeenCompletedEvent_EventIsHandled()
        {
            TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent = TestData.DomainEvents.TransactionHasBeenCompletedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }

        [Fact]
        public void MerchantDomainEventHandler_MerchantNameUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantNameUpdatedEvent domainEvent = TestData.DomainEvents.MerchantNameUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }

        [Fact]
        public void MerchantDomainEventHandler_DeviceSwappedForMerchantEvent_EventIsHandled()
        {
            MerchantDomainEvents.DeviceSwappedForMerchantEvent domainEvent = TestData.DomainEvents.DeviceSwappedForMerchantEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_OperatorRemovedFromMerchantEvent_EventIsHandled()
        {
            MerchantDomainEvents.OperatorRemovedFromMerchantEvent domainEvent = TestData.DomainEvents.OperatorRemovedFromMerchantEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantAddressLine1UpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantAddressLine1UpdatedEvent domainEvent = TestData.DomainEvents.MerchantAddressLine1UpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantAddressLine2UpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantAddressLine2UpdatedEvent domainEvent = TestData.DomainEvents.MerchantAddressLine2UpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantAddressLine3UpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantAddressLine3UpdatedEvent domainEvent = TestData.DomainEvents.MerchantAddressLine3UpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantAddressLine4UpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantAddressLine4UpdatedEvent domainEvent = TestData.DomainEvents.MerchantAddressLine4UpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantCountyUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantCountyUpdatedEvent domainEvent = TestData.DomainEvents.MerchantCountyUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantRegionUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantRegionUpdatedEvent domainEvent = TestData.DomainEvents.MerchantRegionUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantTownUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantTownUpdatedEvent domainEvent = TestData.DomainEvents.MerchantTownUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantPostalCodeUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantPostalCodeUpdatedEvent domainEvent = TestData.DomainEvents.MerchantPostalCodeUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantContactNameUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantContactNameUpdatedEvent domainEvent = TestData.DomainEvents.MerchantContactNameUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantContactEmailAddressUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantContactEmailAddressUpdatedEvent domainEvent = TestData.DomainEvents.MerchantContactEmailAddressUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantContactPhoneNumberUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantContactPhoneNumberUpdatedEvent domainEvent = TestData.DomainEvents.MerchantContactPhoneNumberUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }

        [Fact]
        public void MerchantDomainEventHandler_ContractAddedToMerchantEvent_EventIsHandled()
        {
            MerchantDomainEvents.ContractAddedToMerchantEvent domainEvent = TestData.DomainEvents.ContractAddedToMerchantEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }

        [Fact]
        public void MerchantDomainEventHandler_EstateCreatedEvent_EventIsHandled()
        {
            EstateDomainEvents.EstateCreatedEvent domainEvent = TestData.DomainEvents.EstateCreatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
        }

        #endregion
    }
}
