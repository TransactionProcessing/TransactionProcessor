using Imposter.Abstractions;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.DataTransferObjects.Requests.Operator;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services;

public class OperatorDomainServiceTests{

    private IOperatorDomainService OperatorDomainService;
    private IAggregateServiceImposter AggregateService;

    public OperatorDomainServiceTests(){
        this.AggregateService = new IAggregateServiceImposter();
        IAggregateService AggregateServiceResolver() => this.AggregateService.Instance();
        this.OperatorDomainService = new OperatorDomainService(AggregateServiceResolver);
        StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_OperatorIsCreated(){
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                 .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));
        this.AggregateService.Save(Arg<OperatorAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_OperatorIdIsEmpty_OperatorIsCreated()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());
        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));
        this.AggregateService.Save(Arg<OperatorAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        OperatorCommands.CreateOperatorCommand emptyIdCommand = new(TestData.EstateId,
            new CreateOperatorRequest {
                OperatorId = Guid.Empty,
                Name = TestData.OperatorName,
                RequireCustomMerchantNumber = TestData.RequireCustomMerchantNumber,
                RequireCustomTerminalNumber = TestData.RequireCustomTerminalNumber
            });

        Result result = await this.OperatorDomainService.CreateOperator(emptyIdCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_EstateNotCreated_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);
        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));

        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();

    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_OperatorAlreadyCreated_ResultFailed() {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_OperatorNameEmpty_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());
        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));

        OperatorCommands.CreateOperatorCommand emptyNameCommand = new(TestData.EstateId,
            new CreateOperatorRequest {
                OperatorId = TestData.OperatorId,
                Name = string.Empty,
                RequireCustomMerchantNumber = TestData.RequireCustomMerchantNumber,
                RequireCustomTerminalNumber = TestData.RequireCustomTerminalNumber
            });

        Result result = await this.OperatorDomainService.CreateOperator(emptyNameCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_UpdateOperator_OperatorIsUpdated()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());
        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
        this.AggregateService.Save(Arg<OperatorAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
        Result result = await this.OperatorDomainService.UpdateOperator(TestData.Commands.UpdateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_UpdateOperator_OperatorNotCreated_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());
        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));

        Result result = await this.OperatorDomainService.UpdateOperator(TestData.Commands.UpdateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_GetEstateFailed_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure());

        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_GetOperatorFailed_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());
        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure());

        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_SaveFailed_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());
        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));
        this.AggregateService.Save(Arg<OperatorAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure());

        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_ExceptionThrown_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ThrowsAsync(new Exception());

        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_UpdateOperator_GetEstateFailed_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure());

        Result result = await this.OperatorDomainService.UpdateOperator(TestData.Commands.UpdateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_UpdateOperator_EstateNotCreated_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);
        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

        Result result = await this.OperatorDomainService.UpdateOperator(TestData.Commands.UpdateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_UpdateOperator_GetOperatorFailed_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());
        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure());

        Result result = await this.OperatorDomainService.UpdateOperator(TestData.Commands.UpdateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_UpdateOperator_SaveFailed_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());
        this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
        this.AggregateService.Save(Arg<OperatorAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure());

        Result result = await this.OperatorDomainService.UpdateOperator(TestData.Commands.UpdateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_UpdateOperator_ExceptionThrown_ResultFailed()
    {
        this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ThrowsAsync(new Exception());

        Result result = await this.OperatorDomainService.UpdateOperator(TestData.Commands.UpdateOperatorCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }
}








