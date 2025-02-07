using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.BusinessLogic.Tests.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using EstateManagement.Client;
using EstateManagement.DataTransferObjects.Responses;
using EstateManagement.DataTransferObjects.Responses.Contract;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using ProjectionEngine.Repository;
using ProjectionEngine.State;
using SecurityService.Client;
using Shared.EventStore.EventStore;
using Shared.General;
using Shared.Logger;
using Shouldly;
using Testing;
using TransactionProcessor.BusinessLogic.Common;
using Xunit;

public class TransactionValidationServiceTests {
    private readonly TransactionValidationService TransactionValidationService;
    private readonly Mock<ISecurityServiceClient> SecurityServiceClient;

    private readonly Mock<IProjectionStateRepository<MerchantBalanceState>> StateRepository;
    private readonly Mock<IEventStoreContext> EventStoreContext;
    private readonly Mock<IAggregateRepository<EstateAggregate, DomainEvent>> EstateAggregateRepository;
    private readonly Mock<IAggregateRepository<MerchantAggregate, DomainEvent>> MerchantAggregateRepository;
    public TransactionValidationServiceTests() {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);

        Logger.Initialise(NullLogger.Instance);

        this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
        this.StateRepository = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
        this.EventStoreContext = new Mock<IEventStoreContext>();
        this.EstateAggregateRepository = new Mock<IAggregateRepository<EstateAggregate, DomainEvent>>();
        this.MerchantAggregateRepository = new Mock<IAggregateRepository<MerchantAggregate, DomainEvent>>();

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionValidationService = new TransactionValidationService(this.EventStoreContext.Object,
            this.EstateAggregateRepository.Object,
            this.MerchantAggregateRepository.Object);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_ValidationSuccessful_CorrectResponseReturned() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.Success);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_InvalidEstate_CorrectResponseReturned() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound("Estate not found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_FailureWhileGettingEstate_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Failed"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_InvalidMerchant_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound("Merchant Not Found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_FailureWhileGettingMerchant_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Merchant Not Found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_InvalidDeviceId_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier1, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_MerchantHasNoDevices_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());


        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier1, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.SuccessNeedToAddDevice);
    }
    
    [Fact]
    public async Task ValidateReconciliationTransactionX_ValidationSuccessful_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.Success);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_InvalidEstate_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound("Estate Not Found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_FailureWhileGettingEstate_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Failed"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_InvalidMerchant_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound("Merchant Not Found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_FailureWhileGettingMerchant_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Merchant Not Found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_InvalidDeviceId_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier1, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_MerchantHasNoDevices_CorrectResponseReturned()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier1, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier1,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateFoundButHasNoOperators_ResponseIsInvalidEstateId()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
        
        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.NoEstateOperators);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateFoundButOperatorIsDeleted_ResponseIsOperatorNotEnabledForEstate()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperatorDeleted()));
        
        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.OperatorNotEnabledForEstate);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateFoundButHasNullOperators_ResponseIsInvalidEstateId()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
        
        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.NoEstateOperators);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateFoundOperatorsNotConfiguredForEstate_ResponseIsOperatorNotValidForEstate()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        
        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId2,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.OperatorNotValidForEstate);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateNotFound_ResponseIsInvalidEstateId()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound("Estate Not Found"));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_GetEstateFailed_ResponseIsInvalidEstateId()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Failed"));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
            TestData.MerchantId,
            TestData.ContractId,
            TestData.ProductId,
            TestData.DeviceIdentifier,
            TestData.OperatorId,
            TestData.TransactionAmount,
            CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_InvalidContractId_ResponseIsInvalidContractIdValue()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        this.EventStoreContext.Setup(e => e.GetPartitionStateFromProjection(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(JsonConvert.SerializeObject(TestData.MerchantBalanceProjectionState));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  Guid.Empty,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidContractIdValue);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_InvalidProductId_ResponseIsInvalidProductIdValue()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        this.EventStoreContext.Setup(e => e.GetPartitionStateFromProjection(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonConvert.SerializeObject(TestData.MerchantBalanceProjectionState));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  Guid.Empty,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidProductIdValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task TransactionValidationService_ValidateSaleTransaction_InvalidTransactionAmount_ResponseIsInvalidSaleTransactionAmount(Decimal transactionAmount)
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  transactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidSaleTransactionAmount);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantDeviceListEmpty_ResponseIsNoValidDevices()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedMerchantAggregate());

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier1,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
    }
    
    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantDoesNotHaveSuppliedContract_ResponseIsContractNotValidForMerchant()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        this.EventStoreContext.Setup(e => e.GetPartitionStateFromProjection(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(JsonConvert.SerializeObject(TestData.MerchantBalanceProjectionState));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId1,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.ContractNotValidForMerchant);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantHasNoContracts_ResponseIsMerchantDoesNotHaveEnoughCredit()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithNoContracts(SettlementSchedule.Immediate));



        this.EventStoreContext.Setup(e => e.GetPartitionStateFromProjection(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(JsonConvert.SerializeObject(TestData.MerchantBalanceProjectionState));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.MerchantHasNoContractsConfigured);
    }
    
    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantNotEnoughCredit_ResponseIsMerchantDoesNotHaveEnoughCredit()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));
        this.EventStoreContext.Setup(e => e.GetPartitionStateFromProjection(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonConvert.SerializeObject(TestData.MerchantBalanceProjectionStateNoCredit));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.MerchantDoesNotHaveEnoughCredit);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantNotFound_ResponseIsInvalidMerchantId()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound("Merchant not found"));


        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_FailedGettingMerchant_ResponseIsInvalidMerchantId()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Merchant not found"));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact(Skip = "Need to review if test is needed now")]
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantNotFoundOnGetContract_ResponseIsInvalidMerchantId()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

        this.MerchantAggregateRepository.SetupSequence(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate))
            .ReturnsAsync(Result.NotFound());

        this.EventStoreContext.Setup(e => e.GetPartitionStateFromProjection(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(JsonConvert.SerializeObject(TestData.MerchantBalanceProjectionState));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
    }

    [Fact(Skip = "not sure if this needed now")]
    public async Task TransactionValidationService_ValidateSaleTransaction_FailedGettingMerchantOnGetContract_ResponseIsInvalidMerchantId()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.SetupSequence(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate))
            .ReturnsAsync(Result.NotFound());

        this.EventStoreContext.Setup(e => e.GetPartitionStateFromProjection(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(JsonConvert.SerializeObject(TestData.MerchantBalanceProjectionState));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantOperatorListEmpty_ResponseIsNoMerchantOperators()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

        this.MerchantAggregateRepository.SetupSequence(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithDevice());

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.NoMerchantOperators);
    }
    
    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantOperatorIsDeleted_ResponseIsOperatorNotEnabledForMerchant()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithDeletedOperator(SettlementSchedule.Immediate));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.OperatorNotEnabledForMerchant);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_OperatorNotConfiguredFroMerchant_ResponseIsOperatorNotValidForMerchant()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator2()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));


        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId2,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.OperatorNotValidForMerchant);
    }


    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_ProductIdNotConfigured_ResponseIsProductNotValidForMerchant()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        this.EventStoreContext.Setup(e => e.GetPartitionStateFromProjection(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(JsonConvert.SerializeObject(TestData.MerchantBalanceProjectionState));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId1,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.ProductNotValidForMerchant);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_SuccessfulSale()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.SetupSequence(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        this.EventStoreContext.Setup(e => e.GetPartitionStateFromProjection(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JsonConvert.SerializeObject(TestData.MerchantBalanceProjectionState));

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.Success);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_FailedGettingMerchantBalance_ResponseIsInvalidMerchantId()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate));

        this.EventStoreContext.Setup(e => e.GetPartitionStateFromProjection(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(Result.Failure());

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }
}

