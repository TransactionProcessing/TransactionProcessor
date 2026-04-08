namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class MetadataContants
    {
        #region Others

        /// <summary>
        /// The key name estate identifier
        /// </summary>
        [SuppressMessage("Security", "S2068:Passwords should not be hardcoded", Justification = "Not a password - this is a metadata key name constant")]
        public static readonly String KeyNameEstateId = "estate_id";

        /// <summary>
        /// The key name merchant identifier
        /// </summary>
        [SuppressMessage("Security", "S2068:Passwords should not be hardcoded", Justification = "Not a password - this is a metadata key name constant")]
        public static readonly String KeyNameMerchantId = "merchant_id";

        #endregion
    }
}