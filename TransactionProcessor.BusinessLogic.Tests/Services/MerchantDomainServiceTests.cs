using MessagingService.Client;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using SecurityService.Client;
using SecurityService.DataTransferObjects;
using SecurityService.DataTransferObjects.Responses;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventStore;
using Shared.General;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessagingService.DataTransferObjects;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;
using Xunit;
using AssignOperatorRequest = TransactionProcessor.DataTransferObjects.Requests.Merchant.AssignOperatorRequest;

namespace TransactionProcessor.BusinessLogic.Tests.Services;

public class MerchantDomainServiceTests {
    private readonly Mock<IAggregateService> AggregateService;
    
    private readonly Mock<ISecurityServiceClient> SecurityServiceClient;

    private readonly MerchantDomainService DomainService;
    private readonly Mock<IEventStoreContext> EventStoreContext;

    public MerchantDomainServiceTests() {
        IConfigurationRoot configurationRoot =
            new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);

        Logger.Initialise(new NullLogger());

        this.AggregateService = new Mock<IAggregateService>();
        this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
        this.EventStoreContext = new Mock<IEventStoreContext>();
        this.DomainService = new MerchantDomainService(this.AggregateService.Object,
            this.SecurityServiceClient.Object, this.EventStoreContext.Object);
    }

    [Theory]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate)]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly)]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly)]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.NotSet)]
    public async Task MerchantDomainService_CreateMerchant_MerchantIsCreated(
        DataTransferObjects.Responses.Merchant.SettlementSchedule settlementSchedule) {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        MerchantCommands.CreateMerchantCommand command = TestData.Commands.CreateMerchantCommand;

        command.RequestDto.SettlementSchedule = settlementSchedule;

        Result<Guid> result = await this.DomainService.CreateMerchant(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchant_MerchantIdNotSet_MerchantIsCreated() {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        MerchantCommands.CreateMerchantCommand command = TestData.Commands.CreateMerchantCommand;
        command.RequestDto.MerchantId = null;

        Result<Guid> result = await this.DomainService.CreateMerchant(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchant_AlreadyCreated_MerchantIsCreated() {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        Result<Guid> result =
            await this.DomainService.CreateMerchant(TestData.Commands.CreateMerchantCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result = await this.DomainService.CreateMerchant(TestData.Commands.CreateMerchantCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchant_EstateNotFound_ErrorThrown() {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        Result<Guid> result =
            await this.DomainService.CreateMerchant(TestData.Commands.CreateMerchantCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AssignOperatorToMerchant_OperatorAssigned() {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());

        Result result =
            await this.DomainService.AssignOperatorToMerchant(TestData.Commands.AssignOperatorToMerchantCommand,
                CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AssignOperatorToMerchant_MerchantNotCreated_ErrorThrown() {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());

        Result result = await this.DomainService.AssignOperatorToMerchant(TestData.Commands.AssignOperatorToMerchantCommand,
            CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AssignOperatorToMerchant_EstateNotCreated_ErrorThrown() {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        var result = await this.DomainService.AssignOperatorToMerchant(TestData.Commands.AssignOperatorToMerchantCommand,
                CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AssignOperatorToMerchant_OperatorNotFoundForEstate_ErrorThrown() {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        var result = await this.DomainService.AssignOperatorToMerchant(TestData.Commands.AssignOperatorToMerchantCommand,
                CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    // TODO: Reintroduce when we have an Operator Aggregate
    // https://github.com/TransactionProcessing/EstateManagement/issues/558
    [Theory(Skip = "Needs Operator Aggregate")]
    [InlineData(null)]
    [InlineData("")]
    public async Task MerchantDomainService_AssignOperatorToMerchant_OperatorRequiresMerchantNumber_MerchantNumberNotSet_ErrorThrown(
            String merchantNumber) {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());

        MerchantCommands.AssignOperatorToMerchantCommand command = new(TestData.EstateId, TestData.MerchantId,
            new DataTransferObjects.Requests.Merchant.AssignOperatorRequest() {
                MerchantNumber = merchantNumber,
                OperatorId = TestData.OperatorId,
                TerminalNumber = TestData.TerminalNumber
            });

        var result = await this.DomainService.AssignOperatorToMerchant(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    // TODO: Reintroduce when we have an Operator Aggregate
    // https://github.com/TransactionProcessing/EstateManagement/issues/558
    [Theory(Skip = "Needs Operator Aggregate")]
    [InlineData(null)]
    [InlineData("")]
    public async Task MerchantDomainService_AssignOperatorToMerchant_OperatorRequiresTerminalNumber_TerminalNumberNotSet_ErrorThrown(
            String terminalNumber) {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());

        MerchantCommands.AssignOperatorToMerchantCommand command = new(TestData.EstateId, TestData.MerchantId,
            new AssignOperatorRequest() {
                MerchantNumber = TestData.MerchantNumber,
                OperatorId = TestData.OperatorId,
                TerminalNumber = terminalNumber
            });

        var result = await this.DomainService.AssignOperatorToMerchant(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchantUser_MerchantUserIsCreated() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.SecurityServiceClient
            .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.SecurityServiceClient
            .Setup(s => s.GetUsers(It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<UserDetails>() {
                new UserDetails {
                    UserId = Guid.Parse("FA077CE3-B915-4048-88E3-9B500699317F")
                }
            }));

        var result = await this.DomainService.CreateMerchantUser(TestData.Commands.CreateMerchantUserCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchantUser_EstateNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.SecurityServiceClient
            .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.CreateMerchantUser(TestData.Commands.CreateMerchantUserCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchantUser_MerchantNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MerchantAggregate());
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.SecurityServiceClient
            .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.CreateMerchantUser(TestData.Commands.CreateMerchantUserCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchantUser_ErrorCreatingUser_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.SecurityServiceClient
            .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure);

        var result = await this.DomainService.CreateMerchantUser(TestData.Commands.CreateMerchantUserCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchantUser_ErrorGettingUsers_ErrorThrown()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.SecurityServiceClient
            .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.SecurityServiceClient
            .Setup(s => s.GetUsers(It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure());

        var result = await this.DomainService.CreateMerchantUser(TestData.Commands.CreateMerchantUserCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact] 
    public async Task MerchantDomainService_CreateMerchantUser_UserInListIsNull_ErrorThrown()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.SecurityServiceClient
            .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.SecurityServiceClient
            .Setup(s => s.GetUsers(It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<UserDetails> {
                null
            }));

        var result = await this.DomainService.CreateMerchantUser(TestData.Commands.CreateMerchantUserCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddDeviceToMerchant_MerchantDeviceIsAdded() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddDeviceToMerchant(TestData.Commands.AddMerchantDeviceCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddDeviceToMerchant_EstateNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddDeviceToMerchant(TestData.Commands.AddMerchantDeviceCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddDeviceToMerchant_MerchantNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddDeviceToMerchant(TestData.Commands.AddMerchantDeviceCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantDeposit_DepositIsMade()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService
            .Setup(m => m.GetLatest<MerchantDepositListAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantDeposit_GetDepositListFailed_ResultIsFailed()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService
            .Setup(m => m.GetLatest<MerchantDepositListAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure());
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantDeposit_DepositListSaveFailed_ResultIsFailed()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService
            .Setup(m => m.GetLatest<MerchantDepositListAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure);

        var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantDeposit_EstateNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantDeposit_MerchantNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantDeposit_NoDepositsYet_DepositIsMade() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService
            .Setup(m => m.GetLatest<MerchantDepositListAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantDepositListAggregate));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_SwapMerchantDevice_MerchantDeviceSwapped() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithDevice()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.SwapMerchantDevice(TestData.Commands.SwapMerchantDeviceCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_SwapMerchantDevice_MerchantNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.SwapMerchantDevice(TestData.Commands.SwapMerchantDeviceCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_SwapMerchantDevice_EstateNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithDevice()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.SwapMerchantDevice(TestData.Commands.SwapMerchantDeviceCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_WithdrawalIsMade() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService
            .Setup(m => m.GetLatest<MerchantDepositListAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        this.SecurityServiceClient
            .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EventStoreContext.Setup(e => e.GetPartitionStateFromProjection(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success<String>(JsonConvert.SerializeObject(TestData.MerchantBalanceProjectionState)));

        var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_GetDepositListFailed_ResultIsFailed()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService
            .Setup(m => m.GetLatest<MerchantDepositListAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure());

        this.SecurityServiceClient
            .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.TokenResponse()));

        var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_EstateNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService
            .Setup(m => m.GetLatest<MerchantDepositListAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        this.SecurityServiceClient
            .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.TokenResponse()));
        
        var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_MerchantNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService
            .Setup(m => m.GetLatest<MerchantDepositListAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        this.SecurityServiceClient
            .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.TokenResponse()));

        var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_MerchantDepositListNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService
            .Setup(m => m.GetLatest<MerchantDepositListAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantDepositListAggregate));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        this.SecurityServiceClient
            .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.TokenResponse()));
        
        var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_NotEnoughFundsToWithdraw_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService
            .Setup(m => m.GetLatest<MerchantDepositListAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        this.SecurityServiceClient
            .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.TokenResponse()));

        var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddContractToMerchant_ContractAdded() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.AggregateService.Setup(c => c.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed,
                    FeeType.Merchant));

        var result = await this.DomainService.AddContractToMerchant(TestData.Commands.AddMerchantContractCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddContractToMerchant_ContractNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.AggregateService.Setup(c => c.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyContractAggregate());

        var result = await this.DomainService.AddContractToMerchant(TestData.Commands.AddMerchantContractCommand, CancellationToken.None); 
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddContractToMerchant_GetContractFailed_ErrorThrown()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.AggregateService.Setup(c => c.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());

        var result = await this.DomainService.AddContractToMerchant(TestData.Commands.AddMerchantContractCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddContractToMerchant_MerchantNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.AggregateService.Setup(c => c.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed,
                    FeeType.Merchant));

        var result = await this.DomainService.AddContractToMerchant(TestData.Commands.AddMerchantContractCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddContractToMerchant_EstateNotCreated_ErrorThrown() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.AggregateService.Setup(c => c.Get<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed,
                    FeeType.Merchant));
        
        var result = await this.DomainService.AddContractToMerchant(TestData.Commands.AddMerchantContractCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Theory]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate)]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly)]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly)]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.NotSet)]
    public async Task MerchantDomainService_UpdateMerchant_MerchantIsUpdated(
        DataTransferObjects.Responses.Merchant.SettlementSchedule settlementSchedule) {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        MerchantCommands.UpdateMerchantCommand command = TestData.Commands.UpdateMerchantCommand;

        command.RequestDto.SettlementSchedule = settlementSchedule;

        var result = await this.DomainService.UpdateMerchant(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_UpdateMerchant_ValidationError_ResultIsFailed()
    {
        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        MerchantCommands.UpdateMerchantCommand command = TestData.Commands.UpdateMerchantCommand;

        var result = await this.DomainService.UpdateMerchant(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddMerchantAddress_AddressAdded() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddMerchantAddress(TestData.Commands.AddMerchantAddressCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddMerchantAddress_ValidationFailure_ResultIsFailed()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddMerchantAddress(TestData.Commands.AddMerchantAddressCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_UpdateMerchantAddress_AddressUpdated() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.UpdateMerchantAddress(TestData.Commands.UpdateMerchantAddressCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_UpdateMerchantAddress_ValidationFailed_ResultIsFailed()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.UpdateMerchantAddress(TestData.Commands.UpdateMerchantAddressCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddMerchantContact_ContactAdded() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddMerchantContact(TestData.Commands.AddMerchantContactCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddMerchantContact_ValidationError_ResultIsFailed()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddMerchantContact(TestData.Commands.AddMerchantContactCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_UpdateMerchantContact_ContactUpdated() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.UpdateMerchantContact(TestData.Commands.UpdateMerchantContactCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_UpdateMerchantContact_ValidationError_ResultIsFailed()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.UpdateMerchantContact(TestData.Commands.UpdateMerchantContactCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_RemoveOperatorFromMerchant_OperatorRemoved() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithOperator()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.RemoveOperatorFromMerchant(TestData.Commands.RemoveOperatorFromMerchantCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_RemoveOperatorFromMerchant_ValidationFailed_ResultFailed()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithOperator()));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.RemoveOperatorFromMerchant(TestData.Commands.RemoveOperatorFromMerchantCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_RemoveContractFromMerchant_ContractRemoved() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.RemoveContractFromMerchant(TestData.Commands.RemoveMerchantContractCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_RemoveContractFromMerchant_ValidationFailed_ResultFailed()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

        this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));
        this.AggregateService
            .Setup(m => m.Save(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.RemoveContractFromMerchant(TestData.Commands.RemoveMerchantContractCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }
}

public class MerchantStatementDomainServiceTests {

    private Mock<IAggregateService> AggregateService;
    private Mock<IStatementBuilder> StatementBuilder;
    private Mock<IMessagingServiceClient> MessagingServiceClient;
    private Mock<ISecurityServiceClient> SecurityServiceClient;
    private MerchantStatementDomainService DomainService;
    public MerchantStatementDomainServiceTests() {
        this.AggregateService = new Mock<IAggregateService>();
        this.StatementBuilder = new Mock<IStatementBuilder>();
        this.MessagingServiceClient = new Mock<IMessagingServiceClient>();
        this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
        this.DomainService = new MerchantStatementDomainService(this.AggregateService.Object, this.StatementBuilder.Object, this.MessagingServiceClient.Object, this.SecurityServiceClient.Object);

        IConfigurationRoot configurationRoot =
            new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(new NullLogger());
    }
    
    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_TransactionAddedToStatement() {

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionToMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_TransactionNotAuthorised_TransactionNotAddedToStatement()
    {

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionNotAuthorisedToMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_TransactionHasNotAmount_TransactionNotAddedToStatement()
    {

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionWithNoAmountToMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_SaveFailed_TransactionNotAddedToStatement()
    {

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionToMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddSettledFeeToStatement_SettledFeeAddedToStatement()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddSettledFeeToStatement(TestData.Commands.AddSettledFeeToMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddSettledFeeToStatement_SaveFailed_SettledFeeNotAddedToStatement()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);
        Result result = await this.DomainService.AddSettledFeeToStatement(TestData.Commands.AddSettledFeeToMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }


    [Fact]
    public async Task MerchantStatementDomainService_RecordActivityDateOnMerchantStatement_SaveFailed_ActivityDateNotRecorded() {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
        Result result = await this.DomainService.RecordActivityDateOnMerchantStatement(TestData.Commands.RecordActivityDateOnMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_RecordActivityDateOnMerchantStatement_ActivityDateRecorded()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);
        Result result = await this.DomainService.RecordActivityDateOnMerchantStatement(TestData.Commands.RecordActivityDateOnMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_GenerateStatement_StatementIsGenerated()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementAggregateWithActivityDates()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        Result result = await this.DomainService.GenerateStatement(TestData.Commands.GenerateMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_GenerateStatement_GetStatementForDateFailed_StatementIsNotGenerated()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementAggregateWithActivityDates()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

        Result result = await this.DomainService.GenerateStatement(TestData.Commands.GenerateMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_GenerateStatement_SaveFailed_StatementIsNotGenerated()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementAggregateWithActivityDates()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        Result result = await this.DomainService.GenerateStatement(TestData.Commands.GenerateMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_StatementIsBuilt()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.StatementBuilder.Setup(s => s.GetStatementHtml(It.IsAny<MerchantStatementAggregate>(), It.IsAny<Merchant>(), It.IsAny<CancellationToken>())).ReturnsAsync("<html></html>");

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_GetMerchantFailed_StatementIsNotBuilt()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

        this.StatementBuilder.Setup(s => s.GetStatementHtml(It.IsAny<MerchantStatementAggregate>(), It.IsAny<Merchant>(), It.IsAny<CancellationToken>())).ReturnsAsync("<html></html>");

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_SaveFailed_StatementIsNotBuilt()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.StatementBuilder.Setup(s => s.GetStatementHtml(It.IsAny<MerchantStatementAggregate>(), It.IsAny<Merchant>(), It.IsAny<CancellationToken>())).ReturnsAsync("<html></html>");

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_StatementIsEmailed()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.BuiltMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(), It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.SecurityServiceClient.Setup(m => m.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_MerchantNotFound_StatementIsNotEmailed()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.BuiltMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

        this.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(), It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.SecurityServiceClient.Setup(m => m.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_GetTokenFailed_StatementIsNotEmailed()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.BuiltMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(), It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.SecurityServiceClient.Setup(m => m.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }


}