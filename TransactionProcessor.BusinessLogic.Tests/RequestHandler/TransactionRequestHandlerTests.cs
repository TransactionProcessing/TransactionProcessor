namespace TransactionProcessor.BusinessLogic.Tests.CommandHandler
{
    using System.Threading;
    using BusinessLogic.Services;
    using Commands;
    using MediatR;
    using Moq;
    using RequestHandlers;
    using Requests;
    using Services;
    using Shared.DomainDrivenDesign.CommandHandling;
    using Shouldly;
    using Testing;
    using Xunit;

    public class TransactionRequestHandlerTests
    {
        [Fact]
        public void TransactionRequestHandler_ProcessLogonTransactionRequest_IsHandled()
        {
            Mock<ITransactionDomainService> transactionDomainService = new Mock<ITransactionDomainService>();
            TransactionRequestHandler handler = new TransactionRequestHandler(transactionDomainService.Object);

            ProcessLogonTransactionRequest command = TestData.ProcessLogonTransactionRequest;

            Should.NotThrow(async () =>
                            {
                                await handler.Handle(command, CancellationToken.None);
                            });

        }

        [Fact]
        public void TransactionRequestHandler_ProcessSaleTransactionRequest_IsHandled()
        {
            Mock<ITransactionDomainService> transactionDomainService = new Mock<ITransactionDomainService>();
            TransactionRequestHandler handler = new TransactionRequestHandler(transactionDomainService.Object);

            ProcessSaleTransactionRequest command = TestData.ProcessSaleTransactionRequest;

            Should.NotThrow(async () =>
                            {
                                await handler.Handle(command, CancellationToken.None);
                            });

        }

        [Fact]
        public void TransactionRequestHandler_ProcessReconciliationRequest_IsHandled()
        {
            Mock<ITransactionDomainService> transactionDomainService = new Mock<ITransactionDomainService>();
            TransactionRequestHandler handler = new TransactionRequestHandler(transactionDomainService.Object);

            ProcessReconciliationRequest command = TestData.ProcessReconciliationRequest;

            Should.NotThrow(async () =>
                            {
                                await handler.Handle(command, CancellationToken.None);
                            });

        }
    }

    public class SettlementRequestHandlerTests
    {
        [Fact]
        public void TransactionRequestHandler_ProcessLogonTransactionRequest_IsHandled()
        {
            Mock<ISettlementDomainService> settlementDomainService = new Mock<ISettlementDomainService>();
            SettlementRequestHandler handler = new SettlementRequestHandler(settlementDomainService.Object);

            ProcessSettlementRequest command = TestData.ProcessSettlementRequest;

            Should.NotThrow(async () =>
                            {
                                await handler.Handle(command, CancellationToken.None);
                            });

        }
    }
}