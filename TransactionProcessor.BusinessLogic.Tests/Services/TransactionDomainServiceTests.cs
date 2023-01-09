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
    using EstateManagement.DataTransferObjects.Requests;
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
        private Mock<IEstateClient> estateClient = null;

        private Mock<ISecurityServiceClient> securityServiceClient = null;

        private Mock<IOperatorProxy> operatorProxy = null;

        private TransactionDomainService transactionDomainService = null;

        private Mock<IProjectionStateRepository<MerchantBalanceState>> stateRepository;

        private Mock<IAggregateRepository<ReconciliationAggregate, DomainEvent>> reconciliationAggregateRepository = null;
        private Mock<IAggregateRepository<TransactionAggregate, DomainEvent>> transactionAggregateRepository = null;

        public TransactionDomainServiceTests() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            transactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEvent>>();
            estateClient = new Mock<IEstateClient>();
            securityServiceClient = new Mock<ISecurityServiceClient>();
            operatorProxy = new Mock<IOperatorProxy>();
            reconciliationAggregateRepository = new Mock<IAggregateRepository<ReconciliationAggregate, DomainEvent>>();
            Func<String, IOperatorProxy> operatorProxyResolver = (operatorName) => { return operatorProxy.Object; };
            stateRepository = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
            transactionDomainService = new TransactionDomainService(transactionAggregateRepository.Object,
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

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("0000");
            response.ResponseMessage.ShouldBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_ValidationFailed_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

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

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldNotBe("0000");
            response.ResponseMessage.ShouldNotBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier,
                                                                                                              CancellationToken.None);

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("0000");
            response.ResponseMessage.ShouldBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_LogonFailed_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Estate Not Found")));

            transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                                                                              TestData.EstateId,
                                                                                                              TestData.MerchantId,
                                                                                                              TestData.TransactionDateTime,
                                                                                                              TestData.TransactionNumber,
                                                                                                              TestData.DeviceIdentifier,
                                                                                                              CancellationToken.None);

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldNotBe("0000");
            response.ResponseMessage.ShouldNotBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }


        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);

            transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

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
                                                                                                            CancellationToken.None); ;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("0000");
            response.ResponseMessage.ShouldBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_EstateValidationFailed_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("",new KeyNotFoundException()));
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);

            transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

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
                                                                                                            CancellationToken.None); ;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldNotBe("0000");
            response.ResponseMessage.ShouldNotBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_OperatorProxyThrowsException_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);

            transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

            operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                          It.IsAny<Guid>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<MerchantResponse>(),
                                                          It.IsAny<DateTime>(),
                                                          It.IsAny<String>(),
                                                          It.IsAny<Dictionary<String, String>>(),
                                                          It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Operator Error"));

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
                                                                                                            CancellationToken.None); ;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("0000");
            response.ResponseMessage.ShouldBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_DeclinedByOperator_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);

            transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

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
                                                                                                           IsSuccessful = false,
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
                                                                                                            CancellationToken.None); ;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldNotBe("0000");
            response.ResponseMessage.ShouldNotBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        /*
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
        public async Task TransactionDomainService_ProcessLogonTransaction_EstateClientThrowsException_TransactionIsProcessed()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception"));
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
                .ReturnsAsync(TestData.MerchantBalanceProjectionStateNoCredit);

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
        */

        [Fact]
        public async Task TransactionDomainService_ResendTransactionReceipt_TransactionReceiptResendIsRequested()
        {
            this.transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionWithReceiptRequestedAggregate);

            Should.NotThrow(async () => {
                                await transactionDomainService.ResendTransactionReceipt(TestData.TransactionId, TestData.EstateId, CancellationToken.None);
                            });
        }

        #region New Tests
        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_SuccessfulLogon() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_MerchantDeviceListNull_SuccessfulLogon() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);
            // TODO: Verify device was added...

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.Success);
            this.estateClient.Verify(vf => vf.AddDeviceToMerchant(It.IsAny<String>(),
                                                                  It.IsAny<Guid>(),
                                                                  It.IsAny<Guid>(),
                                                                  It.IsAny<AddMerchantDeviceRequest>(),
                                                                  It.IsAny<CancellationToken>()),
                                     Times.Once);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_MerchantDeviceListEmpty_SuccessfulLogon() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.Success);
            this.estateClient.Verify(vf => vf.AddDeviceToMerchant(It.IsAny<String>(),
                                                                  It.IsAny<Guid>(),
                                                                  It.IsAny<Guid>(),
                                                                  It.IsAny<AddMerchantDeviceRequest>(),
                                                                  It.IsAny<CancellationToken>()),
                                     Times.Once);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_EstateNotFound_ResponseIsInvalidEstateId() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_EstateClientGetEstateThrewOtherException_ResponseIsUnknownFailure() {

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception"));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_MerchantNotFound_ResponseIsInvalidMerchantId() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_EstateClientGetMerchantThrewOtherException_ResponseIsUnknownFailure() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception"));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier1, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_SuccessfulReconciliation() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_EstateNotFound_ResponseIsInvalidEstateId() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_EstateClientGetEstateThrewOtherException_ResponseIsUnknownFailure() {

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception"));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_MerchantNotFound_ResponseIsInvalidMerchantId() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_EstateClientGetMerchantThrewOtherException_ResponseIsUnknownFailure() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception"));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier1,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_MerchantDeviceListNull_ResponseIsResponseIsNoValidDevices() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_MerchantDeviceListEmpty_ResponseIsResponseIsNoValidDevices() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_SuccessfulSale() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.MerchantContractResponses);

            this.stateRepository.Setup(s => s.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);
            
            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateNotFound_ResponseIsInvalidEstateId() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateFoundButHasNullOperators_ResponseIsInvalidEstateId()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithNullOperators);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.NoEstateOperators);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateFoundButHasNoOperators_ResponseIsInvalidEstateId()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithEmptyOperators);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.NoEstateOperators);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateFoundOperatorsNotConfiguredForEstate_ResponseIsOperatorNotValidForEstate()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier2,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.OperatorNotValidForEstate);
        }


        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateClientGetEstateThrewOtherException_ResponseIsUnknownFailure() {

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception"));

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantNotFound_ResponseIsInvalidMerchantId() {

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateClientGetMerchantThrewOtherException_ResponseIsUnknownFailure() {

            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception"));

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier1,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantDeviceListNull_ResponseIsNoValidDevices() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier1,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantDeviceListEmpty_ResponseIsNoValidDevices() {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier1,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantOperatorListNull_ResponseIsNoMerchantOperators()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithNullOperators);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.NoMerchantOperators);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantOperatorListEmpty_ResponseIsNoMerchantOperators()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithEmptyOperators);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.NoMerchantOperators);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_OperatorNotConfiguredFroMerchant_ResponseIsOperatorNotValidForMerchant()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator2);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.OperatorNotValidForMerchant);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantNotFoundOnGetContract_ResponseIsInvalidMerchantId()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateClientThrownOtherExceptionFoundOnGetContract_ResponseIsUnknownFailure()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception"));

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantHasNullContracts_ResponseIsMerchantDoesNotHaveEnoughCredit()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            List<ContractResponse> contracts = null;
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(contracts);

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.MerchantHasNoContractsConfigured);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantHasNoContracts_ResponseIsMerchantDoesNotHaveEnoughCredit()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ContractResponse>());

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.MerchantHasNoContractsConfigured);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantDoesNotHaveSuppliedContract_ResponseIsContractNotValidForMerchant()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantContractResponses);

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId1,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.ContractNotValidForMerchant);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_InvalidProductId_ResponseIsInvalidProductIdValue()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantContractResponses);

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                Guid.Empty,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidProductIdValue);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_ProductIdNotConfigured_ResponseIsProductNotValidForMerchant()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantContractResponses);

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId1,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.ProductNotValidForMerchant);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantNotEnoughCredit_ResponseIsMerchantDoesNotHaveEnoughCredit()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            
            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionStateNoCredit);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.MerchantDoesNotHaveEnoughCredit);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task TransactionDomainService_ValidateSaleTransaction_InvalidTransactionAmount_ResponseIsInvalidSaleTransactionAmount(Decimal transactionAmount)
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                TestData.ContractId,
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                transactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidSaleTransactionAmount);
        }


        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_InvalidContractId_ResponseIsInvalidContractIdValue()
        {
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            (String responseMessage, TransactionResponseCode responseCode) response = await this.transactionDomainService.ValidateSaleTransaction(TestData.EstateId,
                TestData.MerchantId,
                Guid.Empty, 
                TestData.ProductId,
                TestData.DeviceIdentifier,
                TestData.OperatorIdentifier1,
                TestData.TransactionAmount,
                CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidContractIdValue);
        }
    }
    #endregion
}
