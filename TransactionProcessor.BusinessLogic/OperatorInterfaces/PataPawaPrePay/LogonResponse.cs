namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPrePay;

using System;

public class LogonResponse{
    public String Balance{ get; set; }
    public String Key{ get; set; }
    public String Msg{ get; set; }
    public Int32 Status{ get; set; }
}