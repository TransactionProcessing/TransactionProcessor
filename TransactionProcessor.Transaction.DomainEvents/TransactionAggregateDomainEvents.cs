namespace TransactionProcessor.Transaction.DomainEvents{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Shared.DomainDrivenDesign.EventSourcing;

    [ExcludeFromCodeCoverage]
    public record CustomerEmailReceiptResendRequestedEvent(Guid TransactionId,
                                                           Guid EstateId,
                                                           Guid MerchantId) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record CustomerEmailReceiptRequestedEvent(Guid TransactionId,
                                                     Guid EstateId,
                                                     Guid MerchantId,
                                                     String CustomerEmailAddress) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record AdditionalRequestDataRecordedEvent(Guid TransactionId,
                                                     Guid EstateId,
                                                     Guid MerchantId,
                                                     Guid OperatorId,
                                                     Dictionary<String, String> AdditionalTransactionRequestMetadata) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record AdditionalResponseDataRecordedEvent(Guid TransactionId,
                                                      Guid EstateId,
                                                      Guid MerchantId,
                                                      Guid OperatorId,
                                                      Dictionary<String, String> AdditionalTransactionResponseMetadata) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record SettledMerchantFeeAddedToTransactionEvent(Guid TransactionId,
                                                            Guid EstateId,
                                                            Guid MerchantId,
                                                            Decimal CalculatedValue,
                                                            Int32 FeeCalculationType,
                                                            Guid FeeId,
                                                            Decimal FeeValue,
                                                            DateTime FeeCalculatedDateTime,
                                                            DateTime SettledDateTime,
                                                            Guid SettlementId) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record MerchantFeePendingSettlementAddedToTransactionEvent(Guid TransactionId,
                                                                      Guid EstateId,
                                                                      Guid MerchantId,
                                                                      Decimal CalculatedValue,
                                                                      Int32 FeeCalculationType,
                                                                      Guid FeeId,
                                                                      Decimal FeeValue,
                                                                      DateTime FeeCalculatedDateTime,
                                                                      DateTime SettlementDueDate) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record ProductDetailsAddedToTransactionEvent(Guid TransactionId,
                                                        Guid EstateId,
                                                        Guid MerchantId,
                                                        Guid ContractId,
                                                        Guid ProductId) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record ServiceProviderFeeAddedToTransactionEvent(Guid TransactionId,
                                                            Guid EstateId,
                                                            Guid MerchantId,
                                                            Decimal CalculatedValue,
                                                            Int32 FeeCalculationType,
                                                            Guid FeeId,
                                                            Decimal FeeValue,
                                                            DateTime FeeCalculatedDateTime) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record TransactionAuthorisedByOperatorEvent(Guid TransactionId,
                                                       Guid EstateId,
                                                       Guid MerchantId,
                                                       Guid OperatorId,
                                                       String AuthorisationCode,
                                                       String OperatorResponseCode,
                                                       String OperatorResponseMessage,
                                                       String OperatorTransactionId,
                                                       String ResponseCode,
                                                       String ResponseMessage) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record TransactionCostInformationRecordedEvent(Guid TransactionId,
                                                          Guid EstateId,
                                                          Guid MerchantId,
                                                          Decimal? UnitCostValue,
                                                          Decimal? TotalCostValue) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record TransactionDeclinedByOperatorEvent(Guid TransactionId,
                                                     Guid EstateId,
                                                     Guid MerchantId,
                                                     Guid OperatorId,
                                                     String OperatorResponseCode,
                                                     String OperatorResponseMessage,
                                                     String ResponseCode,
                                                     String ResponseMessage) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record TransactionHasBeenCompletedEvent(Guid TransactionId,
                                                   Guid EstateId,
                                                   Guid MerchantId,
                                                   String ResponseCode,
                                                   String ResponseMessage,
                                                   Boolean IsAuthorised,
                                                   DateTime CompletedDateTime,
                                                   Decimal? TransactionAmount) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record TransactionHasBeenLocallyAuthorisedEvent(Guid TransactionId,
                                                           Guid EstateId,
                                                           Guid MerchantId,
                                                           String AuthorisationCode,
                                                           String ResponseCode,
                                                           String ResponseMessage) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record TransactionHasBeenLocallyDeclinedEvent(Guid TransactionId,
                                                         Guid EstateId,
                                                         Guid MerchantId,
                                                         String ResponseCode,
                                                         String ResponseMessage) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record TransactionHasStartedEvent(Guid TransactionId,
                                             Guid EstateId,
                                             Guid MerchantId,
                                             DateTime TransactionDateTime,
                                             String TransactionNumber,
                                             String TransactionType,
                                             String TransactionReference,
                                             String DeviceIdentifier,
                                             Decimal? TransactionAmount) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record TransactionSourceAddedToTransactionEvent(Guid TransactionId,
                                                           Guid EstateId,
                                                           Guid MerchantId,
                                                           Int32 TransactionSource) : DomainEvent(TransactionId, Guid.NewGuid());
}