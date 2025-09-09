using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shouldly;
using SimpleResults;
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
using Operator = TransactionProcessor.Models.Operator.Operator;

namespace TransactionProcessor.BusinessLogic.Tests.Manager
{
    public class TransactionProcessorManagerTests
    {
        private readonly Mock<ITransactionProcessorReadModelRepository> TransactionProcessorReadModelRepository;
        private readonly Mock<IAggregateService> AggregateService;
        
        private readonly TransactionProcessorManager TransactionProcessorManager;

        public TransactionProcessorManagerTests()
        {
            this.TransactionProcessorReadModelRepository = new Mock<ITransactionProcessorReadModelRepository>();

            this.AggregateService = new Mock<IAggregateService>();
            
            this.TransactionProcessorManager = new TransactionProcessorManager(this.TransactionProcessorReadModelRepository.Object, this.AggregateService.Object);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstates_EstatesAreReturned()
        {
            this.AggregateService.Setup(a => a.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetEstate(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.EstateModel));

            Result<List<Estate>> getEstatesResult = await this.TransactionProcessorManager.GetEstates(TestData.EstateId, CancellationToken.None);
            getEstatesResult.IsSuccess.ShouldBeTrue();
            List<Estate> estateModels = getEstatesResult.Data;
            estateModels.ShouldNotBeNull();
            estateModels.ShouldHaveSingleItem();
            estateModels.Single().EstateId.ShouldBe(TestData.EstateModel.EstateId);
            estateModels.Single().Name.ShouldBe(TestData.EstateModel.Name);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstates_RepoCallFails_ResultFailed()
        {
            this.AggregateService.Setup(a => a.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetEstate(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            Result<List<Estate>> getEstatesResult = await this.TransactionProcessorManager.GetEstates(TestData.EstateId, CancellationToken.None);
            getEstatesResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstate_EstateIsReturned()
        {
            this.AggregateService.Setup(a => a.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetEstate(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.EstateModel));
            this.AggregateService.Setup(o => o.GetLatest<OperatorAggregate>(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

            Result<Estate> getEstateResult = await this.TransactionProcessorManager.GetEstate(TestData.EstateId, CancellationToken.None);
            getEstateResult.IsSuccess.ShouldBeTrue();
            Estate estateModel = getEstateResult.Data;
            estateModel.ShouldNotBeNull();
            estateModel.EstateId.ShouldBe(TestData.EstateModel.EstateId);
            estateModel.Name.ShouldBe(TestData.EstateModel.Name);
            estateModel.Operators.ShouldHaveSingleItem();
            estateModel.Operators.Single().OperatorId.ShouldBe(TestData.OperatorId);
            // TODO: add back in with operator aggregate
            estateModel.Operators.Single().Name.ShouldBe(TestData.OperatorName);
            estateModel.Operators.Single().IsDeleted.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstate_InvalidEstateId_ErrorIsThrown()
        {
            this.AggregateService.Setup(a => a.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetEstate(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.EstateModel));

            Result<Estate> getEstateResult = await this.TransactionProcessorManager.GetEstate(TestData.EstateId, CancellationToken.None);
            getEstateResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstate_GetLatestFailed_ErrorIsThrown()
        {
            this.AggregateService.Setup(a => a.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetEstate(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.EstateModel));

            Result<Estate> getEstateResult = await this.TransactionProcessorManager.GetEstate(TestData.EstateId, CancellationToken.None);
            getEstateResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContract_ContractIsReturned()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant)));

            Result<Contract> getContractResult = await this.TransactionProcessorManager.GetContract(TestData.EstateId, TestData.ContractId, CancellationToken.None);
            getContractResult.IsSuccess.ShouldBeTrue();
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
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            Result<Contract> getContractResult = await this.TransactionProcessorManager.GetContract(TestData.EstateId, TestData.ContractId, CancellationToken.None);
            getContractResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContract_GetLatestFails_ErrorIsThrown()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            Result<Contract> getContractResult = await this.TransactionProcessorManager.GetContract(TestData.EstateId, TestData.ContractId, CancellationToken.None);
            getContractResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContracts_ContractAreReturned()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetContracts(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new List<Contract>() { TestData.ContractModelWithProductsAndTransactionFees }));

            Result<List<Contract>> getContractsResult = await this.TransactionProcessorManager.GetContracts(TestData.EstateId, CancellationToken.None);
            getContractsResult.IsSuccess.ShouldBeTrue();
            List<Contract> contractModelList = getContractsResult.Data;
            contractModelList.ShouldNotBeNull();
            contractModelList.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContracts_RepoCallFails_ContractAreReturned()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetContracts(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            Result<List<Contract>> getContractsResult = await this.TransactionProcessorManager.GetContracts(TestData.EstateId, CancellationToken.None);
            getContractsResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_TransactionFeesAreReturned()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant)));

            Result<List<ContractProductTransactionFee>> getTransactionFeesForProductResult = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.VariableContractProductId, CancellationToken.None);
            getTransactionFeesForProductResult.IsSuccess.ShouldBeTrue();
            List<ContractProductTransactionFee> transactionFees = getTransactionFeesForProductResult.Data;
            transactionFees.ShouldNotBeNull();
            transactionFees.ShouldHaveSingleItem();
            transactionFees.First().TransactionFeeId.ShouldBe(TestData.TransactionFeeId);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_ContractNotFound_ErrorThrown()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            Result<List<ContractProductTransactionFee>> result = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.FixedContractProductId, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_ProductNotFound_ErrorThrown()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            Result<List<ContractProductTransactionFee>> result = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.FixedContractProductId, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_GetLatestFails_ErrorThrown()
        {
            this.AggregateService.Setup(c => c.GetLatest<ContractAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            Result<List<ContractProductTransactionFee>> result = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.FixedContractProductId, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }
        /*
        
        [Fact]
        public async Task EstateManagementManager_GetFileDetails_FileDetailsAreReturned()
        {
            this.EstateManagementRepository.Setup(e => e.GetFileDetails(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.FileModel));

            var getFileDetailsResult = await this.EstateManagementManager.GetFileDetails(TestData.EstateId, TestData.FileId, CancellationToken.None);
            getFileDetailsResult.IsSuccess.ShouldBeTrue();
            var fileDetails = getFileDetailsResult.Data;
            fileDetails.ShouldNotBeNull();
        }

        [Fact]
        public async Task EstateManagementManager_GetFileDetails_RepoCallFailed_FileDetailsAreReturned()
        {
            this.EstateManagementRepository.Setup(e => e.GetFileDetails(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            var getFileDetailsResult = await this.EstateManagementManager.GetFileDetails(TestData.EstateId, TestData.FileId, CancellationToken.None);
            getFileDetailsResult.IsFailed.ShouldBeTrue();
        }
        */
        [Fact]
        public async Task TransactionProcessorManager_GetOperator_OperatorDetailsAreReturned()
        {
            this.AggregateService.Setup(o => o.GetLatest<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

            Result<Operator> getOperatorResult = await this.TransactionProcessorManager.GetOperator(TestData.EstateId, TestData.OperatorId, CancellationToken.None);
            getOperatorResult.IsSuccess.ShouldBeTrue();
            Operator operatorDetails = getOperatorResult.Data;
            operatorDetails.ShouldNotBeNull();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperator_OperatorNotCreated_ExceptionThrown()
        {
            this.AggregateService.Setup(o => o.GetLatest<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));

            Result<Operator> getOperatorResult = await this.TransactionProcessorManager.GetOperator(TestData.EstateId, TestData.OperatorId, CancellationToken.None);
            getOperatorResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperator_GetLatestFails_ExceptionThrown()
        {
            this.AggregateService.Setup(o => o.GetLatest<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            Result<Operator> getOperatorResult = await this.TransactionProcessorManager.GetOperator(TestData.EstateId, TestData.OperatorId, CancellationToken.None);
            getOperatorResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperators_OperatorDetailsAreReturned()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetOperators(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new List<Operator>{
                                                                                                                                                           TestData.OperatorModel
                                                                                                                                                       }));

            Result<List<Operator>> getOperatorsResult = await this.TransactionProcessorManager.GetOperators(TestData.EstateId, CancellationToken.None);
            getOperatorsResult.IsSuccess.ShouldBeTrue();
            List<Operator> operators = getOperatorsResult.Data;
            operators.ShouldNotBeNull();
            operators.ShouldHaveSingleItem();
            operators.Single().OperatorId.ShouldBe(TestData.OperatorId);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperators_EmptyList_ExceptionThrown()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetOperators(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new List<Operator>()));

            Result<List<Operator>> getOperatorsResult = await this.TransactionProcessorManager.GetOperators(TestData.EstateId, CancellationToken.None);
            getOperatorsResult.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperators_RepoCallFails_ExceptionThrown()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetOperators(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            Result<List<Operator>> getOperatorsResult = await this.TransactionProcessorManager.GetOperators(TestData.EstateId, CancellationToken.None);
            getOperatorsResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchant_MerchantIsReturned()
        {
            this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));
            this.AggregateService.Setup(o => o.GetLatest<OperatorAggregate>(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

            Merchant expectedModel = TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts();

            Result<Merchant> getMerchantResult = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            getMerchantResult.IsSuccess.ShouldBeTrue();
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
            this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));

            Result<Merchant> result = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchant_GetLatestFails_ErrorThrown()
        {
            this.AggregateService.Setup(m => m.GetLatest<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            Result<Merchant> result = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantContracts_MerchantContractsReturned()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchantContracts(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.MerchantContracts));

            Result<List<Contract>> getMerchantContractsResult = await this.TransactionProcessorManager.GetMerchantContracts(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            getMerchantContractsResult.IsSuccess.ShouldBeTrue();
            List<Contract> merchantContracts = getMerchantContractsResult.Data;
            merchantContracts.ShouldNotBeNull();
            merchantContracts.ShouldHaveSingleItem();
            merchantContracts.Single().ContractId.ShouldBe(TestData.ContractId);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantContracts_EmptyListReturned_ResultFailed()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchantContracts(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.MerchantContractsEmptyList));

            Result<List<Contract>> getMerchantContractsResult = await this.TransactionProcessorManager.GetMerchantContracts(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            getMerchantContractsResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantContracts_RepoCallFailed_ResultFailed()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchantContracts(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            Result<List<Contract>> getMerchantContractsResult = await this.TransactionProcessorManager.GetMerchantContracts(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            getMerchantContractsResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchants_MerchantListIsReturned()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchants(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new List<Merchant>
                                                                                                                                     {
                                                                                                                                         TestData
                                                                                                                                             .MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts()
                                                                                                                                     }));

            Result<List<Merchant>> getMerchantsResult = await this.TransactionProcessorManager.GetMerchants(TestData.EstateId, CancellationToken.None);
            getMerchantsResult.IsSuccess.ShouldBeTrue();
            List<Merchant> merchantList = getMerchantsResult.Data;

            merchantList.ShouldNotBeNull();
            merchantList.ShouldNotBeEmpty();
            merchantList.ShouldHaveSingleItem();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchants_NullMerchants_ExceptionThrown()
        {
            List<Merchant> merchants = null;
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchants(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(merchants));

            Result<List<Merchant>> getMerchantsResult = await this.TransactionProcessorManager.GetMerchants(TestData.EstateId, CancellationToken.None);
            getMerchantsResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchants_EmptyMerchants_ExceptionThrown()
        {
            List<Merchant> merchants = new List<Merchant>();
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchants(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(merchants));

            Result<List<Merchant>> getMerchantsResult = await this.TransactionProcessorManager.GetMerchants(TestData.EstateId, CancellationToken.None);
            getMerchantsResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchants_RepoCallFails_ExceptionThrown()
        {
            List<Merchant> merchants = new List<Merchant>();
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchants(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            Result<List<Merchant>> getMerchantsResult = await this.TransactionProcessorManager.GetMerchants(TestData.EstateId, CancellationToken.None);
            getMerchantsResult.IsFailed.ShouldBeTrue();
        }
    }
}
