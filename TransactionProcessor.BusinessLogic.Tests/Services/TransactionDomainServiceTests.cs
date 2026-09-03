using MessagingService.DataTransferObjects;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.BusinessLogic.Tests.Services{
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.Services;
    using MessagingService.Client;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Imposter;
    using Imposter.Abstractions;
    using SecurityService.Client;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using Shared.Serialisation;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Testing;
    using Xunit;

    public class TransactionDomainServiceTests{
        #region Fields

        private readonly IAggregateServiceImposter AggregateService;
        private readonly IOperatorProxyImposter OperatorProxy;
        private readonly ISecurityServiceClientImposter SecurityServiceClient;
        private readonly TransactionDomainService TransactionDomainService;
        private readonly ITransactionValidationServiceImposter TransactionValidationService;
        private readonly IMemoryCacheWrapperImposter MemoryCacheWrapper;
        private readonly IFeeCalculationManagerImposter FeeCalculationManager;
        private readonly ITransactionReceiptBuilderImposter TransactionReceiptBuilder;
        private readonly IMessagingServiceClientImposter MessagingServiceClient;
        #endregion

        #region Constructors

        public TransactionDomainServiceTests(){
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.AggregateService= new();
            this.SecurityServiceClient = new();
            this.OperatorProxy = new();
            Func<String, IOperatorProxy> operatorProxyResolver = operatorName => this.OperatorProxy.Instance();
            this.TransactionValidationService = new();
            this.MemoryCacheWrapper = new();
            this.FeeCalculationManager = new();
            this.TransactionReceiptBuilder = new();
            this.MessagingServiceClient = new();
            IAggregateService AggregateServiceResolver() => this.AggregateService.Instance();
            this.TransactionDomainService = new TransactionDomainService(AggregateServiceResolver,
                                                                         operatorProxyResolver,
                                                                         this.TransactionValidationService.Instance(),
                                                                         this.SecurityServiceClient.Instance(),
                                                                         this.MemoryCacheWrapper.Instance(),
                                                                         this.FeeCalculationManager.Instance(),
                                                                         this.TransactionReceiptBuilder.Instance(),
                                                                         this.MessagingServiceClient.Instance());
        }

        #endregion

        #region Methods

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_DeviceNeedsAdded_TransactionIsProcessed(){
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.AggregateService.GetLatest<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
            
            this.TransactionValidationService.ValidateLogonTransaction(Arg<Guid>.Any(),
                                                                                    Arg<Guid>.Any(),
                                                                                    Arg<String>.Any(),
                                                                                    Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.SuccessNeedToAddDevice, "SUCCESS")));

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber, TestData.TransactionReceivedDateTime);
            
            var result = await this.TransactionDomainService.ProcessLogonTransaction(command, TestContext.Current.CancellationToken);
            
            result.IsSuccess.ShouldBeTrue();
            result.Data.EstateId.ShouldBe(TestData.EstateId);
            result.Data.MerchantId.ShouldBe(TestData.MerchantId);
            result.Data.ResponseCode.ShouldBe("0001");
            result.Data.TransactionId.ShouldBe(TestData.TransactionId);
            this.AggregateService.GetLatest<MerchantAggregate>(TestData.MerchantId, Arg<CancellationToken>.Any()).Called(Count.Once());
            this.AggregateService.Save(Arg<MerchantAggregate>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_TransactionIsProcessed(){
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            this.TransactionValidationService.ValidateLogonTransaction(Arg<Guid>.Any(),
                                                                                    Arg<Guid>.Any(),
                                                                                    Arg<String>.Any(),
                                                                                    Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessLogonTransaction(command, TestContext.Current.CancellationToken);

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
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.TransactionValidationService.ValidateLogonTransaction(Arg<Guid>.Any(),
                                                                                    Arg<Guid>.Any(),
                                                                                    Arg<String>.Any(),
                                                                                    Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(responseCode, responseCode.ToString())));

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber, TestData.TransactionReceivedDateTime);

            Result<ProcessLogonTransactionResponse> result = await this.TransactionDomainService.ProcessLogonTransaction(command, TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Data.EstateId.ShouldBe(TestData.EstateId);
            result.Data.MerchantId.ShouldBe(TestData.MerchantId);
            result.Data.ResponseCode.ShouldBe(((Int32)responseCode).ToString().PadLeft(4, '0'));
            result.Data.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_ReconciliationIsProcessed(){
            this.AggregateService.GetLatest<ReconciliationAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(new ReconciliationAggregate());
            this.AggregateService.Save(Arg<ReconciliationAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.TransactionValidationService.ValidateReconciliationTransaction(Arg<Guid>.Any(),
                                                                                             Arg<Guid>.Any(),
                                                                                             Arg<String>.Any(),
                                                                                             Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            TransactionCommands.ProcessReconciliationCommand command =
                new(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime,
                    TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            var result  = await this.TransactionDomainService.ProcessReconciliationTransaction(command, TestContext.Current.CancellationToken);
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
            this.AggregateService.GetLatest<ReconciliationAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(new ReconciliationAggregate());
            this.AggregateService.Save(Arg<ReconciliationAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.TransactionValidationService.ValidateReconciliationTransaction(Arg<Guid>.Any(),
                                                                                             Arg<Guid>.Any(),
                                                                                             Arg<String>.Any(),
                                                                                             Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(responseCode, responseCode.ToString())));

            TransactionCommands.ProcessReconciliationCommand command =
                new(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime,
                    TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            var result = await this.TransactionDomainService.ProcessReconciliationTransaction(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
            var response = result.Data;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe(((Int32)responseCode).ToString().PadLeft(4, '0'));
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_DeclinedByOperator_TransactionIsProcessed(){
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Get<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            this.AggregateService.GetLatest<FloatAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetEmptyFloatAggregate()));

            this.TransactionValidationService.ValidateSaleTransaction(Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<String>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Decimal?>.Any(),
                                                                                   Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            this.OperatorProxy.ProcessSaleMessage(Arg<Guid>.Any(),
                                                               Arg<Guid>.Any(),
                                                               Arg<Models.Merchant.Merchant>.Any(),
                                                               Arg<DateTime>.Any(),
                                                               Arg<String>.Any(),
                                                               Arg<Dictionary<String, String>>.Any(),
                                                               Arg<CancellationToken>.Any()).ReturnsAsync(new Result<OperatorResponse>{Data = new OperatorResponse{
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
            
            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, TestContext.Current.CancellationToken);
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
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));

            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Get<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            this.AggregateService.GetLatest<FloatAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetEmptyFloatAggregate()));

            this.TransactionValidationService.ValidateSaleTransaction(Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<String>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Decimal?>.Any(),
                                                                                   Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            this.OperatorProxy.ProcessSaleMessage(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<Models.Merchant.Merchant>.Any(), Arg<DateTime>.Any(), Arg<String>.Any(), Arg<Dictionary<String, String>>.Any(), Arg<CancellationToken>.Any()).Throws(new Exception());
            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
            var response = result.Data;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe("1008");
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_TransactionIsProcessed(){
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));

            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Get<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

            TransactionAggregate transactionAggregate = TestData.GetEmptyTransactionAggregate();
            FloatAggregate floatAggregate = TestData.GetFloatAggregateWithCostValues();

            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(transactionAggregate);
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            this.AggregateService.GetLatest<FloatAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(floatAggregate));

            this.TransactionValidationService.ValidateSaleTransaction(Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<String>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Decimal?>.Any(),
                                                                                   Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            this.OperatorProxy.ProcessSaleMessage(Arg<Guid>.Any(),
                                                               Arg<Guid>.Any(),
                                                               Arg<Models.Merchant.Merchant>.Any(),
                                                               Arg<DateTime>.Any(),
                                                               Arg<String>.Any(),
                                                               Arg<Dictionary<String, String>>.Any(),
                                                               Arg<CancellationToken>.Any()).ReturnsAsync(new OperatorResponse{
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

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, TestContext.Current.CancellationToken);
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
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));

            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Get<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            TransactionAggregate transactionAggregate = TestData.GetEmptyTransactionAggregate();

            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(transactionAggregate);
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            this.AggregateService.GetLatest<FloatAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.NotFound());

            this.TransactionValidationService.ValidateSaleTransaction(Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<String>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Decimal?>.Any(),
                                                                                   Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            this.OperatorProxy.ProcessSaleMessage(Arg<Guid>.Any(),
                                                               Arg<Guid>.Any(),
                                                               Arg<Models.Merchant.Merchant>.Any(),
                                                               Arg<DateTime>.Any(),
                                                               Arg<String>.Any(),
                                                               Arg<Dictionary<String, String>>.Any(),
                                                               Arg<CancellationToken>.Any()).ReturnsAsync(new OperatorResponse
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

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, TestContext.Current.CancellationToken);
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
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            this.TransactionValidationService.ValidateSaleTransaction(Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<String>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Decimal?>.Any(),
                                                                                   Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(responseCode, responseCode.ToString())));

            this.AggregateService.GetLatest<FloatAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetEmptyFloatAggregate()));

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
            var response = result.Data;

            response.EstateId.ShouldBe(TestData.EstateId);
            response.MerchantId.ShouldBe(TestData.MerchantId);
            response.ResponseCode.ShouldBe(((Int32)responseCode).ToString().PadLeft(4, '0'));
            response.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public async Task TransactionDomainService_ResendTransactionReceipt_TransactionReceiptResendIsRequested(){
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionWithReceiptRequestedAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            TransactionCommands.ResendTransactionReceiptCommand command = new(TestData.TransactionId, TestData.EstateId);
            var result = await this.TransactionDomainService.ResendTransactionReceipt(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_RequireFeeCalculation_IsNotAuthorised_ReturnsFalse(){
            
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TransactionType.Sale, TestData.TransactionReference, new TransactionStartContext { EstateId = TestData.EstateId, MerchantId = TestData.MerchantId, DeviceIdentifier = TestData.DeviceIdentifier }, TestData.TransactionAmount);
            transactionAggregate.DeclineTransaction(TestData.OperatorId, "111", "SUCCESS", TransactionResponseCode.Success, "SUCCESS");

            var result = TransactionHelpers.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainService_RequireFeeCalculation_IsNotCompelted_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TransactionType.Sale, TestData.TransactionReference, new TransactionStartContext { EstateId = TestData.EstateId, MerchantId = TestData.MerchantId, DeviceIdentifier = TestData.DeviceIdentifier }, TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, "111", "111", "SUCCESS", "1234", TransactionResponseCode.Success, "SUCCESS");

            var result = TransactionHelpers.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainService_RequireFeeCalculation_IsALogon_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TransactionType.Logon, TestData.TransactionReference, new TransactionStartContext { EstateId = TestData.EstateId, MerchantId = TestData.MerchantId, DeviceIdentifier = TestData.DeviceIdentifier }, TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransactionLocally("111", TransactionResponseCode.InvalidDeviceIdentifier, "SUCCESS");
            transactionAggregate.CompleteTransaction();


            var result = TransactionHelpers.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainService_RequireFeeCalculation_NoContractId_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TransactionType.Sale, TestData.TransactionReference, new TransactionStartContext { EstateId = TestData.EstateId, MerchantId = TestData.MerchantId, DeviceIdentifier = TestData.DeviceIdentifier }, TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, "111", "111", "SUCCESS", "1234", TransactionResponseCode.Success, "SUCCESS");
            transactionAggregate.CompleteTransaction();


            var result = TransactionHelpers.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainService_RequireFeeCalculation_NullAmount_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TransactionType.Sale, TestData.TransactionReference, new TransactionStartContext { EstateId = TestData.EstateId, MerchantId = TestData.MerchantId, DeviceIdentifier = TestData.DeviceIdentifier }, null);
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
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TransactionType.Sale, TestData.TransactionReference, new TransactionStartContext { EstateId = TestData.EstateId, MerchantId = TestData.MerchantId, DeviceIdentifier = TestData.DeviceIdentifier }, TestData.TransactionAmount);
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
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));
            this.AggregateService.Get<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.FeeCalculationManager.CalculateFees(Arg<List<TransactionFeeToCalculate>>.Any(), Arg<Decimal>.Any(), Arg<DateTime>.Any()).Returns(TestData.CalculatedMerchantFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_MerchantWithImmediateSettlement_FeesCalculated()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));
            this.AggregateService.Get<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.FeeCalculationManager.CalculateFees(Arg<List<TransactionFeeToCalculate>>.Any(), Arg<Decimal>.Any(), Arg<DateTime>.Any()).Returns(TestData.CalculatedMerchantFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_NonMerchantFees_FeesCalculated()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));
            this.AggregateService.Get<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.ServiceProvider));
            this.FeeCalculationManager.CalculateFees(Arg<List<TransactionFeeToCalculate>>.Any(), Arg<Decimal>.Any(), Arg<DateTime>.Any()).Returns(TestData.CalculatedServiceProviderFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_TransactionNotNeedingFeeCaclulation_FeesCalculated()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedLogonTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));

            this.FeeCalculationManager.CalculateFees(Arg<List<TransactionFeeToCalculate>>.Any(), Arg<Decimal>.Any(), Arg<DateTime>.Any()).Returns(TestData.CalculatedServiceProviderFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_NoFeesReturned_FeesCalculated()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

            //this.EstateClient.GetTransactionFeesForProduct(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));

            this.FeeCalculationManager.CalculateFees(Arg<List<TransactionFeeToCalculate>>.Any(), Arg<Decimal>.Any(), Arg<DateTime>.Any()).Returns(TestData.CalculatedServiceProviderFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_AddSettledMerchantFee_FeeAdded() {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(TestData.TransactionFeeId)));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

            TransactionCommands.AddSettledMerchantFeeCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.CalculatedFeeValue, TestData.SettlementDate, TestData.SettlementAggregateId);

            var result = await this.TransactionDomainService.AddSettledMerchantFee(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_AddSettledMerchantFee_SaveFailed_ResultFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            TransactionCommands.AddSettledMerchantFeeCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.CalculatedFeeValue, TestData.SettlementDate, TestData.SettlementAggregateId);

            var result = await this.TransactionDomainService.AddSettledMerchantFee(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_SendCustomerEmailReceipt_ReceiptSent() {
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.TransactionReceiptBuilder.GetEmailReceiptMessage(Arg<Models.Transaction>.Any(), Arg<Merchant>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync("EmailMessage");
            this.MessagingServiceClient.SendEmail(Arg<String>.Any(), Arg<SendEmailRequest>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            TransactionCommands.SendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId, Guid.NewGuid(), TestData.CustomerEmailAddress);
            var result = await this.TransactionDomainService.SendCustomerEmailReceipt(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact] 
        public async Task TransactionDomainService_SendCustomerEmailReceipt_GetTransactionFailed_ResultFailed()
        {
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
            
            TransactionCommands.SendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId, Guid.NewGuid(), TestData.CustomerEmailAddress);
            var result = await this.TransactionDomainService.SendCustomerEmailReceipt(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ResendCustomerEmailReceipt_ReceiptSent()
        {
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            
            this.MessagingServiceClient.ResendEmail(Arg<String>.Any(), Arg<ResendEmailRequest>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            TransactionCommands.ResendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId);
            var result = await this.TransactionDomainService.ResendCustomerEmailReceipt(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ResendCustomerEmailReceipt_GetTransactionFailed_ResultFailed()
        {
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            TransactionCommands.ResendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId);
            var result = await this.TransactionDomainService.ResendCustomerEmailReceipt(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessLogonTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_SaveFailed_ResultFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());
            this.TransactionValidationService.ValidateLogonTransaction(Arg<Guid>.Any(),
                                                                                    Arg<Guid>.Any(),
                                                                                    Arg<String>.Any(),
                                                                                    Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            TransactionCommands.ProcessLogonTransactionCommand command = new(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeviceIdentifier,
                TestData.TransactionTypeLogon.ToString(),
                TestData.TransactionDateTime,
                TestData.TransactionNumber, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessLogonTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.GetLatest<ReconciliationAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            TransactionCommands.ProcessReconciliationCommand command =
                new(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime,
                    TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            var result = await this.TransactionDomainService.ProcessReconciliationTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessReconciliationTransaction_SaveFailed_ResultFailed()
        {
            this.AggregateService.GetLatest<ReconciliationAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(new ReconciliationAggregate());
            this.AggregateService.Save(Arg<ReconciliationAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());
            this.TransactionValidationService.ValidateReconciliationTransaction(Arg<Guid>.Any(),
                                                                                             Arg<Guid>.Any(),
                                                                                             Arg<String>.Any(),
                                                                                             Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));

            TransactionCommands.ProcessReconciliationCommand command =
                new(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime,
                    TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            var result = await this.TransactionDomainService.ProcessReconciliationTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ResendTransactionReceipt_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            TransactionCommands.ResendTransactionReceiptCommand command = new(TestData.TransactionId, TestData.EstateId);
            var result = await this.TransactionDomainService.ResendTransactionReceipt(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ResendTransactionReceipt_SaveFailed_ResultFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionWithReceiptRequestedAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            TransactionCommands.ResendTransactionReceiptCommand command = new(TestData.TransactionId, TestData.EstateId);
            var result = await this.TransactionDomainService.ResendTransactionReceipt(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            TransactionCommands.ProcessSaleTransactionCommand command =
                new TransactionCommands.ProcessSaleTransactionCommand(TestData.TransactionId, TestData.EstateId,
                    TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeSale.ToString(),
                    TestData.TransactionDateTime, TestData.TransactionNumber, TestData.OperatorId,
                    TestData.CustomerEmailAddress, TestData.AdditionalTransactionMetaDataForMobileTopup(),
                    TestData.ContractId, TestData.ProductId, TestData.TransactionSource, TestData.TransactionReceivedDateTime);

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ProcessSaleTransaction_SaveFailed_ResultFailed()
        {
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Get<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.GetEmptyTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());
            this.AggregateService.GetLatest<FloatAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.NotFound());
            this.TransactionValidationService.ValidateSaleTransaction(Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<String>.Any(),
                                                                                   Arg<Guid>.Any(),
                                                                                   Arg<Decimal?>.Any(),
                                                                                   Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new TransactionValidationResult(TransactionResponseCode.Success, "SUCCESS")));
            this.OperatorProxy.ProcessSaleMessage(Arg<Guid>.Any(),
                                                               Arg<Guid>.Any(),
                                                               Arg<Models.Merchant.Merchant>.Any(),
                                                               Arg<DateTime>.Any(),
                                                               Arg<String>.Any(),
                                                               Arg<Dictionary<String, String>>.Any(),
                                                               Arg<CancellationToken>.Any()).ReturnsAsync(new OperatorResponse {
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

            var result = await this.TransactionDomainService.ProcessSaleTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_MerchantSettlementScheduleNotSet_ResultFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.EmptyMerchantAggregate());
            this.AggregateService.Get<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.FeeCalculationManager.CalculateFees(Arg<List<TransactionFeeToCalculate>>.Any(), Arg<Decimal>.Any(), Arg<DateTime>.Any()).Returns(TestData.CalculatedMerchantFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_CalculateFeesForTransaction_SaveFailed_ResultFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Save(Arg<TransactionAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Get<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.FeeCalculationManager.CalculateFees(Arg<List<TransactionFeeToCalculate>>.Any(), Arg<Decimal>.Any(), Arg<DateTime>.Any()).Returns(TestData.CalculatedMerchantFees);

            TransactionCommands.CalculateFeesForTransactionCommand command = new(TestData.TransactionId, TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            var result = await this.TransactionDomainService.CalculateFeesForTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_AddSettledMerchantFee_GetAggregateFailed_ResultFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            TransactionCommands.AddSettledMerchantFeeCommand command = new(TestData.TransactionId, TestData.CalculatedFeeValue, TestData.TransactionFeeCalculateDateTime, CalculationType.Fixed, TestData.TransactionFeeId, TestData.CalculatedFeeValue, TestData.SettlementDate, TestData.SettlementAggregateId);

            var result = await this.TransactionDomainService.AddSettledMerchantFee(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_SendCustomerEmailReceipt_GetTokenFailed_ResultFailed()
        {
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            TransactionCommands.SendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId, Guid.NewGuid(), TestData.CustomerEmailAddress);
            var result = await this.TransactionDomainService.SendCustomerEmailReceipt(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_SendCustomerEmailReceipt_GetMerchantFailed_ResultFailed()
        {
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            TransactionCommands.SendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId, Guid.NewGuid(), TestData.CustomerEmailAddress);
            var result = await this.TransactionDomainService.SendCustomerEmailReceipt(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_SendCustomerEmailReceipt_GetEstateFailed_ResultFailed()
        {
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithOperator());
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            TransactionCommands.SendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId, Guid.NewGuid(), TestData.CustomerEmailAddress);
            var result = await this.TransactionDomainService.SendCustomerEmailReceipt(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainService_ResendCustomerEmailReceipt_GetTokenFailed_ResultFailed()
        {
            this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            TransactionCommands.ResendCustomerEmailReceiptCommand command = new(TestData.EstateId, TestData.TransactionId);
            var result = await this.TransactionDomainService.ResendCustomerEmailReceipt(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        #endregion
    }
}

