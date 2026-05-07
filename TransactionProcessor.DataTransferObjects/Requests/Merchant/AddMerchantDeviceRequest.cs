using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    [ExcludeFromCodeCoverage]
    public class AddMerchantDeviceRequest
    {
        public String DeviceIdentifier { get; set; }
    }
}
