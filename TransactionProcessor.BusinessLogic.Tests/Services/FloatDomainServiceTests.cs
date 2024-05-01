using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    using TransactionProcessor.BusinessLogic.Services;
    using Xunit;

    public class FloatDomainServiceTests
    {
        private readonly Mock<IEstateClient> EstateClient;
        private readonly Mock<ISecurityServiceClient> SecurityServiceClient;
        private readonly Mock<IAggregateRepository<FloatAggregate, DomainEvent>> FloatAggregateRepository;

        private readonly FloatDomainService FloatDomainService;

        public FloatDomainServiceTests(){

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.EstateClient = new Mock<IEstateClient>();
            this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
            this.FloatAggregateRepository = new Mock<IAggregateRepository<FloatAggregate, DomainEvent>>();
            this.FloatDomainService = new FloatDomainService(this.FloatAggregateRepository.Object,
                                                             this.EstateClient.Object,
                                                             this.SecurityServiceClient.Object);
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_FloatCreated(){

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FloatAggregate());

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

            CreateFloatForContractProductResponse response = await this.FloatDomainService.CreateFloatForContractProduct(TestData.EstateId,
                                                                                                                  TestData.ContractId,
                                                                                                                  TestData.ProductId,
                                                                                                                  TestData.FloatCreatedDateTime,
                                                                                                                  CancellationToken.None);
            response.FloatId.ShouldNotBe(Guid.Empty);
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidEstate_ErrorThrown()
        {

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FloatAggregate());

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

            Should.Throw<InvalidOperationException>(async () => {
                                                        await this.FloatDomainService.CreateFloatForContractProduct(TestData.EstateId,
                                                                                                                                                                     TestData.ContractId,
                                                                                                                                                                     TestData.ProductId,
                                                                                                                                                                     TestData.FloatCreatedDateTime,
                                                                                                                                                                     CancellationToken.None);
                                                    });
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidContract_ErrorThrown()
        {

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new FloatAggregate());
            
            Should.Throw<InvalidOperationException>(async () => {
                await this.FloatDomainService.CreateFloatForContractProduct(TestData.EstateId,
                                                                                                                             TestData.ContractId,
                                                                                                                             TestData.ProductId,
                                                                                                                             TestData.FloatCreatedDateTime,
                                                                                                                             CancellationToken.None);
            });
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidContractProduct_ErrorThrown()
        {

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

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

            Should.Throw<InvalidOperationException>(async () => {
                                                        await this.FloatDomainService.CreateFloatForContractProduct(TestData.EstateId,
                                                                                                                    TestData.ContractId,
                                                                                                                    TestData.ProductId,
                                                                                                                    TestData.FloatCreatedDateTime,
                                                                                                                    CancellationToken.None);
                                                    });
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_PurchaseRecorded(){
            FloatAggregate floatAggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            floatAggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(floatAggregate);

            await this.FloatDomainService.RecordCreditPurchase(TestData.EstateId,
                                                               TestData.FloatAggregateId,
                                                               TestData.FloatCreditAmount,
                                                               TestData.FloatCreditCostPrice,
                                                               TestData.CreditPurchasedDateTime,
                                                               CancellationToken.None);
        }
    }
}
