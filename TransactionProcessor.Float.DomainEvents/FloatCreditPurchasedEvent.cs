namespace TransactionProcessor.Float.DomainEvents{
    using Shared.DomainDrivenDesign.EventSourcing;

    public record FloatCreditPurchasedEvent(Guid FloatId, Guid EstateId, DateTime CreditPurchasedDateTime,
                                            Decimal Amount, Decimal CostPrice) : DomainEvent(FloatId,Guid.NewGuid());
}