namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;
    using Newtonsoft.Json;
    using OperatorInterfaces;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Logger;
    using TransactionAggregate;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.BusinessLogic.Services.ITransactionAggregateManager" />
    public class TransactionAggregateManager : ITransactionAggregateManager
    {
        #region Fields

        private readonly IAggregateRepository<TransactionAggregate, DomainEvent> TransactionAggregateRepository;

        #endregion

        #region Constructors

        public TransactionAggregateManager(IAggregateRepository<TransactionAggregate, DomainEvent> transactionAggregateRepository) {
            this.TransactionAggregateRepository = transactionAggregateRepository;
        }

        #endregion

        #region Methods

        public async Task AddFee(Guid estateId,
                                 Guid transactionId,
                                 CalculatedFee calculatedFee,
                                 CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.AddFee(calculatedFee);

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        public async Task AddProductDetails(Guid estateId,
                                            Guid transactionId,
                                            Guid contractId,
                                            Guid productId,
                                            CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.AddProductDetails(contractId, productId);

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        public async Task AddSettledFee(Guid estateId,
                                        Guid transactionId,
                                        CalculatedFee calculatedFee,
                                        DateTime settlementDueDate,
                                        DateTime settledDateTime,
                                        CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.AddSettledFee(calculatedFee, settlementDueDate, settledDateTime);

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        public async Task AddTransactionSource(Guid estateId,
                                               Guid transactionId,
                                               TransactionSource transactionSource,
                                               CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.AddTransactionSource(transactionSource);

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);

        }

        public async Task AuthoriseTransaction(Guid estateId,
                                               Guid transactionId,
                                               String operatorIdentifier,
                                               OperatorResponse operatorResponse,
                                               TransactionResponseCode transactionResponseCode,
                                               String responseMessage,
                                               CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.AuthoriseTransaction(operatorIdentifier,
                                                      operatorResponse.AuthorisationCode,
                                                      operatorResponse.ResponseCode,
                                                      operatorResponse.ResponseMessage,
                                                      operatorResponse.TransactionId,
                                                      ((Int32)transactionResponseCode).ToString().PadLeft(4, '0'),
                                                      responseMessage);

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        public async Task AuthoriseTransactionLocally(Guid estateId,
                                                      Guid transactionId,
                                                      String authorisationCode,
                                                      (String responseMessage, TransactionResponseCode responseCode) validationResult,
                                                      CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.AuthoriseTransactionLocally(authorisationCode,
                                                             ((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'),
                                                             validationResult.responseMessage);

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        public async Task CompleteTransaction(Guid estateId,
                                              Guid transactionId,
                                              CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.CompleteTransaction();

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        public async Task DeclineTransaction(Guid estateId,
                                             Guid transactionId,
                                             String operatorIdentifier,
                                             OperatorResponse operatorResponse,
                                             TransactionResponseCode transactionResponseCode,
                                             String responseMessage,
                                             CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.DeclineTransaction(operatorIdentifier,
                                                    operatorResponse.ResponseCode,
                                                    operatorResponse.ResponseMessage,
                                                    ((Int32)transactionResponseCode).ToString().PadLeft(4, '0'),
                                                    responseMessage);

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        public async Task DeclineTransactionLocally(Guid estateId,
                                                    Guid transactionId,
                                                    (String responseMessage, TransactionResponseCode responseCode) validationResult,
                                                    CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.DeclineTransactionLocally(((Int32)validationResult.responseCode).ToString().PadLeft(4, '0'), validationResult.responseMessage);

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        public async Task<TransactionAggregate> GetAggregate(Guid estateId,
                                                             Guid transactionId,
                                                             CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
            return transactionAggregate;
        }

        public async Task RecordAdditionalRequestData(Guid estateId,
                                                      Guid transactionId,
                                                      String operatorIdentifier,
                                                      Dictionary<String, String> additionalTransactionRequestMetadata,
                                                      CancellationToken cancellationToken) {
            if (additionalTransactionRequestMetadata != null && additionalTransactionRequestMetadata.Any()) {
                TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

                transactionAggregate.RecordAdditionalRequestData(operatorIdentifier, additionalTransactionRequestMetadata);

                await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
            }
        }

        public async Task RecordAdditionalResponseData(Guid estateId,
                                                       Guid transactionId,
                                                       String operatorIdentifier,
                                                       Dictionary<String, String> additionalTransactionResponseMetadata,
                                                       CancellationToken cancellationToken) {
            if (additionalTransactionResponseMetadata != null && additionalTransactionResponseMetadata.Any()) {
                TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

                transactionAggregate.RecordAdditionalResponseData(operatorIdentifier, additionalTransactionResponseMetadata);

                await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
            }
        }

        public async Task RequestEmailReceipt(Guid estateId,
                                              Guid transactionId,
                                              String customerEmailAddress,
                                              CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.RequestEmailReceipt(customerEmailAddress);

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        public async Task ResendReceipt(Guid estateId,
                                  Guid transactionId,
                                  CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
            
            var aggregateJson = JsonConvert.SerializeObject(transactionAggregate, Formatting.Indented);
            Logger.LogInformation($"Transaction Id is [{transactionId}]");
            Logger.LogInformation(aggregateJson);

            transactionAggregate.RequestEmailReceiptResend();

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        public async Task StartTransaction(Guid transactionId,
                                           DateTime transactionDateTime,
                                           String transactionNumber,
                                           TransactionType transactionType,
                                           String transactionReference,
                                           Guid estateId,
                                           Guid merchantId,
                                           String deviceIdentifier,
                                           Decimal? transactionAmount,
                                           CancellationToken cancellationToken) {
            TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);

            transactionAggregate.StartTransaction(transactionDateTime,
                                                  transactionNumber,
                                                  transactionType,
                                                  transactionReference,
                                                  estateId,
                                                  merchantId,
                                                  deviceIdentifier,
                                                  transactionAmount);

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        #endregion
    }
}