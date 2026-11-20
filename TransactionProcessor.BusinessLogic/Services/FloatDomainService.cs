using System;
using System.Linq;
using System.Threading.Tasks;
using TransactionProcessing.UnionResults;
using TransactionProcessing.UnionResults;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Services
{
    using System.Threading;
    using TransactionProcessing.Shared.EventStore.Aggregate;
    using TransactionProcessing.Shared.Exceptions;
    using TransactionProcessor.Aggregates;

    public interface IFloatDomainService {
        Task<Result<Guid>> CreateFloat(FloatCommands.CreateFloatCommand command,
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

            if (getEstateResult.IsSuccess() == false) {
                return ResultHelpers.CreateFailure(getEstateResult);
            }
            return Results.Ok();
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
                return new Error(ErrorType.Validation, $"Operator with Id {operatorId} is not supported on Estate [{estate.Name}]");
            }

            return Results.Ok();
        }

        public async Task<Result<Guid>> CreateFloat(FloatCommands.CreateFloatCommand command,
                                                                CancellationToken cancellationToken){

            try {
                Result<EstateAggregate> getEstateResult = await this.GetEstateAggregate(command.EstateId, cancellationToken);
                if (getEstateResult.IsSuccess() == false) {
                    return ResultHelpers.CreateFailure(getEstateResult);
                }

                Result validateOperatorResult = ValidateOperator(getEstateResult.Data, command.OperatorId);
                if (validateOperatorResult.IsSuccess() == false) {
                    return ResultHelpers.CreateFailure(validateOperatorResult);
                }
                
                Result<FloatAggregate> getFloatResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatAggregate>(command.OperatorId, ct), command.OperatorId, cancellationToken, false);
                if (getFloatResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getFloatResult);

                FloatAggregate floatAggregate = getFloatResult.Data;

                Result stateResult = floatAggregate.CreateFloat(command.EstateId, command.CreateDateTime);
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(floatAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);
                return Results.Ok(command.OperatorId);

            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> RecordCreditPurchase(FloatCommands.RecordCreditPurchaseForFloatCommand command, CancellationToken cancellationToken){

            try
            {
                Result<FloatAggregate> getFloatResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatAggregate>(command.RecordFloatCreditPurchaseRequest.FloatId, ct), command.RecordFloatCreditPurchaseRequest.FloatId, cancellationToken);
                if (getFloatResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getFloatResult);

                FloatAggregate floatAggregate = getFloatResult.Data;

                Result stateResult = floatAggregate.RecordCreditPurchase(command.RecordFloatCreditPurchaseRequest.PurchaseDateTime, command.RecordFloatCreditPurchaseRequest.CreditAmount, command.RecordFloatCreditPurchaseRequest.CostPrice,
                    command.RecordFloatCreditPurchaseRequest.HostDateTime);
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(floatAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);
                return saveResult;
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> RecordCreditPurchase(FloatActivityCommands.RecordCreditPurchaseCommand command,
                                                       CancellationToken cancellationToken) {

            try
            {
                Guid floatActivityAggregateId = IdGenerationService.GenerateFloatActivityAggregateId(command.EstateId, command.FloatId, command.CreditPurchasedDateTime.Date);

                Result<FloatActivityAggregate> getFloatActivityResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatActivityAggregate>(floatActivityAggregateId, ct), floatActivityAggregateId, cancellationToken, false);
                if (getFloatActivityResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getFloatActivityResult);

                FloatActivityAggregate floatActivityAggregate = getFloatActivityResult.Data;

                Result stateResult = floatActivityAggregate.RecordCreditPurchase(command.EstateId, command.CreditPurchasedDateTime, command.Amount, command.CreditId);
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);
                Result saveResult = await this.AggregateService.Save(floatActivityAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);
                return saveResult;
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> RecordTransaction(FloatActivityCommands.RecordTransactionCommand command,
                                                    CancellationToken cancellationToken) {

            try {
                Result<TransactionAggregate> getTransactionResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, ct), command.TransactionId, cancellationToken);
                if (getTransactionResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getTransactionResult);
                
                Guid floatId = getTransactionResult.Data.OperatorId;

                // Generate the id for the activity aggregate
                Guid floatActivityAggregateId = IdGenerationService.GenerateFloatActivityAggregateId(command.EstateId, floatId, getTransactionResult.Data.TransactionDateTime.Date);

                Result<FloatActivityAggregate> getFloatActivityResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatActivityAggregate>(floatActivityAggregateId, ct), floatActivityAggregateId, cancellationToken, false);
                if (getFloatActivityResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getFloatActivityResult);

                FloatActivityAggregate floatActivityAggregate = getFloatActivityResult.Data;

                Result stateResult = floatActivityAggregate.RecordTransactionAgainstFloat(command.EstateId, getTransactionResult.Data.TransactionDateTime, getTransactionResult.Data.TransactionAmount.GetValueOrDefault(), command.TransactionId);
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(floatActivityAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);
                return saveResult;
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }
    }
}
