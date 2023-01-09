namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System;
    using System.Collections.Generic;
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
    using ProjectionEngine.Repository;
    using ProjectionEngine.State;
    using ReconciliationAggregate;
    using SecurityService.Client;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using TransactionAggregate;
    using Xunit;

    public class TransactionDomainServiceTests
    {
        #region Fields

        private readonly Mock<IEstateClient> estateClient;

        private readonly Mock<IOperatorProxy> operatorProxy;

        private readonly Mock<IAggregateRepository<ReconciliationAggregate, DomainEvent>> reconciliationAggregateRepository;

        private readonly Mock<ISecurityServiceClient> securityServiceClient;

        private readonly Mock<IProjectionStateRepository<MerchantBalanceState>> stateRepository;

        private readonly Mock<IAggregateRepository<TransactionAggregate, DomainEvent>> transactionAggregateRepository;

        private readonly TransactionDomainService transactionDomainService;

        #endregion

        #region Constructors

        public TransactionDomainServiceTests() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.transactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEvent>>();
            this.estateClient = new Mock<IEstateClient>();
            this.securityServiceClient = new Mock<ISecurityServiceClient>();
            this.operatorProxy = new Mock<IOperatorProxy>();
            this.reconciliationAggregateRepository = new Mock<IAggregateRepository<ReconciliationAggregate, DomainEvent>>();
            Func<String, IOperatorProxy> operatorProxyResolver = operatorName => { return this.operatorProxy.Object; };
            this.stateRepository = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
            this.transactionDomainService = new TransactionDomainService(this.transactionAggregateRepository.Object,
                                                                         this.estateClient.Object,
                                                                         this.securityServiceClient.Object,
                                                                         operatorProxyResolver,
                                                                         this.reconciliationAggregateRepository.Object,
                                                                         this.stateRepository.Object);
        }

        #endregion

        #region Methods

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_LogonFailed_TransactionIsProcessed() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Estate Not Found")));

            this.transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessLogonTransactionResponse response = await this.transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
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
        public async Task TransactionDomainService_ProcessLogonTransaction_TransactionIsProcessed() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            this.transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessLogonTransactionResponse response = await this.transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
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
        public async Task TransactionDomainService_ProcessReconciliationTransaction_TransactionIsProcessed() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            this.reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReconciliationAggregate());

            ProcessReconciliationTransactionResponse response = await this.transactionDomainService.ProcessReconciliationTransaction(TestData.TransactionId,
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
        public async Task TransactionDomainService_ProcessReconciliationTransaction_ValidationFailed_TransactionIsProcessed() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

            this.reconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReconciliationAggregate());

            ProcessReconciliationTransactionResponse response = await this.transactionDomainService.ProcessReconciliationTransaction(TestData.TransactionId,
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
        public async Task TransactionDomainService_ProcessSaleTransaction_DeclinedByOperator_TransactionIsProcessed() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantContractResponses);

            this.transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

            this.operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<MerchantResponse>(),
                                                               It.IsAny<DateTime>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<Dictionary<String, String>>(),
                                                               It.IsAny<CancellationToken>())).ReturnsAsync(new OperatorResponse {
                                                                                                                ResponseMessage = TestData.OperatorResponseMessage,
                                                                                                                IsSuccessful = false,
                                                                                                                AuthorisationCode =
                                                                                                                    TestData.OperatorAuthorisationCode,
                                                                                                                TransactionId = TestData.OperatorTransactionId,
                                                                                                                ResponseCode = TestData.ResponseCode
                                                                                                            });

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await this.transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                                     TestData.EstateId,
                                                                                                                     TestData.MerchantId,
                                                                                                                     TestData.TransactionDateTime,
                                                                                                                     TestData.TransactionNumber,
                                                                                                                     TestData.DeviceIdentifier,
                                                                                                                     TestData.OperatorIdentifier1,
                                                                                                                     TestData.CustomerEmailAddress,
                                                                                                                     TestData
                                                                                                                         .AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                                     TestData.ContractId,
                                                                                                                     TestData.ProductId,
                                                                                                                     TestData.TransactionSource,
                                                                                                                     CancellationToken.None);
            
            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldNotBe("0000");
            response.ResponseMessage.ShouldNotBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_EstateValidationFailed_TransactionIsProcessed() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("", new KeyNotFoundException()));
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantContractResponses);

            this.transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

            this.operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<MerchantResponse>(),
                                                               It.IsAny<DateTime>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<Dictionary<String, String>>(),
                                                               It.IsAny<CancellationToken>())).ReturnsAsync(new OperatorResponse {
                                                                                                                ResponseMessage = TestData.OperatorResponseMessage,
                                                                                                                IsSuccessful = true,
                                                                                                                AuthorisationCode =
                                                                                                                    TestData.OperatorAuthorisationCode,
                                                                                                                TransactionId = TestData.OperatorTransactionId,
                                                                                                                ResponseCode = TestData.ResponseCode
                                                                                                            });

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await this.transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                                     TestData.EstateId,
                                                                                                                     TestData.MerchantId,
                                                                                                                     TestData.TransactionDateTime,
                                                                                                                     TestData.TransactionNumber,
                                                                                                                     TestData.DeviceIdentifier,
                                                                                                                     TestData.OperatorIdentifier1,
                                                                                                                     TestData.CustomerEmailAddress,
                                                                                                                     TestData
                                                                                                                         .AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                                     TestData.ContractId,
                                                                                                                     TestData.ProductId,
                                                                                                                     TestData.TransactionSource,
                                                                                                                     CancellationToken.None);
            
            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldNotBe("0000");
            response.ResponseMessage.ShouldNotBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_OperatorProxyThrowsException_TransactionIsProcessed() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantContractResponses);

            this.transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

            this.operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<MerchantResponse>(),
                                                               It.IsAny<DateTime>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<Dictionary<String, String>>(),
                                                               It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Operator Error"));

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await this.transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                                     TestData.EstateId,
                                                                                                                     TestData.MerchantId,
                                                                                                                     TestData.TransactionDateTime,
                                                                                                                     TestData.TransactionNumber,
                                                                                                                     TestData.DeviceIdentifier,
                                                                                                                     TestData.OperatorIdentifier1,
                                                                                                                     TestData.CustomerEmailAddress,
                                                                                                                     TestData
                                                                                                                         .AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                                     TestData.ContractId,
                                                                                                                     TestData.ProductId,
                                                                                                                     TestData.TransactionSource,
                                                                                                                     CancellationToken.None);
            
            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("0000");
            response.ResponseMessage.ShouldBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_TransactionIsProcessed() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantContractResponses);

            this.transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEmptyTransactionAggregate);

            this.operatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<MerchantResponse>(),
                                                               It.IsAny<DateTime>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<Dictionary<String, String>>(),
                                                               It.IsAny<CancellationToken>())).ReturnsAsync(new OperatorResponse {
                                                                                                                ResponseMessage = TestData.OperatorResponseMessage,
                                                                                                                IsSuccessful = true,
                                                                                                                AuthorisationCode =
                                                                                                                    TestData.OperatorAuthorisationCode,
                                                                                                                TransactionId = TestData.OperatorTransactionId,
                                                                                                                ResponseCode = TestData.ResponseCode
                                                                                                            });

            this.stateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.MerchantBalanceProjectionState);

            ProcessSaleTransactionResponse response = await this.transactionDomainService.ProcessSaleTransaction(TestData.TransactionId,
                                                                                                                     TestData.EstateId,
                                                                                                                     TestData.MerchantId,
                                                                                                                     TestData.TransactionDateTime,
                                                                                                                     TestData.TransactionNumber,
                                                                                                                     TestData.DeviceIdentifier,
                                                                                                                     TestData.OperatorIdentifier1,
                                                                                                                     TestData.CustomerEmailAddress,
                                                                                                                     TestData
                                                                                                                         .AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                                     TestData.ContractId,
                                                                                                                     TestData.ProductId,
                                                                                                                     TestData.TransactionSource,
                                                                                                                     CancellationToken.None);
            
            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("0000");
            response.ResponseMessage.ShouldBe("SUCCESS");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }
        
        [Fact]
        public async Task TransactionDomainService_ResendTransactionReceipt_TransactionReceiptResendIsRequested() {
            this.transactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionWithReceiptRequestedAggregate);

            Should.NotThrow(async () => {
                                await this.transactionDomainService.ResendTransactionReceipt(TestData.TransactionId, TestData.EstateId, CancellationToken.None);
                            });
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier1, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_EstateClientGetEstateThrewOtherException_ResponseIsUnknownFailure() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception"));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_EstateClientGetMerchantThrewOtherException_ResponseIsUnknownFailure() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Exception"));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_EstateNotFound_ResponseIsInvalidEstateId() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_MerchantDeviceListEmpty_SuccessfulLogon() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateLogonTransaction_MerchantDeviceListNull_SuccessfulLogon() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateLogonTransaction_MerchantNotFound_ResponseIsInvalidMerchantId() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateLogonTransaction_SuccessfulLogon() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier1,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_EstateClientGetEstateThrewOtherException_ResponseIsUnknownFailure() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception"));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_EstateClientGetMerchantThrewOtherException_ResponseIsUnknownFailure() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Exception"));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_EstateNotFound_ResponseIsInvalidEstateId() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_MerchantDeviceListEmpty_ResponseIsResponseIsNoValidDevices() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_MerchantDeviceListNull_ResponseIsResponseIsNoValidDevices() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_MerchantNotFound_ResponseIsInvalidMerchantId() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateReconciliationTransaction_SuccessfulReconciliation() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            (String responseMessage, TransactionResponseCode responseCode) response =
                await this.transactionDomainService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

            response.responseCode.ShouldBe(TransactionResponseCode.Success);
        }

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateClientGetEstateThrewOtherException_ResponseIsUnknownFailure() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception"));

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
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateClientGetMerchantThrewOtherException_ResponseIsUnknownFailure() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateClientThrownOtherExceptionFoundOnGetContract_ResponseIsUnknownFailure() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateFoundButHasNoOperators_ResponseIsInvalidEstateId() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateFoundButHasNullOperators_ResponseIsInvalidEstateId() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateFoundOperatorsNotConfiguredForEstate_ResponseIsOperatorNotValidForEstate() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_EstateNotFound_ResponseIsInvalidEstateId() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_InvalidContractId_ResponseIsInvalidContractIdValue() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_InvalidProductId_ResponseIsInvalidProductIdValue() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task TransactionDomainService_ValidateSaleTransaction_InvalidTransactionAmount_ResponseIsInvalidSaleTransactionAmount(Decimal transactionAmount) {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantDeviceListEmpty_ResponseIsNoValidDevices() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantDeviceListNull_ResponseIsNoValidDevices() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantDoesNotHaveSuppliedContract_ResponseIsContractNotValidForMerchant() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantHasNoContracts_ResponseIsMerchantDoesNotHaveEnoughCredit() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantHasNullContracts_ResponseIsMerchantDoesNotHaveEnoughCredit() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantNotEnoughCredit_ResponseIsMerchantDoesNotHaveEnoughCredit() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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

        [Fact]
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantNotFound_ResponseIsInvalidMerchantId() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantNotFoundOnGetContract_ResponseIsInvalidMerchantId() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantOperatorListEmpty_ResponseIsNoMerchantOperators() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_MerchantOperatorListNull_ResponseIsNoMerchantOperators() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_OperatorNotConfiguredFroMerchant_ResponseIsOperatorNotValidForMerchant() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_ProductIdNotConfigured_ResponseIsProductNotValidForMerchant() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
        public async Task TransactionDomainService_ValidateSaleTransaction_SuccessfulSale() {
            this.securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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

        #endregion
    }
}