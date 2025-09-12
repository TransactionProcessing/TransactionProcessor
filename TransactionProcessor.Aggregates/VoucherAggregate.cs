using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using SimpleResults;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.Aggregates;

public static class VoucherAggregateExtensions{
    public static Result AddBarcode(this VoucherAggregate aggregate, String barcodeAsBase64)
    {
        if (String.IsNullOrEmpty(barcodeAsBase64))
            return Result.Invalid("barcode must not ne empty");

        Result result = aggregate.CheckIfVoucherHasBeenGenerated();
        if (result.IsFailed)
            return result;
        result = aggregate.CheckIfVoucherAlreadyIssued();
        if (result.IsFailed)
            return result;

        VoucherDomainEvents.BarcodeAddedEvent barcodeAddedEvent = new VoucherDomainEvents.BarcodeAddedEvent(aggregate.AggregateId, aggregate.EstateId, barcodeAsBase64);

        aggregate.ApplyAndAppend(barcodeAddedEvent);

        return Result.Success();
    }

    public static Result Generate(this VoucherAggregate aggregate,
                        Guid operatorId,
                         Guid estateId,
                         Guid transactionId,
                         DateTime generatedDateTime,
                         Decimal value)
    {
        if (aggregate.IsGenerated)
            return Result.Success();

        if (generatedDateTime == DateTime.MinValue)
            return Result.Invalid("Generated Date Time must be set");
        if (operatorId == Guid.Empty)
            return Result.Invalid("Operator Id must not be an empty Guid");
        if (transactionId == Guid.Empty)
            return Result.Invalid("Transaction Id must not be an empty Guid");
        if (estateId == Guid.Empty)
            return Result.Invalid("Estate Id must not be an empty Guid");
        if (value <= 0)
            return Result.Invalid("Value must be greater than zero");

        // Do the generate process here...
        String voucherCode = aggregate.GenerateVoucherCode();
        DateTime expiryDateTime = generatedDateTime.AddDays(30); // Default to a 30 day expiry for now...
        String message = string.Empty;

        VoucherDomainEvents.VoucherGeneratedEvent voucherGeneratedEvent =
            new VoucherDomainEvents.VoucherGeneratedEvent(aggregate.AggregateId, estateId, transactionId, generatedDateTime, operatorId, value, voucherCode, expiryDateTime, message);

        aggregate.ApplyAndAppend(voucherGeneratedEvent);

        return Result.Success();
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

    public static Result Issue(this VoucherAggregate aggregate,
                             String recipientEmail,
                             String recipientMobile,
                             DateTime issuedDateTime)
    {
        if (aggregate.IsIssued)
            return Result.Success();

        Result result = aggregate.CheckIfVoucherHasBeenGenerated();
        if (result.IsFailed)
            return result;

        if (String.IsNullOrEmpty(recipientEmail) && String.IsNullOrEmpty(recipientMobile)) {
            return Result.Invalid("Either Recipient Email Address or Recipient Mobile number must be set to issue a voucher");
        }

        VoucherDomainEvents.VoucherIssuedEvent voucherIssuedEvent = new VoucherDomainEvents.VoucherIssuedEvent(aggregate.AggregateId, aggregate.EstateId, issuedDateTime, recipientEmail, recipientMobile);

        aggregate.ApplyAndAppend(voucherIssuedEvent);

        return Result.Success();
    }

    public static Result Redeem(this VoucherAggregate aggregate, DateTime redeemedDateTime)
    {
        Result result = aggregate.CheckIfVoucherHasBeenGenerated();
        if (result.IsFailed)
            return result;
        result = aggregate.CheckIfVoucherHasBeenIssued();
        if (result.IsFailed)
            return result;
        result = aggregate.CheckIfVoucherAlreadyRedeemed();
        if (result.IsFailed)
            return result;

        VoucherDomainEvents.VoucherFullyRedeemedEvent voucherFullyRedeemedEvent = new(aggregate.AggregateId, aggregate.EstateId, redeemedDateTime);

        aggregate.ApplyAndAppend(voucherFullyRedeemedEvent);

        return Result.Success();
    }

    private static Result CheckIfVoucherAlreadyIssued(this VoucherAggregate aggregate) {
        if (aggregate.IsIssued) {
            return Result.Invalid($"Voucher Id [{aggregate.AggregateId}] has already been issued");
        }
        return Result.Success();
    }

    private static Result CheckIfVoucherAlreadyRedeemed(this VoucherAggregate aggregate)
    {
        if (aggregate.IsRedeemed)
        {
            return Result.Invalid($"Voucher Id [{aggregate.AggregateId}] has already been redeemed");
        }
        return Result.Success();
    }

    private static Result CheckIfVoucherHasBeenGenerated(this VoucherAggregate aggregate) {
        if (aggregate.IsGenerated == false) {
            return Result.Invalid($"Voucher Id [{aggregate.AggregateId}] has not been generated");
        }
        return Result.Success();
    }

    private static Result CheckIfVoucherHasBeenIssued(this VoucherAggregate aggregate) {
        if (aggregate.IsIssued == false) {
            return Result.Invalid($"Voucher Id [{aggregate.AggregateId}] has not been issued");
        }
        return Result.Success();
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
    
    public static void PlayEvent(this VoucherAggregate aggregate, VoucherDomainEvents.VoucherGeneratedEvent domainEvent)
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
    
    public static void PlayEvent(this VoucherAggregate aggregate, VoucherDomainEvents.VoucherIssuedEvent domainEvent)
    {
        aggregate.IsIssued = true;
        aggregate.IssuedDateTime = domainEvent.IssuedDateTime;
        aggregate.RecipientEmail = domainEvent.RecipientEmail;
        aggregate.RecipientMobile = domainEvent.RecipientMobile;
        aggregate.Balance = aggregate.Value;
    }

    public static void PlayEvent(this VoucherAggregate aggregate, VoucherDomainEvents.BarcodeAddedEvent domainEvent)
    {
        aggregate.Barcode = domainEvent.Barcode;
    }

    public static void PlayEvent(this VoucherAggregate aggregate, VoucherDomainEvents.VoucherFullyRedeemedEvent domainEvent)
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