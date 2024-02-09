namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPrePay;

using System;

public class MeterResponse{
    public String Code { get; set; }
    public String CustomerName { get; set; }
    public String Msg { get; set; }
    public Int32 Status { get; set; }
}