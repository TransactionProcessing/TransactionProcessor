using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shared.Results;
using SimpleResults;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;

namespace TransactionProcessor.BusinessLogic.Manager
{
    using Microsoft.EntityFrameworkCore;
    using ProjectionEngine.Database.Database;
    using ProjectionEngine.Database.Database.Entities;
    using Shared.EntityFramework;
    using Voucher = Models.Voucher;

    public interface IVoucherManagementManager
    {
        #region Methods

        /// <summary>
        /// Gets the voucher by code.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="voucherCode">The voucher code.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Result<Voucher>> GetVoucherByCode(Guid estateId,
                                               String voucherCode,
                                               CancellationToken cancellationToken);

        Task<Result<Voucher>> GetVoucherByTransactionId(Guid estateId,
                                                        Guid transactionId,
                                                        CancellationToken cancellationToken);

        #endregion
    }

    public class VoucherManagementManager : IVoucherManagementManager
    {
        private readonly IDbContextResolver<EstateManagementContext> Resolver;
        private static readonly String EstateManagementDatabaseName = "TransactionProcessorReadModel";
        private readonly IAggregateService AggregateService;
        
        public VoucherManagementManager(IAggregateService aggregateService,
                                        IDbContextResolver<EstateManagementContext> resolver)
        {
            this.AggregateService = aggregateService;
            this.Resolver = resolver;
        }

        #region Methods

        /// <summary>
        /// Gets the voucher by code.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="voucherCode">The voucher code.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">Voucher not found with Voucher Code [{voucherCode}]</exception>
        public async Task<Result<Voucher>> GetVoucherByCode(Guid estateId,
                                                            String voucherCode,
                                                            CancellationToken cancellationToken)
        {
            using ResolvedDbContext<EstateManagementContext>? resolvedContext = this.Resolver.Resolve(EstateManagementDatabaseName, estateId.ToString());
            await using EstateManagementContext context = resolvedContext.Context;

            VoucherProjectionState voucher = await context.VoucherProjectionStates.SingleOrDefaultAsync(v => v.VoucherCode == voucherCode, cancellationToken);

            if (voucher == null)
            {
                throw new NotFoundException($"Voucher not found with Voucher Code [{voucherCode}]");
            }

            // Get the aggregate
            Result<VoucherAggregate> result= await this.AggregateService.Get<VoucherAggregate>(voucher.VoucherId, cancellationToken);

            if (result.IsFailed) {
                return ResultHelpers.CreateFailure(result);
            }

            return result.Data.GetVoucher();
        }

        public async Task<Result<Voucher>> GetVoucherByTransactionId(Guid estateId,
                                                                     Guid transactionId,
                                                                     CancellationToken cancellationToken)
        {
            using ResolvedDbContext<EstateManagementContext>? resolvedContext = this.Resolver.Resolve(EstateManagementDatabaseName, estateId.ToString());
            await using EstateManagementContext context = resolvedContext.Context;
            VoucherProjectionState voucher = await context.VoucherProjectionStates.SingleOrDefaultAsync(v => v.TransactionId == transactionId, cancellationToken);

            if (voucher == null)
            {
                throw new NotFoundException($"Voucher not found with Transaction Id [{transactionId}]");
            }

            // Get the aggregate
            Result<VoucherAggregate> result = await this.AggregateService.Get<VoucherAggregate>(voucher.VoucherId, cancellationToken);

            if (result.IsFailed)
            {
                return ResultHelpers.CreateFailure(result);
            }

            return result.Data.GetVoucher();
        }

        #endregion
    }
}
