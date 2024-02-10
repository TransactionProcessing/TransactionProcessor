namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPrePay;

using System;
using System.Diagnostics.CodeAnalysis;
using SafaricomPinless;

[ExcludeFromCodeCoverage]
public class PataPawaPrePaidConfiguration : BaseOperatorConfiguration
{
    public String Username { get; set; }
    public String Password { get; set; }
    public String Url { get; set; }
}