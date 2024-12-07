using MediatR;
using SimpleResults;
using TransactionProcessor.Float.DomainEvents;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Manager;
    using BusinessLogic.Services;
    using Common;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.DataTransferObjects.Responses.Contract;
    using EstateManagement.DataTransferObjects.Responses.Merchant;
    using EventHandling;
    using FloatAggregate;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Moq;
    using SecurityService.Client;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventStore;
    using Shared.Exceptions;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using TransactionAggregate;
    using TransactionProcessor.Settlement.DomainEvents;
    using TransactionProcessor.Transaction.DomainEvents;
    using Xunit;
    
    public class TransactionDomainEventHandlerTests
    {
        private Mock<IAggregateRepository<SettlementAggregate, DomainEvent>> SettlementAggregateRepository;
        private Mock<IAggregateRepository<TransactionAggregate, DomainEvent>> TransactionAggregateRepository;

        private Mock<IFeeCalculationManager> FeeCalculationManager;

        private Mock<IEstateClient> EstateClient;

        private Mock<ISecurityServiceClient> SecurityServiceClient;

        private Mock<ITransactionReceiptBuilder> TransactionReceiptBuilder;

        private Mock<IMessagingServiceClient> MessagingServiceClient;

        private Mock<IAggregateRepository<FloatActivityAggregate, DomainEvent>> FloatActivityAggregateRepository;

        private Mock<IMemoryCacheWrapper> MemoryCache;

        private TransactionDomainEventHandler TransactionDomainEventHandler;
        private Mock<IMediator> Mediator;

        public TransactionDomainEventHandlerTests()
        {
            this.SettlementAggregateRepository = new Mock<IAggregateRepository<SettlementAggregate, DomainEvent>>();
            this.TransactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEvent>>();
            this.FloatActivityAggregateRepository = new Mock<IAggregateRepository<FloatActivityAggregate, DomainEvent>>();
            this.FeeCalculationManager = new Mock<IFeeCalculationManager>();
            this.EstateClient = new Mock<IEstateClient>();
            this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
            this.TransactionReceiptBuilder = new Mock<ITransactionReceiptBuilder>();
            this.MessagingServiceClient = new Mock<IMessagingServiceClient>();
            this.MemoryCache = new Mock<IMemoryCacheWrapper>();
            this.Mediator= new Mock<IMediator>();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            this.TransactionDomainEventHandler = new TransactionDomainEventHandler(this.TransactionAggregateRepository.Object,
                                                                                   this.FeeCalculationManager.Object,
                                                                                   this.EstateClient.Object,
                                                                                   this.SecurityServiceClient.Object,
                                                                                   this.TransactionReceiptBuilder.Object,
                                                                                   this.MessagingServiceClient.Object,
                                                                                   this.SettlementAggregateRepository.Object,
                                                                                   this.FloatActivityAggregateRepository.Object,
                                                                                   this.MemoryCache.Object,
                                                                                   this.Mediator.Object);

            this.Mediator.Setup(s => s.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        }
        
        
        [Theory]
        [InlineData(typeof(FloatCreditPurchasedEvent))]
        [InlineData(typeof(TransactionCostInformationRecordedEvent))]
        [InlineData(typeof(TransactionHasBeenCompletedEvent))]
        [InlineData(typeof(MerchantFeePendingSettlementAddedToTransactionEvent))]
        [InlineData(typeof(SettledMerchantFeeAddedToTransactionEvent))]
        [InlineData(typeof(MerchantFeeSettledEvent))]
        //[InlineData(typeof(CustomerEmailReceiptRequestedEvent))]
        //[InlineData(typeof(CustomerEmailReceiptResendRequestedEvent))]
        public async Task TransactionDomainEventHandler_EventPassedIn_EventIsHandled(Type eventType) {
            DomainEvent domainEvent = eventType.Name switch {
                nameof(FloatCreditPurchasedEvent) => new FloatCreditPurchasedEvent(TestData.FloatAggregateId, TestData.EstateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice),
                nameof(TransactionCostInformationRecordedEvent) => TestData.TransactionCostInformationRecordedEvent,
                nameof(TransactionHasBeenCompletedEvent) => TestData.TransactionHasBeenCompletedEvent,
                nameof(MerchantFeePendingSettlementAddedToTransactionEvent) => new MerchantFeePendingSettlementAddedToTransactionEvent(TestData.TransactionId, TestData.EstateId, TestData.MerchantId, TestData.CalculatedFeeValue, 0, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeCalculateDateTime, TestData.TransactionFeeSettlementDueDate, TestData.TransactionDateTime),
                nameof(SettledMerchantFeeAddedToTransactionEvent)=> TestData.SettledMerchantFeeAddedToTransactionEvent(TestData.SettlementDate),
                nameof(MerchantFeeSettledEvent)=> new MerchantFeeSettledEvent(TestData.SettlementAggregateId, TestData.EstateId, TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeValue, 0, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeCalculateDateTime, TestData.SettlementDate),
                nameof(CustomerEmailReceiptRequestedEvent)=> TestData.CustomerEmailReceiptRequestedEvent,
                nameof(CustomerEmailReceiptResendRequestedEvent)=> TestData.CustomerEmailReceiptResendRequestedEvent,
                _ => throw new NotSupportedException($"Event {eventType.Name} not supported")
            };

            var result = await this.TransactionDomainEventHandler.Handle(domainEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
    }
}


