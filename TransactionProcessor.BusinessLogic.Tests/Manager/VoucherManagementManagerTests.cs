using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shouldly;
using SimpleResults;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Models;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Manager
{
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Internal;
    using ProjectionEngine.Database.Database;
    using ProjectionEngine.Database.Database.Entities;
    using Shared.EntityFramework;
    using Testing;

    public class VoucherManagementManagerTests
    {
        private EstateManagementContext GetContext(String databaseName) {
            EstateManagementContext context = null;
            DbContextOptionsBuilder<EstateManagementContext> builder = new DbContextOptionsBuilder<EstateManagementContext>().UseInMemoryDatabase(databaseName).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            return new EstateManagementContext(builder.Options);
        }

        public VoucherManagementManagerTests() {
            this.AggregateService = new Mock<IAggregateService>();
            this.DbContextFactory = new Mock<IDbContextResolver<EstateManagementContext>>();
            this.Context = this.GetContext(Guid.NewGuid().ToString("N"));
            var services = new ServiceCollection();
            services.AddTransient<EstateManagementContext>(_ => this.Context);
            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            this.DbContextFactory.Setup(d => d.Resolve(It.IsAny<String>(), It.IsAny<String>())).Returns(new ResolvedDbContext<EstateManagementContext>(scope));
            this.VoucherManagementManager = new VoucherManagementManager(this.AggregateService.Object, this.DbContextFactory.Object);
        }

        private Mock<IAggregateService> AggregateService;
        private Mock<IDbContextResolver<EstateManagementContext>> DbContextFactory;
        private VoucherManagementManager VoucherManagementManager;
        private EstateManagementContext Context;

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByCode_VoucherRetrieved(){
            Byte[] b = new Byte[5];

            await this.Context.VoucherProjectionStates.AddAsync(new VoucherProjectionState
            {
                VoucherId = TestData.VoucherId,
                VoucherCode = TestData.VoucherCode,
                Barcode = TestData.Barcode,
                Timestamp = b
            });
            await this.Context.SaveChangesAsync(CancellationToken.None);
            
            this.AggregateService.Setup(v => v.Get<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));

            Result<Voucher> result = await this.VoucherManagementManager.GetVoucherByCode(TestData.EstateId, TestData.VoucherCode, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            Models.Voucher voucher = result.Data;
            voucher.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByCode_VoucherNotFound_ErrorThrown()
        {
            
            this.AggregateService.Setup(v => v.GetLatest<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

            Should.Throw<NotFoundException>(async () =>
            {
                await this.VoucherManagementManager.GetVoucherByCode(TestData.EstateId, TestData.VoucherCode, CancellationToken.None);
            });
        }
        
        [Fact]
        public async Task VoucherManagementManager_GetVoucherByTransactionId_VoucherRetrieved()
        {
            Byte[] b = new Byte[5];

            await this.Context.VoucherProjectionStates.AddAsync(new VoucherProjectionState
            {
                TransactionId = TestData.TransactionId,
                VoucherId = TestData.VoucherId,
                VoucherCode = TestData.VoucherCode,
                Barcode = TestData.Barcode,
                Timestamp = b
            });
            await this.Context.SaveChangesAsync(CancellationToken.None);

            this.AggregateService.Setup(v => v.Get<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            
            var result = await VoucherManagementManager.GetVoucherByTransactionId(TestData.EstateId, TestData.TransactionId, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            Models.Voucher voucher = result.Data;
            voucher.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByTransactionId_VoucherNotFound_ErrorThrown()
        {
            this.AggregateService.Setup(v => v.GetLatest<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

            Should.Throw<NotFoundException>(async () =>
            {
                await this.VoucherManagementManager.GetVoucherByTransactionId(TestData.EstateId, TestData.TransactionId, CancellationToken.None);
            });
        }
    }
}
