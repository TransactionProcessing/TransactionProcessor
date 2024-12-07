using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shouldly;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Services;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    public class DomainServiceHelperTests
    {
        [Fact]
        public void DomainServiceHelper_HandleGetAggregateResult_SuccessfulGet_ResultHandled() {
            Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
            Result<TestAggregate> result = Result.Success(new TestAggregate {
                AggregateId = aggregateId});
            
            var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, true);
            handleResult.IsSuccess.ShouldBeTrue();
            handleResult.Data.ShouldBeOfType(typeof(TestAggregate));
            handleResult.Data.AggregateId.ShouldBe(aggregateId);
        }

        [Fact]
        public void DomainServiceHelper_HandleGetAggregateResult_FailedGet_ResultHandled()
        {
            Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
            Result<TestAggregate> result = Result.Failure("Failed Get");

            var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, true);
            handleResult.IsFailed.ShouldBeTrue();
            handleResult.Message.ShouldBe("Failed Get");
        }

        [Fact]
        public void DomainServiceHelper_HandleGetAggregateResult_FailedGet_NotFoundButIsError_ResultHandled()
        {
            Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
            Result<TestAggregate> result = Result.NotFound("Failed Get");

            var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, true);
            handleResult.IsFailed.ShouldBeTrue();
            handleResult.Message.ShouldBe("Failed Get");
        }

        [Fact]
        public void DomainServiceHelper_HandleGetAggregateResult_FailedGet_NotFoundButIsNotError_ResultHandled()
        {
            Guid aggregateId = Guid.Parse("0639682D-1D28-4AD8-B29D-4B76619083F1");
            Result<TestAggregate> result = Result.NotFound("Failed Get");

            var handleResult = DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, false);
            handleResult.IsSuccess.ShouldBeTrue();
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
