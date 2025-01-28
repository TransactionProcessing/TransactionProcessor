using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models;
using TransactionProcessor.ProjectionEngine.Repository;
using TransactionProcessor.Repository;

namespace TransactionProcessor.BusinessLogic.Manager
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IEstateManagementManager" />
    public class EstateManagementManager : IEstateManagementManager
    {
        #region Fields
        
        private readonly ITransactionProcessorReadModelRepository EstateManagementRepository;

        private readonly IAggregateRepository<EstateAggregate, DomainEvent> EstateAggregateRepository;

        //private readonly IAggregateRepository<ContractAggregate, DomainEvent> ContractAggregateRepository;

        //private readonly IAggregateRepository<MerchantAggregate, DomainEvent> MerchantAggregateRepository;

        //private readonly IAggregateRepository<OperatorAggregate, DomainEvent> OperatorAggregateRepository;

        //private readonly IModelFactory ModelFactory;

        #endregion

        #region Constructors
        
        public EstateManagementManager(ITransactionProcessorReadModelRepository estateManagementRepository,
                                       IAggregateRepository<EstateAggregate, DomainEvent> estateAggregateRepository)
                                       //IAggregateRepository<ContractAggregate,DomainEvent> contractAggregateRepository,
                                       //IAggregateRepository<MerchantAggregate, DomainEvent> merchantAggregateRepository,
                                       //IModelFactory modelFactory,
                                       //IAggregateRepository<OperatorAggregate, DomainEvent> operatorAggregateRepository)
        {
            this.EstateManagementRepository = estateManagementRepository;
            this.EstateAggregateRepository = estateAggregateRepository;
            //this.ContractAggregateRepository = contractAggregateRepository;
            //this.MerchantAggregateRepository = merchantAggregateRepository;
            //this.OperatorAggregateRepository = operatorAggregateRepository;
            //this.ModelFactory = modelFactory;
        }

        #endregion

        #region Methods

        //public async Task<Result<List<Contract>>> GetContracts(Guid estateId,
        //                                                       CancellationToken cancellationToken)
        //{
        //    Result<List<Contract>> getContractsResult = await this.EstateManagementRepository.GetContracts(estateId, cancellationToken);

        //    if (getContractsResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getContractsResult);

        //    return  Result.Success(getContractsResult.Data);
        //}

        //public async Task<Result<Contract>> GetContract(Guid estateId,
        //                                        Guid contractId,
        //                                        CancellationToken cancellationToken){
        //    Result<ContractAggregate> getContractResult = await this.ContractAggregateRepository.GetLatestVersion(contractId, cancellationToken);
        //    if (getContractResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getContractResult);

        //    ContractAggregate contractAggregate = getContractResult.Data;
            
        //    if (contractAggregate.IsCreated == false){
        //        return Result.NotFound($"No contract found with Id [{estateId}]");
        //    }
        //    Contract contractModel = contractAggregate.GetContract();

        //    return Result.Success(contractModel);
        //}

        public async Task<Result<Models.Estate>> GetEstate(Guid estateId,
                                                  CancellationToken cancellationToken){

            Result<EstateAggregate> getEstateResult = await this.EstateAggregateRepository.GetLatestVersion(estateId, cancellationToken);
            if (getEstateResult.IsFailed)
                return ResultHelpers.CreateFailure(getEstateResult);
            
            EstateAggregate estateAggregate = getEstateResult.Data;
            if (estateAggregate.IsCreated == false){
                return Result.NotFound($"No estate found with Id [{estateId}]");
            }

            Models.Estate estateModel = estateAggregate.GetEstate();

            //if (estateModel.Operators != null){
            //    foreach (Operator @operator in estateModel.Operators){
            //        OperatorAggregate operatorAggregate = await this.OperatorAggregateRepository.GetLatestVersion(@operator.OperatorId, cancellationToken);
            //        @operator.Name = operatorAggregate.Name;
            //    }
            //}

            return Result.Success(estateModel);
        }

        public async Task<Result<List<Models.Estate>>> GetEstates(Guid estateId,
                                                   CancellationToken cancellationToken){
            Result<Models.Estate> getEstateResult= await this.EstateManagementRepository.GetEstate(estateId, cancellationToken);
            if (getEstateResult.IsFailed)
                return Result.NotFound($"No estate found with Id [{estateId}]");
            
            return Result.Success(new List<Models.Estate>(){
                getEstateResult.Data
                                     });
        }

        //public async Task<Result<Merchant>> GetMerchant(Guid estateId,
        //                                        Guid merchantId,
        //                                        CancellationToken cancellationToken)
        //{
        //    Result<MerchantAggregate> getMerchantResult= await this.MerchantAggregateRepository.GetLatestVersion(merchantId, cancellationToken);
        //    if (getMerchantResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getMerchantResult);

        //    MerchantAggregate merchantAggregate = getMerchantResult.Data;
        //    if (merchantAggregate.IsCreated == false)
        //    {
        //        return Result.NotFound($"No merchant found with Id [{merchantId}]");
        //    }

        //    Merchant merchantModel = merchantAggregate.GetMerchant();

        //    if (merchantModel.Operators != null){
        //        foreach (Models.Merchant.Operator @operator in merchantModel.Operators){
        //            OperatorAggregate operatorAggregate = await this.OperatorAggregateRepository.GetLatestVersion(@operator.OperatorId, cancellationToken);
        //            @operator.Name = operatorAggregate.Name;
        //        }
        //    }

        //    return Result.Success(merchantModel);
        //}
        
        //public async Task<Result<List<Contract>>> GetMerchantContracts(Guid estateId,
        //                                                       Guid merchantId,
        //                                                       CancellationToken cancellationToken)
        //{
        //    Result<List<Contract>> getMerchantContractsResult = await this.EstateManagementRepository.GetMerchantContracts(estateId, merchantId, cancellationToken);
        //    if (getMerchantContractsResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getMerchantContractsResult);

        //    var contractModels = getMerchantContractsResult.Data;
        //    if (contractModels.Any() == false)
        //        return Result.NotFound($"No contracts for Estate {estateId} and Merchant {merchantId}");

        //    return Result.Success(contractModels);
        //}

        //public async Task<Result<List<Merchant>>> GetMerchants(Guid estateId,
        //                                               CancellationToken cancellationToken)
        //{
        //    var getMerchantsResult= await this.EstateManagementRepository.GetMerchants(estateId, cancellationToken);
        //    if (getMerchantsResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getMerchantsResult);
        //    var merchants = getMerchantsResult.Data;
        //    if (merchants == null || merchants.Any() == false)
        //    {
        //        return Result.NotFound($"No Merchants found for estate Id {estateId}");
        //    }

        //    return Result.Success(merchants);
        //}
        
        //public async Task<Result<List<Models.Contract.ContractProductTransactionFee>>> GetTransactionFeesForProduct(Guid estateId,
        //                                                                     Guid merchantId,
        //                                                                     Guid contractId,
        //                                                                     Guid productId,
        //                                                                     CancellationToken cancellationToken)
        //{
        //    // TODO: this will need updated to handle merchant specific fees when that is available

        //    Result<ContractAggregate> getContractResult = await this.ContractAggregateRepository.GetLatestVersion(contractId, cancellationToken);
        //    if (getContractResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getContractResult);
        //    ContractAggregate contract = getContractResult.Data;
        //    if (contract.IsCreated == false){
        //        return Result.NotFound($"No contract found with Id [{contractId}]");
        //    }

        //    List<Product> products = contract.GetProducts();

        //    Product product = products.SingleOrDefault(p => p.ContractProductId == productId);

        //    if (product == null){
        //        return Result.NotFound($"No product found with Id [{productId}] on contract Id [{contractId}]");
        //    }

        //    return  Result.Success(product.TransactionFees);

        //}

        //public async Task<Result<File>> GetFileDetails(Guid estateId, Guid fileId, CancellationToken cancellationToken){
        //    var getFileDetailsResult= await this.EstateManagementRepository.GetFileDetails(estateId, fileId, cancellationToken);
        //    if (getFileDetailsResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getFileDetailsResult);

        //    return Result.Success(getFileDetailsResult.Data);
        //}

        //public async Task<Result<Models.Operator.Operator>> GetOperator(Guid estateId, Guid operatorId, CancellationToken cancellationToken){
        //    var getOperatorResult  = await this.OperatorAggregateRepository.GetLatestVersion(operatorId, cancellationToken);
        //    if (getOperatorResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getOperatorResult);
        //    var operatorAggregate = getOperatorResult.Data;
        //    if (operatorAggregate.IsCreated == false){
        //        return Result.NotFound($"No operator found with Id [{operatorId}]");
        //    }

        //    Models.Operator.Operator @operator = operatorAggregate.GetOperator();

        //    return  Result.Success(@operator);
        //}

        //public async Task<Result<List<Models.Operator.Operator>>> GetOperators(Guid estateId, CancellationToken cancellationToken){
        //    Result<List<Models.Operator.Operator>> getOperatorsResult = await this.EstateManagementRepository.GetOperators(estateId, cancellationToken);
        //    if (getOperatorsResult.IsFailed)
        //        return ResultHelpers.CreateFailure(getOperatorsResult);

        //    return Result.Success(getOperatorsResult.Data);
        //}

        #endregion
    }
}