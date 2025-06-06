using System;
using System.Threading;
using System.Threading.Tasks;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;

namespace TransactionProcessor.BusinessLogic.Manager
{
    using Microsoft.EntityFrameworkCore;
    using ProjectionEngine.Database.Database;
    using ProjectionEngine.Database.Database.Entities;
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
        #region Fields

        /// <summary>
        /// The database context factory
        /// </summary>
        private readonly Shared.EntityFramework.IDbContextFactory<EstateManagementContext> DbContextFactory;

        private readonly IAggregateService AggregateService;

        private const String ConnectionStringIdentifier = "EstateReportingReadModel";

        #endregion

        #region Constructors

        public VoucherManagementManager(Shared.EntityFramework.IDbContextFactory<EstateManagementContext> dbContextFactory,
                                        IAggregateService aggregateService)
        {
            this.DbContextFactory = dbContextFactory;
            this.AggregateService = aggregateService;
        }

        #endregion

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
            EstateManagementContext context = await this.DbContextFactory.GetContext(estateId,VoucherManagementManager.ConnectionStringIdentifier, cancellationToken);

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
            EstateManagementContext context = await this.DbContextFactory.GetContext(estateId, ConnectionStringIdentifier, cancellationToken);
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
