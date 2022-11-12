using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.Models;

namespace TransactionProcessor.BusinessLogic.Manager
{
    using EstateReporting.Database;
    using Microsoft.EntityFrameworkCore;
    using VoucherAggregate;
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
        Task<Voucher> GetVoucherByCode(Guid estateId,
                                       String voucherCode,
                                       CancellationToken cancellationToken);

        Task<Voucher> GetVoucherByTransactionId(Guid estateId,
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
        private readonly Shared.EntityFramework.IDbContextFactory<EstateReportingGenericContext> DbContextFactory;

        /// <summary>
        /// The voucher aggregate repository
        /// </summary>
        private readonly IAggregateRepository<VoucherAggregate, DomainEvent> VoucherAggregateRepository;

        private const String ConnectionStringIdentifier = "EstateReportingReadModel";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VoucherManagementManager"/> class.
        /// </summary>
        /// <param name="dbContextFactory">The database context factory.</param>
        /// <param name="voucherAggregateRepository">The voucher aggregate repository.</param>
        public VoucherManagementManager(Shared.EntityFramework.IDbContextFactory<EstateReportingGenericContext> dbContextFactory,
                                        IAggregateRepository<VoucherAggregate, DomainEvent> voucherAggregateRepository)
        {
            this.DbContextFactory = dbContextFactory;
            this.VoucherAggregateRepository = voucherAggregateRepository;
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
        public async Task<Voucher> GetVoucherByCode(Guid estateId,
                                                    String voucherCode,
                                                    CancellationToken cancellationToken)
        {
            EstateReportingGenericContext context = await this.DbContextFactory.GetContext(estateId,VoucherManagementManager.ConnectionStringIdentifier, cancellationToken);

            EstateReporting.Database.Entities.Voucher voucher = await context.Vouchers.SingleOrDefaultAsync(v => v.VoucherCode == voucherCode, cancellationToken);

            if (voucher == null)
            {
                throw new NotFoundException($"Voucher not found with Voucher Code [{voucherCode}]");
            }

            // Get the aggregate
            VoucherAggregate voucherAggregate = await this.VoucherAggregateRepository.GetLatestVersion(voucher.VoucherId, cancellationToken);

            return voucherAggregate.GetVoucher();
        }

        public async Task<Voucher> GetVoucherByTransactionId(Guid estateId,
                                                    Guid transactionId,
                                                    CancellationToken cancellationToken)
        {
            EstateReportingGenericContext context = await this.DbContextFactory.GetContext(estateId, ConnectionStringIdentifier, cancellationToken);

            EstateReporting.Database.Entities.Voucher voucher = await context.Vouchers.SingleOrDefaultAsync(v => v.TransactionId == transactionId, cancellationToken);

            if (voucher == null)
            {
                throw new NotFoundException($"Voucher not found with Transaction Id [{transactionId}]");
            }

            // Get the aggregate
            VoucherAggregate voucherAggregate = await this.VoucherAggregateRepository.GetLatestVersion(voucher.VoucherId, cancellationToken);

            return voucherAggregate.GetVoucher();
        }

        #endregion
    }
}
