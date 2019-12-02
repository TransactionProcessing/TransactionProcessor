namespace TransactionProcessor.BusinessLogic.Tests.CommandHandler
{
    using System.Threading;
    using BusinessLogic.Commands;
    using BusinessLogic.Services;
    using CommandHandlers;
    using Commands;
    using Moq;
    using Services;
    using Shared.DomainDrivenDesign.CommandHandling;
    using Shouldly;
    using Testing;
    using Xunit;

    public class TransactionCommandHandlerTests
    {
        [Fact]
        public void EstateCommandHandler_CreateEstateCommand_IsHandled()
        {
            Mock<ITransactionDomainService> transactionDomainService = new Mock<ITransactionDomainService>();
            ICommandHandler handler = new TransactionCommandHandler(transactionDomainService.Object);

            ProcessLogonTransactionCommand command = TestData.ProcessLogonTransactionCommand;

            Should.NotThrow(async () =>
                            {
                                await handler.Handle(command, CancellationToken.None);
                            });

        }
    }
}