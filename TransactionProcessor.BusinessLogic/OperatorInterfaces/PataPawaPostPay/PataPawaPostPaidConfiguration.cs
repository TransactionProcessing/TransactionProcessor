namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPostPay;

using System;
using System.Diagnostics.CodeAnalysis;
using SafaricomPinless;

[ExcludeFromCodeCoverage]
public class PataPawaPostPaidConfiguration : BaseOperatorConfiguration
{
    public String Username { get; set; }
    public String Password { get; set; }
    public String Url { get; set; }
}