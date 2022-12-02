using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Shared.EventStore.EventHandling;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.Controllers;
using TransactionProcessor.Transaction.DomainEvents;
using Xunit;

namespace TransactionProcessor.Tests.ControllerTests
{
    using System.Threading;
    using Shared.General;
    using Shared.Logger;

    public class ControllerTests
    {
        public ControllerTests()
        {
            Logger.Initialise(new NullLogger());
        }
        [Fact]
        public async Task DomainEventController_EventIdNotPresentInJson_ErrorThrown()
        {
            Mock<IDomainEventHandlerResolver> resolver = new Mock<IDomainEventHandlerResolver>();
            TypeMap.AddType<TransactionHasBeenCompletedEvent>("TransactionHasBeenCompletedEvent");
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["eventType"] = "TransactionHasBeenCompletedEvent";
            DomainEventController controller = new DomainEventController(resolver.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
            String json = "{\r\n  \"completedDateTime\": \"2022-11-08T15:40:07\",\r\n  \"estateId\": \"435613ac-a468-47a3-ac4f-649d89764c22\",\r\n  \"isAuthorised\": true,\r\n  \"transactionAmount\": 35.0,\r\n  \"merchantId\": \"8bc8434d-41f9-4cc3-83bc-e73f20c02e1d\",\r\n  \"responseCode\": \"0000\",\r\n  \"responseMessage\": \"SUCCESS\",\r\n  \"transactionId\": \"626644c5-bb7b-40ca-821e-cf115488867b\",\r\n}";
            Object request = JsonConvert.DeserializeObject(json);
            ArgumentException ex = Should.Throw<ArgumentException>(async () => {
                await controller.PostEventAsync(request, CancellationToken.None);
            });
            ex.Message.ShouldBe("Domain Event must contain an Event Id");
        }

        [Fact]
        public async Task DomainEventController_EventIdPresentInJson_NoErrorThrown()
        {
            Mock<IDomainEventHandlerResolver> resolver = new Mock<IDomainEventHandlerResolver>();
            TypeMap.AddType<TransactionHasBeenCompletedEvent>("TransactionHasBeenCompletedEvent");
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["eventType"] = "TransactionHasBeenCompletedEvent";
            DomainEventController controller = new DomainEventController(resolver.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
            String json = "{\r\n  \"completedDateTime\": \"2022-11-08T15:40:07\",\r\n  \"estateId\": \"435613ac-a468-47a3-ac4f-649d89764c22\",\r\n  \"isAuthorised\": true,\r\n  \"transactionAmount\": 35.0,\r\n  \"merchantId\": \"8bc8434d-41f9-4cc3-83bc-e73f20c02e1d\",\r\n  \"responseCode\": \"0000\",\r\n  \"responseMessage\": \"SUCCESS\",\r\n  \"transactionId\": \"626644c5-bb7b-40ca-821e-cf115488867b\",\r\n  \"eventId\": \"9840045a-df9f-4ae3-879d-db205a744bf3\"\r\n}";
            Object request = JsonConvert.DeserializeObject(json);
            Should.NotThrow(async () => {
                await controller.PostEventAsync(request, CancellationToken.None);
            });
        }
    }
}
