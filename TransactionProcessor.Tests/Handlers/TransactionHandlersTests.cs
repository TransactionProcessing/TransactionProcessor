using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects;
using TransactionProcessor.Handlers;
using TransactionProcessor.Models;
using Xunit;

namespace TransactionProcessor.Tests.Handlers
{
    public class TransactionHandlersTests
    {
        [Fact]
        public async Task TransactionHandlers_PerformTransaction_MetadataMissing_UsesIdsFromSerialisedRequest()
        {
            Guid estateId = Guid.NewGuid();
            Guid merchantId = Guid.NewGuid();
            ProcessLogonTransactionResponse response = new ProcessLogonTransactionResponse
            {
                EstateId = estateId,
                MerchantId = merchantId,
                TransactionId = Guid.NewGuid(),
                ResponseCode = "0000",
                ResponseMessage = "SUCCESS"
            };

            Mock<IMediator> mediator = new Mock<IMediator>(MockBehavior.Strict);
            mediator.Setup(m => m.Send(It.Is<TransactionCommands.ProcessLogonTransactionCommand>(c => c.EstateId == estateId &&
                                                                                                     c.MerchantId == merchantId),
                                       It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Result.Success(response));

            SerialisedMessage request = CreateSerialisedMessage(new LogonTransactionRequest
            {
                EstateId = estateId,
                MerchantId = merchantId,
                DeviceIdentifier = "DEVICE1",
                TransactionDateTime = DateTime.UtcNow,
                TransactionNumber = "123456",
                TransactionType = "Logon"
            });

            IResult result = await TransactionHandlers.PerformTransaction(mediator.Object, new DefaultHttpContext(), request, CancellationToken.None);
            DefaultHttpContext httpContext = await ExecuteResult(result);

            httpContext.Response.StatusCode.ShouldBeOneOf(StatusCodes.Status200OK, StatusCodes.Status201Created);
            mediator.VerifyAll();
        }

        [Fact]
        public async Task TransactionHandlers_PerformTransaction_InvalidMetadataAndMissingBodyIds_ReturnsBadRequest()
        {
            Mock<IMediator> mediator = new Mock<IMediator>(MockBehavior.Strict);
            SerialisedMessage request = CreateSerialisedMessage(new LogonTransactionRequest
            {
                DeviceIdentifier = "DEVICE1",
                TransactionDateTime = DateTime.UtcNow,
                TransactionNumber = "123456",
                TransactionType = "Logon"
            },
            new Dictionary<String, String>
            {
                { MetadataContants.KeyNameEstateId, "not-a-guid" },
                { MetadataContants.KeyNameMerchantId, Guid.NewGuid().ToString() }
            });

            IResult result = await TransactionHandlers.PerformTransaction(mediator.Object, new DefaultHttpContext(), request, CancellationToken.None);
            DefaultHttpContext httpContext = await ExecuteResult(result);

            httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
            mediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task TransactionHandlers_PerformTransaction_InvalidJson_ReturnsBadRequest()
        {
            Mock<IMediator> mediator = new Mock<IMediator>(MockBehavior.Strict);
            SerialisedMessage request = new SerialisedMessage
            {
                SerialisedData = "not-json"
            };

            IResult result = await TransactionHandlers.PerformTransaction(mediator.Object, new DefaultHttpContext(), request, CancellationToken.None);
            DefaultHttpContext httpContext = await ExecuteResult(result);

            httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
            mediator.VerifyNoOtherCalls();
        }

        private static SerialisedMessage CreateSerialisedMessage(DataTransferObject request,
                                                                 Dictionary<string, string> metadata = null)
        {
            return new SerialisedMessage
            {
                Metadata = metadata ?? new Dictionary<String, String>(),
                SerialisedData = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                })
            };
        }

        private static async Task<DefaultHttpContext> ExecuteResult(IResult result)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            await result.ExecuteAsync(httpContext);

            return httpContext;
        }
    }
}
