using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Imposter.Abstractions;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shouldly;
using SimpleResults;
using System;
using System.Threading;
using System.Threading.Tasks;
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
    using Shared.Serialisation;
    using System.Text.Json;
    using Testing;

    public class VoucherManagementManagerTests
    {
        private EstateManagementContext GetContext(String databaseName) {
            EstateManagementContext context = null;
            DbContextOptionsBuilder<EstateManagementContext> builder = new DbContextOptionsBuilder<EstateManagementContext>().UseInMemoryDatabase(databaseName).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            return new EstateManagementContext(builder.Options);
        }

        public VoucherManagementManagerTests() {
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
            this.AggregateService = new IAggregateServiceImposter();
            this.DbContextFactory = new IDbContextResolverImposter<EstateManagementContext>();
            this.Context = this.GetContext(Guid.NewGuid().ToString("N"));
            var services = new ServiceCollection();
            services.AddTransient<EstateManagementContext>(_ => this.Context);
            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            this.DbContextFactory.Resolve(Arg<String>.Any(), Arg<String>.Any()).Returns(new ResolvedDbContext<EstateManagementContext>(scope));
            this.VoucherManagementManager = new VoucherManagementManager(this.AggregateService.Instance(), this.DbContextFactory.Instance());
        }

        private IAggregateServiceImposter AggregateService;
        private IDbContextResolverImposter<EstateManagementContext> DbContextFactory;
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
            }, TestContext.Current.CancellationToken);
            await this.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
            
            this.AggregateService.Get<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));

            Result<Voucher> result = await this.VoucherManagementManager.GetVoucherByCode(TestData.EstateId, TestData.VoucherCode, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
            Models.Voucher voucher = result.Data;
            voucher.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByCode_VoucherNotFound_ErrorThrown()
        {
            
            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.NotFound());

            Should.Throw<NotFoundException>(async () =>
            {
                await this.VoucherManagementManager.GetVoucherByCode(TestData.EstateId, TestData.VoucherCode, TestContext.Current.CancellationToken);
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
            }, TestContext.Current.CancellationToken);
            await this.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

            this.AggregateService.Get<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            
            var result = await VoucherManagementManager.GetVoucherByTransactionId(TestData.EstateId, TestData.TransactionId, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
            Models.Voucher voucher = result.Data;
            voucher.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherManagementManager_GetVoucherByTransactionId_VoucherNotFound_ErrorThrown()
        {
            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.NotFound());

            Should.Throw<NotFoundException>(async () =>
            {
                await this.VoucherManagementManager.GetVoucherByTransactionId(TestData.EstateId, TestData.TransactionId, TestContext.Current.CancellationToken);
            });
        }
    }
}

