namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Manager;
    using BusinessLogic.Services;
    using EstateManagement.Client;
    using EventHandling;
    using MessagingService.Client;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using SecurityService.Client;
    using Shared.EventStore.EventStore;
    using Shared.General;
    using Shared.Logger;
    using Testing;
    using TransactionAggregate;
    using Xunit;

    public class TransactionDomainEventHandlerTests
    {
        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_SuccessfulSale_EventIsHandled()
        {
            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);
            Mock<IFeeCalculationManager> feeCalculationManager = new Mock<IFeeCalculationManager>();
            feeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>())).Returns(TestData.CalculatedMerchantFees);
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            estateClient.Setup(e => e.GetTransactionFeesForProduct(It.IsAny<String>(),
                                                                   It.IsAny<Guid>(),
                                                                   It.IsAny<Guid>(),
                                                                   It.IsAny<Guid>(),
                                                                   It.IsAny<Guid>(),
                                                                   It.IsAny<CancellationToken>())).ReturnsAsync(TestData.ContractProductTransactionFees);

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            Mock<ITransactionReceiptBuilder> transactionReceiptBulder = new Mock<ITransactionReceiptBuilder>();
            Mock<IMessagingServiceClient> messagingServiceClient = new Mock<IMessagingServiceClient>();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            TransactionDomainEventHandler transactionDomainEventHandler = new TransactionDomainEventHandler(transactionAggregateManager.Object,
                                                                                                      feeCalculationManager.Object,
                                                                                                      estateClient.Object,
                                                                                                      securityServiceClient.Object,
                                                                                                      transactionReceiptBulder.Object,
                                                                                                      messagingServiceClient.Object);

            await transactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_UnsuccessfulSale_EventIsHandled()
        {
            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedDeclinedSaleTransactionAggregate);
            Mock<IFeeCalculationManager> feeCalculationManager = new Mock<IFeeCalculationManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            Mock<ITransactionReceiptBuilder> transactionReceiptBulder = new Mock<ITransactionReceiptBuilder>();
            Mock<IMessagingServiceClient> messagingServiceClient = new Mock<IMessagingServiceClient>();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            TransactionDomainEventHandler transactionDomainEventHandler = new TransactionDomainEventHandler(transactionAggregateManager.Object,
                                                                                                      feeCalculationManager.Object,
                                                                                                      estateClient.Object,
                                                                                                      securityServiceClient.Object,
                                                                                                      transactionReceiptBulder.Object,
                                                                                                      messagingServiceClient.Object);

            await transactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_IncompleteSale_EventIsHandled()
        {
            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetIncompleteAuthorisedSaleTransactionAggregate);
            Mock<IFeeCalculationManager> feeCalculationManager = new Mock<IFeeCalculationManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            Mock<ITransactionReceiptBuilder> transactionReceiptBulder = new Mock<ITransactionReceiptBuilder>();
            Mock<IMessagingServiceClient> messagingServiceClient = new Mock<IMessagingServiceClient>();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            TransactionDomainEventHandler transactionDomainEventHandler = new TransactionDomainEventHandler(transactionAggregateManager.Object,
                                                                                                            feeCalculationManager.Object,
                                                                                                            estateClient.Object,
                                                                                                            securityServiceClient.Object,
                                                                                                            transactionReceiptBulder.Object,
                                                                                                            messagingServiceClient.Object);

            await transactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_SaleWithNoProductDetails_EventIsHandled()
        {
            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedSaleWithNoProductDetailsTransactionAggregate);
            Mock<IFeeCalculationManager> feeCalculationManager = new Mock<IFeeCalculationManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            Mock<ITransactionReceiptBuilder> transactionReceiptBulder = new Mock<ITransactionReceiptBuilder>();
            Mock<IMessagingServiceClient> messagingServiceClient = new Mock<IMessagingServiceClient>();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            TransactionDomainEventHandler transactionDomainEventHandler = new TransactionDomainEventHandler(transactionAggregateManager.Object,
                                                                                                            feeCalculationManager.Object,
                                                                                                            estateClient.Object,
                                                                                                            securityServiceClient.Object,
                                                                                                            transactionReceiptBulder.Object,
                                                                                                            messagingServiceClient.Object);

            await transactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_AuthorisedLogon_EventIsHandled()
        {
            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedLogonTransactionAggregate);
            Mock<IFeeCalculationManager> feeCalculationManager = new Mock<IFeeCalculationManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            Mock<ITransactionReceiptBuilder> transactionReceiptBulder = new Mock<ITransactionReceiptBuilder>();
            Mock<IMessagingServiceClient> messagingServiceClient = new Mock<IMessagingServiceClient>();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            TransactionDomainEventHandler transactionDomainEventHandler = new TransactionDomainEventHandler(transactionAggregateManager.Object,
                                                                                                            feeCalculationManager.Object,
                                                                                                            estateClient.Object,
                                                                                                            securityServiceClient.Object,
                                                                                                            transactionReceiptBulder.Object,
                                                                                                            messagingServiceClient.Object);

            await transactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_CustomerEmailReceiptRequestedEvent_EventIsHandled()
        {
            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);
            Mock<IFeeCalculationManager> feeCalculationManager = new Mock<IFeeCalculationManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            Mock<ITransactionReceiptBuilder> transactionReceiptBulder = new Mock<ITransactionReceiptBuilder>();
            Mock<IMessagingServiceClient> messagingServiceClient = new Mock<IMessagingServiceClient>();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            TransactionDomainEventHandler transactionDomainEventHandler = new TransactionDomainEventHandler(transactionAggregateManager.Object,
                                                                                                            feeCalculationManager.Object,
                                                                                                            estateClient.Object,
                                                                                                            securityServiceClient.Object,
                                                                                                            transactionReceiptBulder.Object,
                                                                                                            messagingServiceClient.Object);

            await transactionDomainEventHandler.Handle(TestData.CustomerEmailReceiptRequestedEvent, CancellationToken.None);
        }
    }
}
