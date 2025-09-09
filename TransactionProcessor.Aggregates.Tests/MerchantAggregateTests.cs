using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shouldly;
using SimpleResults;
using System.Reflection;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;
using static TransactionProcessor.Testing.TestData;
using Address = TransactionProcessor.Models.Merchant.Address;
using Contact = TransactionProcessor.Models.Merchant.Contact;
using SettlementSchedule = TransactionProcessor.Models.Merchant.SettlementSchedule;

namespace TransactionProcessor.Aggregates.Tests
{
    public class MerchantAggregateTests{
        [Fact]
        public void MerchantAggregate_CanBeCreated_IsCreated(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            aggregate.AggregateId.ShouldBe(TestData.MerchantId);
            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.ShouldBeNull();
        }
        
        [Fact]
        public void MerchantAggregate_Create_IsCreated(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            Result result = aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            
            result.IsSuccess.ShouldBeTrue();

            aggregate.AggregateId.ShouldBe(TestData.MerchantId);
            aggregate.EstateId.ShouldBe(TestData.EstateId);
            aggregate.Name.ShouldBe(TestData.MerchantName);
            aggregate.DateCreated.ShouldBe(TestData.DateMerchantCreated);
            aggregate.IsCreated.ShouldBeTrue();
            aggregate.MerchantReference.ShouldBe(TestData.MerchantReference);
            aggregate.SettlementSchedule.ShouldBe(TestData.SettlementScheduleModel);
            
            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.ShouldNotBeNull();
            merchantModel.MerchantId.ShouldBe(TestData.MerchantId);
            merchantModel.EstateId.ShouldBe(TestData.EstateId);
            merchantModel.MerchantName.ShouldBe(TestData.MerchantName);
            merchantModel.Reference.ShouldBe(TestData.MerchantReference);
            merchantModel.SettlementSchedule.ShouldBe(TestData.SettlementScheduleModel);
            merchantModel.Addresses.Count.ShouldBe(1);
            merchantModel.Contacts.Count.ShouldBe(1);
        }
        
        [Fact]
        public void MerchantAggregate_Create_MerchantAlreadyCreated_NoError(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            Result result = aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            result.IsSuccess.ShouldBeTrue();
            result = aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            result.IsSuccess.ShouldBeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MerchantAggregate_Create_NoName_ErrorReturned(String merchantName)
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            Result result = aggregate.Create(TestData.EstateId, merchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("Merchant name must be provided when registering a new merchant");
        }

        [Fact]
        public void MerchantAggregate_Create_NoAddress_ErrorReturned()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            Result result = aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, null, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("An Address must be provided when registering a new merchant");
        }

