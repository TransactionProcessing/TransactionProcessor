using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using Shared.ValueObjects;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.Aggregates{
    public static class MerchantDepositListAggregateExtensions{
        #region Methods

        public static void Create(this MerchantDepositListAggregate aggregate,
                                  MerchantAggregate merchantAggregate,
                                  DateTime dateCreated){
            aggregate.EnsureMerchantHasBeenCreated(merchantAggregate);
            // Ensure this aggregate has not already been created
            if (aggregate.IsCreated)
                return;

            MerchantDomainEvents.MerchantDepositListCreatedEvent merchantDepositListCreatedEvent =
                new MerchantDomainEvents.MerchantDepositListCreatedEvent(aggregate.AggregateId, merchantAggregate.EstateId, dateCreated);

            aggregate.ApplyAndAppend(merchantDepositListCreatedEvent);
        }

        public static List<TransactionProcessor.Models.Merchant.Deposit> GetDeposits(this MerchantDepositListAggregate aggregate){
            List<TransactionProcessor.Models.Merchant.Deposit> deposits = new ();
            if (aggregate.Deposits.Any()){
                aggregate.Deposits.ForEach(d => deposits.Add(new TransactionProcessor.Models.Merchant.Deposit {
                    Amount = d.Amount,
                    DepositDateTime = d.DepositDateTime,
                    DepositId = d.DepositId,
                    Reference = d.Reference,
                    Source = d.Source
                }));
            }

            return deposits;
        }

        public static List<Withdrawal> GetWithdrawals(this MerchantDepositListAggregate aggregate){
            List<Withdrawal> withdrawals = new List<Withdrawal>();
            if (aggregate.Withdrawals.Any()){
                aggregate.Withdrawals.ForEach(d => withdrawals.Add(new Withdrawal{
                                                                                     WithdrawalDateTime = d.WithdrawalDateTime,
                                                                                     WithdrawalId = d.WithdrawalId,
                                                                                     Amount = d.Amount
                                                                                 }));
            }

            return withdrawals;
        }

        public static void MakeDeposit(this MerchantDepositListAggregate aggregate,
                                       MerchantDepositSource source,
                                       String reference,
                                       DateTime depositDateTime,
                                       PositiveMoney amount){
            String depositData = $"{depositDateTime.ToString("yyyyMMdd hh:mm:ss.fff")}-{reference}-{amount:N2}-{source}";
            Guid depositId = aggregate.GenerateGuidFromString(depositData);

            aggregate.EnsureMerchantDepositListHasBeenCreated();
            aggregate.EnsureNotDuplicateDeposit(depositId);
            // TODO: Change amount to a value object (PositiveAmount VO)
            aggregate.EnsureDepositSourceHasBeenSet(source);

            if (source == MerchantDepositSource.Manual){
                MerchantDomainEvents.ManualDepositMadeEvent manualDepositMadeEvent =
                    new MerchantDomainEvents.ManualDepositMadeEvent(aggregate.AggregateId, aggregate.EstateId, depositId, reference, depositDateTime, amount.Value);
                aggregate.ApplyAndAppend(manualDepositMadeEvent);
            }
            else if (source == MerchantDepositSource.Automatic){
                MerchantDomainEvents.AutomaticDepositMadeEvent automaticDepositMadeEvent =
                    new MerchantDomainEvents.AutomaticDepositMadeEvent(aggregate.AggregateId, aggregate.EstateId, depositId, reference, depositDateTime, amount.Value);
                aggregate.ApplyAndAppend(automaticDepositMadeEvent);
            }
        }

        public static void MakeWithdrawal(this MerchantDepositListAggregate aggregate,
                                          DateTime withdrawalDateTime,
                                          PositiveMoney amount){
            String depositData = $"{withdrawalDateTime.ToString("yyyyMMdd hh:mm:ss.fff")}-{amount:N2}";
            Guid withdrawalId = aggregate.GenerateGuidFromString(depositData);

            aggregate.EnsureMerchantDepositListHasBeenCreated();
            aggregate.EnsureNotDuplicateWithdrawal(withdrawalId);

            MerchantDomainEvents.WithdrawalMadeEvent withdrawalMadeEvent = new(aggregate.AggregateId, aggregate.EstateId, withdrawalId, withdrawalDateTime, amount.Value);
            aggregate.ApplyAndAppend(withdrawalMadeEvent);
        }

        public static void PlayEvent(this MerchantDepositListAggregate aggregate, MerchantDomainEvents.MerchantDepositListCreatedEvent merchantDepositListCreatedEvent){
            aggregate.IsCreated = true;
            aggregate.EstateId = merchantDepositListCreatedEvent.EstateId;
            aggregate.DateCreated = merchantDepositListCreatedEvent.DateCreated;
            aggregate.AggregateId = merchantDepositListCreatedEvent.AggregateId;
        }

        public static void PlayEvent(this MerchantDepositListAggregate aggregate, MerchantDomainEvents.ManualDepositMadeEvent manualDepositMadeEvent){
            Models.Deposit deposit = new(manualDepositMadeEvent.DepositId,
                                             MerchantDepositSource.Manual,
                                             manualDepositMadeEvent.Reference,
                                             manualDepositMadeEvent.DepositDateTime,
                                             manualDepositMadeEvent.Amount);
            aggregate.Deposits.Add(deposit);
        }

        public static void PlayEvent(this MerchantDepositListAggregate aggregate, MerchantDomainEvents.AutomaticDepositMadeEvent automaticDepositMadeEvent){
            Models.Deposit deposit = new(automaticDepositMadeEvent.DepositId,
                                             MerchantDepositSource.Automatic,
                                             automaticDepositMadeEvent.Reference,
                                             automaticDepositMadeEvent.DepositDateTime,
                                             automaticDepositMadeEvent.Amount);
            aggregate.Deposits.Add(deposit);
        }

        public static void PlayEvent(this MerchantDepositListAggregate aggregate, MerchantDomainEvents.WithdrawalMadeEvent withdrawalMadeEvent){
            Models.Withdrawal withdrawal = new(withdrawalMadeEvent.WithdrawalId, withdrawalMadeEvent.WithdrawalDateTime, withdrawalMadeEvent.Amount);
            aggregate.Withdrawals.Add(withdrawal);
        }

        private static void EnsureDepositSourceHasBeenSet(this MerchantDepositListAggregate aggregate, MerchantDepositSource merchantDepositSource){
            if (merchantDepositSource == MerchantDepositSource.NotSet){
                throw new InvalidOperationException("Merchant deposit source must be set");
            }
        }

        private static void EnsureMerchantDepositListHasBeenCreated(this MerchantDepositListAggregate aggregate){
            if (aggregate.IsCreated == false){
                throw new InvalidOperationException("Merchant Deposit List has not been created");
            }
        }

        private static void EnsureMerchantHasBeenCreated(this MerchantDepositListAggregate aggregate, MerchantAggregate merchantAggregate){
            if (merchantAggregate.IsCreated == false){
                throw new InvalidOperationException("Merchant has not been created");
            }
        }

        private static void EnsureNotDuplicateDeposit(this MerchantDepositListAggregate aggregate, Guid depositId){
            if (aggregate.Deposits.Any(d => d.DepositId == depositId)){
                throw new InvalidOperationException($"Deposit Id [{depositId}] already made for merchant [{aggregate.AggregateId}]");
            }
        }

        private static void EnsureNotDuplicateWithdrawal(this MerchantDepositListAggregate aggregate, Guid withdrawalId){
            if (aggregate.Withdrawals.Any(d => d.WithdrawalId == withdrawalId)){
                throw new InvalidOperationException($"Withdrawal Id [{withdrawalId}] already made for merchant [{aggregate.AggregateId}]");
            }
        }

        private static Guid GenerateGuidFromString(this MerchantDepositListAggregate aggregate, String input){
            using(SHA256 sha256Hash = SHA256.Create()){
                //Generate hash from the key
                Byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                Byte[] j = bytes.Skip(Math.Max(0, bytes.Count() - 16)).ToArray(); //Take last 16

                //Create our Guid.
                return new Guid(j);
            }
        }

        #endregion
    }

    public record MerchantDepositListAggregate : Aggregate{
        #region Fields

        internal readonly List<Models.Deposit> Deposits;

        internal readonly List<Models.Withdrawal> Withdrawals;

        #endregion

        #region Constructors

        [ExcludeFromCodeCoverage]
        public MerchantDepositListAggregate(){
            // Nothing here
            this.Deposits = new List<Models.Deposit>();
            this.Withdrawals = new List<Models.Withdrawal>();
        }

        private MerchantDepositListAggregate(Guid aggregateId){
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.Deposits = new List<Models.Deposit>();
            this.Withdrawals = new List<Models.Withdrawal>();
        }

        #endregion

        #region Properties

        public DateTime DateCreated{ get; internal set; }

        public Guid EstateId{ get; internal set; }

        public Boolean IsCreated{ get; internal set; }

        #endregion

        #region Methods

        public static MerchantDepositListAggregate Create(Guid aggregateId){
            return new MerchantDepositListAggregate(aggregateId);
        }

        public override void PlayEvent(IDomainEvent domainEvent) => MerchantDepositListAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata(){
            return new{
                          this.EstateId,
                          MerchantId = this.AggregateId
                      };
        }

        #endregion
    }
}