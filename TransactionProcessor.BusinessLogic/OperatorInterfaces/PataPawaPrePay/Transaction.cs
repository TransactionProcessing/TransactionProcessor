namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPrePay;

using System;

public class Transaction{
    public Int32 TransactionId { get; set; }
    public String Ref{ get; set; }
    public String Units { get; set; }
    public String Token { get; set; }
    public String StdTokenRctNum { get; set; }
    public Decimal StdTokenAmt { get; set; }
    public Decimal StdTokenTax { get; set; }
    public String MeterNo { get; set; }
    public DateTime Date { get; set; }
    public String CustomerName { get; set; }
}