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
        public async Task AddContract_ContractIsAdded() {
            Result result = await this.Repository.AddContract(TestData.DomainEvents.ContractCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            Contract? contract = await context.Contracts.SingleOrDefaultAsync(c => c.ContractId == TestData.DomainEvents.ContractCreatedEvent.ContractId);
            contract.ShouldNotBeNull();
        }

        [Fact]
        public async Task AddContract_ContractIsAdded_EventReplayHandled() {
            Result result = await this.Repository.AddContract(TestData.DomainEvents.ContractCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

            result = await this.Repository.AddContract(TestData.DomainEvents.ContractCreatedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task AddContractProduct_ContractProductIsAdded()
        {
            Result result = await this.Repository.AddContractProduct(TestData.DomainEvents.FixedValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            ContractProduct? fixedContractProduct = await context.ContractProducts.SingleOrDefaultAsync(c => c.ContractId == TestData.DomainEvents.FixedValueProductAddedToContractEvent.ContractId &&
                                                                                                        c.ContractProductId == TestData.DomainEvents.FixedValueProductAddedToContractEvent.ProductId);
            fixedContractProduct.ShouldNotBeNull();

            result = await this.Repository.AddContractProduct(TestData.DomainEvents.VariableValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            context = this.GetContext();
            ContractProduct? variableContractProduct = await context.ContractProducts.SingleOrDefaultAsync(c => c.ContractId == TestData.DomainEvents.VariableValueProductAddedToContractEvent.ContractId &&
                                                                                                        c.ContractProductId == TestData.DomainEvents.VariableValueProductAddedToContractEvent.ProductId);
            variableContractProduct.ShouldNotBeNull();
        }

        [Fact]
        public async Task AddContractProduct_ContractProductIsAdded_EventReplayHandled()
        {
            Result result = await this.Repository.AddContractProduct(TestData.DomainEvents.FixedValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            EstateManagementContext context = this.GetContext();
            ContractProduct? fixedContractProduct = await context.ContractProducts.SingleOrDefaultAsync(c => c.ContractId == TestData.DomainEvents.FixedValueProductAddedToContractEvent.ContractId &&
                                                                                                             c.ContractProductId == TestData.DomainEvents.FixedValueProductAddedToContractEvent.ProductId);
            fixedContractProduct.ShouldNotBeNull();

            result = await this.Repository.AddContractProduct(TestData.DomainEvents.VariableValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            context = this.GetContext();
            ContractProduct? variableContractProduct = await context.ContractProducts.SingleOrDefaultAsync(c => c.ContractId == TestData.DomainEvents.VariableValueProductAddedToContractEvent.ContractId &&
                                                                                                                c.ContractProductId == TestData.DomainEvents.VariableValueProductAddedToContractEvent.ProductId);
            variableContractProduct.ShouldNotBeNull();

            result = await this.Repository.AddContractProduct(TestData.DomainEvents.FixedValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            
            result = await this.Repository.AddContractProduct(TestData.DomainEvents.VariableValueProductAddedToContractEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            
        }
    }
}
