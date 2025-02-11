using System;
using System.Threading;
using System.Threading.Tasks;
using EstateManagement.DataTransferObjects.Requests.Merchant;
using MediatR;
using Newtonsoft.Json;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventHandling;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Events;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Repository;
using TransactionProcessor.DomainEvents;
using MakeMerchantDepositRequest = TransactionProcessor.DataTransferObjects.Requests.Merchant.MakeMerchantDepositRequest;
using MerchantDepositSource = EstateManagement.DataTransferObjects.Requests.Merchant.MerchantDepositSource;

namespace TransactionProcessor.BusinessLogic.EventHandling
{
    public class MerchantDomainEventHandler : IDomainEventHandler
    {
        #region Fields

        private readonly IMediator Mediator;

        private readonly IAggregateRepository<MerchantAggregate, DomainEvent> MerchantAggregateRepository;

        private readonly ITransactionProcessorReadModelRepository EstateReportingRepository;
        #endregion

        #region Constructors

        public MerchantDomainEventHandler(IAggregateRepository<MerchantAggregate, DomainEvent> merchantAggregateRepository,
                                          ITransactionProcessorReadModelRepository estateReportingRepository,
                                          IMediator mediator)
        {
            this.MerchantAggregateRepository = merchantAggregateRepository;
            this.EstateReportingRepository = estateReportingRepository;
            this.Mediator = mediator;
        }

        #endregion

        #region Methods

        public async Task<Result> Handle(IDomainEvent domainEvent,
                                 CancellationToken cancellationToken)
        {
            Task<Result> t = domainEvent switch{
                MerchantDomainEvents.MerchantCreatedEvent de => this.EstateReportingRepository.AddMerchant(de, cancellationToken),
                MerchantDomainEvents.MerchantNameUpdatedEvent de => this.EstateReportingRepository.UpdateMerchant(de, cancellationToken),
                MerchantDomainEvents.AddressAddedEvent de => this.EstateReportingRepository.AddMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.ContactAddedEvent de => this.EstateReportingRepository.AddMerchantContact(de, cancellationToken),
                MerchantDomainEvents.SecurityUserAddedToMerchantEvent de => this.EstateReportingRepository.AddMerchantSecurityUser(de, cancellationToken),
                MerchantDomainEvents.DeviceAddedToMerchantEvent de => this.EstateReportingRepository.AddMerchantDevice(de, cancellationToken),
                MerchantDomainEvents.OperatorAssignedToMerchantEvent de => this.EstateReportingRepository.AddMerchantOperator(de, cancellationToken),
                MerchantDomainEvents.SettlementScheduleChangedEvent de => this.EstateReportingRepository.UpdateMerchant(de, cancellationToken),
                MerchantDomainEvents.MerchantReferenceAllocatedEvent de => this.EstateReportingRepository.UpdateMerchant(de, cancellationToken),
                //StatementGeneratedEvent de => this.EstateReportingRepository.UpdateMerchant(de, cancellationToken),
                TransactionDomainEvents.TransactionHasBeenCompletedEvent de => this.EstateReportingRepository.UpdateMerchant(de, cancellationToken),
                MerchantDomainEvents.ContractAddedToMerchantEvent de => this.EstateReportingRepository.AddContractToMerchant(de, cancellationToken),
                CallbackReceivedEnrichedEvent de => this.HandleSpecificDomainEvent(de, cancellationToken),
                MerchantDomainEvents.DeviceSwappedForMerchantEvent de => this.EstateReportingRepository.SwapMerchantDevice(de, cancellationToken),
                MerchantDomainEvents.OperatorRemovedFromMerchantEvent de => this.EstateReportingRepository.RemoveOperatorFromMerchant(de, cancellationToken),
                MerchantDomainEvents.MerchantAddressLine1UpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de,cancellationToken),
                MerchantDomainEvents.MerchantAddressLine2UpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantAddressLine3UpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantAddressLine4UpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantCountyUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantRegionUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantTownUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantPostalCodeUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantAddress(de, cancellationToken),
                MerchantDomainEvents.MerchantContactNameUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantContact(de, cancellationToken),
                MerchantDomainEvents.MerchantContactEmailAddressUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantContact(de, cancellationToken),
                MerchantDomainEvents.MerchantContactPhoneNumberUpdatedEvent de => this.EstateReportingRepository.UpdateMerchantContact(de, cancellationToken),
                _ => null
            };
            if (t != null)
                return await t;

            return Result.Success();
        }

        private async Task<Result> HandleSpecificDomainEvent(CallbackReceivedEnrichedEvent domainEvent,
                                                     CancellationToken cancellationToken)
        {
            if (domainEvent.TypeString == typeof(CallbackHandler.DataTransferObjects.Deposit).ToString())
            {
                // Work out the merchant id from the reference field (second part, split on hyphen)
                String merchantReference = domainEvent.Reference.Split("-")[1];

                Result<Merchant> result = await this.EstateReportingRepository.GetMerchantFromReference(domainEvent.EstateId, merchantReference, cancellationToken);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // We now need to deserialise the message from the callback
                CallbackHandler.DataTransferObjects.Deposit callbackMessage = JsonConvert.DeserializeObject<CallbackHandler.DataTransferObjects.Deposit>(domainEvent.CallbackMessage);

                MerchantCommands.MakeMerchantDepositCommand command = new(domainEvent.EstateId,
                                                                          result.Data.MerchantId,
                                                                          DataTransferObjects.Requests.Merchant.MerchantDepositSource.Automatic,
                                                                          new MakeMerchantDepositRequest
                                                                          {
                                                                              DepositDateTime = callbackMessage.DateTime,
                                                                              Reference = callbackMessage.Reference,
                                                                              Amount = callbackMessage.Amount,
                                                                          });
                return await this.Mediator.Send(command, cancellationToken);
            }
            return Result.Success();
        }




        #endregion
    }
}