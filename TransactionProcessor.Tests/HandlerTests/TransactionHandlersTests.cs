using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using Shared.General;
using Shouldly;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects;
using TransactionProcessor.Handlers;
using TransactionProcessor.Models;
using Xunit;

namespace TransactionProcessor.Tests.HandlerTests
{
    public class TransactionHandlersTests
    {
        [Fact]
        public async Task PerformTransaction_LogonPayloadWithoutTypeMetadata_SendsLogonCommand()
        {
            Mock<IMediator> mediator = new Mock<IMediator>(MockBehavior.Strict);
            LogonTransactionRequest request = new LogonTransactionRequest
            {
                DeviceIdentifier = "device-1",
                TransactionDateTime = DateTime.SpecifyKind(new DateTime(2024, 1, 2, 3, 4, 5), DateTimeKind.Utc),
                TransactionNumber = "000001",
                TransactionType = "Logon",
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId
            };

            mediator.Setup(m => m.Send(It.IsAny<TransactionCommands.ProcessLogonTransactionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new ProcessLogonTransactionResponse
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    ResponseCode = "0000",
                    ResponseMessage = "SUCCESS",
                    TransactionId = Guid.NewGuid()
                }));

            IResult result = await TransactionHandlers.PerformLogonTransaction(mediator.Object,
                new DefaultHttpContext(),
                request,
                CancellationToken.None);

            result.ShouldNotBeNull();
            mediator.VerifyAll();
        }

        [Fact]
        public async Task PerformTransaction_SalePayloadWithoutTypeMetadata_SendsSaleCommand()
        {
            Mock<IMediator> mediator = new Mock<IMediator>(MockBehavior.Strict);
            SaleTransactionRequest request = new SaleTransactionRequest
            {
                AdditionalTransactionMetadata = new Dictionary<String, String> { { "amount", "12.34" } },
                ContractId = Guid.NewGuid(),
                CustomerEmailAddress = "customer@test.local",
                DeviceIdentifier = "device-1",
                OperatorId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                TransactionDateTime = new DateTime(2024, 1, 2, 3, 4, 5),
                TransactionNumber = "000002",
                TransactionSource = 2,
                TransactionType = "Sale",
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId
            };

            mediator.Setup(m => m.Send(It.IsAny<TransactionCommands.ProcessSaleTransactionCommand>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new ProcessSaleTransactionResponse
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    ResponseCode = "0000",
                    ResponseMessage = "SUCCESS",
                    TransactionId = Guid.NewGuid()
                }));

            IResult result = await TransactionHandlers.PerformSaleTransaction(mediator.Object,
                new DefaultHttpContext(),
                request,
                CancellationToken.None);

            result.ShouldNotBeNull();
            mediator.VerifyAll();
        }

        [Fact]
        public async Task PerformTransaction_ReconciliationPayloadWithoutTypeMetadata_SendsReconciliationCommand()
        {
            Mock<IMediator> mediator = new Mock<IMediator>(MockBehavior.Strict);
            ReconciliationRequest request = new ReconciliationRequest
            {
                DeviceIdentifier = "device-1",
                OperatorTotals = new List<OperatorTotalRequest>(),
                TransactionCount = 4,
                TransactionDateTime = new DateTime(2024, 1, 2, 3, 4, 5),
                TransactionValue = 42.50m,
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                TransactionType = "Reconciliation"
            };

            mediator.Setup(m => m.Send(It.IsAny<TransactionCommands.ProcessReconciliationCommand>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new ProcessReconciliationTransactionResponse
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    ResponseCode = "0000",
                    ResponseMessage = "SUCCESS",
                    TransactionId = Guid.NewGuid()
                }));

            IResult result = await TransactionHandlers.PerformReconciliationTransaction(mediator.Object,
                new DefaultHttpContext(),
                request,
                CancellationToken.None);

            result.ShouldNotBeNull();
            mediator.VerifyAll();
        }

        private static class TestData
        {
            public static Guid EstateId => Guid.Parse("11111111-1111-1111-1111-111111111111");
            public static Guid MerchantId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        }
    }
}
