using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using SecurityService.Client;
using SecurityService.DataTransferObjects;
using SecurityService.DataTransferObjects.Responses;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;
using Xunit;
using AssignOperatorRequest = TransactionProcessor.DataTransferObjects.Requests.Merchant.AssignOperatorRequest;

namespace TransactionProcessor.BusinessLogic.Tests.Services;

public class MerchantDomainServiceTests {
    private readonly Mock<IAggregateRepository<MerchantAggregate, DomainEvent>> MerchantAggregateRepository;

    //private readonly Mock<IAggregateRepository<MerchantDepositListAggregate, DomainEvent>>
    //    MerchantDepositListAggregateRepository;

    private readonly Mock<IAggregateRepository<EstateAggregate, DomainEvent>> EstateAggregateRepository;

    private readonly Mock<ISecurityServiceClient> SecurityServiceClient;

    //private readonly Mock<ITransactionProcessorClient> TransactionProcessorClient;

    private readonly Mock<IAggregateRepository<ContractAggregate, DomainEvent>> ContractAggregateRepository;

    private readonly MerchantDomainService DomainService;

    public MerchantDomainServiceTests() {
        IConfigurationRoot configurationRoot =
            new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);

        Logger.Initialise(new NullLogger());

        this.MerchantAggregateRepository = new Mock<IAggregateRepository<MerchantAggregate, DomainEvent>>();
        //this.MerchantDepositListAggregateRepository =
        //    new Mock<IAggregateRepository<MerchantDepositListAggregate, DomainEvent>>();
        this.EstateAggregateRepository = new Mock<IAggregateRepository<EstateAggregate, DomainEvent>>();
        this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
        //this.TransactionProcessorClient = new Mock<ITransactionProcessorClient>();
        this.ContractAggregateRepository = new Mock<IAggregateRepository<ContractAggregate, DomainEvent>>();
        this.DomainService = new MerchantDomainService(this.EstateAggregateRepository.Object,
            this.MerchantAggregateRepository.Object, //this.MerchantDepositListAggregateRepository.Object,
            this.ContractAggregateRepository.Object, this.SecurityServiceClient.Object);
    }

    [Theory]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate)]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly)]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly)]
    [InlineData(DataTransferObjects.Responses.Merchant.SettlementSchedule.NotSet)]
    public async Task MerchantDomainService_CreateMerchant_MerchantIsCreated(
        DataTransferObjects.Responses.Merchant.SettlementSchedule settlementSchedule) {
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        MerchantCommands.CreateMerchantCommand command = TestData.Commands.CreateMerchantCommand;

        command.RequestDto.SettlementSchedule = settlementSchedule;

        Result<Guid> result = await this.DomainService.CreateMerchant(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchant_MerchantIdNotSet_MerchantIsCreated() {
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        MerchantCommands.CreateMerchantCommand command = TestData.Commands.CreateMerchantCommand;
        command.RequestDto.MerchantId = null;

        Result<Guid> result = await this.DomainService.CreateMerchant(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchant_AlreadyCreated_MerchantIsCreated() {
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        Result<Guid> result =
            await this.DomainService.CreateMerchant(TestData.Commands.CreateMerchantCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result = await this.DomainService.CreateMerchant(TestData.Commands.CreateMerchantCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchant_EstateNotFound_ErrorThrown() {
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        Result<Guid> result =
            await this.DomainService.CreateMerchant(TestData.Commands.CreateMerchantCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AssignOperatorToMerchant_OperatorAssigned() {
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

        Result result =
            await this.DomainService.AssignOperatorToMerchant(TestData.Commands.AssignOperatorToMerchantCommand,
                CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AssignOperatorToMerchant_MerchantNotCreated_ErrorThrown() {
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

        Result result = await this.DomainService.AssignOperatorToMerchant(TestData.Commands.AssignOperatorToMerchantCommand,
            CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AssignOperatorToMerchant_EstateNotCreated_ErrorThrown() {
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        var result = await this.DomainService.AssignOperatorToMerchant(TestData.Commands.AssignOperatorToMerchantCommand,
                CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AssignOperatorToMerchant_OperatorNotFoundForEstate_ErrorThrown() {
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

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
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

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
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

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
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
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
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.SecurityServiceClient
            .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.CreateMerchantUser(TestData.Commands.CreateMerchantUserCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchantUser_MerchantNotCreated_ErrorThrown() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MerchantAggregate());
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.SecurityServiceClient
            .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.CreateMerchantUser(TestData.Commands.CreateMerchantUserCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_CreateMerchantUser_ErrorCreatingUser_ErrorThrown() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
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
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
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
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
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
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddDeviceToMerchant(TestData.Commands.AddMerchantDeviceCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddDeviceToMerchant_EstateNotCreated_ErrorThrown() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddDeviceToMerchant(TestData.Commands.AddMerchantDeviceCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddDeviceToMerchant_MerchantNotCreated_ErrorThrown() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddDeviceToMerchant(TestData.Commands.AddMerchantDeviceCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantDeposit_DepositIsMade()
    {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success());

        //var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        //result.IsSuccess.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantDeposit_GetDepositListFailed_ResultIsFailed()
    {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Failure());
        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success());

        //var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        //result.IsFailed.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantDeposit_DepositListSaveFailed_ResultIsFailed()
    {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Failure);

        //var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        //result.IsFailed.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantDeposit_EstateNotCreated_ErrorThrown() {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        //result.IsFailed.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantDeposit_MerchantNotCreated_ErrorThrown() {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        //result.IsFailed.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantDeposit_NoDepositsYet_DepositIsMade() {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantDepositListAggregate));
        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success());

        //var result = await this.DomainService.MakeMerchantDeposit(TestData.Commands.MakeMerchantDepositCommand, CancellationToken.None);
        //result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_SwapMerchantDevice_MerchantDeviceSwapped() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithDevice()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.SwapMerchantDevice(TestData.Commands.SwapMerchantDeviceCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_SwapMerchantDevice_MerchantNotCreated_ErrorThrown() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.SwapMerchantDevice(TestData.Commands.SwapMerchantDeviceCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_SwapMerchantDevice_EstateNotCreated_ErrorThrown() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithDevice()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.SwapMerchantDevice(TestData.Commands.SwapMerchantDeviceCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_WithdrawalIsMade() {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success());

        //this.SecurityServiceClient
        //    .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.TokenResponse()));

        //this.TransactionProcessorClient
        //    .Setup(t => t.GetMerchantBalance(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
        //        It.IsAny<CancellationToken>(), true)).ReturnsAsync(TestData.MerchantBalance);

        //var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        //result.IsSuccess.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_GetDepositListFailed_ResultIsFailed()
    {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Failure());
        
        //this.SecurityServiceClient
        //    .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.TokenResponse()));

        //this.TransactionProcessorClient
        //    .Setup(t => t.GetMerchantBalance(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
        //        It.IsAny<CancellationToken>(), true)).ReturnsAsync(TestData.MerchantBalance);

        //var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        //result.IsFailed.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_EstateNotCreated_ErrorThrown() {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success());

        //this.SecurityServiceClient
        //    .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.TokenResponse()));

        //this.TransactionProcessorClient
        //    .Setup(t => t.GetMerchantBalance(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
        //        It.IsAny<CancellationToken>(), true)).ReturnsAsync(TestData.MerchantBalance);

        //var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        //result.IsFailed.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_MerchantNotCreated_ErrorThrown() {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success());

        //this.SecurityServiceClient
        //    .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.TokenResponse()));

        //this.TransactionProcessorClient
        //    .Setup(t => t.GetMerchantBalance(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
        //        It.IsAny<CancellationToken>(), true)).ReturnsAsync(TestData.MerchantBalance);

        //var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        //result.IsFailed.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_MerchantDepositListNotCreated_ErrorThrown() {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantDepositListAggregate));
        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success());

        //this.SecurityServiceClient
        //    .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.TokenResponse()));

        //this.TransactionProcessorClient
        //    .Setup(t => t.GetMerchantBalance(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
        //        It.IsAny<CancellationToken>(), true)).ReturnsAsync(TestData.MerchantBalance);

        //var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        //result.IsFailed.ShouldBeTrue();
    }

    [Fact(Skip = "Needs MerchantDepositListAggregateRepository")]
    public async Task MerchantDomainService_MakeMerchantWithdrawal_NotEnoughFundsToWithdraw_ErrorThrown() {
        //this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        //this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        //this.MerchantAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success);

        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantDepositListAggregate()));
        //this.MerchantDepositListAggregateRepository
        //    .Setup(m => m.SaveChanges(It.IsAny<MerchantDepositListAggregate>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success());

        //this.SecurityServiceClient
        //    .Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>()))
        //    .ReturnsAsync(Result.Success(TestData.TokenResponse()));

        //this.TransactionProcessorClient
        //    .Setup(t => t.GetMerchantBalance(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
        //        It.IsAny<CancellationToken>(), true)).ReturnsAsync(TestData.MerchantBalanceNoAvailableBalance);

        //var result = await this.DomainService.MakeMerchantWithdrawal(TestData.Commands.MakeMerchantWithdrawalCommand, CancellationToken.None);
        //result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddContractToMerchant_ContractAdded() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(
                TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed,
                    FeeType.Merchant)));

        var result = await this.DomainService.AddContractToMerchant(TestData.Commands.AddMerchantContractCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddContractToMerchant_ContractNotCreated_ErrorThrown() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

        var result = await this.DomainService.AddContractToMerchant(TestData.Commands.AddMerchantContractCommand, CancellationToken.None); 
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddContractToMerchant_GetContractFailed_ErrorThrown()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure());

        var result = await this.DomainService.AddContractToMerchant(TestData.Commands.AddMerchantContractCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddContractToMerchant_MerchantNotCreated_ErrorThrown() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(
                TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed,
                    FeeType.Merchant)));

        var result = await this.DomainService.AddContractToMerchant(TestData.Commands.AddMerchantContractCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddContractToMerchant_EstateNotCreated_ErrorThrown() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(
                TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed,
                    FeeType.Merchant)));
        
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
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        MerchantCommands.UpdateMerchantCommand command = TestData.Commands.UpdateMerchantCommand;

        command.RequestDto.SettlementSchedule = settlementSchedule;

        var result = await this.DomainService.UpdateMerchant(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_UpdateMerchant_ValidationError_ResultIsFailed()
    {
        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        MerchantCommands.UpdateMerchantCommand command = TestData.Commands.UpdateMerchantCommand;

        var result = await this.DomainService.UpdateMerchant(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddMerchantAddress_AddressAdded() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddMerchantAddress(TestData.Commands.AddMerchantAddressCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddMerchantAddress_ValidationFailure_ResultIsFailed()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddMerchantAddress(TestData.Commands.AddMerchantAddressCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_UpdateMerchantAddress_AddressUpdated() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.UpdateMerchantAddress(TestData.Commands.UpdateMerchantAddressCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_UpdateMerchantAddress_ValidationFailed_ResultIsFailed()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.UpdateMerchantAddress(TestData.Commands.UpdateMerchantAddressCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddMerchantContact_ContactAdded() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddMerchantContact(TestData.Commands.AddMerchantContactCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_AddMerchantContact_ValidationError_ResultIsFailed()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.AddMerchantContact(TestData.Commands.AddMerchantContactCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_UpdateMerchantContact_ContactUpdated() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.UpdateMerchantContact(TestData.Commands.UpdateMerchantContactCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_UpdateMerchantContact_ValidationError_ResultIsFailed()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedMerchantAggregate()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.UpdateMerchantContact(TestData.Commands.UpdateMerchantContactCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_RemoveOperatorFromMerchant_OperatorRemoved() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithOperator()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.RemoveOperatorFromMerchant(TestData.Commands.RemoveOperatorFromMerchantCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_RemoveOperatorFromMerchant_ValidationFailed_ResultFailed()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithOperator()));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.RemoveOperatorFromMerchant(TestData.Commands.RemoveOperatorFromMerchantCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_RemoveContractFromMerchant_ContractRemoved() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.RemoveContractFromMerchant(TestData.Commands.RemoveMerchantContractCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantDomainService_RemoveContractFromMerchant_ValidationFailed_ResultFailed()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

        this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));
        this.MerchantAggregateRepository
            .Setup(m => m.SaveChanges(It.IsAny<MerchantAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await this.DomainService.RemoveContractFromMerchant(TestData.Commands.RemoveMerchantContractCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }
}