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
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
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
        private Mock<Shared.EntityFramework.IDbContextFactory<EstateManagementContext>> GetMockDbContextFactory()
        {
            return new Mock<Shared.EntityFramework.IDbContextFactory<EstateManagementContext>>();
        }

        private async Task<EstateManagementContext> GetContext(String databaseName, TestDatabaseType databaseType = TestDatabaseType.InMemory)
        {
            EstateManagementContext context = null;
            if (databaseType == TestDatabaseType.InMemory)
            {
                DbContextOptionsBuilder<EstateManagementContext> builder = new DbContextOptionsBuilder<EstateManagementContext>()
                                                                                      .UseInMemoryDatabase(databaseName)
                                                                                      .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                context = new EstateManagementContext(builder.Options);
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

            var context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);
            await context.VoucherProjectionStates.AddAsync(new VoucherProjectionState
            {
                VoucherId = TestData.VoucherId,
                VoucherCode = TestData.VoucherCode,
                Barcode = TestData.Barcode,
                Timestamp = b
            });
            await context.SaveChangesAsync(CancellationToken.None);

            Mock<IDbContextFactory<EstateManagementContext>> dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            Mock<IAggregateService> aggregateService = new();
            aggregateService.Setup(v => v.Get<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));

            VoucherManagementManager manager = new VoucherManagementManager(dbContextFactory.Object, aggregateService.Object);

            var result = await manager.GetVoucherByCode(TestData.EstateId, TestData.VoucherCode, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            Models.Voucher voucher = result.Data;
            voucher.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByCode_VoucherNotFound_ErrorThrown()
        {
            EstateManagementContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);

            await context.SaveChangesAsync(CancellationToken.None);

            Mock<IDbContextFactory<EstateManagementContext>> dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            Mock<IAggregateService> aggregateService = new();
            aggregateService.Setup(v => v.GetLatest<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

            VoucherManagementManager manager = new VoucherManagementManager(dbContextFactory.Object, aggregateService.Object);

            Should.Throw<NotFoundException>(async () =>
            {
                await manager.GetVoucherByCode(TestData.EstateId, TestData.VoucherCode, CancellationToken.None);
            });
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByTransactionId_VoucherRetrieved()
        {
            Byte[] b = new Byte[5];

            EstateManagementContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);
            await context.VoucherProjectionStates.AddAsync(new VoucherProjectionState
            {
                TransactionId = TestData.TransactionId,
                VoucherId = TestData.VoucherId,
                VoucherCode = TestData.VoucherCode,
                Barcode = TestData.Barcode,
                Timestamp = b
            });
            await context.SaveChangesAsync(CancellationToken.None);

            Mock<IDbContextFactory<EstateManagementContext>> dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            Mock<IAggregateService> aggregateService = new();
            aggregateService.Setup(v => v.Get<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));

            VoucherManagementManager manager = new VoucherManagementManager(dbContextFactory.Object, aggregateService.Object);

            var result = await manager.GetVoucherByTransactionId(TestData.EstateId, TestData.TransactionId, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            Models.Voucher voucher = result.Data;
            voucher.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByTransactionId_VoucherNotFound_ErrorThrown()
        {
            EstateManagementContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);

            await context.SaveChangesAsync(CancellationToken.None);

            Mock<IDbContextFactory<EstateManagementContext>> dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            Mock<IAggregateService> aggregateService = new();
            aggregateService.Setup(v => v.GetLatest<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

            VoucherManagementManager manager = new VoucherManagementManager(dbContextFactory.Object, aggregateService.Object);

            Should.Throw<NotFoundException>(async () =>
            {
                await manager.GetVoucherByTransactionId(TestData.EstateId, TestData.TransactionId, CancellationToken.None);
            });
        }
    }
}
