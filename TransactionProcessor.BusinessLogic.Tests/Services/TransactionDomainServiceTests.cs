using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.Services;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Responses;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Moq;
    using OperatorInterfaces;
    using ReconciliationAggregate;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
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
        public async Task TransactionDomainService_ProcessReconciliationTransaction_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();
            reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(new ReconciliationAggregate());

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver,
                                             reconciliationAggregateRepository.Object);

            ProcessReconciliationTransactionResponse response = await transactionDomainService.ProcessReconciliationTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.DeviceIdentifier,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.ReconciliationTransactionCount,
                                                                                                              TestData.ReconciliationTransactionValue,
                                                                                                              CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_IncorrectDevice_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();
            reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(new ReconciliationAggregate());

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessReconciliationTransactionResponse response = await transactionDomainService.ProcessReconciliationTransaction(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier1,
                TestData.TransactionDateTime,
                TestData.ReconciliationTransactionCount,
                TestData.ReconciliationTransactionValue,
                CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidDeviceIdentifier);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_MerchantHasNoDevices_TransactionIsProcessed(Boolean deviceListIsNull)
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            MerchantResponse merchantResponse = deviceListIsNull ? TestData.GetMerchantResponseWithNullDevices : TestData.GetMerchantResponseWithNoDevices;
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(merchantResponse);

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();
            reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(new ReconciliationAggregate());

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessReconciliationTransactionResponse response = await transactionDomainService.ProcessReconciliationTransaction(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier1,
                TestData.TransactionDateTime,
                TestData.ReconciliationTransactionCount,
                TestData.ReconciliationTransactionValue,
                CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoValidDevices);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_InvalidEstate_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();
            reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(new ReconciliationAggregate());

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessReconciliationTransactionResponse response = await transactionDomainService.ProcessReconciliationTransaction(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionDateTime,
                TestData.ReconciliationTransactionCount,
                TestData.ReconciliationTransactionValue,
                CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidEstateId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_InvalidMerchant_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();
            reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(new ReconciliationAggregate());

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessReconciliationTransactionResponse response = await transactionDomainService.ProcessReconciliationTransaction(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionDateTime,
                TestData.ReconciliationTransactionCount,
                TestData.ReconciliationTransactionValue,
                CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidMerchantId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedLogonTransactionAggregate);

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String,IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver,
                                             reconciliationAggregateRepository.Object);
            
            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier,
                                                                                                              CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_MerchantWithNullDevices_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedLogonTransactionAggregate);

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier,
                                                                                                              CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_MerchantWithNoDevices_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedLogonTransactionAggregate);

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier,
                                                                                                              CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_IncorrectDevice_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidDeviceIdentifier));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier1,
                                                                                                              CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidDeviceIdentifier);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_InvalidEstate_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidEstateId));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier1,
                                                                                                              CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidEstateId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_InvalidMerchant_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEmptyMerchantResponse);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidMerchantId));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();
            
            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier1,
                                                                                                              CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidMerchantId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_SuccesfulOperatorResponse_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                          It.IsAny<Guid>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<MerchantResponse>(),
                                                          It.IsAny<DateTime>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<Dictionary<String, String>>(),
                                                          It.IsAny<CancellationToken>())).ReturnsAsync(new OperatorResponse
                                                          {
                                                              ResponseMessage = TestData.OperatorResponseMessage,
                                                              IsSuccessful = true,
                                                              AuthorisationCode = TestData.OperatorAuthorisationCode,
                                                              TransactionId = TestData.OperatorTransactionId,
                                                              ResponseCode = TestData.ResponseCode
                                                          });
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();
            
            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_InvalidTransactionAmountResponse_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidSaleTransactionAmount));

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();
            

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(amount:"0.00"),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidSaleTransactionAmount);
        }

        [Theory]
        [InlineData("amount")]
        [InlineData("Amount")]
        [InlineData("AMOUNT")]
        public async Task TransactionDomainService_ProcessSaleTransaction_MetaDataCaseTests_Amount_TransactionIsProcessed(String amountFieldName)
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                          It.IsAny<Guid>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<MerchantResponse>(),
                                                          It.IsAny<DateTime>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<Dictionary<String, String>>(),
                                                          It.IsAny<CancellationToken>())).ReturnsAsync(new OperatorResponse
                                                          {
                                                              ResponseMessage = TestData.OperatorResponseMessage,
                                                              IsSuccessful = true,
                                                              AuthorisationCode = TestData.OperatorAuthorisationCode,
                                                              TransactionId = TestData.OperatorTransactionId,
                                                              ResponseCode = TestData.ResponseCode
                                                          });
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);


            
            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(amountName:amountFieldName),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.Success);
        }

        [Theory]
        [InlineData("customerAccountNumber")]
        [InlineData("CustomerAccountNumber")]
        [InlineData("CUSTOMERACCOUNTNUMBER")]
        public async Task TransactionDomainService_ProcessSaleTransaction_MetaDataCaseTests_CustomerAccountNumber_TransactionIsProcessed(String customerAccountNumberFieldName)
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                          It.IsAny<Guid>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<MerchantResponse>(),
                                                          It.IsAny<DateTime>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<Dictionary<String, String>>(),
                                                          It.IsAny<CancellationToken>())).ReturnsAsync(new OperatorResponse
                                                          {
                                                              ResponseMessage = TestData.OperatorResponseMessage,
                                                              IsSuccessful = true,
                                                              AuthorisationCode = TestData.OperatorAuthorisationCode,
                                                              TransactionId = TestData.OperatorTransactionId,
                                                              ResponseCode = TestData.ResponseCode
                                                          });
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);



            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(customerAccountNumberName: customerAccountNumberFieldName),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_FailedOperatorResponse_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.TransactionDeclinedByOperator));

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                          It.IsAny<Guid>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<MerchantResponse>(),
                                                          It.IsAny<DateTime>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<Dictionary<String, String>>(),
                                                          It.IsAny<CancellationToken>())).ReturnsAsync(new OperatorResponse
                                                          {
                                                              ResponseMessage = TestData.DeclinedOperatorResponseMessage,
                                                              IsSuccessful = false,
                                                              ResponseCode = TestData.DeclinedOperatorResponseCode
                                                          });
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.TransactionDeclinedByOperator);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_MerchantWithNullDevices_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.NoValidDevices));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoValidDevices);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_MerchantWithNoDevices_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.NoValidDevices));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoValidDevices);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_IncorrectDevice_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidDeviceIdentifier));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier1,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidDeviceIdentifier);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_InvalidEstate_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidEstateId));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier1,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidEstateId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_NotEnoughCredit_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.MerchantDoesNotHaveEnoughCredit));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver,
                                             reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.MerchantDoesNotHaveEnoughCredit);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_InvalidMerchant_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidMerchantId));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();
            
            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier1,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidMerchantId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_EstateWithEmptyOperators_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithEmptyOperators);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.NoEstateOperators));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();
            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoEstateOperators);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_EstateWithNullOperators_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithNullOperators);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.NoEstateOperators));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver,reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoEstateOperators);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_OperatorNotSupportedByEstate_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.OperatorNotValidForEstate));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier2,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.OperatorNotValidForEstate);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_MerchantWithEmptyOperators_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithEmptyOperators);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.NoMerchantOperators));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver,
                                             reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoMerchantOperators);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_MerchantWithNullOperators_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNullOperators);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.NoMerchantOperators));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver,reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoMerchantOperators);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_OperatorNotSupportedByMerchant_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator2);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.OperatorNotValidForMerchant));

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();;

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.OperatorNotValidForMerchant);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_ErrorInOperatorComms_TransactionIsProcessed()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.OperatorCommsError));

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                          It.IsAny<Guid>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<MerchantResponse>(),
                                                          It.IsAny<DateTime>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<Dictionary<String, String>>(),
                                                          It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Comms Error"));
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>> reconciliationAggregateRepository =
                new Mock<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver, reconciliationAggregateRepository.Object);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.OperatorCommsError);
        }

        private void ValidateResponse(ProcessLogonTransactionResponse response,
                                           TransactionResponseCode transactionResponseCode)
        {
            response.ShouldNotBeNull();
            response.ResponseCode.ShouldBe(TestData.GetResponseCodeAsString(transactionResponseCode));

            String messageToValidate = TestData.GetResponseCodeMessage(transactionResponseCode);
            if (transactionResponseCode == TransactionResponseCode.Success)
            {
                messageToValidate = messageToValidate.ToUpper();
            }

            response.ResponseMessage.ShouldBe(messageToValidate);
        }

        private void ValidateResponse(ProcessSaleTransactionResponse response,
                                      TransactionResponseCode transactionResponseCode)
        {
            response.ShouldNotBeNull();
            response.ResponseCode.ShouldBe(TestData.GetResponseCodeAsString(transactionResponseCode));

            String messageToValidate = TestData.GetResponseCodeMessage(transactionResponseCode);
            if (transactionResponseCode == TransactionResponseCode.Success)
            {
                messageToValidate = messageToValidate.ToUpper();
            }

            response.ResponseMessage.ShouldBe(messageToValidate);
        }

        private void ValidateResponse(ProcessReconciliationTransactionResponse response,
                                      TransactionResponseCode transactionResponseCode)
        {
            response.ShouldNotBeNull();
            response.ResponseCode.ShouldBe(TestData.GetResponseCodeAsString(transactionResponseCode));

        }
    }
}
