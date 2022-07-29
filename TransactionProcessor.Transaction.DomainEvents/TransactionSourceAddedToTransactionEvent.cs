namespace TransactionProcessor.Transaction.DomainEvents;

using System;
using Shared.DomainDrivenDesign.EventSourcing;

public record TransactionSourceAddedToTransactionEvent : DomainEvent
{
    #region Constructors

    public TransactionSourceAddedToTransactionEvent(Guid aggregateId,
                                                    Guid estateId,
                                                    Guid merchantId,
                                                    Int32 transactionSource) : base(aggregateId, Guid.NewGuid())
    {
        this.TransactionId = aggregateId;
        this.EstateId = estateId;
        this.MerchantId = merchantId;
        this.TransactionSource = transactionSource;
    }

    #endregion

    #region Properties

    public Guid EstateId { get; init; }

    public Guid MerchantId { get; init; }

    public Int32 TransactionSource { get; init; }

    public Guid TransactionId { get; init; }

    #endregion
}