        [Fact]
        public void MerchantAggregate_Create_NoContact_ErrorReturned()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            Result result = aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, null,
                TestData.SettlementScheduleModel);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("A Contact must be provided when registering a new merchant");
        }

        [Fact]
        public void MerchantAggregate_Create_InvalidSettlementSchedule_ErrorReturned()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            Result result = aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModelNotSet);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("A valid settlement schedule must be provided when registering a new merchant");
        }

        
        [Fact]
        public void MerchantAggregate_AddAddress_AddressIsAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Address newAddress = new(Guid.Empty, TestData.MerchantAddressLine1,
                TestData.MerchantAddressLine2,
                TestData.MerchantAddressLine3,
                TestData.MerchantAddressLine4,
                TestData.MerchantTown,
                TestData.MerchantRegion,
                TestData.MerchantPostalCode,
                TestData.MerchantCountry);

            Result result = aggregate.AddAddress(newAddress);
            result.IsSuccess.ShouldBeTrue();
            
            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Addresses.Count.ShouldBe(2);
            
            Address addressModel = merchantModel.Addresses.SingleOrDefault(a => a.AddressLine1 == TestData.MerchantAddressLine1);
            addressModel.ShouldNotBeNull();
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
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);



            Result result = aggregate.AddAddress(TestData.AddressModel);
            result.IsSuccess.ShouldBeTrue();
            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Addresses.ShouldHaveSingleItem();
        }
        
        [Fact]
        public void MerchantAggregate_AddAddress_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Result result = aggregate.AddAddress(TestData.AddressModel);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain($"Merchant has not been created");
        }
        
        [Fact]
        public void MerchantAggregate_AddContact_ContactIsAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Result result = aggregate.AddContact(TestData.MerchantContactName,
                                 TestData.MerchantContactPhoneNumber,
                                 TestData.MerchantContactEmailAddress);

            result.IsSuccess.ShouldBeTrue();

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Contacts.Count.ShouldBe(2);
            Contact contactModel = merchantModel.Contacts.SingleOrDefault(c => c.ContactName == TestData.MerchantContactName);
            contactModel.ShouldNotBeNull();
            contactModel.ContactName.ShouldBe(TestData.MerchantContactName);
            contactModel.ContactEmailAddress.ShouldBe(TestData.MerchantContactEmailAddress);
            contactModel.ContactPhoneNumber.ShouldBe(TestData.MerchantContactPhoneNumber);
        }

        [Fact]
        public void MerchantAggregate_AddContact_SameContact_ContactNotAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            
            Result result = aggregate.AddContact(TestData.ContactModel.ContactName,
                TestData.ContactModel.ContactPhoneNumber,
                TestData.ContactModel.ContactEmailAddress);
            result.IsSuccess.ShouldBeTrue();

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.Contacts.ShouldHaveSingleItem();
        }

        [Fact]
        public void MerchantAggregate_AddContact_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Result result = aggregate.AddContact(TestData.MerchantContactName,
                                                                                                                   TestData.MerchantContactPhoneNumber,
                                                                                                                   TestData.MerchantContactEmailAddress);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain($"Merchant has not been created");
        }
        
        [Fact]
        public void MerchantAggregate_AssignOperator_OperatorIsAssigned(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            Result result = aggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);
            result.IsSuccess.ShouldBeTrue();

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

            Result result = aggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantAggregate_AssignOperator_OperatorAlreadyAssigned_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            aggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);

            Result result = aggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
        
        [Fact]
        public void MerchantAggregate_AddSecurityUserToMerchant_SecurityUserIsAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Result result = aggregate.AddSecurityUser(TestData.SecurityUserId, TestData.MerchantUserEmailAddress);
            result.IsSuccess.ShouldBeTrue();

            Merchant merchantModel = aggregate.GetMerchant();
            merchantModel.SecurityUsers.ShouldHaveSingleItem();
            SecurityUser securityUser = merchantModel.SecurityUsers.Single();
            securityUser.EmailAddress.ShouldBe(TestData.MerchantUserEmailAddress);
            securityUser.SecurityUserId.ShouldBe(TestData.SecurityUserId);
        }

        [Fact]
        public void MerchantAggregate_AddSecurityUserToMerchant_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Result result = aggregate.AddSecurityUser(TestData.SecurityUserId, TestData.EstateUserEmailAddress);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain("Merchant has not been created");
        }
        
        [Fact]
        public void MerchantAggregate_AddDevice_DeviceAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Result result = aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);
            result.IsSuccess.ShouldBeTrue();

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
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Result result = aggregate.AddDevice(TestData.DeviceId, deviceIdentifier);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantAggregate_AddDevice_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Result result = aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantAggregate_AddDevice_MerchantNoSpaceForDevice_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);

            Result result = aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantAggregate_AddDevice_DuplicateDevice_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);

            Result result = aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
        
        [Fact]
        public void MerchantAggregate_SetSetttlmentSchedule_ScheduleIsSet(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            Result result = aggregate.SetSettlementSchedule(SettlementSchedule.Immediate);
            result.IsSuccess.ShouldBeTrue();
            aggregate.SettlementSchedule.ShouldBe(SettlementSchedule.Immediate);
            aggregate.NextSettlementDueDate.ShouldBe(DateTime.MinValue);

            result = aggregate.SetSettlementSchedule(SettlementSchedule.Weekly);
            result.IsSuccess.ShouldBeTrue();
            aggregate.SettlementSchedule.ShouldBe(SettlementSchedule.Weekly);
            aggregate.NextSettlementDueDate.ShouldBe(DateTime.Now.Date.AddDays(7));

            result = aggregate.SetSettlementSchedule(SettlementSchedule.Immediate);
            result.IsSuccess.ShouldBeTrue();
            aggregate.SettlementSchedule.ShouldBe(SettlementSchedule.Immediate);
            aggregate.NextSettlementDueDate.ShouldBe(DateTime.MinValue);

            result = aggregate.SetSettlementSchedule(SettlementSchedule.Monthly);
            result.IsSuccess.ShouldBeTrue();
            aggregate.SettlementSchedule.ShouldBe(SettlementSchedule.Monthly);
            aggregate.NextSettlementDueDate.ShouldBe(DateTime.Now.Date.AddMonths(1));
        }

        [Fact]
        public void MerchantAggregate_SetSetttlmentSchedule_MerchantNotCreated_ErrorReturned()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            
            Result result = aggregate.SetSettlementSchedule(SettlementSchedule.Immediate);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }


        [Fact]
        public void MerchantAggregate_SetSetttlmentSchedule_NotSet_ErrorReturned()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            Result result = aggregate.SetSettlementSchedule(SettlementSchedule.NotSet);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(SettlementSchedule.Immediate, SettlementSchedule.Immediate)]
        [InlineData(SettlementSchedule.Weekly, SettlementSchedule.Weekly)]
        [InlineData(SettlementSchedule.Monthly, SettlementSchedule.Monthly)]
        public void MerchantAggregate_SetSetttlmentSchedule_SameValue_NoEventRaised(SettlementSchedule originalSettlementSchedule, SettlementSchedule newSettlementSchedule){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                originalSettlementSchedule);

            Result result = aggregate.SetSettlementSchedule(newSettlementSchedule);
            result.IsSuccess.ShouldBeTrue();

            Type type = aggregate.GetType();
            PropertyInfo property = type.GetProperty("PendingEvents", BindingFlags.Instance | BindingFlags.NonPublic);
            Object value = property.GetValue(aggregate);
            value.ShouldNotBeNull();
            List<IDomainEvent> eventHistory = (List<IDomainEvent>)value;
            eventHistory.Count.ShouldBe(5);

            Merchant merchant = aggregate.GetMerchant();
            merchant.SettlementSchedule.ShouldBe(originalSettlementSchedule);
        }
        
        [Fact]
        public void MerchantAggregate_SwapDevice_DeviceIsSwapped(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            aggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);

            Result result = aggregate.SwapDevice(TestData.DeviceIdentifier, TestData.NewDeviceIdentifier);
            result.IsSuccess.ShouldBeTrue();

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

            Result result = aggregate.SwapDevice(originalDeviceIdentifier, TestData.NewDeviceIdentifier);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void MerchantAggregate_SwapDevice_InvalidNewDeviceIdentifier_ErrorThrown(String newDeviceIdentifier){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Result result = aggregate.SwapDevice(TestData.DeviceIdentifier, newDeviceIdentifier);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantAggregate_SwapDevice_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Result result = aggregate.SwapDevice(TestData.DeviceIdentifier, TestData.NewDeviceIdentifier);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantAggregate_SwapDevice_MerchantDoesNotHaveOriginalDevice_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            Result result = aggregate.SwapDevice(TestData.DeviceIdentifier, TestData.NewDeviceIdentifier);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantAggregate_SwapDevice_MerchantAlreadyHasNewDevice_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            aggregate.AddDevice(TestData.DeviceId, TestData.NewDeviceIdentifier);

            Result result = aggregate.SwapDevice(TestData.NewDeviceIdentifier, TestData.NewDeviceIdentifier);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
        
        [Fact]
        public void MerchantAggregate_AddContract_ContractAndProductsAddedToMerchant(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            ContractAggregate contractAggregate = TestData.Aggregates.CreatedContractAggregateWithAProduct();

            aggregate.AddContract(contractAggregate);

            Merchant merchant = aggregate.GetMerchant();
            merchant.Contracts.Count.ShouldBe(1);
            Contract contract = merchant.Contracts.SingleOrDefault();
            contract.ShouldNotBeNull();
            contract.ContractProducts.Count.ShouldBe(contractAggregate.GetProducts().Count);

        }

        [Fact]
        public void MerchantAggregate_AddContract_MerchantNotCreated_ErrorThrown(){
            MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

            ContractAggregate contractAggregate = TestData.Aggregates.CreatedContractAggregateWithAProduct();
            Result result = merchantAggregate.AddContract(contractAggregate);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantAggregate_AddContract_ContractAlreadyAdded_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            ContractAggregate contractAggregate = TestData.Aggregates.CreatedContractAggregateWithAProduct();
            aggregate.AddContract(contractAggregate);

            Result result = aggregate.AddContract(contractAggregate);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
        
        [Fact]
        public void MerchantAggregate_UpdateMerchant_NameUpdated_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Result result = aggregate.UpdateMerchant(TestData.MerchantNameUpdated);
            result.IsSuccess.ShouldBeTrue();
            aggregate.Name.ShouldBe(TestData.MerchantNameUpdated);
        }

        [Fact]
        public void MerchantAggregate_UpdateMerchant_SameName_NoUpdate_ErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Result result = aggregate.UpdateMerchant(TestData.MerchantName);
            result.IsSuccess.ShouldBeTrue();
            aggregate.Name.ShouldBe(TestData.MerchantName);
        }

        [Fact]
        public void MerchantAggregate_UpdateMerchant_MerchantNotCreated_ErrorThrown()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            
            Result result = aggregate.UpdateMerchant(TestData.MerchantNameUpdated);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void MerchantAggregate_UpdateMerchant_NameNotSet_NoUpdate_ErrorThrown(String merchantName){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Result result = aggregate.UpdateMerchant(merchantName);
            result.IsSuccess.ShouldBeTrue();
            aggregate.Name.ShouldBe(TestData.MerchantName);
        }

        [Fact]
        public void MerchantAggregate_UpdateAddress_AddressIsAdded(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Merchant merchantModel = aggregate.GetMerchant();
            Address? address = merchantModel.Addresses.ShouldHaveSingleItem();

            Address newAddress = new(address.AddressId, TestData.MerchantAddressLine1Update, TestData.MerchantAddressLine2Update, TestData.MerchantAddressLine3Update, TestData.MerchantAddressLine4Update, TestData.MerchantTownUpdate, TestData.MerchantRegionUpdate, TestData.MerchantPostalCodeUpdate, TestData.MerchantCountryUpdate);
            Result result = aggregate.UpdateAddress(newAddress);
            result.IsSuccess.ShouldBeTrue();

            merchantModel = aggregate.GetMerchant();
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
        public void MerchantAggregate_UpdateAddress_MerchantNotCreated_ErrorThrown()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Address newAddress = new(Guid.NewGuid(), TestData.MerchantAddressLine1Update, TestData.MerchantAddressLine2Update, TestData.MerchantAddressLine3Update, TestData.MerchantAddressLine4Update, TestData.MerchantTownUpdate, TestData.MerchantRegionUpdate, TestData.MerchantPostalCodeUpdate, TestData.MerchantCountryUpdate);
            Result result = aggregate.UpdateAddress(newAddress);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantAggregate_UpdateAddress_AddressNotFound_NoErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Address newAddress = new(Guid.NewGuid(), TestData.MerchantAddressLine1Update, TestData.MerchantAddressLine2Update, TestData.MerchantAddressLine3Update, TestData.MerchantAddressLine4Update, TestData.MerchantTownUpdate, TestData.MerchantRegionUpdate, TestData.MerchantPostalCodeUpdate, TestData.MerchantCountryUpdate);

            Result result = aggregate.UpdateAddress(newAddress);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void MerchantAggregate_UpdateAddress_UpdatedAddressHasNotChanges_NoErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Merchant merchantModel = aggregate.GetMerchant();
            Address? address = merchantModel.Addresses.ShouldHaveSingleItem();

            Address newAddress = new (address.AddressId, address.AddressLine1, address.AddressLine2, address.AddressLine3, address.AddressLine4, address.Town, address.Region, address.PostalCode, address.Country);

            Result result = aggregate.UpdateAddress(newAddress);

            result.IsSuccess.ShouldBeTrue();
            merchantModel = aggregate.GetMerchant();
            address = merchantModel.Addresses.ShouldHaveSingleItem();
            address.AddressLine1.ShouldBe(address.AddressLine1);
            address.AddressLine2.ShouldBe(address.AddressLine2);
            address.AddressLine3.ShouldBe(address.AddressLine3);
            address.AddressLine4.ShouldBe(address.AddressLine4);
            address.Town.ShouldBe(address.Town);
            address.Region.ShouldBe(address.Region);
            address.PostalCode.ShouldBe(address.PostalCode);
            address.Country.ShouldBe(address.Country);
        }
        
        [Fact]
        public void MerchantAggregate_UpdateContact_ContactIsUpdated(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Merchant merchantModel = aggregate.GetMerchant();
            var contact = merchantModel.Contacts.ShouldHaveSingleItem();

            aggregate.UpdateContact(contact.ContactId,
                                            TestData.ContactNameUpdate,
                                            TestData.ContactEmailUpdate,
                                            TestData.ContactPhoneUpdate);

            merchantModel = aggregate.GetMerchant();
            contact = merchantModel.Contacts.ShouldHaveSingleItem();
            contact.ContactName.ShouldBe(TestData.ContactNameUpdate);
            contact.ContactEmailAddress.ShouldBe(TestData.ContactEmailUpdate);
            contact.ContactPhoneNumber.ShouldBe(TestData.ContactPhoneUpdate);
        }

        [Fact]
        public void MerchantAggregate_UpdateContact_ContactNotFound_NoErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Result result = aggregate.UpdateContact(Guid.NewGuid(),
                                                                TestData.ContactName,
                                                                TestData.ContactEmail,
                                                                TestData.ContactPhone);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void MerchantAggregate_UpdateContact_UpdatedContactHasNotChanges_NoErrorThrown(){
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Merchant merchantModel = aggregate.GetMerchant();
            Contact? contact = merchantModel.Contacts.ShouldHaveSingleItem();

            Result result = aggregate.UpdateContact(contact.ContactId, TestData.ContactName, TestData.ContactEmail, TestData.ContactPhone);
            result.IsSuccess.ShouldBeTrue();

            merchantModel = aggregate.GetMerchant();
            contact = merchantModel.Contacts.ShouldHaveSingleItem();
            contact.ContactName.ShouldBe(TestData.ContactName);
            contact.ContactEmailAddress.ShouldBe(TestData.ContactEmail);
            contact.ContactPhoneNumber.ShouldBe(TestData.ContactPhone);
        }

        [Fact]
        public void MerchantAggregate_UpdateContact_MerchantNotCreated_NoErrorThrown()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);

            Result result = aggregate.UpdateContact(Guid.NewGuid(),
                TestData.ContactName,
                TestData.ContactEmail,
                TestData.ContactPhone);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantAggregate_RemoveOperator_OperatorIsRemoved()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            aggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);
            
            Result result = aggregate.RemoveOperator(TestData.OperatorId);
            result.IsSuccess.ShouldBeTrue();
            
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

            Result result = aggregate.RemoveOperator(TestData.OperatorId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
        
        [Fact]
        public void MerchantAggregate_RemoveOperator_OperatorNotAlreadyAssigned_ErrorThrown()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Result result = aggregate.RemoveOperator(TestData.OperatorId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
        
        [Fact]
        public void MerchantAggregate_RemoveContract_ContractIsRemoved()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);
            aggregate.AddContract(TestData.Aggregates.CreatedContractAggregate());

            Result result = aggregate.RemoveContract(TestData.ContractId);
            result.IsSuccess.ShouldBeTrue();

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

            Result result = aggregate.RemoveContract(TestData.ContractId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantAggregate_RemoveContract_MerchantDoesNotHaveContract_ErrorThrown()
        {
            MerchantAggregate aggregate = MerchantAggregate.Create(TestData.MerchantId);
            aggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated, TestData.AddressModel, TestData.ContactModel,
                TestData.SettlementScheduleModel);

            Result result = aggregate.RemoveContract(TestData.ContractId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
    }
}
