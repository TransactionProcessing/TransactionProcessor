using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleResults;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;
using Contract = TransactionProcessor.Models.Contract.Contract;

namespace TransactionProcessor.BusinessLogic.Manager
{
    public interface IEstateManagementManager
    {
        #region Methods

        Task<Result<List<Contract>>> GetMerchantContracts(Guid estateId,
                                                          Guid merchantId,
                                                          CancellationToken cancellationToken);

        Task<Result<List<Contract>>> GetContracts(Guid estateId,
                                                  CancellationToken cancellationToken);

        Task<Result<Contract>> GetContract(Guid estateId,
                                  Guid contractId,
                                  CancellationToken cancellationToken);

        Task<Result<Models.Estate.Estate>> GetEstate(Guid estateId,
                                                        CancellationToken cancellationToken);

        Task<Result<List<Models.Estate.Estate>>> GetEstates(Guid estateId,
                                                               CancellationToken cancellationToken);

        Task<Result<Merchant>> GetMerchant(Guid estateId, Guid merchantId,
                                                            CancellationToken cancellationToken);

        Task<Result<List<Merchant>>> GetMerchants(Guid estateId, CancellationToken cancellationToken);

        Task<Result<List<ContractProductTransactionFee>>> GetTransactionFeesForProduct(Guid estateId,
                                                                Guid merchantId,
                                                                Guid contractId,
                                                                Guid productId,
                                                                CancellationToken cancellationToken);

        //Task<Result<File>> GetFileDetails(Guid estateId, Guid fileId, CancellationToken cancellationToken);

        Task<Result<Models.Operator.Operator>> GetOperator(Guid estateId, Guid operatorId,
                               CancellationToken cancellationToken);

        Task<Result<List<Models.Operator.Operator>>> GetOperators(Guid estateId, CancellationToken cancellationToken);

        #endregion
    }
}