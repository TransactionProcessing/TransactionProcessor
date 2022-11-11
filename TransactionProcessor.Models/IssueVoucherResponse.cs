namespace TransactionProcessor.Models;

using System;

public class IssueVoucherResponse
{
    public Guid VoucherId { get; set; }

    public DateTime ExpiryDate { get; set; }

    public String VoucherCode { get; set; }

    public String Message { get; set; }
}