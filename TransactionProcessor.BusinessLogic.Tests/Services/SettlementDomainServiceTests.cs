using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using BusinessLogic.Services;
    using Microsoft.Extensions.Configuration;
    using Imposter;
    using Imposter.Abstractions;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using Shared.Serialisation;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Testing;
    using Xunit;

    public class SettlementDomainServiceTests
    {
        private IAggregateServiceImposter AggregateService;
        private SettlementDomainService settlementDomainService;

        public SettlementDomainServiceTests() {
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
            this.AggregateService =
                new IAggregateServiceImposter();
            IAggregateService AggregateServiceResolver() => this.AggregateService.Instance();
            this.settlementDomainService =
                new SettlementDomainService(AggregateServiceResolver);

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementIsProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(0))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(1))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(2))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(3))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(4))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(5))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(6))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(7))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(8))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(9))));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success());

            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_RunOutOfRetries_SettlementIsNotProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(0))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(1))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(2))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(3))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(4))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(5))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(6))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(7))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(8))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(9))));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure(new List<String> { "WrongExpectedVersion" }))
                .Then().ReturnsAsync(Result.Failure(new List<String> { "WrongExpectedVersion" }))
                .Then().ReturnsAsync(Result.Failure(new List<String> { "WrongExpectedVersion" }))
                .Then().ReturnsAsync(Result.Failure(new List<String> { "WrongExpectedVersion" }))
                .Then().ReturnsAsync(Result.Failure(new List<String> { "WrongExpectedVersion" }))
                .Then().ReturnsAsync(Result.Failure(new List<String> { "WrongExpectedVersion" }));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success());

            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeFalse();
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any()).Called(Count.Exactly(12));
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_RetryOnWrongExpected_SettlementIsProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(0))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(1))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(2))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(3))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(4))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(5))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(6))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(7))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(8))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(9))));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure(new List<String>{ "WrongExpectedVersion" }))
                .Then().ReturnsAsync(Result.Success());
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success());

            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any()).Called(Count.Exactly(2));
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_MerchantWithImmediateSettlement_SettlementIsProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(0)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(1)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(2)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(3)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(4)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(5)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(6)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(7)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(8)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(9)));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success());


            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementAggregateNotCreated_NothingProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                         .ReturnsAsync(Result.Success(TestData.GetEmptySettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SettlementAggregateNoFeesToSettles_NothingProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                         .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command =
                new(TestData.SettlementDate, TestData.MerchantId,
                    TestData.EstateId);
            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_AddSettledFeeThrownException_SettlementProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(0)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(1)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(2)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(3)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(4)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(5)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(6)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(7)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(8)))
                .Then().ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(9)));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Failure())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success());

            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_GetTransactionThrownException_SettlementProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                         .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ThrowsAsync(new Exception());

            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_GetMerchantThrownException_SettlementProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ThrowsAsync(new Exception());

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddMerchantFeePendingSettlement_FeeAdded() {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            SettlementCommands.AddMerchantFeePendingSettlementCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeSettlementDueDate, TestData.MerchantId, TestData.EstateId);

            Result result = await settlementDomainService.AddMerchantFeePendingSettlement(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddMerchantFeePendingSettlement_AggregateNotCreated_FeeAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetEmptySettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            SettlementCommands.AddMerchantFeePendingSettlementCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeSettlementDueDate, TestData.MerchantId, TestData.EstateId);

            Result result = await settlementDomainService.AddMerchantFeePendingSettlement(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_FeeAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_ImmediateSettlement_FeeAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_FailedGettingMerchant_FeeAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.NotFound());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_SaveFailed_FeeAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_ExceptionThrown_FeeAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ThrowsAsync(new Exception());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_GetSettlementAggregateFailed_SettlementNotProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("Error retrieving settlement aggregate"));

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_GetMerchantFailed_SettlementNotProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("Error retrieving merchant aggregate"));

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_GetTransactionFailed_SettlementNotProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("Error retrieving transaction aggregate"));

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_SaveSettlementAggregateFailed_SettlementNotProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithNoContracts(SettlementSchedule.Weekly));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("Save failed"));

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_ProcessSettlement_WithScheduledSettlement_SettlementIsProcessed()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetSettlementAggregateWithPendingMerchantFees(10)));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(0))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(1))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(2))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(3))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(4))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(5))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(6))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(7))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(8))))
                .Then().ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.FeeIds.GetValueOrDefault(9))));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success())
                .Then().ReturnsAsync(Result.Success());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithNoContracts(SettlementSchedule.Weekly));

            SettlementCommands.ProcessSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId);

            Result<Guid> result = await settlementDomainService.ProcessSettlement(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task SettlementDomainService_AddMerchantFeePendingSettlement_GetSettlementFailed_FeeNotAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("Error retrieving settlement aggregate"));

            SettlementCommands.AddMerchantFeePendingSettlementCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeSettlementDueDate, TestData.MerchantId, TestData.EstateId);

            Result result = await settlementDomainService.AddMerchantFeePendingSettlement(command, TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddMerchantFeePendingSettlement_StateChangeFailed_FeeNotAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("Save failed"));

            SettlementCommands.AddMerchantFeePendingSettlementCommand command = new(Guid.Empty, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeSettlementDueDate, TestData.MerchantId, TestData.EstateId);

            Result result = await settlementDomainService.AddMerchantFeePendingSettlement(command, TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddMerchantFeePendingSettlement_SaveFailed_FeeNotAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("Save failed"));

            SettlementCommands.AddMerchantFeePendingSettlementCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeSettlementDueDate, TestData.MerchantId, TestData.EstateId);

            Result result = await settlementDomainService.AddMerchantFeePendingSettlement(command, TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddMerchantFeePendingSettlement_ExceptionThrown_FeeNotAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ThrowsAsync(new Exception("Unexpected error"));

            SettlementCommands.AddMerchantFeePendingSettlementCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.TransactionFeeValue, TestData.TransactionFeeSettlementDueDate, TestData.MerchantId, TestData.EstateId);

            Result result = await settlementDomainService.AddMerchantFeePendingSettlement(command, TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_GetSettlementFailed_FeeNotAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("Error retrieving settlement aggregate"));

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task SettlementDomainService_AddSettledFeeToSettlement_WithScheduledSettlement_FeeAdded()
        {
            this.AggregateService.GetLatest<SettlementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCreatedSettlementAggregate()));
            this.AggregateService.Save(Arg<SettlementAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithNoContracts(SettlementSchedule.Weekly));

            SettlementCommands.AddSettledFeeToSettlementCommand command = new(TestData.SettlementDate, TestData.MerchantId, TestData.EstateId, TestData.TransactionFeeId, TestData.TransactionId);

            Result result = await settlementDomainService.AddSettledFeeToSettlement(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
        }
    }
}

