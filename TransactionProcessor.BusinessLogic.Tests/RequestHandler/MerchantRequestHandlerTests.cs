using Imposter;
using Imposter.Abstractions;
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
        IProjectionStateRepository<MerchantBalanceState> merchantBalanceStateRepository = new NullProjectionStateRepository();
        IEventStoreContextImposter eventStoreContext = new();
        ITransactionProcessorReadRepositoryImposter transactionProcessorReadRepository = new();
        IMerchantDomainServiceImposter merchantDomainService = new();
        ITransactionProcessorManagerImposter manager = new();
        MerchantRequestHandler handler = new(
            merchantBalanceStateRepository,
            eventStoreContext.Instance(),
            transactionProcessorReadRepository.Instance(),
            merchantDomainService.Instance(),
            manager.Instance());

        MerchantModel expectedMerchant = TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts();
        expectedMerchant.MerchantReportingId = TestData.MerchantReportingId;

        manager.GetMerchant(TestData.EstateId, TestData.MerchantId, Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(expectedMerchant));

        MerchantQueries.GetMerchantQuery query = TestData.Queries.GetMerchantQuery;

        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.MerchantReportingId.ShouldBe(TestData.MerchantReportingId);
        manager.GetMerchant(TestData.EstateId, TestData.MerchantId, Arg<CancellationToken>.Any()).Called(Count.Once());
    }

    private sealed class NullProjectionStateRepository : IProjectionStateRepository<MerchantBalanceState>
    {
        public Task<Result<MerchantBalanceState>> Load(Shared.DomainDrivenDesign.EventSourcing.IDomainEvent @event, CancellationToken cancellationToken) =>
            Task.FromResult(new Result<MerchantBalanceState>());

        public Task<Result<MerchantBalanceState>> Load(System.Guid estateId, System.Guid stateId, CancellationToken cancellationToken) =>
            Task.FromResult(new Result<MerchantBalanceState>());

        public Task<Result<MerchantBalanceState>> Save(MerchantBalanceState state, Shared.DomainDrivenDesign.EventSourcing.IDomainEvent @event, CancellationToken cancellationToken) =>
            Task.FromResult(new Result<MerchantBalanceState>());
    }
}

