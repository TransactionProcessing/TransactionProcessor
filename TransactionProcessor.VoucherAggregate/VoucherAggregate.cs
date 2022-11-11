namespace TransactionProcessor.VoucherAggregate;

using System.Diagnostics.CodeAnalysis;
using Models;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using Voucher.DomainEvents;

public class VoucherAggregate : Aggregate
{
    #region Fields

    /// <summary>
    /// The random
    /// </summary>
    private static readonly Random _random = new Random();

    /// <summary>
    /// Gets the barcode.
    /// </summary>
    /// <value>
    /// The barcode.
    /// </value>
    private String Barcode;

    /// <summary>
    /// Gets the estate identifier.
    /// </summary>
    /// <value>
    /// The estate identifier.
    /// </value>
    private Guid EstateId;

    /// <summary>
    /// Gets the expiry date.
    /// </summary>
    /// <value>
    /// The expiry date.
    /// </value>
    private DateTime ExpiryDate;

    /// <summary>
    /// Gets a value indicating whether this instance is generated.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is generated; otherwise, <c>false</c>.
    /// </value>
    private Boolean IsGenerated;

    /// <summary>
    /// Gets a value indicating whether this instance is issued.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is issued; otherwise, <c>false</c>.
    /// </value>
    private Boolean IsIssued;

    /// <summary>
    /// The is redeemed
    /// </summary>
    private Boolean IsRedeemed;

    /// <summary>
    /// Gets the issued date time.
    /// </summary>
    /// <value>
    /// The issued date time.
    /// </value>
    private DateTime IssuedDateTime;

    /// <summary>
    /// The generated date time
    /// </summary>
    private DateTime GeneratedDateTime;

    /// <summary>
    /// The redeemed date time
    /// </summary>
    private DateTime RedeemedDateTime;

    /// <summary>
    /// Gets the message.
    /// </summary>
    /// <value>
    /// The message.
    /// </value>
    private String Message;

    /// <summary>
    /// The RDM
    /// </summary>
    private static readonly Random rdm = new Random();

    /// <summary>
    /// The recipient email
    /// </summary>
    private String RecipientEmail;

    /// <summary>
    /// The recipient mobile
    /// </summary>
    private String RecipientMobile;

    /// <summary>
    /// Gets the transaction identifier.
    /// </summary>
    /// <value>
    /// The transaction identifier.
    /// </value>
    private Guid TransactionId;

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    private Decimal Value;

    /// <summary>
    /// The balance
    /// </summary>
    private Decimal Balance;

