using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.Exceptions;
using TransactionProcessing.UnionResults;
using TransactionProcessing.UnionResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Services
{
    public interface IOperatorDomainService
    {
        Task<Result<Guid>> CreateOperator(OperatorCommands.CreateOperatorCommand command, CancellationToken cancellationToken);

        Task<Result> UpdateOperator(OperatorCommands.UpdateOperatorCommand command, CancellationToken cancellationToken);
    }

    public class OperatorDomainService : IOperatorDomainService
    {
        private readonly IAggregateService AggregateService;

        public OperatorDomainService(Func<IAggregateService> aggregateService) {
            this.AggregateService = aggregateService();
        }
        
        public async Task<Result<Guid>> CreateOperator(OperatorCommands.CreateOperatorCommand command, CancellationToken cancellationToken)
        {
            try {
                // Check if we have been sent a merchant id to use
                Guid operatorId = IdGenerationService.GenerateOperatorId(command.EstateId, command.RequestDto.Name);

                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<OperatorAggregate> operatorResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<OperatorAggregate>(operatorId, ct), operatorId, cancellationToken, false);
                if (operatorResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(operatorResult);

                EstateAggregate estateAggregate = estateResult.Data;
                OperatorAggregate operatorAggregate = operatorResult.Data;

                if (estateAggregate.IsCreated == false) {
                    return new Error(ErrorType.Forbidden, $"Estate with Id {command.EstateId} not created");
                }

                if (operatorAggregate.IsCreated) {
                    return new Error(ErrorType.Forbidden, $"Operator with Id {operatorId} already created");
                }

                Result stateResult = operatorAggregate.Create(command.EstateId, command.RequestDto.Name, command.RequestDto.RequireCustomMerchantNumber.GetValueOrDefault(), command.RequestDto.RequireCustomTerminalNumber.GetValueOrDefault());
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(operatorAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return Results.Ok(operatorId);
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> UpdateOperator(OperatorCommands.UpdateOperatorCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<OperatorAggregate> operatorResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<OperatorAggregate>(command.OperatorId, ct), command.OperatorId, cancellationToken);
                if (operatorResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(operatorResult);

                EstateAggregate estateAggregate = estateResult.Data;
                OperatorAggregate operatorAggregate = operatorResult.Data;

                if (estateAggregate.IsCreated == false)
                {
                    return new Error(ErrorType.Forbidden, $"Estate with Id {command.EstateId} not created");
                }

                Result stateResult = operatorAggregate.UpdateOperator(command.RequestDto.Name,
                        command.RequestDto.RequireCustomMerchantNumber.GetValueOrDefault(),
                        command.RequestDto.RequireCustomTerminalNumber.GetValueOrDefault());
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(operatorAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return Results.Ok();
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }

        }
    }
}
