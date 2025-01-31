using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services;

public class OperatorDomainServiceTests{

    private IOperatorDomainService OperatorDomainService;
    private Mock<IAggregateRepository<EstateAggregate, DomainEvent>> EstateAggregateRepository;
    private Mock<IAggregateRepository<OperatorAggregate, DomainEvent>> OperatorAggregateRepository;

    public OperatorDomainServiceTests(){
        this.EstateAggregateRepository = new Mock<IAggregateRepository<EstateAggregate, DomainEvent>>();
        this.OperatorAggregateRepository= new Mock<IAggregateRepository<OperatorAggregate, DomainEvent>>();
        this.OperatorDomainService = new OperatorDomainService(this.EstateAggregateRepository.Object,
                                                               this.OperatorAggregateRepository.Object);
    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_OperatorIsCreated(){
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.OperatorAggregateRepository.Setup(o => o.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));
        this.OperatorAggregateRepository
            .Setup(o => o.SaveChanges(It.IsAny<OperatorAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_EstateNotCreated_ResultFailed()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);
        this.OperatorAggregateRepository.Setup(o => o.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));

        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();

    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_OperatorAlreadyCreated_ResultFailed() {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

        this.OperatorAggregateRepository.Setup(o => o.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_UpdateOperator_OperatorIsUpdated()
    {
        this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
        this.OperatorAggregateRepository.Setup(o => o.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
        this.OperatorAggregateRepository
            .Setup(o => o.SaveChanges(It.IsAny<OperatorAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.OperatorDomainService.UpdateOperator(TestData.Commands.UpdateOperatorCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_UpdateOperator_OperatorNotCreated_ResultFailed()
    {
        this.OperatorAggregateRepository.Setup(o => o.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));

        Result result = await this.OperatorDomainService.UpdateOperator(TestData.Commands.UpdateOperatorCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }
}