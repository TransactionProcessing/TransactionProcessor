using TransactionProcessing.UnionResults;
using TransactionProcessing.UnionResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.BusinessLogic.Services
{
    using Common;
    using Models;
    using Polly;
    using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
    using TransactionProcessing.Shared.EventStore.Aggregate;
    using TransactionProcessing.Shared.Exceptions;
    using TransactionProcessing.Shared.Logger;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TransactionProcessor.Database.Entities;

    public interface ISettlementDomainService
    {
        Task<Result<Guid>> ProcessSettlement(SettlementCommands.ProcessSettlementCommand command, CancellationToken cancellationToken);

        Task<Result> AddMerchantFeePendingSettlement(SettlementCommands.AddMerchantFeePendingSettlementCommand command,
                                                     CancellationToken cancellationToken);

        Task<Result> AddSettledFeeToSettlement(SettlementCommands.AddSettledFeeToSettlementCommand command,
                                               CancellationToken cancellationToken);
    }
    public class SettlementDomainService : ISettlementDomainService
    {
        private readonly IAggregateService AggregateService;
        
        public async Task<Result<Guid>> ProcessSettlement(SettlementCommands.ProcessSettlementCommand command,
                                                          CancellationToken cancellationToken) {

            IAsyncPolicy<Result<Guid>> retryPolicy = PolicyFactory.CreatePolicy<Result<Guid>>(policyTag: "SettlementDomainService - ProcessSettlement");

            try {
                return await PolicyFactory.ExecuteWithPolicyAsync(() => this.ProcessSettlementInternal(command, cancellationToken), retryPolicy, "SettlementDomainService - ProcessSettlement");
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        private async Task<Result<Guid>> ProcessSettlementInternal(SettlementCommands.ProcessSettlementCommand command,
                                                                   CancellationToken cancellationToken) {
            Guid settlementId = IdGenerationService.GenerateSettlementId(command.EstateId, command.MerchantId,command.SettlementDate);

            Result<SettlementAggregate> getSettlementResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<SettlementAggregate>(settlementId, ct), settlementId, cancellationToken, false);
            if (getSettlementResult.IsFailed)
                return ResultHelpers.CreateFailure(getSettlementResult);

            SettlementAggregate settlementAggregate = getSettlementResult.GetValueOrThrow();

            if (settlementAggregate.IsCreated == false) {
                Logger.LogInformation($"No pending settlement for {command.SettlementDate:yyyy-MM-dd}");
                return Result.Success();
            }

            Result<MerchantAggregate> getMerchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken, false);
            if (getMerchantResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantResult);

            Result<List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)>> settlementPreparationResult = await this.PrepareSettlementForProcessing(settlementAggregate, getMerchantResult.GetValueOrThrow(), cancellationToken);
            if (settlementPreparationResult.IsFailed)
                return ResultHelpers.CreateFailure(settlementPreparationResult);

            return await this.ProcessSettlementFees(settlementPreparationResult.GetValueOrThrow(), command.SettlementDate, settlementId, cancellationToken);
        }

        private async Task<Result<List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)>>> PrepareSettlementForProcessing(SettlementAggregate settlementAggregate,
                                                                                                                                             MerchantAggregate merchantAggregate,
                                                                                                                                             CancellationToken cancellationToken) {
            Result settlementSaveResult = Result.Success();
            if (merchantAggregate.SettlementSchedule == SettlementSchedule.Immediate) {
                settlementAggregate.StartProcessing(DateTimeOffset.UtcNow);
                settlementAggregate.ManuallyComplete();
                settlementSaveResult = await this.AggregateService.Save(settlementAggregate, cancellationToken);
            }

            List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> feesToBeSettled = settlementAggregate.GetFeesToBeSettled();

            if (feesToBeSettled.Any()) {
                Result stateResult = settlementAggregate.StartProcessing(DateTimeOffset.UtcNow);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                settlementSaveResult = await this.AggregateService.Save(settlementAggregate, cancellationToken);
            }

            if (settlementSaveResult.IsFailed)
                return ResultHelpers.CreateFailure(settlementSaveResult);

            return Result.Success(feesToBeSettled);
        }

        private async Task<Result<Guid>> ProcessSettlementFees(List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> feesToBeSettled,
                                                               DateTimeOffset settlementDate,
                                                               Guid settlementId,
                                                               CancellationToken cancellationToken) {
            List<Result> failedResults = new();
            foreach ((Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) feeToSettle in feesToBeSettled) {
                try {
                    Result<TransactionAggregate> getTransactionResult = await this.AggregateService.GetLatest<TransactionAggregate>(feeToSettle.transactionId, cancellationToken);
                    if (getTransactionResult.IsFailed)
                        return ResultHelpers.CreateFailure(getTransactionResult);

                    TransactionAggregate transactionAggregate = getTransactionResult.GetValueOrThrow();
                    transactionAggregate.AddSettledFee(feeToSettle.calculatedFee, settlementDate, settlementId);

                    Result saveResult = await this.AggregateService.Save(transactionAggregate, cancellationToken);
                    if (saveResult.IsFailed)
                        return ResultHelpers.CreateFailure(saveResult);
                }
                catch (Exception ex) {
                    Logger.LogError(ex,$"Failed to process transaction {feeToSettle.transactionId} for settlement {settlementId}");
                    failedResults.Add(Result.Failure($"Failed to process transaction {feeToSettle.transactionId} for settlement {settlementId}: {ex.Message}"));
                }
            }

            if (failedResults.Any()) {
                return Result.Failure($"Not all fees were processed successfully {failedResults.Count} have failed");
            }

            return Result.Success(settlementId);
        }

        public async Task<Result> AddMerchantFeePendingSettlement(SettlementCommands.AddMerchantFeePendingSettlementCommand command,
                                                                  CancellationToken cancellationToken) {
            
            try{
            Logger.LogInformation("Processing command AddMerchantFeePendingSettlementCommand");
            
            Guid settlementId = IdGenerationService.GenerateSettlementId(command.EstateId, command.MerchantId, command.SettlementDueDate);

                Result<SettlementAggregate> getSettlementResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<SettlementAggregate>(settlementId, ct), settlementId, cancellationToken, false);
            if (getSettlementResult.IsFailed)
                return ResultHelpers.CreateFailure(getSettlementResult);

            SettlementAggregate settlementAggregate = getSettlementResult.GetValueOrThrow();

                if (settlementAggregate.IsCreated == false)
                {
                    Result createResult = settlementAggregate.Create(command.EstateId, command.MerchantId, command.SettlementDueDate.Date);
                    if (createResult.IsFailed)
                        return ResultHelpers.CreateFailure(createResult);
                }

                // Create Calculated Fee from the domain event
                CalculatedFee calculatedFee = new CalculatedFee
                {
                    CalculatedValue = command.CalculatedValue,
                    FeeCalculatedDateTime = command.FeeCalculatedDateTime,
                    FeeCalculationType = command.FeeCalculationType,
                    FeeId = command.FeeId,
                    FeeType = FeeType.Merchant,
                    FeeValue = command.FeeValue,
                    SettlementDueDate = command.SettlementDueDate
                };
                Result stateResult = settlementAggregate.AddFee(command.MerchantId, command.TransactionId, calculatedFee);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(settlementAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddSettledFeeToSettlement(SettlementCommands.AddSettledFeeToSettlementCommand command,
                                                            CancellationToken cancellationToken) {

            try {
                Guid settlementId = IdGenerationService.GenerateSettlementId(command.EstateId, command.MerchantId, command.SettledDate);

                Result<SettlementAggregate> getSettlementResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<SettlementAggregate>(settlementId, ct), settlementId, cancellationToken, false);
                if (getSettlementResult.IsFailed)
                    return ResultHelpers.CreateFailure(getSettlementResult);

                SettlementAggregate settlementAggregate = getSettlementResult.GetValueOrThrow();


                Result<MerchantAggregate> getMerchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (getMerchantResult.IsFailed)
                    return ResultHelpers.CreateFailure(getMerchantResult);

                MerchantAggregate merchant = getMerchantResult.GetValueOrThrow();
                Result stateResult;
                if (merchant.SettlementSchedule == SettlementSchedule.Immediate) {
                    stateResult = settlementAggregate.ImmediatelyMarkFeeAsSettled(command.MerchantId, command.TransactionId, command.FeeId);
                }
                else {
                    stateResult = settlementAggregate.MarkFeeAsSettled(command.MerchantId, command.TransactionId, command.FeeId, command.SettledDate.Date);
                }

                if (stateResult.IsFailed)
                    return stateResult;


                Result saveResult = await this.AggregateService.Save(settlementAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public SettlementDomainService(Func<IAggregateService> aggregateService)
        {
            this.AggregateService = aggregateService();
        }
    }
}
