using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TransactionProcessor.DataTransferObjects.Responses.Contract;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>
    /// 
    /// </summary>
    public class MerchantResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the addresses.
        /// </summary>
        /// <value>
        /// The addresses.
        /// </value>
        [JsonProperty("addresses")]
        public List<AddressResponse> Addresses { get; set; }
        
        /// <summary>
        /// Gets or sets the contacts.
        /// </summary>
        /// <value>
        /// The contacts.
        /// </value>
        [JsonProperty("contacts")]
        public List<ContactResponse> Contacts { get; set; }

        /// <summary>
        /// Gets or sets the devices.
        /// </summary>
        /// <value>
        /// The devices.
        /// </value>
        [JsonProperty("devices")]
        public Dictionary<Guid, String> Devices { get; set; }

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        [JsonProperty("estate_reporting_id")]
        public Int32 EstateReportingId { get; set; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        [JsonProperty("merchant_id")]
        public Guid MerchantId { get; set; }

        [JsonProperty("merchant_reporting_id")]
        public Int32 MerchantReportingId { get; set; }

        /// <summary>
        /// Gets or sets the name of the merchant.
        /// </summary>
        /// <value>
        /// The name of the merchant.
        /// </value>
        [JsonProperty("merchant_name")]
        public String MerchantName { get; set; }

        /// <summary>
        /// Gets or sets the merchant reference.
        /// </summary>
        /// <value>
        /// The merchant reference.
        /// </value>
        [JsonProperty("merchant_reference")]
        public String MerchantReference { get; set; }

        /// <summary>
        /// Gets or sets the next statement date.
        /// </summary>
        /// <value>
        /// The next statement date.
        /// </value>
        [JsonProperty("next_statement_date")]
        public DateTime NextStatementDate { get; set; }

        /// <summary>
        /// Gets or sets the operators.
        /// </summary>
        /// <value>
        /// The operators.
        /// </value>
        [JsonProperty("operators")]
        public List<MerchantOperatorResponse> Operators { get; set; }

        /// <summary>
        /// Gets or sets the settlement schedule.
        /// </summary>
        /// <value>
        /// The settlement schedule.
        /// </value>
        [JsonProperty("settlement_schedule")]
        public SettlementSchedule SettlementSchedule { get; set; }

        [JsonProperty("contracts")]
        public List<MerchantContractResponse> Contracts { get; set; }

        #endregion
    }
}