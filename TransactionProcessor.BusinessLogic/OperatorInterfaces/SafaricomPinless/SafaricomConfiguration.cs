namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.SafaricomPinless
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SafaricomConfiguration : BaseOperatorConfiguration
    {
        #region Properties

        public String ExtCode { get; set; }

        public String LoginId { get; set; }

        public String MSISDN { get; set; }

        public String Password { get; set; }

        public String Pin { get; set; }
        
        #endregion
    }

    public abstract class BaseOperatorConfiguration
    {
        public String Url { get; set; }
        public Boolean ApiLogonRequired { get; set; }
    }
}