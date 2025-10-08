using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using SimpleResults;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Estate;

namespace TransactionProcessor.Aggregates{
    public static class EstateAggregateExtensions{
        #region Methods

        public static Result AddOperator(this EstateAggregate aggregate,
                                       Guid operatorId){

            Result result = aggregate.CheckEstateHasBeenCreated();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckOperatorHasNotAlreadyBeenCreated(operatorId);
            if (result.IsFailed)
                return result;

            EstateDomainEvents.OperatorAddedToEstateEvent operatorAddedToEstateEvent =
                new EstateDomainEvents.OperatorAddedToEstateEvent(aggregate.AggregateId, operatorId);

            aggregate.ApplyAndAppend(operatorAddedToEstateEvent);

            return Result.Success();
        }

        public static Result RemoveOperator(this EstateAggregate aggregate,
                                       Guid operatorId)
        {

            Result result = aggregate.CheckEstateHasBeenCreated();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckOperatorHasBeenAdded(operatorId);
            if (result.IsFailed)
                return result;
            EstateDomainEvents.OperatorRemovedFromEstateEvent operatorRemovedFromEstateEvent = new(aggregate.AggregateId, operatorId);

            aggregate.ApplyAndAppend(operatorRemovedFromEstateEvent);

            return Result.Success();
        }

        public static Result AddSecurityUser(this EstateAggregate aggregate,
                                           Guid securityUserId,
                                           String emailAddress){
            Result result = aggregate.CheckEstateHasBeenCreated();
            if (result.IsFailed)
                return result;

            EstateDomainEvents.SecurityUserAddedToEstateEvent securityUserAddedEvent = new(aggregate.AggregateId, securityUserId, emailAddress);

            aggregate.ApplyAndAppend(securityUserAddedEvent);

            return Result.Success();
        }

        [Pure]
        public static Result Create(this EstateAggregate aggregate, String estateName){
            if (String.IsNullOrEmpty(estateName))
                return Result.Invalid("Estate name must be provided when registering a new estate");

            // Just return if already created
            if (aggregate.IsCreated)
                return Result.Success();

            EstateDomainEvents.EstateCreatedEvent estateCreatedEvent = new(aggregate.AggregateId, estateName);

            aggregate.ApplyAndAppend(estateCreatedEvent);

            String reference = $"{aggregate.AggregateId.GetHashCode():X}";

            EstateDomainEvents.EstateReferenceAllocatedEvent estateReferenceAllocatedEvent = new EstateDomainEvents.EstateReferenceAllocatedEvent(aggregate.AggregateId, reference);

            aggregate.ApplyAndAppend(estateReferenceAllocatedEvent);

            return Result.Success();
        }
        
        public static Estate GetEstate(this EstateAggregate aggregate) {
            Estate estateModel = new() { EstateId = aggregate.AggregateId, Name = aggregate.EstateName, Reference = aggregate.EstateReference, Operators = [] };

            if (aggregate.Operators.Any()) {

                foreach (KeyValuePair<Guid, Operator> @operator in aggregate.Operators) {
                    estateModel.Operators.Add(new Operator { OperatorId = @operator.Key, IsDeleted = @operator.Value.IsDeleted, });
                }
            }

            estateModel.SecurityUsers = new List<SecurityUser>();
            if (aggregate.SecurityUsers.Any()) {

                foreach (KeyValuePair<Guid, SecurityUser> securityUser in aggregate.SecurityUsers) {
                    estateModel.SecurityUsers.Add(new SecurityUser { EmailAddress = securityUser.Value.EmailAddress, SecurityUserId = securityUser.Key });
                }
            }

            return estateModel;
        }

        public static void PlayEvent(this EstateAggregate aggregate, EstateDomainEvents.SecurityUserAddedToEstateEvent domainEvent) {
            SecurityUser securityUser = new() { EmailAddress = domainEvent.EmailAddress, SecurityUserId = domainEvent.SecurityUserId };

            aggregate.SecurityUsers.Add(domainEvent.SecurityUserId,securityUser);
        }

        public static void PlayEvent(this EstateAggregate aggregate, EstateDomainEvents.EstateCreatedEvent domainEvent){
            aggregate.EstateName = domainEvent.EstateName;
            aggregate.IsCreated = true;
        }

        public static void PlayEvent(this EstateAggregate aggregate, EstateDomainEvents.EstateReferenceAllocatedEvent domainEvent){
            aggregate.EstateReference = domainEvent.EstateReference;
        }

        public static void PlayEvent(this EstateAggregate aggregate, EstateDomainEvents.OperatorAddedToEstateEvent domainEvent){
            TransactionProcessor.Models.Estate.Operator @operator = new() {
              IsDeleted  = false,
              OperatorId = domainEvent.OperatorId
            };

            aggregate.Operators.Add(domainEvent.OperatorId, @operator);
        }

        public static void PlayEvent(this EstateAggregate aggregate, EstateDomainEvents.OperatorRemovedFromEstateEvent domainEvent){
            aggregate.Operators[domainEvent.OperatorId].IsDeleted = true;
        }

        private static Result CheckEstateHasBeenCreated(this EstateAggregate aggregate){
            if (aggregate.IsCreated == false){
                return Result.Invalid("Estate has not been created");
            }
            return Result.Success();
        }

        private static Result CheckOperatorHasNotAlreadyBeenCreated(this EstateAggregate aggregate,
                                                                    Guid operatorId) {
            Boolean operatorRecordExists = aggregate.Operators.ContainsKey(operatorId);
            if (operatorRecordExists == true) {
                return Result.Invalid($"Duplicate operator details are not allowed, an operator already exists on this estate with Id [{operatorId}]");
            }
            return Result.Success();
        }

        private static Result CheckOperatorHasBeenAdded(this EstateAggregate aggregate,
                                                        Guid operatorId) {
            Boolean operatorRecordExists = aggregate.Operators.ContainsKey(operatorId);
            if (operatorRecordExists == false) {
                return Result.Invalid($"Operator not added to this Estate with Id [{operatorId}]");
            }
            return Result.Success();
        }

        #endregion
    }

    public record EstateAggregate : Aggregate{
        #region Fields

        internal readonly Dictionary<Guid, TransactionProcessor.Models.Estate.Operator> Operators;

        internal readonly Dictionary<Guid, SecurityUser> SecurityUsers;

        #endregion

        #region Constructors
        
        [ExcludeFromCodeCoverage]
        public EstateAggregate(){
            // Nothing here
            this.Operators = new Dictionary<Guid, TransactionProcessor.Models.Estate.Operator>();
            this.SecurityUsers = new Dictionary<Guid, SecurityUser>();
        }
        
        private EstateAggregate(Guid aggregateId){
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.Operators = new Dictionary<Guid, TransactionProcessor.Models.Estate.Operator>();
            this.SecurityUsers = new Dictionary<Guid, SecurityUser>();
        }

        #endregion

        #region Properties

        public String EstateName{ get; internal set; }

        public String EstateReference{ get; internal set; }

        public Boolean IsCreated{ get; internal set; }

        #endregion

        #region Methods

        public static EstateAggregate Create(Guid aggregateId){
            return new EstateAggregate(aggregateId);
        }

        public override void PlayEvent(IDomainEvent domainEvent) => EstateAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata(){
            return new{
                          EstateId = this.AggregateId
                      };
        }

        #endregion
    }
}