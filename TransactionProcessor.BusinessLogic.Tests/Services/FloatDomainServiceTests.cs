using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Database.Entities;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System.Threading;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.DataTransferObjects.Responses.Contract;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Moq;
    using SecurityService.Client;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using TransactionProcessor.Aggregates;
    using TransactionProcessor.BusinessLogic.Common;
    using TransactionProcessor.BusinessLogic.Services;
    using Xunit;

    public class FloatDomainServiceTests
    {
        private readonly Mock<IAggregateRepository<FloatAggregate, DomainEvent>> FloatAggregateRepository;
        private readonly Mock<IAggregateRepository<FloatActivityAggregate, DomainEvent>> FloatActivityAggregateRepository;
        private readonly Mock<IAggregateRepository<TransactionAggregate, DomainEvent>> TransactionAggregateRepository;
        private readonly Mock<IAggregateRepository<EstateAggregate, DomainEvent>> EstateAggregateRepository;
        private readonly Mock<IAggregateRepository<ContractAggregate,DomainEvent>> ContractAggregateRepository;
        private readonly FloatDomainService FloatDomainService;

        public FloatDomainServiceTests(){

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.FloatAggregateRepository = new Mock<IAggregateRepository<FloatAggregate, DomainEvent>>();
            this.FloatActivityAggregateRepository = new Mock<IAggregateRepository<FloatActivityAggregate, DomainEvent>>();
            this.TransactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEvent>>();
            this.EstateAggregateRepository = new Mock<IAggregateRepository<EstateAggregate, DomainEvent>>();
            this.ContractAggregateRepository = new Mock<IAggregateRepository<ContractAggregate, DomainEvent>>();

            this.FloatDomainService = new FloatDomainService(this.FloatAggregateRepository.Object,
                this.FloatActivityAggregateRepository.Object,
                this.TransactionAggregateRepository.Object,
                this.EstateAggregateRepository.Object,
                this.ContractAggregateRepository.Object);
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_FloatCreated(){

            this.EstateAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync( Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FloatAggregate());
            this.FloatAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            this.ContractAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(Models.Contract.CalculationType.Fixed,Models.Contract.FeeType.Merchant));
            var command = new FloatCommands.CreateFloatForContractProductCommand(TestData.EstateId, TestData.ContractId,
                TestData.ProductId, TestData.FloatCreatedDateTime);
            var result = await this.FloatDomainService.CreateFloatForContractProduct(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidEstate_ErrorThrown()
        {
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FloatAggregate());
            this.EstateAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());
            
            var command = new FloatCommands.CreateFloatForContractProductCommand(TestData.EstateId, TestData.ContractId,
                TestData.ProductId, TestData.FloatCreatedDateTime);
            var result = await this.FloatDomainService.CreateFloatForContractProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidContract_ErrorThrown()
        {
            this.EstateAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FloatAggregate());
            this.ContractAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

            var command = new FloatCommands.CreateFloatForContractProductCommand(TestData.EstateId, TestData.ContractId,
                TestData.ProductId, TestData.FloatCreatedDateTime);
            var result = await this.FloatDomainService.CreateFloatForContractProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidContractProduct_ErrorThrown()
        {
            this.ContractAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.CreatedContractAggregate());

            this.EstateAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FloatAggregate());

            var command = new FloatCommands.CreateFloatForContractProductCommand(TestData.EstateId, TestData.ContractId,
                TestData.ProductId, TestData.FloatCreatedDateTime);
            var result = await this.FloatDomainService.CreateFloatForContractProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_PurchaseRecorded(){
            FloatAggregate floatAggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            floatAggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(floatAggregate);
            this.FloatAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            
            var command = new FloatCommands.RecordCreditPurchaseForFloatCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice,
                TestData.CreditPurchasedDateTime);
            var result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_SaveFailed()
        {
            FloatAggregate floatAggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            floatAggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(floatAggregate);
            this.FloatAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

            var command = new FloatCommands.RecordCreditPurchaseForFloatCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice,
                TestData.CreditPurchasedDateTime);
            var result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_ExceptionThrown()
        {
            FloatAggregate floatAggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            floatAggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(floatAggregate);
            this.FloatAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            var command = new FloatCommands.RecordCreditPurchaseForFloatCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice,
                TestData.CreditPurchasedDateTime);
            var result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_FloatActivity_PurchaseRecorded()
        {
            FloatActivityAggregate floatAggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(floatAggregate);
            this.FloatActivityAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FloatActivityAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

            var command = new FloatActivityCommands.RecordCreditPurchaseCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
            var result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_FloatActivity_SaveFailed()
        {
            FloatActivityAggregate floatAggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(floatAggregate);
            this.FloatActivityAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FloatActivityAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

            var command = new FloatActivityCommands.RecordCreditPurchaseCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
            var result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_FloatActivity_ExceptionThrown()
        {
            FloatActivityAggregate floatAggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(floatAggregate);
            this.FloatActivityAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FloatActivityAggregate>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            var command = new FloatActivityCommands.RecordCreditPurchaseCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
            var result = await this.FloatDomainService.RecordCreditPurchase(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }
    }
}
