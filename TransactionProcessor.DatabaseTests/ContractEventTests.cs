using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.EntityFramework;
using Shouldly;
using SimpleResults;
using System.ComponentModel.Design;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Repository;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests
{

    public class ContractEventTests : BaseTest
    {
        [Fact]
        public async Task AddOperator_OperatorIsAdded()
        {
            Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            Operator? @operator = await context.Operators.SingleOrDefaultAsync(c => c.OperatorId == TestData.DomainEvents.OperatorCreatedEvent.OperatorId);
            @operator.ShouldNotBeNull();
        }

        [Fact]
        public async Task AddOperator_EventReplayHandled()
        {
            Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

            result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task AddContract_ContractIsAdded() {
            Result result = await this.Repository.AddContract(TestData.DomainEvents.ContractCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            Contract? contract = await context.Contracts.SingleOrDefaultAsync(c => c.ContractId == TestData.DomainEvents.ContractCreatedEvent.ContractId);
            contract.ShouldNotBeNull();
        }

        [Fact]
        public async Task AddContract_EventReplayHandled() {
            Result result = await this.Repository.AddContract(TestData.DomainEvents.ContractCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

            result = await this.Repository.AddContract(TestData.DomainEvents.ContractCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task AddContractProduct_ContractProductIsAdded() {
            Result result = await this.Repository.AddContractProduct(TestData.DomainEvents.FixedValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            ContractProduct? fixedContractProduct = await context.ContractProducts.SingleOrDefaultAsync(c => c.ContractId == TestData.DomainEvents.FixedValueProductAddedToContractEvent.ContractId && c.ContractProductId == TestData.DomainEvents.FixedValueProductAddedToContractEvent.ProductId);
            fixedContractProduct.ShouldNotBeNull();

            result = await this.Repository.AddContractProduct(TestData.DomainEvents.VariableValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            context = this.GetContext();
            ContractProduct? variableContractProduct = await context.ContractProducts.SingleOrDefaultAsync(c => c.ContractId == TestData.DomainEvents.VariableValueProductAddedToContractEvent.ContractId && c.ContractProductId == TestData.DomainEvents.VariableValueProductAddedToContractEvent.ProductId);
            variableContractProduct.ShouldNotBeNull();
        }

        [Fact]
        public async Task AddContractProduct_EventReplayHandled() {
            Result result = await this.Repository.AddContractProduct(TestData.DomainEvents.FixedValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            ContractProduct? fixedContractProduct = await context.ContractProducts.SingleOrDefaultAsync(c => c.ContractId == TestData.DomainEvents.FixedValueProductAddedToContractEvent.ContractId && c.ContractProductId == TestData.DomainEvents.FixedValueProductAddedToContractEvent.ProductId);
            fixedContractProduct.ShouldNotBeNull();

            result = await this.Repository.AddContractProduct(TestData.DomainEvents.VariableValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            context = this.GetContext();
            ContractProduct? variableContractProduct = await context.ContractProducts.SingleOrDefaultAsync(c => c.ContractId == TestData.DomainEvents.VariableValueProductAddedToContractEvent.ContractId && c.ContractProductId == TestData.DomainEvents.VariableValueProductAddedToContractEvent.ProductId);
            variableContractProduct.ShouldNotBeNull();

            result = await this.Repository.AddContractProduct(TestData.DomainEvents.FixedValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

            result = await this.Repository.AddContractProduct(TestData.DomainEvents.VariableValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
    
        [Fact]
        public async Task AddContractProductTransactionFee_ContractIsAdded()
        {
            Result result = await this.Repository.AddContractProductTransactionFee(TestData.DomainEvents.TransactionFeeForProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            ContractProductTransactionFee? contractProductTransactionFee = await context.ContractProductTransactionFees.SingleOrDefaultAsync(c => c.ContractProductId == TestData.DomainEvents.TransactionFeeForProductAddedToContractEvent.ProductId &&
                                                                                                                                                  c.ContractProductTransactionFeeId == TestData.DomainEvents.TransactionFeeForProductAddedToContractEvent.TransactionFeeId);
            contractProductTransactionFee.ShouldNotBeNull();
        }

        [Fact]
        public async Task AddContractProductTransactionFee_EventReplayHandled()
        {
            Result result = await this.Repository.AddContractProductTransactionFee(TestData.DomainEvents.TransactionFeeForProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

            result = await this.Repository.AddContractProductTransactionFee(TestData.DomainEvents.TransactionFeeForProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
    
        [Fact]
        public async Task AddFileImportLog_FileImportLogIsAdded()
        {
            Result result = await this.Repository.AddFileImportLog(TestData.DomainEvents.ImportLogCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            var fileImportLog = await context.FileImportLogs.SingleOrDefaultAsync(f => f.FileImportLogId == TestData.DomainEvents.ImportLogCreatedEvent.FileImportLogId);
            fileImportLog.ShouldNotBeNull();
        }

        [Fact]
        public async Task AddFileImportLog_EventReplayHandled()
        {
            Result result = await this.Repository.AddFileImportLog(TestData.DomainEvents.ImportLogCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

            result = await this.Repository.AddFileImportLog(TestData.DomainEvents.ImportLogCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task AddFileImportLogFile_FileImportLogIsAdded()
        {
            Result result = await this.Repository.AddFileToImportLog(TestData.DomainEvents.FileAddedToImportLogEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            var fileImportLogFile = await context.FileImportLogFiles.SingleOrDefaultAsync(f => f.FileImportLogId == TestData.DomainEvents.FileAddedToImportLogEvent.FileImportLogId && f.FileId == TestData.DomainEvents.FileAddedToImportLogEvent.FileId);
            fileImportLogFile.ShouldNotBeNull();
        }

        [Fact]
        public async Task AddFileImportLogFile_EventReplayHandled()
        {
            Result result = await this.Repository.AddFileToImportLog(TestData.DomainEvents.FileAddedToImportLogEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

            result = await this.Repository.AddFileToImportLog(TestData.DomainEvents.FileAddedToImportLogEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }


        [Fact]
        public async Task AddEstate_EstateIsAdded()
        {
            Result result = await this.Repository.AddEstate(TestData.DomainEvents.EstateCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            var estate = await context.Estates.SingleOrDefaultAsync(f => f.EstateId == TestData.DomainEvents.EstateCreatedEvent.EstateId);
            estate.ShouldNotBeNull();
        }

        [Fact]
        public async Task AddEstate_EventReplayHandled()
        {
            Result result = await this.Repository.AddEstate(TestData.DomainEvents.EstateCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

            result = await this.Repository.AddEstate(TestData.DomainEvents.EstateCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task AddEstateSecurityUser_EstateIsAdded()
        {
            Result result = await this.Repository.AddEstateSecurityUser(TestData.DomainEvents.EstateSecurityUserAddedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            var estateSecurityUser = await context.EstateSecurityUsers.SingleOrDefaultAsync(f => f.EstateId == TestData.DomainEvents.EstateSecurityUserAddedEvent.EstateId && f.SecurityUserId == TestData.DomainEvents.EstateSecurityUserAddedEvent.SecurityUserId);
            estateSecurityUser.ShouldNotBeNull();
        }

        [Fact]
        public async Task AddEstateSecurityUser_EventReplayHandled()
        {
            Result result = await this.Repository.AddEstateSecurityUser(TestData.DomainEvents.EstateSecurityUserAddedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

            result = await this.Repository.AddEstateSecurityUser(TestData.DomainEvents.EstateSecurityUserAddedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
    }
}
