using Imposter.Abstractions;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.Logger;
using TransactionProcessing.Shared.Serialisation;
using Shouldly;
using TransactionProcessing.UnionResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Estate;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Repository;
using TransactionProcessor.Testing;
using Xunit;
using Contract = TransactionProcessor.Models.Contract.Contract;
using MerchantScheduleModel = TransactionProcessor.Models.MerchantSchedule.MerchantSchedule;
using Operator = TransactionProcessor.Models.Operator.Operator;

namespace TransactionProcessor.BusinessLogic.Tests.Manager
{
    public class TransactionProcessorManagerTests
    {
        private readonly ITransactionProcessorReadModelRepositoryImposter TransactionProcessorReadModelRepository;
        private readonly IAggregateServiceImposter AggregateService;
        
        private readonly TransactionProcessorManager TransactionProcessorManager;

        public TransactionProcessorManagerTests()
        {
            Logger.Initialise(new NullLogger());
            this.TransactionProcessorReadModelRepository = new ITransactionProcessorReadModelRepositoryImposter();
            StringSerialiser.Initialise(new TransactionProcessing.Shared.Serialisation.SystemTextJsonSerializer(new System.Text.Json.JsonSerializerOptions()));
            this.AggregateService = new IAggregateServiceImposter();
            
            this.TransactionProcessorManager = new TransactionProcessorManager(this.TransactionProcessorReadModelRepository.Instance(), this.AggregateService.Instance());
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstates_EstatesAreReturned()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.CreatedEstateAggregate()));
            this.TransactionProcessorReadModelRepository.GetEstate(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.EstateModel));

            Result<List<Estate>> getEstatesResult = await this.TransactionProcessorManager.GetEstates(TestData.EstateId, TestContext.Current.CancellationToken);
            getEstatesResult.IsSuccess().ShouldBeTrue();
            List<Estate> estateModels = getEstatesResult.Data;
            estateModels.ShouldNotBeNull();
            estateModels.ShouldHaveSingleItem();
            estateModels.Single().EstateId.ShouldBe(TestData.EstateModel.EstateId);
            estateModels.Single().Name.ShouldBe(TestData.EstateModel.Name);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstates_RepoCallFails_ResultFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.CreatedEstateAggregate()));
            this.TransactionProcessorReadModelRepository.GetEstate(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));

            Result<List<Estate>> getEstatesResult = await this.TransactionProcessorManager.GetEstates(TestData.EstateId, TestContext.Current.CancellationToken);
            getEstatesResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstate_EstateIsReturned()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.EstateAggregateWithOperator()));
            this.TransactionProcessorReadModelRepository.GetEstate(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.EstateModel));
            this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), TestContext.Current.CancellationToken).ReturnsAsync(Results.Ok(TestData.Aggregates.CreatedOperatorAggregate()));

            Result<Estate> getEstateResult = await this.TransactionProcessorManager.GetEstate(TestData.EstateId, TestContext.Current.CancellationToken);
            getEstateResult.IsSuccess().ShouldBeTrue();
            Estate estateModel = getEstateResult.Data;
            estateModel.ShouldNotBeNull();
            estateModel.EstateId.ShouldBe(TestData.EstateModel.EstateId);
            estateModel.Name.ShouldBe(TestData.EstateModel.Name);
            estateModel.Operators.ShouldHaveSingleItem();
            estateModel.Operators.Single().OperatorId.ShouldBe(TestData.OperatorId);
            estateModel.Operators.Single().Name.ShouldBe(TestData.OperatorName);
            estateModel.Operators.Single().IsDeleted.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstate_InvalidEstateId_ErrorIsThrown()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.EmptyEstateAggregate));
            this.TransactionProcessorReadModelRepository.GetEstate(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.EstateModel));

            Result<Estate> getEstateResult = await this.TransactionProcessorManager.GetEstate(TestData.EstateId, TestContext.Current.CancellationToken);
            getEstateResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstate_GetLatestFailed_ErrorIsThrown()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));
            this.TransactionProcessorReadModelRepository.GetEstate(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.EstateModel));

            Result<Estate> getEstateResult = await this.TransactionProcessorManager.GetEstate(TestData.EstateId, TestContext.Current.CancellationToken);
            getEstateResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContract_ContractIsReturned()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant)));

            Result<Contract> getContractResult = await this.TransactionProcessorManager.GetContract(TestData.EstateId, TestData.ContractId, TestContext.Current.CancellationToken);
            getContractResult.IsSuccess().ShouldBeTrue();
            Contract contractModel = getContractResult.Data;

            contractModel.ShouldNotBeNull();
            contractModel.ContractId.ShouldBe(TestData.ContractId);
            contractModel.Description.ShouldBe(TestData.ContractDescription);
            contractModel.OperatorId.ShouldBe(TestData.OperatorId);
            contractModel.Products.ShouldNotBeNull();
            contractModel.Products.First().ContractProductId.ShouldBe(TestData.VariableContractProductId);
            contractModel.Products.First().TransactionFees.ShouldNotBeNull();
            contractModel.Products.First().TransactionFees.First().TransactionFeeId.ShouldBe(TestData.TransactionFeeId);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContract_ContractNotCreated_ErrorIsThrown()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.EmptyContractAggregate()));

            Result<Contract> getContractResult = await this.TransactionProcessorManager.GetContract(TestData.EstateId, TestData.ContractId, TestContext.Current.CancellationToken);
            getContractResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContract_GetLatestFails_ErrorIsThrown()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));

            Result<Contract> getContractResult = await this.TransactionProcessorManager.GetContract(TestData.EstateId, TestData.ContractId, TestContext.Current.CancellationToken);
            getContractResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContracts_ContractAreReturned()
        {
            this.TransactionProcessorReadModelRepository.GetContracts(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(new List<Contract>() { TestData.ContractModelWithProductsAndTransactionFees }));

            Result<List<Contract>> getContractsResult = await this.TransactionProcessorManager.GetContracts(TestData.EstateId, TestContext.Current.CancellationToken);
            getContractsResult.IsSuccess().ShouldBeTrue();
            List<Contract> contractModelList = getContractsResult.Data;
            contractModelList.ShouldNotBeNull();
            contractModelList.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContracts_RepoCallFails_ContractAreReturned()
        {
            this.TransactionProcessorReadModelRepository.GetContracts(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));

            Result<List<Contract>> getContractsResult = await this.TransactionProcessorManager.GetContracts(TestData.EstateId, TestContext.Current.CancellationToken);
            getContractsResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_TransactionFeesAreReturned()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant)));

            Result<List<ContractProductTransactionFee>> getTransactionFeesForProductResult = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.VariableContractProductId, TestContext.Current.CancellationToken);
            getTransactionFeesForProductResult.IsSuccess().ShouldBeTrue();
            List<ContractProductTransactionFee> transactionFees = getTransactionFeesForProductResult.Data;
            transactionFees.ShouldNotBeNull();
            transactionFees.ShouldHaveSingleItem();
            transactionFees.First().TransactionFeeId.ShouldBe(TestData.TransactionFeeId);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_ContractNotFound_ErrorThrown()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.EmptyContractAggregate()));

            Result<List<ContractProductTransactionFee>> result = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.FixedContractProductId, TestContext.Current.CancellationToken);
            result.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_ProductNotFound_ErrorThrown()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.CreatedContractAggregate()));

            Result<List<ContractProductTransactionFee>> result = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.FixedContractProductId, TestContext.Current.CancellationToken);
            result.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_GetLatestFails_ErrorThrown()
        {
            this.AggregateService.GetLatest<ContractAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));

            Result<List<ContractProductTransactionFee>> result = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.FixedContractProductId, TestContext.Current.CancellationToken);
            result.IsSuccess().ShouldBeFalse();
        }
        /*
        
        [Fact]
        public async Task EstateManagementManager_GetFileDetails_FileDetailsAreReturned()
        {
            this.EstateManagementRepository.GetFileDetails(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.FileModel));

            var getFileDetailsResult = await this.EstateManagementManager.GetFileDetails(TestData.EstateId, TestData.FileId, TestContext.Current.CancellationToken);
            getFileDetailsResult.IsSuccess().ShouldBeTrue();
            var fileDetails = getFileDetailsResult.Data;
            fileDetails.ShouldNotBeNull();
        }

        [Fact]
        public async Task EstateManagementManager_GetFileDetails_RepoCallFailed_FileDetailsAreReturned()
        {
            this.EstateManagementRepository.GetFileDetails(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));

            var getFileDetailsResult = await this.EstateManagementManager.GetFileDetails(TestData.EstateId, TestData.FileId, TestContext.Current.CancellationToken);
            getFileDetailsResult.IsSuccess().ShouldBeFalse();
        }
        */
        [Fact]
        public async Task TransactionProcessorManager_GetOperator_OperatorDetailsAreReturned()
        {
            this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.CreatedOperatorAggregate()));

            Result<Operator> getOperatorResult = await this.TransactionProcessorManager.GetOperator(TestData.EstateId, TestData.OperatorId, TestContext.Current.CancellationToken);
            getOperatorResult.IsSuccess().ShouldBeTrue();
            Operator operatorDetails = getOperatorResult.Data;
            operatorDetails.ShouldNotBeNull();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperator_OperatorNotCreated_ExceptionThrown()
        {
            this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.EmptyOperatorAggregate()));

            Result<Operator> getOperatorResult = await this.TransactionProcessorManager.GetOperator(TestData.EstateId, TestData.OperatorId, TestContext.Current.CancellationToken);
            getOperatorResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperator_GetLatestFails_ExceptionThrown()
        {
            this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));

            Result<Operator> getOperatorResult = await this.TransactionProcessorManager.GetOperator(TestData.EstateId, TestData.OperatorId, TestContext.Current.CancellationToken);
            getOperatorResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperators_OperatorDetailsAreReturned()
        {
            this.TransactionProcessorReadModelRepository.GetOperators(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(new List<Operator>{
                                                                                                                                                           TestData.OperatorModel
                                                                                                                                                       }));

            Result<List<Operator>> getOperatorsResult = await this.TransactionProcessorManager.GetOperators(TestData.EstateId, TestContext.Current.CancellationToken);
            getOperatorsResult.IsSuccess().ShouldBeTrue();
            List<Operator> operators = getOperatorsResult.Data;
            operators.ShouldNotBeNull();
            operators.ShouldHaveSingleItem();
            operators.Single().OperatorId.ShouldBe(TestData.OperatorId);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperators_EmptyList_ExceptionThrown()
        {
            this.TransactionProcessorReadModelRepository.GetOperators(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(new List<Operator>()));

            Result<List<Operator>> getOperatorsResult = await this.TransactionProcessorManager.GetOperators(TestData.EstateId, TestContext.Current.CancellationToken);
            getOperatorsResult.IsSuccess().ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperators_RepoCallFails_ExceptionThrown()
        {
            this.TransactionProcessorReadModelRepository.GetOperators(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));

            Result<List<Operator>> getOperatorsResult = await this.TransactionProcessorManager.GetOperators(TestData.EstateId, TestContext.Current.CancellationToken);
            getOperatorsResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchant_MerchantIsReturned()
        {
            this.AggregateService.GetLatest<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));
            this.AggregateService.GetLatest<OperatorAggregate>(Arg<Guid>.Any(), TestContext.Current.CancellationToken).ReturnsAsync(Results.Ok(TestData.Aggregates.CreatedOperatorAggregate()));
            this.TransactionProcessorReadModelRepository.GetMerchant(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Results.Ok(TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts()));

            Merchant expectedModel = TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts();

            Result<Merchant> getMerchantResult = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, TestContext.Current.CancellationToken);
            getMerchantResult.IsSuccess().ShouldBeTrue();
            Merchant merchantModel = getMerchantResult.Data;

            merchantModel.ShouldNotBeNull();
            merchantModel.MerchantReportingId.ShouldBe(expectedModel.MerchantReportingId);
            merchantModel.EstateId.ShouldBe(expectedModel.EstateId);
            merchantModel.EstateReportingId.ShouldBe(expectedModel.EstateReportingId);
            merchantModel.NextStatementDate.ShouldBe(expectedModel.NextStatementDate);
            merchantModel.MerchantId.ShouldBe(expectedModel.MerchantId);
            merchantModel.MerchantName.ShouldBe(expectedModel.MerchantName);
            merchantModel.SettlementSchedule.ShouldBe(expectedModel.SettlementSchedule);

            merchantModel.Addresses.ShouldHaveSingleItem();
            merchantModel.Addresses.Single().AddressId.ShouldNotBe(Guid.Empty);
            merchantModel.Addresses.Single().AddressLine1.ShouldBe(expectedModel.Addresses.Single().AddressLine1);
            merchantModel.Addresses.Single().AddressLine2.ShouldBe(expectedModel.Addresses.Single().AddressLine2);
            merchantModel.Addresses.Single().AddressLine3.ShouldBe(expectedModel.Addresses.Single().AddressLine3);
            merchantModel.Addresses.Single().AddressLine4.ShouldBe(expectedModel.Addresses.Single().AddressLine4);
            merchantModel.Addresses.Single().Country.ShouldBe(expectedModel.Addresses.Single().Country);
            merchantModel.Addresses.Single().PostalCode.ShouldBe(expectedModel.Addresses.Single().PostalCode);
            merchantModel.Addresses.Single().Region.ShouldBe(expectedModel.Addresses.Single().Region);
            merchantModel.Addresses.Single().Town.ShouldBe(expectedModel.Addresses.Single().Town);

            merchantModel.Contacts.ShouldHaveSingleItem();
            merchantModel.Contacts.Single().ContactEmailAddress.ShouldBe(expectedModel.Contacts.Single().ContactEmailAddress);
            merchantModel.Contacts.Single().ContactId.ShouldNotBe(Guid.Empty);
            merchantModel.Contacts.Single().ContactName.ShouldBe(expectedModel.Contacts.Single().ContactName);
            merchantModel.Contacts.Single().ContactPhoneNumber.ShouldBe(expectedModel.Contacts.Single().ContactPhoneNumber);

            merchantModel.Devices.ShouldHaveSingleItem();
            merchantModel.Devices.Single().DeviceId.ShouldBe(expectedModel.Devices.Single().DeviceId);
            merchantModel.Devices.Single().DeviceIdentifier.ShouldBe(expectedModel.Devices.Single().DeviceIdentifier);

            merchantModel.Operators.ShouldHaveSingleItem();
            merchantModel.Operators.Single().MerchantNumber.ShouldBe(expectedModel.Operators.Single().MerchantNumber);
            merchantModel.Operators.Single().Name.ShouldBe(expectedModel.Operators.Single().Name);
            merchantModel.Operators.Single().OperatorId.ShouldBe(expectedModel.Operators.Single().OperatorId);
            merchantModel.Operators.Single().TerminalNumber.ShouldBe(expectedModel.Operators.Single().TerminalNumber);

        }
        
        [Fact]
        public async Task TransactionProcessorManager_GetMerchant_MerchantNotCreated_ErrorThrown()
        {
            this.AggregateService.GetLatest<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.Aggregates.EmptyMerchantAggregate()));

            Result<Merchant> result = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, TestContext.Current.CancellationToken);
            result.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchant_GetLatestFails_ErrorThrown()
        {
            this.AggregateService.GetLatest<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));

            Result<Merchant> result = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, TestContext.Current.CancellationToken);
            result.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantSchedule_MerchantScheduleIsReturned()
        {
            MerchantScheduleAggregate merchantScheduleAggregate = TestData.Aggregates.CreatedMerchantScheduleAggregate();
            merchantScheduleAggregate.UpdateSchedule(TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts().Schedules.Single().Months);

            this.AggregateService.GetLatest<MerchantScheduleAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Results.Ok(merchantScheduleAggregate));

            Result<MerchantScheduleModel> result = await this.TransactionProcessorManager.GetMerchantSchedule(TestData.EstateId, TestData.MerchantId, TestData.MerchantScheduleYear, TestContext.Current.CancellationToken);

            result.IsSuccess().ShouldBeTrue();
            result.Data.Year.ShouldBe(TestData.MerchantScheduleYear);
            result.Data.Months.ShouldHaveSingleItem();
            result.Data.Months.Single().Month.ShouldBe(1);
            result.Data.Months.Single().ClosedDays.ShouldBe([1, 26]);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantSchedule_MerchantScheduleNotCreated_ReturnsFailure()
        {
            this.AggregateService.GetLatest<MerchantScheduleAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Results.Ok(TestData.Aggregates.EmptyMerchantScheduleAggregate()));

            Result<MerchantScheduleModel> result = await this.TransactionProcessorManager.GetMerchantSchedule(TestData.EstateId, TestData.MerchantId, TestData.MerchantScheduleYear, TestContext.Current.CancellationToken);

            result.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantScheduleFromReadModel_MerchantScheduleIsReturned()
        {
            MerchantScheduleModel expectedSchedule = TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts().Schedules.Single();
            this.TransactionProcessorReadModelRepository.GetMerchantSchedule(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<Int32>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Results.Ok(expectedSchedule));

            Result<MerchantScheduleModel> result = await this.TransactionProcessorManager.GetMerchantScheduleFromReadModel(TestData.EstateId, TestData.MerchantId, TestData.MerchantScheduleYear, TestContext.Current.CancellationToken);

            result.IsSuccess().ShouldBeTrue();
            result.Data.Year.ShouldBe(TestData.MerchantScheduleYear);
            result.Data.Months.ShouldHaveSingleItem();
            result.Data.Months.Single().Month.ShouldBe(1);
            result.Data.Months.Single().ClosedDays.ShouldBe([1, 26]);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantScheduleFromReadModel_MerchantScheduleNotFound_ReturnsFailure()
        {
            this.TransactionProcessorReadModelRepository.GetMerchantSchedule(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<Int32>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));

            Result<MerchantScheduleModel> result = await this.TransactionProcessorManager.GetMerchantScheduleFromReadModel(TestData.EstateId, TestData.MerchantId, TestData.MerchantScheduleYear, TestContext.Current.CancellationToken);

            result.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantContracts_MerchantContractsReturned()
        {
            this.TransactionProcessorReadModelRepository.GetMerchantContracts(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.MerchantContracts));

            Result<List<Contract>> getMerchantContractsResult = await this.TransactionProcessorManager.GetMerchantContracts(TestData.EstateId, TestData.MerchantId, TestContext.Current.CancellationToken);
            getMerchantContractsResult.IsSuccess().ShouldBeTrue();
            List<Contract> merchantContracts = getMerchantContractsResult.Data;
            merchantContracts.ShouldNotBeNull();
            merchantContracts.ShouldHaveSingleItem();
            merchantContracts.Single().ContractId.ShouldBe(TestData.ContractId);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantContracts_EmptyListReturned_ResultFailed()
        {
            this.TransactionProcessorReadModelRepository.GetMerchantContracts(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(TestData.MerchantContractsEmptyList));

            Result<List<Contract>> getMerchantContractsResult = await this.TransactionProcessorManager.GetMerchantContracts(TestData.EstateId, TestData.MerchantId, TestContext.Current.CancellationToken);
            getMerchantContractsResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantContracts_RepoCallFailed_ResultFailed()
        {
            this.TransactionProcessorReadModelRepository.GetMerchantContracts(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));

            Result<List<Contract>> getMerchantContractsResult = await this.TransactionProcessorManager.GetMerchantContracts(TestData.EstateId, TestData.MerchantId, TestContext.Current.CancellationToken);
            getMerchantContractsResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantContracts_RepositoryThrows_ResultFailed()
        {
            this.TransactionProcessorReadModelRepository.GetMerchantContracts(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ThrowsAsync(new Exception("Test exception"));

            Result<List<Contract>> getMerchantContractsResult = await this.TransactionProcessorManager.GetMerchantContracts(TestData.EstateId, TestData.MerchantId, TestContext.Current.CancellationToken);

            getMerchantContractsResult.IsSuccess().ShouldBeFalse();
            getMerchantContractsResult.GetErrorMessage().ShouldBe("Test exception");
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchants_MerchantListIsReturned()
        {
            this.TransactionProcessorReadModelRepository.GetMerchants(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(new List<Merchant>
                                                                                                                                     {
                                                                                                                                         TestData
                                                                                                                                             .MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts()
                                                                                                                                     }));

            Result<List<Merchant>> getMerchantsResult = await this.TransactionProcessorManager.GetMerchants(TestData.EstateId, TestContext.Current.CancellationToken);
            getMerchantsResult.IsSuccess().ShouldBeTrue();
            List<Merchant> merchantList = getMerchantsResult.Data;

            merchantList.ShouldNotBeNull();
            merchantList.ShouldNotBeEmpty();
            merchantList.ShouldHaveSingleItem();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchants_NullPayload_ReturnsNotFound()
        {
            this.TransactionProcessorReadModelRepository.GetMerchants(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Results.Ok<List<Merchant>>(null));

            Result<List<Merchant>> result = await this.TransactionProcessorManager.GetMerchants(TestData.EstateId, TestContext.Current.CancellationToken);

            result.GetErrors().ShouldHaveSingleItem().ErrorType.ShouldBe(ErrorType.NotFound);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchants_EmptyMerchants_ExceptionThrown()
        {
            List<Merchant> merchants = new List<Merchant>();
            this.TransactionProcessorReadModelRepository.GetMerchants(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Results.Ok(merchants));

            Result<List<Merchant>> getMerchantsResult = await this.TransactionProcessorManager.GetMerchants(TestData.EstateId, TestContext.Current.CancellationToken);
            getMerchantsResult.IsSuccess().ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchants_RepoCallFails_ExceptionThrown()
        {
            List<Merchant> merchants = new List<Merchant>();
            this.TransactionProcessorReadModelRepository.GetMerchants(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(new Error(ErrorType.Failure, "Test failure"));

            Result<List<Merchant>> getMerchantsResult = await this.TransactionProcessorManager.GetMerchants(TestData.EstateId, TestContext.Current.CancellationToken);
            getMerchantsResult.IsSuccess().ShouldBeFalse();
        }
    }
}
