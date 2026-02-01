using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents {

    [ExcludeFromCodeCoverage]
    public class MerchantDomainEvents {
        public record AddressAddedEvent(Guid MerchantId, Guid EstateId, Guid AddressId, String AddressLine1, String AddressLine2, String AddressLine3, String AddressLine4, String Town, String Region, String PostalCode, String Country) : DomainEvent(MerchantId, Guid.NewGuid());
        public record AutomaticDepositMadeEvent(Guid MerchantId, Guid EstateId, Guid DepositId, String Reference, DateTime DepositDateTime, Decimal Amount) : DomainEvent(MerchantId, Guid.NewGuid());
        public record ContactAddedEvent(Guid MerchantId, Guid EstateId, Guid ContactId, String ContactName, String ContactPhoneNumber, String ContactEmailAddress) : DomainEvent(MerchantId, Guid.NewGuid());
        public record DeviceAddedToMerchantEvent(Guid MerchantId, Guid EstateId, Guid DeviceId, String DeviceIdentifier) : DomainEvent(MerchantId, Guid.NewGuid());
        public record DeviceSwappedForMerchantEvent(Guid MerchantId, Guid EstateId, Guid DeviceId, Guid OriginalDeviceId, String NewDeviceIdentifier) : DomainEvent(MerchantId, Guid.NewGuid());
        public record ManualDepositMadeEvent(Guid MerchantId, Guid EstateId, Guid DepositId, String Reference, DateTime DepositDateTime, Decimal Amount) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantCreatedEvent(Guid MerchantId, Guid EstateId, String MerchantName, DateTime DateCreated) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantDepositListCreatedEvent(Guid MerchantId, Guid EstateId, DateTime DateCreated) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantReferenceAllocatedEvent(Guid MerchantId, Guid EstateId, String MerchantReference) : DomainEvent(MerchantId, Guid.NewGuid());
        public record OperatorAssignedToMerchantEvent(Guid MerchantId, Guid EstateId, Guid OperatorId, String Name, String MerchantNumber, String TerminalNumber) : DomainEvent(MerchantId, Guid.NewGuid());
        public record OperatorRemovedFromMerchantEvent(Guid MerchantId, Guid EstateId, Guid OperatorId) : DomainEvent(MerchantId, Guid.NewGuid());
        public record SecurityUserAddedToMerchantEvent(Guid MerchantId, Guid EstateId, Guid SecurityUserId, String EmailAddress) : DomainEvent(MerchantId, Guid.NewGuid());
        public record SettlementScheduleChangedEvent(Guid MerchantId, Guid EstateId, Int32 SettlementSchedule, DateTime NextSettlementDate) : DomainEvent(MerchantId, Guid.NewGuid());
        public record WithdrawalMadeEvent(Guid MerchantId, Guid EstateId, Guid WithdrawalId, DateTime WithdrawalDateTime, Decimal Amount) : DomainEvent(MerchantId, Guid.NewGuid());
        public record ContractAddedToMerchantEvent(Guid MerchantId, Guid EstateId, Guid ContractId) : DomainEvent(MerchantId, Guid.NewGuid());
        public record ContractRemovedFromMerchantEvent(Guid MerchantId, Guid EstateId, Guid ContractId) : DomainEvent(MerchantId, Guid.NewGuid());
        public record ContractProductAddedToMerchantEvent(Guid MerchantId, Guid EstateId, Guid ContractId, Guid ContractProductId) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantNameUpdatedEvent(Guid MerchantId, Guid EstateId, String MerchantName) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantAddressLine1UpdatedEvent(Guid MerchantId, Guid EstateId, Guid AddressId, String AddressLine1) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantAddressLine2UpdatedEvent(Guid MerchantId, Guid EstateId, Guid AddressId, String AddressLine2) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantAddressLine3UpdatedEvent(Guid MerchantId, Guid EstateId, Guid AddressId, String AddressLine3) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantAddressLine4UpdatedEvent(Guid MerchantId, Guid EstateId, Guid AddressId, String AddressLine4) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantCountyUpdatedEvent(Guid MerchantId, Guid EstateId, Guid AddressId, String Country) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantRegionUpdatedEvent(Guid MerchantId, Guid EstateId, Guid AddressId, String Region) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantTownUpdatedEvent(Guid MerchantId, Guid EstateId, Guid AddressId, String Town) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantPostalCodeUpdatedEvent(Guid MerchantId, Guid EstateId, Guid AddressId, String PostalCode) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantContactNameUpdatedEvent(Guid MerchantId, Guid EstateId, Guid ContactId, String ContactName) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantContactEmailAddressUpdatedEvent(Guid MerchantId, Guid EstateId, Guid ContactId, String ContactEmailAddress) : DomainEvent(MerchantId, Guid.NewGuid());
        public record MerchantContactPhoneNumberUpdatedEvent(Guid MerchantId, Guid EstateId, Guid ContactId, String ContactPhoneNumber) : DomainEvent(MerchantId, Guid.NewGuid());
    }
}
