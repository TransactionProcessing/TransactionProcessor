using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Services;
    using Models;
    using Moq;
    using Shared.DomainDrivenDesign.EventStore;
    using Shared.EventStore.EventStore;
    using Shouldly;
    using Testing;
    using TransactionAggregate;
    using Xunit;

    public class TransactionDomainServiceTests
    {
        [Fact]
        public async Task TransactionDomainService_ProcessLogonTransaction_TransactionIsProcessed()
        {
            Mock<IAggregateRepositoryManager> aggregateRepositoryManager = new Mock<IAggregateRepositoryManager>();
            Mock<IAggregateRepository<TransactionAggregate>> transactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate>>();

            aggregateRepositoryManager.Setup(x => x.GetAggregateRepository<TransactionAggregate>(It.IsAny<Guid>())).Returns(transactionAggregateRepository.Object);
            transactionAggregateRepository.SetupSequence(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                          .ReturnsAsync(TestData.GetEmptyTransactionAggregate)
                                          .ReturnsAsync(TestData.GetStartedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetLocallyAuthorisedTransactionAggregate)
                                          .ReturnsAsync(TestData.GetCompletedTransactionAggregate);
            transactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            TransactionDomainService transactionDomainService = new TransactionDomainService(aggregateRepositoryManager.Object);

            ProcessLogonTransactionResponse response = await transactionDomainService.ProcessLogonTransaction(TestData.TransactionId,
                                                             TestData.EstateId,
                                                             TestData.MerchantId,
                                                             TestData.TransactionDateTime,
                                                             TestData.TransactionNumber,
                                                             TestData.DeviceIdentifier,
                                                             CancellationToken.None);

            response.ShouldNotBeNull();
            response.ResponseCode.ShouldBe(TestData.ResponseCode);
            response.ResponseMessage.ShouldBe(TestData.ResponseMessage);
        }
    }
}