    /// <summary>
    /// Gets the voucher code.
    /// </summary>
    /// <value>
    /// The voucher code.
    /// </value>
    private String VoucherCode;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="VoucherAggregate" /> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public VoucherAggregate()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VoucherAggregate" /> class.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier.</param>
    private VoucherAggregate(Guid aggregateId)
    {
        Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

        this.AggregateId = aggregateId;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds the barcode.
    /// </summary>
    /// <param name="barcodeAsBase64">The barcode as base64.</param>
    public void AddBarcode(String barcodeAsBase64)
    {
        Guard.ThrowIfNullOrEmpty(barcodeAsBase64, nameof(barcodeAsBase64));

        this.CheckIfVoucherHasBeenGenerated();
        this.CheckIfVoucherAlreadyIssued();

        BarcodeAddedEvent barcodeAddedEvent = new BarcodeAddedEvent(this.AggregateId, this.EstateId, barcodeAsBase64);

        this.ApplyAndAppend(barcodeAddedEvent);
    }

    /// <summary>
    /// Creates the specified aggregate identifier.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <returns></returns>
    public static VoucherAggregate Create(Guid aggregateId)
    {
        return new VoucherAggregate(aggregateId);
    }

    /// <summary>
    /// Generates the specified operator identifier.
    /// </summary>
    /// <param name="operatorIdentifier">The operator identifier.</param>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="generatedDateTime">The generated date time.</param>
    /// <param name="value">The value.</param>
    public void Generate(String operatorIdentifier,
                         Guid estateId,
                         Guid transactionId,
                         DateTime generatedDateTime,
                         Decimal value)
    {
        Guard.ThrowIfInvalidDate(generatedDateTime, nameof(generatedDateTime));
        Guard.ThrowIfNullOrEmpty(operatorIdentifier, nameof(operatorIdentifier));
        Guard.ThrowIfInvalidGuid(transactionId, nameof(transactionId));
        Guard.ThrowIfInvalidGuid(estateId, nameof(estateId));
        Guard.ThrowIfNegative(value, nameof(value));
        Guard.ThrowIfZero(value, nameof(value));
        this.CheckIfVoucherAlreadyGenerated();

        // Do the generate process here...
        String voucherCode = this.GenerateVoucherCode();
        DateTime expiryDateTime = generatedDateTime.AddDays(30); // Default to a 30 day expiry for now...
        String message = string.Empty;

        VoucherGeneratedEvent voucherGeneratedEvent =
            new VoucherGeneratedEvent(this.AggregateId, estateId, transactionId, generatedDateTime, operatorIdentifier, value, voucherCode, expiryDateTime, message);

        this.ApplyAndAppend(voucherGeneratedEvent);
    }

    /// <summary>
    /// Gets the voucher.
    /// </summary>
    /// <returns></returns>
    public Voucher GetVoucher()
    {
        return new Voucher
               {
                   EstateId = this.EstateId,
                   Value = this.Value,
                   GeneratedDateTime = this.GeneratedDateTime,
                   IssuedDateTime = this.IssuedDateTime,
                   RedeemedDateTime = this.RedeemedDateTime,
                   TransactionId = this.TransactionId,
                   ExpiryDate = this.ExpiryDate,
                   IsIssued = this.IsIssued,
                   IsRedeemed = this.IsRedeemed,
                   Barcode = this.Barcode,
                   Message = this.Message,
                   VoucherCode = this.VoucherCode,
                   IsGenerated = this.IsGenerated,
                   RecipientEmail = this.RecipientEmail,
                   RecipientMobile = this.RecipientMobile,
                   Balance = this.Balance,
                   VoucherId = this.AggregateId
               };
    }

    /// <summary>
    /// Issues the specified recipient email.
    /// </summary>
    /// <param name="recipientEmail">The recipient email.</param>
    /// <param name="recipientMobile">The recipient mobile.</param>
    /// <param name="issuedDateTime">The issued date time.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Issue(String recipientEmail,
                      String recipientMobile,
                      DateTime issuedDateTime)
    {
        this.CheckIfVoucherHasBeenGenerated();

        if (string.IsNullOrEmpty(recipientEmail) && string.IsNullOrEmpty(recipientMobile))
        {
            throw new ArgumentNullException(message: "Either Recipient Email Address or Recipient Mobile number must be set to issue a voucher", innerException: null);
        }

        this.CheckIfVoucherAlreadyIssued();

        VoucherIssuedEvent voucherIssuedEvent = new VoucherIssuedEvent(this.AggregateId, this.EstateId, issuedDateTime, recipientEmail, recipientMobile);

        this.ApplyAndAppend(voucherIssuedEvent);
    }

    /// <summary>
    /// Redeems the specified redeemed date time.
    /// </summary>
    /// <param name="redeemedDateTime">The redeemed date time.</param>
    public void Redeem(DateTime redeemedDateTime)
    {
        this.CheckIfVoucherHasBeenGenerated();
        this.CheckIfVoucherHasBeenIssued();
        this.CheckIfVoucherAlreadyRedeemed();

        VoucherFullyRedeemedEvent voucherFullyRedeemedEvent = new VoucherFullyRedeemedEvent(this.AggregateId, this.EstateId, redeemedDateTime);

        this.ApplyAndAppend(voucherFullyRedeemedEvent);
    }

    /// <summary>
    /// Gets the metadata.
    /// </summary>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    protected override Object GetMetadata()
    {
        return new
               {
                   this.EstateId
               };
    }

