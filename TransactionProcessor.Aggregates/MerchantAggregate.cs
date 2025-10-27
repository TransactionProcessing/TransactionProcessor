using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using SimpleResults;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;
using Address = TransactionProcessor.Aggregates.Models.Address;
using AddressModel = TransactionProcessor.Models.Merchant.Address;
using Contact = TransactionProcessor.Aggregates.Models.Contact;
using ContactModel = TransactionProcessor.Models.Merchant.Contact;
using Contract = TransactionProcessor.Aggregates.Models.Contract;
using ContractModel = TransactionProcessor.Models.Merchant.Contract;
using Device = TransactionProcessor.Aggregates.Models.Device;
using DeviceModel = TransactionProcessor.Models.Merchant.Device;
using Operator = TransactionProcessor.Aggregates.Models.Operator;
using OperatorModel = TransactionProcessor.Models.Merchant.Operator;
using SecurityUser = TransactionProcessor.Aggregates.Models.SecurityUser;
using SecurityUserModel = TransactionProcessor.Models.Merchant.SecurityUser;


namespace TransactionProcessor.Aggregates
{
    public static class MerchantAggregateExtensions{

        public static Result UpdateContact(this MerchantAggregate aggregate,  Guid contactId, String contactName, String contactEmailAddress, String contactPhoneNumber){
            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;

            Boolean isExistingContact = aggregate.Contacts.ContainsKey(contactId);

            if (isExistingContact == false) {
                // Not an existing contact, what should we do here ??
                return Result.Success();
            }

            Contact existingContact = aggregate.Contacts.Single(a => a.Key == contactId).Value;

            Contact updatedContact = new Contact(contactEmailAddress, contactName, contactPhoneNumber);

            if (updatedContact == existingContact) {
                // No changes
                return Result.Success();
            }

            aggregate.HandleContactUpdates(contactId, existingContact, updatedContact);

            return Result.Success();
        }
    
        public static Result UpdateMerchant(this MerchantAggregate aggregate, String name){
            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;

            if (String.Compare(name, aggregate.Name, StringComparison.InvariantCultureIgnoreCase) != 0 &&
                String.IsNullOrEmpty(name) == false){
                // Name has been updated to raise an event for this
                MerchantDomainEvents.MerchantNameUpdatedEvent merchantNameUpdatedEvent = new(aggregate.AggregateId,
                                                                                                 aggregate.EstateId,
                                                                                                 name);
                aggregate.ApplyAndAppend(merchantNameUpdatedEvent);
            }

            return Result.Success();
        }

        public static Result UpdateAddress(this MerchantAggregate aggregate, AddressModel address)
        {
            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;
            Boolean isExistingAddress = aggregate.Addresses.ContainsKey(address.AddressId);

            if (isExistingAddress == false){
                // Not an existing address, what should we do here ??
                return Result.Success();
            }
            Address existingAddress = aggregate.Addresses.Single(a => a.Key == address.AddressId).Value;

            Address updatedAddress = new Address(address.AddressLine1, address.AddressLine2, address.AddressLine3, address.AddressLine4,
                address.Town, address.Region, address.PostalCode, address.Country);
            
            if (updatedAddress == existingAddress){
                // No changes
                return Result.Success();
            }

            aggregate.HandleAddressUpdates(address.AddressId, existingAddress, updatedAddress);

            return Result.Success();
        }


        private static void HandleContactUpdates(this MerchantAggregate merchantAggregate, Guid contactId, Contact existingContact, Contact updatedContact){
            if (existingContact.ContactName != updatedContact.ContactName){
                MerchantDomainEvents.MerchantContactNameUpdatedEvent merchantContactNameUpdatedEvent = new(merchantAggregate.AggregateId,
                                                                                      merchantAggregate.EstateId,
                                                                                      contactId,
                                                                                      updatedContact.ContactName);
                merchantAggregate.ApplyAndAppend(merchantContactNameUpdatedEvent);
            }

            if (existingContact.ContactEmailAddress != updatedContact.ContactEmailAddress){
                MerchantDomainEvents.MerchantContactEmailAddressUpdatedEvent merchantContactEmailAddressUpdatedEvent = new(merchantAggregate.AggregateId,
                                                                                      merchantAggregate.EstateId,
                                                                                      contactId,
                                                                                      updatedContact.ContactEmailAddress);
                merchantAggregate.ApplyAndAppend(merchantContactEmailAddressUpdatedEvent);
            }

            if (existingContact.ContactPhoneNumber != updatedContact.ContactPhoneNumber)
            {
                MerchantDomainEvents.MerchantContactPhoneNumberUpdatedEvent merchantContactPhoneNumberUpdatedEvent = new(merchantAggregate.AggregateId,
                                                                                      merchantAggregate.EstateId,
                                                                                      contactId,
                                                                                      updatedContact.ContactPhoneNumber);
                merchantAggregate.ApplyAndAppend(merchantContactPhoneNumberUpdatedEvent);
            }
        }

