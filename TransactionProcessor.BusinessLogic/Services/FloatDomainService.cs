using System;
using System.Linq;
using System.Threading.Tasks;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Services
{
    using System.Threading;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using TransactionProcessor.Aggregates;

    public interface IFloatDomainService {
        Task<Result> CreateFloat(FloatCommands.CreateFloatCommand command,
                                                   CancellationToken cancellationToken);

        Task<Result> RecordCreditPurchase(FloatCommands.RecordCreditPurchaseForFloatCommand command,
                                          CancellationToken cancellationToken);

        Task<Result> RecordCreditPurchase(FloatActivityCommands.RecordCreditPurchaseCommand command,
                                          CancellationToken cancellationToken);
        Task<Result> RecordTransaction(FloatActivityCommands.RecordTransactionCommand command,
                                          CancellationToken cancellationToken);
    }

    public class FloatDomainService : IFloatDomainService{
        private readonly IAggregateService AggregateService;

        public FloatDomainService(Func<IAggregateService> aggregateService)
        {
            this.AggregateService = aggregateService();
        }
        
        private async Task<Result> ValidateEstate(Guid estateId, CancellationToken cancellationToken)
        {
            Result<EstateAggregate> getEstateResult= await DomainServiceHelper.GetAggregateOrFailure(
                (token) => this.AggregateService.Get<EstateAggregate>(estateId, token),
                estateId,
                cancellationToken,
                isNotFoundError: true);

            if (getEstateResult.IsFailed) {
                return ResultHelpers.CreateFailure(getEstateResult);
            }
            return Result.Success();
        }

        private async Task<Result<EstateAggregate>> GetEstateAggregate(Guid estateId, CancellationToken cancellationToken)
        {
            return await DomainServiceHelper.GetAggregateOrFailure(
                token => this.AggregateService.Get<EstateAggregate>(estateId, token),
                estateId,
                cancellationToken,
                isNotFoundError: true);
        }

        private static Result ValidateOperator(EstateAggregate estateAggregate, Guid operatorId)
        {
            TransactionProcessor.Models.Estate.Estate estate = estateAggregate.GetEstate();
            Boolean operatorExists = estate.Operators.Any(o => o.OperatorId == operatorId && o.IsDeleted == false);
            if (operatorExists == false) {
                return Result.Invalid($"Operator with Id {operatorId} is not supported on Estate [{estate.Name}]");
            }

            return Result.Success();
        }

        public async Task<Result> CreateFloat(FloatCommands.CreateFloatCommand command,
                                                                CancellationToken cancellationToken){

            try {
                Result<EstateAggregate> getEstateResult = await this.GetEstateAggregate(command.EstateId, cancellationToken);
                if (getEstateResult.IsFailed) {
                    return ResultHelpers.CreateFailure(getEstateResult);
                }

                Result validateOperatorResult = ValidateOperator(getEstateResult.Data, command.FloatId);
                if (validateOperatorResult.IsFailed) {
                    return ResultHelpers.CreateFailure(validateOperatorResult);
                }
                
                Result<FloatAggregate> getFloatResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatAggregate>(command.FloatId, ct), command.FloatId, cancellationToken, false);
                if (getFloatResult.IsFailed)
                    return ResultHelpers.CreateFailure(getFloatResult);

                FloatAggregate floatAggregate = getFloatResult.Data;

                Result stateResult = floatAggregate.CreateFloat(command.EstateId, command.CreateDateTime);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(floatAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);
                return saveResult;
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> RecordCreditPurchase(FloatCommands.RecordCreditPurchaseForFloatCommand command, CancellationToken cancellationToken){

            try
            {
                Result<FloatAggregate> getFloatResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatAggregate>(command.FloatId, ct), command.FloatId, cancellationToken);
                if (getFloatResult.IsFailed)
                    return ResultHelpers.CreateFailure(getFloatResult);

                FloatAggregate floatAggregate = getFloatResult.Data;

                Result stateResult = floatAggregate.RecordCreditPurchase(command.PurchaseDateTime, command.CreditAmount, command.CostPrice);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(floatAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);
                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> RecordCreditPurchase(FloatActivityCommands.RecordCreditPurchaseCommand command,
                                                       CancellationToken cancellationToken) {

            try
            {
                Guid floatActivityAggregateId = IdGenerationService.GenerateFloatActivityAggregateId(command.EstateId, command.FloatId, command.CreditPurchasedDateTime.Date);

                Result<FloatActivityAggregate> getFloatActivityResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatActivityAggregate>(floatActivityAggregateId, ct), floatActivityAggregateId, cancellationToken, false);
                if (getFloatActivityResult.IsFailed)
                    return ResultHelpers.CreateFailure(getFloatActivityResult);

                FloatActivityAggregate floatActivityAggregate = getFloatActivityResult.Data;

                Result stateResult = floatActivityAggregate.RecordCreditPurchase(command.EstateId, command.CreditPurchasedDateTime, command.Amount, command.CreditId);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);
                Result saveResult = await this.AggregateService.Save(floatActivityAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);
                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> RecordTransaction(FloatActivityCommands.RecordTransactionCommand command,
                                                    CancellationToken cancellationToken) {

            try {
                Result<TransactionAggregate> getTransactionResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, ct), command.TransactionId, cancellationToken);
                if (getTransactionResult.IsFailed)
                    return ResultHelpers.CreateFailure(getTransactionResult);
                
                Guid floatId = getTransactionResult.Data.OperatorId;

                // Generate the id for the activity aggregate
                Guid floatActivityAggregateId = IdGenerationService.GenerateFloatActivityAggregateId(command.EstateId, floatId, getTransactionResult.Data.TransactionDateTime.Date);

                Result<FloatActivityAggregate> getFloatActivityResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatActivityAggregate>(floatActivityAggregateId, ct), floatActivityAggregateId, cancellationToken, false);
                if (getFloatActivityResult.IsFailed)
                    return ResultHelpers.CreateFailure(getFloatActivityResult);

                FloatActivityAggregate floatActivityAggregate = getFloatActivityResult.Data;

                Result stateResult = floatActivityAggregate.RecordTransactionAgainstFloat(command.EstateId, getTransactionResult.Data.TransactionDateTime, getTransactionResult.Data.TransactionAmount.GetValueOrDefault(), command.TransactionId);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(floatActivityAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);
                return saveResult;
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }
    }
}
