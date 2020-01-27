namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SerialisedMessage
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialisedMessage"/> class.
        /// </summary>
        public SerialisedMessage()
        {
            this.Metadata = new Dictionary<String, String>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        /// <value>
        /// The metadata.
        /// </value>
        [JsonProperty("metadata")]
        public Dictionary<String, String> Metadata { get; set; }

        /// <summary>
        /// Gets or sets the serialised data.
        /// </summary>
        /// <value>
        /// The serialised data.
        /// </value>
        [JsonProperty("serialised_data")]
        public String SerialisedData { get; set; }

        #endregion
    }
}