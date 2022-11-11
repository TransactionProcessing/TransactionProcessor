namespace TransactionProcessor.Voucher.DomainEvents;

using Shared.DomainDrivenDesign.EventSourcing;

public record VoucherGeneratedEvent : DomainEvent
{
    /// <summary>
    /// Gets or sets the estate identifier.
    /// </summary>
    /// <value>
    /// The estate identifier.
    /// </value>
    public Guid EstateId { get; init; }

    /// <summary>
    /// Gets or sets the transaction identifier.
    /// </summary>
    /// <value>
    /// The transaction identifier.
    /// </value>
    public Guid TransactionId { get; init; }
    /// <summary>
    /// Gets or sets the voucher identifier.
    /// </summary>
    /// <value>
    /// The voucher identifier.
    /// </value>
    public Guid VoucherId { get; init; }
    /// <summary>
    /// Gets or sets the operator identifier.
    /// </summary>
    /// <value>
    /// The operator identifier.
    /// </value>
    public String OperatorIdentifier { get; init; }
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public Decimal Value { get; init; }

    /// <summary>
    /// Gets or sets the voucher code.
    /// </summary>
    /// <value>
    /// The voucher code.
    /// </value>
    public String VoucherCode { get; init; }

    /// <summary>
    /// Gets or sets the expiry date time.
    /// </summary>
    /// <value>
    /// The expiry date time.
    /// </value>
    public DateTime ExpiryDateTime { get; init; }
    /// <summary>
    /// Gets or sets the generated date time.
    /// </summary>
    /// <value>
    /// The generated date time.
    /// </value>
    public DateTime GeneratedDateTime { get; init; }
    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    /// <value>
    /// The message.
    /// </value>
    public String Message { get; init; }


    /// <summary>
    /// Initializes a new instance of the <see cref="VoucherGeneratedEvent" /> class.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="generatedDateTime">The generated date time.</param>
    /// <param name="operatorIdentifier">The operator identifier.</param>
    /// <param name="value">The value.</param>
    /// <param name="voucherCode">The voucher code.</param>
    /// <param name="expiryDateTime">The expiry date time.</param>
    /// <param name="message">The message.</param>
    public VoucherGeneratedEvent(Guid aggregateId,
                                 Guid estateId,
                                 Guid transactionId,
                                 DateTime generatedDateTime,
                                 String operatorIdentifier,
                                 Decimal value,
                                 String voucherCode,
                                 DateTime expiryDateTime,
                                 String message) : base(aggregateId, Guid.NewGuid())
    {
        this.EstateId = estateId;
        this.TransactionId = transactionId;
        this.GeneratedDateTime = generatedDateTime;
        this.OperatorIdentifier = operatorIdentifier;
        this.Value = value;
        this.VoucherCode = voucherCode;
        this.ExpiryDateTime = expiryDateTime;
        this.Message = message;
        this.VoucherId = aggregateId;
    }
}