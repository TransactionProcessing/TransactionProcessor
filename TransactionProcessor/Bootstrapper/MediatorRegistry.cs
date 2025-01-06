using System;
using SimpleResults;
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
    using TransactionProcessor.SettlementAggregates;

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

            this.AddSingleton<IRequestHandler<TransactionCommands.ProcessLogonTransactionCommand, Result<ProcessLogonTransactionResponse>>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.ProcessSaleTransactionCommand, Result<ProcessSaleTransactionResponse>>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.ProcessReconciliationCommand, Result<ProcessReconciliationTransactionResponse>>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.ResendTransactionReceiptCommand, Result>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.AddSettledMerchantFeeCommand, Result>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<TransactionCommands.CalculateFeesForTransactionCommand, Result>, TransactionRequestHandler>();

            this.AddSingleton<IRequestHandler<SettlementCommands.ProcessSettlementCommand, Result<Guid>>, SettlementRequestHandler>();
            this.AddSingleton<IRequestHandler<SettlementCommands.AddMerchantFeePendingSettlementCommand, Result>, SettlementRequestHandler>();
            this.AddSingleton<IRequestHandler<SettlementCommands.AddSettledFeeToSettlementCommand, Result>, SettlementRequestHandler>();
            this.AddSingleton<IRequestHandler<SettlementQueries.GetPendingSettlementQuery, Result<SettlementAggregate>>, SettlementRequestHandler>();

            this.AddSingleton<IRequestHandler<VoucherCommands.IssueVoucherCommand, Result<IssueVoucherResponse>>, VoucherManagementRequestHandler>();
            this.AddSingleton<IRequestHandler<VoucherCommands.RedeemVoucherCommand, Result<RedeemVoucherResponse>>, VoucherManagementRequestHandler>();
            this.AddSingleton<IRequestHandler<VoucherQueries.GetVoucherByVoucherCodeQuery, Result<Voucher>>, VoucherManagementRequestHandler>();
            this.AddSingleton<IRequestHandler<VoucherQueries.GetVoucherByTransactionIdQuery, Result<Voucher>>, VoucherManagementRequestHandler>();

            this.AddSingleton<IRequestHandler<FloatCommands.CreateFloatForContractProductCommand, Result>, FloatRequestHandler>();
            this.AddSingleton<IRequestHandler<FloatCommands.RecordCreditPurchaseForFloatCommand, Result>, FloatRequestHandler>();
            this.AddSingleton<IRequestHandler<FloatActivityCommands.RecordCreditPurchaseCommand, Result>, FloatRequestHandler>();
            this.AddSingleton<IRequestHandler<FloatActivityCommands.RecordTransactionCommand, Result>, FloatRequestHandler>();

            this.AddSingleton<IRequestHandler<MerchantQueries.GetMerchantLiveBalanceQuery, Result<MerchantBalanceProjectionState1>>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantQueries.GetMerchantBalanceQuery, Result<MerchantBalanceState>>, MerchantRequestHandler>();
            this.AddSingleton<IRequestHandler<MerchantQueries.GetMerchantBalanceHistoryQuery, Result<List<MerchantBalanceChangedEntry>>>, MerchantRequestHandler>();
        }

        #endregion
    }
}