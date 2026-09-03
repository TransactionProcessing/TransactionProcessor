using System;
using System.Threading;
using System.Threading.Tasks;
using Imposter.Abstractions;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventStore;
using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    public class ContractDomainServiceTests {
        private ContractDomainService DomainService;
        private IAggregateServiceImposter AggregateService;
        private IEventStoreContextImposter EventStoreContext;
        public ContractDomainServiceTests() {
            this.AggregateService = new IAggregateServiceImposter();
            this.EventStoreContext = new IEventStoreContextImposter();
            IAggregateService AggregateServiceResolver() => this.AggregateService.Instance();
            this.DomainService = new ContractDomainService(AggregateServiceResolver, this.EventStoreContext.Instance());
            StringSerialiser.Initialise(new SystemTextJsonSerializer(SystemTextJsonSerializer.GetDefaultJsonSerializerOptions()));
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_ContractIsCreated()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                     .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.AggregateService.Save<ContractAggregate>(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            
            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_DuplicateContractNameForOperator_ResultFailed()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            String queryResult =
                "{\r\n  \"total\": 1,\r\n  \"contract_Id\": \"3015e4d0-e9a9-49e5-bd55-a5492f193b62\"\r\n}";
            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(queryResult);

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_ContractAlreadyCreated_ResultFailed()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());

            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            
            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_EstateNotCreated_ResultFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_NoOperatorCreatedForEstate_ResultFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }
        
        [Fact]
        public async Task ContractDomainService_CreateContract_OperatorNotFoundForEstate_ResultFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            
            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_ProductAddedToContract()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_ContractNotCreated_ErrorThrown()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_ProductAddedToContract()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_ContractNotCreated_ErrorThrown()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_EstateNotCreated_ErrorThrown()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_EstateNotCreated_ErrorThrown()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        public async Task ContractDomainService_AddTransactionFeeForProductToContract_TransactionFeeIsAddedToProduct(DataTransferObjects.Responses.Contract.CalculationType calculationType, DataTransferObjects.Responses.Contract.FeeType feeType)
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                       .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProduct()));
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(calculationType, feeType);
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Theory]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        public async Task ContractDomainService_AddTransactionFeeForProductToContract_ContractNotCreated_ErrorThrown(DataTransferObjects.Responses.Contract.CalculationType calculationType, DataTransferObjects.Responses.Contract.FeeType feeType)
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                       .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(calculationType,feeType);
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        public async Task ContractDomainService_AddTransactionFeeForProductToContract_ProductNotFound_ErrorThrown(
            DataTransferObjects.Responses.Contract.CalculationType calculationType,
            DataTransferObjects.Responses.Contract.FeeType feeType) {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(calculationType, feeType);
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(CalculationType.Fixed, FeeType.Merchant)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider)]
        public async Task ContractDomainService_DisableTransactionFeeForProduct_TransactionFeeDisabled(
            CalculationType calculationType,
            FeeType feeType) {

            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());

            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(calculationType, feeType));
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");

            ContractCommands.DisableTransactionFeeForProductCommand command = TestData.Commands.DisableTransactionFeeForProductCommand;
            Result result = await this.DomainService.DisableTransactionFeeForProduct(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_DisableTransactionFeeForProduct_GetContractFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.DisableTransactionFeeForProductCommand command = TestData.Commands.DisableTransactionFeeForProductCommand;
            Result result = await this.DomainService.DisableTransactionFeeForProduct(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_DisableTransactionFeeForProduct_StateChangeFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            ContractCommands.DisableTransactionFeeForProductCommand command = TestData.Commands.DisableTransactionFeeForProductCommand;
            Result result = await this.DomainService.DisableTransactionFeeForProduct(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_DisableTransactionFeeForProduct_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.DisableTransactionFeeForProductCommand command = TestData.Commands.DisableTransactionFeeForProductCommand;
            Result result = await this.DomainService.DisableTransactionFeeForProduct(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_DisableTransactionFeeForProduct_ExceptionThrown_ResultIsFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception());

            ContractCommands.DisableTransactionFeeForProductCommand command = TestData.Commands.DisableTransactionFeeForProductCommand;
            Result result = await this.DomainService.DisableTransactionFeeForProduct(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_GetEstateFailed_ResultIsFailed()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_GetContractFailed_ResultIsFailed()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_RunTransientQueryFailed_ResultIsFailed()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_StateChangeFailed_ResultIsFailed()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");
            
            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            command = command with {
                RequestDTO = new CreateContractRequest {
                    Description = String.Empty,
                    OperatorId = TestData.Commands.CreateContractCommand.RequestDTO.OperatorId
                }
            };
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_ExceptionThrown_ResultIsFailed()
        {
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.EventStoreContext.RunTransientQuery(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contract_Id\": \"\"\r\n}");
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception());

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_GetContractFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_GetContractFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_StateChangeFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            command = command with { ProductId = Guid.Empty };
            Result result = await this.DomainService.AddProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        public async Task ContractDomainService_AddTransactionFeeForProductToContract_GetContractFailed_ResultIsFailed(
            DataTransferObjects.Responses.Contract.CalculationType calculationType,
            DataTransferObjects.Responses.Contract.FeeType feeType)
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(calculationType, feeType);
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        public async Task ContractDomainService_AddTransactionFeeForProductToContract_SaveFailed_ResultIsFailed(
            DataTransferObjects.Responses.Contract.CalculationType calculationType,
            DataTransferObjects.Responses.Contract.FeeType feeType)
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProduct()));
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(calculationType, feeType);
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddTransactionFeeForProductToContract_StateChangeFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProduct()));
            this.AggregateService.Save(Arg<ContractAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.Merchant);
            command = command with { TransactionFeeId = Guid.Empty };
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }
    }
}

