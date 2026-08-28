using Moq;
using Shared.EventStore.EventStore;
using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.RequestHandlers;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.ProjectionEngine.Repository;
using TransactionProcessor.ProjectionEngine.State;
using TransactionProcessor.Testing;
using Xunit;

using MerchantModel = TransactionProcessor.Models.Merchant.Merchant;

namespace TransactionProcessor.BusinessLogic.Tests.RequestHandler;

public class MerchantRequestHandlerTests
{
    public MerchantRequestHandlerTests()
    {
        StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
    }

    [Fact]
    public async Task MerchantRequestHandler_GetMerchantQuery_ReturnsMerchantReportingId()
    {
        Mock<IProjectionStateRepository<MerchantBalanceState>> merchantBalanceStateRepository = new();
        Mock<IEventStoreContext> eventStoreContext = new();
        Mock<ITransactionProcessorReadRepository> transactionProcessorReadRepository = new();
        Mock<IMerchantDomainService> merchantDomainService = new();
        Mock<ITransactionProcessorManager> manager = new();
        MerchantRequestHandler handler = new(
            merchantBalanceStateRepository.Object,
            eventStoreContext.Object,
            transactionProcessorReadRepository.Object,
            merchantDomainService.Object,
            manager.Object);

        MerchantModel expectedMerchant = TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts();
        expectedMerchant.MerchantReportingId = TestData.MerchantReportingId;

        manager
            .Setup(m => m.GetMerchant(TestData.EstateId, TestData.MerchantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedMerchant));

        MerchantQueries.GetMerchantQuery query = TestData.Queries.GetMerchantQuery;

        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.MerchantReportingId.ShouldBe(TestData.MerchantReportingId);
        manager.Verify(m => m.GetMerchant(TestData.EstateId, TestData.MerchantId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

