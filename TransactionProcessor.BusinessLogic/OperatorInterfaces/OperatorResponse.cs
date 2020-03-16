namespace TransactionProcessor.BusinessLogic.OperatorInterfaces
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class OperatorResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the authorisation code.
        /// </summary>
        /// <value>
        /// The authorisation code.
        /// </value>
        public String AuthorisationCode { get; set; }

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        public String ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        public String ResponseMessage { get; set; }

        #endregion
    }
}