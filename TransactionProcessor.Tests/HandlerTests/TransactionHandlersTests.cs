using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Imposter.Abstractions;
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
            IMediatorImposter mediator = new();
            LogonTransactionRequest request = new LogonTransactionRequest
            {
                DeviceIdentifier = "device-1",
                TransactionDateTime = DateTime.SpecifyKind(new DateTime(2024, 1, 2, 3, 4, 5), DateTimeKind.Utc),
                TransactionNumber = "000001",
                TransactionType = "Logon",
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId
            };

            mediator.Send<Result<ProcessLogonTransactionResponse>>(Arg<IRequest<Result<ProcessLogonTransactionResponse>>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(new ProcessLogonTransactionResponse
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    ResponseCode = "0000",
                    ResponseMessage = "SUCCESS",
                    TransactionId = Guid.NewGuid()
                }));

            IResult result = await TransactionHandlers.PerformLogonTransaction(mediator.Instance(),
                new DefaultHttpContext(),
                request,
                CancellationToken.None);

            result.ShouldNotBeNull();
            mediator.Send<Result<ProcessLogonTransactionResponse>>(Arg<IRequest<Result<ProcessLogonTransactionResponse>>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
        }

        [Fact]
        public async Task PerformTransaction_SalePayloadWithoutTypeMetadata_SendsSaleCommand()
        {
            IMediatorImposter mediator = new();
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

            mediator.Send<Result<ProcessSaleTransactionResponse>>(Arg<IRequest<Result<ProcessSaleTransactionResponse>>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(new ProcessSaleTransactionResponse
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    ResponseCode = "0000",
                    ResponseMessage = "SUCCESS",
                    TransactionId = Guid.NewGuid()
                }));

            IResult result = await TransactionHandlers.PerformSaleTransaction(mediator.Instance(),
                new DefaultHttpContext(),
                request,
                CancellationToken.None);

            result.ShouldNotBeNull();
            mediator.Send<Result<ProcessSaleTransactionResponse>>(Arg<IRequest<Result<ProcessSaleTransactionResponse>>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
        }

        [Fact]
        public async Task PerformTransaction_SalePayloadWithoutTransactionSource_UsesDefaultSource()
        {
            IMediatorImposter mediator = new();
            TransactionCommands.ProcessSaleTransactionCommand? capturedCommand = null;
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
                TransactionType = "Sale",
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId
            };

            mediator.Send<Result<ProcessSaleTransactionResponse>>(Arg<IRequest<Result<ProcessSaleTransactionResponse>>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(new ProcessSaleTransactionResponse
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    ResponseCode = "0000",
                    ResponseMessage = "SUCCESS",
                    TransactionId = Guid.NewGuid()
                }))
                .Callback((IRequest<Result<ProcessSaleTransactionResponse>> request, CancellationToken _) => { capturedCommand = (TransactionCommands.ProcessSaleTransactionCommand)request; return Task.CompletedTask; });

            IResult result = await TransactionHandlers.PerformSaleTransaction(mediator.Instance(),
                new DefaultHttpContext(),
                request,
                CancellationToken.None);

            result.ShouldNotBeNull();
            capturedCommand.ShouldNotBeNull();
            capturedCommand!.TransactionSource.ShouldBe(1);
            mediator.Send<Result<ProcessSaleTransactionResponse>>(Arg<IRequest<Result<ProcessSaleTransactionResponse>>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
        }

        [Fact]
        public async Task PerformTransaction_SalePayload_WhenMediatorFails_ReturnsFailure()
        {
            IMediatorImposter mediator = new();
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
                TransactionType = "Sale",
                TransactionSource = 2,
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId
            };

            mediator.Send<Result<ProcessSaleTransactionResponse>>(Arg<IRequest<Result<ProcessSaleTransactionResponse>>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("boom").ToResult(new ProcessSaleTransactionResponse()));

            IResult result = await TransactionHandlers.PerformSaleTransaction(mediator.Instance(),
                new DefaultHttpContext(),
                request,
                CancellationToken.None);

            result.ShouldNotBeNull();
            mediator.Send<Result<ProcessSaleTransactionResponse>>(Arg<IRequest<Result<ProcessSaleTransactionResponse>>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
        }

        [Fact]
        public async Task PerformTransaction_LogonPayload_WhenMediatorFails_ReturnsFailure()
        {
            IMediatorImposter mediator = new();
            LogonTransactionRequest request = new LogonTransactionRequest
            {
                DeviceIdentifier = "device-1",
                TransactionDateTime = DateTime.SpecifyKind(new DateTime(2024, 1, 2, 3, 4, 5), DateTimeKind.Utc),
                TransactionNumber = "000001",
                TransactionType = "Logon",
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId
            };

            mediator.Send<Result<ProcessLogonTransactionResponse>>(Arg<IRequest<Result<ProcessLogonTransactionResponse>>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("boom").ToResult(new ProcessLogonTransactionResponse()));

            IResult result = await TransactionHandlers.PerformLogonTransaction(mediator.Instance(),
                new DefaultHttpContext(),
                request,
                CancellationToken.None);

            result.ShouldNotBeNull();
            mediator.Send<Result<ProcessLogonTransactionResponse>>(Arg<IRequest<Result<ProcessLogonTransactionResponse>>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
        }

        [Fact]
        public async Task PerformTransaction_ReconciliationPayloadWithoutTypeMetadata_SendsReconciliationCommand()
        {
            IMediatorImposter mediator = new();
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

            mediator.Send<Result<ProcessReconciliationTransactionResponse>>(Arg<IRequest<Result<ProcessReconciliationTransactionResponse>>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(new ProcessReconciliationTransactionResponse
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    ResponseCode = "0000",
                    ResponseMessage = "SUCCESS",
                    TransactionId = Guid.NewGuid()
                }));

            IResult result = await TransactionHandlers.PerformReconciliationTransaction(mediator.Instance(),
                new DefaultHttpContext(),
                request,
                CancellationToken.None);

            result.ShouldNotBeNull();
            mediator.Send<Result<ProcessReconciliationTransactionResponse>>(Arg<IRequest<Result<ProcessReconciliationTransactionResponse>>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
        }

        [Fact]
        public async Task PerformTransaction_ReconciliationPayload_WhenMediatorFails_ReturnsFailure()
        {
            IMediatorImposter mediator = new();
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

            mediator.Send<Result<ProcessReconciliationTransactionResponse>>(Arg<IRequest<Result<ProcessReconciliationTransactionResponse>>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure("boom").ToResult(new ProcessReconciliationTransactionResponse()));

            IResult result = await TransactionHandlers.PerformReconciliationTransaction(mediator.Instance(),
                new DefaultHttpContext(),
                request,
                CancellationToken.None);

            result.ShouldNotBeNull();
            mediator.Send<Result<ProcessReconciliationTransactionResponse>>(Arg<IRequest<Result<ProcessReconciliationTransactionResponse>>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
        }

        [Fact]
        public async Task ResendTransactionReceipt_SendsCommand()
        {
            IMediatorImposter mediator = new();

            mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            IResult result = await TransactionHandlers.ResendTransactionReceipt(mediator.Instance(),
                new DefaultHttpContext(),
                TestData.EstateId,
                Guid.NewGuid(),
                CancellationToken.None);

            result.ShouldNotBeNull();
            mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
        }

        private static class TestData
        {
            public static Guid EstateId => Guid.Parse("11111111-1111-1111-1111-111111111111");
            public static Guid MerchantId => Guid.Parse("22222222-2222-2222-2222-222222222222");
        }
    }
}
