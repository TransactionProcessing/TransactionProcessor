using System;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// Replacement device identifier for a merchant device.
    /// </summary>
    /// <example>{"newDeviceIdentifier":"EXAMPLE-001"}</example>
    public class SwapMerchantDeviceRequest
    {
        /// <summary>
        /// Replacement device identifier.
        /// </summary>
        public String NewDeviceIdentifier { get; set; }
    }
}
