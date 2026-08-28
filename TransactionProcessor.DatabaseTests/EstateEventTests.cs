using Microsoft.EntityFrameworkCore;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

public class MigrationTests : BaseTest {
    public MigrationTests(DatabaseTestFixture fixture) : base(fixture) {
    }

    public override async ValueTask InitializeAsync() {
        await this.GetRepository();
    }

    [Fact]
    public async Task CreateReadModel_EstateDatabaseIsMigrated()
    {
        Result result = await this.Repository.CreateReadModel(TestData.DomainEvents.EstateCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }
}

public class EstateEventTests : BaseTest {
    public EstateEventTests(DatabaseTestFixture fixture) : base(fixture) {
    }

    [Fact]
    public async Task AddEstate_EstateIsAdded()
    {
        Result result = await this.Repository.AddEstate(TestData.DomainEvents.EstateCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        var estate = await context.Estates.SingleOrDefaultAsync(f => f.EstateId == TestData.DomainEvents.EstateCreatedEvent.EstateId, TestContext.Current.CancellationToken);
        estate.ShouldNotBeNull();
        estate.Name.ShouldBe(TestData.DomainEvents.EstateCreatedEvent.EstateName);
    }

    [Fact]
    public async Task AddEstate_EventReplayHandled()
    {
        Result result = await this.Repository.AddEstate(TestData.DomainEvents.EstateCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.AddEstate(TestData.DomainEvents.EstateCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddEstateSecurityUser_EstateIsAdded()
    {
        Result result = await this.Repository.AddEstateSecurityUser(TestData.DomainEvents.EstateSecurityUserAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        var estateSecurityUser = await context.EstateSecurityUsers.SingleOrDefaultAsync(f => f.EstateId == TestData.DomainEvents.EstateSecurityUserAddedEvent.EstateId && f.SecurityUserId == TestData.DomainEvents.EstateSecurityUserAddedEvent.SecurityUserId, TestContext.Current.CancellationToken);
        estateSecurityUser.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddEstateSecurityUser_EventReplayHandled()
    {
        Result result = await this.Repository.AddEstateSecurityUser(TestData.DomainEvents.EstateSecurityUserAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.AddEstateSecurityUser(TestData.DomainEvents.EstateSecurityUserAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateEstate_EstateReferenceIsUpdated()
    {
        Result result = await this.Repository.AddEstate(TestData.DomainEvents.EstateCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.UpdateEstate(TestData.DomainEvents.EstateReferenceAllocatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        var estate = await context.Estates.SingleOrDefaultAsync(f => f.EstateId == TestData.DomainEvents.EstateReferenceAllocatedEvent.EstateId, TestContext.Current.CancellationToken);
        estate.ShouldNotBeNull();
        estate.Reference.ShouldBe(TestData.DomainEvents.EstateReferenceAllocatedEvent.EstateReference);
    }
}


