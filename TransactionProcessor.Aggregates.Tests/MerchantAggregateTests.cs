using System.Reflection;
using Shared.DomainDrivenDesign.EventSourcing;
using Shouldly;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
    public class MerchantAggregateTests{
        [Fact]
        public void MerchantAggregate_CanBeCreated_IsCreated(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            aggregate.AggregateId.ShouldBe(TestData.MerchantId);
            TransactionProcessor.Models.Merchant.Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.ShouldBeNull();
        }

        [Fact]
        public void MerchantAggregate_Create_IsCreated(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.GenerateReference();

            aggregate.AggregateId.ShouldBe(TestData.MerchantId);
            aggregate.EstateId.ShouldBe(TestData.EstateId);
            aggregate.Name.ShouldBe(TestData.MerchantName);
            aggregate.DateCreated.ShouldBe(TestData.DateMerchantCreated);
            aggregate.IsCreated.ShouldBeTrue();
            aggregate.MerchantReference.ShouldBe(TestData.MerchantReference);
        }

        [Fact]
        public async Task MerchantAggregate_Create_MerchantAlreadyCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            Should.NotThrow(() => { aggregate.Create(TestData.MerchantId, TestData.MerchantName, TestData.DateMerchantCreated); });
        }

        [Fact]
        public void MerchantAggregate_GenerateReference_CalledTwice_NoErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.GenerateReference();

            Should.NotThrow(() => { aggregate.GenerateReference(); });
        }

        [Fact]
        public void MerchantAggregate_AddAddress_AddressIsAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AddAddress(TestData.MerchantAddressLine1,
                                 TestData.MerchantAddressLine2,
                                 TestData.MerchantAddressLine3,
                                 TestData.MerchantAddressLine4,
                                 TestData.MerchantTown,
                                 TestData.MerchantRegion,
                                 TestData.MerchantPostalCode,
                                 TestData.MerchantCountry);

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Addresses.ShouldHaveSingleItem();
            Address addressModel = merchantModel.Addresses.Single();
            addressModel.AddressId.ShouldNotBe(Guid.Empty);
            addressModel.AddressLine1.ShouldBe(TestData.MerchantAddressLine1);
            addressModel.AddressLine2.ShouldBe(TestData.MerchantAddressLine2);
            addressModel.AddressLine3.ShouldBe(TestData.MerchantAddressLine3);
            addressModel.AddressLine4.ShouldBe(TestData.MerchantAddressLine4);
            addressModel.Town.ShouldBe(TestData.MerchantTown);
            addressModel.Region.ShouldBe(TestData.MerchantRegion);
            addressModel.PostalCode.ShouldBe(TestData.MerchantPostalCode);
            addressModel.Country.ShouldBe(TestData.MerchantCountry);
        }

        [Fact]
        public void MerchantAggregate_AddAddress_SameAddress_AddressIsNotAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AddAddress(TestData.MerchantAddressLine1,
                                 TestData.MerchantAddressLine2,
                                 TestData.MerchantAddressLine3,
                                 TestData.MerchantAddressLine4,
                                 TestData.MerchantTown,
                                 TestData.MerchantRegion,
                                 TestData.MerchantPostalCode,
                                 TestData.MerchantCountry);
            aggregate.AddAddress(TestData.MerchantAddressLine1Update,
                                 TestData.MerchantAddressLine2Update,
                                 TestData.MerchantAddressLine3Update,
                                 TestData.MerchantAddressLine4Update,
                                 TestData.MerchantTownUpdate,
                                 TestData.MerchantRegionUpdate,
                                 TestData.MerchantPostalCodeUpdate,
                                 TestData.MerchantCountryUpdate);

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Addresses.Count.ShouldBe(2);

            aggregate.AddAddress(TestData.MerchantAddressLine1,
                                 TestData.MerchantAddressLine2,
                                 TestData.MerchantAddressLine3,
                                 TestData.MerchantAddressLine4,
                                 TestData.MerchantTown,
                                 TestData.MerchantRegion,
                                 TestData.MerchantPostalCode,
                                 TestData.MerchantCountry);

            merchantModel = aggregate.GetMerchant();
            merchantModel.Addresses.Count.ShouldBe(2);
        }

        [Fact]
        public void MerchantAggregate_AddAddress_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => {
                                                                                              aggregate.AddAddress(TestData.MerchantAddressLine1,
                                                                                                                   TestData.MerchantAddressLine2,
                                                                                                                   TestData.MerchantAddressLine3,
                                                                                                                   TestData.MerchantAddressLine4,
                                                                                                                   TestData.MerchantTown,
                                                                                                                   TestData.MerchantRegion,
                                                                                                                   TestData.MerchantPostalCode,
                                                                                                                   TestData.MerchantCountry);
                                                                                          });

            exception.Message.ShouldContain($"Merchant has not been created");
        }

        [Fact]
        public void MerchantAggregate_AddContact_ContactIsAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AddContact(TestData.MerchantContactName,
                                 TestData.MerchantContactPhoneNumber,
                                 TestData.MerchantContactEmailAddress);

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Contacts.ShouldHaveSingleItem();
            Contact contactModel = merchantModel.Contacts.Single();
            //contactModel.ContactId.ShouldNotBe(Guid.Empty);
            contactModel.ContactName.ShouldBe(TestData.MerchantContactName);
            contactModel.ContactEmailAddress.ShouldBe(TestData.MerchantContactEmailAddress);
            contactModel.ContactPhoneNumber.ShouldBe(TestData.MerchantContactPhoneNumber);
        }

        [Fact]
        public void MerchantAggregate_AddContact_SecondContact_ContactIsAdded()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AddContact(TestData.MerchantContactName,
                                 TestData.MerchantContactPhoneNumber,
                                 TestData.MerchantContactEmailAddress);

            aggregate.AddContact(TestData.ContactName,
                                 TestData.ContactPhone,
                                 TestData.ContactEmail);

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Contacts.Count.ShouldBe(2);
        }

        [Fact]
        public void MerchantAggregate_AddContact_SameContact_ContactNotAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AddContact(TestData.MerchantContactName,
                                 TestData.MerchantContactPhoneNumber,
                                 TestData.MerchantContactEmailAddress);
            aggregate.AddContact(TestData.MerchantContactName,
                                 TestData.MerchantContactPhoneNumber,
                                 TestData.MerchantContactEmailAddress);

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Contacts.ShouldHaveSingleItem();
            Contact contactModel = merchantModel.Contacts.Single();
            //contactModel.ContactId.ShouldNotBe(Guid.Empty);
            contactModel.ContactName.ShouldBe(TestData.MerchantContactName);
            contactModel.ContactEmailAddress.ShouldBe(TestData.MerchantContactEmailAddress);
            contactModel.ContactPhoneNumber.ShouldBe(TestData.MerchantContactPhoneNumber);
        }

        [Fact]
        public void MerchantAggregate_AddContact_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => {
                                                                                              aggregate.AddContact(TestData.MerchantContactName,
                                                                                                                   TestData.MerchantContactPhoneNumber,
                                                                                                                   TestData.MerchantContactEmailAddress);
                                                                                          });

            exception.Message.ShouldContain($"Merchant has not been created");
        }

        [Fact]
        public void MerchantAggregate_AssignOperator_OperatorIsAssigned(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Operators.ShouldHaveSingleItem();
            Operator operatorModel = merchantModel.Operators.Single();
            operatorModel.OperatorId.ShouldBe(TestData.OperatorId);
            operatorModel.Name.ShouldBe(TestData.OperatorName);
            operatorModel.MerchantNumber.ShouldBe(TestData.OperatorMerchantNumber);
            operatorModel.TerminalNumber.ShouldBe(TestData.OperatorTerminalNumber);
        }

        [Fact]
        public void MerchantAggregate_AssignOperator_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Should.Throw<InvalidOperationException>(() => { aggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber); });
        }

        [Fact]
        public void MerchantAggregate_AssignOperator_OperatorAlreadyAssigned_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);

            Should.Throw<InvalidOperationException>(() => { aggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber); });
        }

        [Fact]
        public void MerchantAggregate_AddSecurityUserToMerchant_SecurityUserIsAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AddSecurityUser(TestData.SecurityUserId, TestData.MerchantUserEmailAddress);

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.SecurityUsers.ShouldHaveSingleItem();
            SecurityUser securityUser = merchantModel.SecurityUsers.Single();
            securityUser.EmailAddress.ShouldBe(TestData.MerchantUserEmailAddress);
            securityUser.SecurityUserId.ShouldBe(TestData.SecurityUserId);
        }

        [Fact]
        public void MerchantAggregate_AddSecurityUserToMerchant_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => { aggregate.AddSecurityUser(TestData.SecurityUserId, TestData.EstateUserEmailAddress); });

            exception.Message.ShouldContain("Merchant has not been created");
        }

        [Fact]
        public void MerchantAggregate_AddDevice_DeviceAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Devices.ShouldHaveSingleItem();
            merchantModel.Devices.Single().DeviceId.ShouldBe(TestData.DeviceId);
            merchantModel.Devices.Single().DeviceIdentifier.ShouldBe(TestData.DeviceIdentifier);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MerchantAggregate_AddDevice_DeviceIdentifierInvalid_ErrorThrown(String deviceIdentifier){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            Should.Throw<ArgumentNullException>(() => { aggregate.AddDevice(TestData.DeviceId, deviceIdentifier); });
        }

        [Fact]
        public void MerchantAggregate_AddDevice_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Should.Throw<InvalidOperationException>(() => { aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier); });
        }

        [Fact]
        public void MerchantAggregate_AddDevice_MerchantNoSpaceForDevice_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);

            Should.Throw<InvalidOperationException>(() => { aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier); });
        }

        [Fact]
        public void MerchantAggregate_AddDevice_DuplicateDevice_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);

            Should.Throw<InvalidOperationException>(() => { aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier); });
        }



        [Fact]
        public void MerchantAggregate_SetSetttlmentSchedule_ScheduleIsSet(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.SetSettlementSchedule(SettlementSchedule.Immediate);
            aggregate.SettlementSchedule.ShouldBe(SettlementSchedule.Immediate);
            aggregate.NextSettlementDueDate.ShouldBe(DateTime.MinValue);

            aggregate.SetSettlementSchedule(SettlementSchedule.Weekly);
            aggregate.SettlementSchedule.ShouldBe(SettlementSchedule.Weekly);
            aggregate.NextSettlementDueDate.ShouldBe(DateTime.Now.Date.AddDays(7));

            aggregate.SetSettlementSchedule(SettlementSchedule.Immediate);
            aggregate.SettlementSchedule.ShouldBe(SettlementSchedule.Immediate);
            aggregate.NextSettlementDueDate.ShouldBe(DateTime.MinValue);

            aggregate.SetSettlementSchedule(SettlementSchedule.Monthly);
            aggregate.SettlementSchedule.ShouldBe(SettlementSchedule.Monthly);
            aggregate.NextSettlementDueDate.ShouldBe(DateTime.Now.Date.AddMonths(1));
        }

        [Theory]
        [InlineData(SettlementSchedule.Immediate, SettlementSchedule.Immediate)]
        [InlineData(SettlementSchedule.Weekly, SettlementSchedule.Weekly)]
        [InlineData(SettlementSchedule.Monthly, SettlementSchedule.Monthly)]
        public void MerchantAggregate_SetSetttlmentSchedule_SameValue_NoEventRaised(SettlementSchedule originalSettlementSchedule, SettlementSchedule newSettlementSchedule){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.SetSettlementSchedule(originalSettlementSchedule);
            aggregate.SetSettlementSchedule(newSettlementSchedule);

            Type type = aggregate.GetType();
            PropertyInfo property = type.GetProperty("PendingEvents", BindingFlags.Instance | BindingFlags.NonPublic);
            Object value = property.GetValue(aggregate);
            value.ShouldNotBeNull();
            List<IDomainEvent> eventHistory = (List<IDomainEvent>)value;
            eventHistory.Count.ShouldBe(2);

            Merchant merchant = aggregate.GetMerchant();
            merchant.SettlementSchedule.ShouldBe(originalSettlementSchedule);
        }

        [Fact]
        public void MerchantAggregate_SwapDevice_DeviceIsSwapped(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);

            aggregate.SwapDevice(TestData.DeviceIdentifier, TestData.NewDeviceIdentifier);

            Merchant merchant = aggregate.GetMerchant();
            merchant.Devices.Count.ShouldBe(2);
            var originalDevice = merchant.Devices.Single(d => d.DeviceIdentifier == TestData.DeviceIdentifier);
            var newDevice = merchant.Devices.Single(d => d.DeviceIdentifier == TestData.NewDeviceIdentifier);
            originalDevice.IsEnabled.ShouldBeFalse();
            newDevice.IsEnabled.ShouldBeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void MerchantAggregate_SwapDevice_InvalidOriginalDeviceIdentifier_ErrorThrown(String originalDeviceIdentifier){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Should.Throw<ArgumentNullException>(() => { aggregate.SwapDevice(originalDeviceIdentifier, TestData.NewDeviceIdentifier); });
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void MerchantAggregate_SwapDevice_InvalidNewDeviceIdentifier_ErrorThrown(String newDeviceIdentifier){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Should.Throw<ArgumentNullException>(() => { aggregate.SwapDevice(TestData.DeviceIdentifier, newDeviceIdentifier); });
        }

        [Fact]
        public void MerchantAggregate_SwapDevice_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Should.Throw<InvalidOperationException>(() => { aggregate.SwapDevice(TestData.DeviceIdentifier, TestData.NewDeviceIdentifier); });
        }

        [Fact]
        public void MerchantAggregate_SwapDevice_MerchantDoesNotHaveOriginalDevice_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            Should.Throw<InvalidOperationException>(() => { aggregate.SwapDevice(TestData.DeviceIdentifier, TestData.NewDeviceIdentifier); });
        }

        [Fact]
        public void MerchantAggregate_SwapDevice_MerchantAlreadyHasNewDevice_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AddDevice(TestData.DeviceId, TestData.NewDeviceIdentifier);
            Should.Throw<InvalidOperationException>(() => { aggregate.SwapDevice(TestData.NewDeviceIdentifier, TestData.NewDeviceIdentifier); });
        }

        [Fact]
        public void MerchantAggregate_AddContract_ContractAndProductsAddedToMerchant(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            ContractAggregate contractAggregate = TestData.Aggregates.CreatedContractAggregateWithAProduct();

            merchantAggregate.AddContract(contractAggregate);

            Merchant merchant = merchantAggregate.GetMerchant();
            merchant.Contracts.Count.ShouldBe(1);
            Contract contract = merchant.Contracts.SingleOrDefault();
            contract.ShouldNotBeNull();
            contract.ContractProducts.Count.ShouldBe(contractAggregate.GetProducts().Count);

        }

        [Fact]
        public void MerchantAggregate_AddContract_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

            ContractAggregate contractAggregate = TestData.Aggregates.CreatedContractAggregateWithAProduct();
            Should.Throw<InvalidOperationException>(() => { merchantAggregate.AddContract(contractAggregate); });
        }

        [Fact]
        public void MerchantAggregate_AddContract_ContractAlreadyAdded_ErrorThrown(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            ContractAggregate contractAggregate = TestData.Aggregates.CreatedContractAggregateWithAProduct();
            merchantAggregate.AddContract(contractAggregate);

            Should.Throw<InvalidOperationException>(() => { merchantAggregate.AddContract(contractAggregate); });
        }

        [Fact]
        public void MerchantAggregate_UpdateMerchant_NameUpdated_ErrorThrown(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            merchantAggregate.UpdateMerchant(TestData.MerchantNameUpdated);

            merchantAggregate.Name.ShouldBe(TestData.MerchantNameUpdated);
        }

        [Fact]
        public void MerchantAggregate_UpdateMerchant_SameName_NoUpdate_ErrorThrown(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            merchantAggregate.UpdateMerchant(TestData.MerchantName);

            merchantAggregate.Name.ShouldBe(TestData.MerchantName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void MerchantAggregate_UpdateMerchant_NameNotSet_NoUpdate_ErrorThrown(String merchantName){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            merchantAggregate.UpdateMerchant(merchantName);

            merchantAggregate.Name.ShouldBe(TestData.MerchantName);
        }

        [Fact]
        public void MerchantAggregate_UpdateAddress_AddressIsAdded(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            merchantAggregate.AddAddress(TestData.MerchantAddressLine1,
                                         TestData.MerchantAddressLine2,
                                         TestData.MerchantAddressLine3,
                                         TestData.MerchantAddressLine4,
                                         TestData.MerchantTown,
                                         TestData.MerchantRegion,
                                         TestData.MerchantPostalCode,
                                         TestData.MerchantCountry);

            Merchant merchantModel = merchantAggregate.GetMerchant();
            var address = merchantModel.Addresses.ShouldHaveSingleItem();
            address.AddressLine1.ShouldBe(TestData.MerchantAddressLine1);
            address.AddressLine2.ShouldBe(TestData.MerchantAddressLine2);
            address.AddressLine3.ShouldBe(TestData.MerchantAddressLine3);
            address.AddressLine4.ShouldBe(TestData.MerchantAddressLine4);
            address.Town.ShouldBe(TestData.MerchantTown);
            address.Region.ShouldBe(TestData.MerchantRegion);
            address.PostalCode.ShouldBe(TestData.MerchantPostalCode);
            address.Country.ShouldBe(TestData.MerchantCountry);

            merchantAggregate.UpdateAddress(address.AddressId,
                                            TestData.MerchantAddressLine1Update,
                                            TestData.MerchantAddressLine2Update,
                                            TestData.MerchantAddressLine3Update,
                                            TestData.MerchantAddressLine4Update,
                                            TestData.MerchantTownUpdate,
                                            TestData.MerchantRegionUpdate,
                                            TestData.MerchantPostalCodeUpdate,
                                            TestData.MerchantCountryUpdate);

            merchantModel = merchantAggregate.GetMerchant();
            address = merchantModel.Addresses.ShouldHaveSingleItem();
            address.AddressLine1.ShouldBe(TestData.MerchantAddressLine1Update);
            address.AddressLine2.ShouldBe(TestData.MerchantAddressLine2Update);
            address.AddressLine3.ShouldBe(TestData.MerchantAddressLine3Update);
            address.AddressLine4.ShouldBe(TestData.MerchantAddressLine4Update);
            address.Town.ShouldBe(TestData.MerchantTownUpdate);
            address.Region.ShouldBe(TestData.MerchantRegionUpdate);
            address.PostalCode.ShouldBe(TestData.MerchantPostalCodeUpdate);
            address.Country.ShouldBe(TestData.MerchantCountryUpdate);
        }

        [Fact]
        public void MerchantAggregate_UpdateAddress_AddressNotFound_NoErrorThrown(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            Should.NotThrow(() => {
                                merchantAggregate.UpdateAddress(Guid.NewGuid(),
                                                                TestData.MerchantAddressLine1Update,
                                                                TestData.MerchantAddressLine2Update,
                                                                TestData.MerchantAddressLine3Update,
                                                                TestData.MerchantAddressLine4Update,
                                                                TestData.MerchantTownUpdate,
                                                                TestData.MerchantRegionUpdate,
                                                                TestData.MerchantPostalCodeUpdate,
                                                                TestData.MerchantCountryUpdate);
                            });
        }

        [Fact]
        public void MerchantAggregate_UpdateAddress_UpdatedAddressHasNotChanges_NoErrorThrown(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            merchantAggregate.AddAddress(TestData.MerchantAddressLine1,
                                         TestData.MerchantAddressLine2,
                                         TestData.MerchantAddressLine3,
                                         TestData.MerchantAddressLine4,
                                         TestData.MerchantTown,
                                         TestData.MerchantRegion,
                                         TestData.MerchantPostalCode,
                                         TestData.MerchantCountry);

            Merchant merchantModel = merchantAggregate.GetMerchant();
            var address = merchantModel.Addresses.ShouldHaveSingleItem();
            address.AddressLine1.ShouldBe(TestData.MerchantAddressLine1);
            address.AddressLine2.ShouldBe(TestData.MerchantAddressLine2);
            address.AddressLine3.ShouldBe(TestData.MerchantAddressLine3);
            address.AddressLine4.ShouldBe(TestData.MerchantAddressLine4);
            address.Town.ShouldBe(TestData.MerchantTown);
            address.Region.ShouldBe(TestData.MerchantRegion);
            address.PostalCode.ShouldBe(TestData.MerchantPostalCode);
            address.Country.ShouldBe(TestData.MerchantCountry);

            merchantAggregate.UpdateAddress(address.AddressId,
                                            TestData.MerchantAddressLine1,
                                            TestData.MerchantAddressLine2,
                                            TestData.MerchantAddressLine3,
                                            TestData.MerchantAddressLine4,
                                            TestData.MerchantTown,
                                            TestData.MerchantRegion,
                                            TestData.MerchantPostalCode,
                                            TestData.MerchantCountry);

            merchantModel = merchantAggregate.GetMerchant();
            address = merchantModel.Addresses.ShouldHaveSingleItem();
            address.AddressLine1.ShouldBe(TestData.MerchantAddressLine1);
            address.AddressLine2.ShouldBe(TestData.MerchantAddressLine2);
            address.AddressLine3.ShouldBe(TestData.MerchantAddressLine3);
            address.AddressLine4.ShouldBe(TestData.MerchantAddressLine4);
            address.Town.ShouldBe(TestData.MerchantTown);
            address.Region.ShouldBe(TestData.MerchantRegion);
            address.PostalCode.ShouldBe(TestData.MerchantPostalCode);
            address.Country.ShouldBe(TestData.MerchantCountry);
        }

        [Fact]
        public void MerchantAggregate_UpdateContact_ContactIsUpdated(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            merchantAggregate.AddContact(TestData.ContactName, TestData.ContactPhone, TestData.ContactEmail);

            Merchant merchantModel = merchantAggregate.GetMerchant();
            var contact = merchantModel.Contacts.ShouldHaveSingleItem();
            contact.ContactName.ShouldBe(TestData.ContactName);
            contact.ContactEmailAddress.ShouldBe(TestData.ContactEmail);
            contact.ContactPhoneNumber.ShouldBe(TestData.ContactPhone);

            merchantAggregate.UpdateContact(contact.ContactId,
                                            TestData.ContactNameUpdate,
                                            TestData.ContactEmailUpdate,
                                            TestData.ContactPhoneUpdate);

            merchantModel = merchantAggregate.GetMerchant();
            contact = merchantModel.Contacts.ShouldHaveSingleItem();
            contact.ContactName.ShouldBe(TestData.ContactNameUpdate);
            contact.ContactEmailAddress.ShouldBe(TestData.ContactEmailUpdate);
            contact.ContactPhoneNumber.ShouldBe(TestData.ContactPhoneUpdate);
        }

        [Fact]
        public void MerchantAggregate_UpdateContact_ContactNotFound_NoErrorThrown(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

            Should.NotThrow(() => {
                                merchantAggregate.UpdateContact(Guid.NewGuid(),
                                                                TestData.ContactName,
                                                                TestData.ContactEmail,
                                                                TestData.ContactPhone);
                            });
        }

        [Fact]
        public void MerchantAggregate_UpdateContact_UpdatedContactHasNotChanges_NoErrorThrown(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);
            merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            merchantAggregate.AddContact(TestData.ContactName, TestData.ContactPhone, TestData.ContactEmail);

            Merchant merchantModel = merchantAggregate.GetMerchant();
            var contact = merchantModel.Contacts.ShouldHaveSingleItem();
            contact.ContactName.ShouldBe(TestData.ContactName);
            contact.ContactEmailAddress.ShouldBe(TestData.ContactEmail);
            contact.ContactPhoneNumber.ShouldBe(TestData.ContactPhone);

            merchantAggregate.UpdateContact(contact.ContactId, TestData.ContactName, TestData.ContactEmail, TestData.ContactPhone);

            merchantModel = merchantAggregate.GetMerchant();
            contact = merchantModel.Contacts.ShouldHaveSingleItem();
            contact.ContactName.ShouldBe(TestData.ContactName);
            contact.ContactEmailAddress.ShouldBe(TestData.ContactEmail);
            contact.ContactPhoneNumber.ShouldBe(TestData.ContactPhone);
        }

        [Fact]
        public void MerchantAggregate_RemoveOperator_OperatorIsRemoved()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);
            aggregate.RemoveOperator(TestData.OperatorId);

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Operators.ShouldHaveSingleItem();
            Operator operatorModel = merchantModel.Operators.Single();
            operatorModel.OperatorId.ShouldBe(TestData.OperatorId);
            operatorModel.Name.ShouldBe(TestData.OperatorName);
            operatorModel.MerchantNumber.ShouldBe(TestData.OperatorMerchantNumber);
            operatorModel.TerminalNumber.ShouldBe(TestData.OperatorTerminalNumber);
            operatorModel.IsDeleted.ShouldBeTrue();
        }

        [Fact]
        public void MerchantAggregate_RemoveOperator_MerchantNotCreated_ErrorThrown()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Should.Throw<InvalidOperationException>(() => { aggregate.RemoveOperator(TestData.OperatorId); });
        }

        [Fact]
        public void MerchantAggregate_AssignOperator_OperatorNotAlreadyAssigned_ErrorThrown()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            
            Should.Throw<InvalidOperationException>(() => { aggregate.RemoveOperator(TestData.OperatorId); });
        }

        [Fact]
        public void MerchantAggregate_RemoveContract_ContractIsRemoved()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            aggregate.AddContract(TestData.Aggregates.CreatedContractAggregate());
            aggregate.RemoveContract(TestData.ContractId);

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Contracts.ShouldHaveSingleItem();
            Contract contractModel = merchantModel.Contracts.Single();
            contractModel.ContractId.ShouldBe(TestData.ContractId);
            contractModel.IsDeleted.ShouldBeTrue();
        }

        [Fact]
        public void MerchantAggregate_RemoveContract_MerchantNotCreated_ErrorThrown()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            
            Should.Throw<InvalidOperationException>(() =>
            {
                aggregate.RemoveContract(TestData.ContractId);
            });
        }

        [Fact]
        public void MerchantAggregate_RemoveContract_MerchantDoesNotHaveContract_ErrorThrown()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.RemoveContract(TestData.ContractId);
                                                    });
        }
    }
}
