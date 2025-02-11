using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.BusinessLogic.Events
{
    [ExcludeFromCodeCoverage]
    public record CallbackReceivedEnrichedEvent : DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackReceivedEnrichedEvent"/> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        public CallbackReceivedEnrichedEvent(Guid aggregateId) : base(aggregateId, Guid.NewGuid())
        {
        }

        public CallbackReceivedEnrichedEvent(Guid aggregateId, Guid estateId, Int32 messageFormat,
                                             String reference, String typeString,
                                             String callbackMessage) : base(aggregateId, Guid.NewGuid()){
            this.EstateId = estateId;
            this.MessageFormat = messageFormat;
            this.Reference = reference;
            this.TypeString = typeString;
            this.CallbackMessage = callbackMessage;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the callback message.
        /// </summary>
        /// <value>
        /// The callback message.
        /// </value>
        [JsonProperty("callbackMessage")]
        public String CallbackMessage { get; set; }

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty("estateid")]
        public Guid EstateId { get; set; }

        /// <summary>
        /// Gets or sets the message format.
        /// </summary>
        /// <value>
        /// The message format.
        /// </value>
        [JsonProperty("messageFormat")]
        public Int32 MessageFormat { get; set; }

        /// <summary>
        /// Gets or sets the reference.
        /// </summary>
        /// <value>
        /// The reference.
        /// </value>
        [JsonProperty("reference")]
        public String Reference { get; set; }

        /// <summary>
        /// Gets or sets the type string.
        /// </summary>
        /// <value>
        /// The type string.
        /// </value>
        [JsonProperty("typeString")]
        public String TypeString { get; set; }

        #endregion
    }
}