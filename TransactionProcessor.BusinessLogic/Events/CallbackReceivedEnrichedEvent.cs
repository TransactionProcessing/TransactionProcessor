using System;
using System.Diagnostics.CodeAnalysis;
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

        public String CallbackMessage { get; set; }

        public Guid EstateId { get; set; }

        public Int32 MessageFormat { get; set; }

        public String Reference { get; set; }

        public String TypeString { get; set; }

        #endregion
    }
}