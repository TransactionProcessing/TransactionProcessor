namespace TransactionProcessor.Models
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class ProcessLogonTransactionResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        public Int32 ResponseCode { get; set; }

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