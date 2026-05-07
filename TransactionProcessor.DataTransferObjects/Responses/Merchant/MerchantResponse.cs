using System;
using System.Collections.Generic;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Responses.Contract;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>
    /// 
    /// </summary>
    public class MerchantResponse
    {
        #region Properties

        public List<AddressResponse> Addresses { get; set; }
        
        public List<ContactResponse> Contacts { get; set; }

        public Dictionary<Guid, String> Devices { get; set; }

        public Guid EstateId { get; set; }

        public Int32 EstateReportingId { get; set; }

        public Guid MerchantId { get; set; }

        public Int32 MerchantReportingId { get; set; }

        public String MerchantName { get; set; }
        
        public String MerchantReference { get; set; }

        public DateTime NextStatementDate { get; set; }

        public List<MerchantOperatorResponse> Operators { get; set; }

        public SettlementSchedule SettlementSchedule { get; set; }

        public List<MerchantContractResponse> Contracts { get; set; }

        public Dictionary<DayOfWeek, OpeningHoursResponse> OpeningHours { get; set; }

        public List<MerchantScheduleResponse> Schedules { get; set; }

        #endregion
    }
}
