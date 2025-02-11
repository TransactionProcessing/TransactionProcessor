using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using TransactionProcessor.Aggregates.Models;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.Aggregates
{
    public static class MerchantStatementAggregateExtenions{
        public static void AddSettledFeeToStatement(this MerchantStatementAggregate aggregate,
                                                    Guid statementId,
                                                    Guid eventId,
                                                    DateTime createdDate,
                                                    Guid estateId,
                                                    Guid merchantId,
                                                    SettledFee settledFee){
            // Create statement id required
            aggregate.CreateStatement(statementId, createdDate, estateId, merchantId);

            SettledFeeAddedToStatementEvent settledFeeAddedToStatementEvent =
                new SettledFeeAddedToStatementEvent(aggregate.AggregateId,
                                                    eventId,
                                                    aggregate.EstateId,
                                                    aggregate.MerchantId,
                                                    settledFee.SettledFeeId,
                                                    settledFee.TransactionId,
                                                    settledFee.DateTime,
                                                    settledFee.Amount);

            aggregate.ApplyAndAppend(settledFeeAddedToStatementEvent);
        }

        public static void AddTransactionToStatement(this MerchantStatementAggregate aggregate,
                                                     Guid statementId,
                                                     Guid eventId,
                                                     DateTime createdDate,
                                                     Guid estateId,
                                                     Guid merchantId,
                                                     Transaction transaction){
            // Create statement id required
            aggregate.CreateStatement(statementId, createdDate, estateId, merchantId);

            TransactionAddedToStatementEvent transactionAddedToStatementEvent = new TransactionAddedToStatementEvent(aggregate.AggregateId,
                                                                                                                     eventId,
                                                                                                                     aggregate.EstateId,
                                                                                                                     aggregate.MerchantId,
                                                                                                                     transaction.TransactionId,
                                                                                                                     transaction.DateTime,
                                                                                                                     transaction.Amount);

            aggregate.ApplyAndAppend(transactionAddedToStatementEvent);
        }

        private static void CreateStatement(this MerchantStatementAggregate aggregate, 
                                            Guid statementId,
                                            DateTime createdDate,
                                            Guid estateId,
                                            Guid merchantId)
        {
            if (aggregate.IsCreated == false)
            {
                Guard.ThrowIfInvalidGuid(statementId, nameof(statementId));
                Guard.ThrowIfInvalidGuid(estateId, nameof(estateId));
                Guard.ThrowIfInvalidGuid(merchantId, nameof(merchantId));

                StatementCreatedEvent statementCreatedEvent = new StatementCreatedEvent(statementId, estateId, merchantId, createdDate);

                aggregate.ApplyAndAppend(statementCreatedEvent);
            }
        }

        public static void EmailStatement(this MerchantStatementAggregate aggregate, 
                                          DateTime emailedDateTime,
                                          Guid messageId)
        {
            aggregate.EnsureStatementHasBeenCreated();
            aggregate.EnsureStatementHasBeenGenerated();

            StatementEmailedEvent statementEmailedEvent = new StatementEmailedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, emailedDateTime, messageId);

            aggregate.ApplyAndAppend(statementEmailedEvent);
        }

        public static void GenerateStatement(this MerchantStatementAggregate aggregate, 
                                             DateTime generatedDateTime)
        {
            aggregate.EnsureStatementHasNotAlreadyBeenGenerated();

            if (aggregate.Transactions.Any() == false && aggregate.SettledFees.Any() == false)
            {
                throw new InvalidOperationException("Statement has no transactions or settled fees");
            }

            StatementGeneratedEvent statementGeneratedEvent = new StatementGeneratedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, generatedDateTime);

            aggregate.ApplyAndAppend(statementGeneratedEvent);
        }

        public static MerchantStatement GetStatement(this MerchantStatementAggregate aggregate, Boolean includeStatementLines = false)
        {
            MerchantStatement merchantStatement = new MerchantStatement
            {
                EstateId = aggregate.EstateId,
                MerchantId = aggregate.MerchantId,
                MerchantStatementId = aggregate.AggregateId,
                IsCreated = aggregate.IsCreated,
                IsGenerated = aggregate.IsGenerated,
                HasBeenEmailed = aggregate.HasBeenEmailed,
                StatementCreatedDateTime = aggregate.CreatedDateTime,
                StatementGeneratedDateTime = aggregate.GeneratedDateTime
            };

            if (includeStatementLines)
            {
                foreach (Transaction transaction in aggregate.Transactions)
                {
                    merchantStatement.AddStatementLine(new MerchantStatementLine
                    {
                        Amount = transaction.Amount,
                        DateTime = transaction.DateTime,
                        Description = string.Empty,
                        LineType = 1 // Transaction
                    });
                }

                foreach (SettledFee settledFee in aggregate.SettledFees)
                {
                    merchantStatement.AddStatementLine(new MerchantStatementLine
                    {
                        Amount = settledFee.Amount,
                        DateTime = settledFee.DateTime,
                        Description = string.Empty,
                        LineType = 2 // Settled Fee
                    });
                }
            }

            return merchantStatement;
        }

        private static void EnsureStatementHasBeenCreated(this MerchantStatementAggregate aggregate)
        {
            if (aggregate.IsCreated == false)
            {
                throw new InvalidOperationException("Statement has not been created");
            }
        }

        private static void EnsureStatementHasBeenGenerated(this MerchantStatementAggregate aggregate)
        {
            if (aggregate.IsGenerated == false)
            {
                throw new InvalidOperationException("Statement has not been generated");
            }
        }

        private static void EnsureStatementHasNotAlreadyBeenGenerated(this MerchantStatementAggregate aggregate)
        {
            if (aggregate.IsGenerated)
            {
                throw new InvalidOperationException("Statement has already been generated");
            }
        }

        public static void PlayEvent(this MerchantStatementAggregate aggregate, StatementCreatedEvent domainEvent)
        {
            aggregate.IsCreated = true;
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.MerchantId = domainEvent.MerchantId;
            aggregate.CreatedDateTime = domainEvent.DateCreated;
        }

        public static void PlayEvent(this MerchantStatementAggregate aggregate, TransactionAddedToStatementEvent domainEvent)
        {
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.MerchantId = domainEvent.MerchantId;

            aggregate.Transactions.Add(new Transaction(domainEvent.TransactionId, domainEvent.TransactionDateTime, domainEvent.TransactionValue));
        }

        public static void PlayEvent(this MerchantStatementAggregate aggregate, SettledFeeAddedToStatementEvent domainEvent)
        {
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.MerchantId = domainEvent.MerchantId;

            aggregate.SettledFees.Add(new SettledFee(domainEvent.SettledFeeId, domainEvent.TransactionId, domainEvent.SettledDateTime, domainEvent.SettledValue));
        }

        public static void PlayEvent(this MerchantStatementAggregate aggregate, StatementGeneratedEvent domainEvent)
        {
            aggregate.IsGenerated = true;
            aggregate.GeneratedDateTime = domainEvent.DateGenerated;
        }

        public static void PlayEvent(this MerchantStatementAggregate aggregate, StatementEmailedEvent domainEvent)
        {
            aggregate.HasBeenEmailed = true;
            aggregate.EmailedDateTime = domainEvent.DateEmailed;
            aggregate.EmailMessageId = domainEvent.MessageId;
        }
    }

    public record MerchantStatementAggregate : Aggregate
    {
        #region Fields

        /// <summary>
        /// The created date time
        /// </summary>
        internal DateTime CreatedDateTime;

        /// <summary>
        /// The emailed date time
        /// </summary>
        internal DateTime EmailedDateTime;

        /// <summary>
        /// The email message identifier
        /// </summary>
        internal Guid EmailMessageId;

        /// <summary>
        /// The estate identifier
        /// </summary>
        internal Guid EstateId;

        /// <summary>
        /// The generated date time
        /// </summary>
        internal DateTime GeneratedDateTime;

        /// <summary>
        /// The has been emailed
        /// </summary>
        internal Boolean HasBeenEmailed;

        /// <summary>
        /// The is created
        /// </summary>
        internal Boolean IsCreated;

        /// <summary>
        /// The is generated
        /// </summary>
        internal Boolean IsGenerated;

        /// <summary>
        /// The merchant identifier
        /// </summary>
        internal Guid MerchantId;

        /// <summary>
        /// The settled fees
        /// </summary>
        internal readonly List<SettledFee> SettledFees;

        /// <summary>
        /// The transactions
        /// </summary>
        internal readonly List<Transaction> Transactions;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MerchantStatementAggregate"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public MerchantStatementAggregate()
        {
            // Nothing here
            this.Transactions = new List<Transaction>();
            this.SettledFees = new List<SettledFee>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MerchantStatementAggregate"/> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        private MerchantStatementAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.Transactions = new List<Transaction>();
            this.SettledFees = new List<SettledFee>();
        }

        #endregion

        #region Methods
        
        public static MerchantStatementAggregate Create(Guid aggregateId)
        {
            return new MerchantStatementAggregate(aggregateId);
        }

        public override void PlayEvent(IDomainEvent domainEvent) => MerchantStatementAggregateExtenions.PlayEvent(this, (dynamic)domainEvent);
        
        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return null;
        }
        
        #endregion
    }
}