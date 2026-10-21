using System;
using System.Collections.Generic;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Responses.Contract;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>
    /// Merchant details including contacts, devices, contracts, schedule, and opening hours.
    /// </summary>
    /// <example>{"addresses":[{"addressId":"00000000-0000-0000-0000-000000000119","addressLine1":"123 Example Street","addressLine2":"123 Example Street","addressLine3":"123 Example Street","addressLine4":"123 Example Street","country":"US","postalCode":"10001","region":"Example City","town":"Example City"}],"contacts":[{"contactEmailAddress":"customer@example.com","contactId":"00000000-0000-0000-0000-000000000125","contactName":"Example Name","contactPhoneNumber":"+15550100"}],"devices":{"00000000-0000-0000-0000-000000000001":"EXAMPLE-001"},"estateId":"00000000-0000-0000-0000-000000000023","estateReportingId":1,"merchantId":"00000000-0000-0000-0000-000000000227","merchantReportingId":1,"merchantName":"Example Name","merchantReference":"REF-2026-001","nextStatementDate":"2026-01-15T10:30:00Z","operators":[{"name":"Example Name","operatorId":"00000000-0000-0000-0000-000000000253","merchantNumber":"EXAMPLE-001","terminalNumber":"EXAMPLE-001","isDeleted":false}],"settlementSchedule":0,"contracts":[{"contractId":"00000000-0000-0000-0000-000000000239","isDeleted":false,"contractProducts":[]}],"openingHours":{"1":{"opening":"09:00","closing":"17:00"}},"schedules":[{"year":1,"months":[{"month":1,"closedDays":[]}] }]}</example>
    public class MerchantResponse
    {
        #region Properties
        /// <summary>The addresses.</summary>

        public List<AddressResponse> Addresses { get; set; }
        /// <summary>The contacts.</summary>
        
        public List<ContactResponse> Contacts { get; set; }

        /// <summary>Devices registered to the merchant, keyed by device ID.</summary>
        public Dictionary<Guid, String> Devices { get; set; }
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The estate reporting id.</summary>

        public Int32 EstateReportingId { get; set; }
        /// <summary>The merchant id.</summary>

        public Guid MerchantId { get; set; }
        /// <summary>The merchant reporting id.</summary>

        public Int32 MerchantReportingId { get; set; }
        /// <summary>The merchant name.</summary>

        public String MerchantName { get; set; }
        /// <summary>The merchant reference.</summary>
        
        public String MerchantReference { get; set; }
        /// <summary>The next statement date.</summary>

        public DateTime NextStatementDate { get; set; }
        /// <summary>The operators.</summary>

        public List<MerchantOperatorResponse> Operators { get; set; }
        /// <summary>The settlement schedule.</summary>

        public SettlementSchedule SettlementSchedule { get; set; }
        /// <summary>The contracts.</summary>

        public List<MerchantContractResponse> Contracts { get; set; }

        /// <summary>Opening and closing times keyed by day of week.</summary>
        public Dictionary<DayOfWeek, OpeningHoursResponse> OpeningHours { get; set; }
        /// <summary>The schedules.</summary>

        public List<MerchantScheduleResponse> Schedules { get; set; }

        #endregion
    }
}
