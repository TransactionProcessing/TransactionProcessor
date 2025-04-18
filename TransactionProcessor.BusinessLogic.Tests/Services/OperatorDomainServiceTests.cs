﻿using System;
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
    private Mock<IAggregateService> AggregateService;

    public OperatorDomainServiceTests(){
        this.AggregateService = new Mock<IAggregateService>();
        this.OperatorDomainService = new OperatorDomainService(this.AggregateService.Object);
    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_OperatorIsCreated(){
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(o => o.GetLatest<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));
        this.AggregateService
            .Setup(o => o.Save(It.IsAny<OperatorAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_EstateNotCreated_ResultFailed()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);
        this.AggregateService.Setup(o => o.GetLatest<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));

        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();

    }

    [Fact]
    public async Task OperatorDomainService_CreateOperator_OperatorAlreadyCreated_ResultFailed() {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());

        this.AggregateService.Setup(o => o.GetLatest<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

        Result result = await this.OperatorDomainService.CreateOperator(TestData.Commands.CreateOperatorCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_UpdateOperator_OperatorIsUpdated()
    {
        this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());
        this.AggregateService.Setup(o => o.GetLatest<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
        this.AggregateService
            .Setup(o => o.Save(It.IsAny<OperatorAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.OperatorDomainService.UpdateOperator(TestData.Commands.UpdateOperatorCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task OperatorDomainService_UpdateOperator_OperatorNotCreated_ResultFailed()
    {
        this.AggregateService.Setup(o => o.GetLatest<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));

        Result result = await this.OperatorDomainService.UpdateOperator(TestData.Commands.UpdateOperatorCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }
}