﻿using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Services;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using Xunit;

    public class SettlementDomainServiceTests
    {
        private Mock<IAggregateRepository<TransactionAggregate, DomainEvent>> transactionAggregateRepository;
        private Mock<IAggregateRepository<SettlementAggregate, DomainEvent>> settlementAggregateRepository;
        private Mock<IAggregateRepository<MerchantAggregate, DomainEvent>> merchantAggregateRepository;
        private SettlementDomainService settlementDomainService;

        public SettlementDomainServiceTests() {
            this.transactionAggregateRepository =
                new Mock<IAggregateRepository<TransactionAggregate, DomainEvent>>();
            this.settlementAggregateRepository =
                new Mock<IAggregateRepository<SettlementAggregate, DomainEvent>>();
            this.merchantAggregateRepository =
                new Mock<IAggregateRepository<MerchantAggregate, DomainEvent>>();
            
            this.settlementDomainService =
                new SettlementDomainService(this.transactionAggregateRepository.Object, settlementAggregateRepository.Object,
                                            merchantAggregateRepository.Object);

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

            this.merchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            var result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_MerchantWithImmediateSettlement_SettlementIsProcessed()
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


            this.merchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

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
            this.merchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

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

            this.merchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            var result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_GetTransactionThrownException_SettlementProcessed()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            this.transactionAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            this.merchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            var result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_GetMerchantThrownException_SettlementProcessed()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            this.merchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            var result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddMerchantFeePendingSettlement_FeeAdded() {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            SettlementCommands.AddMerchantFeePendingSettlementCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeSettlementDueDate, TestData.MerchantId, TestData.EstateId);

            var result = await settlementDomainService.AddMerchantFeePendingSettlement(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddMerchantFeePendingSettlement_AggregateNotCreated_FeeAdded()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptySettlementAggregate()));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            SettlementCommands.AddMerchantFeePendingSettlementCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeSettlementDueDate, TestData.MerchantId, TestData.EstateId);

            var result = await settlementDomainService.AddMerchantFeePendingSettlement(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_FeeAdded()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.merchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            var result = await settlementDomainService.AddSettledFeeToSettlement(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_ImmediateSettlement_FeeAdded()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.merchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            var result = await settlementDomainService.AddSettledFeeToSettlement(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_FailedGettingMerchant_FeeAdded()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.merchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            var result = await settlementDomainService.AddSettledFeeToSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_SaveFailed_FeeAdded()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure);
            this.merchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            var result = await settlementDomainService.AddSettledFeeToSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_ExceptionThrown_FeeAdded()
        {
            settlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.settlementAggregateRepository
                .Setup(s => s.SaveChanges(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            this.merchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            var result = await settlementDomainService.AddSettledFeeToSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }
    }
}
