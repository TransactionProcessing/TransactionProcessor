using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Results;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Models.Settlement;
using TransactionProcessor.Repository;
using static TransactionProcessor.BusinessLogic.Requests.SettlementQueries;
using Contract = TransactionProcessor.Models.Contract.Contract;
using Operator = TransactionProcessor.Models.Estate.Operator;

namespace TransactionProcessor.BusinessLogic.Manager
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ITransactionProcessorManager" />
    public class TransactionProcessorManager : ITransactionProcessorManager
    {
        #region Fields
        
        private readonly ITransactionProcessorReadModelRepository TransactionProcessorReadModelRepository;
        private readonly IAggregateService AggregateService;

        #endregion

        #region Constructors
        
        public TransactionProcessorManager(ITransactionProcessorReadModelRepository transactionProcessorReadModelRepository,
                                       IAggregateService aggregateService)
        {
            this.TransactionProcessorReadModelRepository = transactionProcessorReadModelRepository;
            this.AggregateService = aggregateService;
        }

        #endregion

        #region Methods

        public async Task<Result<List<Contract>>> GetContracts(Guid estateId,
                                                                    CancellationToken cancellationToken)
        {
            Result<List<Contract>> getContractsResult = await this.TransactionProcessorReadModelRepository.GetContracts(estateId, cancellationToken);

            if (getContractsResult.IsFailed)
                return ResultHelpers.CreateFailure(getContractsResult);

            return Result.Success(getContractsResult.Data);
        }

        public async Task<Result<Contract>> GetContract(Guid estateId,
                                                Guid contractId,
                                                CancellationToken cancellationToken)
        {
            Result<ContractAggregate> getContractResult = await this.AggregateService.GetLatest<ContractAggregate>(contractId, cancellationToken);
            if (getContractResult.IsFailed)
                return ResultHelpers.CreateFailure(getContractResult);

            ContractAggregate contractAggregate = getContractResult.Data;

            if (contractAggregate.IsCreated == false)
            {
                return Result.NotFound($"No contract found with Id [{estateId}]");
            }
            Contract contractModel = contractAggregate.GetContract();

            return Result.Success(contractModel);
        }

        public async Task<Result<Models.Estate.Estate>> GetEstate(Guid estateId,
                                                  CancellationToken cancellationToken){

            Result<EstateAggregate> getEstateResult = await this.AggregateService.GetLatest<EstateAggregate>(estateId, cancellationToken);
            if (getEstateResult.IsFailed)
                return ResultHelpers.CreateFailure(getEstateResult);
            
            EstateAggregate estateAggregate = getEstateResult.Data;
            if (estateAggregate.IsCreated == false){
                return Result.NotFound($"No estate found with Id [{estateId}]");
            }

            Models.Estate.Estate estateModel = estateAggregate.GetEstate();

            if (estateModel.Operators != null)
            {
                foreach (Operator @operator in estateModel.Operators)
                {
                    var getOperatorResult = await this.AggregateService.GetLatest<OperatorAggregate>(@operator.OperatorId, cancellationToken);
                    if (getOperatorResult.IsSuccess) {
                        OperatorAggregate operatorAggregate = getOperatorResult.Data;
                        @operator.Name = operatorAggregate.Name;
                    }
                }
            }

            return Result.Success(estateModel);
        }

        public async Task<Result<List<Models.Estate.Estate>>> GetEstates(Guid estateId,
                                                   CancellationToken cancellationToken){
            Result<Models.Estate.Estate> getEstateResult= await this.TransactionProcessorReadModelRepository.GetEstate(estateId, cancellationToken);
            if (getEstateResult.IsFailed)
                return Result.NotFound($"No estate found with Id [{estateId}]");
            
            return Result.Success(new List<Models.Estate.Estate>(){
                getEstateResult.Data
                                     });
        }

        public async Task<Result<Merchant>> GetMerchant(Guid estateId,
                                                Guid merchantId,
                                                CancellationToken cancellationToken)
        {
            Result<MerchantAggregate> getMerchantResult = await this.AggregateService.GetLatest<MerchantAggregate>(merchantId, cancellationToken);
            if (getMerchantResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantResult);

            MerchantAggregate merchantAggregate = getMerchantResult.Data;
            if (merchantAggregate.IsCreated == false)
            {
                return Result.NotFound($"No merchant found with Id [{merchantId}]");
            }

            Merchant merchantModel = merchantAggregate.GetMerchant();

            if (merchantModel.Operators != null)
            {
                List<Models.Merchant.Operator> operators = new();
                foreach (Models.Merchant.Operator @operator in merchantModel.Operators)
                {
                    var getOperatorResult = await this.AggregateService.GetLatest<OperatorAggregate>(@operator.OperatorId, cancellationToken);
                    if (getOperatorResult.IsFailed)
                        return ResultHelpers.CreateFailure(getOperatorResult);
                    OperatorAggregate operatorAggregate = getOperatorResult.Data;
                    Models.Merchant.Operator newOperator = @operator with { Name = operatorAggregate.Name };
                    operators.Add(newOperator);
                }
            }

            return Result.Success(merchantModel);
        }

        public async Task<Result<List<Contract>>> GetMerchantContracts(Guid estateId,
                                                               Guid merchantId,
                                                               CancellationToken cancellationToken)
        {
            Result<List<Contract>> getMerchantContractsResult = await this.TransactionProcessorReadModelRepository.GetMerchantContracts(estateId, merchantId, cancellationToken);
            if (getMerchantContractsResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantContractsResult);

            List<Contract> contractModels = getMerchantContractsResult.Data;
            if (contractModels.Any() == false)
                return Result.NotFound($"No contracts for Estate {estateId} and Merchant {merchantId}");

            return Result.Success(contractModels);
        }

        public async Task<Result<List<Merchant>>> GetMerchants(Guid estateId,
                                                       CancellationToken cancellationToken)
        {
            Result<List<Merchant>> getMerchantsResult = await this.TransactionProcessorReadModelRepository.GetMerchants(estateId, cancellationToken);
            if (getMerchantsResult.IsFailed)
                return ResultHelpers.CreateFailure(getMerchantsResult);
            List<Merchant> merchants = getMerchantsResult.Data;
            if (merchants == null || merchants.Any() == false)
            {
                return Result.NotFound($"No Merchants found for estate Id {estateId}");
            }

            return Result.Success(merchants);
        }

        public async Task<Result<List<Models.Contract.ContractProductTransactionFee>>> GetTransactionFeesForProduct(Guid estateId,
                                                                             Guid merchantId,
                                                                             Guid contractId,
                                                                             Guid productId,
                                                                             CancellationToken cancellationToken)
        {
            Result<ContractAggregate> getContractResult = await this.AggregateService.GetLatest<ContractAggregate>(contractId, cancellationToken);
            if (getContractResult.IsFailed)
                return ResultHelpers.CreateFailure(getContractResult);
            ContractAggregate contract = getContractResult.Data;
            if (contract.IsCreated == false)
            {
                return Result.NotFound($"No contract found with Id [{contractId}]");
            }

            List<Product> products = contract.GetProducts();

            Product product = products.SingleOrDefault(p => p.ContractProductId == productId);

            if (product == null)
            {
                return Result.NotFound($"No product found with Id [{productId}] on contract Id [{contractId}]");
            }

            return Result.Success(product.TransactionFees);
        }

        public async Task<Result<Models.Operator.Operator>> GetOperator(Guid estateId, Guid operatorId, CancellationToken cancellationToken)
        {
            Result<OperatorAggregate> getOperatorResult = await this.AggregateService.GetLatest<OperatorAggregate>(operatorId, cancellationToken);
            if (getOperatorResult.IsFailed)
                return ResultHelpers.CreateFailure(getOperatorResult);
            OperatorAggregate operatorAggregate = getOperatorResult.Data;
            if (operatorAggregate.IsCreated == false)
            {
                return Result.NotFound($"No operator found with Id [{operatorId}]");
            }

            Models.Operator.Operator @operator = operatorAggregate.GetOperator();

            return Result.Success(@operator);
        }

        public async Task<Result<List<Models.Operator.Operator>>> GetOperators(Guid estateId, CancellationToken cancellationToken)
        {
            Result<List<Models.Operator.Operator>> getOperatorsResult = await this.TransactionProcessorReadModelRepository.GetOperators(estateId, cancellationToken);
            if (getOperatorsResult.IsFailed)
                return ResultHelpers.CreateFailure(getOperatorsResult);

            return Result.Success(getOperatorsResult.Data);
        }

        public async Task<Result<SettlementModel>> GetSettlement(Guid estateId,
                                                                 Guid merchantId,
                                                                 Guid settlementId,
                                                                 CancellationToken cancellationToken)
        {
            return await this.TransactionProcessorReadModelRepository.GetSettlement(estateId, merchantId, settlementId, cancellationToken);
        }

        public async Task<Result<List<SettlementModel>>> GetSettlements(Guid estateId,
                                                                        Guid? merchantId,
                                                                        String startDate,
                                                                        String endDate,
                                                                        CancellationToken cancellationToken)
        {
            return await this.TransactionProcessorReadModelRepository.GetSettlements(estateId, merchantId, startDate, endDate, cancellationToken);
        }

        public async Task<Result<PendingSettlementModel>> GetPendingSettlement(Guid estateId,
                                                                               Guid merchantId,
                                                                               DateTime settlementDate,
                                                                               CancellationToken cancellationToken) {

            Guid aggregateId = Helpers.CalculateSettlementAggregateId(settlementDate, merchantId, estateId);

            Result<SettlementAggregate> getSettlementResult = await this.AggregateService.GetLatest<SettlementAggregate>(aggregateId, cancellationToken);
            if (getSettlementResult.IsFailed)
                return ResultHelpers.CreateFailure(getSettlementResult);

            SettlementAggregate settlementAggregate = getSettlementResult.Data;

            PendingSettlementModel model = new PendingSettlementModel {
                EstateId = settlementAggregate.EstateId,
                MerchantId = settlementAggregate.MerchantId,
                NumberOfFeesPendingSettlement = settlementAggregate.GetNumberOfFeesPendingSettlement(),
                NumberOfFeesSettled = settlementAggregate.GetNumberOfFeesSettled(),
                SettlementDate = settlementAggregate.SettlementDate,
                SettlementCompleted = settlementAggregate.SettlementComplete,
            };

            return Result.Success(model);
        }

        #endregion
    }
}