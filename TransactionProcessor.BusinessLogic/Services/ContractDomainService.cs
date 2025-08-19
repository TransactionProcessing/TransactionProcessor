using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventStore;
using Shared.Exceptions;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.BusinessLogic.Services
{
    public interface IContractDomainService
    {
        #region Methods

        Task<Result> AddProductToContract(ContractCommands.AddProductToContractCommand command, CancellationToken cancellationToken);

        Task<Result> AddTransactionFeeForProductToContract(ContractCommands.AddTransactionFeeForProductToContractCommand command, CancellationToken cancellationToken);

        Task<Result> DisableTransactionFeeForProduct(ContractCommands.DisableTransactionFeeForProductCommand command, CancellationToken cancellationToken);

        Task<Result> CreateContract(ContractCommands.CreateContractCommand command, CancellationToken cancellationToken);

        #endregion
    }
    public class ContractDomainService : IContractDomainService
    {

        #region Fields
        
        private readonly IAggregateService AggregateService;
        private readonly IEventStoreContext Context;

        #endregion

        #region Constructors

        public ContractDomainService(Func<IAggregateService> aggregateService, IEventStoreContext context) {
            this.AggregateService = aggregateService();
            this.Context = context;
        }

        #endregion

        #region Methods

        private async Task<Result> ApplyUpdates(Func<(EstateAggregate estateAggregate, ContractAggregate contractAggregate), Task<Result>> action, Guid estateId, Guid contractId, CancellationToken cancellationToken, Boolean isNotFoundError = true)
        {
            try
            {
                Result<EstateAggregate> getResult = await this.AggregateService.Get<EstateAggregate>(estateId, cancellationToken);
                if (getResult.IsFailed)
                    return ResultHelpers.CreateFailure(getResult);
                EstateAggregate estateAggregate = getResult.Data;
                if (estateAggregate.IsCreated == false)
                    return Result.Failure("Estate is not created");
                
                Result<ContractAggregate> getContractResult = await this.AggregateService.GetLatest<ContractAggregate>(contractId, cancellationToken);
                Result<ContractAggregate> contractAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getContractResult, contractId, isNotFoundError);
                if (contractAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(contractAggregateResult);

                ContractAggregate contractAggregate = contractAggregateResult.Data;
                Result result = await action((estateAggregate, contractAggregate));
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.AggregateService.Save(contractAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddProductToContract(ContractCommands.AddProductToContractCommand command, CancellationToken cancellationToken)
        {
            Models.Contract.ProductType productType = (Models.Contract.ProductType)command.RequestDTO.ProductType;

            Result result = await this.ApplyUpdates(async ((EstateAggregate estateAggregate, ContractAggregate contractAggregate) aggregates) => {
                if (aggregates.estateAggregate.IsCreated == false) {
                    return Result.Forbidden($"Estate with Id {command.EstateId} not created");
                }

                if (aggregates.contractAggregate.IsCreated == false) {
                    return Result.Forbidden($"Contract Id [{command.ContractId}] must be created to add products");
                }

                if (command.RequestDTO.Value.HasValue) {
                    aggregates.contractAggregate.AddFixedValueProduct(command.ProductId, command.RequestDTO.ProductName,
                        command.RequestDTO.DisplayText, command.RequestDTO.Value.Value, productType);
                }
                else {
                    aggregates.contractAggregate.AddVariableValueProduct(command.ProductId, command.RequestDTO.ProductName,
                        command.RequestDTO.DisplayText, productType);
                }

                return Result.Success();
            }, command.EstateId, command.ContractId, cancellationToken);
            return result;
        }

        public async Task<Result> AddTransactionFeeForProductToContract(ContractCommands.AddTransactionFeeForProductToContractCommand command, CancellationToken cancellationToken)
        {
            Models.Contract.CalculationType calculationType = (Models.Contract.CalculationType)command.RequestDTO.CalculationType;
            Models.Contract.FeeType feeType = (Models.Contract.FeeType)command.RequestDTO.FeeType;

            Result result = await this.ApplyUpdates(async ((EstateAggregate estateAggregate, ContractAggregate contractAggregate) aggregates) => {

                if (aggregates.contractAggregate.IsCreated == false)
                {
                    return Result.Forbidden($"Contract Id [{command.ContractId}] must be created to add transaction fees");
                }

                List<Product> products = aggregates.contractAggregate.GetProducts();
                Product product = products.SingleOrDefault(p => p.ContractProductId == command.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product Id [{command.ProductId}] not added to contract [{aggregates.contractAggregate.Description}]");
                }

                aggregates.contractAggregate.AddTransactionFee(product, command.TransactionFeeId, command.RequestDTO.Description, calculationType, feeType, command.RequestDTO.Value);

                return Result.Success();
            }, command.EstateId, command.ContractId, cancellationToken);
            return result;

        }

        public async Task<Result> DisableTransactionFeeForProduct(ContractCommands.DisableTransactionFeeForProductCommand command, CancellationToken cancellationToken)
        {
            Result result = await this.ApplyUpdates(async ((EstateAggregate estateAggregate, ContractAggregate contractAggregate) aggregates) => {

                aggregates.contractAggregate.DisableTransactionFee(command.ProductId, command.TransactionFeeId);
                return Result.Success();
            }, command.EstateId, command.ContractId, cancellationToken);
            return result;
        }

        public async Task<Result> CreateContract(ContractCommands.CreateContractCommand command, CancellationToken cancellationToken)
        {
            Result result = await this.ApplyUpdates(async ((EstateAggregate estateAggregate, ContractAggregate contractAggregate) aggregates) => {

                Models.Estate.Estate estate = aggregates.estateAggregate.GetEstate();
                if (estate.Operators.Any(o => o.OperatorId == command.RequestDTO.OperatorId) == false)
                {
                    return Result.NotFound($"Unable to create a contract for an operator that is not setup on estate [{estate.Name}]");
                }

                // Validate a duplicate name
                String projection =
                    $"fromCategory(\"ContractAggregate\")\n.when({{\n    $init: function (s, e) {{\n                        return {{\n                            total: 0,\n                            contractId: 0\n                        }};\n                    }},\n    'ContractCreatedEvent': function(s,e){{\n        // Check if it matches\n        if (e.data.description === '{command.RequestDTO.Description}' \n            && e.data.operatorId === '{command.RequestDTO.OperatorId}'){{\n            s.total += 1;\n            s.contractId = e.data.contractId\n        }}\n    }}\n}})";

                Result<String> result = await this.Context.RunTransientQuery(projection, cancellationToken);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);
                String resultString = result.Data;
                if (String.IsNullOrEmpty(resultString) == false)
                {
                    JObject jsonResult = JObject.Parse(resultString);

                    String contractIdString = jsonResult.Property("contractId").Values<String>().Single();

                    Guid.TryParse(contractIdString, out Guid contractIdResult);

                    if (contractIdResult != Guid.Empty) {
                        return Result.Conflict(
                            $"Contract Description {command.RequestDTO.Description} already in use for operator {command.RequestDTO.OperatorId}");
                    }
                }

                if (aggregates.contractAggregate.IsCreated)
                {
                    return Result.Forbidden($"Contract Id [{command.ContractId}] already created for estate [{estate.Name}]");
                }
                aggregates.contractAggregate.Create(command.EstateId, command.RequestDTO.OperatorId, command.RequestDTO.Description);

                return Result.Success();
            }, command.EstateId, command.ContractId, cancellationToken, false);
            return result;
        }

        #endregion
    }
}