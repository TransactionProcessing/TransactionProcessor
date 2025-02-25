using EventStore.Client;
using Grpc.Core;
using MediatR;
using SimpleResults;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EventHandling;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using Xunit;
    
    public class TransactionDomainEventHandlerTests
    {
        private TransactionDomainEventHandler TransactionDomainEventHandler;
        private Mock<IMediator> Mediator;

        public TransactionDomainEventHandlerTests()
        {
            this.Mediator= new Mock<IMediator>();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            this.TransactionDomainEventHandler = new TransactionDomainEventHandler(this.Mediator.Object);

            this.Mediator.Setup(s => s.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        }
        
        
        [Theory]
        [InlineData(typeof(FloatDomainEvents.FloatCreditPurchasedEvent))]
        [InlineData(typeof(TransactionDomainEvents.TransactionCostInformationRecordedEvent))]
        [InlineData(typeof(TransactionDomainEvents.TransactionHasBeenCompletedEvent))]
        [InlineData(typeof(TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent))]
        [InlineData(typeof(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent))]
        [InlineData(typeof(SettlementDomainEvents.MerchantFeeSettledEvent))]
        //[InlineData(typeof(CustomerEmailReceiptRequestedEvent))]
        //[InlineData(typeof(CustomerEmailReceiptResendRequestedEvent))]
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

        [Theory]
        [InlineData(typeof(FloatDomainEvents.FloatCreditPurchasedEvent))]
        [InlineData(typeof(TransactionDomainEvents.TransactionCostInformationRecordedEvent))]
        //[InlineData(typeof(TransactionDomainEvents.TransactionHasBeenCompletedEvent))]
        [InlineData(typeof(TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent))]
        [InlineData(typeof(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent))]
        [InlineData(typeof(SettlementDomainEvents.MerchantFeeSettledEvent))]
        //[InlineData(typeof(CustomerEmailReceiptRequestedEvent))]
        //[InlineData(typeof(CustomerEmailReceiptResendRequestedEvent))]
        public async Task TransactionDomainEventHandler_EventPassedIn_Retry_EventIsHandled(Type eventType)
        {
            this.Mediator.SetupSequence(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new WrongExpectedVersionException("Stream1", StreamRevision.None, StreamRevision.None))
                .ThrowsAsync(new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded")))
                .ReturnsAsync(Result.Success());

            DomainEvent domainEvent = eventType.Name switch
            {
                nameof(FloatDomainEvents.FloatCreditPurchasedEvent) => new FloatDomainEvents.FloatCreditPurchasedEvent(TestData.FloatAggregateId, TestData.EstateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice),
                nameof(TransactionDomainEvents.TransactionCostInformationRecordedEvent) => TestData.TransactionCostInformationRecordedEvent,
                nameof(TransactionDomainEvents.TransactionHasBeenCompletedEvent) => TestData.DomainEvents.TransactionHasBeenCompletedEvent,
                nameof(TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent) => new TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent(TestData.TransactionId, TestData.EstateId, TestData.MerchantId, TestData.CalculatedFeeValue, 0, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeCalculateDateTime, TestData.TransactionFeeSettlementDueDate, TestData.TransactionDateTime),
                nameof(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent) => TestData.SettledMerchantFeeAddedToTransactionEvent(TestData.SettlementDate),
                nameof(SettlementDomainEvents.MerchantFeeSettledEvent) => new SettlementDomainEvents.MerchantFeeSettledEvent(TestData.SettlementAggregateId, TestData.EstateId, TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeValue, 0, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeCalculateDateTime, TestData.SettlementDate),
                //nameof(TransactionDomainEvents.CustomerEmailReceiptRequestedEvent) => TestData.CustomerEmailReceiptRequestedEvent,
                //nameof(TransactionDomainEvents.CustomerEmailReceiptResendRequestedEvent) => TestData.CustomerEmailReceiptResendRequestedEvent,
                _ => throw new NotSupportedException($"Event {eventType.Name} not supported")
            };

            var result = await this.TransactionDomainEventHandler.Handle(domainEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
    }
}


