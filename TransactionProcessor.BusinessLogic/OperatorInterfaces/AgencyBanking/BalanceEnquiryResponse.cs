using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.AgencyBanking
{
    [ExcludeFromCodeCoverage]
    public class BalanceEnquiryRequest
    {
        public String AgentId { get; set; }

        public string AccountNumber { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BalanceEnquiryResponse
    {
        public String ResponseCode { get; set; }

        public string ResponseMessage { get; set; }

        public decimal AvailableBalance { get; set; }
    }
}
