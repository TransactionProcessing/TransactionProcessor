namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPrePay;

using System;

public class VendResponse{
    public String Msg { get; set; }
    public Int32 Status { get; set; }
    public Transaction Transaction{ get; set; }
}