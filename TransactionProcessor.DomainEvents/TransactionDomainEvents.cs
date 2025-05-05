using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents;

[ExcludeFromCodeCoverage]
public class TransactionDomainEvents {
    public record CustomerEmailReceiptResendRequestedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record CustomerEmailReceiptRequestedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, String CustomerEmailAddress, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record AdditionalRequestDataRecordedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Guid OperatorId, Dictionary<String, String> AdditionalTransactionRequestMetadata, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record AdditionalResponseDataRecordedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Guid OperatorId, Dictionary<String, String> AdditionalTransactionResponseMetadata, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record SettledMerchantFeeAddedToTransactionEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Decimal CalculatedValue, Int32 FeeCalculationType, Guid FeeId, Decimal FeeValue, DateTime FeeCalculatedDateTime, DateTime SettledDateTime, Guid SettlementId, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record MerchantFeePendingSettlementAddedToTransactionEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Decimal CalculatedValue, Int32 FeeCalculationType, Guid FeeId, Decimal FeeValue, DateTime FeeCalculatedDateTime, DateTime SettlementDueDate, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record ProductDetailsAddedToTransactionEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Guid ContractId, Guid ProductId, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record ServiceProviderFeeAddedToTransactionEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Decimal CalculatedValue, Int32 FeeCalculationType, Guid FeeId, Decimal FeeValue, DateTime FeeCalculatedDateTime, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record TransactionAuthorisedByOperatorEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Guid OperatorId, String AuthorisationCode, String OperatorResponseCode, String OperatorResponseMessage, String OperatorTransactionId, String ResponseCode, String ResponseMessage, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record TransactionCostInformationRecordedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Decimal? UnitCostValue, Decimal? TotalCostValue, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record TransactionDeclinedByOperatorEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Guid OperatorId, String OperatorResponseCode, String OperatorResponseMessage, String ResponseCode, String ResponseMessage, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record TransactionHasBeenCompletedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, String ResponseCode, String ResponseMessage, Boolean IsAuthorised, DateTime CompletedDateTime, Decimal? TransactionAmount, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record TransactionHasBeenLocallyAuthorisedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, String AuthorisationCode, String ResponseCode, String ResponseMessage, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record TransactionHasBeenLocallyDeclinedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, String ResponseCode, String ResponseMessage, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record TransactionHasStartedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, DateTime TransactionDateTime, String TransactionNumber, String TransactionType, String TransactionReference, String DeviceIdentifier, Decimal? TransactionAmount) : DomainEvent(TransactionId, Guid.NewGuid());
    public record TransactionSourceAddedToTransactionEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Int32 TransactionSource, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());

    public record TransactionTimingsAddedToTransactionEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, DateTime TransactionStartedDateTime, DateTime? OperatorCommunicationsStartedEvent,
                                                            DateTime? OperatorCommunicationsCompletedEvent, DateTime TransactionCompletedDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
}