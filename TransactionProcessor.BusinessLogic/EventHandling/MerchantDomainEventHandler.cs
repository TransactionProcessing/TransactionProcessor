using MediatR;
using Polly;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.EventStore.EventHandling;
using TransactionProcessing.UnionResults;
using TransactionProcessing.UnionResults;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessing.Shared.Serialisation;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Events;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Repository;
using MakeMerchantDepositRequest = TransactionProcessor.DataTransferObjects.Requests.Merchant.MakeMerchantDepositRequest;

namespace TransactionProcessor.BusinessLogic.EventHandling
{
    public class MerchantDomainEventHandler : IDomainEventHandler
    {
        #region Fields

        private readonly IMediator Mediator;
        private readonly ITransactionProcessorReadModelRepository EstateReportingRepository;
        #endregion

        #region Constructors

        public MerchantDomainEventHandler(ITransactionProcessorReadModelRepository estateReportingRepository,
                                          IMediator mediator)
        {
            this.EstateReportingRepository = estateReportingRepository;
            this.Mediator = mediator;
        }

        #endregion

        #region Methods

        public async Task<Result> Handle(IDomainEvent domainEvent,
                                 CancellationToken cancellationToken)
        {
            Task<Result> t = domainEvent switch{
                CallbackReceivedEnrichedEvent de => this.HandleSpecificDomainEvent(de, cancellationToken),
                _ => null
            };
            if (t != null)
                return await t;

            return Result.Success();
        }

        private async Task<Result> HandleSpecificDomainEvent(CallbackReceivedEnrichedEvent domainEvent,
                                                             CancellationToken cancellationToken) {
            if (domainEvent.TypeString == typeof(CallbackHandler.DataTransferObjects.Deposit).ToString()) {
                // Work out the merchant id from the reference field (second part, split on hyphen)
                String merchantReference = domainEvent.Reference.Split("-")[1];

                Result<Merchant> result = await this.EstateReportingRepository.GetMerchantFromReference(domainEvent.EstateId, merchantReference, cancellationToken);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // We now need to deserialise the message from the callback
                CallbackHandler.DataTransferObjects.Deposit callbackMessage = StringSerialiser.Deserialise<CallbackHandler.DataTransferObjects.Deposit>(domainEvent.CallbackMessage);

                MerchantCommands.MakeMerchantDepositCommand command = new(domainEvent.EstateId, result.GetValueOrThrow().MerchantId, DataTransferObjects.Requests.Merchant.MerchantDepositSource.Automatic, new MakeMerchantDepositRequest { DepositDateTime = callbackMessage.DateTime, Reference = callbackMessage.Reference, Amount = callbackMessage.Amount,HostDateTime = DateTimeOffset.UtcNow });
                return await this.Mediator.Send(command, cancellationToken);
            }

            return Result.Success();
        }

        #endregion
    }
}