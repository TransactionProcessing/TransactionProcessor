namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.SafaricomPinless
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "COMMAND")]
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
        public String TYPE { get; set; }

        /// <summary>
        /// Gets or sets the txnstatus.
        /// </summary>
        /// <value>
        /// The txnstatus.
        /// </value>
        [XmlElement(ElementName = "TXNSTATUS")]
        public Int32 TXNSTATUS { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>
        /// The date.
        /// </value>
        [XmlElement(ElementName = "DATE")]
        public String DATE { get; set; }

        /// <summary>
        /// Gets or sets the extrefnum.
        /// </summary>
        /// <value>
        /// The extrefnum.
        /// </value>
        [XmlElement(ElementName = "EXTREFNUM")]
        public String EXTREFNUM { get; set; }

        /// <summary>
        /// Gets or sets the txnid.
        /// </summary>
        /// <value>
        /// The txnid.
        /// </value>
        [XmlElement(ElementName = "TXNID")]
        public String TXNID { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [XmlElement(ElementName = "MESSAGE")]
        public String MESSAGE { get; set; }

        #endregion
    }
}