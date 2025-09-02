using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Services
{
    public interface IOperatorDomainService
    {
        Task<Result> CreateOperator(OperatorCommands.CreateOperatorCommand command, CancellationToken cancellationToken);

        Task<Result> UpdateOperator(OperatorCommands.UpdateOperatorCommand command, CancellationToken cancellationToken);
    }

    public class OperatorDomainService : IOperatorDomainService
    {
        private readonly IAggregateService AggregateService;

        public OperatorDomainService(Func<IAggregateService> aggregateService) {
            this.AggregateService = aggregateService();
        }
        
        public async Task<Result> CreateOperator(OperatorCommands.CreateOperatorCommand command, CancellationToken cancellationToken)
        {
            try {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<OperatorAggregate> operatorResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<OperatorAggregate>(command.RequestDto.OperatorId, ct), command.RequestDto.OperatorId, cancellationToken, false);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(operatorResult);

                EstateAggregate estateAggregate = estateResult.Data;
                OperatorAggregate operatorAggregate = operatorResult.Data;

                if (estateAggregate.IsCreated == false) {
                    return Result.Forbidden($"Estate with Id {command.EstateId} not created");
                }

                if (operatorAggregate.IsCreated) {
                    return Result.Forbidden($"Operator with Id {command.RequestDto.OperatorId} already created");
                }

                Result stateResult = operatorAggregate.Create(command.EstateId, command.RequestDto.Name, command.RequestDto.RequireCustomMerchantNumber.GetValueOrDefault(), command.RequestDto.RequireCustomTerminalNumber.GetValueOrDefault());
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(operatorAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> UpdateOperator(OperatorCommands.UpdateOperatorCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<OperatorAggregate> operatorResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<OperatorAggregate>(command.OperatorId, ct), command.OperatorId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(operatorResult);

                EstateAggregate estateAggregate = estateResult.Data;
                OperatorAggregate operatorAggregate = operatorResult.Data;

                if (estateAggregate.IsCreated == false)
                {
                    return Result.Forbidden($"Estate with Id {command.EstateId} not created");
                }

                Result stateResult = operatorAggregate.UpdateOperator(command.RequestDto.Name,
                        command.RequestDto.RequireCustomMerchantNumber.GetValueOrDefault(),
                        command.RequestDto.RequireCustomTerminalNumber.GetValueOrDefault());
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(operatorAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }

        }
    }
}
