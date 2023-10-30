namespace TransactionProcessor.Float.DomainEvents;

using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

[ExcludeFromCodeCoverage]
public record FloatCreatedForContractProductEvent(Guid FloatId,
                                                  Guid EstateId,
                                                  Guid ContractId,
                                                  Guid ProductId,
                                                  DateTime CreatedDateTime)
    : DomainEvent(FloatId, Guid.NewGuid());

[ExcludeFromCodeCoverage]
public record FloatCreditPurchasedEvent(Guid FloatId,
                                        Guid EstateId,
                                        DateTime CreditPurchasedDateTime,
                                        Decimal Amount,
                                        Decimal CostPrice) : DomainEvent(FloatId, Guid.NewGuid());

[ExcludeFromCodeCoverage]
public record FloatDecreasedByTransactionEvent(Guid FloatId, Guid EstateId, Guid TransactionId, Decimal Amount) : DomainEvent(FloatId, Guid.NewGuid());