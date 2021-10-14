namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Services;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Moq;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using Xunit;

    public class SettlementDomainServiceTests
    {
        [Fact]
        public async Task TransactionDomainService_ProcessSettlement_SettlementIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent>> settlementAggregateRepository =
                new Mock<IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent>>();
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(TestData.GetSettlementAggregateWithPendingMerchantFees(10));

            SettlementDomainService settlementDomainService=
                new SettlementDomainService(transactionAggregateManager.Object, settlementAggregateRepository.Object);

            ProcessSettlementResponse response = await settlementDomainService.ProcessSettlement(TestData.SettlementDate,
                                                                                                 TestData.EstateId,
                                                                                                 CancellationToken.None);

            response.ShouldNotBeNull();
            response.NumberOfFeesFailedToSettle.ShouldBe(0);
            response.NumberOfFeesPendingSettlement.ShouldBe(0);
            response.NumberOfFeesSuccessfullySettled.ShouldBe(10);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSettlement_SettlementAggregateNotCreated_NothingProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent>> settlementAggregateRepository =
                new Mock<IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent>>();
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(TestData.GetEmptySettlementAggregate);

            SettlementDomainService settlementDomainService =
                new SettlementDomainService(transactionAggregateManager.Object,
                                            settlementAggregateRepository.Object);

            ProcessSettlementResponse response = await settlementDomainService.ProcessSettlement(TestData.SettlementDate,
                                                                                                 TestData.EstateId,
                                                                                                 CancellationToken.None);

            response.ShouldNotBeNull();
            response.NumberOfFeesFailedToSettle.ShouldBe(0);
            response.NumberOfFeesPendingSettlement.ShouldBe(0);
            response.NumberOfFeesSuccessfullySettled.ShouldBe(0);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSettlement_SettlementAggregateNoFeesToSettles_NothingProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent>> settlementAggregateRepository =
                new Mock<IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent>>();
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(TestData.GetCreatedSettlementAggregate);

            SettlementDomainService settlementDomainService =
                new SettlementDomainService(transactionAggregateManager.Object,
                                            settlementAggregateRepository.Object);

            ProcessSettlementResponse response = await settlementDomainService.ProcessSettlement(TestData.SettlementDate,
                                                                                                 TestData.EstateId,
                                                                                                 CancellationToken.None);

            response.ShouldNotBeNull();
            response.NumberOfFeesFailedToSettle.ShouldBe(0);
            response.NumberOfFeesPendingSettlement.ShouldBe(0);
            response.NumberOfFeesSuccessfullySettled.ShouldBe(0);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSettlement_AddSettledFeeThrownException_SettlementProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            transactionAggregateManager.Setup(t => t.AddSettledFee(It.IsAny<Guid>(),
                                                                   It.IsAny<Guid>(),
                                                                   It.IsAny<CalculatedFee>(),
                                                                   It.IsAny<DateTime>(),
                                                                   It.IsAny<DateTime>(),
                                                                   It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
            
            Mock<IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent>> settlementAggregateRepository =
                new Mock<IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent>>();
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(TestData.GetSettlementAggregateWithPendingMerchantFees(10));

            SettlementDomainService settlementDomainService =
                new SettlementDomainService(transactionAggregateManager.Object, 
                                            settlementAggregateRepository.Object);

            ProcessSettlementResponse response = await settlementDomainService.ProcessSettlement(TestData.SettlementDate,
                                                                                                 TestData.EstateId,
                                                                                                 CancellationToken.None);

            response.ShouldNotBeNull();
            response.NumberOfFeesFailedToSettle.ShouldBe(10);
            response.NumberOfFeesPendingSettlement.ShouldBe(0);
            response.NumberOfFeesSuccessfullySettled.ShouldBe(0);
        }
    }
}