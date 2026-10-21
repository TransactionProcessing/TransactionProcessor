using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// Device identifier to register for a merchant.
    /// </summary>
    /// <example>{"deviceIdentifier":"EXAMPLE-001"}</example>
    [ExcludeFromCodeCoverage]
    public class AddMerchantDeviceRequest
    {
        /// <summary>
        /// Identifier of the merchant device to register.
        /// </summary>
        public String DeviceIdentifier { get; set; }
    }
}
