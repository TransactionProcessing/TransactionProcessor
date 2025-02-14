using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Manager
{
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using ProjectionEngine.Database.Database;
    using ProjectionEngine.Database.Database.Entities;
    using Shared.EntityFramework;
    using Testing;

    public class VoucherManagementManagerTests
    {
        private Mock<Shared.EntityFramework.IDbContextFactory<TransactionProcessorGenericContext>> GetMockDbContextFactory()
        {
            return new Mock<Shared.EntityFramework.IDbContextFactory<TransactionProcessorGenericContext>>();
        }

        private async Task<TransactionProcessorGenericContext> GetContext(String databaseName, TestDatabaseType databaseType = TestDatabaseType.InMemory)
        {
            TransactionProcessorGenericContext context = null;
            if (databaseType == TestDatabaseType.InMemory)
            {
                DbContextOptionsBuilder<TransactionProcessorGenericContext> builder = new DbContextOptionsBuilder<TransactionProcessorGenericContext>()
                                                                                      .UseInMemoryDatabase(databaseName)
                                                                                      .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                context = new TransactionProcessorSqlServerContext(builder.Options);
            }
            else
            {
                throw new NotSupportedException($"Database type [{databaseType}] not supported");
            }

            return context;
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByCode_VoucherRetrieved(){
            Byte[] b = new Byte[5];

            TransactionProcessorGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);
            await context.VoucherProjectionState.AddAsync(new VoucherProjectionState
            {
                VoucherId = TestData.VoucherId,
                VoucherCode = TestData.VoucherCode,
                Barcode = TestData.Barcode,
                Timestamp = b
            });
            await context.SaveChangesAsync(CancellationToken.None);

            Mock<IDbContextFactory<TransactionProcessorGenericContext>> dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));

            VoucherManagementManager manager = new VoucherManagementManager(dbContextFactory.Object, voucherAggregateRepository.Object);

            Models.Voucher voucher = await manager.GetVoucherByCode(TestData.EstateId, TestData.VoucherCode, CancellationToken.None);

            voucher.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByCode_VoucherNotFound_ErrorThrown()
        {
            TransactionProcessorGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);

            await context.SaveChangesAsync(CancellationToken.None);

            Mock<IDbContextFactory<TransactionProcessorGenericContext>> dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();

            VoucherManagementManager manager = new VoucherManagementManager(dbContextFactory.Object, voucherAggregateRepository.Object);

            Should.Throw<NotFoundException>(async () =>
            {
                await manager.GetVoucherByCode(TestData.EstateId, TestData.VoucherCode, CancellationToken.None);
            });
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByTransactionId_VoucherRetrieved()
        {
            Byte[] b = new Byte[5];

            TransactionProcessorGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);
            await context.VoucherProjectionState.AddAsync(new VoucherProjectionState
            {
                TransactionId = TestData.TransactionId,
                VoucherId = TestData.VoucherId,
                VoucherCode = TestData.VoucherCode,
                Barcode = TestData.Barcode,
                Timestamp = b
            });
            await context.SaveChangesAsync(CancellationToken.None);

            Mock<IDbContextFactory<TransactionProcessorGenericContext>> dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));

            VoucherManagementManager manager = new VoucherManagementManager(dbContextFactory.Object, voucherAggregateRepository.Object);

            Models.Voucher voucher = await manager.GetVoucherByTransactionId(TestData.EstateId, TestData.TransactionId, CancellationToken.None);

            voucher.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByTransactionId_VoucherNotFound_ErrorThrown()
        {
            TransactionProcessorGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);

            await context.SaveChangesAsync(CancellationToken.None);

            Mock<IDbContextFactory<TransactionProcessorGenericContext>> dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();

            VoucherManagementManager manager = new VoucherManagementManager(dbContextFactory.Object, voucherAggregateRepository.Object);

            Should.Throw<NotFoundException>(async () =>
            {
                await manager.GetVoucherByTransactionId(TestData.EstateId, TestData.TransactionId, CancellationToken.None);
            });
        }
    }
}