        private static void HandleAddressUpdates(this MerchantAggregate merchantAggregate, Guid addressId, Address existingAddress, Address updatedAddress){
            if (existingAddress.AddressLine1 != updatedAddress.AddressLine1){
                MerchantDomainEvents.MerchantAddressLine1UpdatedEvent merchantAddressLine1UpdatedEvent = new (merchantAggregate.AggregateId, merchantAggregate.EstateId, addressId,
                                                                                                                         updatedAddress.AddressLine1);
                merchantAggregate.ApplyAndAppend(merchantAddressLine1UpdatedEvent);
            }

            if (existingAddress.AddressLine2 != updatedAddress.AddressLine2)
            {
                MerchantDomainEvents.MerchantAddressLine2UpdatedEvent merchantAddressLine2UpdatedEvent = new (merchantAggregate.AggregateId, merchantAggregate.EstateId, addressId,
                                                                                                                         updatedAddress.AddressLine2);
                merchantAggregate.ApplyAndAppend(merchantAddressLine2UpdatedEvent);
            }

            if (existingAddress.AddressLine3 != updatedAddress.AddressLine3)
            {
                MerchantDomainEvents.MerchantAddressLine3UpdatedEvent merchantAddressLine3UpdatedEvent = new (merchantAggregate.AggregateId, merchantAggregate.EstateId, addressId,
                                                                                                                         updatedAddress.AddressLine3);
                merchantAggregate.ApplyAndAppend(merchantAddressLine3UpdatedEvent);
            }

            if (existingAddress.AddressLine4 != updatedAddress.AddressLine4)
            {
                MerchantDomainEvents.MerchantAddressLine4UpdatedEvent merchantAddressLine4UpdatedEvent = new (merchantAggregate.AggregateId, merchantAggregate.EstateId, addressId,
                                                                                                                         updatedAddress.AddressLine4);
                merchantAggregate.ApplyAndAppend(merchantAddressLine4UpdatedEvent);
            }

            if (existingAddress.Country != updatedAddress.Country)
            {
                MerchantDomainEvents.MerchantCountyUpdatedEvent merchantCountyUpdatedEvent = new (merchantAggregate.AggregateId, merchantAggregate.EstateId, addressId,
                                                                                                             updatedAddress.Country);
                merchantAggregate.ApplyAndAppend(merchantCountyUpdatedEvent);
            }

            if (existingAddress.PostalCode != updatedAddress.PostalCode)
            {
                MerchantDomainEvents.MerchantPostalCodeUpdatedEvent merchantPostalCodeUpdatedEvent = new (merchantAggregate.AggregateId, merchantAggregate.EstateId, addressId,
                                                                                                       updatedAddress.PostalCode);
                merchantAggregate.ApplyAndAppend(merchantPostalCodeUpdatedEvent);
            }

            if (existingAddress.Region != updatedAddress.Region)
            {
                MerchantDomainEvents.MerchantRegionUpdatedEvent merchantRegionUpdatedEvent = new(merchantAggregate.AggregateId, merchantAggregate.EstateId, addressId,
                                                                                    updatedAddress.Region);
                merchantAggregate.ApplyAndAppend(merchantRegionUpdatedEvent);
            }

            if (existingAddress.Town != updatedAddress.Town)
            {
                MerchantDomainEvents.MerchantTownUpdatedEvent merchantTownUpdatedEvent = new(merchantAggregate.AggregateId, merchantAggregate.EstateId, addressId,
                                                                            updatedAddress.Town);
                merchantAggregate.ApplyAndAppend(merchantTownUpdatedEvent);
            }
        }

        public static Result AddAddress(this MerchantAggregate aggregate, AddressModel address)
        {
            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;

            if (IsDuplicateAddress(aggregate, address))
                return Result.Success(); // No need to throw an error, just ignore as we already have this address

            MerchantDomainEvents.AddressAddedEvent addressAddedEvent = new(aggregate.AggregateId, aggregate.EstateId, Guid.NewGuid(), 
                address.AddressLine1, address.AddressLine2, address.AddressLine3, address.AddressLine4, address.Town, 
                address.Region, address.PostalCode, address.Country);

            aggregate.ApplyAndAppend(addressAddedEvent);

            return Result.Success();
        }
        
        public static Result RemoveContract(this MerchantAggregate aggregate, Guid contractId){
            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;
            result = aggregate.EnsureContractHasBeenAdded(contractId);
            if (result.IsFailed)
                return result;

            MerchantDomainEvents.ContractRemovedFromMerchantEvent contractRemovedFromMerchantEvent = new MerchantDomainEvents.ContractRemovedFromMerchantEvent(aggregate.AggregateId, aggregate.EstateId, contractId);

            aggregate.ApplyAndAppend(contractRemovedFromMerchantEvent);

            return Result.Success();
        }

