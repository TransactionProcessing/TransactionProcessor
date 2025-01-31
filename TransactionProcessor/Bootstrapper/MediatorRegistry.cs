using System;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.DataTransferObjects;
using TransactionProcessor.ProjectionEngine.Models;

namespace TransactionProcessor.Bootstrapper
{
    using BusinessLogic.RequestHandlers;
    using BusinessLogic.Requests;
    using Lamar;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Models;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using TransactionProcessor.ProjectionEngine.State;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    [ExcludeFromCodeCoverage]
    public class MediatorRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediatorRegistry"/> class.
        /// </summary>
        public MediatorRegistry()
        {
            this.AddTransient<IMediator, Mediator>();

            // request & notification handlers
            this.RegisterMerchantRequestHandler();
            this.RegisterFloatRequestHandler();
            this.RegisterVoucherManagementRequestHandler();
            this.RegisterTransactionRequestHandler();
            this.RegisterSettlementRequestHandler();
            this.RegisterEstateRequestHandler();
            this.RegisterOperatorRequestHandler();
            this.RegisterContractRequestHandler();
        }

        #endregion

        private void RegisterMerchantRequestHandler() {
            this.AddSingleton<IRequestHandler<MerchantQueries.GetMerchantLiveBalanceQuery, Result<MerchantBalanceProjectionState1>>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantQueries.GetMerchantBalanceQuery, Result<MerchantBalanceState>>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantQueries.GetMerchantBalanceHistoryQuery, Result<List<MerchantBalanceChangedEntry>>>, MerchantRequestHandler>();
        }

        private void RegisterFloatRequestHandler() {
            this.AddSingleton<IRequestHandler<FloatCommands.CreateFloatForContractProductCommand, Result>, FloatRequestHandler>();
            this.AddSingleton<IRequestHandler<FloatCommands.RecordCreditPurchaseForFloatCommand, Result>, FloatRequestHandler>();
            this.AddSingleton<IRequestHandler<FloatActivityCommands.RecordCreditPurchaseCommand, Result>, FloatRequestHandler>();
            this.AddSingleton<IRequestHandler<FloatActivityCommands.RecordTransactionCommand, Result>, FloatRequestHandler>();
        }

        private void RegisterVoucherManagementRequestHandler() {
            this.AddSingleton<IRequestHandler<VoucherCommands.IssueVoucherCommand, Result<IssueVoucherResponse>>, VoucherManagementRequestHandler>();
            this.AddSingleton<IRequestHandler<VoucherCommands.RedeemVoucherCommand, Result<RedeemVoucherResponse>>, VoucherManagementRequestHandler>();
            this.AddSingleton<IRequestHandler<VoucherQueries.GetVoucherByVoucherCodeQuery, Result<Voucher>>, VoucherManagementRequestHandler>();
            this.AddSingleton<IRequestHandler<VoucherQueries.GetVoucherByTransactionIdQuery, Result<Voucher>>, VoucherManagementRequestHandler>();
        }

        private void RegisterEstateRequestHandler() {
            this.AddSingleton<IRequestHandler<EstateCommands.CreateEstateCommand, Result>, EstateRequestHandler>();
            this.AddSingleton<IRequestHandler<EstateCommands.CreateEstateUserCommand, Result>, EstateRequestHandler>();
            this.AddSingleton<IRequestHandler<EstateCommands.AddOperatorToEstateCommand, Result>, EstateRequestHandler>();
            this.AddSingleton<IRequestHandler<EstateCommands.RemoveOperatorFromEstateCommand, Result>, EstateRequestHandler>();
            this.AddSingleton<IRequestHandler<EstateQueries.GetEstateQuery, Result<Models.Estate.Estate>>, EstateRequestHandler>();
            this.AddSingleton<IRequestHandler<EstateQueries.GetEstatesQuery, Result<List<Models.Estate.Estate>>>, EstateRequestHandler>();
        }

        private void RegisterSettlementRequestHandler() {
            this.AddSingleton<IRequestHandler<SettlementCommands.ProcessSettlementCommand, Result<Guid>>, SettlementRequestHandler>();
            this.AddSingleton<IRequestHandler<SettlementCommands.AddMerchantFeePendingSettlementCommand, Result>, SettlementRequestHandler>();
            this.AddSingleton<IRequestHandler<SettlementCommands.AddSettledFeeToSettlementCommand, Result>, SettlementRequestHandler>();
            this.AddSingleton<IRequestHandler<SettlementQueries.GetPendingSettlementQuery, Result<SettlementAggregate>>, SettlementRequestHandler>();
        }

        private void RegisterTransactionRequestHandler() {
            this.AddSingleton<IRequestHandler<TransactionCommands.ProcessLogonTransactionCommand, Result<ProcessLogonTransactionResponse>>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.ProcessSaleTransactionCommand, Result<ProcessSaleTransactionResponse>>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.ProcessReconciliationCommand, Result<ProcessReconciliationTransactionResponse>>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.ResendTransactionReceiptCommand, Result>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.AddSettledMerchantFeeCommand, Result>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.CalculateFeesForTransactionCommand, Result>, TransactionRequestHandler>();
        }

        private void RegisterOperatorRequestHandler()
        {
            this.AddSingleton<IRequestHandler<OperatorCommands.CreateOperatorCommand, Result>, OperatorRequestHandler>();
            this.AddSingleton<IRequestHandler<OperatorCommands.UpdateOperatorCommand, Result>, OperatorRequestHandler>();
            this.AddSingleton<IRequestHandler<OperatorQueries.GetOperatorQuery, Result<Models.Operator.Operator>>, OperatorRequestHandler>();
            this.AddSingleton<IRequestHandler<OperatorQueries.GetOperatorsQuery, Result<List<Models.Operator.Operator>>>, OperatorRequestHandler>();
        }

        private void RegisterContractRequestHandler() {
            this.AddSingleton<IRequestHandler<ContractCommands.CreateContractCommand, Result>, ContractRequestHandler>();
            this.AddSingleton<IRequestHandler<ContractCommands.AddProductToContractCommand, Result>, ContractRequestHandler>();
            this.AddSingleton<IRequestHandler<ContractCommands.DisableTransactionFeeForProductCommand, Result>, ContractRequestHandler>();
            this.AddSingleton<IRequestHandler<ContractCommands.AddTransactionFeeForProductToContractCommand, Result>, ContractRequestHandler>();

            this.AddSingleton<IRequestHandler<ContractQueries.GetContractQuery, Result<Models.Contract.Contract>>, ContractRequestHandler>();
            this.AddSingleton<IRequestHandler<ContractQueries.GetContractsQuery, Result<List<Models.Contract.Contract>>>, ContractRequestHandler>();
        }
    }
}