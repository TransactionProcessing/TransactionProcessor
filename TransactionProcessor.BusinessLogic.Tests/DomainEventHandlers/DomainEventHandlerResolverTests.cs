using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Shared.EventStore.EventHandling;
using Shouldly;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers
{
    public class DomainEventHandlerResolverTests
    {
        [Fact]
        public void DomainEventHandlerResolver_CanBeCreated_IsCreated()
        {
            Dictionary<String, String[]> eventHandlerConfiguration = new Dictionary<String, String[]>();

            eventHandlerConfiguration.Add("TestEventType1", new String[] { "TransactionProcessor.BusinessLogic.EventHandling.TransactionDomainEventHandler, TransactionProcessor.BusinessLogic" });

            Mock<IDomainEventHandler> domainEventHandler = new Mock<IDomainEventHandler>();
            Func<Type, IDomainEventHandler> createDomainEventHandlerFunc = (type) => { return domainEventHandler.Object; };
            DomainEventHandlerResolver resolver = new DomainEventHandlerResolver(eventHandlerConfiguration, createDomainEventHandlerFunc);

            resolver.ShouldNotBeNull();
        }

        [Fact]
        public void DomainEventHandlerResolver_CanBeCreated_InvalidEventHandlerType_ErrorThrown()
        {
            Dictionary<String, String[]> eventHandlerConfiguration = new Dictionary<String, String[]>();

            eventHandlerConfiguration.Add("TestEventType1", new String[] { "TransactionProcessor.BusinessLogic.EventHandling.NonExistantDomainEventHandler, TransactionProcessor.BusinessLogic" });

            Mock<IDomainEventHandler> domainEventHandler = new Mock<IDomainEventHandler>();
            Func<Type, IDomainEventHandler> createDomainEventHandlerFunc = (type) => { return domainEventHandler.Object; };

            Should.Throw<NotSupportedException>(() => new DomainEventHandlerResolver(eventHandlerConfiguration, createDomainEventHandlerFunc));
        }

        [Fact]
        public void DomainEventHandlerResolver_GetDomainEventHandlers_TransactionHasBeenCompletedEvent_EventHandlersReturned()
        {
                String handlerTypeName = "TransactionProcessor.BusinessLogic.EventHandling.TransactionDomainEventHandler, TransactionProcessor.BusinessLogic";
                Dictionary<String, String[]> eventHandlerConfiguration = new Dictionary<String, String[]>();

            TransactionDomainEvents.TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent = TestData.TransactionHasBeenCompletedEvent;

            eventHandlerConfiguration.Add(transactionHasBeenCompletedEvent.GetType().Name, new String[] { handlerTypeName });

            Mock<IDomainEventHandler> domainEventHandler = new Mock<IDomainEventHandler>();
            Func<Type, IDomainEventHandler> createDomainEventHandlerFunc = (type) => { return domainEventHandler.Object; };

            DomainEventHandlerResolver resolver = new DomainEventHandlerResolver(eventHandlerConfiguration, createDomainEventHandlerFunc);

            List<IDomainEventHandler> handlers = resolver.GetDomainEventHandlers(transactionHasBeenCompletedEvent);

            handlers.ShouldNotBeNull();
            handlers.Any().ShouldBeTrue();
            handlers.Count.ShouldBe(1);
        }

        [Fact]
        public void DomainEventHandlerResolver_GetDomainEventHandlers_TransactionHasBeenCompletedEvent_EventNotConfigured_EventHandlersReturned()
        {
            String handlerTypeName = "TransactionProcessor.BusinessLogic.EventHandling.TransactionDomainEventHandler, TransactionProcessor.BusinessLogic";
            Dictionary<String, String[]> eventHandlerConfiguration = new Dictionary<String, String[]>();

            TransactionDomainEvents.TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent = TestData.TransactionHasBeenCompletedEvent;

            eventHandlerConfiguration.Add("RandomEvent", new String[] { handlerTypeName });
            Mock<IDomainEventHandler> domainEventHandler = new Mock<IDomainEventHandler>();
            Func<Type, IDomainEventHandler> createDomainEventHandlerFunc = (type) => { return domainEventHandler.Object; };

            DomainEventHandlerResolver resolver = new DomainEventHandlerResolver(eventHandlerConfiguration, createDomainEventHandlerFunc);

            List<IDomainEventHandler> handlers = resolver.GetDomainEventHandlers(transactionHasBeenCompletedEvent);

            handlers.ShouldBeNull();
        }

        [Fact]
        public void DomainEventHandlerResolver_GetDomainEventHandlers_TransactionHasBeenCompletedEvent_NoHandlersConfigured_EventHandlersReturned()
        {
            Dictionary<String, String[]> eventHandlerConfiguration = new Dictionary<String, String[]>();

            TransactionDomainEvents.TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent = TestData.TransactionHasBeenCompletedEvent;
            Mock<IDomainEventHandler> domainEventHandler = new Mock<IDomainEventHandler>();

            Func<Type, IDomainEventHandler> createDomainEventHandlerFunc = (type) => { return domainEventHandler.Object; };

            DomainEventHandlerResolver resolver = new DomainEventHandlerResolver(eventHandlerConfiguration, createDomainEventHandlerFunc);

            List<IDomainEventHandler> handlers = resolver.GetDomainEventHandlers(transactionHasBeenCompletedEvent);

            handlers.ShouldBeNull();
        }
    }
}