        public static Result AddContract(this MerchantAggregate aggregate, ContractAggregate contractAggregate){
            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;
            result = aggregate.EnsureContractHasNotAlreadyBeenAdded(contractAggregate.AggregateId);
            if (result.IsFailed)
                return result;

            MerchantDomainEvents.ContractAddedToMerchantEvent contractAddedToMerchantEvent = new(aggregate.AggregateId, aggregate.EstateId, contractAggregate.AggregateId);

            aggregate.ApplyAndAppend(contractAddedToMerchantEvent);

            foreach (Product product in contractAggregate.GetProducts()){
                MerchantDomainEvents.ContractProductAddedToMerchantEvent contractProductAddedToMerchantEvent = new(aggregate.AggregateId,
                                                                                                                                  aggregate.EstateId,
                                                                                                                                  contractAggregate.AggregateId,
                                                                                                                                  product.ContractProductId);
                aggregate.ApplyAndAppend(contractProductAddedToMerchantEvent);
            }

            return Result.Success();
        }

        public static Result AddContact(this MerchantAggregate aggregate, 
                                      String contactName,
                                      String contactPhoneNumber,
                                      String contactEmailAddress)
        {
            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;

            if (IsDuplicateContact(aggregate, contactName, contactEmailAddress, contactPhoneNumber))
                return Result.Success();

            MerchantDomainEvents.ContactAddedEvent contactAddedEvent = new(aggregate.AggregateId, aggregate.EstateId, Guid.NewGuid(), contactName, contactPhoneNumber, contactEmailAddress);

            aggregate.ApplyAndAppend(contactAddedEvent);

            return Result.Success();
        }

        public static Result AddDevice(this MerchantAggregate aggregate, 
                                     Guid deviceId,
                                     String deviceIdentifier)
        {
            if (String.IsNullOrEmpty(deviceIdentifier))
                return Result.Invalid("Device Identifier cannot be null or empty");

            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;
            result = aggregate.EnsureMerchantHasSpaceForDevice();
            if (result.IsFailed)
                return result;

            MerchantDomainEvents.DeviceAddedToMerchantEvent deviceAddedToMerchantEvent = new MerchantDomainEvents.DeviceAddedToMerchantEvent(aggregate.AggregateId, aggregate.EstateId, deviceId, deviceIdentifier);

            aggregate.ApplyAndAppend(deviceAddedToMerchantEvent);

            return Result.Success();
        }

        public static Result SwapDevice(this MerchantAggregate aggregate, 
                                      String originalDeviceIdentifier,
                                      String newDeviceIdentifier)
        {
            if (String.IsNullOrEmpty(originalDeviceIdentifier))
                return Result.Invalid("Original Device Identifier cannot be null or empty");

            if (String.IsNullOrEmpty(newDeviceIdentifier))
                return Result.Invalid("New Device Identifier cannot be null or empty");

            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;
            result = aggregate.EnsureDeviceBelongsToMerchant(originalDeviceIdentifier);
            if (result.IsFailed)
                return result;
            result = aggregate.EnsureDeviceDoesNotAlreadyBelongToMerchant(newDeviceIdentifier);
            if (result.IsFailed)
                return result;

            Guid deviceId = Guid.NewGuid();
            
            MerchantDomainEvents.DeviceSwappedForMerchantEvent deviceSwappedForMerchantEvent = new MerchantDomainEvents.DeviceSwappedForMerchantEvent(aggregate.AggregateId, aggregate.EstateId,
                                                                                                            deviceId, originalDeviceIdentifier, newDeviceIdentifier);

            aggregate.ApplyAndAppend(deviceSwappedForMerchantEvent);

            return Result.Success();
        }

        public static Result AddSecurityUser(this MerchantAggregate aggregate, 
                                           Guid securityUserId,
                                           String emailAddress)
        {
            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;

            MerchantDomainEvents.SecurityUserAddedToMerchantEvent securityUserAddedEvent = new MerchantDomainEvents.SecurityUserAddedToMerchantEvent(aggregate.AggregateId, aggregate.EstateId, securityUserId, emailAddress);

            aggregate.ApplyAndAppend(securityUserAddedEvent);

            return Result.Success();
        }
        
        public static Result AssignOperator(this MerchantAggregate aggregate, 
                                          Guid operatorId,
                                          String operatorName,
                                          String merchantNumber,
                                          String terminalNumber)
        {
            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;
            result = aggregate.EnsureOperatorHasNotAlreadyBeenAssigned(operatorId);
            if (result.IsFailed)
                return result;

            MerchantDomainEvents.OperatorAssignedToMerchantEvent operatorAssignedToMerchantEvent = new(aggregate.AggregateId, aggregate.EstateId, operatorId, operatorName, merchantNumber, terminalNumber);

            aggregate.ApplyAndAppend(operatorAssignedToMerchantEvent);

            return Result.Success();
        }

