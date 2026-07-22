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

    [ExcludeFromCodeCoverage]
    public class DepositRequest
    {
        public string TransactionId { get; set; }
        public string CustomerId { get; set; }

        public string AgentId { get; set; }

        public string AccountNumber { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public String Channel { get; set; }
        public String Narration { get; set; }
        public String ReferenceNumber { get; set; }
    }

    public class WithdrawalRequest
    {
        public string TransactionId { get; set; }

        public string CustomerId { get; set; }

        public string AgentId { get; set; }

        public string AccountNumber { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public String Channel { get; set; }
        public String Narration { get; set; }
        public String ReferenceNumber { get; set; }
    }

    public class TransactionResult
    {
        public string ResponseCode { get; set; } = "";

        public string TransactionId { get; set; } = "";
    }
}
