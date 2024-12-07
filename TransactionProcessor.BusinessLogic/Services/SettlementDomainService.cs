using Shared.Results;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects;
    using EstateManagement.DataTransferObjects.Responses;
    using Models;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using Shared.General;
    using Shared.Logger;
    using TransactionAggregate;

    public class SettlementDomainService : ISettlementDomainService
    {
        private readonly IAggregateRepository<TransactionAggregate, DomainEvent> TransactionAggregateRepository;
        private readonly IAggregateRepository<SettlementAggregate, DomainEvent> SettlementAggregateRepository;

        private readonly ISecurityServiceClient SecurityServiceClient;

        private readonly IEstateClient EstateClient;

        private async Task<Result> ApplySettlementUpdates(Func<SettlementAggregate, Task<Result>> action,
                                                     Guid settlementId,
                                                     CancellationToken cancellationToken,
                                                     Boolean isNotFoundError = true)
        {
            try
            {

                Result<SettlementAggregate> getSettlementResult = await this.SettlementAggregateRepository.GetLatestVersion(settlementId, cancellationToken);
                Result<SettlementAggregate> settlementAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getSettlementResult, settlementId, isNotFoundError);
                Logger.LogInformation("In ApplySettlementUpdates - got aggregate");
                SettlementAggregate settlementAggregate = settlementAggregateResult.Data;
                Result result = await action(settlementAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);
                Logger.LogInformation("In ApplySettlementUpdates - action successful");
                Result saveResult = await this.SettlementAggregateRepository.SaveChanges(settlementAggregate, cancellationToken);
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

                Result<TransactionAggregate> getTransactionResult = await this.TransactionAggregateRepository.GetLatestVersion(transactionId, cancellationToken);
                Result<TransactionAggregate> transactionAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getTransactionResult, transactionId, isNotFoundError);

                TransactionAggregate transactionAggregate = transactionAggregateResult.Data;
                Result result = await action(transactionAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
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
                                                              CancellationToken cancellationToken)
        {
            Guid settlementAggregateId = Helpers.CalculateSettlementAggregateId(command.SettlementDate, command.MerchantId,command.EstateId);
            List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> feesToBeSettled = new();

            Result settlementResult = await ApplySettlementUpdates(async (SettlementAggregate settlementAggregate) => {
                if (settlementAggregate.IsCreated == false)
                {
                    Logger.LogInformation($"No pending settlement for {command.SettlementDate:yyyy-MM-dd}");
                    // Not pending settlement for this date
                    return Result.Success();
                }

                this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

                EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken,
                    command.EstateId,
                    command.MerchantId,
                    cancellationToken);

                if (merchant.SettlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate)
                {
                    // Mark the settlement as completed
                    settlementAggregate.StartProcessing(DateTime.Now);
                    settlementAggregate.ManuallyComplete();
                    Result result = await this.SettlementAggregateRepository.SaveChanges(settlementAggregate, cancellationToken);
                    return result;
                }

                feesToBeSettled = settlementAggregate.GetFeesToBeSettled();
                
                if (feesToBeSettled.Any())
                {
                    // Record the process call
                    settlementAggregate.StartProcessing(DateTime.Now);
                    return await this.SettlementAggregateRepository.SaveChanges(settlementAggregate, cancellationToken);
                }

                return Result.Success();

            }, settlementAggregateId, cancellationToken);

            if (settlementResult.IsFailed)
                return settlementResult;

            List<Result> failedResults = new();
            foreach ((Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) feeToSettle in feesToBeSettled) {
                Result transactionResult = await ApplyTransactionUpdates(
                    async (TransactionAggregate transactionAggregate) => {
                        try {
                            transactionAggregate.AddSettledFee(feeToSettle.calculatedFee, command.SettlementDate,
                                settlementAggregateId);
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
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            Guid aggregateId = Helpers.CalculateSettlementAggregateId(command.SettledDate.Date, command.MerchantId, command.EstateId);
            Result result = await ApplySettlementUpdates(async (SettlementAggregate settlementAggregate) => {
                
                var getMerchantResult = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, command.EstateId, command.MerchantId, cancellationToken);
                if (getMerchantResult.IsFailed) {
                    return ResultHelpers.CreateFailure(getMerchantResult);
                }

                var merchant = getMerchantResult.Data;
                if (merchant.SettlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate){
                    settlementAggregate.ImmediatelyMarkFeeAsSettled(command.MerchantId, command.TransactionId, command.FeeId);
                }
                else {
                    settlementAggregate.MarkFeeAsSettled(command.MerchantId, command.TransactionId, command.FeeId, command.SettledDate.Date);
                }

                return Result.Success();

            }, aggregateId, cancellationToken);
            return result;
        }

        public SettlementDomainService(IAggregateRepository<TransactionAggregate, DomainEvent> transactionAggregateRepository,
                                       IAggregateRepository<SettlementAggregate, DomainEvent> settlementAggregateRepository,
                                       ISecurityServiceClient securityServiceClient,
                                       IEstateClient estateClient)
        {
            this.TransactionAggregateRepository = transactionAggregateRepository;
            this.SettlementAggregateRepository = settlementAggregateRepository;
            this.SecurityServiceClient = securityServiceClient;
            this.EstateClient = estateClient;
        }

        private TokenResponse TokenResponse;
    }
}