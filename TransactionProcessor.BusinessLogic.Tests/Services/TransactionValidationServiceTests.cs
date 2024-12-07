using SimpleResults;

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
using Xunit;

/*
public class TransactionValidationServiceTests{
    private readonly TransactionValidationService TransactionValidationService;
    private readonly Mock<ISecurityServiceClient> SecurityServiceClient;

    private readonly Mock<IProjectionStateRepository<MerchantBalanceState>> StateRepository;
    private readonly Mock<IEstateClient> EstateClient;
    private readonly Mock<IEventStoreContext> EventStoreContext;

    public TransactionValidationServiceTests(){
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);

        Logger.Initialise(NullLogger.Instance);

        this.EstateClient = new Mock<IEstateClient>();
        this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
        this.StateRepository = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
        this.EventStoreContext = new Mock<IEventStoreContext>();

        this.TransactionValidationService = new TransactionValidationService(this.EstateClient.Object,
                                                                             this.SecurityServiceClient.Object,
                                                                             this.StateRepository.Object,
                                                                             this.EventStoreContext.Object);
    }
    
    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

        Result<TransactionValidationResult> result=
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier1, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
    }
    
    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_EstateNotFound_ResponseIsInvalidEstateId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());

        Result<TransactionValidationResult> result =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_MerchantDeviceListEmpty_SuccessfulLogon() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);

        var result =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.SuccessNeedToAddDevice);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_MerchantDeviceListNull_SuccessfulLogon() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);
        // TODO: Verify device was added...

        var result =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.SuccessNeedToAddDevice);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_MerchantNotFound_ResponseIsInvalidMerchantId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient
            .Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

        var result =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

            result.IsFailed.ShouldBeTrue();
            result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_SuccessfulLogon() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

        var result =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.Success);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

        var result =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier1,
                                                                                      CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_EstateNotFound_ResponseIsInvalidEstateId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());

        var result =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_MerchantDeviceListEmpty_ResponseIsResponseIsNoValidDevices() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);

        var result =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_MerchantDeviceListNull_ResponseIsResponseIsNoValidDevices() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);

        var response =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_MerchantNotFound_ResponseIsInvalidMerchantId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient
            .Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_SuccessfulReconciliation() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.Success);
    }

    
}*/

public class TransactionValidationServiceTests_New {
    private readonly TransactionValidationService TransactionValidationService;
    private readonly Mock<ISecurityServiceClient> SecurityServiceClient;

    private readonly Mock<IProjectionStateRepository<MerchantBalanceState>> StateRepository;
    private readonly Mock<IEstateClient> EstateClient;
    private readonly Mock<IEventStoreContext> EventStoreContext;
    public TransactionValidationServiceTests_New() {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);

        Logger.Initialise(NullLogger.Instance);

        this.EstateClient = new Mock<IEstateClient>();
        this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
        this.StateRepository = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
        this.EventStoreContext = new Mock<IEventStoreContext>();

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionValidationService = new TransactionValidationService(this.EstateClient.Object,
            this.SecurityServiceClient.Object,
            this.StateRepository.Object,
            this.EventStoreContext.Object);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_ValidationSuccessful_CorrectResponseReturned() {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEstateResponseWithOperator1));
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantResponseWithOperator1));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.Success);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_InvalidEstate_CorrectResponseReturned() {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound("Estate not found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_FailureWhileGettingEstate_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Failure Message"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_InvalidMerchant_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEstateResponseWithOperator1));
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound("Merchant Not Found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_FailureWhileGettingMerchant_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Merchant Not Found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_InvalidDeviceId_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEstateResponseWithOperator1));
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantResponseWithOperator1));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier1, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_MerchantHasNoDevices_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEstateResponseWithOperator1));
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantResponseWithNoDevices));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier1, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.SuccessNeedToAddDevice);
    }

    [Fact]
    public async Task ValidateLogonTransactionX_MerchantHasNullDevices_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEstateResponseWithOperator1));
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantResponseWithNullDevices));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier1, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.SuccessNeedToAddDevice);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_ValidationSuccessful_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEstateResponseWithOperator1));
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantResponseWithOperator1));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.Success);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_InvalidEstate_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound("Estate not found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_FailureWhileGettingEstate_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Failure Message"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_InvalidMerchant_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEstateResponseWithOperator1));
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound("Merchant Not Found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_FailureWhileGettingMerchant_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Merchant Not Found"));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_InvalidDeviceId_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEstateResponseWithOperator1));
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantResponseWithOperator1));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier1, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_MerchantHasNoDevices_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEstateResponseWithOperator1));
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantResponseWithNoDevices));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier1, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
    }

    [Fact]
    public async Task ValidateReconciliationTransactionX_MerchantHasNullDevices_CorrectResponseReturned()
    {
        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEstateResponseWithOperator1));
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantResponseWithNullDevices));

        Result<TransactionValidationResult> result = await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId, TestData.MerchantId,
            TestData.DeviceIdentifier1, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithEmptyOperators);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEmptyMerchantResponse);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1Deleted);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEmptyMerchantResponse);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithNullOperators);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEmptyMerchantResponse);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEmptyMerchantResponse);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_InvalidContractId_ResponseIsInvalidContractIdValue()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantContractResponses);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantContractResponses);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);

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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantDeviceListNull_ResponseIsNoValidDevices()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantContractResponses);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.SetupSequence(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1)
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1AndEmptyContracts);


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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantHasNullContracts_ResponseIsMerchantDoesNotHaveEnoughCredit()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.SetupSequence(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1)
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1AndNullContracts);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantContractResponses);
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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantNotFoundOnGetContract_ResponseIsInvalidMerchantId()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.SetupSequence(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetMerchantResponseWithOperator1)
            .ReturnsAsync(Result.NotFound());
        //this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

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

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_FailedGettingMerchantOnGetContract_ResponseIsInvalidMerchantId()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.SetupSequence(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetMerchantResponseWithOperator1)
            .ReturnsAsync(Result.Failure());
        //this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithEmptyOperators);

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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantOperatorListNull_ResponseIsNoMerchantOperators()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNullOperators);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1Deleted);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator2);

        var result = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorId,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe(TransactionResponseCode.OperatorNotValidForMerchant);
    }


    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_ProductIdNotConfigured_ResponseIsProductNotValidForMerchant()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantContractResponses);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantContractResponses);

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

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.SetupSequence(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetMerchantResponseWithOperator1)
            .ReturnsAsync(Result.Success(TestData.GetMerchantResponseWithOperator1));
        
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

