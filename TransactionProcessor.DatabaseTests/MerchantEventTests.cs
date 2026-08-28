using Microsoft.EntityFrameworkCore;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

public class MerchantEventTests : BaseTest {
    public MerchantEventTests(DatabaseTestFixture fixture) : base(fixture) {
    }

    private async Task CreateMerchantAsync()
    {
        Result result = await this.Repository.AddMerchant(TestData.DomainEvents.MerchantCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    private static MerchantDomainEvents.MerchantOpeningHoursUpdatedEvent CreateMerchantOpeningHoursUpdatedEvent(DayOfWeek dayOfWeek, String opening, String closing)
    {
        return new MerchantDomainEvents.MerchantOpeningHoursUpdatedEvent(TestData.MerchantId, TestData.EstateId, (Int32)dayOfWeek, opening, closing);
    }

    [Fact]
    public async Task AddMerchant_MerchantIsAdded()
    {
        Result result = await this.Repository.AddMerchant(TestData.DomainEvents.MerchantCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        Merchant? merchant = await context.Merchants.SingleOrDefaultAsync(c => c.MerchantId == TestData.DomainEvents.MerchantCreatedEvent.MerchantId, TestContext.Current.CancellationToken);
        merchant.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddMerchant_EventReplayHandled() {
        Result result = await this.Repository.AddMerchant(TestData.DomainEvents.MerchantCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.AddMerchant(TestData.DomainEvents.MerchantCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddMerchantAddress_MerchantAddressIsAdded()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantAddress(TestData.DomainEvents.AddressAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantAddress? merchantAddress = await context.MerchantAddresses.SingleOrDefaultAsync(c => c.AddressId == TestData.DomainEvents.AddressAddedEvent.AddressId, TestContext.Current.CancellationToken);
        merchantAddress.ShouldNotBeNull();
        merchantAddress.MerchantId.ShouldBe(TestData.DomainEvents.AddressAddedEvent.MerchantId);
        merchantAddress.AddressLine1.ShouldBe(TestData.DomainEvents.AddressAddedEvent.AddressLine1);
        merchantAddress.AddressLine2.ShouldBe(TestData.DomainEvents.AddressAddedEvent.AddressLine2);
        merchantAddress.AddressLine3.ShouldBe(TestData.DomainEvents.AddressAddedEvent.AddressLine3);
        merchantAddress.AddressLine4.ShouldBe(TestData.DomainEvents.AddressAddedEvent.AddressLine4);
        merchantAddress.Town.ShouldBe(TestData.DomainEvents.AddressAddedEvent.Town);
        merchantAddress.Region.ShouldBe(TestData.DomainEvents.AddressAddedEvent.Region);
        merchantAddress.PostalCode.ShouldBe(TestData.DomainEvents.AddressAddedEvent.PostalCode);
        merchantAddress.Country.ShouldBe(TestData.DomainEvents.AddressAddedEvent.Country);
    }

    [Fact]
    public async Task AddMerchantAddress_EventReplayHandled()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantAddress(TestData.DomainEvents.AddressAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.AddMerchantAddress(TestData.DomainEvents.AddressAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddMerchantContact_MerchantContactIsAdded()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantContact(TestData.DomainEvents.ContactAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantContact? merchantContact = await context.MerchantContacts.SingleOrDefaultAsync(c => c.ContactId == TestData.DomainEvents.ContactAddedEvent.ContactId, TestContext.Current.CancellationToken);
        merchantContact.ShouldNotBeNull();
        merchantContact.MerchantId.ShouldBe(TestData.DomainEvents.ContactAddedEvent.MerchantId);
        merchantContact.Name.ShouldBe(TestData.DomainEvents.ContactAddedEvent.ContactName);
        merchantContact.EmailAddress.ShouldBe(TestData.DomainEvents.ContactAddedEvent.ContactEmailAddress);
        merchantContact.PhoneNumber.ShouldBe(TestData.DomainEvents.ContactAddedEvent.ContactPhoneNumber);
    }

    [Fact]
    public async Task AddMerchantContact_EventReplayHandled()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantContact(TestData.DomainEvents.ContactAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.AddMerchantContact(TestData.DomainEvents.ContactAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddMerchantDevice_MerchantContractIsAdded()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantDevice(TestData.DomainEvents.DeviceAddedToMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        var merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceId, TestContext.Current.CancellationToken);
        merchantDevice.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddMerchantDevice_EventReplayHandled() {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantDevice(TestData.DomainEvents.DeviceAddedToMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.AddMerchantDevice(TestData.DomainEvents.DeviceAddedToMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddMerchantOperator_MerchantOperatorIsAdded()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantOperator(TestData.DomainEvents.OperatorAssignedToMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantOperator? merchantOperator = await context.MerchantOperators.SingleOrDefaultAsync(c => c.OperatorId == TestData.DomainEvents.OperatorAssignedToMerchantEvent.OperatorId && c.MerchantId == TestData.DomainEvents.OperatorAssignedToMerchantEvent.MerchantId, TestContext.Current.CancellationToken);
        merchantOperator.ShouldNotBeNull();
        merchantOperator.Name.ShouldBe(TestData.DomainEvents.OperatorAssignedToMerchantEvent.Name);
        merchantOperator.MerchantNumber.ShouldBe(TestData.DomainEvents.OperatorAssignedToMerchantEvent.MerchantNumber);
        merchantOperator.TerminalNumber.ShouldBe(TestData.DomainEvents.OperatorAssignedToMerchantEvent.TerminalNumber);
        merchantOperator.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public async Task AddMerchantOperator_EventReplayHandled()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantOperator(TestData.DomainEvents.OperatorAssignedToMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.AddMerchantOperator(TestData.DomainEvents.OperatorAssignedToMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddMerchantSecurityUser_MerchantSecurityUserIsAdded()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantSecurityUser(TestData.DomainEvents.MerchantSecurityUserAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantSecurityUser? merchantSecurityUser = await context.MerchantSecurityUsers.SingleOrDefaultAsync(c => c.SecurityUserId == TestData.DomainEvents.MerchantSecurityUserAddedEvent.SecurityUserId, TestContext.Current.CancellationToken);
        merchantSecurityUser.ShouldNotBeNull();
        merchantSecurityUser.MerchantId.ShouldBe(TestData.DomainEvents.MerchantSecurityUserAddedEvent.MerchantId);
        merchantSecurityUser.EmailAddress.ShouldBe(TestData.DomainEvents.MerchantSecurityUserAddedEvent.EmailAddress);
    }

    [Fact]
    public async Task AddMerchantSecurityUser_EventReplayHandled()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantSecurityUser(TestData.DomainEvents.MerchantSecurityUserAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.AddMerchantSecurityUser(TestData.DomainEvents.MerchantSecurityUserAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddMerchantSchedule_MerchantScheduleIsAdded()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantSchedule(TestData.DomainEvents.MerchantScheduleCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantSchedule? merchantSchedule = await context.MerchantSchedules.SingleOrDefaultAsync(c => c.MerchantScheduleId == TestData.DomainEvents.MerchantScheduleCreatedEvent.MerchantScheduleId, TestContext.Current.CancellationToken);
        merchantSchedule.ShouldNotBeNull();
        merchantSchedule.EstateId.ShouldBe(TestData.DomainEvents.MerchantScheduleCreatedEvent.EstateId);
        merchantSchedule.MerchantId.ShouldBe(TestData.DomainEvents.MerchantScheduleCreatedEvent.MerchantId);
        merchantSchedule.Year.ShouldBe(TestData.DomainEvents.MerchantScheduleCreatedEvent.Year);
    }

    [Fact]
    public async Task AddMerchantSchedule_EventReplayHandled()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantSchedule(TestData.DomainEvents.MerchantScheduleCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.AddMerchantSchedule(TestData.DomainEvents.MerchantScheduleCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task RemoveOperatorFromMerchant_MerchantOperatorIsMarkedDeleted()
    {
        await this.CreateMerchantAsync();
        Result result = await this.Repository.AddMerchantOperator(TestData.DomainEvents.OperatorAssignedToMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.RemoveOperatorFromMerchant(TestData.DomainEvents.OperatorRemovedFromMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantOperator? merchantOperator = await context.MerchantOperators.SingleOrDefaultAsync(c => c.OperatorId == TestData.DomainEvents.OperatorRemovedFromMerchantEvent.OperatorId && c.MerchantId == TestData.DomainEvents.OperatorRemovedFromMerchantEvent.MerchantId, TestContext.Current.CancellationToken);
        merchantOperator.ShouldNotBeNull();
        merchantOperator.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public async Task RemoveOperatorFromMerchant_EventReplayHandled()
    {
        await this.CreateMerchantAsync();
        Result result = await this.Repository.AddMerchantOperator(TestData.DomainEvents.OperatorAssignedToMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.RemoveOperatorFromMerchant(TestData.DomainEvents.OperatorRemovedFromMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.RemoveOperatorFromMerchant(TestData.DomainEvents.OperatorRemovedFromMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task SwapMerchantDevice_MerchantDeviceIsAdded()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantDevice(TestData.DomainEvents.DeviceAddedToMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.SwapMerchantDevice(TestData.DomainEvents.DeviceSwappedForMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        var merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceId, TestContext.Current.CancellationToken);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeFalse();

        merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceSwappedForMerchantEvent.DeviceId, TestContext.Current.CancellationToken);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceSwappedForMerchantEvent.NewDeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public async Task SwapMerchantDevice_EventReplayHandled()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.AddMerchantDevice(TestData.DomainEvents.DeviceAddedToMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.SwapMerchantDevice(TestData.DomainEvents.DeviceSwappedForMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        var merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceId, TestContext.Current.CancellationToken);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeFalse();

        merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceSwappedForMerchantEvent.DeviceId, TestContext.Current.CancellationToken);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceSwappedForMerchantEvent.NewDeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeTrue();

        result = await this.Repository.SwapMerchantDevice(TestData.DomainEvents.DeviceSwappedForMerchantEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceId, TestContext.Current.CancellationToken);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceAddedToMerchantEvent.DeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeFalse();

        merchantDevice = await context.MerchantDevices.SingleOrDefaultAsync(c => c.DeviceId == TestData.DomainEvents.DeviceSwappedForMerchantEvent.DeviceId, TestContext.Current.CancellationToken);
        merchantDevice.ShouldNotBeNull();
        merchantDevice.DeviceIdentifier.ShouldBe(TestData.DomainEvents.DeviceSwappedForMerchantEvent.NewDeviceIdentifier);
        merchantDevice.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateMerchantName_MerchantIsUpdated()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.UpdateMerchant(TestData.DomainEvents.MerchantNameUpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Merchant? merchant = await context.Merchants.SingleOrDefaultAsync(c => c.MerchantId == TestData.DomainEvents.MerchantNameUpdatedEvent.MerchantId, TestContext.Current.CancellationToken);
        merchant.ShouldNotBeNull();
        merchant.Name.ShouldBe(TestData.DomainEvents.MerchantNameUpdatedEvent.MerchantName);
    }

    [Fact]
    public async Task UpdateMerchantReference_MerchantIsUpdated()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.UpdateMerchant(TestData.DomainEvents.MerchantReferenceAllocatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Merchant? merchant = await context.Merchants.SingleOrDefaultAsync(c => c.MerchantId == TestData.DomainEvents.MerchantReferenceAllocatedEvent.MerchantId, TestContext.Current.CancellationToken);
        merchant.ShouldNotBeNull();
        merchant.Reference.ShouldBe(TestData.DomainEvents.MerchantReferenceAllocatedEvent.MerchantReference);
    }

    [Fact]
    public async Task UpdateMerchantSettlementSchedule_MerchantIsUpdated()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.UpdateMerchant(TestData.DomainEvents.SettlementScheduleChangedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Merchant? merchant = await context.Merchants.SingleOrDefaultAsync(c => c.MerchantId == TestData.DomainEvents.SettlementScheduleChangedEvent.MerchantId, TestContext.Current.CancellationToken);
        merchant.ShouldNotBeNull();
        merchant.SettlementSchedule.ShouldBe(TestData.DomainEvents.SettlementScheduleChangedEvent.SettlementSchedule);
    }

    [Fact]
    public async Task UpdateMerchantAddress_MerchantAddressIsUpdated()
    {
        await this.CreateMerchantAsync();
        Result result = await this.Repository.AddMerchantAddress(TestData.DomainEvents.AddressAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.UpdateMerchantAddress(TestData.DomainEvents.MerchantAddressLine1UpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantAddress(TestData.DomainEvents.MerchantAddressLine2UpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantAddress(TestData.DomainEvents.MerchantAddressLine3UpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantAddress(TestData.DomainEvents.MerchantAddressLine4UpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantAddress(TestData.DomainEvents.MerchantCountyUpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantAddress(TestData.DomainEvents.MerchantRegionUpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantAddress(TestData.DomainEvents.MerchantTownUpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantAddress(TestData.DomainEvents.MerchantPostalCodeUpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantAddress? merchantAddress = await context.MerchantAddresses.SingleOrDefaultAsync(c => c.AddressId == TestData.DomainEvents.AddressAddedEvent.AddressId, TestContext.Current.CancellationToken);
        merchantAddress.ShouldNotBeNull();
        merchantAddress.AddressLine1.ShouldBe(TestData.DomainEvents.MerchantAddressLine1UpdatedEvent.AddressLine1);
        merchantAddress.AddressLine2.ShouldBe(TestData.DomainEvents.MerchantAddressLine2UpdatedEvent.AddressLine2);
        merchantAddress.AddressLine3.ShouldBe(TestData.DomainEvents.MerchantAddressLine3UpdatedEvent.AddressLine3);
        merchantAddress.AddressLine4.ShouldBe(TestData.DomainEvents.MerchantAddressLine4UpdatedEvent.AddressLine4);
        merchantAddress.Country.ShouldBe(TestData.DomainEvents.MerchantCountyUpdatedEvent.Country);
        merchantAddress.Region.ShouldBe(TestData.DomainEvents.MerchantRegionUpdatedEvent.Region);
        merchantAddress.Town.ShouldBe(TestData.DomainEvents.MerchantTownUpdatedEvent.Town);
        merchantAddress.PostalCode.ShouldBe(TestData.DomainEvents.MerchantPostalCodeUpdatedEvent.PostalCode);
    }

    [Fact]
    public async Task UpdateMerchantContact_MerchantContactIsUpdated()
    {
        await this.CreateMerchantAsync();
        Result result = await this.Repository.AddMerchantContact(TestData.DomainEvents.ContactAddedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.UpdateMerchantContact(TestData.DomainEvents.MerchantContactNameUpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantContact(TestData.DomainEvents.MerchantContactEmailAddressUpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantContact(TestData.DomainEvents.MerchantContactPhoneNumberUpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantContact? merchantContact = await context.MerchantContacts.SingleOrDefaultAsync(c => c.ContactId == TestData.DomainEvents.ContactAddedEvent.ContactId, TestContext.Current.CancellationToken);
        merchantContact.ShouldNotBeNull();
        merchantContact.Name.ShouldBe(TestData.DomainEvents.MerchantContactNameUpdatedEvent.ContactName);
        merchantContact.EmailAddress.ShouldBe(TestData.DomainEvents.MerchantContactEmailAddressUpdatedEvent.ContactEmailAddress);
        merchantContact.PhoneNumber.ShouldBe(TestData.DomainEvents.MerchantContactPhoneNumberUpdatedEvent.ContactPhoneNumber);
    }

    [Fact]
    public async Task UpdateMerchantOpeningHours_MerchantOpeningHoursIsUpdated()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.UpdateMerchantOpeningHours(TestData.DomainEvents.MerchantOpeningHoursUpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.UpdateMerchantOpeningHours(CreateMerchantOpeningHoursUpdatedEvent(DayOfWeek.Tuesday, "0900", "1800"), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantOpeningHours(CreateMerchantOpeningHoursUpdatedEvent(DayOfWeek.Wednesday, "1000", "1900"), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantOpeningHours(CreateMerchantOpeningHoursUpdatedEvent(DayOfWeek.Thursday, "1100", "2000"), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantOpeningHours(CreateMerchantOpeningHoursUpdatedEvent(DayOfWeek.Friday, "1200", "2100"), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantOpeningHours(CreateMerchantOpeningHoursUpdatedEvent(DayOfWeek.Saturday, "1300", "2200"), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.UpdateMerchantOpeningHours(CreateMerchantOpeningHoursUpdatedEvent(DayOfWeek.Sunday, "1400", "2300"), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantOpeningHours? merchantOpeningHours = await context.MerchantOpeningHours.SingleOrDefaultAsync(c => c.MerchantId == TestData.MerchantId, TestContext.Current.CancellationToken);
        merchantOpeningHours.ShouldNotBeNull();
        merchantOpeningHours.MondayOpening.ShouldBe("0800");
        merchantOpeningHours.MondayClosing.ShouldBe("1700");
        merchantOpeningHours.TuesdayOpening.ShouldBe("0900");
        merchantOpeningHours.TuesdayClosing.ShouldBe("1800");
        merchantOpeningHours.WednesdayOpening.ShouldBe("1000");
        merchantOpeningHours.WednesdayClosing.ShouldBe("1900");
        merchantOpeningHours.ThursdayOpening.ShouldBe("1100");
        merchantOpeningHours.ThursdayClosing.ShouldBe("2000");
        merchantOpeningHours.FridayOpening.ShouldBe("1200");
        merchantOpeningHours.FridayClosing.ShouldBe("2100");
        merchantOpeningHours.SaturdayOpening.ShouldBe("1300");
        merchantOpeningHours.SaturdayClosing.ShouldBe("2200");
        merchantOpeningHours.SundayOpening.ShouldBe("1400");
        merchantOpeningHours.SundayClosing.ShouldBe("2300");
    }
}


