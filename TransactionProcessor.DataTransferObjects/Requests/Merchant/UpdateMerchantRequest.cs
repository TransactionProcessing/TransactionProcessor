using System;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// Opening and closing times by day of week for a merchant.
    /// </summary>
    /// <example>{"name":"Example Name","settlementSchedule":0}</example>
    public class UpdateMerchantRequest
    {
        #region Properties
        /// <summary>
        /// Display name for the estate, merchant, or operator.
        /// </summary>
        public String Name { get; set; }
        
        /// <summary>
        /// Settlement schedule assigned to the merchant.
        /// </summary>
        public SettlementSchedule SettlementSchedule { get; set; }

        #endregion
    }

    /// <summary>
    /// Opening and closing times for each day of the merchant week.
    /// </summary>
    /// <example>{"sunday":{"opening":"example","closing":"example"},"monday":{"opening":"example","closing":"example"},"tuesday":{"opening":"example","closing":"example"},"wednesday":{"opening":"example","closing":"example"},"thursday":{"opening":"example","closing":"example"},"friday":{"opening":"example","closing":"example"},"saturday":{"opening":"example","closing":"example"}}</example>
    public class MerchantOpeningRequest {
        /// <summary>
        /// Opening and closing times for Sunday.
        /// </summary>
        public OpeningHours Sunday { get; set; }
        /// <summary>
        /// Opening and closing times for Monday.
        /// </summary>
        public OpeningHours Monday { get; set; }
        /// <summary>
        /// Opening and closing times for Tuesday.
        /// </summary>
        public OpeningHours Tuesday { get; set; }
        /// <summary>
        /// Opening and closing times for Wednesday.
        /// </summary>
        public OpeningHours Wednesday { get; set; }
        /// <summary>
        /// Opening and closing times for Thursday.
        /// </summary>
        public OpeningHours Thursday { get; set; }
        /// <summary>
        /// Opening and closing times for Friday.
        /// </summary>
        public OpeningHours Friday { get; set; }
        /// <summary>
        /// Opening and closing times for Saturday.
        /// </summary>
        public OpeningHours Saturday { get; set; }
    }


    /// <summary>
    /// Opening and closing time for one day.
    /// </summary>
    /// <example>{"opening":"example","closing":"example"}</example>
    public class OpeningHours
    {
        /// <summary>
        /// Local opening time for the day, represented as a string.
        /// </summary>
        public string Opening { get; set; }
        /// <summary>
        /// Local closing time for the day, represented as a string.
        /// </summary>
        public string Closing { get; set; }
    }
    /// <summary>Data transfer object for OpeningHoursResponse.</summary>
    /// <example>{"opening":"example","closing":"example"}</example>

    public class OpeningHoursResponse
    {
        /// <summary>
        /// Local opening time for the day, represented as a string.
        /// </summary>
        public string Opening { get; set; }
        /// <summary>
        /// Local closing time for the day, represented as a string.
        /// </summary>
        public string Closing { get; set; }
    }
}
