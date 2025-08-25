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

        private async Task<Result> ValidateContractProduct(Guid estateId, Guid contractId, Guid productId, CancellationToken cancellationToken)
        {
            Result<ContractAggregate> getContractResult = await DomainServiceHelper.GetAggregateOrFailure(
                (token) => this.AggregateService.Get<ContractAggregate>(contractId, token),
                contractId,
                cancellationToken,
                isNotFoundError: true);

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

            try {
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

                Result<FloatAggregate> getFloatResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatAggregate>(floatId, ct), floatId, cancellationToken, false);
                if (getFloatResult.IsFailed)
                    return ResultHelpers.CreateFailure(getFloatResult);

                FloatAggregate floatAggregate = getFloatResult.Data;

                floatAggregate.CreateFloat(command.EstateId, command.ContractId, command.ProductId, command.CreateDateTime);

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

                floatAggregate.RecordCreditPurchase(command.PurchaseDateTime, command.CreditAmount, command.CostPrice);

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

                Result<FloatActivityAggregate> getFloatActivityResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatActivityAggregate>(floatActivityAggregateId, ct), floatActivityAggregateId, cancellationToken);
                if (getFloatActivityResult.IsFailed)
                    return ResultHelpers.CreateFailure(getFloatActivityResult);

                FloatActivityAggregate floatActivityAggregate = getFloatActivityResult.Data;

                floatActivityAggregate.RecordCreditPurchase(command.EstateId, command.CreditPurchasedDateTime, command.Amount, command.CreditId);

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


                Guid floatId = IdGenerationService.GenerateFloatAggregateId(command.EstateId, getTransactionResult.Data.ContractId, getTransactionResult.Data.ProductId);

                // Generate the id for the activity aggregate
                Guid floatActivityAggregateId = IdGenerationService.GenerateFloatActivityAggregateId(command.EstateId, floatId, getTransactionResult.Data.TransactionDateTime.Date);

                Result<FloatActivityAggregate> getFloatActivityResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<FloatActivityAggregate>(floatActivityAggregateId, ct), floatActivityAggregateId, cancellationToken);
                if (getFloatActivityResult.IsFailed)
                    return ResultHelpers.CreateFailure(getFloatActivityResult);

                FloatActivityAggregate floatActivityAggregate = getFloatActivityResult.Data;

                floatActivityAggregate.RecordTransactionAgainstFloat(command.EstateId, getTransactionResult.Data.TransactionDateTime, getTransactionResult.Data.TransactionAmount.GetValueOrDefault(), command.TransactionId);

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
