namespace TransactionProcessor.Voucher.DomainEvents;

using Shared.DomainDrivenDesign.EventSourcing;

public record BarcodeAddedEvent : DomainEvent
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="BarcodeAddedEvent" /> class.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="barcode">The barcode.</param>
    public BarcodeAddedEvent(Guid aggregateId,
                             Guid estateId,
                             String barcode) : base(aggregateId, Guid.NewGuid())
    {
        this.EstateId = estateId;
        this.VoucherId = aggregateId;
        this.Barcode = barcode;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the barcode.
    /// </summary>
    /// <value>
    /// The barcode.
    /// </value>
    public String Barcode { get; init; }

    /// <summary>
    /// Gets or sets the estate identifier.
    /// </summary>
    /// <value>
    /// The estate identifier.
    /// </value>
    public Guid EstateId { get; init; }
    /// <summary>
    /// Gets or sets the voucher identifier.
    /// </summary>
    /// <value>
    /// The voucher identifier.
    /// </value>
    public Guid VoucherId { get; init; }

    #endregion
}