        public static Result RemoveOperator(this MerchantAggregate aggregate,
                                          Guid operatorId)
        {
            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;
            result = aggregate.EnsureOperatorHasBeenAssigned(operatorId);
            if (result.IsFailed)
                return result;

            MerchantDomainEvents.OperatorRemovedFromMerchantEvent operatorRemovedFromMerchantEvent = new(aggregate.AggregateId, aggregate.EstateId, operatorId);

            aggregate.ApplyAndAppend(operatorRemovedFromMerchantEvent);
            return Result.Success();
        }

        public static Merchant GetMerchant(this MerchantAggregate aggregate)
        {
            if (aggregate.IsCreated == false)
            {
                return null;
            }

            Merchant merchantModel = new();

            merchantModel.EstateId = aggregate.EstateId;
            merchantModel.MerchantId = aggregate.AggregateId;
            merchantModel.MerchantName = aggregate.Name;
            merchantModel.Reference = aggregate.MerchantReference;
            merchantModel.SettlementSchedule = aggregate.SettlementSchedule;
            merchantModel.NextSettlementDueDate = aggregate.NextSettlementDueDate;

            if (aggregate.Addresses.Any())
            {
                merchantModel.Addresses = new();
                foreach (KeyValuePair<Guid, Address> aggregateAddress in aggregate.Addresses) {
                    AddressModel address = new AddressModel(aggregateAddress.Key, aggregateAddress.Value.AddressLine1, aggregateAddress.Value.AddressLine2, aggregateAddress.Value.AddressLine3, aggregateAddress.Value.AddressLine4, aggregateAddress.Value.Town, aggregateAddress.Value.Region, aggregateAddress.Value.PostalCode, aggregateAddress.Value.Country);
                    
                    merchantModel.Addresses.Add(address);
                }
            }

            if (aggregate.Contacts.Any())
            {
                merchantModel.Contacts = new();
                foreach (KeyValuePair<Guid, Contact> aggregateContact in aggregate.Contacts) {
                    var contact = new ContactModel(aggregateContact.Key, aggregateContact.Value.ContactEmailAddress, aggregateContact.Value.ContactName, aggregateContact.Value.ContactPhoneNumber);
                    merchantModel.Contacts.Add(contact);
                }
            }

            if (aggregate.Operators.Any())
            {
                merchantModel.Operators = new();
                foreach (KeyValuePair<Guid, Operator> aggregateOperator in aggregate.Operators) {
                    merchantModel.Operators.Add(new OperatorModel(aggregateOperator.Key, aggregateOperator.Value.Name, aggregateOperator.Value.MerchantNumber, aggregateOperator.Value.TerminalNumber, aggregateOperator.Value.IsDeleted));
                }
            }

            if (aggregate.SecurityUsers.Any())
            {
                merchantModel.SecurityUsers = new();
                aggregate.SecurityUsers.ForEach(s => merchantModel.SecurityUsers.Add(new SecurityUserModel(s.SecurityUserId,s.EmailAddress)));
            }

            if (aggregate.Devices.Any()){
                merchantModel.Devices = new ();
                foreach ((Guid key, Device device) in aggregate.Devices)
                {
                    merchantModel.Devices.Add(new DeviceModel(key,device.DeviceIdentifier,device.IsEnabled));
                }
            }

            if (aggregate.Contracts.Any()){
                merchantModel.Contracts = new();
                foreach (KeyValuePair<Guid, Contract> aggregateContract in aggregate.Contracts){
                    var contract = new ContractModel(aggregateContract.Key, aggregateContract.Value.IsDeleted);
                    aggregateContract.Value.ContractProducts.ForEach(cp => contract.ContractProducts.Add(cp));
                    merchantModel.Contracts.Add(contract);    
                }
            }

            return merchantModel;
        }

        static DateTime CalculateNextSettlementDate(this MerchantAggregate aggregate, SettlementSchedule settlementSchedule) {
            DateTime nextSettlementDate = DateTime.MinValue;
            if (settlementSchedule != SettlementSchedule.Immediate)
            {
                // Calculate next settlement date
                DateTime dateForCalculation = aggregate.NextSettlementDueDate;
                if (dateForCalculation == DateTime.MinValue)
                {
                    // No date set previously so use current date as start point
                    dateForCalculation = DateTime.Now.Date;
                }

                if (settlementSchedule == SettlementSchedule.Weekly)
                {
                    nextSettlementDate = dateForCalculation.AddDays(7);
                }
                else if (settlementSchedule == SettlementSchedule.Monthly)
                {
                    nextSettlementDate = dateForCalculation.AddMonths(1);
                }
            }
            return nextSettlementDate;
        }
        
