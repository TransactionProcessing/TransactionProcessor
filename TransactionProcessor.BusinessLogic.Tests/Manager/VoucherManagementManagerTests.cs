using EstateReporting.Database;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Manager
{
    using EstateReporting.Database.Entities;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Testing;
    using VoucherAggregate;

    public class VoucherManagementManagerTests
    {
        private Mock<Shared.EntityFramework.IDbContextFactory<EstateReportingGenericContext>> GetMockDbContextFactory()
        {
            return new Mock<Shared.EntityFramework.IDbContextFactory<EstateReportingGenericContext>>();
        }

        private async Task<EstateReportingGenericContext> GetContext(String databaseName, TestDatabaseType databaseType = TestDatabaseType.InMemory)
        {
            EstateReportingGenericContext context = null;
            if (databaseType == TestDatabaseType.InMemory)
            {
                DbContextOptionsBuilder<EstateReportingGenericContext> builder = new DbContextOptionsBuilder<EstateReportingGenericContext>()
                                                                                 .UseInMemoryDatabase(databaseName)
                                                                                 .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                context = new EstateReportingSqlServerContext(builder.Options);
            }
            else
            {
                throw new NotSupportedException($"Database type [{databaseType}] not supported");
            }

            return context;
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByCode_VoucherRetrieved()
        {
            EstateReportingGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);
            await context.Vouchers.AddAsync(new Voucher
            {
                EstateId = TestData.EstateId,
                VoucherId = TestData.VoucherId,
                VoucherCode = TestData.VoucherCode
            });
            await context.SaveChangesAsync(CancellationToken.None);

            var dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetVoucherAggregateWithRecipientMobile);

            VoucherManagementManager manager = new VoucherManagementManager(dbContextFactory.Object, voucherAggregateRepository.Object);

            Models.Voucher voucher = await manager.GetVoucherByCode(TestData.EstateId, TestData.VoucherCode, CancellationToken.None);

            voucher.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByCode_VoucherNotFound_ErrorThrown()
        {
            EstateReportingGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);

            await context.SaveChangesAsync(CancellationToken.None);

            var dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();

            VoucherManagementManager manager = new VoucherManagementManager(dbContextFactory.Object, voucherAggregateRepository.Object);

            Should.Throw<NotFoundException>(async () =>
            {
                await manager.GetVoucherByCode(TestData.EstateId, TestData.VoucherCode, CancellationToken.None);
            });
        }
    }
}
