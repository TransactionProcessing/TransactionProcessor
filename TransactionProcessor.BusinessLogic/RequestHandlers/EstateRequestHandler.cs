using MediatR;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;

namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    public class EstateRequestHandler : IRequestHandler<EstateCommands.CreateEstateCommand, Result>,
                                        IRequestHandler<EstateCommands.AddOperatorToEstateCommand, Result>,
                                        IRequestHandler<EstateCommands.RemoveOperatorFromEstateCommand, Result>,
                                        IRequestHandler<EstateCommands.CreateEstateUserCommand, Result>,
                                        IRequestHandler<EstateQueries.GetEstateQuery, Result<Models.Estate>>,
                                        IRequestHandler<EstateQueries.GetEstatesQuery, Result<List<Models.Estate>>>
    {
        #region Fields

        private readonly IEstateDomainService EstateDomainService;

        private readonly IEstateManagementManager EstateManagementManager;

        #endregion

        #region Constructors

        public EstateRequestHandler(IEstateDomainService estateDomainService, IEstateManagementManager manager) {
            this.EstateDomainService = estateDomainService;
            this.EstateManagementManager = manager;
        }

        #endregion

        #region Methods

        public async Task<Result> Handle(EstateCommands.CreateEstateCommand command, CancellationToken cancellationToken)
        {
            return await this.EstateDomainService.CreateEstate(command, cancellationToken);
        }

        public async Task<Result> Handle(EstateCommands.CreateEstateUserCommand command, CancellationToken cancellationToken)
        {
            return await this.EstateDomainService.CreateEstateUser(command, cancellationToken);
        }

        public async Task<Result> Handle(EstateCommands.AddOperatorToEstateCommand command,
                                         CancellationToken cancellationToken)
        {
            return await this.EstateDomainService.AddOperatorToEstate(command, cancellationToken);
        }
        
        public async Task<Result> Handle(EstateCommands.RemoveOperatorFromEstateCommand command, CancellationToken cancellationToken)
        {
            return await this.EstateDomainService.RemoveOperatorFromEstate(command, cancellationToken);
        }

        public async Task<Result<Models.Estate>> Handle(EstateQueries.GetEstateQuery query, CancellationToken cancellationToken)
        {
            return await this.EstateManagementManager.GetEstate(query.EstateId, cancellationToken);
        }

        public async Task<Result<List<Models.Estate>>> Handle(EstateQueries.GetEstatesQuery query, CancellationToken cancellationToken)
        {
            return await this.EstateManagementManager.GetEstates(query.EstateId, cancellationToken);
        }
        #endregion
    }
}
