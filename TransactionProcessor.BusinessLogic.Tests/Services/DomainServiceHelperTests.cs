using System;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using Shouldly;
using TransactionProcessing.UnionResults;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Services;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    public class DomainServiceHelperTests
    {
        [Fact]
        public void DomainServiceHelper_HandleGetAggregateResult_SuccessfulGet_ResultHandled() {
            Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
            Result<TestAggregate> result = Results.Ok(new TestAggregate {
                AggregateId = aggregateId});
            
            var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, true);
            handleResult.IsSuccess().ShouldBeTrue();
            handleResult.Data.ShouldBeOfType(typeof(TestAggregate));
            handleResult.Data.AggregateId.ShouldBe(aggregateId);
        }

        [Fact]
        public void DomainServiceHelper_HandleGetAggregateResult_FailedGet_ResultHandled()
        {
            Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
            Result<TestAggregate> result = new Error(ErrorType.Failure, "Failed Get");

            var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, true);
            handleResult.IsSuccess().ShouldBeFalse();
            handleResult.GetErrorMessage().ShouldBe("Failed Get");
        }

        [Fact]
        public void DomainServiceHelper_HandleGetAggregateResult_FailedGet_NotFoundButIsError_ResultHandled()
        {
            Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
            Result<TestAggregate> result = new Error(ErrorType.NotFound, "Failed Get");

            var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, true);
            handleResult.IsSuccess().ShouldBeFalse();
            handleResult.GetErrorMessage().ShouldBe("Failed Get");
        }

        [Fact]
        public void DomainServiceHelper_HandleGetAggregateResult_FailedGet_NotFoundButIsNotError_ResultHandled()
        {
            Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
            Result<TestAggregate> result = new Error(ErrorType.NotFound, "Failed Get");

            var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, false);
            handleResult.IsSuccess().ShouldBeTrue();
            handleResult.Data.ShouldBeOfType(typeof(TestAggregate));
            handleResult.Data.AggregateId.ShouldBe(aggregateId);
        }
    }

    public record TestAggregate : Aggregate {
        public override void PlayEvent(IDomainEvent domainEvent) {
            
        }

        protected override Object GetMetadata() {
            return new Object();
        }
    }

}
