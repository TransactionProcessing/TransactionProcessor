using System;
using System.Threading;
using System.Threading.Tasks;
using Imposter.Abstractions;
using MediatR;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.BusinessLogic.Events;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Repository;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers
{
    public class MerchantDomainEventHandlerTests : DomainEventHandlerTests
    {
        private readonly ITransactionProcessorReadModelRepositoryImposter TransactionProcessorReadModelRepository;
        private readonly MerchantDomainEventHandler DomainEventHandler;

        public MerchantDomainEventHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)  {
            this.TransactionProcessorReadModelRepository = new ITransactionProcessorReadModelRepositoryImposter();

            this.DomainEventHandler = new MerchantDomainEventHandler(this.TransactionProcessorReadModelRepository.Instance(),
                                                                     this.Mediator.Instance());
        }

        [Fact]
        public async Task MerchantDomainEventHandler_Handle_CallbackReceivedEnrichedEvent_Deposit_EventIsHandled()
        {
            this.TransactionProcessorReadModelRepository.GetMerchantFromReference(Arg<Guid>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any())
                                      .ReturnsAsync(Result.Success(TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts()));
            this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            CallbackReceivedEnrichedEvent domainEvent = TestData.DomainEvents.CallbackReceivedEnrichedEventDeposit;

            var result = await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();

        }

        [Fact]
        public async Task MerchantDomainEventHandler_Handle_CallbackReceivedEnrichedEvent_OtherType_EventIsHandled()
        {
            CallbackReceivedEnrichedEvent domainEvent = TestData.DomainEvents.CallbackReceivedEnrichedEventOtherType;

            var result = await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task MerchantDomainEventHandler_Handle_CallbackReceivedEnrichedEvent_Deposit_GetMerchantFailed_ResultIsFailure()
        {
            this.TransactionProcessorReadModelRepository.GetMerchantFromReference(Arg<Guid>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            CallbackReceivedEnrichedEvent domainEvent = TestData.DomainEvents.CallbackReceivedEnrichedEventDeposit;

            var result = await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();

        }

        #region Methods

        [Fact]
        public void MerchantDomainEventHandler_AddressAddedEvent_EventIsHandled()
        {
            MerchantDomainEvents.AddressAddedEvent addressAddedEvent = TestData.DomainEvents.AddressAddedEvent;
            
            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(addressAddedEvent, TestContext.Current.CancellationToken); });
        }
        
        [Fact]
        public void MerchantDomainEventHandler_ContactAddedEvent_EventIsHandled()
        {
            MerchantDomainEvents.ContactAddedEvent contactAddedEvent = TestData.DomainEvents.ContactAddedEvent;
            
            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(contactAddedEvent, TestContext.Current.CancellationToken); });
        }

        [Fact]
        public void MerchantDomainEventHandler_MerchantReferenceAllocatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantReferenceAllocatedEvent merchantReferenceAllocatedEvent = TestData.DomainEvents.MerchantReferenceAllocatedEvent;
            
            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(merchantReferenceAllocatedEvent, TestContext.Current.CancellationToken); });
        }

        [Fact]
        public void MerchantDomainEventHandler_DeviceAddedToMerchantEvent_EventIsHandled()
        {
            MerchantDomainEvents.DeviceAddedToMerchantEvent deviceAddedToMerchantEvent = TestData.DomainEvents.DeviceAddedToMerchantEvent;
            
            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(deviceAddedToMerchantEvent, TestContext.Current.CancellationToken); });
        }
        
        [Fact]
        public void MerchantDomainEventHandler_MerchantCreatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantCreatedEvent merchantCreatedEvent = TestData.DomainEvents.MerchantCreatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(merchantCreatedEvent, TestContext.Current.CancellationToken); });
        }

        [Fact]
        public void MerchantDomainEventHandler_OperatorAssignedToMerchantEvent_EventIsHandled()
        {
            MerchantDomainEvents.OperatorAssignedToMerchantEvent operatorAssignedToMerchantEvent = TestData.DomainEvents.OperatorAssignedToMerchantEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(operatorAssignedToMerchantEvent, TestContext.Current.CancellationToken); });
        }

        [Fact]
        public void MerchantDomainEventHandler_SecurityUserAddedEvent_EventIsHandled()
        {
            MerchantDomainEvents.SecurityUserAddedToMerchantEvent merchantSecurityUserAddedEvent = TestData.DomainEvents.MerchantSecurityUserAddedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(merchantSecurityUserAddedEvent, TestContext.Current.CancellationToken); });
        }

        [Fact]
        public void MerchantDomainEventHandler_SettlementScheduleChangedEvent_EventIsHandled()
        {
            MerchantDomainEvents.SettlementScheduleChangedEvent settlementScheduleChangedEvent = TestData.DomainEvents.SettlementScheduleChangedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(settlementScheduleChangedEvent, TestContext.Current.CancellationToken); });
        }

        [Fact(Skip = "No event yet")]
        public void MerchantDomainEventHandler_SettlementGeneratedEvent_EventIsHandled()
        {
            //StatementGeneratedEvent statementGeneratedEvent = TestData.StatementGeneratedEvent;

            //Should.NotThrow(async () => { await this.DomainEventHandler.Handle(statementGeneratedEvent, TestContext.Current.CancellationToken); });
        }

        [Fact]
        public void MerchantDomainEventHandler_TransactionHasBeenCompletedEvent_EventIsHandled()
        {
            TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent = TestData.DomainEvents.TransactionHasBeenCompletedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }

        [Fact]
        public void MerchantDomainEventHandler_MerchantNameUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantNameUpdatedEvent domainEvent = TestData.DomainEvents.MerchantNameUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }

        [Fact]
        public void MerchantDomainEventHandler_DeviceSwappedForMerchantEvent_EventIsHandled()
        {
            MerchantDomainEvents.DeviceSwappedForMerchantEvent domainEvent = TestData.DomainEvents.DeviceSwappedForMerchantEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_OperatorRemovedFromMerchantEvent_EventIsHandled()
        {
            MerchantDomainEvents.OperatorRemovedFromMerchantEvent domainEvent = TestData.DomainEvents.OperatorRemovedFromMerchantEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantAddressLine1UpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantAddressLine1UpdatedEvent domainEvent = TestData.DomainEvents.MerchantAddressLine1UpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantAddressLine2UpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantAddressLine2UpdatedEvent domainEvent = TestData.DomainEvents.MerchantAddressLine2UpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantAddressLine3UpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantAddressLine3UpdatedEvent domainEvent = TestData.DomainEvents.MerchantAddressLine3UpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantAddressLine4UpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantAddressLine4UpdatedEvent domainEvent = TestData.DomainEvents.MerchantAddressLine4UpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantCountyUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantCountyUpdatedEvent domainEvent = TestData.DomainEvents.MerchantCountyUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantRegionUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantRegionUpdatedEvent domainEvent = TestData.DomainEvents.MerchantRegionUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantTownUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantTownUpdatedEvent domainEvent = TestData.DomainEvents.MerchantTownUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantPostalCodeUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantPostalCodeUpdatedEvent domainEvent = TestData.DomainEvents.MerchantPostalCodeUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantContactNameUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantContactNameUpdatedEvent domainEvent = TestData.DomainEvents.MerchantContactNameUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantContactEmailAddressUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantContactEmailAddressUpdatedEvent domainEvent = TestData.DomainEvents.MerchantContactEmailAddressUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }
        [Fact]
        public void MerchantDomainEventHandler_MerchantContactPhoneNumberUpdatedEvent_EventIsHandled()
        {
            MerchantDomainEvents.MerchantContactPhoneNumberUpdatedEvent domainEvent = TestData.DomainEvents.MerchantContactPhoneNumberUpdatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }

        [Fact]
        public void MerchantDomainEventHandler_ContractAddedToMerchantEvent_EventIsHandled()
        {
            MerchantDomainEvents.ContractAddedToMerchantEvent domainEvent = TestData.DomainEvents.ContractAddedToMerchantEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }

        [Fact]
        public void MerchantDomainEventHandler_EstateCreatedEvent_EventIsHandled()
        {
            EstateDomainEvents.EstateCreatedEvent domainEvent = TestData.DomainEvents.EstateCreatedEvent;

            Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken); });
        }

        #endregion
    }
}

