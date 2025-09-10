using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
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
        
        public async Task<Result> AddProductToContract(ContractCommands.AddProductToContractCommand command, CancellationToken cancellationToken)
        {
            Models.Contract.ProductType productType = (Models.Contract.ProductType)command.RequestDTO.ProductType;

            try
            {
                Result<ContractAggregate> contractResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<ContractAggregate>(command.ContractId, ct), command.ContractId, cancellationToken);
                if (contractResult.IsFailed)
                    return ResultHelpers.CreateFailure(contractResult);

                ContractAggregate contractAggregate = contractResult.Data;
                
                if (contractAggregate.IsCreated == false) {
                    return Result.Forbidden($"Contract Id [{command.ContractId}] must be created to add products");
                }

                Result stateResult = command.RequestDTO.Value.HasValue switch {
                    true => contractAggregate.AddFixedValueProduct(command.ProductId, command.RequestDTO.ProductName, command.RequestDTO.DisplayText, command.RequestDTO.Value.Value, productType),
                    _ => contractAggregate.AddVariableValueProduct(command.ProductId, command.RequestDTO.ProductName, command.RequestDTO.DisplayText, productType)
                };
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(contractAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddTransactionFeeForProductToContract(ContractCommands.AddTransactionFeeForProductToContractCommand command, CancellationToken cancellationToken)
        {
            Models.Contract.CalculationType calculationType = (Models.Contract.CalculationType)command.RequestDTO.CalculationType;
            Models.Contract.FeeType feeType = (Models.Contract.FeeType)command.RequestDTO.FeeType;

            try
            {
                Result<ContractAggregate> contractResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<ContractAggregate>(command.ContractId, ct), command.ContractId, cancellationToken);
                if (contractResult.IsFailed)
                    return ResultHelpers.CreateFailure(contractResult);

                ContractAggregate contractAggregate = contractResult.Data;
                if (contractAggregate.IsCreated == false)
                {
                    return Result.Forbidden($"Contract Id [{command.ContractId}] must be created to add transaction fees");
                }

                List<Product> products = contractAggregate.GetProducts();
                Product product = products.SingleOrDefault(p => p.ContractProductId == command.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product Id [{command.ProductId}] not added to contract [{contractAggregate.Description}]");
                }

                Result stateResult = contractAggregate.AddTransactionFee(product, command.TransactionFeeId, command.RequestDTO.Description, calculationType, feeType, command.RequestDTO.Value);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(contractAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }

        }

        public async Task<Result> DisableTransactionFeeForProduct(ContractCommands.DisableTransactionFeeForProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<ContractAggregate> contractResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<ContractAggregate>(command.ContractId, ct), command.ContractId, cancellationToken);
                if (contractResult.IsFailed)
                    return ResultHelpers.CreateFailure(contractResult);

                ContractAggregate contractAggregate = contractResult.Data;

                Result stateResult = contractAggregate.DisableTransactionFee(command.ProductId, command.TransactionFeeId);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(contractAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> CreateContract(ContractCommands.CreateContractCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<ContractAggregate> contractResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<ContractAggregate>(command.ContractId, ct), command.ContractId, cancellationToken, false);
                if (contractResult.IsFailed)
                    return ResultHelpers.CreateFailure(contractResult);

                ContractAggregate contractAggregate = contractResult.Data;
                EstateAggregate estateAggregate = estateResult.Data;


                Models.Estate.Estate estate = estateAggregate.GetEstate();
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

                if (contractAggregate.IsCreated)
                {
                    return Result.Forbidden($"Contract Id [{command.ContractId}] already created for estate [{estate.Name}]");
                }
                Result stateResult = contractAggregate.Create(command.EstateId, command.RequestDTO.OperatorId, command.RequestDTO.Description);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(contractAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        #endregion
    }
}