        public static Result SetSettlementSchedule(this MerchantAggregate aggregate, SettlementSchedule settlementSchedule)
        {
            Result result = aggregate.EnsureMerchantHasBeenCreated();
            if (result.IsFailed)
                return result;

            if (settlementSchedule == SettlementSchedule.NotSet)
                return Result.Invalid("A valid settlement schedule must be provided");

            // Check if there has actually been a change or not, if not ignore the request
            if (aggregate.SettlementSchedule == settlementSchedule)
                return Result.Success();

            DateTime nextSettlementDate = CalculateNextSettlementDate(aggregate, settlementSchedule);
        
            MerchantDomainEvents.SettlementScheduleChangedEvent settlementScheduleChangedEvent = new(aggregate.AggregateId, aggregate.EstateId, (Int32)settlementSchedule, nextSettlementDate);

            aggregate.ApplyAndAppend(settlementScheduleChangedEvent);

            return Result.Success();
        }

        public static Result Create(this MerchantAggregate aggregate, 
                           Guid estateId,
                           String merchantName,
                           DateTime dateCreated, 
                           TransactionProcessor.Models.Merchant.Address address,
                           TransactionProcessor.Models.Merchant.Contact contact,
                           SettlementSchedule settlementSchedule)
        {
            // Ensure this merchant has not already been created
            if (aggregate.IsCreated)
                return Result.Success();

            if (String.IsNullOrEmpty(merchantName))
                return Result.Invalid("Merchant name must be provided when registering a new merchant");

            if (address == null)
                return Result.Invalid("An Address must be provided when registering a new merchant");

            if (contact == null)
                return Result.Invalid("A Contact must be provided when registering a new merchant");

            if (settlementSchedule == SettlementSchedule.NotSet)
                return Result.Invalid("A valid settlement schedule must be provided when registering a new merchant");
            
            MerchantDomainEvents.MerchantCreatedEvent merchantCreatedEvent = new(aggregate.AggregateId, estateId, merchantName, dateCreated);

            aggregate.ApplyAndAppend(merchantCreatedEvent);

            String reference = $"{aggregate.AggregateId.GetHashCode():X}";

            MerchantDomainEvents.MerchantReferenceAllocatedEvent merchantReferenceAllocatedEvent = new(aggregate.AggregateId, aggregate.EstateId, reference);

            aggregate.ApplyAndAppend(merchantReferenceAllocatedEvent);

            MerchantDomainEvents.AddressAddedEvent addressAddedEvent = new(aggregate.AggregateId,
                aggregate.EstateId,
                Guid.NewGuid(),
                address.AddressLine1,
                address.AddressLine2,
                address.AddressLine3,
                address.AddressLine4,
                address.Town,
                address.Region,
                address.PostalCode,
                address.Country);

            aggregate.ApplyAndAppend(addressAddedEvent);

            MerchantDomainEvents.ContactAddedEvent contactAddedEvent = new(aggregate.AggregateId, aggregate.EstateId, Guid.NewGuid(), contact.ContactName, contact.ContactPhoneNumber, contact.ContactEmailAddress);

            aggregate.ApplyAndAppend(contactAddedEvent);

            DateTime nextSettlementDate = CalculateNextSettlementDate(aggregate, settlementSchedule);

            MerchantDomainEvents.SettlementScheduleChangedEvent settlementScheduleChangedEvent = new(aggregate.AggregateId, aggregate.EstateId, (Int32)settlementSchedule, nextSettlementDate);

            aggregate.ApplyAndAppend(settlementScheduleChangedEvent);

            return Result.Success();
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantReferenceAllocatedEvent domainEvent)
        {
            aggregate.MerchantReference = domainEvent.MerchantReference;
        }

        private static Result EnsureMerchantHasBeenCreated(this MerchantAggregate aggregate) {
            if (aggregate.IsCreated == false) {
                return Result.Invalid("Merchant has not been created");
            }
            return Result.Success();
        }

        private static Boolean IsDuplicateAddress(this MerchantAggregate aggregate, AddressModel address)
        {
            // create record of "new" address
            Address newAddress = new Address(address.AddressLine1, address.AddressLine2, address.AddressLine3, address.AddressLine4, address.Town, address.Region, address.PostalCode, address.Country);

            foreach (KeyValuePair<Guid, Address> aggregateAddress in aggregate.Addresses){
                if (newAddress == aggregateAddress.Value){
                    return true;
                }
            }

            return false;
        }

