using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>Data transfer object for AddMerchantDeviceResponse.</summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","deviceId":"00000000-0000-0000-0000-000000000001"}</example>
    [ExcludeFromCodeCoverage]
    public class AddMerchantDeviceResponse
    {
        #region Properties
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The merchant id.</summary>

        public Guid MerchantId { get; set; }
        /// <summary>The device id.</summary>

        public Guid DeviceId { get; set; }

        #endregion
    }
}
