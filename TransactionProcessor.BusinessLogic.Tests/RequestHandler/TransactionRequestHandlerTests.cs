using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using SimpleResults;
using TransactionProcessor.BusinessLogic.RequestHandlers;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.RequestHandler
{
    public class TransactionRequestHandlerTests
    {
        [Fact]
        public async Task TransactionRequestHandler_ProcessLogonTransactionRequest_IsHandled()
        {
            Mock<ITransactionDomainService> transactionDomainService = new Mock<ITransactionDomainService>();
            TransactionRequestHandler handler = new TransactionRequestHandler(transactionDomainService.Object);
            transactionDomainService.Setup(t => t.ProcessLogonTransaction(It.IsAny<TransactionCommands.ProcessLogonTransactionCommand>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
            TransactionCommands.ProcessLogonTransactionCommand command = TestData.Commands.ProcessLogonTransactionCommand;

            var result = await handler.Handle(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

        }

        [Fact]
        public async Task TransactionRequestHandler_ProcessSaleTransactionRequest_IsHandled()
        {
            Mock<ITransactionDomainService> transactionDomainService = new Mock<ITransactionDomainService>();
            TransactionRequestHandler handler = new TransactionRequestHandler(transactionDomainService.Object);
            transactionDomainService.Setup(t => t.ProcessSaleTransaction(It.IsAny<TransactionCommands.ProcessSaleTransactionCommand>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
            TransactionCommands.ProcessSaleTransactionCommand command = TestData.Commands.ProcessSaleTransactionCommand;

            var result = await handler.Handle(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

        }

        [Fact]
        public async Task TransactionRequestHandler_ProcessReconciliationRequest_IsHandled()
        {
            Mock<ITransactionDomainService> transactionDomainService = new Mock<ITransactionDomainService>();
            TransactionRequestHandler handler = new TransactionRequestHandler(transactionDomainService.Object);
            transactionDomainService.Setup(t => t.ProcessReconciliationTransaction(It.IsAny<TransactionCommands.ProcessReconciliationCommand>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
            TransactionCommands.ProcessReconciliationCommand command = TestData.Commands.ProcessReconciliationCommand;

            var result = await handler.Handle(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();

        }

        [Fact]
        public async Task TransactionRequestHandler_ResendTransactionReceiptRequest_IsHandled()
        {
            Mock<ITransactionDomainService> transactionDomainService = new Mock<ITransactionDomainService>();
            TransactionRequestHandler handler = new TransactionRequestHandler(transactionDomainService.Object);
            transactionDomainService
                .Setup(t => t.ResendTransactionReceipt(It.IsAny<TransactionCommands.ResendTransactionReceiptCommand>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
            TransactionCommands.ResendTransactionReceiptCommand command = TestData.Commands.ResendTransactionReceiptCommand;

            var result = await handler.Handle(command, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
    }
}