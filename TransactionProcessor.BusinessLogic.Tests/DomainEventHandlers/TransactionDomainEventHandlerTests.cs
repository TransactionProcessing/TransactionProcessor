using EventStore.Client;
using Grpc.Core;
using MediatR;
using SimpleResults;
using TransactionProcessor.DomainEvents;
using Xunit.Abstractions;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers
{
    using EventHandling;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Testing;
    using Xunit;
    
    [Collection("Sequential")]
    public abstract class DomainEventHandlerTests
    {
        public Mock<IMediator> Mediator;

        public DomainEventHandlerTests(ITestOutputHelper testOutputHelper) {
            this.Mediator = new Mock<IMediator>();
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            this.Mediator.Setup(s => s.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        }
    }

    public class TransactionDomainEventHandlerTests : DomainEventHandlerTests
    {
        private readonly TransactionDomainEventHandler TransactionDomainEventHandler;
        
        public TransactionDomainEventHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            this.TransactionDomainEventHandler = new TransactionDomainEventHandler(this.Mediator.Object);
        }
        
        
        [Theory]
        [InlineData(typeof(FloatDomainEvents.FloatCreditPurchasedEvent))]
        [InlineData(typeof(TransactionDomainEvents.TransactionCostInformationRecordedEvent))]
        [InlineData(typeof(TransactionDomainEvents.TransactionHasBeenCompletedEvent))]
        [InlineData(typeof(TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent))]
        [InlineData(typeof(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent))]
        [InlineData(typeof(SettlementDomainEvents.MerchantFeeSettledEvent))]
        [InlineData(typeof(TransactionDomainEvents.CustomerEmailReceiptRequestedEvent))]
        [InlineData(typeof(TransactionDomainEvents.CustomerEmailReceiptResendRequestedEvent))]
        public async Task TransactionDomainEventHandler_EventPassedIn_EventIsHandled(Type eventType) {
            DomainEvent domainEvent = eventType.Name switch {
                nameof(FloatDomainEvents.FloatCreditPurchasedEvent) => new FloatDomainEvents.FloatCreditPurchasedEvent(TestData.FloatAggregateId, TestData.EstateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice),
                nameof(TransactionDomainEvents.TransactionCostInformationRecordedEvent) => TestData.TransactionCostInformationRecordedEvent,
                nameof(TransactionDomainEvents.TransactionHasBeenCompletedEvent) => TestData.DomainEvents.TransactionHasBeenCompletedEvent,
                nameof(TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent) => new TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent(TestData.TransactionId, TestData.EstateId, TestData.MerchantId, TestData.CalculatedFeeValue, 0, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeCalculateDateTime, TestData.TransactionFeeSettlementDueDate, TestData.TransactionDateTime),
                nameof(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent)=> TestData.SettledMerchantFeeAddedToTransactionEvent(TestData.SettlementDate),
                nameof(SettlementDomainEvents.MerchantFeeSettledEvent)=> new SettlementDomainEvents.MerchantFeeSettledEvent(TestData.SettlementAggregateId, TestData.EstateId, TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeValue, 0, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeCalculateDateTime, TestData.SettlementDate),
                nameof(TransactionDomainEvents.CustomerEmailReceiptRequestedEvent)=> TestData.CustomerEmailReceiptRequestedEvent,
                nameof(TransactionDomainEvents.CustomerEmailReceiptResendRequestedEvent)=> TestData.CustomerEmailReceiptResendRequestedEvent,
                _ => throw new NotSupportedException($"Event {eventType.Name} not supported")
            };

            var result = await this.TransactionDomainEventHandler.Handle(domainEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
    }
}


