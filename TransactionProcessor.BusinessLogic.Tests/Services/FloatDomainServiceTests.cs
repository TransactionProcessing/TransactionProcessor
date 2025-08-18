using System;
using System.Threading.Tasks;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using TransactionProcessor.Aggregates;
    using TransactionProcessor.BusinessLogic.Services;
    using Xunit;

    public class FloatDomainServiceTests
    {
        private readonly Mock<IAggregateService> AggregateService;
        private readonly FloatDomainService FloatDomainService;

        public FloatDomainServiceTests(){

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.AggregateService = new Mock<IAggregateService>();
            IAggregateService AggregateServiceResolver() => this.AggregateService.Object;
            this.FloatDomainService = new FloatDomainService(AggregateServiceResolver);
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_FloatCreated(){

            this.AggregateService.Setup(f => f.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Setup(f => f.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(Models.Contract.CalculationType.Fixed, Models.Contract.FeeType.Merchant)));

            this.AggregateService.Setup(f => f.GetLatest<FloatAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatAggregate()));
            this.AggregateService.Setup(f => f.Save<FloatAggregate>(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

            FloatCommands.CreateFloatForContractProductCommand command = new FloatCommands.CreateFloatForContractProductCommand(TestData.EstateId, TestData.ContractId,
                TestData.ProductId, TestData.FloatCreatedDateTime);
            Result result = await this.FloatDomainService.CreateFloatForContractProduct(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidEstate_ErrorThrown()
        {
            this.AggregateService.Setup(f => f.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());
            FloatCommands.CreateFloatForContractProductCommand command = new FloatCommands.CreateFloatForContractProductCommand(TestData.EstateId, TestData.ContractId,
                TestData.ProductId, TestData.FloatCreatedDateTime);
            Result result = await this.FloatDomainService.CreateFloatForContractProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidContract_ErrorThrown()
        {
            this.AggregateService.Setup(f => f.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Setup(f => f.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

            FloatCommands.CreateFloatForContractProductCommand command = new FloatCommands.CreateFloatForContractProductCommand(TestData.EstateId, TestData.ContractId,
                TestData.ProductId, TestData.FloatCreatedDateTime);
            Result result = await this.FloatDomainService.CreateFloatForContractProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidContractProduct_ErrorThrown()
        {
            this.AggregateService.Setup(f => f.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Setup(f => f.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            FloatCommands.CreateFloatForContractProductCommand command = new FloatCommands.CreateFloatForContractProductCommand(TestData.EstateId, TestData.ContractId,
                TestData.ProductId, TestData.FloatCreatedDateTime);
            Result result = await this.FloatDomainService.CreateFloatForContractProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_PurchaseRecorded(){
            FloatAggregate floatAggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            floatAggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            this.AggregateService.Setup(f => f.GetLatest<FloatAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Setup(f => f.Save<FloatAggregate>(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

            FloatCommands.RecordCreditPurchaseForFloatCommand command = new FloatCommands.RecordCreditPurchaseForFloatCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice,
                TestData.CreditPurchasedDateTime);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_SaveFailed()
        {
            FloatAggregate floatAggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            floatAggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            this.AggregateService.Setup(f => f.GetLatest<FloatAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Setup(f => f.Save<FloatAggregate>(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

            FloatCommands.RecordCreditPurchaseForFloatCommand command = new FloatCommands.RecordCreditPurchaseForFloatCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice,
                TestData.CreditPurchasedDateTime);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_ExceptionThrown()
        {
            FloatAggregate floatAggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            floatAggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            this.AggregateService.Setup(f => f.GetLatest<FloatAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Setup(f => f.Save<FloatAggregate>(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            FloatCommands.RecordCreditPurchaseForFloatCommand command = new FloatCommands.RecordCreditPurchaseForFloatCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice,
                TestData.CreditPurchasedDateTime);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_FloatActivity_PurchaseRecorded()
        {
            FloatActivityAggregate floatAggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
            this.AggregateService.Setup(f => f.GetLatest<FloatActivityAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Setup(f => f.Save<FloatActivityAggregate>(It.IsAny<FloatActivityAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

            FloatActivityCommands.RecordCreditPurchaseCommand command = new FloatActivityCommands.RecordCreditPurchaseCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_FloatActivity_SaveFailed()
        {
            FloatActivityAggregate floatAggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
            this.AggregateService.Setup(f => f.GetLatest<FloatActivityAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Setup(f => f.Save<FloatActivityAggregate>(It.IsAny<FloatActivityAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

            FloatActivityCommands.RecordCreditPurchaseCommand command = new FloatActivityCommands.RecordCreditPurchaseCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_FloatActivity_ExceptionThrown()
        {
            FloatActivityAggregate floatAggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
            this.AggregateService.Setup(f => f.GetLatest<FloatActivityAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Setup(f => f.Save<FloatActivityAggregate>(It.IsAny<FloatActivityAggregate>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            FloatActivityCommands.RecordCreditPurchaseCommand command = new FloatActivityCommands.RecordCreditPurchaseCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }
    }
}
