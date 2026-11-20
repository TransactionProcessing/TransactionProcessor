using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.UnionResults;
using TransactionProcessing.UnionResults;
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
using MerchantScheduleModel = TransactionProcessor.Models.MerchantSchedule.MerchantSchedule;
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
            return await AsyncExecutor.ExecuteSafeAsync(async ct =>{
                Result<List<Contract>> getContractsResult = await this.TransactionProcessorReadModelRepository.GetContracts(estateId, ct);

                if (getContractsResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getContractsResult);

                return Results.Ok(getContractsResult.Data);
            }, cancellationToken);
        }

        public async Task<Result<Contract>> GetContract(Guid estateId,
                                                Guid contractId,
                                                CancellationToken cancellationToken)
        {
            return await AsyncExecutor.ExecuteSafeAsync(async ct => {
                Result<ContractAggregate> getContractResult = await this.AggregateService.GetLatest<ContractAggregate>(contractId, ct);
                if (getContractResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getContractResult);

                ContractAggregate contractAggregate = getContractResult.Data;

                if (contractAggregate.IsCreated == false) {
                    return new Error(ErrorType.NotFound, $"No contract found with Id [{estateId}]");
                }

                Contract contractModel = contractAggregate.GetContract();

                return Results.Ok(contractModel);
            }, cancellationToken);
        }

        public async Task<Result<Models.Estate.Estate>> GetEstate(Guid estateId,
                                                  CancellationToken cancellationToken) {

            return await AsyncExecutor.ExecuteSafeAsync(async ct => {
                Result<EstateAggregate> getEstateResult = await this.AggregateService.GetLatest<EstateAggregate>(estateId, ct);
                if (getEstateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getEstateResult);

                EstateAggregate estateAggregate = getEstateResult.Data;
                if (estateAggregate.IsCreated == false) {
                    return new Error(ErrorType.NotFound, $"No estate found with Id [{estateId}]");
                }

                Models.Estate.Estate estateModel = estateAggregate.GetEstate();

                if (estateModel.Operators != null) {
                    foreach (Operator @operator in estateModel.Operators) {
                        var getOperatorResult = await this.AggregateService.GetLatest<OperatorAggregate>(@operator.OperatorId, ct);
                        if (getOperatorResult.IsSuccess()) {
                            OperatorAggregate operatorAggregate = getOperatorResult.Data;
                            @operator.Name = operatorAggregate.Name;
                        }
                    }
                }

                return Results.Ok(estateModel);
            }, cancellationToken);
        }

        public async Task<Result<List<Models.Estate.Estate>>> GetEstates(Guid estateId,
                                                   CancellationToken cancellationToken){
            return await AsyncExecutor.ExecuteSafeAsync(async ct => {
                    Result<Models.Estate.Estate> getEstateResult = await this.TransactionProcessorReadModelRepository.GetEstate(estateId, ct);
                    if (getEstateResult.IsSuccess() == false)
                        return new Error(ErrorType.NotFound, $"No estate found with Id [{estateId}]");

                    return Results.Ok(new List<Models.Estate.Estate>() { getEstateResult.Data });
                },
                cancellationToken);
        }

        public async Task<Result<Merchant>> GetMerchant(Guid estateId,
                                                Guid merchantId,
                                                CancellationToken cancellationToken)
        {
            Result<MerchantAggregate> getMerchantResult = await this.AggregateService.GetLatest<MerchantAggregate>(merchantId, cancellationToken);
            if (getMerchantResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(getMerchantResult);

            MerchantAggregate merchantAggregate = getMerchantResult.Data;
            if (merchantAggregate.IsCreated == false)
            {
                return new Error(ErrorType.NotFound, $"No merchant found with Id [{merchantId}]");
            }

            Merchant merchantModel = merchantAggregate.GetMerchant();

            if (merchantModel.Operators != null)
            {
                List<Models.Merchant.Operator> operators = new();
                foreach (Models.Merchant.Operator @operator in merchantModel.Operators)
                {
                    var getOperatorResult = await this.AggregateService.GetLatest<OperatorAggregate>(@operator.OperatorId, cancellationToken);
                    if (getOperatorResult.IsSuccess() == false)
                        return ResultHelpers.CreateFailure(getOperatorResult);
                    OperatorAggregate operatorAggregate = getOperatorResult.Data;
                    Models.Merchant.Operator newOperator = @operator with { Name = operatorAggregate.Name };
                    operators.Add(newOperator);
                }
            }

            Result<Merchant> merchantReadModelResult = await this.TransactionProcessorReadModelRepository.GetMerchant(estateId, merchantId, cancellationToken);
            if (merchantReadModelResult.IsSuccess() == false) {
                return ResultHelpers.CreateFailure(merchantReadModelResult);
            }

            merchantModel.MerchantReportingId = merchantReadModelResult.Data.MerchantReportingId;

            return Results.Ok(merchantModel);
        }

        public async Task<Result<MerchantScheduleModel>> GetMerchantSchedule(Guid estateId,
                                                                              Guid merchantId,
                                                                              Int32 year,
                                                                              CancellationToken cancellationToken)
        {
            return await AsyncExecutor.ExecuteSafeAsync(async ct => {
                Guid merchantScheduleId = IdGenerationService.GenerateMerchantScheduleAggregateId(estateId, merchantId, year);
                Result<MerchantScheduleAggregate> getMerchantScheduleResult = await this.AggregateService.GetLatest<MerchantScheduleAggregate>(merchantScheduleId, ct);
                if (getMerchantScheduleResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getMerchantScheduleResult);

                MerchantScheduleAggregate merchantScheduleAggregate = getMerchantScheduleResult.Data;
                if (merchantScheduleAggregate.IsCreated == false)
                    return new Error(ErrorType.NotFound, $"No merchant schedule found for Merchant [{merchantId}] in Year [{year}]");

                MerchantScheduleModel merchantScheduleModel = merchantScheduleAggregate.GetSchedule();

                return Results.Ok(merchantScheduleModel);
            }, cancellationToken);
        }

        public async Task<Result<MerchantScheduleModel>> GetMerchantScheduleFromReadModel(Guid estateId,
                                                                                           Guid merchantId,
                                                                                           Int32 year,
                                                                                           CancellationToken cancellationToken)
        {
            return await AsyncExecutor.ExecuteSafeAsync(async ct => {
                Result<MerchantScheduleModel> getMerchantScheduleResult = await this.TransactionProcessorReadModelRepository.GetMerchantSchedule(estateId, merchantId, year, ct);
                if (getMerchantScheduleResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getMerchantScheduleResult);

                return Results.Ok(getMerchantScheduleResult.Data);
            }, cancellationToken);
        }

        public async Task<Result<List<Contract>>> GetMerchantContracts(Guid estateId,
                                                                        Guid merchantId,
                                                                       CancellationToken cancellationToken)
        {
            return await AsyncExecutor.ExecuteSafeAsync(async ct => {
                Result<List<Contract>> getMerchantContractsResult = await this.TransactionProcessorReadModelRepository.GetMerchantContracts(estateId, merchantId, ct);
                if (getMerchantContractsResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getMerchantContractsResult);

                List<Contract> contractModels = getMerchantContractsResult.Data;
                if (contractModels.Any() == false)
                    return new Error(ErrorType.NotFound, $"No contracts for Estate {estateId} and Merchant {merchantId}");

                return Results.Ok(contractModels);
            }, cancellationToken);
        }

        public async Task<Result<List<Merchant>>> GetMerchants(Guid estateId,
                                                       CancellationToken cancellationToken)
        {
            return await AsyncExecutor.ExecuteSafeAsync(async ct => {
                Result<List<Merchant>> getMerchantsResult = await this.TransactionProcessorReadModelRepository.GetMerchants(estateId, ct);
                if (getMerchantsResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getMerchantsResult);
                List<Merchant> merchants = getMerchantsResult.Data;
                if (merchants == null || merchants.Any() == false) {
                    return new Error(ErrorType.NotFound, $"No Merchants found for estate Id {estateId}");
                }

                return Results.Ok(merchants);
            },cancellationToken);
        }

        public async Task<Result<List<Models.Contract.ContractProductTransactionFee>>> GetTransactionFeesForProduct(Guid estateId,
                                                                             Guid merchantId,
                                                                             Guid contractId,
                                                                             Guid productId,
                                                                             CancellationToken cancellationToken)
        {
            Result<ContractAggregate> getContractResult = await this.AggregateService.GetLatest<ContractAggregate>(contractId, cancellationToken);
            if (getContractResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(getContractResult);
            ContractAggregate contract = getContractResult.Data;
            if (contract.IsCreated == false)
            {
                return new Error(ErrorType.NotFound, $"No contract found with Id [{contractId}]");
            }

            List<Product> products = contract.GetProducts();

            Product product = products.SingleOrDefault(p => p.ContractProductId == productId);

            if (product == null)
            {
                return new Error(ErrorType.NotFound, $"No product found with Id [{productId}] on contract Id [{contractId}]");
            }

            return Results.Ok(product.TransactionFees);
        }

        public async Task<Result<Models.Operator.Operator>> GetOperator(Guid estateId, Guid operatorId, CancellationToken cancellationToken)
        {
            Result<OperatorAggregate> getOperatorResult = await this.AggregateService.GetLatest<OperatorAggregate>(operatorId, cancellationToken);
            if (getOperatorResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(getOperatorResult);
            OperatorAggregate operatorAggregate = getOperatorResult.Data;
            if (operatorAggregate.IsCreated == false)
            {
                return new Error(ErrorType.NotFound, $"No operator found with Id [{operatorId}]");
            }

            Models.Operator.Operator @operator = operatorAggregate.GetOperator();

            return Results.Ok(@operator);
        }

        public async Task<Result<List<Models.Operator.Operator>>> GetOperators(Guid estateId, CancellationToken cancellationToken)
        {
            return await AsyncExecutor.ExecuteSafeAsync(async ct => {
                Result<List<Models.Operator.Operator>> getOperatorsResult = await this.TransactionProcessorReadModelRepository.GetOperators(estateId, ct);
                if (getOperatorsResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getOperatorsResult);

                return Results.Ok(getOperatorsResult.Data);
            }, cancellationToken);
        }

        public async Task<Result<SettlementModel>> GetSettlement(Guid estateId,
                                                                 Guid merchantId,
                                                                 Guid settlementId,
                                                                 CancellationToken cancellationToken)
        {
            return await AsyncExecutor.ExecuteSafeAsync(async ct => { return await this.TransactionProcessorReadModelRepository.GetSettlement(estateId, merchantId, settlementId, ct); },
                cancellationToken);
        }

        public async Task<Result<List<SettlementModel>>> GetSettlements(Guid estateId,
                                                                        Guid? merchantId,
                                                                        String startDate,
                                                                        String endDate,
                                                                        CancellationToken cancellationToken)
        {
            return await AsyncExecutor.ExecuteSafeAsync(async ct => {
                Result<List<SettlementModel>> getSettlementsResult = await this.TransactionProcessorReadModelRepository.GetSettlements(estateId, merchantId, startDate, endDate, ct);
                if (getSettlementsResult.IsSuccess() == false) return ResultHelpers.CreateFailure(getSettlementsResult);

                return Results.Ok(getSettlementsResult.Data);
            }, cancellationToken);
        }

        public async Task<Result<PendingSettlementModel>> GetPendingSettlement(Guid estateId,
                                                                               Guid merchantId,
                                                                               DateTimeOffset settlementDate,
                                                                               CancellationToken cancellationToken) {

            Guid aggregateId = IdGenerationService.GenerateSettlementId(estateId, merchantId, settlementDate);

            Result<SettlementAggregate> getSettlementResult = await this.AggregateService.GetLatest<SettlementAggregate>(aggregateId, cancellationToken);
            if (getSettlementResult.IsSuccess() == false)
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

            return Results.Ok(model);
        }

        #endregion
    }
}
