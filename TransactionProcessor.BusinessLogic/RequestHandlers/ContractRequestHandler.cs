using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;

namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="CreateContractRequest.String}" />
    /// <seealso cref="AddProductToContractRequest.String}" />
    /// <seealso cref="AddTransactionFeeForProductToContractRequest.String}" />
    public class ContractRequestHandler : IRequestHandler<ContractCommands.CreateContractCommand, Result>,
                                          IRequestHandler<ContractCommands.AddProductToContractCommand, Result>,
                                          IRequestHandler<ContractCommands.AddTransactionFeeForProductToContractCommand, Result>,
                                          IRequestHandler<ContractCommands.DisableTransactionFeeForProductCommand, Result>,
                                          IRequestHandler<ContractQueries.GetContractQuery, Result<Models.Contract.Contract>>,
                                          IRequestHandler<ContractQueries.GetContractsQuery, Result<List<Models.Contract.Contract>>>
    {
        #region Fields

        private readonly IContractDomainService ContractDomainService;
        private readonly ITransactionProcessorManager TransactionProcessorManager;

        #endregion

        #region Constructors

        public ContractRequestHandler(IContractDomainService contractDomainService, ITransactionProcessorManager transactionProcessorManager) {
            this.ContractDomainService = contractDomainService;
            this.TransactionProcessorManager = transactionProcessorManager;
        }

        #endregion

        #region Methods

        public async Task<Result> Handle(ContractCommands.CreateContractCommand command,
                                 CancellationToken cancellationToken)
        {
            return await this.ContractDomainService.CreateContract(command, cancellationToken);
        }
        
        public async Task<Result> Handle(ContractCommands.AddProductToContractCommand command,
                                         CancellationToken cancellationToken)
        {
            return await this.ContractDomainService.AddProductToContract(command, cancellationToken);
        }
        
        public async Task<Result> Handle(ContractCommands.AddTransactionFeeForProductToContractCommand command,
                                 CancellationToken cancellationToken)
        {
            return await this.ContractDomainService.AddTransactionFeeForProductToContract(command, cancellationToken);
        }

        #endregion

        public async Task<Result> Handle(ContractCommands.DisableTransactionFeeForProductCommand command, CancellationToken cancellationToken)
        {
            return await this.ContractDomainService.DisableTransactionFeeForProduct(command, cancellationToken);
        }

        public async Task<Result<Models.Contract.Contract>> Handle(ContractQueries.GetContractQuery query,
                                                                   CancellationToken cancellationToken) {
            Result<Models.Contract.Contract> result =
                await this.TransactionProcessorManager.GetContract(query.EstateId, query.ContractId, cancellationToken);
            return result;
        }

        public async Task<Result<List<Models.Contract.Contract>>> Handle(ContractQueries.GetContractsQuery query,
                                                                         CancellationToken cancellationToken) {
            Result<List<Models.Contract.Contract>> result = await this.TransactionProcessorManager.GetContracts(query.EstateId, cancellationToken);
            return result;
        }
    }
}