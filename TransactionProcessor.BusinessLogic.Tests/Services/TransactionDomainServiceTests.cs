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

            Mock<ITransactionAggregateManager> transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedTransactionAggregate);

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            Func<String,IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);
            
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
                                       .ReturnsAsync(TestData.GetCompletedTransactionAggregate);

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

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
                                       .ReturnsAsync(TestData.GetCompletedTransactionAggregate);

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

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
                                       .ReturnsAsync(TestData.GetCompletedTransactionAggregate);

            Mock<IOperatorProxy> operatorProxy = new Mock<IOperatorProxy>();
            operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<Guid>(),
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
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
            operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<Guid>(),
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier1,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier1,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidEstateId);
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
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEmptyMerchantResponse);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidMerchantId));

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier1,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier2,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
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

            TransactionDomainService transactionDomainService =
                new TransactionDomainService(transactionAggregateManager.Object, estateClient.Object, securityServiceClient.Object,
                                             operatorProxyResolver);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaData,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.OperatorNotValidForMerchant);
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
    }
}
