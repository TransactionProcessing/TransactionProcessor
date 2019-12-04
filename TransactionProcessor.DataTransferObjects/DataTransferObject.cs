﻿namespace TransactionProcessor.DataTransferObjects
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public abstract class DataTransferObject
    {
        #region Properties

        // This only here as a shell base class...
        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; set; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; set; }

        #endregion
    }

    public class MetadataContants
    {
        public const String KeyNameEstateId = "EstateId";
        public const String KeyNameMerchantId = "MerchantId";
    }
}