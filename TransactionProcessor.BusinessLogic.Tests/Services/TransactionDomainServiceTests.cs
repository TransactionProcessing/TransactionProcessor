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
    using ProjectionEngine.Repository;
    using ProjectionEngine.State;
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
        private Mock<ITransactionAggregateManager> transactionAggregateManager = null;

        private Mock<IEstateClient> estateClient = null;

        private Mock<ISecurityServiceClient> securityServiceClient = null;

        private Mock<IOperatorProxy> operatorProxy = null;

        private TransactionDomainService transactionDomainService = null;

        private Mock<IProjectionStateRepository<MerchantBalanceState>> stateRepository;

        private Mock<IAggregateRepository<ReconciliationAggregate, DomainEvent>> reconciliationAggregateRepository = null;

        public TransactionDomainServiceTests() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            transactionAggregateManager = new Mock<ITransactionAggregateManager>();
            estateClient = new Mock<IEstateClient>();
            securityServiceClient = new Mock<ISecurityServiceClient>();
            operatorProxy = new Mock<IOperatorProxy>();
            reconciliationAggregateRepository = new Mock<IAggregateRepository<ReconciliationAggregate, DomainEvent>>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };
            stateRepository = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
            transactionDomainService = new TransactionDomainService(transactionAggregateManager.Object,
                                                                    estateClient.Object,
                                                                    securityServiceClient.Object,
                                                                    operatorProxyResolver,
                                                                    reconciliationAggregateRepository.Object,
                                                                    stateRepository.Object);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(new ReconciliationAggregate());
            
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
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(new ReconciliationAggregate());
            
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
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            MerchantResponse merchantResponse = deviceListIsNull ? TestData.GetMerchantResponseWithNullDevices : TestData.GetMerchantResponseWithNoDevices;
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(merchantResponse);

            reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(new ReconciliationAggregate());

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
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(new ReconciliationAggregate());

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
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));
            
            reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                             .ReturnsAsync(new ReconciliationAggregate());

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
           securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
           
           estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedLogonTransactionAggregate);
            
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
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedLogonTransactionAggregate);
            
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
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedLogonTransactionAggregate);

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
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidDeviceIdentifier));

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
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidEstateId));

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
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEmptyMerchantResponse);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidMerchantId));

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
        public async Task TransactionDomainService_ProcessSaleTransaction_SuccessfulOperatorResponse_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);

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

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_InvalidTransactionAmountResponse_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidSaleTransactionAmount));
            
            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(amount:"0.00"),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidSaleTransactionAmount);
        }

        [Theory]
        [InlineData("amount")]
        [InlineData("Amount")]
        [InlineData("AMOUNT")]
        public async Task TransactionDomainService_ProcessSaleTransaction_MetaDataCaseTests_Amount_TransactionIsProcessed(String amountFieldName)
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);

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

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(amountName:amountFieldName),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.Success);
        }

        [Theory]
        [InlineData("customerAccountNumber")]
        [InlineData("CustomerAccountNumber")]
        [InlineData("CUSTOMERACCOUNTNUMBER")]
        public async Task TransactionDomainService_ProcessSaleTransaction_MetaDataCaseTests_CustomerAccountNumber_TransactionIsProcessed(String customerAccountNumberFieldName)
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);

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

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(customerAccountNumberName: customerAccountNumberFieldName),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_FailedOperatorResponse_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.TransactionDeclinedByOperator));

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

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.TransactionDeclinedByOperator);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_MerchantWithNullDevices_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.NoValidDevices));

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoValidDevices);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_MerchantWithNoDevices_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.NoValidDevices));

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoValidDevices);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_IncorrectDevice_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidDeviceIdentifier));

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier1,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidDeviceIdentifier);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_InvalidEstate_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidEstateId));

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier1,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidEstateId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_NotEnoughCredit_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.MerchantDoesNotHaveEnoughCredit));

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.MerchantDoesNotHaveEnoughCredit);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_InvalidMerchant_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.InvalidMerchantId));

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier1,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.InvalidMerchantId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_EstateWithEmptyOperators_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithEmptyOperators);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.NoEstateOperators));

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoEstateOperators);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_EstateWithNullOperators_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithNullOperators);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.NoEstateOperators));

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoEstateOperators);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_OperatorNotSupportedByEstate_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.OperatorNotValidForEstate));

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier2,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.OperatorNotValidForEstate);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_MerchantWithEmptyOperators_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithEmptyOperators);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.NoMerchantOperators));

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoMerchantOperators);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", TransactionResponseCode.InvalidContractIdValue)]
        [InlineData("DB641DAF-B0C2-4CA5-B141-13882F3ACEFA", TransactionResponseCode.ContractNotValidForMerchant)]
        public async Task TransactionDomainService_ProcessSaleTransaction_ContractId_TransactionIsProcessed(String contractId, TransactionResponseCode expectedResponseCode)
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(expectedResponseCode));

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            Guid.Parse(contractId),
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, expectedResponseCode);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", TransactionResponseCode.InvalidProductIdValue)]
        [InlineData("DB641DAF-B0C2-4CA5-B141-13882F3ACEFA", TransactionResponseCode.ProductNotValidForMerchant)]
        public async Task TransactionDomainService_ProcessSaleTransaction_ProductId_TransactionIsProcessed(String productId,
                                                                                                           TransactionResponseCode expectedResponseCode) {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(expectedResponseCode));

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            Guid.Parse(productId),
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, expectedResponseCode);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_MerchantWithNullOperators_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNullOperators);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.NoMerchantOperators));

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.NoMerchantOperators);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_OperatorNotSupportedByMerchant_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator2);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetDeclinedTransactionAggregate(TransactionResponseCode.OperatorNotValidForMerchant));

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.OperatorNotValidForMerchant);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_ErrorInOperatorComms_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);
            
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetLocallyDeclinedTransactionAggregate(TransactionResponseCode.OperatorCommsError));

            operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                          It.IsAny<Guid>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<MerchantResponse>(),
                                                          It.IsAny<DateTime>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<Dictionary<String, String>>(),
                                                          It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Comms Error"));

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.TransactionNumber,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.OperatorIdentifier1,
                                                                                                            TestData.CustomerEmailAddress,
                                                                                                            TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                            TestData.ContractId,
                                                                                                            TestData.ProductId,
                                                                                                            TestData.TransactionSource,
                                                                                                            CancellationToken.None);

            this.ValidateResponse(response, TransactionResponseCode.OperatorCommsError);
        }

        [Fact]
        public async Task TransactionDomainService_ResendTransactionReceipt_TransactionReceiptResendIsRequested()
        {
            transactionAggregateManager.Setup(t => t.GetAggregate(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);
            
            Should.NotThrow(async () => {
                                await transactionDomainService.ResendTransactionReceipt(TestData.TransactionId, TestData.EstateId, CancellationToken.None);
                            });
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
