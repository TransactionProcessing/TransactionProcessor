using Shared.Results;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.BusinessLogic.Services
{
    using Common;
    using Models;
    using Polly;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using Shared.Logger;
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

            IAsyncPolicy<Result<Guid>> retryPolicy = PolicyFactory.CreatePolicy<Guid>(policyTag: "SettlementDomainService - ProcessSettlement");

            try {
                return await PolicyFactory.ExecuteWithPolicyAsync<Guid>(async () => {

                    Guid settlementAggregateId = Helpers.CalculateSettlementAggregateId(command.SettlementDate, command.MerchantId, command.EstateId);

                    Result<SettlementAggregate> getSettlementResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<SettlementAggregate>(settlementAggregateId, ct), settlementAggregateId, cancellationToken, false);
                    if (getSettlementResult.IsFailed)
                        return ResultHelpers.CreateFailure(getSettlementResult);

                    SettlementAggregate settlementAggregate = getSettlementResult.Data;

                    if (settlementAggregate.IsCreated == false) {
                        Logger.LogInformation($"No pending settlement for {command.SettlementDate:yyyy-MM-dd}");
                        // Not pending settlement for this date
                        return Result.Success();
                    }

                    Result<MerchantAggregate> getMerchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken, false);
                    if (getMerchantResult.IsFailed)
                        return ResultHelpers.CreateFailure(getMerchantResult);

                    Result settlementSaveResult = Result.Success();
                    MerchantAggregate merchant = getMerchantResult.Data;
                    if (merchant.SettlementSchedule == SettlementSchedule.Immediate) {
                        // Mark the settlement as completed
                        settlementAggregate.StartProcessing(DateTime.Now);
                        settlementAggregate.ManuallyComplete();
                        settlementSaveResult = await this.AggregateService.Save(settlementAggregate, cancellationToken);
                    }

                    List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> feesToBeSettled = settlementAggregate.GetFeesToBeSettled();

                    if (feesToBeSettled.Any()) {
                        // Record the process call
                        Result stateResult = settlementAggregate.StartProcessing(DateTime.Now);
                        if (stateResult.IsFailed)
                            return ResultHelpers.CreateFailure(stateResult);
                        settlementSaveResult = await this.AggregateService.Save(settlementAggregate, cancellationToken);
                    }

                    // Settlement updates done, now we need to update the transactions
                    if (settlementSaveResult.IsFailed)
                        return ResultHelpers.CreateFailure(settlementSaveResult);

                    List<Result> failedResults = new();
                    foreach ((Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) feeToSettle in feesToBeSettled) {
                        try {

                            Result<TransactionAggregate> getTransactionResult = await this.AggregateService.GetLatest<TransactionAggregate>(feeToSettle.transactionId, cancellationToken);
                            if (getTransactionResult.IsFailed)
                                return ResultHelpers.CreateFailure(getTransactionResult);
                            var transactionAggregate = getTransactionResult.Data;

                            transactionAggregate.AddSettledFee(feeToSettle.calculatedFee, command.SettlementDate, settlementAggregateId);

                            Result saveResult = await this.AggregateService.Save(transactionAggregate, cancellationToken);
                            if (saveResult.IsFailed)
                                return ResultHelpers.CreateFailure(saveResult);
                        }
                        catch (Exception ex) {
                            Logger.LogError($"Failed to process transaction {feeToSettle.transactionId} for settlement {settlementAggregateId}", ex);
                            failedResults.Add(Result.Failure($"Failed to process transaction {feeToSettle.transactionId} for settlement {settlementAggregateId}: {ex.Message}"));
                        }
                    }

                    if (failedResults.Any()) {
                        return Result.Failure($"Not all fees were processed successfully {failedResults.Count} have failed");
                    }

                    return Result.Success(settlementAggregateId);
                }, retryPolicy, "SettlementDomainService - ProcessSettlement");
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddMerchantFeePendingSettlement(SettlementCommands.AddMerchantFeePendingSettlementCommand command,
                                                                  CancellationToken cancellationToken) {
            
            try{
            Logger.LogInformation("Processing command AddMerchantFeePendingSettlementCommand");
            
            Guid settlementAggregateId = Helpers.CalculateSettlementAggregateId(command.SettlementDueDate.Date, command.MerchantId, command.EstateId);

            Result<SettlementAggregate> getSettlementResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<SettlementAggregate>(settlementAggregateId, ct), settlementAggregateId, cancellationToken, false);
            if (getSettlementResult.IsFailed)
                return ResultHelpers.CreateFailure(getSettlementResult);

            SettlementAggregate settlementAggregate = getSettlementResult.Data;

            Logger.LogInformation("In ApplySettlementUpdates");
                if (settlementAggregate.IsCreated == false)
                {
                    Logger.LogInformation("In ApplySettlementUpdates - aggregate not created");
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
                Logger.LogInformation("In ApplySettlementUpdates - about to add fee");
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
                Guid settlementAggregateId = Helpers.CalculateSettlementAggregateId(command.SettledDate.Date, command.MerchantId, command.EstateId);

                Result<SettlementAggregate> getSettlementResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<SettlementAggregate>(settlementAggregateId, ct), settlementAggregateId, cancellationToken, false);
                if (getSettlementResult.IsFailed)
                    return ResultHelpers.CreateFailure(getSettlementResult);

                SettlementAggregate settlementAggregate = getSettlementResult.Data;


                Result<MerchantAggregate> getMerchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (getMerchantResult.IsFailed)
                    return ResultHelpers.CreateFailure(getMerchantResult);

                MerchantAggregate merchant = getMerchantResult.Data;
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