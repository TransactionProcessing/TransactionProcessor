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

            if (merchantAddresses != null && merchantAddresses.Any())
            {
                merchantModel.Addresses = new List<MerchantAddressModel>();
                merchantAddresses.ForEach(ma => merchantModel.Addresses.Add(new MerchantAddressModel(ma.AddressId,
                    ma.AddressLine1,
                    ma.AddressLine2,
                    ma.AddressLine3,
                    ma.AddressLine4,ma.Town,ma.Region, ma.PostalCode, ma.Country)));
            }

            if (merchantContacts != null && merchantContacts.Any())
            {
                merchantModel.Contacts = new List<MerchantContactModel>();
                merchantContacts.ForEach(mc => merchantModel.Contacts.Add(new MerchantContactModel(mc.ContactId, mc.EmailAddress, mc.Name, mc.PhoneNumber)));
            }

            if (merchantOperators != null && merchantOperators.Any())
            {
                merchantModel.Operators = new List<MerchantOperatorModel>();
                merchantOperators.ForEach(mo => merchantModel.Operators.Add(new MerchantOperatorModel(mo.OperatorId, mo.Name, mo.MerchantNumber, mo.TerminalNumber, mo.IsDeleted)));
            }

            if (merchantDevices != null && merchantDevices.Any())
            {
                merchantModel.Devices = new List<Device>();
                merchantDevices.ForEach(md => merchantModel.Devices.Add(new Device(md.DeviceId, md.DeviceIdentifier)));
            }

            if (merchantSecurityUsers != null && merchantSecurityUsers.Any())
            {
                merchantModel.SecurityUsers = new List<MerchantSecurityUserModel>();
                merchantSecurityUsers.ForEach(msu => merchantModel.SecurityUsers.Add(new MerchantSecurityUserModel(msu.SecurityUserId, msu.EmailAddress)));
            }

            return merchantModel;
        }
    }
}
