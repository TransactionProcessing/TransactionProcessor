﻿namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHanders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MessagingService.BusinessLogic.EventHandling;
    using Moq;
    using Shouldly;
    using Testing;
    using Transaction.DomainEvents;
    using Xunit;

    public class DomainEventHandlerResolverTests
    {
        [Fact]
        public void DomainEventHandlerResolver_CanBeCreated_IsCreated()
        {
            Dictionary<String, String[]> eventHandlerConfiguration = new Dictionary<String, String[]>();

            eventHandlerConfiguration.Add("TestEventType1", new String[] { "TransactionProcessor.BusinessLogic.EventHandling.TransactionDomainEventHandler" });

            Mock<IDomainEventHandler> domainEventHandler = new Mock<IDomainEventHandler>();
            Func<Type, IDomainEventHandler> createDomainEventHandlerFunc = (type) => { return domainEventHandler.Object; };
            DomainEventHandlerResolver resolver = new DomainEventHandlerResolver(eventHandlerConfiguration, createDomainEventHandlerFunc);

            resolver.ShouldNotBeNull();
        }

        [Fact]
        public void DomainEventHandlerResolver_CanBeCreated_InvalidEventHandlerType_ErrorThrown()
        {
            Dictionary<String, String[]> eventHandlerConfiguration = new Dictionary<String, String[]>();

            eventHandlerConfiguration.Add("TestEventType1", new String[] { "TransactionProcessor.BusinessLogic.EventHandling.NonExistantDomainEventHandler" });

            Mock<IDomainEventHandler> domainEventHandler = new Mock<IDomainEventHandler>();
            Func<Type, IDomainEventHandler> createDomainEventHandlerFunc = (type) => { return domainEventHandler.Object; };

            Should.Throw<NotSupportedException>(() => new DomainEventHandlerResolver(eventHandlerConfiguration, createDomainEventHandlerFunc));
        }

        [Fact]
        public void DomainEventHandlerResolver_GetDomainEventHandlers_TransactionHasBeenCompletedEvent_EventHandlersReturned()
        {
                String handlerTypeName = "TransactionProcessor.BusinessLogic.EventHandling.TransactionDomainEventHandler";
                Dictionary<String, String[]> eventHandlerConfiguration = new Dictionary<String, String[]>();

            TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent = TestData.TransactionHasBeenCompletedEvent;

            eventHandlerConfiguration.Add(transactionHasBeenCompletedEvent.GetType().FullName, new String[] { handlerTypeName });

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
            String handlerTypeName = "TransactionProcessor.BusinessLogic.EventHandling.TransactionDomainEventHandler";
            Dictionary<String, String[]> eventHandlerConfiguration = new Dictionary<String, String[]>();

            TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent = TestData.TransactionHasBeenCompletedEvent;

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

            TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent = TestData.TransactionHasBeenCompletedEvent;
            Mock<IDomainEventHandler> domainEventHandler = new Mock<IDomainEventHandler>();

            Func<Type, IDomainEventHandler> createDomainEventHandlerFunc = (type) => { return domainEventHandler.Object; };

            DomainEventHandlerResolver resolver = new DomainEventHandlerResolver(eventHandlerConfiguration, createDomainEventHandlerFunc);

            List<IDomainEventHandler> handlers = resolver.GetDomainEventHandlers(transactionHasBeenCompletedEvent);

            handlers.ShouldBeNull();
        }
    }
}