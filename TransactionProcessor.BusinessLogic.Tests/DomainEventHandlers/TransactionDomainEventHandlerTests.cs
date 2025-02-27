using EventStore.Client;
using Grpc.Core;
using MediatR;
using SimpleResults;
using TransactionProcessor.DomainEvents;
using Xunit.Abstractions;

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
    
    public class TestLogger : ILogger
    {
        private readonly ITestOutputHelper TestOutputHelper;

        public TestLogger(ITestOutputHelper testOutputHelper) {
            this.TestOutputHelper = testOutputHelper;
        }
        public void LogCritical(string message)
        {
            this.TestOutputHelper.WriteLine(message);
        }

        public void LogCritical(Exception exception) {
            this.TestOutputHelper.WriteLine(exception.Message);
        }

        public void LogCritical(String message,
                                Exception exception) {
            this.TestOutputHelper.WriteLine(message);
            this.TestOutputHelper.WriteLine(exception.Message);
        }

        public void LogDebug(string message)
        {
            this.TestOutputHelper.WriteLine(message);
        }

        public void LogError(Exception exception) {
            this.TestOutputHelper.WriteLine(exception.Message);
        }

        public void LogError(String message,
                             Exception exception) {
            this.TestOutputHelper.WriteLine(message);
            this.TestOutputHelper.WriteLine(exception.Message);
        }

        public void LogError(string message)
        {
            this.TestOutputHelper.WriteLine(message);
        }
        public void LogInformation(string message)
        {
            this.TestOutputHelper.WriteLine(message);
        }
        public void LogTrace(string message)
        {
            this.TestOutputHelper.WriteLine(message);
        }
        public void LogWarning(string message)
        {
            this.TestOutputHelper.WriteLine(message);
        }

        public Boolean IsInitialised { get; set; }
    }

    [Collection("Sequential")]
    public abstract class DomainEventHandlerTests
    {
        public Mock<IMediator> Mediator;

        public DomainEventHandlerTests(ITestOutputHelper testOutputHelper) {
            this.Mediator = new Mock<IMediator>();
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(new TestLogger(testOutputHelper));
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

        [Theory]
        [InlineData(typeof(FloatDomainEvents.FloatCreditPurchasedEvent))]
        [InlineData(typeof(TransactionDomainEvents.TransactionCostInformationRecordedEvent))]
        //[InlineData(typeof(TransactionDomainEvents.TransactionHasBeenCompletedEvent))]
        [InlineData(typeof(TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent))]
        [InlineData(typeof(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent))]
        [InlineData(typeof(SettlementDomainEvents.MerchantFeeSettledEvent))]
        //[InlineData(typeof(CustomerEmailReceiptRequestedEvent))]
        //[InlineData(typeof(CustomerEmailReceiptResendRequestedEvent))]
        public async Task TransactionDomainEventHandler_EventPassedIn_ExceptionNotRetrid_EventIsHandled(Type eventType)
        {
            this.Mediator.SetupSequence(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ApplicationException());

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
            result.IsFailed.ShouldBeTrue();
        }
    }
}


