using Microsoft.EntityFrameworkCore;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

public class EstateEventTests : BaseTest {
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