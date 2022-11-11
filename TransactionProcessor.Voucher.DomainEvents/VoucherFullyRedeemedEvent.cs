namespace TransactionProcessor.Voucher.DomainEvents;

using Shared.DomainDrivenDesign.EventSourcing;

public record VoucherFullyRedeemedEvent : DomainEvent
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="VoucherFullyRedeemedEvent" /> class.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="redeemedDateTime">The redeemed date time.</param>
    public VoucherFullyRedeemedEvent(Guid aggregateId,
                                     Guid estateId,
                                     DateTime redeemedDateTime) : base(aggregateId, Guid.NewGuid())
    {
        this.EstateId = estateId;
        this.RedeemedDateTime = redeemedDateTime;
        this.VoucherId = aggregateId;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the estate identifier.
    /// </summary>
    /// <value>
    /// The estate identifier.
    /// </value>
    public Guid EstateId { get; init; }

    /// <summary>
    /// Gets or sets the redeemed date time.
    /// </summary>
    /// <value>
    /// The redeemed date time.
    /// </value>
    public DateTime RedeemedDateTime { get; init; }

    /// <summary>
    /// Gets or sets the voucher identifier.
    /// </summary>
    /// <value>
    /// The voucher identifier.
    /// </value>
    public Guid VoucherId { get; init; }

    #endregion
}