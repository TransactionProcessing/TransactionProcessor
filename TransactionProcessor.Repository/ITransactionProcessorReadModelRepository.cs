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
using TransactionProcessor.Database.Entities;
using Shared.Results;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Contract;
using Contract = TransactionProcessor.Database.Entities.Contract;
using ContractModel = TransactionProcessor.Models.Contract.Contract;
using ContractProductTransactionFee = TransactionProcessor.Database.Entities.ContractProductTransactionFee;

namespace TransactionProcessor.Repository {
    public interface ITransactionProcessorReadModelRepository {

        Task<Result> UpdateOperator(OperatorDomainEvents.OperatorNameUpdatedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateOperator(OperatorDomainEvents.OperatorRequireCustomMerchantNumberChangedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateOperator(OperatorDomainEvents.OperatorRequireCustomTerminalNumberChangedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> AddOperator(OperatorDomainEvents.OperatorCreatedEvent domainEvent,
                                 CancellationToken cancellationToken);

        Task<Result> AddContract(ContractDomainEvents.ContractCreatedEvent domainEvent,
                                 CancellationToken cancellationToken);

        Task<Result> AddContractProduct(ContractDomainEvents.VariableValueProductAddedToContractEvent domainEvent,
                                        CancellationToken cancellationToken);

        Task<Result> AddContractProduct(ContractDomainEvents.FixedValueProductAddedToContractEvent domainEvent,
                                        CancellationToken cancellationToken);

        Task<Result> AddContractProductTransactionFee(ContractDomainEvents.TransactionFeeForProductAddedToContractEvent domainEvent,
                                                      CancellationToken cancellationToken);

        //Task<Result> AddContractToMerchant(ContractAddedToMerchantEvent domainEvent,
        //                                   CancellationToken cancellationToken);

        Task<Result> AddEstate(EstateDomainEvents.EstateCreatedEvent domainEvent,
                               CancellationToken cancellationToken);

        Task<Result> AddEstateSecurityUser(EstateDomainEvents.SecurityUserAddedToEstateEvent domainEvent,
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

        Task<Result> AddProductDetailsToTransaction(TransactionDomainEvents.ProductDetailsAddedToTransactionEvent domainEvent,
                                                    CancellationToken cancellationToken);

        //Task<Result> AddSettledFeeToStatement(SettledFeeAddedToStatementEvent domainEvent,
        //                                      CancellationToken cancellationToken);

        Task<Result> AddSettledMerchantFeeToSettlement(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent domainEvent,
                                                       CancellationToken cancellationToken);

        Task<Result> AddSourceDetailsToTransaction(TransactionDomainEvents.TransactionSourceAddedToTransactionEvent domainEvent,
                                                   CancellationToken cancellationToken);

        //Task<Result> AddTransactionToStatement(TransactionAddedToStatementEvent domainEvent,
        //                                       CancellationToken cancellationToken);

        Task<Result> CompleteReconciliation(ReconciliationDomainEvents.ReconciliationHasCompletedEvent domainEvent,
                                            CancellationToken cancellationToken);

        Task<Result> CompleteTransaction(TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent,
                                         CancellationToken cancellationToken);

        Task<Result> CreateFloat(FloatDomainEvents.FloatCreatedForContractProductEvent domainEvent,
                                 CancellationToken cancellationToken);

        Task<Result> CreateFloatActivity(FloatDomainEvents.FloatCreditPurchasedEvent domainEvent,
                                         CancellationToken cancellationToken);

        Task<Result> CreateFloatActivity(FloatDomainEvents.FloatDecreasedByTransactionEvent domainEvent,
                                         CancellationToken cancellationToken);

        Task<Result> CreateReadModel(EstateDomainEvents.EstateCreatedEvent domainEvent,
                                     CancellationToken cancellationToken);

        Task<Result> CreateSettlement(SettlementDomainEvents.SettlementCreatedForDateEvent domainEvent,
                                      CancellationToken cancellationToken);

        //Task<Result> CreateStatement(StatementCreatedEvent domainEvent,
        //                             CancellationToken cancellationToken);

        Task<Result> DisableContractProductTransactionFee(ContractDomainEvents.TransactionFeeForProductDisabledEvent domainEvent,
                                                          CancellationToken cancellationToken);

        Task<Result> MarkMerchantFeeAsSettled(SettlementDomainEvents.MerchantFeeSettledEvent domainEvent,
                                              CancellationToken cancellationToken);

        Task<Result> MarkSettlementAsCompleted(SettlementDomainEvents.SettlementCompletedEvent domainEvent,
                                               CancellationToken cancellationToken);

        Task<Result> MarkSettlementAsProcessingStarted(SettlementDomainEvents.SettlementProcessingStartedEvent domainEvent,
                                                       CancellationToken cancellationToken);

        //Task<Result> MarkStatementAsGenerated(StatementGeneratedEvent domainEvent,
        //                                      CancellationToken cancellationToken);

        Task<Result> RecordTransactionAdditionalRequestData(TransactionDomainEvents.AdditionalRequestDataRecordedEvent domainEvent,
                                                            CancellationToken cancellationToken);

        Task<Result> RecordTransactionAdditionalResponseData(TransactionDomainEvents.AdditionalResponseDataRecordedEvent domainEvent,
                                                             CancellationToken cancellationToken);

        Task<Result> SetTransactionAmount(TransactionDomainEvents.AdditionalRequestDataRecordedEvent domainEvent,
                                          CancellationToken cancellationToken);

        Task<Result> StartReconciliation(ReconciliationDomainEvents.ReconciliationHasStartedEvent domainEvent,
                                         CancellationToken cancellationToken);

        Task<Result> StartTransaction(TransactionDomainEvents.TransactionHasStartedEvent domainEvent,
                                      CancellationToken cancellationToken);

        Task<Result> UpdateEstate(EstateDomainEvents.EstateReferenceAllocatedEvent domainEvent,
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

        Task<Result> UpdateMerchant(TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateReconciliationOverallTotals(ReconciliationDomainEvents.OverallTotalsRecordedEvent domainEvent,
                                                       CancellationToken cancellationToken);

        Task<Result> UpdateReconciliationStatus(ReconciliationDomainEvents.ReconciliationHasBeenLocallyAuthorisedEvent domainEvent,
                                                CancellationToken cancellationToken);

        Task<Result> UpdateReconciliationStatus(ReconciliationDomainEvents.ReconciliationHasBeenLocallyDeclinedEvent domainEvent,
                                                CancellationToken cancellationToken);

        Task<Result> UpdateTransactionAuthorisation(TransactionDomainEvents.TransactionHasBeenLocallyAuthorisedEvent domainEvent,
                                                    CancellationToken cancellationToken);

        Task<Result> UpdateTransactionAuthorisation(TransactionDomainEvents.TransactionHasBeenLocallyDeclinedEvent domainEvent,
                                                    CancellationToken cancellationToken);

        Task<Result> UpdateTransactionAuthorisation(TransactionDomainEvents.TransactionAuthorisedByOperatorEvent domainEvent,
                                                    CancellationToken cancellationToken);

        Task<Result> UpdateTransactionAuthorisation(TransactionDomainEvents.TransactionDeclinedByOperatorEvent domainEvent,
                                                    CancellationToken cancellationToken);

        Task<Result> UpdateVoucherIssueDetails(VoucherDomainEvents.VoucherIssuedEvent domainEvent,
                                               CancellationToken cancellationToken);

        Task<Result> UpdateVoucherRedemptionDetails(VoucherDomainEvents.VoucherFullyRedeemedEvent domainEvent,
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

        Task<Result<List<ContractModel>>> GetContracts(Guid estateId,
                                                  CancellationToken cancellationToken);
    }

    [ExcludeFromCodeCoverage]
    public class TransactionProcessorReadModelRepository : ITransactionProcessorReadModelRepository {
        private readonly Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext> DbContextFactory;

        private const String ConnectionStringIdentifier = "EstateReportingReadModel";

        public TransactionProcessorReadModelRepository(Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext> dbContextFactory) {
   this.DbContextFactory = dbContextFactory;
        
        }

        public async Task<Result<List<ContractModel>>> GetContracts(Guid estateId,
                                                            CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.DbContextFactory.GetContext(estateId, ConnectionStringIdentifier, cancellationToken);

            var query = await (from c in context.Contracts
                               join cp in context.ContractProducts on c.ContractId equals cp.ContractId into cps
                               from contractprouduct in cps.DefaultIfEmpty()
                               join eo in context.Operators on c.OperatorId equals eo.OperatorId
                               join e in context.Estates on eo.EstateId equals e.EstateId
                               select new
                               {
                                   Estate = e,
                                   Contract = c,
                                   Product = contractprouduct,
                                   Operator = eo
                               }).ToListAsync(cancellationToken);

            List<ContractModel> contracts = new List<ContractModel>();

            foreach (var contractData in query)
            {
                // attempt to find the contract
                ContractModel contract = contracts.SingleOrDefault(c => c.ContractId == contractData.Contract.ContractId);

                if (contract == null)
                {
                    // create the contract
                    contract = new ContractModel
                    {
                        EstateReportingId = contractData.Estate.EstateReportingId,
                        EstateId = contractData.Estate.EstateId,
                        OperatorId = contractData.Contract.OperatorId,
                        OperatorName = contractData.Operator.Name,
                        Products = new List<Product>(),
                        Description = contractData.Contract.Description,
                        IsCreated = true,
                        ContractId = contractData.Contract.ContractId,
                        ContractReportingId = contractData.Contract.ContractReportingId
                    };

                    contracts.Add(contract);
                }

                // Now add the product if not already added
                Boolean productFound = contract.Products.Any(p => p.ContractProductId == contractData.Product.ContractProductId);

                if (productFound == false)
                {
                    if (contractData.Product != null)
                    {
                        // Not already there so need to add it
                        contract.Products.Add(new Product
                        {
                            ContractProductId = contractData.Product.ContractProductId,
                            TransactionFees = null,
                            Value = contractData.Product.Value,
                            Name = contractData.Product.ProductName,
                            DisplayText = contractData.Product.DisplayText,
                            ContractProductReportingId = contractData.Product.ContractProductReportingId
                        });
                    }
                }
            }

            return Result.Success(contracts);
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

        public async Task<Result> AddContractProductTransactionFee(ContractDomainEvents.TransactionFeeForProductAddedToContractEvent domainEvent,
                                                                   CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            ContractProductTransactionFee contractProductTransactionFee = new ContractProductTransactionFee
            {
                ContractProductId = domainEvent.ProductId,
                Description = domainEvent.Description,
                Value = domainEvent.Value,
                ContractProductTransactionFeeId = domainEvent.TransactionFeeId,
                CalculationType = domainEvent.CalculationType,
                IsEnabled = true,
                FeeType = domainEvent.FeeType
            };

            await context.ContractProductTransactionFees.AddAsync(contractProductTransactionFee, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddEstate(EstateDomainEvents.EstateCreatedEvent domainEvent,
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

        public async Task<Result> AddEstateSecurityUser(EstateDomainEvents.SecurityUserAddedToEstateEvent domainEvent,
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

        public async Task<Result> AddProductDetailsToTransaction(TransactionDomainEvents.ProductDetailsAddedToTransactionEvent domainEvent,
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

        public async Task<Result> AddSettledMerchantFeeToSettlement(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent domainEvent,
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

        public async Task<Result> AddSourceDetailsToTransaction(TransactionDomainEvents.TransactionSourceAddedToTransactionEvent domainEvent,
                                                                CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            transaction.TransactionSource = domainEvent.TransactionSource;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CompleteReconciliation(ReconciliationDomainEvents.ReconciliationHasCompletedEvent domainEvent,
                                                         CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getReconcilationResult = await context.LoadReconcilation(domainEvent, cancellationToken);
            if (getReconcilationResult.IsFailed)
                return ResultHelpers.CreateFailure(getReconcilationResult);
            var reconciliation = getReconcilationResult.Data;

            reconciliation.IsCompleted = true;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CompleteTransaction(TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent,
                                                      CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            transaction.IsCompleted = true;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CreateFloat(FloatDomainEvents.FloatCreatedForContractProductEvent domainEvent,
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

        public async Task<Result> CreateFloatActivity(FloatDomainEvents.FloatCreditPurchasedEvent domainEvent,
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

        public async Task<Result> CreateFloatActivity(FloatDomainEvents.FloatDecreasedByTransactionEvent domainEvent,
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

        public async Task<Result> CreateReadModel(EstateDomainEvents.EstateCreatedEvent domainEvent,
                                                  CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Logger.LogInformation($"About to run migrations on Read Model database for estate [{domainEvent.EstateId}]");

            // Ensure the db is at the latest version
            await context.MigrateAsync(cancellationToken);

            Logger.LogWarning($"Read Model database for estate [{domainEvent.EstateId}] migrated to latest version");
            return Result.Success();
        }

        public async Task<Result> CreateSettlement(SettlementDomainEvents.SettlementCreatedForDateEvent domainEvent,
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

        public async Task<Result> DisableContractProductTransactionFee(ContractDomainEvents.TransactionFeeForProductDisabledEvent domainEvent,
                                                                       CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            ContractProductTransactionFee transactionFee = await context.LoadContractProductTransactionFee(domainEvent, cancellationToken);

            transactionFee.IsEnabled = false;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> MarkMerchantFeeAsSettled(SettlementDomainEvents.MerchantFeeSettledEvent domainEvent,
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

        public async Task<Result> MarkSettlementAsCompleted(SettlementDomainEvents.SettlementCompletedEvent domainEvent,
                                                            CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getSettlementResult = await context.LoadSettlement(domainEvent, cancellationToken);
            if (getSettlementResult.IsFailed)
                return ResultHelpers.CreateFailure(getSettlementResult);
            var settlement = getSettlementResult.Data;

            settlement.IsCompleted = true;
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> MarkSettlementAsProcessingStarted(SettlementDomainEvents.SettlementProcessingStartedEvent domainEvent,
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

        public async Task<Result> RecordTransactionAdditionalRequestData(TransactionDomainEvents.AdditionalRequestDataRecordedEvent domainEvent,
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

        public async Task<Result> RecordTransactionAdditionalResponseData(TransactionDomainEvents.AdditionalResponseDataRecordedEvent domainEvent,
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

        public async Task<Result> SetTransactionAmount(TransactionDomainEvents.AdditionalRequestDataRecordedEvent domainEvent,
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

        public async Task<Result> StartReconciliation(ReconciliationDomainEvents.ReconciliationHasStartedEvent domainEvent,
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

        public async Task<Result> StartTransaction(TransactionDomainEvents.TransactionHasStartedEvent domainEvent,
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

        public async Task<Result> UpdateEstate(EstateDomainEvents.EstateReferenceAllocatedEvent domainEvent,
                                               CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            var getEstateResult = await context.LoadEstate(domainEvent, cancellationToken);
            if (getEstateResult.IsFailed)
                return ResultHelpers.CreateFailure(getEstateResult);

            var estate = getEstateResult.Data;
            estate.Reference = domainEvent.EstateReference;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchant(TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent,
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

        public async Task<Result> UpdateReconciliationOverallTotals(ReconciliationDomainEvents.OverallTotalsRecordedEvent domainEvent,
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

        public async Task<Result> UpdateReconciliationStatus(ReconciliationDomainEvents.ReconciliationHasBeenLocallyAuthorisedEvent domainEvent,
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

        public async Task<Result> UpdateReconciliationStatus(ReconciliationDomainEvents.ReconciliationHasBeenLocallyDeclinedEvent domainEvent,
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

        public async Task<Result> UpdateTransactionAuthorisation(TransactionDomainEvents.TransactionHasBeenLocallyAuthorisedEvent domainEvent,
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

        public async Task<Result> UpdateTransactionAuthorisation(TransactionDomainEvents.TransactionHasBeenLocallyDeclinedEvent domainEvent,
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

        public async Task<Result> UpdateTransactionAuthorisation(TransactionDomainEvents.TransactionAuthorisedByOperatorEvent domainEvent,
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

        public async Task<Result> UpdateTransactionAuthorisation(TransactionDomainEvents.TransactionDeclinedByOperatorEvent domainEvent,
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

        public async Task<Result> UpdateVoucherIssueDetails(VoucherDomainEvents.VoucherIssuedEvent domainEvent,
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

        public async Task<Result> UpdateVoucherRedemptionDetails(VoucherDomainEvents.VoucherFullyRedeemedEvent domainEvent,
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

        public async Task<Result> UpdateOperator(OperatorDomainEvents.OperatorNameUpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Result<Database.Entities.Operator> operatorResult = await context.LoadOperator(domainEvent, cancellationToken);
            if (operatorResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorResult);
            Database.Entities.Operator @operator = operatorResult.Data;
            @operator.Name = domainEvent.Name;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateOperator(OperatorDomainEvents.OperatorRequireCustomMerchantNumberChangedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Result<Database.Entities.Operator> operatorResult = await context.LoadOperator(domainEvent, cancellationToken);
            if (operatorResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorResult);
            Database.Entities.Operator @operator = operatorResult.Data;

            @operator.RequireCustomMerchantNumber = domainEvent.RequireCustomMerchantNumber;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateOperator(OperatorDomainEvents.OperatorRequireCustomTerminalNumberChangedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Result<Database.Entities.Operator> operatorResult = await context.LoadOperator(domainEvent, cancellationToken);
            if (operatorResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorResult);
            Database.Entities.Operator @operator = operatorResult.Data;

            @operator.RequireCustomTerminalNumber = domainEvent.RequireCustomTerminalNumber;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddOperator(OperatorDomainEvents.OperatorCreatedEvent domainEvent, CancellationToken cancellationToken)
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

        public async Task<Result> AddContract(ContractDomainEvents.ContractCreatedEvent domainEvent,
                                              CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            Contract contract = new Contract
            {
                EstateId = domainEvent.EstateId,
                OperatorId = domainEvent.OperatorId,
                ContractId = domainEvent.ContractId,
                Description = domainEvent.Description
            };

            await context.Contracts.AddAsync(contract, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddContractProduct(ContractDomainEvents.VariableValueProductAddedToContractEvent domainEvent,
                                                     CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            ContractProduct contractProduct = new ContractProduct
            {
                ContractId = domainEvent.ContractId,
                ContractProductId = domainEvent.ProductId,
                DisplayText = domainEvent.DisplayText,
                ProductName = domainEvent.ProductName,
                Value = null,
                ProductType = domainEvent.ProductType
            };

            await context.ContractProducts.AddAsync(contractProduct, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddContractProduct(ContractDomainEvents.FixedValueProductAddedToContractEvent domainEvent,
                                                     CancellationToken cancellationToken) {
            EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

            ContractProduct contractProduct = new ContractProduct
            {
                ContractId = domainEvent.ContractId,
                ContractProductId = domainEvent.ProductId,
                DisplayText = domainEvent.DisplayText,
                ProductName = domainEvent.ProductName,
                Value = domainEvent.Value,
                ProductType = domainEvent.ProductType
            };

            await context.ContractProducts.AddAsync(contractProduct, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }
    }
}
