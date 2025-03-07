﻿using System;
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
using TransactionProcessor.Models.Contract;
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
        private readonly Mock<IAggregateRepository<EstateAggregate, DomainEvent>> EstateAggregateRepository;
        private readonly Mock<IAggregateRepository<ContractAggregate, DomainEvent>> ContractAggregateRepository;
        private readonly Mock<IAggregateRepository<MerchantAggregate, DomainEvent>> MerchantAggregateRepository;
        private readonly Mock<IAggregateRepository<OperatorAggregate, DomainEvent>> OperatorAggregateRepository;

        private readonly TransactionProcessorManager TransactionProcessorManager;

        public TransactionProcessorManagerTests()
        {
            this.TransactionProcessorReadModelRepository = new Mock<ITransactionProcessorReadModelRepository>();

            this.EstateAggregateRepository = new Mock<IAggregateRepository<EstateAggregate, DomainEvent>>();
            this.ContractAggregateRepository = new Mock<IAggregateRepository<ContractAggregate, DomainEvent>>();
            this.MerchantAggregateRepository = new Mock<IAggregateRepository<MerchantAggregate, DomainEvent>>();
            this.OperatorAggregateRepository = new Mock<IAggregateRepository<OperatorAggregate, DomainEvent>>();

            this.TransactionProcessorManager = new TransactionProcessorManager(this.TransactionProcessorReadModelRepository.Object, this.EstateAggregateRepository.Object,
            this.ContractAggregateRepository.Object,
            this.MerchantAggregateRepository.Object,
            this.OperatorAggregateRepository.Object);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstates_EstatesAreReturned()
        {
            this.EstateAggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetEstate(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.EstateModel));

            var getEstatesResult = await this.TransactionProcessorManager.GetEstates(TestData.EstateId, CancellationToken.None);
            getEstatesResult.IsSuccess.ShouldBeTrue();
            var estateModels = getEstatesResult.Data;
            estateModels.ShouldNotBeNull();
            estateModels.ShouldHaveSingleItem();
            estateModels.Single().EstateId.ShouldBe(TestData.EstateModel.EstateId);
            estateModels.Single().Name.ShouldBe(TestData.EstateModel.Name);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstates_RepoCallFails_ResultFailed()
        {
            this.EstateAggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetEstate(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            var getEstatesResult = await this.TransactionProcessorManager.GetEstates(TestData.EstateId, CancellationToken.None);
            getEstatesResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstate_EstateIsReturned()
        {
            this.EstateAggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetEstate(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.EstateModel));
            this.OperatorAggregateRepository.Setup(o => o.GetLatestVersion(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

            var getEstateResult = await this.TransactionProcessorManager.GetEstate(TestData.EstateId, CancellationToken.None);
            getEstateResult.IsSuccess.ShouldBeTrue();
            var estateModel = getEstateResult.Data;
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
            this.EstateAggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetEstate(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.EstateModel));

            var getEstateResult = await this.TransactionProcessorManager.GetEstate(TestData.EstateId, CancellationToken.None);
            getEstateResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetEstate_GetLatestFailed_ErrorIsThrown()
        {
            this.EstateAggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetEstate(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.EstateModel));

            var getEstateResult = await this.TransactionProcessorManager.GetEstate(TestData.EstateId, CancellationToken.None);
            getEstateResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContract_ContractIsReturned()
        {
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant)));

            Result<Contract> getContractResult = await this.TransactionProcessorManager.GetContract(TestData.EstateId, TestData.ContractId, CancellationToken.None);
            getContractResult.IsSuccess.ShouldBeTrue();
            var contractModel = getContractResult.Data;

            contractModel.ShouldNotBeNull();
            contractModel.ContractId.ShouldBe(TestData.ContractId);
            contractModel.Description.ShouldBe(TestData.ContractDescription);
            contractModel.OperatorId.ShouldBe(TestData.OperatorId);
            contractModel.Products.ShouldNotBeNull();
            contractModel.Products.First().ContractProductId.ShouldBe(TestData.ContractProductId);
            contractModel.Products.First().TransactionFees.ShouldNotBeNull();
            contractModel.Products.First().TransactionFees.First().TransactionFeeId.ShouldBe(TestData.TransactionFeeId);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContract_ContractNotCreated_ErrorIsThrown()
        {
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            Result<Contract> getContractResult = await this.TransactionProcessorManager.GetContract(TestData.EstateId, TestData.ContractId, CancellationToken.None);
            getContractResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContract_GetLatestFails_ErrorIsThrown()
        {
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            Result<Contract> getContractResult = await this.TransactionProcessorManager.GetContract(TestData.EstateId, TestData.ContractId, CancellationToken.None);
            getContractResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContracts_ContractAreReturned()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetContracts(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new List<Contract>() { TestData.ContractModelWithProductsAndTransactionFees }));

            var getContractsResult = await this.TransactionProcessorManager.GetContracts(TestData.EstateId, CancellationToken.None);
            getContractsResult.IsSuccess.ShouldBeTrue();
            var contractModelList = getContractsResult.Data;
            contractModelList.ShouldNotBeNull();
            contractModelList.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetContracts_RepoCallFails_ContractAreReturned()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetContracts(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            var getContractsResult = await this.TransactionProcessorManager.GetContracts(TestData.EstateId, CancellationToken.None);
            getContractsResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_TransactionFeesAreReturned()
        {
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed, FeeType.Merchant)));

            var getTransactionFeesForProductResult = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.ContractProductId, CancellationToken.None);
            getTransactionFeesForProductResult.IsSuccess.ShouldBeTrue();
            var transactionFees = getTransactionFeesForProductResult.Data;
            transactionFees.ShouldNotBeNull();
            transactionFees.ShouldHaveSingleItem();
            transactionFees.First().TransactionFeeId.ShouldBe(TestData.TransactionFeeId);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_ContractNotFound_ErrorThrown()
        {
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyContractAggregate()));

            var result = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.ContractProductId, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_ProductNotFound_ErrorThrown()
        {
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedContractAggregate()));

            var result = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.ContractProductId, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetTransactionFeesForProduct_GetLatestFails_ErrorThrown()
        {
            this.ContractAggregateRepository.Setup(c => c.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            var result = await this.TransactionProcessorManager.GetTransactionFeesForProduct(TestData.EstateId, TestData.MerchantId, TestData.ContractId, TestData.ContractProductId, CancellationToken.None);
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
            this.OperatorAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

            var getOperatorResult = await this.TransactionProcessorManager.GetOperator(TestData.EstateId, TestData.OperatorId, CancellationToken.None);
            getOperatorResult.IsSuccess.ShouldBeTrue();
            var operatorDetails = getOperatorResult.Data;
            operatorDetails.ShouldNotBeNull();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperator_OperatorNotCreated_ExceptionThrown()
        {
            this.OperatorAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));

            var getOperatorResult = await this.TransactionProcessorManager.GetOperator(TestData.EstateId, TestData.OperatorId, CancellationToken.None);
            getOperatorResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperator_GetLatestFails_ExceptionThrown()
        {
            this.OperatorAggregateRepository.Setup(e => e.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            var getOperatorResult = await this.TransactionProcessorManager.GetOperator(TestData.EstateId, TestData.OperatorId, CancellationToken.None);
            getOperatorResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperators_OperatorDetailsAreReturned()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetOperators(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new List<Operator>{
                                                                                                                                                           TestData.OperatorModel
                                                                                                                                                       }));

            var getOperatorsResult = await this.TransactionProcessorManager.GetOperators(TestData.EstateId, CancellationToken.None);
            getOperatorsResult.IsSuccess.ShouldBeTrue();
            var operators = getOperatorsResult.Data;
            operators.ShouldNotBeNull();
            operators.ShouldHaveSingleItem();
            operators.Single().OperatorId.ShouldBe(TestData.OperatorId);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperators_EmptyList_ExceptionThrown()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetOperators(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new List<Operator>()));

            var getOperatorsResult = await this.TransactionProcessorManager.GetOperators(TestData.EstateId, CancellationToken.None);
            getOperatorsResult.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetOperators_RepoCallFails_ExceptionThrown()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetOperators(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            var getOperatorsResult = await this.TransactionProcessorManager.GetOperators(TestData.EstateId, CancellationToken.None);
            getOperatorsResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchant_MerchantIsReturned()
        {
            this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));
            this.OperatorAggregateRepository.Setup(o => o.GetLatestVersion(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

            Merchant expectedModel = TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts();

            var getMerchantResult = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            getMerchantResult.IsSuccess.ShouldBeTrue();
            var merchantModel = getMerchantResult.Data;

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
        public async Task TransactionProcessorManager_GetMerchant_MerchantIsReturnedWithNullAddressesAndContacts()
        {
            this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithOperator()));
            this.OperatorAggregateRepository.Setup(o => o.GetLatestVersion(It.IsAny<Guid>(), CancellationToken.None)).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyOperatorAggregate()));

            var getMerchantResult = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            getMerchantResult.IsSuccess.ShouldBeTrue();
            var merchantModel = getMerchantResult.Data;

            merchantModel.ShouldNotBeNull();
            merchantModel.MerchantId.ShouldBe(TestData.MerchantId);
            merchantModel.MerchantName.ShouldBe(TestData.MerchantName);
            merchantModel.Addresses.ShouldBeNull();
            merchantModel.Contacts.ShouldBeNull();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchant_WithAddress_MerchantIsReturnedWithNullContacts()
        {
            this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithAddress()));

            var getMerchantResult = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            getMerchantResult.IsSuccess.ShouldBeTrue();
            var merchantModel = getMerchantResult.Data;

            merchantModel.ShouldNotBeNull();
            merchantModel.MerchantId.ShouldBe(TestData.MerchantId);
            merchantModel.MerchantName.ShouldBe(TestData.MerchantName);
            merchantModel.Addresses.ShouldHaveSingleItem();
            merchantModel.Contacts.ShouldBeNull();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchant_WithContact_MerchantIsReturnedWithNullAddresses()
        {
            this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithContact()));

            var getMerchantResult = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            getMerchantResult.IsSuccess.ShouldBeTrue();
            var merchantModel = getMerchantResult.Data;

            merchantModel.ShouldNotBeNull();
            merchantModel.MerchantId.ShouldBe(TestData.MerchantId);
            merchantModel.MerchantName.ShouldBe(TestData.MerchantName);
            merchantModel.Addresses.ShouldBeNull();
            merchantModel.Contacts.ShouldHaveSingleItem();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchant_MerchantNotCreated_ErrorThrown()
        {
            this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));

            var result = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchant_GetLatestFails_ErrorThrown()
        {
            this.MerchantAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            var result = await this.TransactionProcessorManager.GetMerchant(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantContracts_MerchantContractsReturned()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchantContracts(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.MerchantContracts));

            var getMerchantContractsResult = await this.TransactionProcessorManager.GetMerchantContracts(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            getMerchantContractsResult.IsSuccess.ShouldBeTrue();
            var merchantContracts = getMerchantContractsResult.Data;
            merchantContracts.ShouldNotBeNull();
            merchantContracts.ShouldHaveSingleItem();
            merchantContracts.Single().ContractId.ShouldBe(TestData.ContractId);
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantContracts_EmptyListReturned_ResultFailed()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchantContracts(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.MerchantContractsEmptyList));

            var getMerchantContractsResult = await this.TransactionProcessorManager.GetMerchantContracts(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
            getMerchantContractsResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionProcessorManager_GetMerchantContracts_RepoCallFailed_ResultFailed()
        {
            this.TransactionProcessorReadModelRepository.Setup(e => e.GetMerchantContracts(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            var getMerchantContractsResult = await this.TransactionProcessorManager.GetMerchantContracts(TestData.EstateId, TestData.MerchantId, CancellationToken.None);
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
            var merchantList = getMerchantsResult.Data;

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
