using System;
using System.Collections.Generic;

namespace TransactionProcessor.Models.Merchant
{
    public class Merchant
    {
        #region Properties

        public List<Address> Addresses { get; set; }

        public List<Contact> Contacts { get; set; }

        public List<Contract> Contracts { get; set; }

        public List<Device> Devices { get; set; }

        public Guid EstateId { get; set; }

        public Int32 EstateReportingId { get; set; }

        public Int32 MerchantReportingId { get; set; }

        public Guid MerchantId { get; set; }

        public String MerchantName { get; set; }

        public String Reference { get; set; }

        public List<Operator> Operators { get; set; }

        public List<SecurityUser> SecurityUsers { get; set; }

        public SettlementSchedule SettlementSchedule { get; set; }

        public DateTime NextSettlementDueDate { get; set; }

        public DateTime NextStatementDate { get; set; }

        #endregion
    }
}