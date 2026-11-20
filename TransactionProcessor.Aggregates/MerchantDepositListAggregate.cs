using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.General;
using TransactionProcessing.Shared.ValueObjects;
using TransactionProcessing.UnionResults;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.Aggregates{
    public static class MerchantDepositListAggregateExtensions{
        #region Methods

        public static Result Create(this MerchantDepositListAggregate aggregate,
                                  MerchantAggregate merchantAggregate,
                                  DateTimeOffset dateCreated){
            Result result = aggregate.EnsureMerchantHasBeenCreated(merchantAggregate);
            if (result.IsSuccess() == false)
                return result;

            // Ensure this aggregate has not already been created
            if (aggregate.IsCreated)
                return Results.Ok();

            MerchantDomainEvents.MerchantDepositListCreatedEvent merchantDepositListCreatedEvent = new(aggregate.AggregateId, merchantAggregate.EstateId, dateCreated);

            aggregate.ApplyAndAppend(merchantDepositListCreatedEvent);

            return Results.Ok();
        }

        public static List<Deposit> GetDeposits(this MerchantDepositListAggregate aggregate){
            List<Deposit> deposits = new ();
            if (aggregate.Deposits.Any()){
                aggregate.Deposits.ForEach(d => deposits.Add(new Deposit {
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

        public static Result MakeDeposit(this MerchantDepositListAggregate aggregate,
                                       MerchantDepositSource source,
                                       String reference,
                                       DateTimeOffset depositDateTime,
                                       PositiveMoney amount,
                                       DateTimeOffset hostDateTime) {
            String depositData = $"{depositDateTime:yyyyMMdd hh:mm:ss.fff}-{reference}-{amount:N2}-{source}";
            Guid depositId = aggregate.GenerateGuidFromString(depositData);

            Result result = aggregate.EnsureMerchantDepositListHasBeenCreated();
            if (result.IsSuccess() == false)
                return result;

            result = aggregate.EnsureNotDuplicateDeposit(depositId);
            if (result.IsSuccess() == false)
                return result;

            result = aggregate.EnsureDepositSourceHasBeenSet(source);
            if (result.IsSuccess() == false)
                return result;

            if (source == MerchantDepositSource.Manual){
                MerchantDomainEvents.ManualDepositMadeEvent manualDepositMadeEvent = new(aggregate.AggregateId, aggregate.EstateId, depositId, reference, depositDateTime, amount.Value, hostDateTime);
                aggregate.ApplyAndAppend(manualDepositMadeEvent);
                return Results.Ok();
            }

            if (source == MerchantDepositSource.Automatic){
                MerchantDomainEvents.AutomaticDepositMadeEvent automaticDepositMadeEvent = new(aggregate.AggregateId, aggregate.EstateId, depositId, reference, depositDateTime, amount.Value, hostDateTime);
                aggregate.ApplyAndAppend(automaticDepositMadeEvent);
                return Results.Ok();
            }

            return new Error(ErrorType.Validation, "Invalid Deposit Source");
        }

        public static Result MakeWithdrawal(this MerchantDepositListAggregate aggregate,
                                            DateTimeOffset withdrawalDateTime,
                                          PositiveMoney amount, DateTimeOffset hostDateTime){
            String depositData = $"{withdrawalDateTime.ToString("yyyyMMdd hh:mm:ss.fff")}-{amount:N2}";
            Guid withdrawalId = aggregate.GenerateGuidFromString(depositData);

            Result result = aggregate.EnsureMerchantDepositListHasBeenCreated();
            if (result.IsSuccess() == false)
                return result;
            result = aggregate.EnsureNotDuplicateWithdrawal(withdrawalId);
            if (result.IsSuccess() == false)
                return result;

            MerchantDomainEvents.WithdrawalMadeEvent withdrawalMadeEvent = new(aggregate.AggregateId, aggregate.EstateId, withdrawalId, withdrawalDateTime, amount.Value,
                hostDateTime);
            aggregate.ApplyAndAppend(withdrawalMadeEvent);

            return Results.Ok();
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

        private static Result EnsureDepositSourceHasBeenSet(this MerchantDepositListAggregate aggregate, MerchantDepositSource merchantDepositSource){
            if (merchantDepositSource == MerchantDepositSource.NotSet){
                return new Error(ErrorType.Validation, "Merchant deposit source must be set");
            }
            return Results.Ok();
        }

        private static Result EnsureMerchantDepositListHasBeenCreated(this MerchantDepositListAggregate aggregate){
            if (aggregate.IsCreated == false){
                return new Error(ErrorType.Validation, "Merchant Deposit List has not been created");
            }
            return Results.Ok();
        }

        private static Result EnsureMerchantHasBeenCreated(this MerchantDepositListAggregate aggregate, MerchantAggregate merchantAggregate){
            if (merchantAggregate.IsCreated == false){
                return new Error(ErrorType.Validation, "Merchant has not been created");
            }
            return Results.Ok();
        }

        private static Result EnsureNotDuplicateDeposit(this MerchantDepositListAggregate aggregate, Guid depositId){
            if (aggregate.Deposits.Any(d => d.DepositId == depositId)){
                return new Error(ErrorType.Validation, $"Deposit Id [{depositId}] already made for merchant [{aggregate.AggregateId}]");
            }
            return Results.Ok();
        }

        private static Result EnsureNotDuplicateWithdrawal(this MerchantDepositListAggregate aggregate, Guid withdrawalId){
            if (aggregate.Withdrawals.Any(d => d.WithdrawalId == withdrawalId)){
                return new Error(ErrorType.Validation, $"Withdrawal Id [{withdrawalId}] already made for merchant [{aggregate.AggregateId}]");
            }
            return Results.Ok();
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
            if (aggregateId == Guid.Empty)
                throw new ArgumentNullException(nameof(aggregateId));

            this.AggregateId = aggregateId;
            this.Deposits = new List<Models.Deposit>();
            this.Withdrawals = new List<Models.Withdrawal>();
        }

        #endregion

        #region Properties

        public DateTimeOffset DateCreated{ get; internal set; }

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