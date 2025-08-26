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
    }
}
