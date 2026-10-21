using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>Data transfer object for SwapMerchantDeviceResponse.</summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","deviceId":"00000000-0000-0000-0000-000000000001"}</example>
    [ExcludeFromCodeCoverage]
    public class SwapMerchantDeviceResponse
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
