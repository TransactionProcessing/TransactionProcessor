﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Manager
{
    public interface IEstateManagementManager
    {
        #region Methods

        //Task<Result<List<Contract>>> GetMerchantContracts(Guid estateId,
        //                                                  Guid merchantId,
        //                                                  CancellationToken cancellationToken);

        //Task<Result<List<Contract>>> GetContracts(Guid estateId,
        //                                          CancellationToken cancellationToken);

        //Task<Result<Contract>> GetContract(Guid estateId,
        //                          Guid contractId,
        //                          CancellationToken cancellationToken);

        Task<Result<Models.Estate>> GetEstate(Guid estateId,
                                                        CancellationToken cancellationToken);

        Task<Result<List<Models.Estate>>> GetEstates(Guid estateId,
                                                               CancellationToken cancellationToken);

        //Task<Result<EstateManagement.Merchant>> GetMerchant(Guid estateId, Guid merchantId,
        //                                                    CancellationToken cancellationToken);

        //Task<Result<List<EstateManagement.Merchant>>> GetMerchants(Guid estateId, CancellationToken cancellationToken);

        //Task<Result<List<ContractProductTransactionFee>>> GetTransactionFeesForProduct(Guid estateId,
        //                                                        Guid merchantId,
        //                                                        Guid contractId,
        //                                                        Guid productId,
        //                                                        CancellationToken cancellationToken);

        //Task<Result<File>> GetFileDetails(Guid estateId, Guid fileId, CancellationToken cancellationToken);

        //Task<Result<Operator>> GetOperator(Guid estateId,Guid operatorId,
        //                       CancellationToken cancellationToken);

        //Task<Result<List<Operator>>> GetOperators(Guid estateId, CancellationToken cancellationToken);

        #endregion
    }
}