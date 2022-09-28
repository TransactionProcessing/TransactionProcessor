namespace TransactionProcessor.Transaction.DomainEvents;

using System;
using Shared.DomainDrivenDesign.EventSourcing;

public record CustomerEmailReceiptResendRequestedEvent : DomainEvent
{
    #region Constructors

    public CustomerEmailReceiptResendRequestedEvent(Guid aggregateId,
                                                    Guid estateId,
                                                    Guid merchantId) : base(aggregateId, Guid.NewGuid())
    {
        this.TransactionId = aggregateId;
        this.EstateId = estateId;
        this.MerchantId = merchantId;
    }

    #endregion

    #region Properties
    
    /// <summary>
    /// Gets the estate identifier.
    /// </summary>
    /// <value>
    /// The estate identifier.
    /// </value>
    public Guid EstateId { get; init; }

    /// <summary>
    /// Gets the merchant identifier.
    /// </summary>
    /// <value>
    /// The merchant identifier.
    /// </value>
    public Guid MerchantId { get; init; }

    /// <summary>
    /// Gets the transaction identifier.
    /// </summary>
    /// <value>
    /// The transaction identifier.
    /// </value>
    public Guid TransactionId { get; init; }

    #endregion
}