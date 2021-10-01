using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Services;
    using Models;
    using Moq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventStore;
    using Shouldly;
    using Testing;
    using TransactionAggregate;
    using Xunit;

    public class TransactionAggregateManagerTests
    {
        [Fact]
        public async Task TransactionAggregateManager_AuthoriseTransaction_TransactionAuthorised()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetStartedTransactionAggregate());
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);
            
            await transactionAggregateManager.AuthoriseTransaction(TestData.EstateId,
                                                             TestData.TransactionId,
                                                             TestData.OperatorIdentifier1,
                                                             TestData.OperatorResponse,
                                                             TestData.TransactionResponseCodeSuccess,
                                                             TestData.ResponseMessage,
                                                             CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_AuthoriseTransactionLocally_TransactionLocallyAuthorised()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetStartedTransactionAggregate());
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.AuthoriseTransactionLocally(TestData.EstateId,
                                                                   TestData.TransactionId,
                                                                   TestData.AuthorisationCode,
                                                                   (TestData.ResponseMessage, TestData.TransactionResponseCodeSuccess),
                                                                   CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_DeclineTransaction_TransactionDeclined()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetStartedTransactionAggregate());
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.DeclineTransaction(TestData.EstateId,
                                                                   TestData.TransactionId,
                                                                   TestData.OperatorIdentifier1,
                                                                   TestData.OperatorResponse,
                                                                   TestData.TransactionResponseCodeDeclinedByOperator,
                                                                   TestData.ResponseMessage,
                                                                   CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_DeclineTransactionLocally_TransactionLocallyDeclined()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetStartedTransactionAggregate());
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.DeclineTransactionLocally(TestData.EstateId,
                                                                          TestData.TransactionId,
                                                                          (TestData.ResponseMessage, TestData.TransactionResponseCodeSuccess),
                                                                          CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_GetAggregate_AggregateReturned()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCompletedLogonTransactionAggregate);
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            TransactionAggregate result = await transactionAggregateManager.GetAggregate(TestData.EstateId,
                                                                                         TestData.TransactionId,
                                                                                         CancellationToken.None);

            result.ShouldNotBeNull();
        }

        [Fact]
        public async Task TransactionAggregateManager_RecordAdditionalRequestData_AdditionalRequestDataRecorded()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetStartedTransactionAggregate());
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.RecordAdditionalRequestData(TestData.EstateId,
                                                                        TestData.TransactionId,
                                                                        TestData.OperatorIdentifier1,
                                                                        TestData.AdditionalTransactionMetaData(),
                                                                        CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_RecordAdditionalRequestData_NullAdditionalRequestData_NoActionPerformed()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetStartedTransactionAggregate());
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.RecordAdditionalRequestData(TestData.EstateId,
                                                                          TestData.TransactionId,
                                                                          TestData.OperatorIdentifier1,
                                                                          TestData.NullAdditionalTransactionMetaData,
                                                                          CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_RecordAdditionalRequestData_EmptyAdditionalRequestData_NoActionPerformed()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetStartedTransactionAggregate());
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.RecordAdditionalRequestData(TestData.EstateId,
                                                                          TestData.TransactionId,
                                                                          TestData.OperatorIdentifier1,
                                                                          TestData.EmptyAdditionalTransactionMetaData,
                                                                          CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_RecordAdditionalResponseData_AdditionalResponseDataRecorded()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetStartedTransactionAggregate());
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.RecordAdditionalResponseData(TestData.EstateId,
                                                                          TestData.TransactionId,
                                                                          TestData.OperatorIdentifier1,
                                                                          TestData.AdditionalTransactionMetaData(),
                                                                          CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_RecordAdditionalResponseData_NullAdditionalResponseData_NoActionPerformed()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetStartedTransactionAggregate());
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.RecordAdditionalResponseData(TestData.EstateId,
                                                                           TestData.TransactionId,
                                                                           TestData.OperatorIdentifier1,
                                                                           TestData.NullAdditionalTransactionMetaData,
                                                                           CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_RecordAdditionalResponseData_EmptyAdditionalResponseData_NoActionPerformed()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetStartedTransactionAggregate());
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.RecordAdditionalResponseData(TestData.EstateId,
                                                                           TestData.TransactionId,
                                                                           TestData.OperatorIdentifier1,
                                                                           TestData.EmptyAdditionalTransactionMetaData,
                                                                           CancellationToken.None);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public async Task TransactionAggregateManager_StartTransaction_TransactionStarted(TransactionType transactionType)
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEmptyTransactionAggregate);
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.StartTransaction(TestData.TransactionId,
                                                               TestData.TransactionDateTime,
                                                               TestData.TransactionNumber,
                                                               transactionType,
                                                               TestData.TransactionReference,
                                                               TestData.EstateId,
                                                               TestData.MerchantId,
                                                               TestData.DeviceIdentifier,
                                                               TestData.TransactionAmount,
                                                               CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_CompleteTransaction_TransactionCompleted()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetLocallyAuthorisedTransactionAggregate);
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.CompleteTransaction(TestData.EstateId,
                                                                  TestData.TransactionId,
                                                                  CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_RequestEmailReceipt_EmailRecieptRequested()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCompletedLogonTransactionAggregate);
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.RequestEmailReceipt(TestData.EstateId,
                                                                  TestData.TransactionId,
                                                                  TestData.CustomerEmailAddress,
                                                                  CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_AddProductDetails_ProductDetailsAddedToTransaction()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetStartedTransactionAggregate());
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.AddProductDetails(TestData.EstateId,
                                                                   TestData.TransactionId,
                                                                   TestData.ContractId,
                                                                   TestData.ProductId,
                                                                   CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_AddFee_FeeAddedToTransaction()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.AddFee(TestData.EstateId,
                                                     TestData.TransactionId,
                                                     TestData.CalculatedFeeServiceProviderFee,
                                                     CancellationToken.None);
        }

        [Fact]
        public async Task TransactionAggregateManager_AddSettledFee_FeeAddedToTransaction()
        {
            Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>> aggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            aggregateRepository.Setup(a => a.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);
            TransactionAggregateManager transactionAggregateManager = new TransactionAggregateManager(aggregateRepository.Object);

            await transactionAggregateManager.AddSettledFee(TestData.EstateId,
                                                     TestData.TransactionId,
                                                     TestData.CalculatedFeeMerchantFee2,
                                                     TestData.TransactionFeeSettlementDueDate,
                                                     TestData.TransactionFeeSettledDateTime,
                                                     CancellationToken.None);
        }
    }
}
