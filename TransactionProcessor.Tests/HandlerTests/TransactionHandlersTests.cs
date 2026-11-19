using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Imposter.Abstractions;
using TransactionProcessing.Shared.General;
using Shouldly;
using TransactionProcessing.UnionResults;
using TransactionProcessing.UnionResults.AspNetCore;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Handlers;
using TransactionProcessor.Models;
using Xunit;
using TransactionProcessor.DataTransferObjects.Requests.Transaction;

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

            IResultOutcome result = await TransactionHandlers.PerformLogonTransaction(mediator.Instance(),
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

            IResultOutcome result = await TransactionHandlers.PerformSaleTransaction(mediator.Instance(),
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

            IResultOutcome result = await TransactionHandlers.PerformSaleTransaction(mediator.Instance(),
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
                .ReturnsAsync(Result<ProcessSaleTransactionResponse>.Fail(Error.Failure("boom")));

            IResultOutcome result = await TransactionHandlers.PerformSaleTransaction(mediator.Instance(),
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
                .ReturnsAsync(Result<ProcessLogonTransactionResponse>.Fail(Error.Failure("boom")));

            IResultOutcome result = await TransactionHandlers.PerformLogonTransaction(mediator.Instance(),
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

            IResultOutcome result = await TransactionHandlers.PerformReconciliationTransaction(mediator.Instance(),
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
                .ReturnsAsync(Result<ProcessReconciliationTransactionResponse>.Fail(Error.Failure("boom")));

            IResultOutcome result = await TransactionHandlers.PerformReconciliationTransaction(mediator.Instance(),
                new DefaultHttpContext(),
                request,
                CancellationToken.None);

            result.ShouldNotBeNull();
            mediator.Send<Result<ProcessReconciliationTransactionResponse>>(Arg<IRequest<Result<ProcessReconciliationTransactionResponse>>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
        }

        [Fact]
        public async Task PerformReconciliationTransaction_WhenTotalsAreInvalid_ReturnsBadRequest()
        {
            CancellationToken cancellationToken = TestContext.Current.CancellationToken;
            IMediatorImposter mediator = new();
            mediator.Send<Result<ProcessReconciliationTransactionResponse>>(
                Arg<IRequest<Result<ProcessReconciliationTransactionResponse>>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Invalid("Transaction Count is 0 but Value is 25"));

            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddSingleton<IMediator>(mediator.Instance());
            await using WebApplication app = builder.Build();
            app.MapGroup("/api/transactions").AddUnionResultFilter()
                .MapPost("/reconciliation", TransactionHandlers.PerformReconciliationTransaction);
            await app.StartAsync(cancellationToken);
            using HttpClient client = app.GetTestClient();

            ReconciliationRequest request = new()
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                DeviceIdentifier = "device-1",
                TransactionType = "Reconciliation",
                TransactionDateTime = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                TransactionCount = 0,
                TransactionValue = 25m
            };
            using HttpResponseMessage response = await client.PostAsJsonAsync("/api/transactions/reconciliation", request, cancellationToken);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            (await response.Content.ReadAsStringAsync(cancellationToken)).ShouldContain("Transaction Count is 0 but Value is 25");
        }

        [Fact]
        public async Task ResendTransactionReceipt_SendsCommand()
        {
            IMediatorImposter mediator = new();

            mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            IResultOutcome result = await TransactionHandlers.ResendTransactionReceipt(mediator.Instance(),
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
