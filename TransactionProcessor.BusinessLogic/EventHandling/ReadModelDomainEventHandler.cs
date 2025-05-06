using System.Threading;
using System.Threading.Tasks;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.EventHandling;
using SimpleResults;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Repository;
using static TransactionProcessor.DomainEvents.FloatDomainEvents;

namespace TransactionProcessor.BusinessLogic.EventHandling
{
    public class ReadModelDomainEventHandler : IDomainEventHandler
    {
        private readonly ITransactionProcessorReadModelRepository EstateReportingRepository;

        public ReadModelDomainEventHandler(ITransactionProcessorReadModelRepository estateReportingRepository) {
            this.EstateReportingRepository = estateReportingRepository;
        }

        public async Task<Result> Handle(IDomainEvent domainEvent,
                                         CancellationToken cancellationToken) {
            Task<Result> task = domainEvent switch
            {
                EstateDomainEvents.EstateCreatedEvent de => this.HandleSpecificDomainEvent(de, cancellationToken),
                EstateDomainEvents.SecurityUserAddedToEstateEvent de => this.EstateReportingRepository.AddEstateSecurityUser(de, cancellationToken),
                EstateDomainEvents.EstateReferenceAllocatedEvent de => this.EstateReportingRepository.UpdateEstate(de, cancellationToken),
                
                OperatorDomainEvents.OperatorCreatedEvent de => this.EstateReportingRepository.AddOperator(de, cancellationToken),
                OperatorDomainEvents.OperatorNameUpdatedEvent de => this.EstateReportingRepository.UpdateOperator(de, cancellationToken),
                OperatorDomainEvents.OperatorRequireCustomMerchantNumberChangedEvent de => this.EstateReportingRepository.UpdateOperator(de, cancellationToken),
                OperatorDomainEvents.OperatorRequireCustomTerminalNumberChangedEvent de => this.EstateReportingRepository.UpdateOperator(de, cancellationToken),
                
                ContractDomainEvents.ContractCreatedEvent de => this.EstateReportingRepository.AddContract(de, cancellationToken),
                ContractDomainEvents.FixedValueProductAddedToContractEvent de => this.EstateReportingRepository.AddContractProduct(de, cancellationToken),
                ContractDomainEvents.VariableValueProductAddedToContractEvent de => this.EstateReportingRepository.AddContractProduct(de, cancellationToken),
                ContractDomainEvents.TransactionFeeForProductAddedToContractEvent de => this.EstateReportingRepository.AddContractProductTransactionFee(de, cancellationToken),
                ContractDomainEvents.TransactionFeeForProductDisabledEvent de => this.EstateReportingRepository.DisableContractProductTransactionFee(de, cancellationToken),

                MerchantDomainEvents.MerchantCreatedEvent de => this.EstateReportingRepository.AddMerchant(de, cancellationToken),
                MerchantDomainEvents.MerchantReferenceAllocatedEvent de => this.EstateReportingRepository.UpdateMerchant(de, cancellationToken),
                MerchantDomainEvents.AddressAddedEvent de => this.EstateReportingRepository.AddMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.ContactAddedEvent de => this.EstateReportingRepository.AddMerchantContact(de, cancellationToken),
                MerchantDomainEvents.SecurityUserAddedToMerchantEvent de => this.EstateReportingRepository.AddMerchantSecurityUser(de, cancellationToken),
                MerchantDomainEvents.DeviceAddedToMerchantEvent de => this.EstateReportingRepository.AddMerchantDevice(de, cancellationToken),
                MerchantDomainEvents.DeviceSwappedForMerchantEvent de => this.EstateReportingRepository.SwapMerchantDevice(de, cancellationToken),
                MerchantDomainEvents.OperatorAssignedToMerchantEvent de => this.EstateReportingRepository.AddMerchantOperator(de, cancellationToken),
                MerchantDomainEvents.OperatorRemovedFromMerchantEvent de => this.EstateReportingRepository.RemoveOperatorFromMerchant(de, cancellationToken),
                MerchantDomainEvents.SettlementScheduleChangedEvent de => this.EstateReportingRepository.UpdateMerchant(de, cancellationToken),
                MerchantDomainEvents.ContractAddedToMerchantEvent de => this.EstateReportingRepository.AddContractToMerchant(de, cancellationToken),
                MerchantDomainEvents.ContractRemovedFromMerchantEvent de => this.EstateReportingRepository.RemoveContractFromMerchant(de, cancellationToken),
                MerchantDomainEvents.MerchantNameUpdatedEvent de => this.EstateReportingRepository.UpdateMerchant(de, cancellationToken),
                MerchantDomainEvents.MerchantAddressLine1UpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantAddressLine2UpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantAddressLine3UpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantAddressLine4UpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantCountyUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantRegionUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantTownUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantPostalCodeUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantContactNameUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantContact(de, cancellationToken),
                MerchantDomainEvents.MerchantContactEmailAddressUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantContact(de, cancellationToken),
                MerchantDomainEvents.MerchantContactPhoneNumberUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantContact(de, cancellationToken),
                
                TransactionDomainEvents.TransactionHasStartedEvent de => this.EstateReportingRepository.StartTransaction(de, cancellationToken),
                TransactionDomainEvents.AdditionalRequestDataRecordedEvent de => this.HandleSpecificDomainEvent(de, cancellationToken),
                TransactionDomainEvents.AdditionalResponseDataRecordedEvent de => this.EstateReportingRepository.RecordTransactionAdditionalResponseData(de, cancellationToken),
                TransactionDomainEvents.TransactionHasBeenLocallyAuthorisedEvent de => this.EstateReportingRepository.UpdateTransactionAuthorisation(de, cancellationToken),
                TransactionDomainEvents.TransactionHasBeenLocallyDeclinedEvent de => this.EstateReportingRepository.UpdateTransactionAuthorisation(de, cancellationToken),
                TransactionDomainEvents.TransactionAuthorisedByOperatorEvent de => this.EstateReportingRepository.UpdateTransactionAuthorisation(de, cancellationToken),
                TransactionDomainEvents.TransactionDeclinedByOperatorEvent de => this.EstateReportingRepository.UpdateTransactionAuthorisation(de, cancellationToken),
                TransactionDomainEvents.TransactionSourceAddedToTransactionEvent de => this.EstateReportingRepository.AddSourceDetailsToTransaction(de, cancellationToken),
                TransactionDomainEvents.ProductDetailsAddedToTransactionEvent de => this.EstateReportingRepository.AddProductDetailsToTransaction(de, cancellationToken),
                TransactionDomainEvents.TransactionHasBeenCompletedEvent de => this.EstateReportingRepository.CompleteTransaction(de, cancellationToken),
                TransactionDomainEvents.TransactionTimingsAddedToTransactionEvent de => this.EstateReportingRepository.RecordTransactionTimings(de, cancellationToken),
                ReconciliationDomainEvents.ReconciliationHasStartedEvent de => this.EstateReportingRepository.StartReconciliation(de, cancellationToken),
                ReconciliationDomainEvents.OverallTotalsRecordedEvent de => this.EstateReportingRepository.UpdateReconciliationOverallTotals(de, cancellationToken),
                ReconciliationDomainEvents.ReconciliationHasBeenLocallyAuthorisedEvent de => this.EstateReportingRepository.UpdateReconciliationStatus(de, cancellationToken),
                ReconciliationDomainEvents.ReconciliationHasBeenLocallyDeclinedEvent de => this.EstateReportingRepository.UpdateReconciliationStatus(de, cancellationToken),
                ReconciliationDomainEvents.ReconciliationHasCompletedEvent de => this.EstateReportingRepository.CompleteReconciliation(de, cancellationToken),
                //VoucherDomainEvents.VoucherGeneratedEvent de => this.EstateReportingRepository.AddGeneratedVoucher(de, cancellationToken),
                //VoucherDomainEvents.VoucherIssuedEvent de => this.EstateReportingRepository.UpdateVoucherIssueDetails(de, cancellationToken),
                //VoucherDomainEvents.VoucherFullyRedeemedEvent de => this.EstateReportingRepository.UpdateVoucherRedemptionDetails(de, cancellationToken),

                FileProcessor.FileImportLog.DomainEvents.ImportLogCreatedEvent de => this.EstateReportingRepository.AddFileImportLog(de, cancellationToken),
                FileProcessor.FileImportLog.DomainEvents.FileAddedToImportLogEvent de => this.EstateReportingRepository.AddFileToImportLog(de, cancellationToken),

                FileProcessor.File.DomainEvents.FileCreatedEvent de => this.EstateReportingRepository.AddFile(de, cancellationToken),
                FileProcessor.File.DomainEvents.FileLineAddedEvent de => this.EstateReportingRepository.AddFileLineToFile(de, cancellationToken),
                FileProcessor.File.DomainEvents.FileLineProcessingSuccessfulEvent de => this.EstateReportingRepository.UpdateFileLine(de, cancellationToken),
                FileProcessor.File.DomainEvents.FileLineProcessingFailedEvent de => this.EstateReportingRepository.UpdateFileLine(de, cancellationToken),
                FileProcessor.File.DomainEvents.FileLineProcessingIgnoredEvent de => this.EstateReportingRepository.UpdateFileLine(de, cancellationToken),
                FileProcessor.File.DomainEvents.FileProcessingCompletedEvent de => this.EstateReportingRepository.UpdateFileAsComplete(de, cancellationToken),

                MerchantStatementDomainEvents.StatementCreatedEvent de => this.EstateReportingRepository.CreateStatement(de, cancellationToken),
                // TODO@ Add this back in
                //MerchantStatementDomainEvents.TransactionAddedToStatementEvent de => this.EstateReportingRepository.AddTransactionToStatement(de, cancellationToken),
                // TODO@ Add this back in
                //MerchantStatementDomainEvents.SettledFeeAddedToStatementEvent de => this.EstateReportingRepository.AddSettledFeeToStatement(de, cancellationToken),
                MerchantStatementDomainEvents.StatementGeneratedEvent de => this.HandleSpecificDomainEvent(de, cancellationToken),

                FloatCreatedForContractProductEvent de => this.EstateReportingRepository.CreateFloat(de, cancellationToken),
                FloatCreditPurchasedEvent de => this.EstateReportingRepository.CreateFloatActivity(de, cancellationToken),
                FloatDecreasedByTransactionEvent de => this.EstateReportingRepository.CreateFloatActivity(de, cancellationToken),
                
                SettlementDomainEvents.SettlementCreatedForDateEvent de => this.EstateReportingRepository.CreateSettlement(de, cancellationToken),
                SettlementDomainEvents.SettlementProcessingStartedEvent de => this.EstateReportingRepository.MarkSettlementAsProcessingStarted(de, cancellationToken),
                SettlementDomainEvents.MerchantFeeAddedPendingSettlementEvent de => this.EstateReportingRepository.AddPendingMerchantFeeToSettlement(de, cancellationToken),
                TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent de => this.EstateReportingRepository.AddSettledMerchantFeeToSettlement(de, cancellationToken),
                SettlementDomainEvents.MerchantFeeSettledEvent de => this.EstateReportingRepository.MarkMerchantFeeAsSettled(de, cancellationToken),
                SettlementDomainEvents.SettlementCompletedEvent de => this.EstateReportingRepository.MarkSettlementAsCompleted(de, cancellationToken),

                _ => Task.FromResult(Result.Success())
            };

            return await task;
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionDomainEvents.AdditionalRequestDataRecordedEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            var result = await this.EstateReportingRepository.RecordTransactionAdditionalRequestData(domainEvent, cancellationToken);
            if (result.IsFailed)
                return result;
            return await this.EstateReportingRepository.SetTransactionAmount(domainEvent, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(EstateDomainEvents.EstateCreatedEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            Result createResult = await this.EstateReportingRepository.CreateReadModel(domainEvent, cancellationToken);
            if (createResult.IsFailed)
                return createResult;

            return await this.EstateReportingRepository.AddEstate(domainEvent, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(MerchantStatementDomainEvents.StatementGeneratedEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            Result createResult = await this.EstateReportingRepository.MarkStatementAsGenerated(domainEvent, cancellationToken);
            if (createResult.IsFailed)
                return createResult;

            return await this.EstateReportingRepository.UpdateMerchant(domainEvent, cancellationToken);
        }
    }
}
