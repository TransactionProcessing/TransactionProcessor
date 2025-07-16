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
        
        private async Task<Result> ApplySettlementUpdates(Func<SettlementAggregate, Task<Result>> action,
                                                     Guid settlementId,
                                                     CancellationToken cancellationToken,
                                                     Boolean isNotFoundError = true)
        {
            try
            {

                Result<SettlementAggregate> getSettlementResult = await this.AggregateService.GetLatest<SettlementAggregate>(settlementId, cancellationToken);
                Result<SettlementAggregate> settlementAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getSettlementResult, settlementId, isNotFoundError);

                if (settlementAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(settlementAggregateResult);

                Logger.LogInformation("In ApplySettlementUpdates - got aggregate");
                SettlementAggregate settlementAggregate = settlementAggregateResult.Data;
                Result result = await action(settlementAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);
                Logger.LogInformation("In ApplySettlementUpdates - action successful");
                Result saveResult = await this.AggregateService.Save(settlementAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);
                Logger.LogInformation("In ApplySettlementUpdates - save successful");
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"In ApplySettlementUpdates failed - {ex.Message}");
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        private async Task<Result> ApplyTransactionUpdates(Func<TransactionAggregate, Task<Result>> action,
                                                          Guid transactionId,
                                                          CancellationToken cancellationToken,
                                                          Boolean isNotFoundError = true)
        {
            try
            {

                Result<TransactionAggregate> getTransactionResult = await this.AggregateService.GetLatest<TransactionAggregate>(transactionId, cancellationToken);
                Result<TransactionAggregate> transactionAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getTransactionResult, transactionId, isNotFoundError);

                TransactionAggregate transactionAggregate = transactionAggregateResult.Data;
                Result result = await action(transactionAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.AggregateService.Save(transactionAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        // TODO: Add in a Get Settlement 

        public async Task<Result<Guid>> ProcessSettlement(SettlementCommands.ProcessSettlementCommand command,
                                                          CancellationToken cancellationToken) {

            IAsyncPolicy<Result<Guid>> retryPolicy = PolicyFactory.CreatePolicy<Guid>(policyTag: "SettlementDomainService - ProcessSettlement");

            return await PolicyFactory.ExecuteWithPolicyAsync<Guid>(async () => {

                Guid settlementAggregateId = Helpers.CalculateSettlementAggregateId(command.SettlementDate, command.MerchantId, command.EstateId);
                List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> feesToBeSettled = new();

                Result settlementResult = await ApplySettlementUpdates(async (SettlementAggregate settlementAggregate) => {
                    if (settlementAggregate.IsCreated == false) {
                        Logger.LogInformation($"No pending settlement for {command.SettlementDate:yyyy-MM-dd}");
                        // Not pending settlement for this date
                        return Result.Success();
                    }

                    Result<MerchantAggregate> getMerchantResult = await this.AggregateService.Get<MerchantAggregate>(command.MerchantId, cancellationToken);
                    if (getMerchantResult.IsFailed)
                        return ResultHelpers.CreateFailure(getMerchantResult);

                    MerchantAggregate merchant = getMerchantResult.Data;
                    if (merchant.SettlementSchedule == SettlementSchedule.Immediate) {
                        // Mark the settlement as completed
                        settlementAggregate.StartProcessing(DateTime.Now);
                        settlementAggregate.ManuallyComplete();
                        Result result = await this.AggregateService.Save(settlementAggregate, cancellationToken);
                        return result;
                    }

                    feesToBeSettled = settlementAggregate.GetFeesToBeSettled();

                    if (feesToBeSettled.Any()) {
                        // Record the process call
                        settlementAggregate.StartProcessing(DateTime.Now);
                        return await this.AggregateService.Save(settlementAggregate, cancellationToken);
                    }

                    return Result.Success();

                }, settlementAggregateId, cancellationToken);

                if (settlementResult.IsFailed)
                    return settlementResult;

                List<Result> failedResults = new();
                foreach ((Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) feeToSettle in feesToBeSettled) {
                    Result transactionResult = await ApplyTransactionUpdates(async (TransactionAggregate transactionAggregate) => {
                        try {
                            transactionAggregate.AddSettledFee(feeToSettle.calculatedFee, command.SettlementDate, settlementAggregateId);
                            return Result.Success();
                        }
                        catch (Exception ex) {
                            Logger.LogError(ex);
                            return Result.Failure();
                        }
                    }, feeToSettle.transactionId, cancellationToken);

                    if (transactionResult.IsFailed) {
                        failedResults.Add(transactionResult);
                    }
                }

                if (failedResults.Any()) {
                    return Result.Failure($"Not all fees were processed successfully {failedResults.Count} have failed");
                }

                return Result.Success(settlementAggregateId);
            }, retryPolicy, "SettlementDomainService - ProcessSettlement");

        }

        public async Task<Result> AddMerchantFeePendingSettlement(SettlementCommands.AddMerchantFeePendingSettlementCommand command,
                                                                  CancellationToken cancellationToken) {
            Logger.LogInformation("Processing command AddMerchantFeePendingSettlementCommand");

            Guid aggregateId = Helpers.CalculateSettlementAggregateId(command.SettlementDueDate.Date, command.MerchantId, command.EstateId);
            Result result = await ApplySettlementUpdates(async (SettlementAggregate settlementAggregate) => {

                Logger.LogInformation("In ApplySettlementUpdates");
                if (settlementAggregate.IsCreated == false)
                {
                    Logger.LogInformation("In ApplySettlementUpdates - aggregate not created");
                    settlementAggregate.Create(command.EstateId, command.MerchantId, command.SettlementDueDate.Date);
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
                settlementAggregate.AddFee(command.MerchantId, command.TransactionId, calculatedFee);

                return Result.Success();

            }, aggregateId, cancellationToken, false);
            return result;
        }

        public async Task<Result> AddSettledFeeToSettlement(SettlementCommands.AddSettledFeeToSettlementCommand command,
                                                            CancellationToken cancellationToken) {
            Guid aggregateId = Helpers.CalculateSettlementAggregateId(command.SettledDate.Date, command.MerchantId, command.EstateId);
            Result result = await ApplySettlementUpdates(async (SettlementAggregate settlementAggregate) => {

                Result<MerchantAggregate> getMerchantResult = await this.AggregateService.Get<MerchantAggregate>(command.MerchantId, cancellationToken);
                if (getMerchantResult.IsFailed)
                    return ResultHelpers.CreateFailure(getMerchantResult);

                MerchantAggregate merchant = getMerchantResult.Data;

                if (merchant.SettlementSchedule == SettlementSchedule.Immediate){
                    settlementAggregate.ImmediatelyMarkFeeAsSettled(command.MerchantId, command.TransactionId, command.FeeId);
                }
                else {
                    settlementAggregate.MarkFeeAsSettled(command.MerchantId, command.TransactionId, command.FeeId, command.SettledDate.Date);
                }

                return Result.Success();

            }, aggregateId, cancellationToken);
            return result;
        }

        public SettlementDomainService(IAggregateService aggregateService)
        {
            this.AggregateService = aggregateService;
        }
    }
}