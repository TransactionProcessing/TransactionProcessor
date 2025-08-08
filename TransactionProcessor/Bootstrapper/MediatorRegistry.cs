using System;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models.Settlement;
using TransactionProcessor.ProjectionEngine.Models;

namespace TransactionProcessor.Bootstrapper
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using BusinessLogic.RequestHandlers;
    using BusinessLogic.Requests;
    using Lamar;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Models;
    using TransactionProcessor.ProjectionEngine.State;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    [ExcludeFromCodeCoverage]
    public class MediatorRegistry : ServiceRegistry
    {
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
            this.RegisterMerchantStatementRequestHandler();
            this.RegisterMerchantBalanceRequestHandler();
        }

        private void RegisterMerchantBalanceRequestHandler()
        {
            this.AddSingleton<IRequestHandler<MerchantBalanceCommands.RecordDepositCommand, Result>, MerchantBalanceRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantBalanceCommands.RecordWithdrawalCommand, Result>, MerchantBalanceRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantBalanceCommands.RecordAuthorisedSaleCommand, Result>, MerchantBalanceRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantBalanceCommands.RecordDeclinedSaleCommand, Result>, MerchantBalanceRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantBalanceCommands.RecordSettledFeeCommand, Result>, MerchantBalanceRequestHandler>();
        }

        private void RegisterMerchantStatementRequestHandler() {
            this.AddSingleton<IRequestHandler<MerchantStatementCommands.AddTransactionToMerchantStatementCommand, Result>, MerchantStatementRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantStatementCommands.AddSettledFeeToMerchantStatementCommand, Result>, MerchantStatementRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.GenerateMerchantStatementCommand, Result>, MerchantStatementRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantStatementCommands.BuildMerchantStatementCommand, Result>, MerchantStatementRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantStatementCommands.EmailMerchantStatementCommand, Result>, MerchantStatementRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantStatementCommands.RecordActivityDateOnMerchantStatementCommand, Result>, MerchantStatementRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantStatementCommands.AddDepositToMerchantStatementCommand, Result>, MerchantStatementRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantStatementCommands.AddWithdrawalToMerchantStatementCommand, Result>, MerchantStatementRequestHandler>();
        }
        
        private void RegisterMerchantRequestHandler() {
            this.AddSingleton<IRequestHandler<MerchantCommands.CreateMerchantCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.AssignOperatorToMerchantCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.CreateMerchantUserCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.AddMerchantDeviceCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.MakeMerchantDepositCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.MakeMerchantWithdrawalCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.SwapMerchantDeviceCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.AddMerchantContractCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.UpdateMerchantCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.AddMerchantAddressCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.UpdateMerchantAddressCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.AddMerchantContactCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.UpdateMerchantContactCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.RemoveOperatorFromMerchantCommand, Result>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantCommands.RemoveMerchantContractCommand, Result>, MerchantRequestHandler>();

            this.AddSingleton<IRequestHandler<MerchantQueries.GetMerchantQuery, Result<Models.Merchant.Merchant>>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantQueries.GetMerchantContractsQuery, Result<List<Models.Contract.Contract>>>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantQueries.GetMerchantsQuery, Result<List<Models.Merchant.Merchant>>>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantQueries.GetTransactionFeesForProductQuery, Result<List<Models.Contract.ContractProductTransactionFee>>>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantQueries.GetMerchantLiveBalanceQuery, Result<Decimal>>, MerchantRequestHandler>();
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
            this.AddSingleton<IRequestHandler<SettlementQueries.GetSettlementQuery, Result<SettlementModel>>, SettlementRequestHandler>();
            this.AddSingleton<IRequestHandler<SettlementQueries.GetSettlementsQuery, Result<List<SettlementModel>>>, SettlementRequestHandler>();
        }

        private void RegisterTransactionRequestHandler() {
            this.AddSingleton<IRequestHandler<TransactionCommands.ProcessLogonTransactionCommand, Result<ProcessLogonTransactionResponse>>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.ProcessSaleTransactionCommand, Result<ProcessSaleTransactionResponse>>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.ProcessReconciliationCommand, Result<ProcessReconciliationTransactionResponse>>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.ResendTransactionReceiptCommand, Result>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.AddSettledMerchantFeeCommand, Result>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.CalculateFeesForTransactionCommand, Result>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.SendCustomerEmailReceiptCommand, Result>, TransactionRequestHandler>();
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