    /// <summary>
    /// Plays the event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    public override void PlayEvent(IDomainEvent domainEvent)
    {
        this.PlayEvent((dynamic)domainEvent);
    }

    /// <summary>
    /// Checks if voucher already generated.
    /// </summary>
    /// <exception cref="InvalidOperationException">Voucher Id [{this.AggregateId}] has already been generated</exception>
    private void CheckIfVoucherAlreadyGenerated()
    {
        if (this.IsGenerated)
        {
            throw new InvalidOperationException($"Voucher Id [{this.AggregateId}] has already been generated");
        }
    }

    /// <summary>
    /// Checks if voucher already issued.
    /// </summary>
    /// <exception cref="InvalidOperationException">Voucher Id [{this.AggregateId}] has already been issued</exception>
    private void CheckIfVoucherAlreadyIssued()
    {
        if (this.IsIssued)
        {
            throw new InvalidOperationException($"Voucher Id [{this.AggregateId}] has already been issued");
        }
    }

    /// <summary>
    /// Checks if voucher already redeemed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Voucher Id [{this.AggregateId}] has already been redeemed</exception>
    private void CheckIfVoucherAlreadyRedeemed()
    {
        if (this.IsRedeemed)
        {
            throw new InvalidOperationException($"Voucher Id [{this.AggregateId}] has already been redeemed");
        }
    }

    /// <summary>
    /// Checks if voucher has been generated.
    /// </summary>
    /// <exception cref="InvalidOperationException">Voucher Id [{this.AggregateId}] has not been generated</exception>
    private void CheckIfVoucherHasBeenGenerated()
    {
        if (this.IsGenerated == false)
        {
            throw new InvalidOperationException($"Voucher Id [{this.AggregateId}] has not been generated");
        }
    }

    /// <summary>
    /// Checks if voucher has been issued.
    /// </summary>
    /// <exception cref="InvalidOperationException">Voucher Id [{this.AggregateId}] has not been issued</exception>
    private void CheckIfVoucherHasBeenIssued()
    {
        if (this.IsIssued == false)
        {
            throw new InvalidOperationException($"Voucher Id [{this.AggregateId}] has not been issued");
        }
    }

    /// <summary>
    /// Generates the voucher code.
    /// </summary>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    private String GenerateVoucherCode(Int32 length = 10)
    {
        // validate length to be greater than 0
        if (length <= 1) length = 10;

        Int32 min = (Int32)Math.Pow(10, length - 1);
        Int32 max = (Int32)Math.Pow(10, length) - 1;

        return VoucherAggregate.rdm.Next(min, max).ToString();
    }

    /// <summary>
    /// Plays the event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    private void PlayEvent(VoucherGeneratedEvent domainEvent)
    {
        this.IsGenerated = true;
        this.EstateId = domainEvent.EstateId;
        this.GeneratedDateTime = domainEvent.GeneratedDateTime;
        this.ExpiryDate = domainEvent.ExpiryDateTime;
        this.VoucherCode = domainEvent.VoucherCode;
        this.Message = domainEvent.Message;
        this.TransactionId = domainEvent.TransactionId;
        this.Value = domainEvent.Value;
        this.Balance = 0;
    }

    /// <summary>
    /// Plays the event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    private void PlayEvent(VoucherIssuedEvent domainEvent)
    {
        this.IsIssued = true;
        this.IssuedDateTime = domainEvent.IssuedDateTime;
        this.RecipientEmail = domainEvent.RecipientEmail;
        this.RecipientMobile = domainEvent.RecipientMobile;
        this.Balance = this.Value;
    }

    /// <summary>
    /// Plays the event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    private void PlayEvent(BarcodeAddedEvent domainEvent)
    {
        this.Barcode = domainEvent.Barcode;
    }

    /// <summary>
    /// Plays the event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    private void PlayEvent(VoucherFullyRedeemedEvent domainEvent)
    {
        this.RedeemedDateTime = domainEvent.RedeemedDateTime;
        this.IsRedeemed = true;
        this.Balance = 0;
    }

    #endregion
}