        private static Boolean IsDuplicateContact(this MerchantAggregate aggregate, String contactName,
                                                  String contactEmailAddress,
                                                  String contactPhoneNumber)
        {
            // create record of "new" contact
            Contact newContact = new Contact(contactEmailAddress, contactName, contactPhoneNumber);

            foreach (KeyValuePair<Guid, Contact> aggregateContacts in aggregate.Contacts)
            {
                if (newContact == aggregateContacts.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private static Result EnsureDeviceBelongsToMerchant(this MerchantAggregate aggregate,
                                                            String originalDeviceIdentifier) {
            if (aggregate.Devices.Any(d => d.Value.DeviceIdentifier == originalDeviceIdentifier) == false) {
                return Result.Invalid("Merchant does not have this device allocated");
            }

            return Result.Success();
        }

        private static Result EnsureDeviceDoesNotAlreadyBelongToMerchant(this MerchantAggregate aggregate,
                                                                         String newDeviceIdentifier) {
            if (aggregate.Devices.Any(d => d.Value.DeviceIdentifier == newDeviceIdentifier)) {
                return Result.Invalid("Merchant already has this device allocated");
            }

            return Result.Success();
        }

        private static Result EnsureMerchantHasSpaceForDevice(this MerchantAggregate aggregate) {
            if (aggregate.Devices.Count + 1 > aggregate.MaximumDevices) {
                return Result.Invalid($"Merchant {aggregate.Name} already has the maximum devices allocated");
            }

            return Result.Success();
        }

        private static Result EnsureOperatorHasNotAlreadyBeenAssigned(this MerchantAggregate aggregate,
                                                                      Guid operatorId) {
            if (aggregate.Operators.Any(o => o.Key == operatorId)) {
                return Result.Invalid($"Operator {operatorId} has already been assigned to merchant");
            }

            return Result.Success();
        }

        private static Result EnsureOperatorHasBeenAssigned(this MerchantAggregate aggregate,
                                                            Guid operatorId) {
            if (aggregate.Operators.Any(o => o.Key == operatorId) == false) {
                return Result.Invalid($"Operator {operatorId} has not been assigned to merchant");
            }

            return Result.Success();
        }

        private static Result EnsureContractHasNotAlreadyBeenAdded(this MerchantAggregate aggregate,
                                                                   Guid contractId) {
            if (aggregate.Contracts.ContainsKey(contractId)) {
                return Result.Invalid($"Contract {contractId} has already been assigned to merchant");
            }

            return Result.Success();
        }

        private static Result EnsureContractHasBeenAdded(this MerchantAggregate aggregate,
                                                         Guid contractId) {
            if (aggregate.Contracts.ContainsKey(contractId) == false) {
                return Result.Invalid($"Contract {contractId} has not been assigned to merchant");
            }

            return Result.Success();
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantCreatedEvent merchantCreatedEvent)
        {
            aggregate.IsCreated = true;
            aggregate.EstateId = merchantCreatedEvent.EstateId;
            aggregate.Name = merchantCreatedEvent.MerchantName;
            aggregate.AggregateId = merchantCreatedEvent.AggregateId;
            aggregate.DateCreated = merchantCreatedEvent.DateCreated;
            aggregate.MaximumDevices = 1;
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantNameUpdatedEvent merchantNameUpdatedEvent){
            aggregate.Name = merchantNameUpdatedEvent.MerchantName;
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.AddressAddedEvent addressAddedEvent)
        {

            Address address = new Address(addressAddedEvent.AddressLine1,
                addressAddedEvent.AddressLine2,
                addressAddedEvent.AddressLine3,
                addressAddedEvent.AddressLine4,
                addressAddedEvent.Town,
                addressAddedEvent.Region,
                addressAddedEvent.PostalCode,
                addressAddedEvent.Country);

            aggregate.Addresses.Add(addressAddedEvent.AddressId,address);
        }

        private static void UpdateAddress(this MerchantAggregate aggregate, Guid addressId, IDomainEvent domainEvent){
            KeyValuePair<Guid, Address> address = aggregate.Addresses.Single(a => a.Key == addressId);

            // Now update the record
            Address updatedAddress = domainEvent switch{
                MerchantDomainEvents.MerchantAddressLine1UpdatedEvent addressLine1UpdatedEvent => address.Value with{
                                                                                                   AddressLine1 = addressLine1UpdatedEvent.AddressLine1
                                                                                               },
                MerchantDomainEvents.MerchantAddressLine2UpdatedEvent addressLine2UpdatedEvent => address.Value with
                                                                             {
                                                                                 AddressLine2 = addressLine2UpdatedEvent.AddressLine2
                                                                             },
                MerchantDomainEvents.MerchantAddressLine3UpdatedEvent addressLine3UpdatedEvent => address.Value with
                                                                             {
                                                                                 AddressLine3 = addressLine3UpdatedEvent.AddressLine3
                                                                             },
                MerchantDomainEvents.MerchantAddressLine4UpdatedEvent addressLine4UpdatedEvent => address.Value with
                                                                             {
                                                                                 AddressLine4 = addressLine4UpdatedEvent.AddressLine4
                                                                             },
                MerchantDomainEvents.MerchantPostalCodeUpdatedEvent merchantPostalCodeUpdatedEvent => address.Value with
                                                                                 {
                                                                                     PostalCode = merchantPostalCodeUpdatedEvent.PostalCode
                                                                                 },
                MerchantDomainEvents.MerchantTownUpdatedEvent merchantTownUpdatedEvent => address.Value with
                                                                     {
                                                                         Town = merchantTownUpdatedEvent.Town
                                                                     },
                MerchantDomainEvents.MerchantRegionUpdatedEvent merchantRegionUpdatedEvent => address.Value with
                                                                         {
                                                                             Region = merchantRegionUpdatedEvent.Region
                                                                         },
                MerchantDomainEvents.MerchantCountyUpdatedEvent merchantCountyUpdatedEvent => address.Value with
                                                                         {
                                                                             Country = merchantCountyUpdatedEvent.Country
                                                                         },
                                _ => address.Value,
            };

            aggregate.Addresses[addressId] = updatedAddress;
        }

        private static void UpdateContact(this MerchantAggregate aggregate, Guid contactId, IDomainEvent domainEvent)
        {
            KeyValuePair<Guid, Contact> contact = aggregate.Contacts.Single(a => a.Key == contactId);

            // Now update the record
            Contact updatedContact = domainEvent switch
            {
                MerchantDomainEvents.MerchantContactNameUpdatedEvent merchantContactNameUpdatedEvent => contact.Value with
                                                                                   {
                                                                                       ContactName = merchantContactNameUpdatedEvent.ContactName
                                                                                   },
                MerchantDomainEvents.MerchantContactEmailAddressUpdatedEvent merchantContactEmailAddressUpdatedEvent => contact.Value with
                                                                                                   {
                                                                                                       ContactEmailAddress = merchantContactEmailAddressUpdatedEvent.ContactEmailAddress
                                                                                                   },
                MerchantDomainEvents.MerchantContactPhoneNumberUpdatedEvent merchantContactPhoneNumberUpdatedEvent => contact.Value with
                                                                                                 {
                                                                                                     ContactPhoneNumber = merchantContactPhoneNumberUpdatedEvent.ContactPhoneNumber
                                                                                                 },
               
                _ => contact.Value,
            };

            aggregate.Contacts[contactId] = updatedContact;
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantAddressLine1UpdatedEvent addressLine1UpdatedEvent){
            aggregate.UpdateAddress(addressLine1UpdatedEvent.AddressId, addressLine1UpdatedEvent);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantAddressLine2UpdatedEvent addressLine2UpdatedEvent){
            aggregate.UpdateAddress(addressLine2UpdatedEvent.AddressId, addressLine2UpdatedEvent);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantAddressLine3UpdatedEvent addressLine3UpdatedEvent){
            aggregate.UpdateAddress(addressLine3UpdatedEvent.AddressId, addressLine3UpdatedEvent);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantAddressLine4UpdatedEvent addressLine4UpdatedEvent){
            aggregate.UpdateAddress(addressLine4UpdatedEvent.AddressId, addressLine4UpdatedEvent);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantPostalCodeUpdatedEvent merchantPostalCodeUpdatedEvent){
            aggregate.UpdateAddress(merchantPostalCodeUpdatedEvent.AddressId, merchantPostalCodeUpdatedEvent);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantContactNameUpdatedEvent merchantContactNameUpdatedEvent){
            aggregate.UpdateContact(merchantContactNameUpdatedEvent.ContactId, merchantContactNameUpdatedEvent);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantContactEmailAddressUpdatedEvent merchantContactEmailAddressUpdatedEvent){
            aggregate.UpdateContact(merchantContactEmailAddressUpdatedEvent.ContactId, merchantContactEmailAddressUpdatedEvent);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantContactPhoneNumberUpdatedEvent merchantContactPhoneNumberUpdatedEvent){
            aggregate.UpdateContact(merchantContactPhoneNumberUpdatedEvent.ContactId, merchantContactPhoneNumberUpdatedEvent);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantTownUpdatedEvent merchantTownUpdatedEvent){
            aggregate.UpdateAddress(merchantTownUpdatedEvent.AddressId, merchantTownUpdatedEvent);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantRegionUpdatedEvent merchantRegionUpdatedEvent){
            aggregate.UpdateAddress(merchantRegionUpdatedEvent.AddressId, merchantRegionUpdatedEvent);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.MerchantCountyUpdatedEvent merchantCountyUpdatedEvent){
            aggregate.UpdateAddress(merchantCountyUpdatedEvent.AddressId, merchantCountyUpdatedEvent);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.ContactAddedEvent contactAddedEvent)
        {
            Contact contact = new Contact(contactAddedEvent.ContactEmailAddress, contactAddedEvent.ContactName, contactAddedEvent.ContactPhoneNumber);

            aggregate.Contacts.Add(contactAddedEvent.ContactId, contact);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.OperatorAssignedToMerchantEvent operatorAssignedToMerchantEvent)
        {
            Operator @operator = new Operator(operatorAssignedToMerchantEvent.OperatorId, operatorAssignedToMerchantEvent.Name,
                                              operatorAssignedToMerchantEvent.MerchantNumber,
                                              operatorAssignedToMerchantEvent.TerminalNumber);

            aggregate.Operators.Add(operatorAssignedToMerchantEvent.OperatorId, @operator);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.OperatorRemovedFromMerchantEvent operatorRemovedFromMerchantEvent){
            KeyValuePair<Guid, Operator> @operator = aggregate.Operators.Single(o => o.Key == operatorRemovedFromMerchantEvent.OperatorId);

            aggregate.Operators[operatorRemovedFromMerchantEvent.OperatorId] = @operator.Value with{
                                                                                                       IsDeleted = true
                                                                                                   };
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.SecurityUserAddedToMerchantEvent domainEvent)
        {
            SecurityUser securityUser = new SecurityUser(domainEvent.SecurityUserId, domainEvent.EmailAddress);

            aggregate.SecurityUsers.Add(securityUser);
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.SettlementScheduleChangedEvent domainEvent)
        {
            aggregate.SettlementSchedule = (SettlementSchedule)domainEvent.SettlementSchedule;
            aggregate.NextSettlementDueDate = domainEvent.NextSettlementDate;
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.ContractAddedToMerchantEvent domainEvent){
            aggregate.Contracts.Add(domainEvent.ContractId, new Contract(domainEvent.ContractId));
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.ContractRemovedFromMerchantEvent domainEvent)
        {
            KeyValuePair<Guid, Contract> contract = aggregate.Contracts.Single(c => c.Key == domainEvent.ContractId);
            
            aggregate.Contracts[domainEvent.ContractId] = contract.Value with{
                                                                                 IsDeleted = true
                                                                             };
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.ContractProductAddedToMerchantEvent domainEvent){
            KeyValuePair<Guid, Contract> contract = aggregate.Contracts.Single(c => c.Key == domainEvent.ContractId);
            contract.Value.ContractProducts.Add(domainEvent.ContractProductId);
            aggregate.Contracts[domainEvent.ContractId] = contract.Value;
        }

        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.DeviceSwappedForMerchantEvent domainEvent)
        {
            KeyValuePair<Guid, Device> device = aggregate.Devices.Single(d => d.Value.DeviceIdentifier == domainEvent.OriginalDeviceIdentifier);
            aggregate.Devices[device.Key] = device.Value with{
                                                                 IsEnabled = false
                                                             };

            aggregate.Devices.Add(domainEvent.DeviceId, new Device(domainEvent.DeviceId, domainEvent.NewDeviceIdentifier));

        }
        public static void PlayEvent(this MerchantAggregate aggregate, MerchantDomainEvents.DeviceAddedToMerchantEvent domainEvent)
        {
            Device device = new Device(domainEvent.DeviceId, domainEvent.DeviceIdentifier);
            aggregate.Devices.Add(domainEvent.DeviceId, device);
        }
    }

    public record MerchantAggregate : Aggregate
    {
        #region Fields

        internal readonly Dictionary<Guid, Address> Addresses;

        internal readonly Dictionary<Guid, Contact> Contacts;

        internal readonly Dictionary<Guid, Device> Devices;

        internal readonly Dictionary<Guid, Operator> Operators;

        internal readonly List<SecurityUser> SecurityUsers;

        internal readonly Dictionary<Guid, Contract> Contracts;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MerchantAggregate" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public MerchantAggregate()
        {
            // Nothing here
            this.Addresses = new Dictionary<Guid, Address>();
            this.Contacts = new Dictionary<Guid,Contact>();
            this.Operators = new Dictionary<Guid, Operator>();
            this.SecurityUsers = new List<SecurityUser>();
            this.Devices = new Dictionary<Guid, Device>();
            this.Contracts = new Dictionary<Guid, Contract>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MerchantAggregate" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        private MerchantAggregate(Guid aggregateId)
        {
            if (aggregateId == Guid.Empty)
                throw new ArgumentNullException(nameof(aggregateId));

            this.AggregateId = aggregateId;
            this.Addresses = new Dictionary<Guid, Address>();
            this.Contacts = new Dictionary<Guid, Contact>();
            this.Operators = new Dictionary<Guid, Operator>();
            this.SecurityUsers = new List<SecurityUser>();
            this.Devices = new Dictionary<Guid, Device>();
            this.Contracts = new Dictionary<Guid, Contract>();
        }

        #endregion

        #region Properties

        public DateTime DateCreated { get; internal set; }

        public DateTime NextSettlementDueDate { get; internal set; }
        
        public Guid EstateId { get; internal set; }

        public Boolean IsCreated { get; internal set; }

        public Int32 MaximumDevices { get; internal set; }

        public String Name { get; internal set; }

        public String MerchantReference { get; internal set; }

        public SettlementSchedule SettlementSchedule { get; internal set; }

        #endregion

        #region Methods
        
        public static MerchantAggregate Create(Guid aggregateId)
        {
            return new MerchantAggregate(aggregateId);
        }
        
        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return new
                   {
                       this.EstateId,
                       MerchantId = this.AggregateId
                   };
        }
        
        public override void PlayEvent(IDomainEvent domainEvent) => MerchantAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);


        #endregion

        
    }
}