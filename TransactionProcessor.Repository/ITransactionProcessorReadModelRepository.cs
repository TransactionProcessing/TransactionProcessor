using Shared.DomainDrivenDesign.EventSourcing;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.EntityFramework;
using Shared.Logger;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Estate.DomainEvents;
using TransactionProcessor.Float.DomainEvents;
using TransactionProcessor.Reconciliation.DomainEvents;
using TransactionProcessor.Settlement.DomainEvents;
using TransactionProcessor.Transaction.DomainEvents;
using TransactionProcessor.Voucher.DomainEvents;
using TransactionProcessor.Database.Entities;
using Shared.Results;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TransactionProcessor.Operator.DomainEvents;

namespace TransactionProcessor.Repository {
    public interface ITransactionProcessorReadModelRepository {

        Task<Result> UpdateOperator(OperatorNameUpdatedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateOperator(OperatorRequireCustomMerchantNumberChangedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateOperator(OperatorRequireCustomTerminalNumberChangedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> AddOperator(OperatorCreatedEvent domainEvent,
                                 CancellationToken cancellationToken);

        //Task<Result> AddContract(ContractCreatedEvent domainEvent,
        //                         CancellationToken cancellationToken);

        //Task<Result> AddContractProduct(VariableValueProductAddedToContractEvent domainEvent,
        //                                CancellationToken cancellationToken);

        //Task<Result> AddContractProduct(FixedValueProductAddedToContractEvent domainEvent,
        //                                CancellationToken cancellationToken);

        //Task<Result> AddContractProductTransactionFee(TransactionFeeForProductAddedToContractEvent domainEvent,
        //                                              CancellationToken cancellationToken);

        //Task<Result> AddContractToMerchant(ContractAddedToMerchantEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        Task<Result> AddEstate(EstateCreatedEvent domainEvent,
                               CancellationToken cancellationToken);

        Task<Result> AddEstateSecurityUser(SecurityUserAddedToEstateEvent domainEvent,
                                           CancellationToken cancellationToken);

        //Task<Result> AddFile(FileCreatedEvent domainEvent,
        //                     CancellationToken cancellationToken);

        //Task<Result> AddFileImportLog(ImportLogCreatedEvent domainEvent,
        //                              CancellationToken cancellationToken);

        //Task<Result> AddFileLineToFile(FileLineAddedEvent domainEvent,
        //                               CancellationToken cancellationToken);

        //Task<Result> AddFileToImportLog(FileAddedToImportLogEvent domainEvent,
        //                                CancellationToken cancellationToken);

        //Task<Result> AddGeneratedVoucher(VoucherGeneratedEvent domainEvent,
        //                                 CancellationToken cancellationToken);

        //Task<Result> AddMerchant(MerchantCreatedEvent domainEvent,
        //                         CancellationToken cancellationToken);

        //Task<Result> UpdateMerchant(MerchantNameUpdatedEvent domainEvent,
        //                            CancellationToken cancellationToken);

        //Task<Result> AddMerchantAddress(AddressAddedEvent domainEvent,
        //                                CancellationToken cancellationToken);

        //Task<Result> AddMerchantContact(ContactAddedEvent domainEvent,
        //                                CancellationToken cancellationToken);

        //Task<Result> AddMerchantDevice(DeviceAddedToMerchantEvent domainEvent,
        //                               CancellationToken cancellationToken);

        //Task<Result> SwapMerchantDevice(DeviceSwappedForMerchantEvent domainEvent,
        //                                CancellationToken cancellationToken);

        //Task<Result> AddMerchantOperator(OperatorAssignedToMerchantEvent domainEvent,
        //                                 CancellationToken cancellationToken);

        //Task<Result> AddMerchantSecurityUser(SecurityUserAddedToMerchantEvent domainEvent,
        //                                     CancellationToken cancellationToken);

        //Task<Result> AddPendingMerchantFeeToSettlement(MerchantFeeAddedPendingSettlementEvent domainEvent,
        //                                               CancellationToken cancellationToken);

        Task<Result> AddProductDetailsToTransaction(ProductDetailsAddedToTransactionEvent domainEvent,
                                                    CancellationToken cancellationToken);

        //Task<Result> AddSettledFeeToStatement(SettledFeeAddedToStatementEvent domainEvent,
        //                                      CancellationToken cancellationToken);

        Task<Result> AddSettledMerchantFeeToSettlement(SettledMerchantFeeAddedToTransactionEvent domainEvent,
                                                       CancellationToken cancellationToken);

        Task<Result> AddSourceDetailsToTransaction(TransactionSourceAddedToTransactionEvent domainEvent,
                                                   CancellationToken cancellationToken);

        //Task<Result> AddTransactionToStatement(TransactionAddedToStatementEvent domainEvent,
        //                                       CancellationToken cancellationToken);

        Task<Result> CompleteReconciliation(ReconciliationHasCompletedEvent domainEvent,
                                            CancellationToken cancellationToken);

        Task<Result> CompleteTransaction(TransactionHasBeenCompletedEvent domainEvent,
                                         CancellationToken cancellationToken);

        Task<Result> CreateFloat(FloatCreatedForContractProductEvent domainEvent,
                                 CancellationToken cancellationToken);

        Task<Result> CreateFloatActivity(FloatCreditPurchasedEvent domainEvent,
                                         CancellationToken cancellationToken);

        Task<Result> CreateFloatActivity(FloatDecreasedByTransactionEvent domainEvent,
                                         CancellationToken cancellationToken);

        Task<Result> CreateReadModel(EstateCreatedEvent domainEvent,
                                     CancellationToken cancellationToken);

        Task<Result> CreateSettlement(SettlementCreatedForDateEvent domainEvent,
                                      CancellationToken cancellationToken);

        //Task<Result> CreateStatement(StatementCreatedEvent domainEvent,
        //                             CancellationToken cancellationToken);

        //Task<Result> DisableContractProductTransactionFee(TransactionFeeForProductDisabledEvent domainEvent,
        //                                                  CancellationToken cancellationToken);

        Task<Result> MarkMerchantFeeAsSettled(MerchantFeeSettledEvent domainEvent,
                                              CancellationToken cancellationToken);

        Task<Result> MarkSettlementAsCompleted(SettlementCompletedEvent domainEvent,
                                               CancellationToken cancellationToken);

        Task<Result> MarkSettlementAsProcessingStarted(SettlementProcessingStartedEvent domainEvent,
                                                       CancellationToken cancellationToken);

        //Task<Result> MarkStatementAsGenerated(StatementGeneratedEvent domainEvent,
        //                                      CancellationToken cancellationToken);

        Task<Result> RecordTransactionAdditionalRequestData(AdditionalRequestDataRecordedEvent domainEvent,
                                                            CancellationToken cancellationToken);

        Task<Result> RecordTransactionAdditionalResponseData(AdditionalResponseDataRecordedEvent domainEvent,
                                                             CancellationToken cancellationToken);

        Task<Result> SetTransactionAmount(AdditionalRequestDataRecordedEvent domainEvent,
                                          CancellationToken cancellationToken);

        Task<Result> StartReconciliation(ReconciliationHasStartedEvent domainEvent,
                                         CancellationToken cancellationToken);

        Task<Result> StartTransaction(TransactionHasStartedEvent domainEvent,
                                      CancellationToken cancellationToken);

        Task<Result> UpdateEstate(EstateReferenceAllocatedEvent domainEvent,
                                  CancellationToken cancellationToken);

        //Task<Result> UpdateFileAsComplete(FileProcessingCompletedEvent domainEvent,
        //                                  CancellationToken cancellationToken);

        //Task<Result> UpdateFileLine(FileLineProcessingSuccessfulEvent domainEvent,
        //                            CancellationToken cancellationToken);

        //Task<Result> UpdateFileLine(FileLineProcessingFailedEvent domainEvent,
        //                            CancellationToken cancellationToken);

        //Task<Result> UpdateFileLine(FileLineProcessingIgnoredEvent domainEvent,
        //                            CancellationToken cancellationToken);

        //Task<Result> UpdateMerchant(MerchantReferenceAllocatedEvent domainEvent,
        //                            CancellationToken cancellationToken);

        //Task<Result> UpdateMerchant(StatementGeneratedEvent domainEvent,
        //                            CancellationToken cancellationToken);

        //Task<Result> UpdateMerchant(SettlementScheduleChangedEvent domainEvent,
        //                            CancellationToken cancellationToken);

        Task<Result> UpdateMerchant(TransactionHasBeenCompletedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateReconciliationOverallTotals(OverallTotalsRecordedEvent domainEvent,
                                                       CancellationToken cancellationToken);

        Task<Result> UpdateReconciliationStatus(ReconciliationHasBeenLocallyAuthorisedEvent domainEvent,
                                                CancellationToken cancellationToken);

        Task<Result> UpdateReconciliationStatus(ReconciliationHasBeenLocallyDeclinedEvent domainEvent,
                                                CancellationToken cancellationToken);

        Task<Result> UpdateTransactionAuthorisation(TransactionHasBeenLocallyAuthorisedEvent domainEvent,
                                                    CancellationToken cancellationToken);

        Task<Result> UpdateTransactionAuthorisation(TransactionHasBeenLocallyDeclinedEvent domainEvent,
                                                    CancellationToken cancellationToken);

        Task<Result> UpdateTransactionAuthorisation(TransactionAuthorisedByOperatorEvent domainEvent,
                                                    CancellationToken cancellationToken);

        Task<Result> UpdateTransactionAuthorisation(TransactionDeclinedByOperatorEvent domainEvent,
                                                    CancellationToken cancellationToken);

        Task<Result> UpdateVoucherIssueDetails(VoucherIssuedEvent domainEvent,
                                               CancellationToken cancellationToken);

        Task<Result> UpdateVoucherRedemptionDetails(VoucherFullyRedeemedEvent domainEvent,
                                                    CancellationToken cancellationToken);

        //Task<Result> RemoveOperatorFromMerchant(OperatorRemovedFromMerchantEvent domainEvent,
        //                                        CancellationToken cancellationToken);

        //Task<Result> RemoveContractFromMerchant(ContractRemovedFromMerchantEvent domainEvent,
        //                                        CancellationToken cancellationToken);

        //Task<Result> UpdateMerchantAddress(MerchantAddressLine1UpdatedEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        //Task<Result> UpdateMerchantAddress(MerchantAddressLine2UpdatedEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        //Task<Result> UpdateMerchantAddress(MerchantAddressLine3UpdatedEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        //Task<Result> UpdateMerchantAddress(MerchantAddressLine4UpdatedEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        //Task<Result> UpdateMerchantAddress(MerchantCountyUpdatedEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        //Task<Result> UpdateMerchantAddress(MerchantRegionUpdatedEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        //Task<Result> UpdateMerchantAddress(MerchantTownUpdatedEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        //Task<Result> UpdateMerchantAddress(MerchantPostalCodeUpdatedEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        //Task<Result> UpdateMerchantContact(MerchantContactNameUpdatedEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        //Task<Result> UpdateMerchantContact(MerchantContactEmailAddressUpdatedEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        //Task<Result> UpdateMerchantContact(MerchantContactPhoneNumberUpdatedEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        Task<Result<Models.Estate.Estate>> GetEstate(Guid estateId,
                                              CancellationToken cancellationToken);

        Task<Result<List<Models.Operator.Operator>>> GetOperators(Guid estateId,
                                                                  CancellationToken cancellationToken);
    }

    [ExcludeFromCodeCoverage]
    public class TransactionProcessorReadModelRepository : ITransactionProcessorReadModelRepository {
        private readonly Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext> DbContextFactory;

        private const String ConnectionStringIdentifier = "EstateReportingReadModel";

        public TransactionProcessorReadModelRepository(Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext> dbContextFactory) {
   this.DbContextFactory = dbContextFactory;
        
        }

        public async Task<Result<Models.Estate.Estate>> GetEstate(Guid estateId,
                                                           CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.DbContextFactory.GetContext(estateId, ConnectionStringIdentifier, cancellationToken);

            Database.Entities.Estate? estate = await context.Estates.SingleOrDefaultAsync(e => e.EstateId == estateId, cancellationToken);

            if (estate == null)
            {
                return Result.NotFound($"No estate found in read model with Id [{estateId}]");
            }

            List<EstateSecurityUser> estateSecurityUsers = await context.EstateSecurityUsers.Where(esu => esu.EstateId == estate.EstateId).ToListAsync(cancellationToken);
            List<Database.Entities.Operator> operators = await context.Operators.Where(eo => eo.EstateId == estate.EstateId).ToListAsync(cancellationToken);

            return Result.Success(ModelFactory.ConvertFrom(estate, estateSecurityUsers, operators));
        }

        private async Task<EstateManagementGenericContext> GetContextFromDomainEvent(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            Guid estateId = Database.Contexts.DomainEventHelper.GetEstateId(domainEvent);
            if (estateId == Guid.Empty)
            {
                throw new Exception($"Unable to resolve context for Domain Event {domainEvent.GetType()}");
            }

            return await this.DbContextFactory.GetContext(estateId, ConnectionStringIdentifier, cancellationToken);
        }

        public async Task<Result> AddEstate(EstateCreatedEvent domainEvent,
                                            CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            // Add the estate to the read model
            Database.Entities.Estate estate = new Database.Entities.Estate
            {
                EstateId = domainEvent.EstateId,
                Name = domainEvent.EstateName,
                Reference = String.Empty
            };
            await context.Estates.AddAsync(estate, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddEstateSecurityUser(SecurityUserAddedToEstateEvent domainEvent,
                                                        CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            EstateSecurityUser estateSecurityUser = new EstateSecurityUser
            {
                EstateId = domainEvent.EstateId,
                EmailAddress = domainEvent.EmailAddress,
                SecurityUserId = domainEvent.SecurityUserId,
                CreatedDateTime = domainEvent.EventTimestamp.DateTime
            };

            await context.EstateSecurityUsers.AddAsync(estateSecurityUser, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddProductDetailsToTransaction(ProductDetailsAddedToTransactionEvent domainEvent,
                                                                 CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var getContractResult = await context.LoadContract(domainEvent, cancellationToken);
            if (getContractResult.IsFailed)
                return ResultHelpers.CreateFailure(getContractResult);

            var transaction = getTransactionResult.Data;
            var contract = getContractResult.Data;

            transaction.ContractId = domainEvent.ContractId;
            transaction.ContractProductId = domainEvent.ProductId;
            transaction.OperatorId = contract.OperatorId;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddSettledMerchantFeeToSettlement(SettledMerchantFeeAddedToTransactionEvent domainEvent,
                                                                    CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            MerchantSettlementFee merchantSettlementFee = new MerchantSettlementFee
            {
                SettlementId = domainEvent.SettlementId,
                CalculatedValue = domainEvent.CalculatedValue,
                FeeCalculatedDateTime = domainEvent.FeeCalculatedDateTime,
                ContractProductTransactionFeeId = domainEvent.FeeId,
                FeeValue = domainEvent.FeeValue,
                IsSettled = true,
                MerchantId = domainEvent.MerchantId,
                TransactionId = domainEvent.TransactionId
            };
            await context.MerchantSettlementFees.AddAsync(merchantSettlementFee, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddSourceDetailsToTransaction(TransactionSourceAddedToTransactionEvent domainEvent,
                                                                CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            transaction.TransactionSource = domainEvent.TransactionSource;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CompleteReconciliation(ReconciliationHasCompletedEvent domainEvent,
                                                         CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getReconcilationResult = await context.LoadReconcilation(domainEvent, cancellationToken);
            if (getReconcilationResult.IsFailed)
                return ResultHelpers.CreateFailure(getReconcilationResult);
            var reconciliation = getReconcilationResult.Data;

            reconciliation.IsCompleted = true;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CompleteTransaction(TransactionHasBeenCompletedEvent domainEvent,
                                                      CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            transaction.IsCompleted = true;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CreateFloat(FloatCreatedForContractProductEvent domainEvent,
                                              CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Database.Entities.Float floatRecord = new Database.Entities.Float
            {
                CreatedDate = domainEvent.CreatedDateTime.Date,
                CreatedDateTime = domainEvent.CreatedDateTime,
                ContractId = domainEvent.ContractId,
                EstateId = domainEvent.EstateId,
                FloatId = domainEvent.FloatId,
                ProductId = domainEvent.ProductId
            };
            await context.Floats.AddAsync(floatRecord, cancellationToken);
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CreateFloatActivity(FloatCreditPurchasedEvent domainEvent,
                                                      CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            FloatActivity floatActivity = new FloatActivity
            {
                ActivityDate = domainEvent.CreditPurchasedDateTime.Date,
                ActivityDateTime = domainEvent.CreditPurchasedDateTime,
                Amount = domainEvent.Amount,
                CostPrice = domainEvent.CostPrice,
                CreditOrDebit = "C",
                EventId = domainEvent.EventId,
                FloatId = domainEvent.FloatId
            };
            await context.FloatActivity.AddAsync(floatActivity, cancellationToken);
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CreateFloatActivity(FloatDecreasedByTransactionEvent domainEvent,
                                                      CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            FloatActivity floatActivity = new FloatActivity
            {
                ActivityDate = transaction.TransactionDate,
                ActivityDateTime = transaction.TransactionDateTime,
                Amount = domainEvent.Amount,
                CostPrice = 0,
                CreditOrDebit = "D",
                EventId = domainEvent.EventId,
                FloatId = domainEvent.FloatId
            };
            await context.FloatActivity.AddAsync(floatActivity, cancellationToken);
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CreateReadModel(EstateCreatedEvent domainEvent,
                                                  CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Logger.LogInformation($"About to run migrations on Read Model database for estate [{domainEvent.EstateId}]");

            // Ensure the db is at the latest version
            await context.MigrateAsync(cancellationToken);

            Logger.LogWarning($"Read Model database for estate [{domainEvent.EstateId}] migrated to latest version");
            return Result.Success();
        }

        public async Task<Result> CreateSettlement(SettlementCreatedForDateEvent domainEvent,
                                                   CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Database.Entities.Settlement settlement = new Database.Entities.Settlement
            {
                EstateId = domainEvent.EstateId,
                MerchantId = domainEvent.MerchantId,
                IsCompleted = false,
                SettlementDate = domainEvent.SettlementDate.Date,
                SettlementId = domainEvent.SettlementId
            };

            await context.Settlements.AddAsync(settlement, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> MarkMerchantFeeAsSettled(MerchantFeeSettledEvent domainEvent,
                                                           CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            // TODO: LoadMerchantSettlementFee
            MerchantSettlementFee merchantFee = await context.MerchantSettlementFees.Where(m =>
                    m.MerchantId == domainEvent.MerchantId &&
                    m.TransactionId == domainEvent.TransactionId &&
                    m.SettlementId == domainEvent.SettlementId &&
                    m.ContractProductTransactionFeeId == domainEvent.FeeId)
                .SingleOrDefaultAsync(cancellationToken);

            if (merchantFee == null)
            {
                return Result.NotFound("Merchant Fee not found to update as settled");
            }

            merchantFee.IsSettled = true;
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> MarkSettlementAsCompleted(SettlementCompletedEvent domainEvent,
                                                            CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getSettlementResult = await context.LoadSettlement(domainEvent, cancellationToken);
            if (getSettlementResult.IsFailed)
                return ResultHelpers.CreateFailure(getSettlementResult);
            var settlement = getSettlementResult.Data;

            settlement.IsCompleted = true;
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> MarkSettlementAsProcessingStarted(SettlementProcessingStartedEvent domainEvent,
                                                                    CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getSettlementResult = await context.LoadSettlement(domainEvent, cancellationToken);
            if (getSettlementResult.IsFailed)
                return ResultHelpers.CreateFailure(getSettlementResult);
            var settlement = getSettlementResult.Data;
            settlement.ProcessingStarted = true;
            settlement.ProcessingStartedDateTIme = domainEvent.ProcessingStartedDateTime;

            return await context.SaveChangesAsync(cancellationToken);
        }

        private readonly List<String> AdditionalRequestFields = new List<String>{
            "Amount",
            "CustomerAccountNumber"
        };


        private readonly List<String> AdditionalResponseFields = new List<String>();

        public async Task<Result> RecordTransactionAdditionalRequestData(AdditionalRequestDataRecordedEvent domainEvent,
                                                                         CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            TransactionAdditionalRequestData additionalRequestData = new TransactionAdditionalRequestData
            {
                TransactionId = domainEvent.TransactionId
            };

            foreach (String additionalRequestField in this.AdditionalRequestFields)
            {
                Logger.LogDebug($"Field to look for [{additionalRequestField}]");
            }

            foreach (KeyValuePair<String, String> additionalRequestField in domainEvent.AdditionalTransactionRequestMetadata)
            {
                Logger.LogDebug($"Key: [{additionalRequestField.Key}] Value: [{additionalRequestField.Value}]");
            }

            foreach (String additionalRequestField in this.AdditionalRequestFields)
            {
                if (domainEvent.AdditionalTransactionRequestMetadata.Any(m => m.Key.ToLower() == additionalRequestField.ToLower()))
                {
                    Type dbTableType = additionalRequestData.GetType();
                    PropertyInfo propertyInfo = dbTableType.GetProperty(additionalRequestField);

                    if (propertyInfo != null)
                    {
                        String value = domainEvent.AdditionalTransactionRequestMetadata.Single(m => m.Key.ToLower() == additionalRequestField.ToLower()).Value;
                        propertyInfo.SetValue(additionalRequestData, value);
                    }
                    else
                    {
                        Logger.LogInformation("propertyInfo == null");
                    }
                }
            }

            await context.TransactionsAdditionalRequestData.AddAsync(additionalRequestData, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> RecordTransactionAdditionalResponseData(AdditionalResponseDataRecordedEvent domainEvent,
                                                                          CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            TransactionAdditionalResponseData additionalResponseData = new TransactionAdditionalResponseData
            {
                TransactionId = domainEvent.TransactionId
            };

            foreach (String additionalResponseField in this.AdditionalResponseFields)
            {
                if (domainEvent.AdditionalTransactionResponseMetadata.Any(m => m.Key.ToLower() == additionalResponseField.ToLower()))
                {
                    Type dbTableType = additionalResponseData.GetType();
                    PropertyInfo propertyInfo = dbTableType.GetProperty(additionalResponseField);

                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(additionalResponseData,
                            domainEvent.AdditionalTransactionResponseMetadata.Single(m => m.Key.ToLower() == additionalResponseField.ToLower()).Value);
                    }
                }
            }

            await context.TransactionsAdditionalResponseData.AddAsync(additionalResponseData, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> SetTransactionAmount(AdditionalRequestDataRecordedEvent domainEvent,
                                                       CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            foreach (String additionalRequestField in this.AdditionalRequestFields)
            {
                if (domainEvent.AdditionalTransactionRequestMetadata.Any(m => m.Key.ToLower() == additionalRequestField.ToLower()))
                {
                    if (additionalRequestField == "Amount")
                    {
                        String value = domainEvent.AdditionalTransactionRequestMetadata.Single(m => m.Key.ToLower() == additionalRequestField.ToLower()).Value;
                        // Load this value to the transaction as well
                        transaction.TransactionAmount = Decimal.Parse(value);
                        break;
                    }
                }
            }

            context.Transactions.Entry(transaction).State = EntityState.Modified;
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> StartReconciliation(ReconciliationHasStartedEvent domainEvent,
                                                      CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Database.Entities.Reconciliation reconciliation = new Database.Entities.Reconciliation
            {
                MerchantId = domainEvent.MerchantId,
                TransactionDate = domainEvent.TransactionDateTime.Date,
                TransactionDateTime = domainEvent.TransactionDateTime,
                TransactionTime = domainEvent.TransactionDateTime.TimeOfDay,
                TransactionId = domainEvent.TransactionId,
            };

            await context.Reconciliations.AddAsync(reconciliation, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken); ;
        }

        public async Task<Result> StartTransaction(TransactionHasStartedEvent domainEvent,
                                                   CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Database.Entities.Transaction t = new Database.Entities.Transaction
            {
                MerchantId = domainEvent.MerchantId,
                TransactionDate = domainEvent.TransactionDateTime.Date,
                TransactionDateTime = domainEvent.TransactionDateTime,
                TransactionTime = domainEvent.TransactionDateTime.TimeOfDay,
                TransactionId = domainEvent.TransactionId,
                TransactionNumber = domainEvent.TransactionNumber,
                TransactionReference = domainEvent.TransactionReference,
                TransactionType = domainEvent.TransactionType,
                DeviceIdentifier = domainEvent.DeviceIdentifier
            };

            if (domainEvent.TransactionAmount.HasValue)
            {
                t.TransactionAmount = domainEvent.TransactionAmount.Value;
            }

            await context.AddAsync(t, cancellationToken);

            Logger.LogDebug($"Transaction Loaded with Id [{domainEvent.TransactionId}]");
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateEstate(EstateReferenceAllocatedEvent domainEvent,
                                               CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getEstateResult = await context.LoadEstate(domainEvent, cancellationToken);
            if (getEstateResult.IsFailed)
                return ResultHelpers.CreateFailure(getEstateResult);

            var estate = getEstateResult.Data;
            estate.Reference = domainEvent.EstateReference;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchant(TransactionHasBeenCompletedEvent domainEvent,
                                                 CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Result<Merchant> merchantResult = await context.LoadMerchant(domainEvent, cancellationToken);
            if (merchantResult.IsFailed)
                return ResultHelpers.CreateFailure(merchantResult);
            var merchant = merchantResult.Data;

            if (domainEvent.CompletedDateTime > merchant.LastSaleDateTime)
            {
                merchant.LastSaleDate = domainEvent.CompletedDateTime.Date;
                merchant.LastSaleDateTime = domainEvent.CompletedDateTime;
            }

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateReconciliationOverallTotals(OverallTotalsRecordedEvent domainEvent,
                                                                    CancellationToken cancellationToken) {
            Guid estateId = domainEvent.EstateId;

            EstateManagementGenericContext context = await this.DbContextFactory.GetContext(estateId, ConnectionStringIdentifier, cancellationToken);

            var getReconcilationResult = await context.LoadReconcilation(domainEvent, cancellationToken);
            if (getReconcilationResult.IsFailed)
                return ResultHelpers.CreateFailure(getReconcilationResult);
            var reconciliation = getReconcilationResult.Data;

            reconciliation.TransactionCount = domainEvent.TransactionCount;
            reconciliation.TransactionValue = domainEvent.TransactionValue;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateReconciliationStatus(ReconciliationHasBeenLocallyAuthorisedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getReconcilationResult = await context.LoadReconcilation(domainEvent, cancellationToken);
            if (getReconcilationResult.IsFailed)
                return ResultHelpers.CreateFailure(getReconcilationResult);
            var reconciliation = getReconcilationResult.Data;

            reconciliation.IsAuthorised = true;
            reconciliation.ResponseCode = domainEvent.ResponseCode;
            reconciliation.ResponseMessage = domainEvent.ResponseMessage;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateReconciliationStatus(ReconciliationHasBeenLocallyDeclinedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getReconcilationResult = await context.LoadReconcilation(domainEvent, cancellationToken);
            if (getReconcilationResult.IsFailed)
                return ResultHelpers.CreateFailure(getReconcilationResult);
            var reconciliation = getReconcilationResult.Data;

            reconciliation.IsAuthorised = false;
            reconciliation.ResponseCode = domainEvent.ResponseCode;
            reconciliation.ResponseMessage = domainEvent.ResponseMessage;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateTransactionAuthorisation(TransactionHasBeenLocallyAuthorisedEvent domainEvent,
                                                                 CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            transaction.IsAuthorised = true;
            transaction.ResponseCode = domainEvent.ResponseCode;
            transaction.AuthorisationCode = domainEvent.AuthorisationCode;
            transaction.ResponseMessage = domainEvent.ResponseMessage;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateTransactionAuthorisation(TransactionHasBeenLocallyDeclinedEvent domainEvent,
                                                                 CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            transaction.IsAuthorised = false;
            transaction.ResponseCode = domainEvent.ResponseCode;
            transaction.ResponseMessage = domainEvent.ResponseMessage;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateTransactionAuthorisation(TransactionAuthorisedByOperatorEvent domainEvent,
                                                                 CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            transaction.IsAuthorised = true;
            transaction.ResponseCode = domainEvent.ResponseCode;
            transaction.AuthorisationCode = domainEvent.AuthorisationCode;
            transaction.ResponseMessage = domainEvent.ResponseMessage;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateTransactionAuthorisation(TransactionDeclinedByOperatorEvent domainEvent,
                                                                 CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            transaction.IsAuthorised = false;
            transaction.ResponseCode = domainEvent.ResponseCode;
            transaction.ResponseMessage = domainEvent.ResponseMessage;

            return await context.SaveChangesAsync(cancellationToken); ;
        }

        public async Task<Result> UpdateVoucherIssueDetails(VoucherIssuedEvent domainEvent,
                                                            CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getVoucherResult = await context.LoadVoucher(domainEvent, cancellationToken);
            if (getVoucherResult.IsFailed)
                return ResultHelpers.CreateFailure(getVoucherResult);
            var voucher = getVoucherResult.Data;
            voucher.IsIssued = true;
            voucher.RecipientEmail = domainEvent.RecipientEmail;
            voucher.RecipientMobile = domainEvent.RecipientMobile;
            voucher.IssuedDateTime = domainEvent.IssuedDateTime;
            voucher.IssuedDate = domainEvent.IssuedDateTime.Date;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateVoucherRedemptionDetails(VoucherFullyRedeemedEvent domainEvent,
                                                                 CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getVoucherResult = await context.LoadVoucher(domainEvent, cancellationToken);
            if (getVoucherResult.IsFailed)
                return ResultHelpers.CreateFailure(getVoucherResult);
            var voucher = getVoucherResult.Data;

            voucher.IsRedeemed = true;
            voucher.RedeemedDateTime = domainEvent.RedeemedDateTime;
            voucher.RedeemedDate = domainEvent.RedeemedDateTime.Date;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result<List<Models.Operator.Operator>>> GetOperators(Guid estateId, CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.DbContextFactory.GetContext(estateId, ConnectionStringIdentifier, cancellationToken);

            Database.Entities.Estate estate = await context.Estates.SingleOrDefaultAsync(e => e.EstateId == estateId, cancellationToken: cancellationToken);
            List<Database.Entities.Operator> operators = await (from o in context.Operators where o.EstateId == estate.EstateId select o).ToListAsync(cancellationToken);

            List<Models.Operator.Operator> models = new();

            foreach (Database.Entities.Operator @operator in operators)
            {
                models.Add(new Models.Operator.Operator
                {
                    OperatorId = @operator.OperatorId,
                    RequireCustomTerminalNumber = @operator.RequireCustomTerminalNumber,
                    RequireCustomMerchantNumber = @operator.RequireCustomMerchantNumber,
                    Name = @operator.Name,
                });
            }

            return Result.Success(models);
        }

        public async Task<Result> UpdateOperator(OperatorNameUpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Result<Database.Entities.Operator> operatorResult = await context.LoadOperator(domainEvent, cancellationToken);
            if (operatorResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorResult);
            Database.Entities.Operator @operator = operatorResult.Data;
            @operator.Name = domainEvent.Name;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateOperator(OperatorRequireCustomMerchantNumberChangedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Result<Database.Entities.Operator> operatorResult = await context.LoadOperator(domainEvent, cancellationToken);
            if (operatorResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorResult);
            Database.Entities.Operator @operator = operatorResult.Data;

            @operator.RequireCustomMerchantNumber = domainEvent.RequireCustomMerchantNumber;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateOperator(OperatorRequireCustomTerminalNumberChangedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Result<Database.Entities.Operator> operatorResult = await context.LoadOperator(domainEvent, cancellationToken);
            if (operatorResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorResult);
            Database.Entities.Operator @operator = operatorResult.Data;

            @operator.RequireCustomTerminalNumber = domainEvent.RequireCustomTerminalNumber;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddOperator(OperatorCreatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Database.Entities.Operator @operator = new Database.Entities.Operator
            {
                RequireCustomTerminalNumber = domainEvent.RequireCustomTerminalNumber,
                OperatorId = domainEvent.OperatorId,
                Name = domainEvent.Name,
                RequireCustomMerchantNumber = domainEvent.RequireCustomMerchantNumber,
                EstateId = domainEvent.EstateId
            };

            await context.Operators.AddAsync(@operator, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }
    }
}
