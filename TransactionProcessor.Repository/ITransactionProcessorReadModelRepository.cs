using FileProcessor.File.DomainEvents;
using FileProcessor.FileImportLog.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EntityFramework;
using Shared.Logger;
using Shared.Results;
using SimpleResults;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Database.ViewEntities;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Estate;
using TransactionProcessor.Models.Settlement;
using static TransactionProcessor.DomainEvents.MerchantDomainEvents;
using static TransactionProcessor.DomainEvents.MerchantStatementDomainEvents;
using Contract = TransactionProcessor.Database.Entities.Contract;
using ContractModel = TransactionProcessor.Models.Contract.Contract;
using ContractProductTransactionFee = TransactionProcessor.Database.Entities.ContractProductTransactionFee;
using Estate = TransactionProcessor.Database.Entities.Estate;
using File = TransactionProcessor.Database.Entities.File;
using MerchantModel = TransactionProcessor.Models.Merchant.Merchant;
using Operator = TransactionProcessor.Database.Entities.Operator;

namespace TransactionProcessor.Repository {
    public interface ITransactionProcessorReadModelRepository {

        Task<Result<MerchantModel>> GetMerchantFromReference(Guid estateId,
                                                             String reference,
                                                             CancellationToken cancellationToken);
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

        Task<Result> AddContractToMerchant(ContractAddedToMerchantEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> AddEstate(EstateDomainEvents.EstateCreatedEvent domainEvent,
                               CancellationToken cancellationToken);

        Task<Result> AddEstateSecurityUser(EstateDomainEvents.SecurityUserAddedToEstateEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> AddFile(FileCreatedEvent domainEvent,
                             CancellationToken cancellationToken);

        Task<Result> AddFileImportLog(ImportLogCreatedEvent domainEvent,
                                      CancellationToken cancellationToken);

        Task<Result> AddFileLineToFile(FileLineAddedEvent domainEvent,
                                       CancellationToken cancellationToken);

        Task<Result> AddFileToImportLog(FileAddedToImportLogEvent domainEvent,
                                        CancellationToken cancellationToken);

        Task<Result> AddMerchant(MerchantDomainEvents.MerchantCreatedEvent domainEvent,
                                CancellationToken cancellationToken);

        Task<Result> UpdateMerchant(MerchantDomainEvents.MerchantNameUpdatedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> AddMerchantAddress(MerchantDomainEvents.AddressAddedEvent domainEvent,
                                        CancellationToken cancellationToken);

        Task<Result> AddMerchantContact(MerchantDomainEvents.ContactAddedEvent domainEvent,
                                        CancellationToken cancellationToken);

        Task<Result> AddMerchantDevice(MerchantDomainEvents.DeviceAddedToMerchantEvent domainEvent,
                                       CancellationToken cancellationToken);

        Task<Result> SwapMerchantDevice(MerchantDomainEvents.DeviceSwappedForMerchantEvent domainEvent,
                                        CancellationToken cancellationToken);

        Task<Result> AddMerchantOperator(MerchantDomainEvents.OperatorAssignedToMerchantEvent domainEvent,
                                         CancellationToken cancellationToken);

        Task<Result> AddMerchantSecurityUser(MerchantDomainEvents.SecurityUserAddedToMerchantEvent domainEvent,
                                             CancellationToken cancellationToken);

        Task<Result> AddPendingMerchantFeeToSettlement(SettlementDomainEvents.MerchantFeeAddedPendingSettlementEvent domainEvent,
                                                       CancellationToken cancellationToken);

        Task<Result> AddProductDetailsToTransaction(TransactionDomainEvents.ProductDetailsAddedToTransactionEvent domainEvent,
                                                    CancellationToken cancellationToken);

        Task<Result> AddSettledMerchantFeeToSettlement(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent domainEvent,
                                                       CancellationToken cancellationToken);

        Task<Result> AddSourceDetailsToTransaction(TransactionDomainEvents.TransactionSourceAddedToTransactionEvent domainEvent,
                                                   CancellationToken cancellationToken);

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

        Task<Result> CreateStatement(StatementCreatedEvent domainEvent,
                                     CancellationToken cancellationToken);

        Task<Result> DisableContractProductTransactionFee(ContractDomainEvents.TransactionFeeForProductDisabledEvent domainEvent,
                                                          CancellationToken cancellationToken);

        Task<Result> MarkMerchantFeeAsSettled(SettlementDomainEvents.MerchantFeeSettledEvent domainEvent,
                                              CancellationToken cancellationToken);

        Task<Result> MarkSettlementAsCompleted(SettlementDomainEvents.SettlementCompletedEvent domainEvent,
                                               CancellationToken cancellationToken);

        Task<Result> MarkSettlementAsProcessingStarted(SettlementDomainEvents.SettlementProcessingStartedEvent domainEvent,
                                                       CancellationToken cancellationToken);

        Task<Result> MarkStatementAsGenerated(StatementGeneratedEvent domainEvent,
                                              CancellationToken cancellationToken);

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

        Task<Result> UpdateFileAsComplete(FileProcessingCompletedEvent domainEvent,
                                          CancellationToken cancellationToken);

        Task<Result> UpdateFileLine(FileLineProcessingSuccessfulEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateFileLine(FileLineProcessingFailedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateFileLine(FileLineProcessingIgnoredEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateMerchant(MerchantDomainEvents.MerchantReferenceAllocatedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateMerchant(StatementGeneratedEvent domainEvent,
                                    CancellationToken cancellationToken);

        Task<Result> UpdateMerchant(MerchantDomainEvents.SettlementScheduleChangedEvent domainEvent,
                                    CancellationToken cancellationToken);

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

        //Task<Result> UpdateVoucherIssueDetails(VoucherDomainEvents.VoucherIssuedEvent domainEvent,
        //                                       CancellationToken cancellationToken);

        //Task<Result> UpdateVoucherRedemptionDetails(VoucherDomainEvents.VoucherFullyRedeemedEvent domainEvent,
        //                                            CancellationToken cancellationToken);

        Task<Result> RemoveOperatorFromMerchant(MerchantDomainEvents.OperatorRemovedFromMerchantEvent domainEvent,
                                                CancellationToken cancellationToken);

        Task<Result> RemoveContractFromMerchant(MerchantDomainEvents.ContractRemovedFromMerchantEvent domainEvent,
                                                CancellationToken cancellationToken);

        Task<Result> UpdateMerchantAddress(MerchantDomainEvents.MerchantAddressLine1UpdatedEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchantAddress(MerchantDomainEvents.MerchantAddressLine2UpdatedEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchantAddress(MerchantDomainEvents.MerchantAddressLine3UpdatedEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchantAddress(MerchantDomainEvents.MerchantAddressLine4UpdatedEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchantAddress(MerchantDomainEvents.MerchantCountyUpdatedEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchantAddress(MerchantDomainEvents.MerchantRegionUpdatedEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchantAddress(MerchantDomainEvents.MerchantTownUpdatedEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchantAddress(MerchantDomainEvents.MerchantPostalCodeUpdatedEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchantContact(MerchantDomainEvents.MerchantContactNameUpdatedEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchantContact(MerchantDomainEvents.MerchantContactEmailAddressUpdatedEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result> UpdateMerchantContact(MerchantDomainEvents.MerchantContactPhoneNumberUpdatedEvent domainEvent,
                                           CancellationToken cancellationToken);

        Task<Result<Models.Estate.Estate>> GetEstate(Guid estateId,
                                              CancellationToken cancellationToken);

        Task<Result<List<Models.Operator.Operator>>> GetOperators(Guid estateId,
                                                                  CancellationToken cancellationToken);
        Task<Result<List<ContractModel>>> GetContracts(Guid estateId,
                                                  CancellationToken cancellationToken);

        Task<Result<List<ContractModel>>> GetMerchantContracts(Guid estateId,
                                                               Guid merchantId,
                                                               CancellationToken cancellationToken);

        Task<Result<List<MerchantModel>>> GetMerchants(Guid estateId,
                                                       CancellationToken cancellationToken);

        Task<Result<SettlementModel>> GetSettlement(Guid estateId,
                                                    Guid merchantId,
                                                    Guid settlementId,
                                                    CancellationToken cancellationToken);

        Task<Result<List<SettlementModel>>> GetSettlements(Guid estateId,
                                                           Guid? merchantId,
                                                           String startDate,
                                                           String endDate,
                                                           CancellationToken cancellationToken);

        Task<Result> RecordTransactionTimings(TransactionDomainEvents.TransactionTimingsAddedToTransactionEvent domainEvent,
                                              CancellationToken cancellationToken);

        Task<Result> AddTransactionToStatement(MerchantStatementForDateDomainEvents.TransactionAddedToStatementForDateEvent domainEvent,
                                               CancellationToken cancellationToken);

        Task<Result> AddSettledFeeToStatement(MerchantStatementForDateDomainEvents.SettledFeeAddedToStatementForDateEvent domainEvent,
                                              CancellationToken cancellationToken);

        Task<Result> AddEstateOperator(EstateDomainEvents.OperatorAddedToEstateEvent de,
                                       CancellationToken cancellationToken);

        Task<Result> RemoveOperatorFromEstate(EstateDomainEvents.OperatorRemovedFromEstateEvent de,
                                              CancellationToken cancellationToken);
    }

    [ExcludeFromCodeCoverage]
    public class TransactionProcessorReadModelRepository : ITransactionProcessorReadModelRepository {
        private readonly IDbContextResolver<EstateManagementContext> Resolver;
        private readonly IDbContextFactory<EstateManagementContext> DbContextFactory;
        private static readonly String EstateManagementDatabaseName = "TransactionProcessorReadModel";

        public TransactionProcessorReadModelRepository(IDbContextResolver<EstateManagementContext> resolver) {
            this.Resolver = resolver;
        }

        public async Task<Result> UpdateMerchant(StatementGeneratedEvent domainEvent,
                                                 CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Result<Merchant> merchantResult = await context.LoadMerchant(domainEvent, cancellationToken);
            if (merchantResult.IsFailed)
                return ResultHelpers.CreateFailure(merchantResult);
            Merchant? merchant = merchantResult.Data;

            if (merchant.LastStatementGenerated > domainEvent.DateGenerated)
                return Result.Success();

            merchant.LastStatementGenerated = domainEvent.DateGenerated;
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result<MerchantModel>> GetMerchantFromReference(Guid estateId,
                                                                          String reference,
                                                                          CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(estateId);

            Merchant merchant = await (from m in context.Merchants where m.Reference == reference select m).SingleOrDefaultAsync(cancellationToken);

            if (merchant == null)
                return Result.NotFound($"No merchant found with reference {reference}");

            return Result.Success(ModelFactory.ConvertFrom(estateId, merchant, null, null, null, null, null));
        }

        public async Task<Result> AddMerchant(MerchantDomainEvents.MerchantCreatedEvent domainEvent,
                                              CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Merchant merchant = new Merchant
            {
                EstateId = domainEvent.EstateId,
                MerchantId = domainEvent.MerchantId,
                Name = domainEvent.MerchantName,
                CreatedDateTime = domainEvent.DateCreated,
                LastStatementGenerated = DateTime.MinValue,
                SettlementSchedule = 0
            };

            await context.Merchants.AddAsync(merchant, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> UpdateMerchant(MerchantNameUpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Result<Merchant> merchantResult = await context.LoadMerchant(domainEvent, cancellationToken);
            if (merchantResult.IsFailed)
                return ResultHelpers.CreateFailure(merchantResult);
            var merchant = merchantResult.Data;
            merchant.Name = domainEvent.MerchantName;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddMerchantAddress(AddressAddedEvent domainEvent,
                                             CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            MerchantAddress merchantAddress = new MerchantAddress
            {
                MerchantId = domainEvent.MerchantId,
                AddressId = domainEvent.AddressId,
                AddressLine1 = domainEvent.AddressLine1,
                AddressLine2 = domainEvent.AddressLine2,
                AddressLine3 = domainEvent.AddressLine3,
                AddressLine4 = domainEvent.AddressLine4,
                Country = domainEvent.Country,
                PostalCode = domainEvent.PostalCode,
                Region = domainEvent.Region,
                Town = domainEvent.Town
            };

            await context.MerchantAddresses.AddAsync(merchantAddress, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddMerchantContact(ContactAddedEvent domainEvent,
                                             CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            MerchantContact merchantContact = new MerchantContact
            {
                MerchantId = domainEvent.MerchantId,
                Name = domainEvent.ContactName,
                ContactId = domainEvent.ContactId,
                EmailAddress = domainEvent.ContactEmailAddress,
                PhoneNumber = domainEvent.ContactPhoneNumber
            };

            await context.MerchantContacts.AddAsync(merchantContact, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddMerchantDevice(DeviceAddedToMerchantEvent domainEvent,
                                            CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            MerchantDevice merchantDevice = new() {
                MerchantId = domainEvent.MerchantId,
                DeviceId = domainEvent.DeviceId,
                DeviceIdentifier = domainEvent.DeviceIdentifier
            };

            await context.MerchantDevices.AddAsync(merchantDevice, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> SwapMerchantDevice(DeviceSwappedForMerchantEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Result<MerchantDevice> getOriginalDeviceResult = await context.LoadOriginalMerchantDevice(domainEvent, cancellationToken);
            if (getOriginalDeviceResult.IsFailed)
                return ResultHelpers.CreateFailure(getOriginalDeviceResult);
            MerchantDevice? originalDevice = getOriginalDeviceResult.Data;

            originalDevice.IsEnabled = false;

            MerchantDevice newDevice = new() { DeviceId = domainEvent.DeviceId, IsEnabled = true, DeviceIdentifier = domainEvent.NewDeviceIdentifier, MerchantId = domainEvent.MerchantId };
            await context.MerchantDevices.AddAsync(newDevice, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddMerchantOperator(OperatorAssignedToMerchantEvent domainEvent,
                                              CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);
            String operatorName = domainEvent.Name;
            if (String.IsNullOrEmpty(operatorName))
            {
                // Lookup the operator
                Operator @operator = await context.Operators.SingleOrDefaultAsync(o => o.OperatorId == domainEvent.OperatorId, cancellationToken);
                operatorName = @operator.Name;
            }

            if (String.IsNullOrEmpty(operatorName))
            {
                return Result.Failure("Unable to get operator name and this can't be null");
            }

            MerchantOperator merchantOperator = new MerchantOperator
            {
                Name = operatorName,
                MerchantId = domainEvent.MerchantId,
                MerchantNumber = domainEvent.MerchantNumber,
                OperatorId = domainEvent.OperatorId,
                TerminalNumber = domainEvent.TerminalNumber
            };

            await context.MerchantOperators.AddAsync(merchantOperator, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddMerchantSecurityUser(SecurityUserAddedToMerchantEvent domainEvent,
                                                  CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            MerchantSecurityUser merchantSecurityUser = new MerchantSecurityUser
            {
                MerchantId = domainEvent.MerchantId,
                EmailAddress = domainEvent.EmailAddress,
                SecurityUserId = domainEvent.SecurityUserId
            };

            await context.MerchantSecurityUsers.AddAsync(merchantSecurityUser, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }
        public async Task<Result> UpdateMerchant(SettlementScheduleChangedEvent domainEvent,
                                                 CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Result<Merchant> merchantResult = await context.LoadMerchant(domainEvent, cancellationToken);
            if (merchantResult.IsFailed)
                return ResultHelpers.CreateFailure(merchantResult);
            var merchant = merchantResult.Data;

            merchant.SettlementSchedule = domainEvent.SettlementSchedule;
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> RemoveOperatorFromMerchant(OperatorRemovedFromMerchantEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            MerchantOperator merchantOperator = await context.MerchantOperators.SingleOrDefaultAsync(o => o.OperatorId == domainEvent.OperatorId &&
                                                                                                          o.MerchantId == domainEvent.MerchantId,
                cancellationToken: cancellationToken);
            if (merchantOperator == null)
            {
                return Result.NotFound($"No operator {domainEvent.OperatorId} found for merchant {domainEvent.MerchantId}");
            }

            merchantOperator.IsDeleted = true;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> RemoveContractFromMerchant(ContractRemovedFromMerchantEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            MerchantContract merchantContract = await context.MerchantContracts.SingleOrDefaultAsync(o => o.ContractId == domainEvent.ContractId &&
                                                                                                          o.MerchantId == domainEvent.MerchantId,
                cancellationToken: cancellationToken);
            if (merchantContract == null)
            {
                return Result.NotFound($"No contract {domainEvent.ContractId} found for merchant {domainEvent.MerchantId}");
            }


            merchantContract.IsDeleted = true;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchantAddress(MerchantAddressLine1UpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getMerchantAddressResult = await context.LoadMerchantAddress(domainEvent, cancellationToken);
            if (getMerchantAddressResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantAddressResult);
            var merchantAddress = getMerchantAddressResult.Data;

            merchantAddress.AddressLine1 = domainEvent.AddressLine1;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchantAddress(MerchantAddressLine2UpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getMerchantAddressResult = await context.LoadMerchantAddress(domainEvent, cancellationToken);
            if (getMerchantAddressResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantAddressResult);
            var merchantAddress = getMerchantAddressResult.Data;

            merchantAddress.AddressLine2 = domainEvent.AddressLine2;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchantAddress(MerchantAddressLine3UpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getMerchantAddressResult = await context.LoadMerchantAddress(domainEvent, cancellationToken);
            if (getMerchantAddressResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantAddressResult);
            var merchantAddress = getMerchantAddressResult.Data;

            merchantAddress.AddressLine3 = domainEvent.AddressLine3;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchantAddress(MerchantAddressLine4UpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getMerchantAddressResult = await context.LoadMerchantAddress(domainEvent, cancellationToken);
            if (getMerchantAddressResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantAddressResult);
            var merchantAddress = getMerchantAddressResult.Data;

            merchantAddress.AddressLine4 = domainEvent.AddressLine4;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchantAddress(MerchantCountyUpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getMerchantAddressResult = await context.LoadMerchantAddress(domainEvent, cancellationToken);
            if (getMerchantAddressResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantAddressResult);
            var merchantAddress = getMerchantAddressResult.Data;

            merchantAddress.Country = domainEvent.Country;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchantAddress(MerchantRegionUpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getMerchantAddressResult = await context.LoadMerchantAddress(domainEvent, cancellationToken);
            if (getMerchantAddressResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantAddressResult);
            var merchantAddress = getMerchantAddressResult.Data;

            merchantAddress.Region = domainEvent.Region;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchantAddress(MerchantTownUpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getMerchantAddressResult = await context.LoadMerchantAddress(domainEvent, cancellationToken);
            if (getMerchantAddressResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantAddressResult);
            var merchantAddress = getMerchantAddressResult.Data;

            merchantAddress.Town = domainEvent.Town;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchantAddress(MerchantPostalCodeUpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getMerchantAddressResult = await context.LoadMerchantAddress(domainEvent, cancellationToken);
            if (getMerchantAddressResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantAddressResult);
            var merchantAddress = getMerchantAddressResult.Data;

            merchantAddress.PostalCode = domainEvent.PostalCode;

            return await context.SaveChangesAsync(cancellationToken);
        }

        //public async Task<Result> AddGeneratedVoucher(VoucherDomainEvents.VoucherGeneratedEvent domainEvent,
        //                                              CancellationToken cancellationToken)
        //{
        //    EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

        //    Voucher voucher = new Voucher
        //    {
        //        ExpiryDateTime = domainEvent.ExpiryDateTime,
        //        ExpiryDate = domainEvent.ExpiryDateTime.Date,
        //        IsGenerated = true,
        //        IsIssued = false,
        //        OperatorIdentifier = domainEvent.OperatorId.ToString(),
        //        Value = domainEvent.Value,
        //        VoucherCode = domainEvent.VoucherCode,
        //        VoucherId = domainEvent.VoucherId,
        //        TransactionId = domainEvent.TransactionId,
        //        GenerateDateTime = domainEvent.GeneratedDateTime,
        //        GenerateDate = domainEvent.GeneratedDateTime.Date
        //    };

        //    await context.Vouchers.AddAsync(voucher, cancellationToken);

        //    return await context.SaveChangesAsync(cancellationToken);
        //}

        public async Task<Result> UpdateMerchantContact(MerchantContactNameUpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getMerchantContactResult = await context.LoadMerchantContact(domainEvent, cancellationToken);
            if (getMerchantContactResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantContactResult);
            var merchantContact = getMerchantContactResult.Data;

            merchantContact.Name = domainEvent.ContactName;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchantContact(MerchantContactEmailAddressUpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getMerchantContactResult = await context.LoadMerchantContact(domainEvent, cancellationToken);
            if (getMerchantContactResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantContactResult);
            var merchantContact = getMerchantContactResult.Data;

            merchantContact.EmailAddress = domainEvent.ContactEmailAddress;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchantContact(MerchantContactPhoneNumberUpdatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getMerchantContactResult = await context.LoadMerchantContact(domainEvent, cancellationToken);
            if (getMerchantContactResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantContactResult);
            var merchantContact = getMerchantContactResult.Data;

            merchantContact.PhoneNumber = domainEvent.ContactPhoneNumber;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddContractToMerchant(ContractAddedToMerchantEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            MerchantContract merchantContract = new MerchantContract
            {
                MerchantId = domainEvent.MerchantId,
                ContractId = domainEvent.ContractId
            };

            await context.MerchantContracts.AddAsync(merchantContract, cancellationToken);
            return await context.SaveChangesAsync(cancellationToken);
        }
        public async Task<Result> UpdateMerchant(MerchantReferenceAllocatedEvent domainEvent,
                                                 CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Result<Merchant> merchantResult = await context.LoadMerchant(domainEvent, cancellationToken);
            if (merchantResult.IsFailed)
                return ResultHelpers.CreateFailure(merchantResult);
            var merchant = merchantResult.Data;

            merchant.Reference = domainEvent.MerchantReference;
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result<List<MerchantModel>>> GetMerchants(Guid estateId,
                                                                  CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(estateId);

            List<Merchant> merchants = await (from m in context.Merchants where m.EstateId == estateId select m).ToListAsync(cancellationToken);
            List<MerchantAddress> merchantAddresses = await (from a in context.MerchantAddresses where merchants.Select(m => m.MerchantId).Contains(a.MerchantId) select a).ToListAsync(cancellationToken);
            List<MerchantContact> merchantContacts = await (from c in context.MerchantContacts where merchants.Select(m => m.MerchantId).Contains(c.MerchantId) select c).ToListAsync(cancellationToken);
            List<MerchantOperator> merchantOperators = await (from o in context.MerchantOperators where merchants.Select(m => m.MerchantId).Contains(o.MerchantId) select o).ToListAsync(cancellationToken);
            List<MerchantSecurityUser> merchantSecurityUsers = await (from u in context.MerchantSecurityUsers where merchants.Select(m => m.MerchantId).Contains(u.MerchantId) select u).ToListAsync(cancellationToken);
            List<MerchantDevice> merchantDevices = await (from d in context.MerchantDevices where merchants.Select(m => m.MerchantId).Contains(d.MerchantId) select d).ToListAsync(cancellationToken);

            if (merchants.Any() == false)
            {
                return Result.NotFound($"No merchants found for estate {estateId}");
            }

            List<MerchantModel> models = new List<MerchantModel>();

            foreach (Merchant m in merchants)
            {
                List<MerchantAddress> a = merchantAddresses.Where(ma => ma.MerchantId == m.MerchantId).ToList();
                List<MerchantContact> c = merchantContacts.Where(mc => mc.MerchantId == m.MerchantId).ToList();
                List<MerchantOperator> o = merchantOperators.Where(mo => mo.MerchantId == m.MerchantId).ToList();
                List<MerchantSecurityUser> u = merchantSecurityUsers.Where(msu => msu.MerchantId == m.MerchantId).ToList();
                List<MerchantDevice> d = merchantDevices.Where(ma => ma.MerchantId == m.MerchantId).ToList();

                models.Add(ModelFactory.ConvertFrom(estateId, m, a, c, o, d, u));
            }

            return Result.Success(models);
        }

        public async Task<Result<List<ContractModel>>> GetMerchantContracts(Guid estateId,
                                                                            Guid merchantId,
                                                                            CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(estateId);

            var x = await (from c in context.Contracts
                           join cp in context.ContractProducts on c.ContractId equals cp.ContractId
                           join eo in context.Operators on c.OperatorId equals eo.OperatorId
                           join m in context.Merchants on c.EstateId equals m.EstateId
                           join e in context.Estates on c.EstateId equals e.EstateId
                           join mc in context.MerchantContracts on new { c.ContractId, m.MerchantId } equals new { mc.ContractId, mc.MerchantId }
                           where m.MerchantId == merchantId && e.EstateId == estateId
                           select new
                           {
                               Contract = c,
                               Product = cp,
                               Operator = eo
                           }).ToListAsync(cancellationToken);

            List<ContractModel> contracts = new List<ContractModel>();

            foreach (var test in x)
            {
                // attempt to find the contract
                ContractModel contract = contracts.SingleOrDefault(c => c.ContractId == test.Contract.ContractId);

                if (contract == null)
                {
                    // create the contract
                    contract = new ContractModel
                    {
                        OperatorId = test.Contract.OperatorId,
                        OperatorName = test.Operator.Name,
                        Products = new List<Product>(),
                        Description = test.Contract.Description,
                        IsCreated = true,
                        ContractId = test.Contract.ContractId,
                        ContractReportingId = test.Contract.ContractReportingId,
                        EstateId = estateId
                    };

                    contracts.Add(contract);
                }

                // Now add the product if not already added
                Boolean productFound = contract.Products.Any(p => p.ContractProductId == test.Product.ContractProductId);

                if (productFound == false)
                {
                    // Not already there so need to add it
                    contract.Products.Add(new Product
                    {
                        ContractProductId = test.Product.ContractProductId,
                        ContractProductReportingId = test.Product.ContractProductReportingId,
                        TransactionFees = null,
                        Value = test.Product.Value,
                        Name = test.Product.ProductName,
                        DisplayText = test.Product.DisplayText
                    });
                }
            }

            return Result.Success(contracts);
        }

        public async Task<Result<List<ContractModel>>> GetContracts(Guid estateId,
                                                            CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(estateId);

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
            EstateManagementContext context = await this.GetContext(estateId);

            Database.Entities.Estate? estate = await context.Estates.SingleOrDefaultAsync(e => e.EstateId == estateId, cancellationToken);

            if (estate == null)
            {
                return Result.NotFound($"No estate found in read model with Id [{estateId}]");
            }

            List<EstateSecurityUser> estateSecurityUsers = await context.EstateSecurityUsers.Where(esu => esu.EstateId == estate.EstateId).ToListAsync(cancellationToken);
            List<Database.Entities.Operator> operators = await context.Operators.Where(eo => eo.EstateId == estate.EstateId).ToListAsync(cancellationToken);

            return Result.Success(ModelFactory.ConvertFrom(estate, estateSecurityUsers, operators));
        }

        private async Task<EstateManagementContext> GetContext(Guid estateId)
        {
            ResolvedDbContext<EstateManagementContext>? resolvedContext = this.Resolver.Resolve(EstateManagementDatabaseName, estateId.ToString());
            return resolvedContext.Context;
        }

        public async Task<Result> AddContractProductTransactionFee(ContractDomainEvents.TransactionFeeForProductAddedToContractEvent domainEvent,
                                                                   CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddEstate(EstateDomainEvents.EstateCreatedEvent domainEvent,
                                            CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            // Add the estate to the read model
            Database.Entities.Estate estate = new Database.Entities.Estate
            {
                EstateId = domainEvent.EstateId,
                Name = domainEvent.EstateName,
                Reference = String.Empty
            };
            await context.Estates.AddAsync(estate, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddEstateSecurityUser(EstateDomainEvents.SecurityUserAddedToEstateEvent domainEvent,
                                                        CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            EstateSecurityUser estateSecurityUser = new EstateSecurityUser
            {
                EstateId = domainEvent.EstateId,
                EmailAddress = domainEvent.EmailAddress,
                SecurityUserId = domainEvent.SecurityUserId,
                CreatedDateTime = domainEvent.EventTimestamp.DateTime
            };

            await context.EstateSecurityUsers.AddAsync(estateSecurityUser, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddProductDetailsToTransaction(TransactionDomainEvents.ProductDetailsAddedToTransactionEvent domainEvent,
                                                                 CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            transaction.TransactionSource = domainEvent.TransactionSource;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CompleteReconciliation(ReconciliationDomainEvents.ReconciliationHasCompletedEvent domainEvent,
                                                         CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getReconcilationResult = await context.LoadReconcilation(domainEvent, cancellationToken);
            if (getReconcilationResult.IsFailed)
                return ResultHelpers.CreateFailure(getReconcilationResult);
            var reconciliation = getReconcilationResult.Data;

            reconciliation.IsCompleted = true;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CompleteTransaction(TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent,
                                                      CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            transaction.IsCompleted = true;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> CreateFloat(FloatDomainEvents.FloatCreatedForContractProductEvent domainEvent,
                                              CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> CreateFloatActivity(FloatDomainEvents.FloatCreditPurchasedEvent domainEvent,
                                                      CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Logger.LogInformation($"About to run migrations on Read Model database for estate [{domainEvent.EstateId}]");

            // Ensure the db is at the latest version
            await context.MigrateAsync(cancellationToken);

            Logger.LogWarning($"Read Model database for estate [{domainEvent.EstateId}] migrated to latest version");
            return Result.Success();
        }

        public async Task<Result> CreateSettlement(SettlementDomainEvents.SettlementCreatedForDateEvent domainEvent,
                                                   CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Result<ContractProductTransactionFee> loadContractProductTransactionFeeResult = await context.LoadContractProductTransactionFee(domainEvent, cancellationToken);
            if (loadContractProductTransactionFeeResult.IsFailed)
                return ResultHelpers.CreateFailure(loadContractProductTransactionFeeResult);

            ContractProductTransactionFee transactionFee = loadContractProductTransactionFeeResult.Data;

            transactionFee.IsEnabled = false;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> MarkMerchantFeeAsSettled(SettlementDomainEvents.MerchantFeeSettledEvent domainEvent,
                                                           CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getSettlementResult = await context.LoadSettlement(domainEvent, cancellationToken);
            if (getSettlementResult.IsFailed)
                return ResultHelpers.CreateFailure(getSettlementResult);
            var settlement = getSettlementResult.Data;

            settlement.IsCompleted = true;
            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> MarkSettlementAsProcessingStarted(SettlementDomainEvents.SettlementProcessingStartedEvent domainEvent,
                                                                    CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> RecordTransactionAdditionalResponseData(TransactionDomainEvents.AdditionalResponseDataRecordedEvent domainEvent,
                                                                          CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> SetTransactionAmount(TransactionDomainEvents.AdditionalRequestDataRecordedEvent domainEvent,
                                                       CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Database.Entities.Reconciliation reconciliation = new Database.Entities.Reconciliation
            {
                MerchantId = domainEvent.MerchantId,
                TransactionDate = domainEvent.TransactionDateTime.Date,
                TransactionDateTime = domainEvent.TransactionDateTime,
                TransactionTime = domainEvent.TransactionDateTime.TimeOfDay,
                TransactionId = domainEvent.TransactionId,
            };

            await context.Reconciliations.AddAsync(reconciliation, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> StartTransaction(TransactionDomainEvents.TransactionHasStartedEvent domainEvent,
                                                   CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> UpdateEstate(EstateDomainEvents.EstateReferenceAllocatedEvent domainEvent,
                                               CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getEstateResult = await context.LoadEstate(domainEvent, cancellationToken);
            if (getEstateResult.IsFailed)
                return ResultHelpers.CreateFailure(getEstateResult);

            var estate = getEstateResult.Data;
            estate.Reference = domainEvent.EstateReference;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateMerchant(TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent,
                                                 CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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

            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            transaction.IsAuthorised = false;
            transaction.ResponseCode = domainEvent.ResponseCode;
            transaction.ResponseMessage = domainEvent.ResponseMessage;

            return await context.SaveChangesAsync(cancellationToken); ;
        }

        //public async Task<Result> UpdateVoucherIssueDetails(VoucherDomainEvents.VoucherIssuedEvent domainEvent,
        //                                                    CancellationToken cancellationToken) {
        //    EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

        //    var getVoucherResult = await context.LoadVoucher(domainEvent, cancellationToken);
        //    if (getVoucherResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getVoucherResult);
        //    var voucher = getVoucherResult.Data;
        //    voucher.IsIssued = true;
        //    voucher.RecipientEmail = domainEvent.RecipientEmail;
        //    voucher.RecipientMobile = domainEvent.RecipientMobile;
        //    voucher.IssuedDateTime = domainEvent.IssuedDateTime;
        //    voucher.IssuedDate = domainEvent.IssuedDateTime.Date;

        //    return await context.SaveChangesAsync(cancellationToken);
        //}

        //public async Task<Result> UpdateVoucherRedemptionDetails(VoucherDomainEvents.VoucherFullyRedeemedEvent domainEvent,
        //                                                         CancellationToken cancellationToken) {
        //    EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

        //    var getVoucherResult = await context.LoadVoucher(domainEvent, cancellationToken);
        //    if (getVoucherResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getVoucherResult);
        //    var voucher = getVoucherResult.Data;

        //    voucher.IsRedeemed = true;
        //    voucher.RedeemedDateTime = domainEvent.RedeemedDateTime;
        //    voucher.RedeemedDate = domainEvent.RedeemedDateTime.Date;

        //    return await context.SaveChangesAsync(cancellationToken);
        //}

        public async Task<Result<List<Models.Operator.Operator>>> GetOperators(Guid estateId, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(estateId);

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
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Result<Database.Entities.Operator> operatorResult = await context.LoadOperator(domainEvent, cancellationToken);
            if (operatorResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorResult);
            Database.Entities.Operator @operator = operatorResult.Data;
            @operator.Name = domainEvent.Name;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateOperator(OperatorDomainEvents.OperatorRequireCustomMerchantNumberChangedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Result<Database.Entities.Operator> operatorResult = await context.LoadOperator(domainEvent, cancellationToken);
            if (operatorResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorResult);
            Database.Entities.Operator @operator = operatorResult.Data;

            @operator.RequireCustomMerchantNumber = domainEvent.RequireCustomMerchantNumber;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateOperator(OperatorDomainEvents.OperatorRequireCustomTerminalNumberChangedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Result<Database.Entities.Operator> operatorResult = await context.LoadOperator(domainEvent, cancellationToken);
            if (operatorResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorResult);
            Database.Entities.Operator @operator = operatorResult.Data;

            @operator.RequireCustomTerminalNumber = domainEvent.RequireCustomTerminalNumber;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddOperator(OperatorDomainEvents.OperatorCreatedEvent domainEvent, CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Database.Entities.Operator @operator = new Database.Entities.Operator
            {
                RequireCustomTerminalNumber = domainEvent.RequireCustomTerminalNumber,
                OperatorId = domainEvent.OperatorId,
                Name = domainEvent.Name,
                RequireCustomMerchantNumber = domainEvent.RequireCustomMerchantNumber,
                EstateId = domainEvent.EstateId
            };

            await context.Operators.AddAsync(@operator, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddContract(ContractDomainEvents.ContractCreatedEvent domainEvent,
                                              CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            Contract contract = new Contract
            {
                EstateId = domainEvent.EstateId,
                OperatorId = domainEvent.OperatorId,
                ContractId = domainEvent.ContractId,
                Description = domainEvent.Description
            };

            await context.Contracts.AddAsync(contract, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddContractProduct(ContractDomainEvents.VariableValueProductAddedToContractEvent domainEvent,
                                                     CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddContractProduct(ContractDomainEvents.FixedValueProductAddedToContractEvent domainEvent,
                                                     CancellationToken cancellationToken) {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

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

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> MarkStatementAsGenerated(StatementGeneratedEvent domainEvent,
                                                           CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getLoadStatementHeaderResult = await context.LoadStatementHeader(domainEvent, cancellationToken);
            if (getLoadStatementHeaderResult.IsFailed)
                return ResultHelpers.CreateFailure(getLoadStatementHeaderResult);
            var statementHeader = getLoadStatementHeaderResult.Data;

            statementHeader.StatementGeneratedDate = domainEvent.DateGenerated;
            return await context.SaveChangesAsync(cancellationToken);
        }

        // TODO@ Add this back in
        //public async Task<Result> AddSettledFeeToStatement(SettledFeeAddedToStatementEvent domainEvent,
        //                                                   CancellationToken cancellationToken)
        //{
        //    EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

        //    // Find the corresponding transaction
        //    var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
        //    if (getTransactionResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getTransactionResult);
        //    var transaction = getTransactionResult.Data;

        //    Result<Operator> operatorResult = await context.LoadOperator(transaction.OperatorId, cancellationToken);
        //    if (operatorResult.IsFailed)
        //        return ResultHelpers.CreateFailure(operatorResult);
        //    var @operator = operatorResult.Data;

        //    StatementLine line = new StatementLine
        //    {
        //        StatementId = domainEvent.MerchantStatementId,
        //        ActivityDateTime = domainEvent.SettledDateTime,
        //        ActivityDate = domainEvent.SettledDateTime.Date,
        //        ActivityDescription = $"{@operator.Name} Transaction Fee",
        //        ActivityType = 2, // Transaction Fee
        //        TransactionId = domainEvent.TransactionId,
        //        InAmount = domainEvent.SettledValue
        //    };

        //    await context.StatementLines.AddAsync(line, cancellationToken);

        //    return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        //}

        public async Task<Result> CreateStatement(StatementCreatedEvent domainEvent,
                                                  CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            StatementHeader header = new StatementHeader
            {
                MerchantId = domainEvent.MerchantId,
                StatementCreatedDateTime = domainEvent.StatementDate,
                StatementCreatedDate = domainEvent.StatementDate.Date,
                StatementId = domainEvent.MerchantStatementId
            };

            await context.StatementHeaders.AddAsync(header, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        // TODO@ Add this back in
        //public async Task<Result> AddTransactionToStatement(TransactionAddedToStatementEvent domainEvent,
        //                                                    CancellationToken cancellationToken)
        //{
        //    EstateManagementGenericContext context = await this.GetContextFromDomainEvent(domainEvent, cancellationToken);

        //    // Find the corresponding transaction
        //    Result<Transaction> transactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
        //    if (transactionResult.IsFailed)
        //        return ResultHelpers.CreateFailure(transactionResult);

        //    Transaction transaction = transactionResult.Data;

        //    Result<Operator> operatorResult = await context.LoadOperator(transaction.OperatorId, cancellationToken);
        //    if (operatorResult.IsFailed)
        //        return ResultHelpers.CreateFailure(operatorResult);
        //    Operator @operator = operatorResult.Data;

        //    StatementLine line = new StatementLine
        //    {
        //        StatementId = domainEvent.MerchantStatementId,
        //        ActivityDateTime = domainEvent.TransactionDateTime,
        //        ActivityDate = domainEvent.TransactionDateTime.Date,
        //        ActivityDescription = $"{@operator.Name} Transaction",
        //        ActivityType = 1, // Transaction
        //        TransactionId = domainEvent.TransactionId,
        //        OutAmount = domainEvent.TransactionValue
        //    };

        //    await context.StatementLines.AddAsync(line, cancellationToken);

        //    return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        //}

        public async Task<Result> AddFile(FileCreatedEvent domainEvent,
                              CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            File file = new File
            {
                EstateId = domainEvent.EstateId,
                MerchantId = domainEvent.MerchantId,
                FileImportLogId = domainEvent.FileImportLogId,
                UserId = domainEvent.UserId,
                FileId = domainEvent.FileId,
                FileProfileId = domainEvent.FileProfileId,
                FileLocation = domainEvent.FileLocation,
                FileReceivedDateTime = domainEvent.FileReceivedDateTime,
                FileReceivedDate = domainEvent.FileReceivedDateTime.Date
            };

            await context.Files.AddAsync(file, cancellationToken);

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AddFileImportLog(ImportLogCreatedEvent domainEvent,
                                           CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            FileImportLog fileImportLog = new FileImportLog
            {
                EstateId = domainEvent.EstateId,
                FileImportLogId = domainEvent.FileImportLogId,
                ImportLogDateTime = domainEvent.ImportLogDateTime,
                ImportLogDate = domainEvent.ImportLogDateTime.Date
            };

            await context.FileImportLogs.AddAsync(fileImportLog, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddFileLineToFile(FileLineAddedEvent domainEvent,
                                            CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            FileLine fileLine = new FileLine
            {
                FileId = domainEvent.FileId,
                LineNumber = domainEvent.LineNumber,
                FileLineData = domainEvent.FileLine,
                Status = "P" // Pending
            };

            await context.FileLines.AddAsync(fileLine, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddFileToImportLog(FileAddedToImportLogEvent domainEvent,
                                             CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            FileImportLogFile fileImportLogFile = new FileImportLogFile
            {
                MerchantId = domainEvent.MerchantId,
                FileImportLogId = domainEvent.FileImportLogId,
                FileId = domainEvent.FileId,
                FilePath = domainEvent.FilePath,
                FileProfileId = domainEvent.FileProfileId,
                FileUploadedDateTime = domainEvent.FileUploadedDateTime,
                FileUploadedDate = domainEvent.FileUploadedDateTime.Date,
                OriginalFileName = domainEvent.OriginalFileName,
                UserId = domainEvent.UserId
            };

            await context.FileImportLogFiles.AddAsync(fileImportLogFile, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> UpdateFileAsComplete(FileProcessingCompletedEvent domainEvent,
                                           CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var getFileResult = await context.LoadFile(domainEvent, cancellationToken);
            if (getFileResult.IsFailed)
                return ResultHelpers.CreateFailure(getFileResult);

            var file = getFileResult.Data;
            file.IsCompleted = true;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateFileLine(FileLineProcessingSuccessfulEvent domainEvent,
                                         CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            return await this.UpdateFileLineStatus(context,
                                            domainEvent.FileId,
                                            domainEvent.LineNumber,
                                            domainEvent.TransactionId,
                                            "S",
                                            cancellationToken);
        }

        private async Task<Result> UpdateFileLineStatus(EstateManagementContext context,
                                                        Guid fileId,
                                                        Int32 lineNumber,
                                                        Guid transactionId,
                                                        String newStatus,
                                                        CancellationToken cancellationToken)
        {
            FileLine fileLine = await context.FileLines.SingleOrDefaultAsync(f => f.FileId == fileId && f.LineNumber == lineNumber, cancellationToken: cancellationToken);

            if (fileLine == null)
            {
                return Result.NotFound($"FileLine number {lineNumber} in File Id {fileId} not found");
            }

            fileLine.Status = newStatus;
            fileLine.TransactionId = transactionId;

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateFileLine(FileLineProcessingFailedEvent domainEvent,
                                         CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            return await this.UpdateFileLineStatus(context,
                domainEvent.FileId,
                domainEvent.LineNumber,
                domainEvent.TransactionId,
                                            "F",
                                            cancellationToken);
        }

        public async Task<Result> UpdateFileLine(FileLineProcessingIgnoredEvent domainEvent,
                                         CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            return await this.UpdateFileLineStatus(context,
                                            domainEvent.FileId,
                                            domainEvent.LineNumber,
                                            Guid.Empty,
                                            "I",
                                            cancellationToken);
        }

        public async Task<Result> AddPendingMerchantFeeToSettlement(SettlementDomainEvents.MerchantFeeAddedPendingSettlementEvent domainEvent,
                                                                    CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            MerchantSettlementFee merchantSettlementFee = new MerchantSettlementFee
            {
                SettlementId = domainEvent.SettlementId,
                CalculatedValue = domainEvent.CalculatedValue,
                FeeCalculatedDateTime = domainEvent.FeeCalculatedDateTime,
                ContractProductTransactionFeeId = domainEvent.FeeId,
                FeeValue = domainEvent.FeeValue,
                IsSettled = false,
                MerchantId = domainEvent.MerchantId,
                TransactionId = domainEvent.TransactionId
            };

            await context.MerchantSettlementFees.AddAsync(merchantSettlementFee, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result<SettlementModel>> GetSettlement(Guid estateId,
                                                             Guid merchantId,
                                                             Guid settlementId,
                                                             CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(estateId);

            IQueryable<SettlementView> query = context.SettlementsView.Where(t => t.EstateId == estateId && t.SettlementId == settlementId
                                                                             && t.MerchantId == merchantId).AsQueryable();

            var result = query.AsEnumerable().GroupBy(t => new {
                t.SettlementId,
                t.SettlementDate,
                t.IsCompleted
            }).SingleOrDefault();

            if (result == null)
                return Result.NotFound($"Settlement with Id {settlementId} not found");

            SettlementModel model = new SettlementModel
            {
                SettlementDate = result.Key.SettlementDate,
                SettlementId = result.Key.SettlementId,
                NumberOfFeesSettled = result.Count(),
                ValueOfFeesSettled = result.Sum(x => x.CalculatedValue),
                IsCompleted = result.Key.IsCompleted
            };

            result.ToList().ForEach(f => model.SettlementFees.Add(new SettlementFeeModel
            {
                SettlementDate = f.SettlementDate,
                SettlementId = f.SettlementId,
                CalculatedValue = f.CalculatedValue,
                MerchantId = f.MerchantId,
                MerchantName = f.MerchantName,
                FeeDescription = f.FeeDescription,
                IsSettled = f.IsSettled,
                TransactionId = f.TransactionId,
                OperatorIdentifier = f.OperatorIdentifier
            }));

            return Result.Success(model);
        }

        public async Task<Result<List<SettlementModel>>> GetSettlements(Guid estateId,
                                                                        Guid? merchantId,
                                                                        String startDate,
                                                                        String endDate,
                                                                        CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(estateId);

            DateTime queryStartDate = DateTime.ParseExact(startDate, "yyyyMMdd", null);
            DateTime queryEndDate = DateTime.ParseExact(endDate, "yyyyMMdd", null);

            IQueryable<SettlementView> query = context.SettlementsView.Where(t => t.EstateId == estateId &&
                                                                                  t.SettlementDate >= queryStartDate.Date && t.SettlementDate <= queryEndDate.Date)
                                                      .AsQueryable();

            if (merchantId.HasValue)
            {
                query = query.Where(t => t.MerchantId == merchantId);
            }

            List<SettlementModel> result = await query.GroupBy(t => new
            {
                t.SettlementId,
                t.SettlementDate,
                t.IsCompleted
            }).Select(t => new SettlementModel
            {
                SettlementId = t.Key.SettlementId,
                SettlementDate = t.Key.SettlementDate,
                NumberOfFeesSettled = t.Count(),
                ValueOfFeesSettled = t.Sum(x => x.CalculatedValue),
                IsCompleted = t.Key.IsCompleted
            }).OrderByDescending(t => t.SettlementDate)
                                                      .ToListAsync(cancellationToken);

            return Result.Success(result);
        }

        public async Task<Result> RecordTransactionTimings(TransactionDomainEvents.TransactionTimingsAddedToTransactionEvent domainEvent,
                                                           CancellationToken cancellationToken) {
            // Calculate the timings for the transaction
            TimeSpan totalTime = domainEvent.TransactionCompletedDateTime.Subtract(domainEvent.TransactionStartedDateTime);
            // Calculate the timings for the operator communications
            TimeSpan operatorCommunicationsTime = domainEvent.OperatorCommunicationsCompletedEvent?.Subtract(domainEvent.OperatorCommunicationsStartedEvent ?? DateTime.MinValue) ?? TimeSpan.Zero;
            // Calculate the timings for the transaction without operator timings
            TimeSpan transactionTime = totalTime.Subtract(operatorCommunicationsTime);

            // Load this information to the database
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            TransactionTimings timings = new() {
                TransactionStartedDateTime = domainEvent.TransactionStartedDateTime,
                TransactionCompletedDateTime = domainEvent.TransactionCompletedDateTime,
                OperatorCommunicationsCompletedDateTime = domainEvent.OperatorCommunicationsCompletedEvent,
                OperatorCommunicationsStartedDateTime = domainEvent.OperatorCommunicationsStartedEvent,
                TransactionId = domainEvent.TransactionId,
                OperatorCommunicationsDurationInMilliseconds = operatorCommunicationsTime.TotalMilliseconds,
                TotalTransactionInMilliseconds = totalTime.TotalMilliseconds,
                TransactionProcessingDurationInMilliseconds = transactionTime.TotalMilliseconds
            };
            await context.TransactionTimings.AddAsync(timings, cancellationToken);
            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddTransactionToStatement(MerchantStatementForDateDomainEvents.TransactionAddedToStatementForDateEvent domainEvent,
                                                            CancellationToken cancellationToken) {
            // Load this information to the database
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            // Find the corresponding transaction
            Result<Transaction> transactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (transactionResult.IsFailed)
                return ResultHelpers.CreateFailure(transactionResult);

            Transaction transaction = transactionResult.Data;

            Result<Operator> operatorResult = await context.LoadOperator(transaction.OperatorId, cancellationToken);
            if (operatorResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorResult);
            Operator @operator = operatorResult.Data;

            StatementLine line = new StatementLine
            {
                StatementId = domainEvent.MerchantStatementId,
                ActivityDateTime = domainEvent.TransactionDateTime,
                ActivityDate = domainEvent.TransactionDateTime.Date,
                ActivityDescription = $"{@operator.Name} Transaction",
                ActivityType = 1, // Transaction
                TransactionId = domainEvent.TransactionId,
                OutAmount = domainEvent.TransactionValue
            };

            await context.StatementLines.AddAsync(line, cancellationToken);
            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddSettledFeeToStatement(MerchantStatementForDateDomainEvents.SettledFeeAddedToStatementForDateEvent domainEvent,
                                                           CancellationToken cancellationToken)
        {
            // Load this information to the database
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            // Find the corresponding transaction
            var getTransactionResult = await context.LoadTransaction(domainEvent, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);
            var transaction = getTransactionResult.Data;

            Result<Operator> operatorResult = await context.LoadOperator(transaction.OperatorId, cancellationToken);
            if (operatorResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorResult);
            var @operator = operatorResult.Data;

            StatementLine line = new StatementLine
            {
                StatementId = domainEvent.MerchantStatementId,
                ActivityDateTime = domainEvent.SettledDateTime,
                ActivityDate = domainEvent.SettledDateTime.Date,
                ActivityDescription = $"{@operator.Name} Transaction Fee",
                ActivityType = 2, // Transaction Fee
                TransactionId = domainEvent.TransactionId,
                InAmount = domainEvent.SettledValue
            };

            await context.StatementLines.AddAsync(line, cancellationToken);

            return await context.SaveChangesWithDuplicateHandling(cancellationToken);
        }

        public async Task<Result> AddEstateOperator(EstateDomainEvents.OperatorAddedToEstateEvent domainEvent,
                                                    CancellationToken cancellationToken) {
            // Load this information to the database
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var estateOperator = await context.EstateOperators.SingleOrDefaultAsync(eo => eo.EstateId == domainEvent.EstateId && eo.OperatorId == domainEvent.OperatorId, cancellationToken);

            if (estateOperator == null) {
                await context.EstateOperators.AddAsync(new EstateOperator { EstateId = domainEvent.EstateId, OperatorId = domainEvent.OperatorId }, cancellationToken);
            }
            else {
                // Re-enable the record
                estateOperator.IsDeleted = null;
            }

            return await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> RemoveOperatorFromEstate(EstateDomainEvents.OperatorRemovedFromEstateEvent domainEvent,
                                                           CancellationToken cancellationToken) {
            // Load this information to the database
            EstateManagementContext context = await this.GetContext(domainEvent.EstateId);

            var estateOperator = await context.EstateOperators.SingleOrDefaultAsync(eo => eo.EstateId == domainEvent.EstateId && eo.OperatorId == domainEvent.OperatorId, cancellationToken);

            if (estateOperator == null) {
                await context.EstateOperators.AddAsync(new EstateOperator { EstateId = domainEvent.EstateId, OperatorId = domainEvent.OperatorId, IsDeleted = true}, cancellationToken);
            }
            else
            {
                // delete the record
                estateOperator.IsDeleted = true;
            }

            return await context.SaveChangesAsync(cancellationToken);
        }
    }
}
