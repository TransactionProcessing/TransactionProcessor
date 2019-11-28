using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.Tests.CommandHandler
{
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Commands;
    using CommandHandlers;
    using Commands;
    using Moq;
    using Services;
    using Shared.DomainDrivenDesign.CommandHandling;
    using Shouldly;
    using Testing;
    using Xunit;

    public class CommandRouterTests
    {
        [Fact]
        public void CommandRouter_ProcessLogonTransactionCommand_IsRouted()
        {
            Mock<ITransactionDomainService> transactionDomainService = new Mock<ITransactionDomainService>();
            ICommandRouter router = new CommandRouter(transactionDomainService.Object);

            ProcessLogonTransactionCommand command = TestData.ProcessLogonTransactionCommand;

            Should.NotThrow(async () =>
            {
                await router.Route(command, CancellationToken.None);
            });
        }
    }

    public class TransactionDomainServiceTests
    {
        [Fact(Skip = "Complete once aggregate in place")]
        public async Task TransactionDomainService_ProcessLogonTransaction_LogonTransactionIsProcesses()
        {
            //Mock<IAggregateRepository<EstateAggregate>> estateAggregateRepository = new Mock<IAggregateRepository<EstateAggregate>>();
            //estateAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EstateAggregate());
            //estateAggregateRepository.Setup(m => m.SaveChanges(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            //Mock<IAggregateRepositoryManager> aggregateRepositoryManager = new Mock<IAggregateRepositoryManager>();
            //aggregateRepositoryManager.Setup(x => x.GetAggregateRepository<EstateAggregate>(It.IsAny<Guid>())).Returns(estateAggregateRepository.Object);

            //EstateDomainService domainService = new EstateDomainService(aggregateRepositoryManager.Object);

            //Should.NotThrow(async () =>
            //{
            //    await domainService.CreateEstate(TestData.EstateId,
            //                                       TestData.EstateName,
            //                                       CancellationToken.None);
            //});
            throw new NotImplementedException();
        }
    }
}


