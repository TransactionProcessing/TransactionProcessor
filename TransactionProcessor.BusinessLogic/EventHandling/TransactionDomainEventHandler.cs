using System.Diagnostics;
using MediatR;
using Shared.Exceptions;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Float.DomainEvents;

namespace TransactionProcessor.BusinessLogic.EventHandling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.DataTransferObjects.Responses.Estate;
    using Manager;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Microsoft.Extensions.Caching.Memory;
    using Models;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Services;
    using Settlement.DomainEvents;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.General;
    using Shared.Logger;
    using Transaction.DomainEvents;
    using static TransactionProcessor.BusinessLogic.Requests.SettlementCommands;
    using static TransactionProcessor.BusinessLogic.Requests.TransactionCommands;
    using CalculationType = Models.CalculationType;
    using ContractProductTransactionFee = EstateManagement.DataTransferObjects.Responses.Contract.ContractProductTransactionFee;
    using FeeType = Models.FeeType;
    using MerchantResponse = EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse;
    using Transaction = Models.Transaction;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.EventStore.EventHandling.IDomainEventHandler" />
    /// <seealso cref="IDomainEventHandler" />
    public class TransactionDomainEventHandler : IDomainEventHandler
    {
        #region Fields
        
        private readonly IMediator Mediator;

        private TokenResponse TokenResponse;


        #endregion

        #region Constructors

        public TransactionDomainEventHandler(IMediator mediator) {
            this.Mediator = mediator;
        }

        #endregion

        #region Methods

        public async Task<Result> Handle(IDomainEvent domainEvent,
                                         CancellationToken cancellationToken) {

            Logger.LogWarning($"|{domainEvent.EventId}|Transaction Domain Event Handler - Inside Handle {domainEvent.EventType}");
            Stopwatch sw = Stopwatch.StartNew();
            var result = await this.HandleSpecificDomainEvent((dynamic)domainEvent, cancellationToken);
            sw.Stop();
            Logger.LogWarning($"|{domainEvent.EventId}|Transaction Domain Event Handler - after HandleSpecificDomainEvent {domainEvent.EventType} time {sw.ElapsedMilliseconds}ms");

            return result;
        }

        private async Task<Result> HandleSpecificDomainEvent(FloatCreditPurchasedEvent domainEvent,
                                                     CancellationToken cancellationToken) {
            FloatActivityCommands.RecordCreditPurchaseCommand command =
                new(domainEvent.EstateId, domainEvent.FloatId,
                    domainEvent.CreditPurchasedDateTime, domainEvent.Amount, domainEvent.EventId);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionCostInformationRecordedEvent domainEvent, CancellationToken cancellationToken){
            FloatActivityCommands.RecordTransactionCommand command = new(domainEvent.EstateId,
                domainEvent.TransactionId);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(TransactionHasBeenCompletedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            CalculateFeesForTransactionCommand command = new(domainEvent.TransactionId, domainEvent.CompletedDateTime, domainEvent.EstateId, domainEvent.MerchantId);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(MerchantFeePendingSettlementAddedToTransactionEvent domainEvent,
                                                             CancellationToken cancellationToken) {

            SettlementCommands.AddMerchantFeePendingSettlementCommand command =
                new SettlementCommands.AddMerchantFeePendingSettlementCommand(domainEvent.TransactionId,
                    domainEvent.CalculatedValue, domainEvent.FeeCalculatedDateTime,
                    (CalculationType)domainEvent.FeeCalculationType, domainEvent.FeeId, domainEvent.FeeValue,
                    domainEvent.SettlementDueDate, domainEvent.MerchantId, domainEvent.EstateId);
            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(SettledMerchantFeeAddedToTransactionEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            AddSettledFeeToSettlementCommand command = new AddSettledFeeToSettlementCommand(
                domainEvent.SettledDateTime.Date, domainEvent.MerchantId, domainEvent.EstateId, domainEvent.FeeId, domainEvent.TransactionId);
            return await this.Mediator.Send(command, cancellationToken);

        }

        private async Task<Result> HandleSpecificDomainEvent(MerchantFeeSettledEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            AddSettledMerchantFeeCommand command = new(domainEvent.TransactionId, domainEvent.CalculatedValue,
                domainEvent.FeeCalculatedDateTime, (CalculationType)domainEvent.FeeCalculationType, domainEvent.FeeId,
                domainEvent.FeeValue, domainEvent.SettledDateTime, domainEvent.SettlementId);
            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(CustomerEmailReceiptRequestedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            SendCustomerEmailReceiptCommand command = new(domainEvent.EstateId, domainEvent.TransactionId, domainEvent.EventId, domainEvent.CustomerEmailAddress);

            return await this.Mediator.Send(command, cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(CustomerEmailReceiptResendRequestedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            TransactionCommands.ResendCustomerEmailReceiptCommand command = new(domainEvent.EstateId, domainEvent.TransactionId);

            return await this.Mediator.Send(command, cancellationToken);
        }
        
        #endregion
    }
}