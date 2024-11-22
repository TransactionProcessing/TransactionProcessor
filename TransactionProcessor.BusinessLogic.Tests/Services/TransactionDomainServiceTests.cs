using Microsoft.Extensions.Caching.Memory;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Tests.Services{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.Services;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Requests;
    using EstateManagement.DataTransferObjects.Requests.Merchant;
    using EstateManagement.DataTransferObjects.Responses.Merchant;
    using FloatAggregate;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Moq;
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

    public class TransactionDomainServiceTests{
        #region Fields

        private readonly Mock<IEstateClient> EstateClient;

        private readonly Mock<IOperatorProxy> OperatorProxy;

        private readonly Mock<IAggregateRepository<ReconciliationAggregate, DomainEvent>> ReconciliationAggregateRepository;

        private readonly Mock<ISecurityServiceClient> SecurityServiceClient;

        private readonly Mock<IAggregateRepository<TransactionAggregate, DomainEvent>> TransactionAggregateRepository;

        private readonly TransactionDomainService TransactionDomainService;

        private readonly Mock<ITransactionValidationService> TransactionValidationService;

        private readonly Mock<IAggregateRepository<FloatAggregate, DomainEvent>> FloatAggregateRepository;
        private readonly Mock<IMemoryCacheWrapper> MemoryCacheWrapper;
        private readonly Mock<IFeeCalculationManager> FeeCalculationManager;
        #endregion

        #region Constructors

        public TransactionDomainServiceTests(){
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.TransactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEvent>>();
            this.EstateClient = new Mock<IEstateClient>();
            this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
            this.OperatorProxy = new Mock<IOperatorProxy>();
            this.ReconciliationAggregateRepository = new Mock<IAggregateRepository<ReconciliationAggregate, DomainEvent>>();
            Func<String, IOperatorProxy> operatorProxyResolver = operatorName => { return this.OperatorProxy.Object; };
            this.TransactionValidationService = new Mock<ITransactionValidationService>();
            this.FloatAggregateRepository = new Mock<IAggregateRepository<FloatAggregate, DomainEvent>>();
            this.MemoryCacheWrapper = new Mock<IMemoryCacheWrapper>();
            this.FeeCalculationManager = new Mock<IFeeCalculationManager>();

            this.TransactionDomainService = new TransactionDomainService(this.TransactionAggregateRepository.Object,
                                                                         this.EstateClient.Object,
                                                                         operatorProxyResolver,
                                                                         this.ReconciliationAggregateRepository.Object,
                                                                         this.TransactionValidationService.Object,
                                                                         this.SecurityServiceClient.Object,
                                                                         this.FloatAggregateRepository.Object,
                                                                         this.MemoryCacheWrapper.Object,
                                                                         this.FeeCalculationManager.Object);
        }

        #endregion

        #region Methods

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_DeviceNeedsAdded_TransactionIsProcessed(){
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.TransactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.EstateClient.Setup(e => e.AddDeviceToMerchant(It.IsAny<String>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<AddMerchantDeviceRequest>(),
                                                               It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

            this.TransactionValidationService.Setup(t => t.ValidateLogonTransactionX(It.IsAny<Guid>(),
                                                                                    It.IsAny<Guid>(),
                                                                                    It.IsAny<String>(),
                                                                                    It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.SuccessNeedToAddDevice, "SUCCESS")));

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber);
            
            var result = await this.TransactionDomainService.ProcessLogonTransaction(command, CancellationToken.None);
            
            result.IsSuccess.ShouldBeTrue();
            result.Data.EstateId.ShouldBe(TestData.EstateId);
            result.Data.MerchantId.ShouldBe(TestData.MerchantId);
            result.Data.ResponseCode.ShouldBe("0001");
            result.Data.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_TransactionIsProcessed(){
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.TransactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.TransactionValidationService.Setup(t => t.ValidateLogonTransactionX(It.IsAny<Guid>(),
                                                                                    It.IsAny<Guid>(),
                                                                                    It.IsAny<String>(),
                                                                                    It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber);

            var result = await this.TransactionDomainService.ProcessLogonTransaction(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.EstateId.ShouldBe(TestData.EstateId);
            result.Data.MerchantId.ShouldBe(TestData.MerchantId);
            result.Data.ResponseCode.ShouldBe("0000");
            result.Data.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Theory]
        [InlineData(TransactionResponseCode.InvalidEstateId)]
        [InlineData(TransactionResponseCode.InvalidMerchantId)]
        [InlineData(TransactionResponseCode.InvalidDeviceIdentifier)]
        public async Task TransactionDomainService_ProcessLogonTransaction_ValidationFailed_TransactionIsProcessed(TransactionResponseCode responseCode){
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.TransactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            this.TransactionValidationService.Setup(t => t.ValidateLogonTransactionX(It.IsAny<Guid>(),
                                                                                    It.IsAny<Guid>(),
                                                                                    It.IsAny<String>(),
                                                                                    It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(responseCode, responseCode.ToString())));

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber);

            Result<ProcessLogonTransactionResponse> result = await this.TransactionDomainService.ProcessLogonTransaction(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.EstateId.ShouldBe(TestData.EstateId);
            result.Data.MerchantId.ShouldBe(TestData.MerchantId);
            result.Data.ResponseCode.ShouldBe(((Int32)responseCode).ToString().PadLeft(4, '0'));
            result.Data.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_ReconciliationIsProcessed(){
            this.ReconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReconciliationAggregate());
            this.ReconciliationAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<ReconciliationAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            this.TransactionValidationService.Setup(t => t.ValidateReconciliationTransactionX(It.IsAny<Guid>(),
                                                                                             It.IsAny<Guid>(),
                                                                                             It.IsAny<String>(),
                                                                                             It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            TransactionCommands.ProcessReconciliationCommand command =
                new(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime,
                    TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            ProcessReconciliationTransactionResponse response = await this.TransactionDomainService.ProcessReconciliationTransaction(command, CancellationToken.None);
            ;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("0000");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Theory]
        [InlineData(TransactionResponseCode.InvalidEstateId)]
        [InlineData(TransactionResponseCode.InvalidMerchantId)]
        [InlineData(TransactionResponseCode.NoValidDevices)]
        [InlineData(TransactionResponseCode.InvalidDeviceIdentifier)]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_ValidationFailed_ReconciliationIsProcessed(TransactionResponseCode responseCode){
            this.ReconciliationAggregateRepository.Setup(r => r.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReconciliationAggregate());
            this.ReconciliationAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<ReconciliationAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            this.TransactionValidationService.Setup(t => t.ValidateReconciliationTransactionX(It.IsAny<Guid>(),
                                                                                             It.IsAny<Guid>(),
                                                                                             It.IsAny<String>(),
                                                                                             It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(responseCode, responseCode.ToString())));

            TransactionCommands.ProcessReconciliationCommand command =
                new(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime,
                    TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            ProcessReconciliationTransactionResponse response = await this.TransactionDomainService.ProcessReconciliationTransaction(command, CancellationToken.None);
            ;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe(((Int32)responseCode).ToString().PadLeft(4, '0'));
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_DeclinedByOperator_TransactionIsProcessed(){
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

            this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.TransactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatAggregate()));

            this.TransactionValidationService.Setup(t => t.ValidateSaleTransactionX(It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<String>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Decimal>(),
                                                                                   It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            this.OperatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse>(),
                                                               It.IsAny<DateTime>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<Dictionary<String, String>>(),
                                                               It.IsAny<CancellationToken>())).ReturnsAsync(new Result<OperatorResponse>{Data = new OperatorResponse{
                                                                                                                                    ResponseMessage = TestData.OperatorResponseMessage,
                                                                                                                                    IsSuccessful = false,
                                                                                                                                    AuthorisationCode =
                                                                                                                                        TestData.OperatorAuthorisationCode,
                                                                                                                                    TransactionId = TestData.OperatorTransactionId,
                                                                                                                                    ResponseCode = TestData.ResponseCode}, IsSuccess = false});

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource);
            
            ProcessSaleTransactionResponse response = await this.TransactionDomainService.ProcessSaleTransaction(command, CancellationToken.None);

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("1008");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }
        
        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_TransactionIsProcessed(){
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

            this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            TransactionAggregate transactionAggregate = TestData.GetEmptyTransactionAggregate();
            FloatAggregate floatAggregate = TestData.GetFloatAggregateWithCostValues();

            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionAggregate);
            this.TransactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(floatAggregate);

            this.TransactionValidationService.Setup(t => t.ValidateSaleTransactionX(It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<String>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Decimal>(),
                                                                                   It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            this.OperatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse>(),
                                                               It.IsAny<DateTime>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<Dictionary<String, String>>(),
                                                               It.IsAny<CancellationToken>())).ReturnsAsync(new OperatorResponse{
                                                                                                                                    ResponseMessage = TestData.OperatorResponseMessage,
                                                                                                                                    IsSuccessful = true,
                                                                                                                                    AuthorisationCode =
                                                                                                                                        TestData.OperatorAuthorisationCode,
                                                                                                                                    TransactionId = TestData.OperatorTransactionId,
                                                                                                                                    ResponseCode = TestData.ResponseCode
                                                                                                                                });

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource);

            ProcessSaleTransactionResponse response = await this.TransactionDomainService.ProcessSaleTransaction(command, CancellationToken.None);

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("0000");
            response.TransactionId.ShouldBe(TestData.TransactionId);

            // check the cost values
            transactionAggregate.UnitCost.ShouldBe(floatAggregate.GetUnitCostPrice());
            transactionAggregate.TotalCost.ShouldBe(floatAggregate.GetUnitCostPrice() * TestData.TransactionAmount);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_NoFloatFound_TransactionIsProcessed()
        {
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

            this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
            this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            TransactionAggregate transactionAggregate = TestData.GetEmptyTransactionAggregate();
            FloatAggregate floatAggregate = TestData.GetFloatAggregateWithCostValues();

            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionAggregate);
            this.TransactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatAggregate()));

            this.TransactionValidationService.Setup(t => t.ValidateSaleTransactionX(It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<String>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Decimal>(),
                                                                                   It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            this.OperatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<String>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse>(),
                                                               It.IsAny<DateTime>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<Dictionary<String, String>>(),
                                                               It.IsAny<CancellationToken>())).ReturnsAsync(new OperatorResponse
                                                               {
                                                                   ResponseMessage = TestData.OperatorResponseMessage,
                                                                   IsSuccessful = true,
                                                                   AuthorisationCode =
                                                                                                                                        TestData.OperatorAuthorisationCode,
                                                                   TransactionId = TestData.OperatorTransactionId,
                                                                   ResponseCode = TestData.ResponseCode
                                                               });

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource);

            ProcessSaleTransactionResponse response = await this.TransactionDomainService.ProcessSaleTransaction(command, CancellationToken.None);

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("0000");
            response.TransactionId.ShouldBe(TestData.TransactionId);

            // check the cost values
            transactionAggregate.UnitCost.ShouldBeNull();
            transactionAggregate.TotalCost.ShouldBeNull();
        }

        [Theory]
        [InlineData(TransactionResponseCode.InvalidEstateId)]
        [InlineData(TransactionResponseCode.InvalidContractIdValue)]
        [InlineData(TransactionResponseCode.InvalidProductIdValue)]
        [InlineData(TransactionResponseCode.ContractNotValidForMerchant)]
        [InlineData(TransactionResponseCode.ProductNotValidForMerchant)]
        public async Task TransactionDomainService_ProcessSaleTransaction_ValidationFailed_TransactionIsProcessed(TransactionResponseCode responseCode){
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.TransactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.TransactionValidationService.Setup(t => t.ValidateSaleTransactionX(It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<String>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Decimal>(),
                                                                                   It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(responseCode, responseCode.ToString())));

            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatAggregate()));

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource);

            ProcessSaleTransactionResponse response = await this.TransactionDomainService.ProcessSaleTransaction(command, CancellationToken.None);

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe(((Int32)responseCode).ToString().PadLeft(4, '0'));
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ResendTransactionReceipt_TransactionReceiptResendIsRequested(){
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionWithReceiptRequestedAggregate()));
            this.TransactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            TransactionCommands.ResendTransactionReceiptCommand command = new(TestData.TransactionId, TestData.EstateId);
            var result = await this.TransactionDomainService.ResendTransactionReceipt(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        #endregion
    }
}