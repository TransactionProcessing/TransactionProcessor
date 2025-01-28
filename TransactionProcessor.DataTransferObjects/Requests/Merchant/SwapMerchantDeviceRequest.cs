using System;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    public class SwapMerchantDeviceRequest
    {
        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        [JsonProperty("new_device_identifier")]
        public String NewDeviceIdentifier { get; set; }
    }
}