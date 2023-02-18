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
    using TransactionAggregate;
    using Xunit;

    public class SettlementDomainServiceTests
    {
        private Mock<IAggregateRepository<TransactionAggregate, DomainEvent>> transactionAggregateRepository;

        private Mock<IAggregateRepository<SettlementAggregate, DomainEvent>> settlementAggregateRepository;

        private SettlementDomainService settlementDomainService;

        public SettlementDomainServiceTests() {
            this.transactionAggregateRepository =
                new Mock<IAggregateRepository<TransactionAggregate, DomainEvent>>();
            this.settlementAggregateRepository =
                new Mock<IAggregateRepository<SettlementAggregate, DomainEvent>>();
            
            this.settlementDomainService =
                new SettlementDomainService(this.transactionAggregateRepository.Object, settlementAggregateRepository.Object);

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementIsProcessed()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(TestData.GetSettlementAggregateWithPendingMerchantFees(10));
            this.transactionAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);

            ProcessSettlementResponse response = await settlementDomainService.ProcessSettlement(TestData.SettlementDate,
                                                                                                 TestData.EstateId,
                                                                                                 CancellationToken.None);

            response.ShouldNotBeNull();
            response.NumberOfFeesFailedToSettle.ShouldBe(0);
            response.NumberOfFeesPendingSettlement.ShouldBe(0);
            response.NumberOfFeesSuccessfullySettled.ShouldBe(10);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementAggregateNotCreated_NothingProcessed()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(TestData.GetEmptySettlementAggregate);
            
            ProcessSettlementResponse response = await settlementDomainService.ProcessSettlement(TestData.SettlementDate,
                                                                                                 TestData.EstateId,
                                                                                                 CancellationToken.None);

            response.ShouldNotBeNull();
            response.NumberOfFeesFailedToSettle.ShouldBe(0);
            response.NumberOfFeesPendingSettlement.ShouldBe(0);
            response.NumberOfFeesSuccessfullySettled.ShouldBe(0);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementAggregateNoFeesToSettles_NothingProcessed()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(TestData.GetCreatedSettlementAggregate);

            ProcessSettlementResponse response = await settlementDomainService.ProcessSettlement(TestData.SettlementDate,
                                                                                                 TestData.EstateId,
                                                                                                 CancellationToken.None);

            response.ShouldNotBeNull();
            response.NumberOfFeesFailedToSettle.ShouldBe(0);
            response.NumberOfFeesPendingSettlement.ShouldBe(0);
            response.NumberOfFeesSuccessfullySettled.ShouldBe(0);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_AddSettledFeeThrownException_SettlementProcessed()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(TestData.GetSettlementAggregateWithPendingMerchantFees(10));

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