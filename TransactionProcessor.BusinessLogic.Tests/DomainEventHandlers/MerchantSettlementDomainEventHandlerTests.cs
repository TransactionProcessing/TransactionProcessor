﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using Shouldly;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.Testing;
using Xunit;
using static TransactionProcessor.DomainEvents.SettlementDomainEvents;
using MediatR;
using Moq;
using Shared.Logger;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using Grpc.Core;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers
{
    public class MerchantSettlementDomainEventHandlerTests
    {
        private readonly MerchantSettlementDomainEventHandler EventHandler;
        private readonly Mock<IMediator> Mediator;

        public MerchantSettlementDomainEventHandlerTests()
        {
            this.Mediator = new Mock<IMediator>();
            this.EventHandler = new MerchantSettlementDomainEventHandler(this.Mediator.Object);
            Logger.Initialise(new NullLogger());
        }

        [Fact]
        public async Task MerchantSettlementDomainEventHandler_Handle_MerchantFeeSettledEvent_EventIsHandled()
        {
            this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            Result result = await this.EventHandler.Handle(TestData.DomainEvents.MerchantFeeSettledEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task MerchantSettlementDomainEventHandler_Handle_MerchantFeeSettledEvent_Retry_EventIsHandled()
        {
            this.Mediator.SetupSequence(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new WrongExpectedVersionException("Stream1", StreamRevision.None, StreamRevision.None))
                .ThrowsAsync(new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded")))
                .ReturnsAsync(Result.Success());                       

            Result result = await this.EventHandler.Handle(TestData.DomainEvents.MerchantFeeSettledEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
    }
}
