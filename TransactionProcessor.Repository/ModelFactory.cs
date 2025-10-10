using TransactionProcessor.Database.Entities;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.Repository
{
    using EstateEntity = Database.Entities.Estate;
    using EstateModel = Models.Estate.Estate;
    using EstateOperatorModel = Models.Estate.Operator;
    using EstateSecurityUserEntity = Database.Entities.EstateSecurityUser;
    using EstateSecurityUserModel = Models.Estate.SecurityUser;
    using MerchantAddressEntity = TransactionProcessor.Database.Entities.MerchantAddress;
    using MerchantAddressModel = Models.Merchant.Address;
    using MerchantContactEntity = TransactionProcessor.Database.Entities.MerchantContact;
    using MerchantContactModel = Models.Merchant.Contact;
    using MerchantDeviceEntity = TransactionProcessor.Database.Entities.MerchantDevice;
    using MerchantEntity = TransactionProcessor.Database.Entities.Merchant;
    using MerchantModel = Models.Merchant.Merchant;
    using MerchantOperatorEntity = TransactionProcessor.Database.Entities.MerchantOperator;
    using MerchantOperatorModel = Models.Merchant.Operator;
    using MerchantSecurityUserEntity = TransactionProcessor.Database.Entities.MerchantSecurityUser;
    using MerchantSecurityUserModel = Models.Merchant.SecurityUser;
    using OperatorEntity = Database.Entities.Operator;

    public static class ModelFactory
    {
        public static EstateModel ConvertFrom(EstateEntity estate,
                                       List<EstateSecurityUserEntity> estateSecurityUsers,
                                       List<OperatorEntity> operators)
        {
            EstateModel estateModel = new EstateModel();
            estateModel.EstateId = estate.EstateId;
            estateModel.EstateReportingId = estate.EstateReportingId;
            estateModel.Name = estate.Name;
            estateModel.Reference = estate.Reference;

            if (operators != null && operators.Any())
            {
                estateModel.Operators = new List<EstateOperatorModel>();
                foreach (OperatorEntity @operator in operators)
                {
                    estateModel.Operators.Add(new EstateOperatorModel
                    {
                        OperatorId = @operator.OperatorId,
                        Name = @operator.Name
                    });
                }
            }

            if (estateSecurityUsers != null && estateSecurityUsers.Any())
            {
                estateModel.SecurityUsers = new List<EstateSecurityUserModel>();
                estateSecurityUsers.ForEach(esu => estateModel.SecurityUsers.Add(new EstateSecurityUserModel
                {
                    SecurityUserId = esu.SecurityUserId,
                    EmailAddress = esu.EmailAddress
                }));
            }

            return estateModel;
        }

        public static MerchantModel ConvertFrom(Guid estateId, MerchantEntity merchant)
        {
            MerchantModel merchantModel = new MerchantModel();
            merchantModel.EstateId = estateId;
            merchantModel.MerchantReportingId = merchant.MerchantReportingId;
            merchantModel.MerchantId = merchant.MerchantId;
            merchantModel.MerchantName = merchant.Name;
            merchantModel.Reference = merchant.Reference;
            merchantModel.SettlementSchedule = (SettlementSchedule)merchant.SettlementSchedule;

            return merchantModel;
        }

        public static MerchantModel ConvertFrom(Guid estateId,
                                                MerchantEntity merchant,
                                                List<MerchantAddressEntity> merchantAddresses,
                                                List<MerchantContactEntity> merchantContacts,
                                                List<MerchantOperatorEntity> merchantOperators,
                                                List<MerchantDeviceEntity> merchantDevices,
                                                List<MerchantSecurityUserEntity> merchantSecurityUsers)
        {
            MerchantModel merchantModel = ModelFactory.ConvertFrom(estateId, merchant);

            merchantModel.Addresses = ConvertFrom(merchantAddresses);
            merchantModel.Contacts = ConvertFrom(merchantContacts);
            merchantModel.Operators = ConvertFrom(merchantOperators);
            merchantModel.Devices = ConvertFrom(merchantDevices);
            merchantModel.SecurityUsers = ConvertFrom(merchantSecurityUsers);

            return merchantModel;
        }

        private static List<MerchantSecurityUserModel> ConvertFrom(List<MerchantSecurityUserEntity> merchantSecurityUsers)
        {
            List<MerchantSecurityUserModel> users = new List<MerchantSecurityUserModel>();
            if (merchantSecurityUsers != null && merchantSecurityUsers.Any())
            {
                merchantSecurityUsers.ForEach(msu => users.Add(new MerchantSecurityUserModel(msu.SecurityUserId, msu.EmailAddress)));
            }

            return users;
        }

        private static List<MerchantAddressModel> ConvertFrom(List<MerchantAddressEntity> merchantAddresses) {
            List<MerchantAddressModel> addresses = new List<MerchantAddressModel>();
            if (merchantAddresses != null && merchantAddresses.Any()) {
                merchantAddresses.ForEach(ma => addresses.Add(new MerchantAddressModel(ma.AddressId, ma.AddressLine1, ma.AddressLine2, ma.AddressLine3, ma.AddressLine4, ma.Town, ma.Region, ma.PostalCode, ma.Country)));
            }

            return addresses;
        }

        private static List<Device> ConvertFrom(List<MerchantDeviceEntity> merchantDevices)
        {
            List<Device> devices = new List<Device> ();
            if (merchantDevices != null && merchantDevices.Any())
            {
                merchantDevices.ForEach(md => devices.Add(new Device(md.DeviceId, md.DeviceIdentifier)));
            }

            return devices;
        }

        private static List<MerchantOperatorModel> ConvertFrom(List<MerchantOperatorEntity> merchantOperators)
        {
            List<MerchantOperatorModel> operators = new List<MerchantOperatorModel>();
            if (merchantOperators != null && merchantOperators.Any())
            {
                merchantOperators.ForEach(mo => operators.Add(new MerchantOperatorModel(mo.OperatorId, mo.Name, mo.MerchantNumber, mo.TerminalNumber, mo.IsDeleted)));
            }

            return operators;
        }

        private static List<MerchantContactModel> ConvertFrom(List<MerchantContactEntity> merchantContacts)
        {
            List<MerchantContactModel> contacts = new List<MerchantContactModel>();
            if (merchantContacts != null && merchantContacts.Any())
            {
                merchantContacts.ForEach(mc => contacts.Add(new MerchantContactModel(mc.ContactId, mc.EmailAddress, mc.Name, mc.PhoneNumber)));
            }

            return contacts;
        }
    }
}
