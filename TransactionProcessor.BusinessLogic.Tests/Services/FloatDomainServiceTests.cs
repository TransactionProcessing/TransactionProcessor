using SimpleResults;
using System;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using Microsoft.Extensions.Configuration;
    using Imposter.Abstractions;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using Shared.Serialisation;
    using Shouldly;
    using System.Text.Json;
    using System.Threading;
    using Testing;
    using TransactionProcessor.BusinessLogic.Common;
    using TransactionProcessor.Aggregates;
    using TransactionProcessor.BusinessLogic.Services;
    using Xunit;

    public class FloatDomainServiceTests
    {
        private readonly IAggregateServiceImposter AggregateService;
        private readonly FloatDomainService FloatDomainService;

        public FloatDomainServiceTests(){
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.AggregateService = new IAggregateServiceImposter();
            IAggregateService AggregateServiceResolver() => this.AggregateService.Instance();
            this.FloatDomainService = new FloatDomainService(AggregateServiceResolver);
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_FloatCreated(){

            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

            this.AggregateService.GetLatest<FloatAggregate>(TestData.OperatorId, Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(FloatAggregate.Create(TestData.OperatorId)));
            this.AggregateService.Save<FloatAggregate>(Arg<FloatAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

            FloatCommands.CreateFloatCommand command = new FloatCommands.CreateFloatCommand(TestData.EstateId, TestData.OperatorId, TestData.FloatCreatedDateTime);
            Result result = await this.FloatDomainService.CreateFloat(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidEstate_ErrorThrown()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.NotFound());
            FloatCommands.CreateFloatCommand command = new FloatCommands.CreateFloatCommand(TestData.EstateId, TestData.OperatorId, TestData.FloatCreatedDateTime);
            Result result = await this.FloatDomainService.CreateFloat(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_InvalidOperator_ErrorThrown()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

            FloatCommands.CreateFloatCommand command = new FloatCommands.CreateFloatCommand(TestData.EstateId, TestData.OperatorId2, TestData.FloatCreatedDateTime);
            Result result = await this.FloatDomainService.CreateFloat(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_DeletedOperator_ErrorThrown()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperatorDeleted()));

            FloatCommands.CreateFloatCommand command = new FloatCommands.CreateFloatCommand(TestData.EstateId, TestData.OperatorId, TestData.FloatCreatedDateTime);
            Result result = await this.FloatDomainService.CreateFloat(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_PurchaseRecorded(){
            FloatAggregate floatAggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            floatAggregate.CreateFloat(TestData.EstateId, TestData.FloatCreatedDateTime);
            this.AggregateService.GetLatest<FloatAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Save<FloatAggregate>(Arg<FloatAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

            FloatCommands.RecordCreditPurchaseForFloatCommand command = new FloatCommands.RecordCreditPurchaseForFloatCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice,
                TestData.CreditPurchasedDateTime);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_SaveFailed()
        {
            FloatAggregate floatAggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            floatAggregate.CreateFloat(TestData.EstateId, TestData.FloatCreatedDateTime);
            this.AggregateService.GetLatest<FloatAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Save<FloatAggregate>(Arg<FloatAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            FloatCommands.RecordCreditPurchaseForFloatCommand command = new FloatCommands.RecordCreditPurchaseForFloatCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice,
                TestData.CreditPurchasedDateTime);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_ExceptionThrown()
        {
            FloatAggregate floatAggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            floatAggregate.CreateFloat(TestData.EstateId, TestData.FloatCreatedDateTime);
            this.AggregateService.GetLatest<FloatAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Save<FloatAggregate>(Arg<FloatAggregate>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception());

            FloatCommands.RecordCreditPurchaseForFloatCommand command = new FloatCommands.RecordCreditPurchaseForFloatCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice,
                TestData.CreditPurchasedDateTime);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_FloatActivity_PurchaseRecorded()
        {
            FloatActivityAggregate floatAggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
            this.AggregateService.GetLatest<FloatActivityAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Save<FloatActivityAggregate>(Arg<FloatActivityAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

            FloatActivityCommands.RecordCreditPurchaseCommand command = new FloatActivityCommands.RecordCreditPurchaseCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_FloatActivity_SaveFailed()
        {
            FloatActivityAggregate floatAggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
            this.AggregateService.GetLatest<FloatActivityAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Save<FloatActivityAggregate>(Arg<FloatActivityAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            FloatActivityCommands.RecordCreditPurchaseCommand command = new FloatActivityCommands.RecordCreditPurchaseCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_FloatActivity_ExceptionThrown()
        {
            FloatActivityAggregate floatAggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
            this.AggregateService.GetLatest<FloatActivityAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(floatAggregate));
            this.AggregateService.Save<FloatActivityAggregate>(Arg<FloatActivityAggregate>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception());

            FloatActivityCommands.RecordCreditPurchaseCommand command = new FloatActivityCommands.RecordCreditPurchaseCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_GetFloatFailed()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.GetLatest<FloatAggregate>(TestData.OperatorId, Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            FloatCommands.CreateFloatCommand command = new FloatCommands.CreateFloatCommand(TestData.EstateId, TestData.OperatorId, TestData.FloatCreatedDateTime);
            Result result = await this.FloatDomainService.CreateFloat(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_SaveFailed()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.GetLatest<FloatAggregate>(TestData.OperatorId, Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(FloatAggregate.Create(TestData.OperatorId)));
            this.AggregateService.Save<FloatAggregate>(Arg<FloatAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            FloatCommands.CreateFloatCommand command = new FloatCommands.CreateFloatCommand(TestData.EstateId, TestData.OperatorId, TestData.FloatCreatedDateTime);
            Result result = await this.FloatDomainService.CreateFloat(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_CreateFloatForContractProduct_ExceptionThrown()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.GetLatest<FloatAggregate>(TestData.OperatorId, Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(FloatAggregate.Create(TestData.OperatorId)));
            this.AggregateService.Save<FloatAggregate>(Arg<FloatAggregate>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception());

            FloatCommands.CreateFloatCommand command = new FloatCommands.CreateFloatCommand(TestData.EstateId, TestData.OperatorId, TestData.FloatCreatedDateTime);
            Result result = await this.FloatDomainService.CreateFloat(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_GetFloatFailed()
        {
            this.AggregateService.GetLatest<FloatAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            FloatCommands.RecordCreditPurchaseForFloatCommand command = new FloatCommands.RecordCreditPurchaseForFloatCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice,
                TestData.CreditPurchasedDateTime);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordCreditPurchase_FloatActivity_GetFloatActivityFailed()
        {
            this.AggregateService.GetLatest<FloatActivityAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            FloatActivityCommands.RecordCreditPurchaseCommand command = new FloatActivityCommands.RecordCreditPurchaseCommand(TestData.EstateId,
                TestData.FloatAggregateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
            Result result = await this.FloatDomainService.RecordCreditPurchase(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordTransaction_TransactionRecorded()
        {
            Guid floatActivityAggregateId = IdGenerationService.GenerateFloatActivityAggregateId(TestData.EstateId, TestData.OperatorId, TestData.TransactionDateTime.Date);
            FloatActivityAggregate floatActivityAggregate = FloatActivityAggregate.Create(floatActivityAggregateId);
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.GetLatest<FloatActivityAggregate>(floatActivityAggregateId, Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(floatActivityAggregate));
            this.AggregateService.Save<FloatActivityAggregate>(Arg<FloatActivityAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

            FloatActivityCommands.RecordTransactionCommand command = new FloatActivityCommands.RecordTransactionCommand(TestData.EstateId, TestData.TransactionId);
            Result result = await this.FloatDomainService.RecordTransaction(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordTransaction_GetTransactionFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            FloatActivityCommands.RecordTransactionCommand command = new FloatActivityCommands.RecordTransactionCommand(TestData.EstateId, TestData.TransactionId);
            Result result = await this.FloatDomainService.RecordTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordTransaction_GetFloatActivityFailed()
        {
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.GetLatest<FloatActivityAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            FloatActivityCommands.RecordTransactionCommand command = new FloatActivityCommands.RecordTransactionCommand(TestData.EstateId, TestData.TransactionId);
            Result result = await this.FloatDomainService.RecordTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordTransaction_SaveFailed()
        {
            Guid floatActivityAggregateId = IdGenerationService.GenerateFloatActivityAggregateId(TestData.EstateId, TestData.OperatorId, TestData.TransactionDateTime.Date);
            FloatActivityAggregate floatActivityAggregate = FloatActivityAggregate.Create(floatActivityAggregateId);
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.GetLatest<FloatActivityAggregate>(floatActivityAggregateId, Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(floatActivityAggregate));
            this.AggregateService.Save<FloatActivityAggregate>(Arg<FloatActivityAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

            FloatActivityCommands.RecordTransactionCommand command = new FloatActivityCommands.RecordTransactionCommand(TestData.EstateId, TestData.TransactionId);
            Result result = await this.FloatDomainService.RecordTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FloatDomainService_RecordTransaction_ExceptionThrown()
        {
            Guid floatActivityAggregateId = IdGenerationService.GenerateFloatActivityAggregateId(TestData.EstateId, TestData.OperatorId, TestData.TransactionDateTime.Date);
            FloatActivityAggregate floatActivityAggregate = FloatActivityAggregate.Create(floatActivityAggregateId);
            this.AggregateService.GetLatest<TransactionAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.AggregateService.GetLatest<FloatActivityAggregate>(floatActivityAggregateId, Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(floatActivityAggregate));
            this.AggregateService.Save<FloatActivityAggregate>(Arg<FloatActivityAggregate>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception());

            FloatActivityCommands.RecordTransactionCommand command = new FloatActivityCommands.RecordTransactionCommand(TestData.EstateId, TestData.TransactionId);
            Result result = await this.FloatDomainService.RecordTransaction(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }
    }
}


