using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Services;
    using EstateManagement.Client;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Moq;
    using SecurityService.Client;
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

        private Mock<ISecurityServiceClient> securityServiceClient;

        private Mock<IEstateClient> estateClient;

        private SettlementDomainService settlementDomainService;

        public SettlementDomainServiceTests() {
            this.transactionAggregateRepository =
                new Mock<IAggregateRepository<TransactionAggregate, DomainEvent>>();
            this.settlementAggregateRepository =
                new Mock<IAggregateRepository<SettlementAggregate, DomainEvent>>();
            this.securityServiceClient = new Mock<ISecurityServiceClient>();
            this.estateClient = new Mock<IEstateClient>();
            
            this.settlementDomainService =
                new SettlementDomainService(this.transactionAggregateRepository.Object, settlementAggregateRepository.Object,
                                            this.securityServiceClient.Object, this.estateClient.Object);

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementIsProcessed()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.transactionAggregateRepository.SetupSequence(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(0)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(1)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(2)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(3)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(4)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(5)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(6)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(7)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(8)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(9)));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.transactionAggregateRepository.SetupSequence(s => s.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success());


            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            var result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementAggregateNotCreated_NothingProcessed()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetEmptySettlementAggregate()));
            settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            var result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementAggregateNoFeesToSettles_NothingProcessed()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);
            var result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_AddSettledFeeThrownException_SettlementProcessed()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.transactionAggregateRepository.SetupSequence(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(0)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(1)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(2)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(3)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(4)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(5)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(6)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(7)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(8)))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(9)));
            this.transactionAggregateRepository.SetupSequence(s => s.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Failure())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success());

            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            var result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }
    }
}
