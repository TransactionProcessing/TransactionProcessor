using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.Aggregates
{
    public static class OperatorAggregateExtensions
    {
        public static TransactionProcessor.Models.Operator.Operator GetOperator(this OperatorAggregate aggregate) {
            return new TransactionProcessor.Models.Operator.Operator() { Name = aggregate.Name, OperatorId = aggregate.AggregateId, RequireCustomMerchantNumber = aggregate.RequireCustomMerchantNumber, RequireCustomTerminalNumber = aggregate.RequireCustomTerminalNumber };
        }

        public static void PlayEvent(this OperatorAggregate aggregate, OperatorDomainEvents.OperatorCreatedEvent domainEvent)
        {
            aggregate.IsCreated = true;
            aggregate.Name = domainEvent.Name;
            aggregate.RequireCustomMerchantNumber = domainEvent.RequireCustomMerchantNumber;
            aggregate.RequireCustomTerminalNumber = domainEvent.RequireCustomTerminalNumber;
            aggregate.EstateId = domainEvent.EstateId;
        }

        public static void PlayEvent(this OperatorAggregate aggregate, OperatorDomainEvents.OperatorNameUpdatedEvent domainEvent)
        {
            aggregate.Name = domainEvent.Name;
        }

        public static void PlayEvent(this OperatorAggregate aggregate, OperatorDomainEvents.OperatorRequireCustomMerchantNumberChangedEvent domainEvent)
        {
            aggregate.RequireCustomMerchantNumber = domainEvent.RequireCustomMerchantNumber;
        }

        public static void PlayEvent(this OperatorAggregate aggregate, OperatorDomainEvents.OperatorRequireCustomTerminalNumberChangedEvent domainEvent)
        {
            aggregate.IsCreated = true;
            aggregate.RequireCustomTerminalNumber = domainEvent.RequireCustomTerminalNumber;
        }

        public static void Create(this OperatorAggregate aggregate,
                                  Guid estateId,
                                  String name,
                                  Boolean requireCustomMerchantNumber,
                                  Boolean requireCustomTerminalNumber)
        {
            Guard.ThrowIfInvalidGuid(estateId, typeof(ArgumentNullException), "Estate Id must not be an empty Guid");
            Guard.ThrowIfNullOrEmpty(name, typeof(ArgumentNullException), "Operator name must not be null or empty");

            OperatorDomainEvents.OperatorCreatedEvent operatorCreatedEvent = new(aggregate.AggregateId, estateId, name, requireCustomMerchantNumber, requireCustomTerminalNumber);

            aggregate.ApplyAndAppend(operatorCreatedEvent);
        }

        public static void UpdateOperator(this OperatorAggregate aggregate,
                                          String name,
                                          Boolean requireCustomMerchantNumber,
                                          Boolean requireCustomTerminalNumber)
        {
            if (String.Compare(name, aggregate.Name, StringComparison.InvariantCultureIgnoreCase) != 0 &&
                String.IsNullOrEmpty(name) == false)
            {
                OperatorDomainEvents.OperatorNameUpdatedEvent operatorNameUpdatedEvent = new(aggregate.AggregateId, aggregate.EstateId, name);
                aggregate.ApplyAndAppend(operatorNameUpdatedEvent);
            }

            if (requireCustomMerchantNumber != aggregate.RequireCustomMerchantNumber)
            {
                OperatorDomainEvents.OperatorRequireCustomMerchantNumberChangedEvent operatorRequireCustomMerchantNumberChangedEvent = new(aggregate.AggregateId, aggregate.EstateId, requireCustomMerchantNumber);
                aggregate.ApplyAndAppend(operatorRequireCustomMerchantNumberChangedEvent);
            }

            if (requireCustomTerminalNumber != aggregate.RequireCustomTerminalNumber)
            {
                OperatorDomainEvents.OperatorRequireCustomTerminalNumberChangedEvent operatorRequireCustomTerminalNumberChangedEvent = new(aggregate.AggregateId, aggregate.EstateId, requireCustomTerminalNumber);
                aggregate.ApplyAndAppend(operatorRequireCustomTerminalNumberChangedEvent);
            }
        }
    }

    public record OperatorAggregate : Aggregate
    {
        public Boolean IsCreated { get; internal set; }
        public Guid EstateId { get; internal set; }

        public String Name { get; internal set; }
        public Boolean RequireCustomMerchantNumber { get; internal set; }
        public Boolean RequireCustomTerminalNumber { get; internal set; }

        public static OperatorAggregate Create(Guid aggregateId)
        {
            return new OperatorAggregate(aggregateId);
        }

        #region Constructors

        [ExcludeFromCodeCoverage]
        public OperatorAggregate()
        {
        }

        private OperatorAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
        }

        #endregion

        public override void PlayEvent(IDomainEvent domainEvent) => OperatorAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return new
            {
                EstateId = Guid.NewGuid() // TODO: Populate
            };
        }
    }
}
