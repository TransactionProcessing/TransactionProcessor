using Imposter.Abstractions;
using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.RequestHandlers;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.RequestHandler
{
    public class TransactionRequestHandlerTests
    {
        public TransactionRequestHandlerTests() {
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
        }

        [Fact]
        public async Task TransactionRequestHandler_ProcessLogonTransactionRequest_IsHandled()
        {
            ITransactionDomainServiceImposter transactionDomainService = new ITransactionDomainServiceImposter();
            TransactionRequestHandler handler = new TransactionRequestHandler(transactionDomainService.Instance());
            transactionDomainService.ProcessLogonTransaction(Arg<TransactionCommands.ProcessLogonTransactionCommand>.Any(),
                Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            TransactionCommands.ProcessLogonTransactionCommand command = TestData.Commands.ProcessLogonTransactionCommand;

            var result = await handler.Handle(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();

        }

        [Fact]
        public async Task TransactionRequestHandler_ProcessSaleTransactionRequest_IsHandled()
        {
            ITransactionDomainServiceImposter transactionDomainService = new ITransactionDomainServiceImposter();
            TransactionRequestHandler handler = new TransactionRequestHandler(transactionDomainService.Instance());
            transactionDomainService.ProcessSaleTransaction(Arg<TransactionCommands.ProcessSaleTransactionCommand>.Any(),
                Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            TransactionCommands.ProcessSaleTransactionCommand command = TestData.Commands.ProcessSaleTransactionCommand;

            var result = await handler.Handle(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();

        }

        [Fact]
        public async Task TransactionRequestHandler_ProcessReconciliationRequest_IsHandled()
        {
            ITransactionDomainServiceImposter transactionDomainService = new ITransactionDomainServiceImposter();
            TransactionRequestHandler handler = new TransactionRequestHandler(transactionDomainService.Instance());
            transactionDomainService.ProcessReconciliationTransaction(Arg<TransactionCommands.ProcessReconciliationCommand>.Any(),
                Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            TransactionCommands.ProcessReconciliationCommand command = TestData.Commands.ProcessReconciliationCommand;

            var result = await handler.Handle(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();

        }

        [Fact]
        public async Task TransactionRequestHandler_ResendTransactionReceiptRequest_IsHandled()
        {
            ITransactionDomainServiceImposter transactionDomainService = new ITransactionDomainServiceImposter();
            TransactionRequestHandler handler = new TransactionRequestHandler(transactionDomainService.Instance());
            transactionDomainService.ResendTransactionReceipt(Arg<TransactionCommands.ResendTransactionReceiptCommand>.Any(),
                    Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            TransactionCommands.ResendTransactionReceiptCommand command = TestData.Commands.ResendTransactionReceiptCommand;

            var result = await handler.Handle(command, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }
    }
}
