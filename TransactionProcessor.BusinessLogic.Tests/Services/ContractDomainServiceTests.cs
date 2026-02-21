using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventStore;
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
        private Mock<IAggregateService> AggregateService;
        private Mock<IEventStoreContext> EventStoreContext;
        public ContractDomainServiceTests() {
            this.AggregateService = new Mock<IAggregateService>();
            this.EventStoreContext = new Mock<IEventStoreContext>();
            IAggregateService AggregateServiceResolver() => this.AggregateService.Object;
            this.DomainService = new ContractDomainService(AggregateServiceResolver, this.EventStoreContext.Object);
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_ContractIsCreated()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                     .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.AggregateService.Setup(c => c.Save<ContractAggregate>(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_DuplicateContractNameForOperator_ResultFailed()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            String queryResult =
                "{\r\n  \"total\": 1,\r\n  \"contractId\": \"3015e4d0-e9a9-49e5-bd55-a5492f193b62\"\r\n}";
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_ContractAlreadyCreated_ResultFailed()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());

            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_EstateNotCreated_ResultFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_NoOperatorCreatedForEstate_ResultFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }
        
        [Fact]
        public async Task ContractDomainService_CreateContract_OperatorNotFoundForEstate_ResultFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_ProductAddedToContract()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_ContractNotCreated_ErrorThrown()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_ProductAddedToContract()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_ContractNotCreated_ErrorThrown()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_EstateNotCreated_ErrorThrown()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_EstateNotCreated_ErrorThrown()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        public async Task ContractDomainService_AddTransactionFeeForProductToContract_TransactionFeeIsAddedToProduct(DataTransferObjects.Responses.Contract.CalculationType calculationType, DataTransferObjects.Responses.Contract.FeeType feeType)
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProduct()));
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(calculationType, feeType);
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Theory]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.Merchant)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        [InlineData(DataTransferObjects.Responses.Contract.CalculationType.Percentage, DataTransferObjects.Responses.Contract.FeeType.ServiceProvider)]
        public async Task ContractDomainService_AddTransactionFeeForProductToContract_ContractNotCreated_ErrorThrown(DataTransferObjects.Responses.Contract.CalculationType calculationType, DataTransferObjects.Responses.Contract.FeeType feeType)
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(calculationType,feeType);
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, CancellationToken.None);
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
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(calculationType, feeType);
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, CancellationToken.None);
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

            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());

            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(calculationType, feeType));
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.DisableTransactionFeeForProductCommand command = TestData.Commands.DisableTransactionFeeForProductCommand;
            Result result = await this.DomainService.DisableTransactionFeeForProduct(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_DisableTransactionFeeForProduct_GetContractFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.DisableTransactionFeeForProductCommand command = TestData.Commands.DisableTransactionFeeForProductCommand;
            Result result = await this.DomainService.DisableTransactionFeeForProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_DisableTransactionFeeForProduct_StateChangeFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            ContractCommands.DisableTransactionFeeForProductCommand command = TestData.Commands.DisableTransactionFeeForProductCommand;
            Result result = await this.DomainService.DisableTransactionFeeForProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_DisableTransactionFeeForProduct_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.DisableTransactionFeeForProductCommand command = TestData.Commands.DisableTransactionFeeForProductCommand;
            Result result = await this.DomainService.DisableTransactionFeeForProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_DisableTransactionFeeForProduct_ExceptionThrown_ResultIsFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant));
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            ContractCommands.DisableTransactionFeeForProductCommand command = TestData.Commands.DisableTransactionFeeForProductCommand;
            Result result = await this.DomainService.DisableTransactionFeeForProduct(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_GetEstateFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_GetContractFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_RunTransientQueryFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_StateChangeFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");
            //this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>()))
            //    .ReturnsAsync(Result.Failure());

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            command = command with {
                RequestDTO = new CreateContractRequest {
                    Description = String.Empty,
                    OperatorId = TestData.Commands.CreateContractCommand.RequestDTO.OperatorId
                }
            };
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_ExceptionThrown_ResultIsFailed()
        {
            this.AggregateService.Setup(e => e.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_GetContractFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_GetContractFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_StateChangeFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            command = command with { ProductId = Guid.Empty };
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
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
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(calculationType, feeType);
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, CancellationToken.None);
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
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProduct()));
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(calculationType, feeType);
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddTransactionFeeForProductToContract_StateChangeFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProduct()));
            this.AggregateService.Setup(c => c.Save(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                TestData.Commands.AddTransactionFeeForProductToContractCommand(DataTransferObjects.Responses.Contract.CalculationType.Fixed, DataTransferObjects.Responses.Contract.FeeType.Merchant);
            command = command with { TransactionFeeId = Guid.Empty };
            Result result = await this.DomainService.AddTransactionFeeForProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }
    }
}
