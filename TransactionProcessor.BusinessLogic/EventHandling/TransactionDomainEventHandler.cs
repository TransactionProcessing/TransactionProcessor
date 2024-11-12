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
    using EstateManagement.Database.Entities;
    using EstateManagement.DataTransferObjects;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.DataTransferObjects.Responses.Estate;
    using FloatAggregate;
    using Manager;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Microsoft.Extensions.Caching.Memory;
    using Models;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Services;
    using Settlement.DomainEvents;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.General;
    using Shared.Logger;
    using Transaction.DomainEvents;
    using TransactionAggregate;
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

        private readonly IEstateClient EstateClient;

        private readonly IFeeCalculationManager FeeCalculationManager;

        private readonly IMessagingServiceClient MessagingServiceClient;

        private readonly ISecurityServiceClient SecurityServiceClient;

        private readonly IAggregateRepository<SettlementAggregate, DomainEvent> SettlementAggregateRepository;

        private readonly IAggregateRepository<FloatActivityAggregate, DomainEvent> FloatActivityAggregateRepository;

        private readonly IMemoryCacheWrapper MemoryCache;
        private readonly IMediator Mediator;

        private readonly IAggregateRepository<TransactionAggregate, DomainEvent> TransactionAggregateRepository;

        private TokenResponse TokenResponse;

        private readonly ITransactionReceiptBuilder TransactionReceiptBuilder;

        #endregion

        #region Constructors

        public TransactionDomainEventHandler(IAggregateRepository<TransactionAggregate, DomainEvent> transactionAggregateRepository,
                                             IFeeCalculationManager feeCalculationManager,
                                             IEstateClient estateClient,
                                             ISecurityServiceClient securityServiceClient,
                                             ITransactionReceiptBuilder transactionReceiptBuilder,
                                             IMessagingServiceClient messagingServiceClient,
                                             IAggregateRepository<SettlementAggregate, DomainEvent> settlementAggregateRepository,
                                             IAggregateRepository<FloatActivityAggregate, DomainEvent> floatActivityAggregateRepository,
                                             IMemoryCacheWrapper memoryCache,
                                             IMediator mediator) {
            this.TransactionAggregateRepository = transactionAggregateRepository;
            this.FeeCalculationManager = feeCalculationManager;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
            this.TransactionReceiptBuilder = transactionReceiptBuilder;
            this.MessagingServiceClient = messagingServiceClient;
            this.SettlementAggregateRepository = settlementAggregateRepository;
            this.FloatActivityAggregateRepository = floatActivityAggregateRepository;
            this.MemoryCache = memoryCache;
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
                    domainEvent.CreditPurchasedDateTime, domainEvent.Amount);

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
            return Result.Success();

            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            TransactionAggregate transactionAggregate =
                await this.TransactionAggregateRepository.GetLatestVersion(domainEvent.TransactionId, cancellationToken);
            Transaction transaction = transactionAggregate.GetTransaction();

            MerchantResponse merchant =
                await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, domainEvent.EstateId, domainEvent.MerchantId, cancellationToken);

            EstateResponse estate = await this.EstateClient.GetEstate(this.TokenResponse.AccessToken, domainEvent.EstateId, cancellationToken);
            EstateOperatorResponse @operator = estate.Operators.Single(o => o.OperatorId == transaction.OperatorId);

            // Determine the body of the email
            String receiptMessage = await this.TransactionReceiptBuilder.GetEmailReceiptMessage(transactionAggregate.GetTransaction(), merchant, @operator.Name, cancellationToken);

            // Send the message
            return await this.SendEmailMessage(this.TokenResponse.AccessToken,
                                        domainEvent.EventId,
                                        domainEvent.EstateId,
                                        "Transaction Successful",
                                        receiptMessage,
                                        domainEvent.CustomerEmailAddress,
                                        cancellationToken);
        }

        private async Task<Result> HandleSpecificDomainEvent(CustomerEmailReceiptResendRequestedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            // Send the message
            return await this.ResendEmailMessage(this.TokenResponse.AccessToken, domainEvent.EventId, domainEvent.EstateId, cancellationToken);
        }
        
        private async Task<Result> ResendEmailMessage(String accessToken,
                                                      Guid messageId,
                                                      Guid estateId,
                                                      CancellationToken cancellationToken) {
            ResendEmailRequest resendEmailRequest = new ResendEmailRequest {
                                                                               ConnectionIdentifier = estateId,
                                                                               MessageId = messageId
                                                                           };
            try {
                return await this.MessagingServiceClient.ResendEmail(accessToken, resendEmailRequest, cancellationToken);
            }
            catch(Exception ex) when(ex.InnerException != null && ex.InnerException.GetType() == typeof(InvalidOperationException)) {
                // Only bubble up if not a duplicate message
                if (ex.InnerException.Message.Contains("Cannot send a message to provider that has already been sent", StringComparison.InvariantCultureIgnoreCase) ==
                    false) {
                    return Result.Failure(ex.GetExceptionMessages());
                }
                return Result.Success("Duplicate message send");
            }
            
        }

        private async Task<Result> SendEmailMessage(String accessToken,
                                                    Guid messageId,
                                                    Guid estateId,
                                                    String subject,
                                                    String body,
                                                    String emailAddress,
                                                    CancellationToken cancellationToken) {
            SendEmailRequest sendEmailRequest = new SendEmailRequest {
                                                                         MessageId = messageId,
                                                                         Body = body,
                                                                         ConnectionIdentifier = estateId,
                                                                         FromAddress = "golfhandicapping@btinternet.com", // TODO: lookup from config
                                                                         IsHtml = true,
                                                                         Subject = subject,
                                                                         ToAddresses = new List<String> {
                                                                                                            emailAddress
                                                                                                        }
                                                                     };

            // TODO: may decide to record the message Id againsts the Transaction Aggregate in future, but for now
            // we wont do this...
            try {
                return await this.MessagingServiceClient.SendEmail(accessToken, sendEmailRequest, cancellationToken);
            }
            catch(Exception ex) when(ex.InnerException != null && ex.InnerException.GetType() == typeof(InvalidOperationException)) {
                if (ex.InnerException.Message.Contains("Cannot send a message to provider that has already been sent", StringComparison.InvariantCultureIgnoreCase) ==
                    false)
                {
                    return Result.Failure(ex.GetExceptionMessages());
                }
                return Result.Success("Duplicate message send");
            }
        }

        #endregion
    }
}