namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.SafaricomPinless
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SafaricomConfiguration
    {
        #region Properties

        /// <summary>
        /// Gets or sets the ext code.
        /// </summary>
        /// <value>
        /// The ext code.
        /// </value>
        public String ExtCode { get; set; }

        /// <summary>
        /// Gets or sets the login identifier.
        /// </summary>
        /// <value>
        /// The login identifier.
        /// </value>
        public String LoginId { get; set; }

        /// <summary>
        /// Gets or sets the msisdn.
        /// </summary>
        /// <value>
        /// The msisdn.
        /// </value>
        public String MSISDN { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public String Password { get; set; }

        /// <summary>
        /// Gets or sets the pin.
        /// </summary>
        /// <value>
        /// The pin.
        /// </value>
        public String Pin { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public String Url { get; set; }

        #endregion
    }
}