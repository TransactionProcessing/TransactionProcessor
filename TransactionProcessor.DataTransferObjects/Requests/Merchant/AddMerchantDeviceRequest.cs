using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    [ExcludeFromCodeCoverage]
    public class AddMerchantDeviceRequest
    {
        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        [JsonProperty("device_identifier")]
        public String DeviceIdentifier { get; set; }
    }
}
