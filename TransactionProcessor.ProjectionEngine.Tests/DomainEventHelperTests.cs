using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.ProjectionEngine.Tests
{
    using Common;
    using EstateManagement.Merchant.DomainEvents;
    using Microsoft.Identity.Client;
    using Moq;
    using NLog;
    using Repository;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shouldly;
    using State;
    using TransactionProcessor.ProjectionEngine.Dispatchers;
    using TransactionProcessor.Transaction.DomainEvents;

    public class DomainEventHelperTests
    {
        [Theory]
        [InlineData("MerchantId",true)]
        [InlineData("MissingMerchantId", false)]
        public void DomainEventHelper_HasProperty_ResultAsExpected(String propertyName, Boolean expectedResult){
            Boolean result = DomainEventHelper.HasProperty(TestData.MerchantCreatedEvent, propertyName);
            result.ShouldBe(expectedResult);
        }

        [Theory]
        [InlineData("EstateId", true, "C81CD4E6-1F3B-431F-AA63-0ACAB7BC0CD3")]
        [InlineData("estateid", true, "C81CD4E6-1F3B-431F-AA63-0ACAB7BC0CD3")]
        [InlineData("EstateId", false,"C81CD4E6-1F3B-431F-AA63-0ACAB7BC0CD3")]
        [InlineData("estateid", false,"00000000-0000-0000-0000-000000000000")]
        public void DomainEventHelper_GetProperty_ResultAsExpected(String property, Boolean ignoreCase, String expectedValue)
        {
            Guid estateId = DomainEventHelper.GetProperty<Guid>(TestData.MerchantCreatedEvent, property, ignoreCase);
            estateId.ShouldBe(Guid.Parse(expectedValue));
        }

        [Fact]
        public void DomainEventHelper_GetEstateId_ResultAsExpected(){
            Guid estateId = DomainEventHelper.GetEstateId(TestData.MerchantCreatedEvent);
            estateId.ShouldBe(TestData.MerchantCreatedEvent.EstateId);
        }

        [Fact]
        public void DomainEventHelper_GetMerchantId_ResultAsExpected()
        {
            Guid merchantId = DomainEventHelper.GetMerchantId(TestData.MerchantCreatedEvent);
            merchantId.ShouldBe(TestData.MerchantCreatedEvent.MerchantId);
        }
    }

    public class MerchantBalanceStateDispatcherTests{

        [Theory]
        [InlineData(typeof(MerchantCreatedEvent))]
        [InlineData(typeof(ManualDepositMadeEvent))]
        [InlineData(typeof(AutomaticDepositMadeEvent))]
        [InlineData(typeof(WithdrawalMadeEvent))]
        [InlineData(typeof(TransactionHasStartedEvent))]
        [InlineData(typeof(TransactionHasBeenCompletedEvent))]
        [InlineData(typeof(SettledMerchantFeeAddedToTransactionEvent))]
        [InlineData(typeof(AddressAddedEvent))]
        public async Task MerchantBalanceStateDispatcher_Dispatch_EventHandled(Type eventType)
        {
            Mock<ITransactionProcessorReadRepository> repo = new Mock<ITransactionProcessorReadRepository>();
            MerchantBalanceStateDispatcher dispatcher = new MerchantBalanceStateDispatcher(repo.Object);

            MerchantBalanceState state = new MerchantBalanceState();

            IDomainEvent domainEvent = eventType switch{
                _ when eventType == typeof(MerchantCreatedEvent) => TestData.MerchantCreatedEvent,
                _ when eventType == typeof(ManualDepositMadeEvent) => TestData.ManualDepositMadeEvent,
                _ when eventType == typeof(WithdrawalMadeEvent) => TestData.WithdrawalMadeEvent,
                _ when eventType == typeof(AutomaticDepositMadeEvent) => TestData.AutomaticDepositMadeEvent,
                _ when eventType == typeof(TransactionHasStartedEvent) => TestData.GetTransactionHasStartedEvent(),
                _ when eventType == typeof(TransactionHasBeenCompletedEvent) => TestData.GetTransactionHasBeenCompletedEvent(),
                _ when eventType == typeof(SettledMerchantFeeAddedToTransactionEvent) => TestData.GetSettledMerchantFeeAddedToTransactionEvent(),
                _ => TestData.AddressAddedEvent
            };

            await dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
        }

        [Fact]
        public async Task MerchantBalanceStateDispatcher_Dispatch_TransactionHasBeenCompletedEvent_NotAuthorised_EventHandled()
        {
            Mock<ITransactionProcessorReadRepository> repo = new Mock<ITransactionProcessorReadRepository>();
            MerchantBalanceStateDispatcher dispatcher = new MerchantBalanceStateDispatcher(repo.Object);

            MerchantBalanceState state = new MerchantBalanceState();

            TransactionHasBeenCompletedEvent domainEvent = TestData.GetTransactionHasBeenCompletedEvent(false);

            await dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
        }

        [Fact]
        public async Task MerchantBalanceStateDispatcher_Dispatch_TransactionHasBeenCompletedEvent_NoAmount_EventHandled()
        {
            Mock<ITransactionProcessorReadRepository> repo = new Mock<ITransactionProcessorReadRepository>();
            MerchantBalanceStateDispatcher dispatcher = new MerchantBalanceStateDispatcher(repo.Object);

            MerchantBalanceState state = new MerchantBalanceState();

            TransactionHasBeenCompletedEvent domainEvent = TestData.GetTransactionHasBeenCompletedEvent(true, 0);

            await dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
        }

        [Fact]
        public async Task MerchantBalanceStateDispatcher_Dispatch_TransactionHasBeenCompletedEvent_NegativeAmount_EventHandled()
        {
            Mock<ITransactionProcessorReadRepository> repo = new Mock<ITransactionProcessorReadRepository>();
            MerchantBalanceStateDispatcher dispatcher = new MerchantBalanceStateDispatcher(repo.Object);

            MerchantBalanceState state = new MerchantBalanceState();

            TransactionHasBeenCompletedEvent domainEvent = TestData.GetTransactionHasBeenCompletedEvent(true, -1);

            await dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
        }
    }
}
