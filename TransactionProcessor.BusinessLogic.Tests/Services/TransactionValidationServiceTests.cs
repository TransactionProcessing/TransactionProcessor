namespace TransactionProcessor.BusinessLogic.Tests.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using EstateManagement.Client;
using EstateManagement.DataTransferObjects.Responses;
using Microsoft.Extensions.Configuration;
using Moq;
using ProjectionEngine.Repository;
using ProjectionEngine.State;
using SecurityService.Client;
using Shared.General;
using Shared.Logger;
using Shouldly;
using Testing;
using Xunit;

public class TransactionValidationServiceTests{
    private readonly TransactionValidationService TransactionValidationService;
    private readonly Mock<ISecurityServiceClient> SecurityServiceClient;

    private readonly Mock<IProjectionStateRepository<MerchantBalanceState>> StateRepository;
    private readonly Mock<IEstateClient> EstateClient;

    public TransactionValidationServiceTests(){
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);

        Logger.Initialise(NullLogger.Instance);

        this.EstateClient = new Mock<IEstateClient>();
        this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
        this.StateRepository = new Mock<IProjectionStateRepository<MerchantBalanceState>>();

        this.TransactionValidationService = new TransactionValidationService(this.EstateClient.Object,
                                                                             this.SecurityServiceClient.Object,
                                                                             this.StateRepository.Object);
    }
    
    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier1, CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
    }
        
    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_EstateClientGetEstateThrewOtherException_ResponseIsUnknownFailure() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception"));

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_EstateClientGetMerchantThrewOtherException_ResponseIsUnknownFailure() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Exception"));

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }
        
    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_EstateNotFound_ResponseIsInvalidEstateId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_MerchantDeviceListEmpty_SuccessfulLogon() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.SuccessNeedToAddDevice);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_MerchantDeviceListNull_SuccessfulLogon() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);
        // TODO: Verify device was added...

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.SuccessNeedToAddDevice);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_MerchantNotFound_ResponseIsInvalidMerchantId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateLogonTransaction_SuccessfulLogon() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateLogonTransaction(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.Success);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier1,
                                                                                      CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.InvalidDeviceIdentifier);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_EstateClientGetEstateThrewOtherException_ResponseIsUnknownFailure() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception"));

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_EstateClientGetMerchantThrewOtherException_ResponseIsUnknownFailure() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Exception"));

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.UnknownFailure);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_EstateNotFound_ResponseIsInvalidEstateId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.InvalidEstateId);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_MerchantDeviceListEmpty_ResponseIsResponseIsNoValidDevices() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_MerchantDeviceListNull_ResponseIsResponseIsNoValidDevices() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.NoValidDevices);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_MerchantNotFound_ResponseIsInvalidMerchantId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

        (String responseMessage, TransactionResponseCode responseCode) response =
            await this.TransactionValidationService.ValidateReconciliationTransaction(TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.DeviceIdentifier,
                                                                                      CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.InvalidMerchantId);
    }

    [Fact]
    public async Task TransactionValidationService_ValidateReconciliationTransaction_SuccessfulReconciliation() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

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

    [Fact]
    public async Task TransactionValidationService_ValidateSaleTransaction_DeviceNotRegisteredToMerchant_ResponseIsInvalidDeviceIdentifier() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateClientGetEstateThrewOtherException_ResponseIsUnknownFailure() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Exception"));

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateClientGetMerchantThrewOtherException_ResponseIsUnknownFailure() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Exception"));

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateClientThrownOtherExceptionFoundOnGetContract_ResponseIsUnknownFailure() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Exception"));

        this.StateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantBalanceProjectionState);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateFoundButHasNoOperators_ResponseIsInvalidEstateId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithEmptyOperators);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateFoundButHasNullOperators_ResponseIsInvalidEstateId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithNullOperators);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateFoundOperatorsNotConfiguredForEstate_ResponseIsOperatorNotValidForEstate() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_EstateNotFound_ResponseIsInvalidEstateId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_InvalidContractId_ResponseIsInvalidContractIdValue() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

        this.StateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantBalanceProjectionState);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_InvalidProductId_ResponseIsInvalidProductIdValue() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantContractResponses);

        this.StateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantBalanceProjectionState);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_InvalidTransactionAmount_ResponseIsInvalidSaleTransactionAmount(Decimal transactionAmount) {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantDeviceListEmpty_ResponseIsNoValidDevices() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNoDevices);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantDeviceListNull_ResponseIsNoValidDevices() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNullDevices);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantDoesNotHaveSuppliedContract_ResponseIsContractNotValidForMerchant() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantContractResponses);

        this.StateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantBalanceProjectionState);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantHasNoContracts_ResponseIsMerchantDoesNotHaveEnoughCredit() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContractResponse>());

        this.StateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantBalanceProjectionState);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantHasNullContracts_ResponseIsMerchantDoesNotHaveEnoughCredit() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        List<ContractResponse> contracts = null;
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contracts);

        this.StateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantBalanceProjectionState);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantNotEnoughCredit_ResponseIsMerchantDoesNotHaveEnoughCredit() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

        this.StateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantBalanceProjectionStateNoCredit);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantNotFound_ResponseIsInvalidMerchantId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantNotFoundOnGetContract_ResponseIsInvalidMerchantId() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Merchant")));

        this.StateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantBalanceProjectionState);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantOperatorListEmpty_ResponseIsNoMerchantOperators() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithEmptyOperators);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_MerchantOperatorListNull_ResponseIsNoMerchantOperators() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithNullOperators);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_OperatorNotConfiguredFroMerchant_ResponseIsOperatorNotValidForMerchant() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator2);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_ProductIdNotConfigured_ResponseIsProductNotValidForMerchant() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantContractResponses);

        this.StateRepository.Setup(p => p.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantBalanceProjectionState);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
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
    public async Task TransactionValidationService_ValidateSaleTransaction_SuccessfulSale() {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEstateResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator1);
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantContractResponses);

        this.StateRepository.Setup(s => s.Load(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.MerchantBalanceProjectionState);

        (String responseMessage, TransactionResponseCode responseCode) response = await this.TransactionValidationService.ValidateSaleTransaction(TestData.EstateId,
                                                                                                                                                  TestData.MerchantId,
                                                                                                                                                  TestData.ContractId,
                                                                                                                                                  TestData.ProductId,
                                                                                                                                                  TestData.DeviceIdentifier,
                                                                                                                                                  TestData.OperatorIdentifier1,
                                                                                                                                                  TestData.TransactionAmount,
                                                                                                                                                  CancellationToken.None);

        response.responseCode.ShouldBe(TransactionResponseCode.Success);
    }
}