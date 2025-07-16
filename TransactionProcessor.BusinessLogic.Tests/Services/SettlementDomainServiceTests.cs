using SimpleResults;
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
        private Mock<IAggregateService> AggregateService;
        private SettlementDomainService settlementDomainService;

        public SettlementDomainServiceTests() {
            this.AggregateService =
                new Mock<IAggregateService>();
            
            this.settlementDomainService =
                new SettlementDomainService(this.AggregateService.Object);

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementIsProcessed()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.SetupSequence(s => s.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(0))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(1))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(2))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(3))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(4))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(5))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(6))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(7))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(8))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(9))));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.AggregateService.SetupSequence(s => s.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
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

            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_RunOutOfRetries_SettlementIsNotProcessed()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.SetupSequence(s => s.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(0))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(1))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(2))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(3))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(4))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(5))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(6))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(7))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(8))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(9))));
            this.AggregateService.SetupSequence(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Failure(new List<String>() { "WrongExpectedVersion" }))
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Failure(new List<String>() { "WrongExpectedVersion" }))
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Failure(new List<String>() { "WrongExpectedVersion" }))
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Failure(new List<String>() { "WrongExpectedVersion" }))
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Failure(new List<String>() { "WrongExpectedVersion" }))
                .ReturnsAsync(Result.Failure(new List<String>() { "WrongExpectedVersion" }));
            this.AggregateService.SetupSequence(s => s.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
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

            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
            this.AggregateService.Verify(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()), Times.Exactly(11));
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_RetryOnWrongExpected_SettlementIsProcessed()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.SetupSequence(s => s.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(0))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(1))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(2))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(3))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(4))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(5))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(6))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(7))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(8))))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(9))));
            this.AggregateService.SetupSequence(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Failure(new List<String>(){ "WrongExpectedVersion" }))
                .ReturnsAsync(Result.Success())
                .ReturnsAsync(Result.Success());
            this.AggregateService.SetupSequence(s => s.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
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

            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
            this.AggregateService.Verify(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_MerchantWithImmediateSettlement_SettlementIsProcessed()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.SetupSequence(s => s.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.AggregateService.SetupSequence(s => s.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
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


            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementAggregateNotCreated_NothingProcessed()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetEmptySettlementAggregate()));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementAggregateNoFeesToSettles_NothingProcessed()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);
            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_AddSettledFeeThrownException_SettlementProcessed()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            this.AggregateService.SetupSequence(s => s.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
            this.AggregateService.SetupSequence(s => s.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
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

            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_GetTransactionThrownException_SettlementProcessed()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            this.AggregateService.Setup(s => s.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_GetMerchantThrownException_SettlementProcessed()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddMerchantFeePendingSettlement_FeeAdded() {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            SettlementCommands.AddMerchantFeePendingSettlementCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeSettlementDueDate, TestData.MerchantId, TestData.EstateId);

            Result result = await settlementDomainService.AddMerchantFeePendingSettlement(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddMerchantFeePendingSettlement_AggregateNotCreated_FeeAdded()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptySettlementAggregate()));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);

            SettlementCommands.AddMerchantFeePendingSettlementCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeSettlementDueDate, TestData.MerchantId, TestData.EstateId);

            Result result = await settlementDomainService.AddMerchantFeePendingSettlement(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_FeeAdded()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_ImmediateSettlement_FeeAdded()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_FailedGettingMerchant_FeeAdded()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.NotFound());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_SaveFailed_FeeAdded()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure);
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_ExceptionThrown_FeeAdded()
        {
            this.AggregateService.Setup(s => s.GetLatest<SettlementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService
                .Setup(s => s.Save(It.IsAny<SettlementAggregate>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }
    }
}
