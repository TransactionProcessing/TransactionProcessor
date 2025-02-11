using TransactionProcessor.Models.Estate;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.Repository
{
    using EstateModel = Models.Estate.Estate;
    using EstateEntity = Database.Entities.Estate;
    using EstateSecurityUserEntity = Database.Entities.EstateSecurityUser;
    using EstateOperatorModel = Models.Estate.Operator;
    using EstateSecurityUserModel = Models.Estate.SecurityUser;
    using OperatorEntity = Database.Entities.Operator;
    using MerchantModel = Models.Merchant.Merchant;
    using MerchantAddressModel = Models.Merchant.Address;
    using MerchantContactModel = Models.Merchant.Contact;
    using MerchantOperatorModel = Models.Merchant.Operator;
    using MerchantSecurityUserModel = Models.Merchant.SecurityUser;
    using MerchantEntity = TransactionProcessor.Database.Entities.Merchant;
    using MerchantAddressEntity = TransactionProcessor.Database.Entities.MerchantAddress;
    using MerchantContactEntity = TransactionProcessor.Database.Entities.MerchantContact;
    using MerchantOperatorEntity = TransactionProcessor.Database.Entities.MerchantOperator;
    using MerchantDeviceEntity = TransactionProcessor.Database.Entities.MerchantDevice;
    using MerchantSecurityUserEntity = TransactionProcessor.Database.Entities.MerchantSecurityUser;

    public static class ModelFactory
    {
        //public static MerchantModel ConvertFrom(Guid estateId, MerchantEntity merchant)
        //{
        //    MerchantModel merchantModel = new MerchantModel();
        //    merchantModel.EstateId = estateId;
        //    merchantModel.MerchantReportingId = merchant.MerchantReportingId;
        //    merchantModel.MerchantId = merchant.MerchantId;
        //    merchantModel.MerchantName = merchant.Name;
        //    merchantModel.Reference = merchant.Reference;
        //    merchantModel.SettlementSchedule = (SettlementSchedule)merchant.SettlementSchedule;

        //    return merchantModel;
        //}

        //public static MerchantModel ConvertFrom(Guid estateId,
        //                                        MerchantEntity merchant,
        //                                        List<MerchantAddressEntity> merchantAddresses,
        //                                        List<MerchantContactEntity> merchantContacts,
        //                                        List<MerchantOperatorEntity> merchantOperators,
        //                                        List<MerchantDeviceEntity> merchantDevices,
        //                                        List<MerchantSecurityUserEntity> merchantSecurityUsers)
        //{
        //    MerchantModel merchantModel = this.ConvertFrom(estateId, merchant);

        //    if (merchantAddresses != null && merchantAddresses.Any())
        //    {
        //        merchantModel.Addresses = new List<MerchantAddressModel>();
        //        merchantAddresses.ForEach(ma => merchantModel.Addresses.Add(new MerchantAddressModel
        //        {
        //            AddressId = ma.AddressId,
        //            AddressLine1 = ma.AddressLine1,
        //            AddressLine2 = ma.AddressLine2,
        //            AddressLine3 = ma.AddressLine3,
        //            AddressLine4 = ma.AddressLine4,
        //            Country = ma.Country,
        //            PostalCode = ma.PostalCode,
        //            Region = ma.Region,
        //            Town = ma.Town
        //        }));
        //    }

        //    if (merchantContacts != null && merchantContacts.Any())
        //    {
        //        merchantModel.Contacts = new List<MerchantContactModel>();
        //        merchantContacts.ForEach(mc => merchantModel.Contacts.Add(new MerchantContactModel
        //        {
        //            ContactEmailAddress = mc.EmailAddress,
        //            ContactId = mc.ContactId,
        //            ContactName = mc.Name,
        //            ContactPhoneNumber = mc.PhoneNumber
        //        }));
        //    }

        //    if (merchantOperators != null && merchantOperators.Any())
        //    {
        //        merchantModel.Operators = new List<MerchantOperatorModel>();
        //        merchantOperators.ForEach(mo => merchantModel.Operators.Add(new MerchantOperatorModel
        //        {
        //            Name = mo.Name,
        //            MerchantNumber = mo.MerchantNumber,
        //            OperatorId = mo.OperatorId,
        //            TerminalNumber = mo.TerminalNumber
        //        }));
        //    }

        //    if (merchantDevices != null && merchantDevices.Any())
        //    {
        //        merchantModel.Devices = new List<Device>();
        //        merchantDevices.ForEach(md => merchantModel.Devices.Add(new Device
        //        {
        //            DeviceIdentifier = md.DeviceIdentifier,
        //            DeviceId = md.DeviceId,
        //        }));
        //    }

        //    if (merchantSecurityUsers != null && merchantSecurityUsers.Any())
        //    {
        //        merchantModel.SecurityUsers = new List<SecurityUserModel>();
        //        merchantSecurityUsers.ForEach(msu => merchantModel.SecurityUsers.Add(new SecurityUserModel
        //        {
        //            EmailAddress = msu.EmailAddress,
        //            SecurityUserId = msu.SecurityUserId
        //        }));
        //    }

        //    return merchantModel;
        //}

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
