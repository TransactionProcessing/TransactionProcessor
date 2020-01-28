using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Services;
    using EstateManagement.Client;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Moq;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.DomainDrivenDesign.EventStore;
    using Shared.EventStore.EventStore;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using TransactionAggregate;
    using Xunit;

    public class TransactionDomainServiceTests
    {
        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IAggregateRepositoryManager> aggregateRepositoryManager = new Mock<IAggregateRepositoryManager>();
            Mock<IAggregateRepository<TransactionAggregate>> transactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate>>();

            aggregateRepositoryManager.Setup(x => x.GetAggregateRepository<TransactionAggregate>(It.IsAny<Guid>())).Returns(transactionAggregateRepository.Object);
            transactionAggregateRepository.SetupSequence(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate)
                                          .ReturnsAsync(TestData.GetStartedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetLocallyAuthorisedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetCompletedTransactionAggregate);
            transactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponse);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponse);

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(aggregateRepositoryManager.Object, estateClient.Object, securityServiceClient.Object);
            
            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier,
                                                                                                              CancellationToken.None);

            response.ShouldNotBeNull();
            response.ResponseCode.ShouldBe(TestData.ResponseCode);
            response.ResponseMessage.ShouldBe(TestData.ResponseMessage);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_MerchantWithNullDevices_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IAggregateRepositoryManager> aggregateRepositoryManager = new Mock<IAggregateRepositoryManager>();
            Mock<IAggregateRepository<TransactionAggregate>> transactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate>>();

            aggregateRepositoryManager.Setup(x => x.GetAggregateRepository<TransactionAggregate>(It.IsAny<Guid>())).Returns(transactionAggregateRepository.Object);
            transactionAggregateRepository.SetupSequence(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate)
                                          .ReturnsAsync(TestData.GetStartedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetLocallyAuthorisedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetCompletedTransactionAggregate);
            transactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponse);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(aggregateRepositoryManager.Object, estateClient.Object, securityServiceClient.Object);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier,
                                                                                                              CancellationToken.None);

            response.ShouldNotBeNull();
            response.ResponseCode.ShouldBe(TestData.ResponseCode);
            response.ResponseMessage.ShouldBe(TestData.ResponseMessage);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_MerchantWithNoDevices_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IAggregateRepositoryManager> aggregateRepositoryManager = new Mock<IAggregateRepositoryManager>();
            Mock<IAggregateRepository<TransactionAggregate>> transactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate>>();

            aggregateRepositoryManager.Setup(x => x.GetAggregateRepository<TransactionAggregate>(It.IsAny<Guid>())).Returns(transactionAggregateRepository.Object);
            transactionAggregateRepository.SetupSequence(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate)
                                          .ReturnsAsync(TestData.GetStartedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetLocallyAuthorisedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetCompletedTransactionAggregate);
            transactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponse);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(aggregateRepositoryManager.Object, estateClient.Object, securityServiceClient.Object);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier,
                                                                                                              CancellationToken.None);

            response.ShouldNotBeNull();
            response.ResponseCode.ShouldBe(TestData.ResponseCode);
            response.ResponseMessage.ShouldBe(TestData.ResponseMessage);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_IncorrectDevice_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IAggregateRepositoryManager> aggregateRepositoryManager = new Mock<IAggregateRepositoryManager>();
            Mock<IAggregateRepository<TransactionAggregate>> transactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate>>();

            aggregateRepositoryManager.Setup(x => x.GetAggregateRepository<TransactionAggregate>(It.IsAny<Guid>())).Returns(transactionAggregateRepository.Object);
            transactionAggregateRepository.SetupSequence(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate)
                                          .ReturnsAsync(TestData.GetStartedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetLocallyAuthorisedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetCompletedTransactionAggregate);
            transactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponse);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponse);

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(aggregateRepositoryManager.Object, estateClient.Object, securityServiceClient.Object);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier1,
                                                                                                              CancellationToken.None);

            response.ShouldNotBeNull();
            response.ResponseCode.ShouldBe(TestData.ResponseCode);
            response.ResponseMessage.ShouldBe(TestData.ResponseMessage);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_InvlaidEstate_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IAggregateRepositoryManager> aggregateRepositoryManager = new Mock<IAggregateRepositoryManager>();
            Mock<IAggregateRepository<TransactionAggregate>> transactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate>>();

            aggregateRepositoryManager.Setup(x => x.GetAggregateRepository<TransactionAggregate>(It.IsAny<Guid>())).Returns(transactionAggregateRepository.Object);
            transactionAggregateRepository.SetupSequence(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate)
                                          .ReturnsAsync(TestData.GetStartedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetLocallyAuthorisedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetCompletedTransactionAggregate);
            transactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEmptyEstateResponse);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponse);

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(aggregateRepositoryManager.Object, estateClient.Object, securityServiceClient.Object);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier1,
                                                                                                              CancellationToken.None);

            response.ShouldNotBeNull();
            response.ResponseCode.ShouldBe(TestData.ResponseCode);
            response.ResponseMessage.ShouldBe(TestData.ResponseMessage);
        }

    }
}
