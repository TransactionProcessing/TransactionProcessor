using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System.Threading;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.DataTransferObjects.Responses.Contract;
    using FloatAggregate;
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
    using TransactionProcessor.BusinessLogic.Common;
    using TransactionProcessor.BusinessLogic.Services;
    using Xunit;

    public class FloatDomainServiceTests
    {
        private readonly Mock<IIntermediateEstateClient> EstateClient;
        private readonly Mock<ISecurityServiceClient> SecurityServiceClient;
        private readonly Mock<IAggregateRepository<FloatAggregate, DomainEvent>> FloatAggregateRepository;
        private readonly Mock<IAggregateRepository<FloatActivityAggregate, DomainEvent>> FloatActivityAggregateRepository;
        private readonly Mock<IAggregateRepository<TransactionAggregate.TransactionAggregate, DomainEvent>> TransactionAggregateRepository;

        private readonly FloatDomainService FloatDomainService;

        public FloatDomainServiceTests(){

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.EstateClient = new Mock<IIntermediateEstateClient>();
            this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
            this.FloatAggregateRepository = new Mock<IAggregateRepository<FloatAggregate, DomainEvent>>();
            this.FloatActivityAggregateRepository = new Mock<IAggregateRepository<FloatActivityAggregate, DomainEvent>>();
            this.TransactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate.TransactionAggregate, DomainEvent>>();
            this.FloatDomainService = new FloatDomainService(this.FloatAggregateRepository.Object,
                this.FloatActivityAggregateRepository.Object,
                this.TransactionAggregateRepository.Object,
                                                             this.EstateClient.Object,
                                                             this.SecurityServiceClient.Object);
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_FloatCreated(){

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

            this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FloatAggregate());
            this.FloatAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            this.EstateClient.Setup(e => e.GetContract(It.IsAny<String>(),
                                                       It.IsAny<Guid>(),
                                                       It.IsAny<Guid>(),
                                                       It.IsAny<CancellationToken>())).ReturnsAsync(new ContractResponse{
                                                                                                                            EstateId = TestData.EstateId,
                                                                                                                            ContractId = TestData.ContractId,
                                                                                                                            Products = new List<ContractProduct>{
                                                                                                                                                                    new ContractProduct{
                                                                                                                                                                                           ProductId = TestData.ProductId,
                                                                                                                                                                                       }
                                                                                                                                                                }
                                                                                                                        });

            var command = new FloatCommands.CreateFloatForContractProductCommand(TestData.EstateId, TestData.ContractId,
                TestData.ProductId, TestData.FloatCreatedDateTime);
            var result = await this.FloatDomainService.CreateFloatForContractProduct(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidEstate_ErrorThrown()
        {

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
            
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FloatAggregate());
            this.EstateClient
                .Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());
            this.EstateClient.Setup(e => e.GetContract(It.IsAny<String>(),
                                                       It.IsAny<Guid>(),
                                                       It.IsAny<Guid>(),
                                                       It.IsAny<CancellationToken>())).ReturnsAsync(new ContractResponse
                                                       {
                                                           EstateId = TestData.EstateId,
                                                           ContractId = TestData.ContractId,
                                                           Products = new List<ContractProduct>{
                                                                                                                                                                    new ContractProduct{
                                                                                                                                                                                           ProductId = TestData.ProductId,
                                                                                                                                                                                       }
                                                                                                                                                                }
                                                       });

            var command = new FloatCommands.CreateFloatForContractProductCommand(TestData.EstateId, TestData.ContractId,
                TestData.ProductId, TestData.FloatCreatedDateTime);
            var result = await this.FloatDomainService.CreateFloatForContractProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidContract_ErrorThrown()
        {

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

            this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.EstateClient.Setup(e => e.GetContract(It.IsAny<String>(),
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FloatAggregate());

            var command = new FloatCommands.CreateFloatForContractProductCommand(TestData.EstateId, TestData.ContractId,
                TestData.ProductId, TestData.FloatCreatedDateTime);
            var result = await this.FloatDomainService.CreateFloatForContractProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidContractProduct_ErrorThrown()
        {

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

            this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.EstateClient.Setup(e => e.GetContract(It.IsAny<String>(),
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new ContractResponse()));
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FloatAggregate());

            this.EstateClient.Setup(e => e.GetContract(It.IsAny<String>(),
                                                       It.IsAny<Guid>(),
                                                       It.IsAny<Guid>(),
                                                       It.IsAny<CancellationToken>())).ReturnsAsync(new ContractResponse
                                                       {
                                                           EstateId = TestData.EstateId,
                                                           ContractId = TestData.ContractId,
                                                           Products = new List<ContractProduct>()
                                                       });

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
