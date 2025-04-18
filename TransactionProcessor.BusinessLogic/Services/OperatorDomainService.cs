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

        public OperatorDomainService(IAggregateService aggregateService) {
            this.AggregateService = aggregateService;
        }

        private async Task<Result> ApplyUpdates(Func<(EstateAggregate, OperatorAggregate), Result> action, Guid estateId, Guid operatorId, CancellationToken cancellationToken, Boolean isNotFoundError = true)
        {
            try
            {
                Result<EstateAggregate> getEstateResult = await this.AggregateService.Get<EstateAggregate>(estateId, cancellationToken);
                if (getEstateResult.IsFailed) {
                    return ResultHelpers.CreateFailure(getEstateResult);
                }
                EstateAggregate estateAggregate = getEstateResult.Data;
                Result<OperatorAggregate> getOperatorResult = await this.AggregateService.GetLatest<OperatorAggregate>(operatorId, cancellationToken);
                Result<OperatorAggregate> operatorAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getOperatorResult, operatorId, isNotFoundError);
                if (operatorAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(operatorAggregateResult);

                OperatorAggregate operatorAggregate = operatorAggregateResult.Data;

                Result result = action((estateAggregate, operatorAggregate));
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

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

        public async Task<Result> CreateOperator(OperatorCommands.CreateOperatorCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(((EstateAggregate estateAggregate, OperatorAggregate operatorAggregate) aggregates) => {
                if (aggregates.estateAggregate.IsCreated == false)
                {
                    return Result.Forbidden($"Estate with Id {command.EstateId} not created");
                }

                if (aggregates.operatorAggregate.IsCreated)
                {
                    return Result.Forbidden($"Operator with Id {command.RequestDto.OperatorId} already created");
                }

                aggregates.operatorAggregate.Create(command.EstateId, command.RequestDto.Name,
                    command.RequestDto.RequireCustomMerchantNumber.GetValueOrDefault(),
                    command.RequestDto.RequireCustomTerminalNumber.GetValueOrDefault());

                return Result.Success();

            }, command.EstateId, command.RequestDto.OperatorId, cancellationToken, isNotFoundError: false);

            return result;
        }

        public async Task<Result> UpdateOperator(OperatorCommands.UpdateOperatorCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(((EstateAggregate estateAggregate, OperatorAggregate operatorAggregate) aggregates) => {
                aggregates.operatorAggregate.UpdateOperator(command.RequestDto.Name,
                    command.RequestDto.RequireCustomMerchantNumber.GetValueOrDefault(),
                    command.RequestDto.RequireCustomTerminalNumber.GetValueOrDefault());

                return Result.Success();

            }, command.EstateId, command.OperatorId, cancellationToken);

            return result;
        }
    }
}
