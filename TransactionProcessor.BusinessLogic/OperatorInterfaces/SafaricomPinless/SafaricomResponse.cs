namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.SafaricomPinless
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Serialization;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "COMMAND")]
    [ExcludeFromCodeCoverage]
    public class SafaricomResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [XmlElement(ElementName = "TYPE")]
        public String TransactionType { get; set; }

        /// <summary>
        /// Gets or sets the txnstatus.
        /// </summary>
        /// <value>
        /// The txnstatus.
        /// </value>
        [XmlElement(ElementName = "TXNSTATUS")]
        public Int32 TransactionStatus { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        [XmlElement(ElementName = "DATE")]
        public String TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets the extrefnum.
        /// </summary>
        /// <value>
        /// The extrefnum.
        /// </value>
        [XmlElement(ElementName = "EXTREFNUM")]
        public String ExternalReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the txnid.
        /// </summary>
        /// <value>
        /// The txnid.
        /// </value>
        [XmlElement(ElementName = "TXNID")]
        public String TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [XmlElement(ElementName = "MESSAGE")]
        public String Message { get; set; }

        #endregion
    }
}