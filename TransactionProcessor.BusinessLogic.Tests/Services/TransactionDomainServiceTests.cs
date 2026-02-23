using MessagingService.DataTransferObjects;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.BusinessLogic.Tests.Services{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.Services;
    using MessagingService.Client;
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
    using Xunit;

    public class TransactionDomainServiceTests{
        #region Fields

        private readonly Mock<IAggregateService> AggregateService;
        private readonly Mock<IOperatorProxy> OperatorProxy;
        private readonly Mock<ISecurityServiceClient> SecurityServiceClient;
        private readonly TransactionDomainService TransactionDomainService;
        private readonly Mock<ITransactionValidationService> TransactionValidationService;
        private readonly Mock<IMemoryCacheWrapper> MemoryCacheWrapper;
        private readonly Mock<IFeeCalculationManager> FeeCalculationManager;
        private readonly Mock<ITransactionReceiptBuilder> TransactionReceiptBuilder;
        private readonly Mock<IMessagingServiceClient> MessagingServiceClient;
        #endregion

        #region Constructors

        public TransactionDomainServiceTests(){
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.AggregateService= new Mock<IAggregateService>();
            this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
            this.OperatorProxy = new Mock<IOperatorProxy>();
            Func<String, IOperatorProxy> operatorProxyResolver = operatorName => { return this.OperatorProxy.Object; };
            this.TransactionValidationService = new Mock<ITransactionValidationService>();
            this.MemoryCacheWrapper = new Mock<IMemoryCacheWrapper>();
            this.FeeCalculationManager = new Mock<IFeeCalculationManager>();
            this.TransactionReceiptBuilder = new Mock<ITransactionReceiptBuilder>();
            this.MessagingServiceClient = new Mock<IMessagingServiceClient>();
            IAggregateService AggregateServiceResolver() => this.AggregateService.Object;
            this.TransactionDomainService = new TransactionDomainService(AggregateServiceResolver,
                                                                         operatorProxyResolver,
                                                                         this.TransactionValidationService.Object,
                                                                         this.SecurityServiceClient.Object,
                                                                         this.MemoryCacheWrapper.Object,
                                                                         this.FeeCalculationManager.Object,
                                                                         this.TransactionReceiptBuilder.Object,
                                                                         this.MessagingServiceClient.Object);
        }

        #endregion

        #region Methods

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_DeviceNeedsAdded_TransactionIsProcessed(){
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            this.AggregateService.Setup(e => e.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
            
            this.TransactionValidationService.Setup(t => t.ValidateLogonTransaction(It.IsAny<Guid>(),
                                                                                    It.IsAny<Guid>(),
                                                                                    It.IsAny<String>(),
                                                                                    It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.SuccessNeedToAddDevice, "SUCCESS")));

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber, TestData.TransactionReceivedDateTime);
            
            var result = await this.TransactionDomainService.ProcessLogonTransaction(command, CancellationToken.None);
            
            result.IsSuccess.ShouldBeTrue();
            result.Data.EstateId.ShouldBe(TestData.EstateId);
            result.Data.MerchantId.ShouldBe(TestData.MerchantId);
            result.Data.ResponseCode.ShouldBe("0001");
            result.Data.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_TransactionIsProcessed(){
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.TransactionValidationService.Setup(t => t.ValidateLogonTransaction(It.IsAny<Guid>(),
                                                                                    It.IsAny<Guid>(),
                                                                                    It.IsAny<String>(),
                                                                                    It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber, TestData.TransactionReceivedDateTime);

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
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            this.TransactionValidationService.Setup(t => t.ValidateLogonTransaction(It.IsAny<Guid>(),
                                                                                    It.IsAny<Guid>(),
                                                                                    It.IsAny<String>(),
                                                                                    It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(responseCode, responseCode.ToString())));

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber, TestData.TransactionReceivedDateTime);

            Result<ProcessLogonTransactionResponse> result = await this.TransactionDomainService.ProcessLogonTransaction(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.EstateId.ShouldBe(TestData.EstateId);
            result.Data.MerchantId.ShouldBe(TestData.MerchantId);
            result.Data.ResponseCode.ShouldBe(((Int32)responseCode).ToString().PadLeft(4, '0'));
            result.Data.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_ReconciliationIsProcessed(){
            this.AggregateService.Setup(r => r.GetLatest<ReconciliationAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReconciliationAggregate());
            this.AggregateService.Setup(t => t.Save(It.IsAny<ReconciliationAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            this.TransactionValidationService.Setup(t => t.ValidateReconciliationTransaction(It.IsAny<Guid>(),
                                                                                             It.IsAny<Guid>(),
                                                                                             It.IsAny<String>(),
                                                                                             It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            TransactionCommands.ProcessReconciliationCommand command =
                new(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime,
                    TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            var result  = await this.TransactionDomainService.ProcessReconciliationTransaction(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            ProcessReconciliationTransactionResponse response = result.Data;

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
            this.AggregateService.Setup(r => r.GetLatest<ReconciliationAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReconciliationAggregate());
            this.AggregateService.Setup(t => t.Save(It.IsAny<ReconciliationAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            this.TransactionValidationService.Setup(t => t.ValidateReconciliationTransaction(It.IsAny<Guid>(),
                                                                                             It.IsAny<Guid>(),
                                                                                             It.IsAny<String>(),
                                                                                             It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(responseCode, responseCode.ToString())));

            TransactionCommands.ProcessReconciliationCommand command =
                new(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime,
                    TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            var result = await this.TransactionDomainService.ProcessReconciliationTransaction(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            var response = result.Data;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe(((Int32)responseCode).ToString().PadLeft(4, '0'));
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_DeclinedByOperator_TransactionIsProcessed(){
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Setup(c => c.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Setup(o => o.Get<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.AggregateService.Setup(f => f.GetLatest<FloatAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatAggregate()));

            this.TransactionValidationService.Setup(t => t.ValidateSaleTransaction(It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<String>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Decimal>(),
                                                                                   It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            this.OperatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<Guid>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<Models.Merchant.Merchant>(),
                                                               It.IsAny<DateTime>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<Dictionary<String, String>>(),
                                                               It.IsAny<CancellationToken>())).ReturnsAsync(new Result<OperatorResponse>{Data = new OperatorResponse{
                                                                                                                                    ResponseMessage = TestData.OperatorResponseMessage,
                                                                                                                                    IsSuccessful = false,
                                                                                                                                    AuthorisationCode =
                                                                                                                                        TestData.OperatorAuthorisationCode,
                                                                                                                                    TransactionId = TestData.OperatorTransactionId,
                                                                                                                                    ResponseCode = TestData.ResponseCode.ToCodeString()
            }, IsSuccess = false});

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource, TestData.TransactionReceivedDateTime);
            
            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            var response = result.Data;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("1008");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_OperatorProxyThrowsException_TransactionIsProcessed()
        {
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Setup(c => c.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Setup(o => o.Get<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.AggregateService.Setup(f => f.GetLatest<FloatAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatAggregate()));

            this.TransactionValidationService.Setup(t => t.ValidateSaleTransaction(It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<String>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Decimal>(),
                                                                                   It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            this.OperatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Models.Merchant.Merchant>(), It.IsAny<DateTime>(), It.IsAny<String>(), It.IsAny<Dictionary<String, String>>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            var response = result.Data;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("1008");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_TransactionIsProcessed(){
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Setup(c => c.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Setup(o => o.Get<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

            TransactionAggregate transactionAggregate = TestData.GetEmptyTransactionAggregate();
            FloatAggregate floatAggregate = TestData.GetFloatAggregateWithCostValues();

            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionAggregate);
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.AggregateService.Setup(f => f.GetLatest<FloatAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(floatAggregate));

            this.TransactionValidationService.Setup(t => t.ValidateSaleTransaction(It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<String>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Decimal>(),
                                                                                   It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            this.OperatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<Guid>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<Models.Merchant.Merchant>(),
                                                               It.IsAny<DateTime>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<Dictionary<String, String>>(),
                                                               It.IsAny<CancellationToken>())).ReturnsAsync(new OperatorResponse{
                                                                                                                                    ResponseMessage = TestData.OperatorResponseMessage,
                                                                                                                                    IsSuccessful = true,
                                                                                                                                    AuthorisationCode =
                                                                                                                                        TestData.OperatorAuthorisationCode,
                                                                                                                                    TransactionId = TestData.OperatorTransactionId,
                                                                                                                                    ResponseCode = TestData.ResponseCode.ToCodeString()
                                                               });

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            var response = result.Data;

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

            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Setup(c => c.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Setup(o => o.Get<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            TransactionAggregate transactionAggregate = TestData.GetEmptyTransactionAggregate();

            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionAggregate);
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.AggregateService.Setup(f => f.GetLatest<FloatAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

            this.TransactionValidationService.Setup(t => t.ValidateSaleTransaction(It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<String>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Decimal>(),
                                                                                   It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            this.OperatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<Guid>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<Models.Merchant.Merchant>(),
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
                                                                   ResponseCode = TestData.ResponseCode.ToCodeString()
                                                               });

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            var response = result.Data;

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
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            this.TransactionValidationService.Setup(t => t.ValidateSaleTransaction(It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<String>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Decimal>(),
                                                                                   It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(responseCode, responseCode.ToString())));

            this.AggregateService.Setup(f => f.GetLatest<FloatAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatAggregate()));

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            var response = result.Data;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe(((Int32)responseCode).ToString().PadLeft(4, '0'));
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ResendTransactionReceipt_TransactionReceiptResendIsRequested(){
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionWithReceiptRequestedAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            TransactionCommands.ResendTransactionReceiptCommand command = new(TestData.TransactionId, TestData.EstateId);
            var result = await this.TransactionDomainService.ResendTransactionReceipt(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_RequireFeeCalculation_IsNotAuthorised_ReturnsFalse(){
            
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Sale, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.DeclineTransaction(TestData.OperatorId, "111", "SUCCESS", TransactionResponseCode.Success, "SUCCESS");

            // TODO: maybe move this to an extension on aggregate
            var result = TransactionHelpers.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainService_RequireFeeCalculation_IsNotCompelted_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Sale, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, "111", "111", "SUCCESS", "1234", TransactionResponseCode.Success, "SUCCESS");

            var result = TransactionHelpers.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainService_RequireFeeCalculation_IsALogon_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Logon, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransactionLocally("111", TransactionResponseCode.InvalidDeviceIdentifier, "SUCCESS");
            transactionAggregate.CompleteTransaction();


            var result = TransactionHelpers.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainService_RequireFeeCalculation_NoContractId_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Sale, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, "111", "111", "SUCCESS", "1234", TransactionResponseCode.Success, "SUCCESS");
            transactionAggregate.CompleteTransaction();


            var result = TransactionHelpers.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainService_RequireFeeCalculation_NullAmount_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Sale, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  null);
            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, "111", "111", "SUCCESS", "1234", TransactionResponseCode.Success, "SUCCESS");
            transactionAggregate.CompleteTransaction();


            var result = TransactionHelpers.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainService_RequireFeeCalculation_ReturnsTrue()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Sale, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, "111", "111", "SUCCESS", "1234", TransactionResponseCode.Success, "SUCCESS");
            transactionAggregate.CompleteTransaction();


            var result = TransactionHelpers.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeTrue();
        }

        [Theory]
        [InlineData(SettlementSchedule.Immediate, "2024-05-01", "2024-05-01")]
        [InlineData(SettlementSchedule.NotSet, "2024-05-01", "2024-05-01")]
        [InlineData(SettlementSchedule.Weekly, "2024-05-01", "2024-05-08")]
        [InlineData(SettlementSchedule.Monthly, "2024-05-01", "2024-06-01")]
        public async Task TransactionDomainService_CalculateSettlementDate_CorrectDateReturned(SettlementSchedule settlementSchedule, String completedDateString, String expectedDateString){

            DateTime completedDate = DateTime.ParseExact(completedDateString, "yyyy-MM-dd", null);
            DateTime expectedDate = DateTime.ParseExact(expectedDateString, "yyyy-MM-dd", null);
            DateTime result = TransactionHelpers.CalculateSettlementDate(settlementSchedule, completedDate);
            result.Date.ShouldBe(expectedDate.Date);
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_FeesCalculated() {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
            this.AggregateService.Setup(c => c.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(TestData.CalculatedMerchantFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_MerchantWithImmediateSettlement_FeesCalculated()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
            this.AggregateService.Setup(c => c.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(TestData.CalculatedMerchantFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_NonMerchantFees_FeesCalculated()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
            this.AggregateService.Setup(c => c.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.ServiceProvider));
            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(TestData.CalculatedServiceProviderFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_TransactionNotNeedingFeeCaclulation_FeesCalculated()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedLogonTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(TestData.CalculatedServiceProviderFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_NoFeesReturned_FeesCalculated()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

            //this.EstateClient.Setup(e => e.GetTransactionFeesForProduct(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(TestData.CalculatedServiceProviderFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_AddSettledMerchantFee_FeeAdded() {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.TransactionFeeId)));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

            TransactionCommands.AddSettledMerchantFeeCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.CalculatedFeeValue, TestData.SettlementDate, TestData.SettlementAggregateId);

            var result = await this.TransactionDomainService.AddSettledMerchantFee(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_AddSettledMerchantFee_SaveFailed_ResultFailed()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            TransactionCommands.AddSettledMerchantFeeCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.CalculatedFeeValue, TestData.SettlementDate, TestData.SettlementAggregateId);

            var result = await this.TransactionDomainService.AddSettledMerchantFee(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_SendCustomerEmailReceipt_ReceiptSent() {
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Setup(c => c.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.TransactionReceiptBuilder.Setup(r => r.GetEmailReceiptMessage(It.IsAny<Models.Transaction>(), It.IsAny<Merchant>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync("EmailMessage");
            this.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(), It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            TransactionCommands.SendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId, Guid.NewGuid(), TestData.CustomerEmailAddress);
            var result = await this.TransactionDomainService.SendCustomerEmailReceipt(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact] 
        public async Task TransactionDomainService_SendCustomerEmailReceipt_GetTransactionFailed_ResultFailed()
        {
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());
            
            TransactionCommands.SendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId, Guid.NewGuid(), TestData.CustomerEmailAddress);
            var result = await this.TransactionDomainService.SendCustomerEmailReceipt(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ResendCustomerEmailReceipt_ReceiptSent()
        {
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            
            this.MessagingServiceClient.Setup(m => m.ResendEmail(It.IsAny<String>(), It.IsAny<ResendEmailRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            TransactionCommands.ResendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId);
            var result = await this.TransactionDomainService.ResendCustomerEmailReceipt(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ResendCustomerEmailReceipt_GetTransactionFailed_ResultFailed()
        {
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            TransactionCommands.ResendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId);
            var result = await this.TransactionDomainService.ResendCustomerEmailReceipt(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessLogonTransaction(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_SaveFailed_ResultFailed()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());
            this.TransactionValidationService.Setup(t => t.ValidateLogonTransaction(It.IsAny<Guid>(),
                                                                                    It.IsAny<Guid>(),
                                                                                    It.IsAny<String>(),
                                                                                    It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessLogonTransaction(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.Setup(r => r.GetLatest<ReconciliationAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            TransactionCommands.ProcessReconciliationCommand command =
                new(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime,
                    TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            var result = await this.TransactionDomainService.ProcessReconciliationTransaction(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_SaveFailed_ResultFailed()
        {
            this.AggregateService.Setup(r => r.GetLatest<ReconciliationAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReconciliationAggregate());
            this.AggregateService.Setup(t => t.Save(It.IsAny<ReconciliationAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());
            this.TransactionValidationService.Setup(t => t.ValidateReconciliationTransaction(It.IsAny<Guid>(),
                                                                                             It.IsAny<Guid>(),
                                                                                             It.IsAny<String>(),
                                                                                             It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            TransactionCommands.ProcessReconciliationCommand command =
                new(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime,
                    TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            var result = await this.TransactionDomainService.ProcessReconciliationTransaction(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ResendTransactionReceipt_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            TransactionCommands.ResendTransactionReceiptCommand command = new(TestData.TransactionId, TestData.EstateId);
            var result = await this.TransactionDomainService.ResendTransactionReceipt(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ResendTransactionReceipt_SaveFailed_ResultFailed()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionWithReceiptRequestedAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            TransactionCommands.ResendTransactionReceiptCommand command = new(TestData.TransactionId, TestData.EstateId);
            var result = await this.TransactionDomainService.ResendTransactionReceipt(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_SaveFailed_ResultFailed()
        {
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Setup(o => o.Get<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());
            this.AggregateService.Setup(f => f.GetLatest<FloatAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());
            this.TransactionValidationService.Setup(t => t.ValidateSaleTransaction(It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<String>(),
                                                                                   It.IsAny<Guid>(),
                                                                                   It.IsAny<Decimal>(),
                                                                                   It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));
            this.OperatorProxy.Setup(o => o.ProcessSaleMessage(It.IsAny<Guid>(),
                                                               It.IsAny<Guid>(),
                                                               It.IsAny<Models.Merchant.Merchant>(),
                                                               It.IsAny<DateTime>(),
                                                               It.IsAny<String>(),
                                                               It.IsAny<Dictionary<String, String>>(),
                                                               It.IsAny<CancellationToken>())).ReturnsAsync(new OperatorResponse {
                                                                   ResponseMessage = TestData.OperatorResponseMessage,
                                                                   IsSuccessful = true,
                                                                   AuthorisationCode = TestData.OperatorAuthorisationCode,
                                                                   TransactionId = TestData.OperatorTransactionId,
                                                                   ResponseCode = TestData.ResponseCode.ToCodeString()
                                                               });

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_MerchantSettlementScheduleNotSet_ResultFailed()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.EmptyMerchantAggregate());
            this.AggregateService.Setup(c => c.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(TestData.CalculatedMerchantFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_SaveFailed_ResultFailed()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Setup(t => t.Save(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Setup(c => c.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(TestData.CalculatedMerchantFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_AddSettledMerchantFee_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            TransactionCommands.AddSettledMerchantFeeCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.CalculatedFeeValue, TestData.SettlementDate, TestData.SettlementAggregateId);

            var result = await this.TransactionDomainService.AddSettledMerchantFee(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_SendCustomerEmailReceipt_GetTokenFailed_ResultFailed()
        {
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            TransactionCommands.SendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId, Guid.NewGuid(), TestData.CustomerEmailAddress);
            var result = await this.TransactionDomainService.SendCustomerEmailReceipt(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_SendCustomerEmailReceipt_GetMerchantFailed_ResultFailed()
        {
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            TransactionCommands.SendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId, Guid.NewGuid(), TestData.CustomerEmailAddress);
            var result = await this.TransactionDomainService.SendCustomerEmailReceipt(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_SendCustomerEmailReceipt_GetEstateFailed_ResultFailed()
        {
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
            this.AggregateService.Setup(t => t.GetLatest<TransactionAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Setup(e => e.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Setup(c => c.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            TransactionCommands.SendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId, Guid.NewGuid(), TestData.CustomerEmailAddress);
            var result = await this.TransactionDomainService.SendCustomerEmailReceipt(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ResendCustomerEmailReceipt_GetTokenFailed_ResultFailed()
        {
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            TransactionCommands.ResendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId);
            var result = await this.TransactionDomainService.ResendCustomerEmailReceipt(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        #endregion
    }
}