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
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    public class ContractDomainServiceTests {
        private ContractDomainService DomainService;
        private Mock<IAggregateRepository<EstateAggregate, DomainEvent>> EstateAggregateRepository;
        private Mock<IAggregateRepository<ContractAggregate, DomainEvent>> ContractAggregateRepository;
        private Mock<IEventStoreContext> EventStoreContext;
        public ContractDomainServiceTests() {
            this.EstateAggregateRepository = new Mock<IAggregateRepository<EstateAggregate, DomainEvent>>();
            this.ContractAggregateRepository = new Mock<IAggregateRepository<ContractAggregate, DomainEvent>>();
            this.EventStoreContext = new Mock<IEventStoreContext>();
            this.DomainService = new ContractDomainService(this.EstateAggregateRepository.Object, this.ContractAggregateRepository.Object, this.EventStoreContext.Object);
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_ContractIsCreated()
        {
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                     .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.ContractAggregateRepository.Setup(c => c.SaveChanges(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_DuplicateContractNameForOperator_ResultFailed()
        {
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
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
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                     .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());

            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_EstateNotCreated_ResultFailed()
        {
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_CreateContract_NoOperatorCreatedForEstate_ResultFailed()
        {
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }
        
        [Fact]
        public async Task ContractDomainService_CreateContract_OperatorNotFoundForEstate_ResultFailed()
        {
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.CreateContractCommand command = TestData.Commands.CreateContractCommand;
            Result result = await this.DomainService.CreateContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_ProductAddedToContract()
        {
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.ContractAggregateRepository.Setup(c => c.SaveChanges(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_ContractNotCreated_ErrorThrown()
        {
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_FixedValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_ProductAddedToContract()
        {
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));
            this.ContractAggregateRepository.Setup(c => c.SaveChanges(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_ContractNotCreated_ErrorThrown()
        {
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_VariableValue_EstateNotCreated_ErrorThrown()
        {
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.AddProductToContractCommand command = TestData.Commands.AddProductToContractCommand_VariableValue;
            Result result = await this.DomainService.AddProductToContract(command, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task ContractDomainService_AddProductToContract_FixedValue_EstateNotCreated_ErrorThrown()
        {
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

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
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                       .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProduct()));
            this.ContractAggregateRepository.Setup(c => c.SaveChanges(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

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
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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

            this.EstateAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.EstateAggregateWithOperator());

            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(calculationType, feeType));
            this.ContractAggregateRepository.Setup(c => c.SaveChanges(It.IsAny<ContractAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

            this.EventStoreContext.Setup(c => c.RunTransientQuery(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("{\r\n  \"total\": 0,\r\n  \"contractId\": \"\"\r\n}");

            ContractCommands.DisableTransactionFeeForProductCommand command = TestData.Commands.DisableTransactionFeeForProductCommand;
            Result result = await this.DomainService.DisableTransactionFeeForProduct(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
    }
}
