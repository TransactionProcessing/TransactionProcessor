using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using TransactionProcessor.Models;
using TransactionProcessor.Voucher.DomainEvents;

namespace TransactionProcessor.Aggregates;

public static class VoucherAggregateExtensions{
    public static void AddBarcode(this VoucherAggregate aggregate, String barcodeAsBase64)
    {
        Guard.ThrowIfNullOrEmpty(barcodeAsBase64, nameof(barcodeAsBase64));

        aggregate.CheckIfVoucherHasBeenGenerated();
        aggregate.CheckIfVoucherAlreadyIssued();

        BarcodeAddedEvent barcodeAddedEvent = new BarcodeAddedEvent(aggregate.AggregateId, aggregate.EstateId, barcodeAsBase64);

        aggregate.ApplyAndAppend(barcodeAddedEvent);
    }

    public static void Generate(this VoucherAggregate aggregate,
                        Guid operatorId,
                         Guid estateId,
                         Guid transactionId,
                         DateTime generatedDateTime,
                         Decimal value)
    {
        Guard.ThrowIfInvalidDate(generatedDateTime, nameof(generatedDateTime));
        Guard.ThrowIfInvalidGuid(operatorId, nameof(operatorId));
        Guard.ThrowIfInvalidGuid(transactionId, nameof(transactionId));
        Guard.ThrowIfInvalidGuid(estateId, nameof(estateId));
        Guard.ThrowIfNegative(value, nameof(value));
        Guard.ThrowIfZero(value, nameof(value));
        aggregate.CheckIfVoucherAlreadyGenerated();

        // Do the generate process here...
        String voucherCode = aggregate.GenerateVoucherCode();
        DateTime expiryDateTime = generatedDateTime.AddDays(30); // Default to a 30 day expiry for now...
        String message = string.Empty;

        VoucherGeneratedEvent voucherGeneratedEvent =
            new VoucherGeneratedEvent(aggregate.AggregateId, estateId, transactionId, generatedDateTime, operatorId, value, voucherCode, expiryDateTime, message);

        aggregate.ApplyAndAppend(voucherGeneratedEvent);
    }

    public static TransactionProcessor.Models.Voucher GetVoucher(this VoucherAggregate aggregate)
    {
        return new TransactionProcessor.Models.Voucher
               {
                   EstateId = aggregate.EstateId,
                   Value = aggregate.Value,
                   GeneratedDateTime = aggregate.GeneratedDateTime,
                   IssuedDateTime = aggregate.IssuedDateTime,
                   RedeemedDateTime = aggregate.RedeemedDateTime,
                   TransactionId = aggregate.TransactionId,
                   ExpiryDate = aggregate.ExpiryDate,
                   IsIssued = aggregate.IsIssued,
                   IsRedeemed = aggregate.IsRedeemed,
                   Barcode = aggregate.Barcode,
                   Message = aggregate.Message,
                   VoucherCode = aggregate.VoucherCode,
                   IsGenerated = aggregate.IsGenerated,
                   RecipientEmail = aggregate.RecipientEmail,
                   RecipientMobile = aggregate.RecipientMobile,
                   Balance = aggregate.Balance,
                   VoucherId = aggregate.AggregateId
               };
    }

    public static void Issue(this VoucherAggregate aggregate,
                             String recipientEmail,
                             String recipientMobile,
                             DateTime issuedDateTime)
    {
        aggregate.CheckIfVoucherHasBeenGenerated();

        if (string.IsNullOrEmpty(recipientEmail) && string.IsNullOrEmpty(recipientMobile))
        {
            throw new ArgumentNullException(message: "Either Recipient Email Address or Recipient Mobile number must be set to issue a voucher", innerException: null);
        }

        aggregate.CheckIfVoucherAlreadyIssued();

        VoucherIssuedEvent voucherIssuedEvent = new VoucherIssuedEvent(aggregate.AggregateId, aggregate.EstateId, issuedDateTime, recipientEmail, recipientMobile);

        aggregate.ApplyAndAppend(voucherIssuedEvent);
    }

    public static void Redeem(this VoucherAggregate aggregate, DateTime redeemedDateTime)
    {
        aggregate.CheckIfVoucherHasBeenGenerated();
        aggregate.CheckIfVoucherHasBeenIssued();
        aggregate.CheckIfVoucherAlreadyRedeemed();

        VoucherFullyRedeemedEvent voucherFullyRedeemedEvent = new VoucherFullyRedeemedEvent(aggregate.AggregateId, aggregate.EstateId, redeemedDateTime);

        aggregate.ApplyAndAppend(voucherFullyRedeemedEvent);
    }

