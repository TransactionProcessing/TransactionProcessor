using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateMerchantRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        [JsonProperty("address")]
        [Required]
        public Address Address { get; set; }

        /// <summary>
        /// Gets or sets the contact.
        /// </summary>
        /// <value>
        /// The contact.
        /// </value>
        [JsonProperty("contact")]
        [Required]
        public Contact Contact { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [JsonProperty("name")]
        public String Name { get; set; }
        
        [JsonProperty("settlement_schedule")]
        public SettlementSchedule SettlementSchedule { get; set; }

        [JsonProperty("merchant_id")]
        public Guid? MerchantId { get; set; }

        [JsonProperty("created_date_time")]
        public DateTime? CreatedDateTime { get; set; }

        #endregion
    }
}