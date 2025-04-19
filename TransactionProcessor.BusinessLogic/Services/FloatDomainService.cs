using System;
using System.Linq;
using System.Threading.Tasks;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Services
{
    using System.Threading;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using TransactionProcessor.Aggregates;

    public interface IFloatDomainService {
        Task<Result> CreateFloatForContractProduct(FloatCommands.CreateFloatForContractProductCommand command,
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

        public FloatDomainService(IAggregateService aggregateService)
        {
            this.AggregateService = aggregateService;
        }

        private async Task<Result> ApplyFloatUpdates(Func<FloatAggregate, Result> action, Guid floatId, CancellationToken cancellationToken, Boolean isNotFoundError = true)
        {
            try
            {
                Result<FloatAggregate> getFloatResult = await this.AggregateService.GetLatest<FloatAggregate>(floatId, cancellationToken);
                Result<FloatAggregate> floatAggregateResult = DomainServiceHelper.HandleGetAggregateResult(getFloatResult, floatId, isNotFoundError);

                if (floatAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(floatAggregateResult);

                FloatAggregate floatAggregate = floatAggregateResult.Data;
                
                Result result = action(floatAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.AggregateService.Save(floatAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        private async Task<Result> ApplyFloatActivityUpdates(Func<FloatActivityAggregate, Result> action, Guid floatId, CancellationToken cancellationToken, Boolean isNotFoundError = true)
        {
            try
            {
                Result<FloatActivityAggregate> getFloatResult = await this.AggregateService.GetLatest<FloatActivityAggregate>(floatId, cancellationToken);
                Result<FloatActivityAggregate> floatActivityAggregateResult = DomainServiceHelper.HandleGetAggregateResult(getFloatResult, floatId, isNotFoundError);

                if (floatActivityAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(floatActivityAggregateResult);

                FloatActivityAggregate floatActivityAggregate = floatActivityAggregateResult.Data;

                Result result = action(floatActivityAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.AggregateService.Save(floatActivityAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }


        private async Task<Result> ValidateEstate(Guid estateId, CancellationToken cancellationToken)
        {
            Result<EstateAggregate> getEstateResult= await this.AggregateService.Get<EstateAggregate>(estateId, cancellationToken);

            if (getEstateResult.IsFailed) {
                return ResultHelpers.CreateFailure(getEstateResult);
            }
            return Result.Success();
        }

        private async Task<Result> ValidateContractProduct(Guid estateId, Guid contractId, Guid productId, CancellationToken cancellationToken)
        {
            Result<ContractAggregate> getContractResult = await this.AggregateService.Get<ContractAggregate>(contractId, cancellationToken);
            if (getContractResult.IsFailed)
            {
                return ResultHelpers.CreateFailure(getContractResult);
            }
            ContractAggregate contractAggregate = getContractResult.Data;
            Models.Contract.Contract contract = contractAggregate.GetContract();
            Boolean productExists = contract.Products.Any(cp => cp.ContractProductId == productId);

            return productExists switch {
                false => Result.NotFound($"Contract Product with Id {productId} not found in Contract Id {contractId} for Estate Id {estateId}"),
                _ => Result.Success()
            };
        }

        public async Task<Result> CreateFloatForContractProduct(FloatCommands.CreateFloatForContractProductCommand command,
                                                                CancellationToken cancellationToken){
            
            Result validateEstateResult = await this.ValidateEstate(command.EstateId, cancellationToken);
            if (validateEstateResult.IsFailed) {
                return ResultHelpers.CreateFailure(validateEstateResult);
            }

            Result validateProductResult = await this.ValidateContractProduct(command.EstateId, command.ContractId, command.ProductId, cancellationToken);
            if (validateProductResult.IsFailed) {
                return ResultHelpers.CreateFailure(validateProductResult);
            }

            // Generate the float id
            Guid floatId = IdGenerationService.GenerateFloatAggregateId(command.EstateId, command.ContractId, command.ProductId);

            Result result = await this.ApplyFloatUpdates((FloatAggregate floatAggregate) => {
                floatAggregate.CreateFloat(command.EstateId, command.ContractId, command.ProductId, command.CreateDateTime);
                return Result.Success();
            }, floatId,cancellationToken, false);

            return result;
        }

        public async Task<Result> RecordCreditPurchase(FloatCommands.RecordCreditPurchaseForFloatCommand command, CancellationToken cancellationToken){
            Result result = await this.ApplyFloatUpdates((FloatAggregate floatAggregate) => {
                floatAggregate.RecordCreditPurchase(command.PurchaseDateTime, command.CreditAmount, command.CostPrice);
                return Result.Success();
            }, command.FloatId, cancellationToken);

            return result;
        }

        public async Task<Result> RecordCreditPurchase(FloatActivityCommands.RecordCreditPurchaseCommand command,
                                                       CancellationToken cancellationToken) {

            // Generate the id for the activity aggregate
            Guid floatActivityAggregateId = IdGenerationService.GenerateFloatActivityAggregateId(command.EstateId, command.FloatId, command.CreditPurchasedDateTime.Date);

            Result result = await ApplyFloatActivityUpdates((floatAggregate) => {
                floatAggregate.RecordCreditPurchase(command.EstateId, command.CreditPurchasedDateTime, command.Amount, command.CreditId);
                return Result.Success();
            }, floatActivityAggregateId, cancellationToken,false);
            return result;
        }

        public async Task<Result> RecordTransaction(FloatActivityCommands.RecordTransactionCommand command,
                                                    CancellationToken cancellationToken) {
            
            Result<TransactionAggregate> getTransactionResult = await this.AggregateService.GetLatest<TransactionAggregate>(command.TransactionId, cancellationToken);
            if (getTransactionResult.IsFailed)
                return ResultHelpers.CreateFailure(getTransactionResult);

            Guid floatId = IdGenerationService.GenerateFloatAggregateId(command.EstateId, getTransactionResult.Data.ContractId, getTransactionResult.Data.ProductId);

            // Generate the id for the activity aggregate
            Guid floatActivityAggregateId = IdGenerationService.GenerateFloatActivityAggregateId(command.EstateId, floatId, getTransactionResult.Data.TransactionDateTime.Date);

            Result result = await ApplyFloatActivityUpdates((floatAggregate) => {
                floatAggregate.RecordTransactionAgainstFloat(command.EstateId, getTransactionResult.Data.TransactionDateTime, getTransactionResult.Data.TransactionAmount.GetValueOrDefault(), command.TransactionId);
                return Result.Success();
            }, floatActivityAggregateId, cancellationToken, false);
            return result;
        }
    }

    public static class DomainServiceHelper
    {
        public static Result<T> HandleGetAggregateResult<T>(Result<T> result, Guid aggregateId, bool isNotFoundError = true)
            where T : Aggregate, new()  // Constraint: T is a subclass of Aggregate and has a parameterless constructor
        {
            if (result.IsFailed && result.Status != ResultStatus.NotFound)
            {
                return ResultHelpers.CreateFailure(result);
            }

            if (result.Status == ResultStatus.NotFound && isNotFoundError)
            {
                return ResultHelpers.CreateFailure(result);
            }

            T aggregate = result.Status switch
            {
                ResultStatus.NotFound => new T { AggregateId = aggregateId },  // Set AggregateId when creating a new instance
                _ => result.Data
            };

            return Result.Success(aggregate);
        }
    }
}