    private static void CheckIfVoucherAlreadyGenerated(this VoucherAggregate aggregate)
    {
        if (aggregate.IsGenerated)
        {
            throw new InvalidOperationException($"Voucher Id [{aggregate.AggregateId}] has already been generated");
        }
    }
    
    private static void CheckIfVoucherAlreadyIssued(this VoucherAggregate aggregate)
    {
        if (aggregate.IsIssued)
        {
            throw new InvalidOperationException($"Voucher Id [{aggregate.AggregateId}] has already been issued");
        }
    }
    
    private static void CheckIfVoucherAlreadyRedeemed(this VoucherAggregate aggregate)
    {
        if (aggregate.IsRedeemed)
        {
            throw new InvalidOperationException($"Voucher Id [{aggregate.AggregateId}] has already been redeemed");
        }
    }
    
    private static void CheckIfVoucherHasBeenGenerated(this VoucherAggregate aggregate)
    {
        if (aggregate.IsGenerated == false)
        {
            throw new InvalidOperationException($"Voucher Id [{aggregate.AggregateId}] has not been generated");
        }
    }
    
    private static void CheckIfVoucherHasBeenIssued(this VoucherAggregate aggregate)
    {
        if (aggregate.IsIssued == false)
        {
            throw new InvalidOperationException($"Voucher Id [{aggregate.AggregateId}] has not been issued");
        }
    }

    private static String GenerateVoucherCode(this VoucherAggregate aggregate, Int32 length = 10)
    {
        // validate length to be greater than 0
        if (length <= 1) length = 10;

        Int32 min = (Int32)Math.Pow(10, length - 1);
        Int32 max = (Int32)Math.Pow(10, length) - 1;

        return rdm.Next(min, max).ToString();
    }
    
    private static readonly Random rdm = new Random();
    
    public static void PlayEvent(this VoucherAggregate aggregate, VoucherGeneratedEvent domainEvent)
    {
        aggregate.IsGenerated = true;
        aggregate.EstateId = domainEvent.EstateId;
        aggregate.GeneratedDateTime = domainEvent.GeneratedDateTime;
        aggregate.ExpiryDate = domainEvent.ExpiryDateTime;
        aggregate.VoucherCode = domainEvent.VoucherCode;
        aggregate.Message = domainEvent.Message;
        aggregate.TransactionId = domainEvent.TransactionId;
        aggregate.Value = domainEvent.Value;
        aggregate.Balance = 0;
    }
    
    public static void PlayEvent(this VoucherAggregate aggregate, VoucherIssuedEvent domainEvent)
    {
        aggregate.IsIssued = true;
        aggregate.IssuedDateTime = domainEvent.IssuedDateTime;
        aggregate.RecipientEmail = domainEvent.RecipientEmail;
        aggregate.RecipientMobile = domainEvent.RecipientMobile;
        aggregate.Balance = aggregate.Value;
    }

    public static void PlayEvent(this VoucherAggregate aggregate, BarcodeAddedEvent domainEvent)
    {
        aggregate.Barcode = domainEvent.Barcode;
    }

    public static void PlayEvent(this VoucherAggregate aggregate, VoucherFullyRedeemedEvent domainEvent)
    {
        aggregate.RedeemedDateTime = domainEvent.RedeemedDateTime;
        aggregate.IsRedeemed = true;
        aggregate.Balance = 0;
    }
}

public record VoucherAggregate : Aggregate
{
    #region Fields
   
    internal String Barcode;

    internal Guid EstateId;

    internal DateTime ExpiryDate;

    internal Boolean IsGenerated;

    internal Boolean IsIssued;

    internal Boolean IsRedeemed;

    internal DateTime IssuedDateTime;

    internal DateTime GeneratedDateTime;

    internal  DateTime RedeemedDateTime;
    
    internal  String Message;
    
    internal  String RecipientEmail;
    
    internal  String RecipientMobile;
    
    internal  Guid TransactionId;
    
    internal  Decimal Value;
    
    internal  Decimal Balance;
    
    internal String VoucherCode;

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
    /// Creates the specified aggregate identifier.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <returns></returns>
    public static VoucherAggregate Create(Guid aggregateId)
    {
        return new VoucherAggregate(aggregateId);
    }
    
    [ExcludeFromCodeCoverage]
    protected override Object GetMetadata()
    {
        return new
               {
                   this.EstateId
               };
    }

    public override void PlayEvent(IDomainEvent domainEvent) => VoucherAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

    #endregion
}