namespace TransactionProcessor.Database.Entities.Summary
{
    public class SettlementSummary
    {
        public bool IsCompleted { get; set; }
        public bool IsSettled { get; set; }
        public DateTime SettlementDate { get; set; }
        public int MerchantReportingId { get; set; }
        public int OperatorReportingId { get; set; }
        public int ContractProductReportingId { get; set; }
        public decimal? SalesValue { get; set; }
        public decimal? FeeValue { get; set; }
        public int? SalesCount { get; set; }
        public int? FeeCount { get; set; }
    }

    public class TodayTransaction
    {
        public int MerchantReportingId { get; set; }
        public int ContractProductReportingId { get; set; }
        public int ContractReportingId { get; set; }
        public int OperatorReportingId { get; set; }
        public Guid TransactionId { get; set; }
        public string AuthorisationCode { get; set; }
        public string DeviceIdentifier { get; set; }
        public bool IsAuthorised { get; set; }
        public bool IsCompleted { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string TransactionNumber { get; set; }
        public string TransactionReference { get; set; }
        public TimeSpan TransactionTime { get; set; }
        public int TransactionSource { get; set; }
        public string TransactionType { get; set; }
        public int TransactionReportingId { get; set; }
        public decimal TransactionAmount { get; set; }
        public int? Hour { get; set; }
    }

    public class TransactionHistory
    {
        public int MerchantReportingId { get; set; }
        public int ContractProductReportingId { get; set; }
        public int ContractReportingId { get; set; }
        public int OperatorReportingId { get; set; }
        public Guid TransactionId { get; set; }
        public string AuthorisationCode { get; set; }
        public string DeviceIdentifier { get; set; }
        public bool IsAuthorised { get; set; }
        public bool IsCompleted { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string TransactionNumber { get; set; }
        public string TransactionReference { get; set; }
        public TimeSpan TransactionTime { get; set; }
        public int TransactionSource { get; set; }
        public string TransactionType { get; set; }
        public int TransactionReportingId { get; set; }
        public decimal TransactionAmount { get; set; }
        public int? Hour { get; set; }
    }

}
