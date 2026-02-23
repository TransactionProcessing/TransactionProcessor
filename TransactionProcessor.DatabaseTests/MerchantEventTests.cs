using Microsoft.EntityFrameworkCore;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

public class MerchantEventTests : BaseTest {
    [Fact]
    public async Task AddMerchant_MerchantIsAdded()
    {
        Result result = await this.Repository.AddMerchant(TestData.DomainEvents.MerchantCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        Merchant? merchant = await context.Merchants.SingleOrDefaultAsync(c => c.MerchantId == TestData.DomainEvents.MerchantCreatedEvent.MerchantId);
        merchant.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddMerchant_EventReplayHandled() {
        Result result = await this.Repository.AddMerchant(TestData.DomainEvents.MerchantCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.AddMerchant(TestData.DomainEvents.MerchantCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddMerchantDevice_MerchantContractIsAdded()
    {
        Result result = await this.Repository.AddMerchantDevice(TestData.DomainEvents.DeviceAddedToMerchantEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        var merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceId);
        merchantDevice.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddMerchantDevice_EventReplayHandled() {
        Result result = await this.Repository.AddMerchantDevice(TestData.DomainEvents.DeviceAddedToMerchantEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.AddMerchantDevice(TestData.DomainEvents.DeviceAddedToMerchantEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task SwapMerchantDevice_MerchantDeviceIsAdded()
    {
        Result result = await this.Repository.AddMerchantDevice(TestData.DomainEvents.DeviceAddedToMerchantEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.SwapMerchantDevice(TestData.DomainEvents.DeviceSwappedForMerchantEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        var merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceId);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeFalse();

        merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceSwappedForMerchantEvent.DeviceId);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceSwappedForMerchantEvent.NewDeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public async Task SwapMerchantDevice_EventReplayHandled()
    {
        Result result = await this.Repository.AddMerchantDevice(TestData.DomainEvents.DeviceAddedToMerchantEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.SwapMerchantDevice(TestData.DomainEvents.DeviceSwappedForMerchantEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        var merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceId);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeFalse();

        merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceSwappedForMerchantEvent.DeviceId);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceSwappedForMerchantEvent.NewDeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeTrue();

        result = await this.Repository.SwapMerchantDevice(TestData.DomainEvents.DeviceSwappedForMerchantEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceId);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeFalse();

        merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceSwappedForMerchantEvent.DeviceId);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceSwappedForMerchantEvent.NewDeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeTrue();
    }
}