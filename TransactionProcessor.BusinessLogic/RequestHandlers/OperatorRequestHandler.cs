using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;

namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    public class OperatorRequestHandler : IRequestHandler<OperatorCommands.CreateOperatorCommand, Result>,
        IRequestHandler<OperatorQueries.GetOperatorQuery, Result<Models.Operator.Operator>>,
        IRequestHandler<OperatorQueries.GetOperatorsQuery, Result<List<Models.Operator.Operator>>>,
        IRequestHandler<OperatorCommands.UpdateOperatorCommand, Result>
    {
        private readonly IOperatorDomainService OperatorDomainService;

        private readonly IEstateManagementManager EstateManagementManager;

        public OperatorRequestHandler(IOperatorDomainService operatorDomainService, IEstateManagementManager estateManagementManager)
        {
            this.OperatorDomainService = operatorDomainService;
            this.EstateManagementManager = estateManagementManager;
        }
        public async Task<Result> Handle(OperatorCommands.CreateOperatorCommand command, CancellationToken cancellationToken)
        {
            return await this.OperatorDomainService.CreateOperator(command, cancellationToken);
        }

        public async Task<Result<Models.Operator.Operator>> Handle(OperatorQueries.GetOperatorQuery query, CancellationToken cancellationToken)
        {
            return await this.EstateManagementManager.GetOperator(query.EstateId, query.OperatorId, cancellationToken);
        }

        public async Task<Result<List<Models.Operator.Operator>>> Handle(OperatorQueries.GetOperatorsQuery query, CancellationToken cancellationToken)
        {
            return await this.EstateManagementManager.GetOperators(query.EstateId, cancellationToken);
        }

        public async Task<Result> Handle(OperatorCommands.UpdateOperatorCommand command, CancellationToken cancellationToken)
        {
            return await this.OperatorDomainService.UpdateOperator(command, cancellationToken);
        }
    }
}
