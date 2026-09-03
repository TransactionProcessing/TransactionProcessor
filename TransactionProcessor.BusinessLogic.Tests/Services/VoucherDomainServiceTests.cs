using SimpleResults;
using Imposter;
using Imposter.Abstractions;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Models;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using BusinessLogic.Services;
    using DomainEventHandlers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EntityFramework;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using Shared.Serialisation;
    using Shouldly;
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Testing;
    using Xunit;

    public class VoucherDomainServiceTests
    {
        #region Methods

        private IAggregateServiceImposter AggregateService;
        private VoucherDomainService VoucherDomainService;
        private readonly EstateManagementContext Context;
        private readonly IDbContextResolverImposter<EstateManagementContext> DbContextFactory;

        public VoucherDomainServiceTests() {
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.AggregateService  = new IAggregateServiceImposter();
            IAggregateService AggregateServiceResolver() => this.AggregateService.Instance();
            this.DbContextFactory = new IDbContextResolverImposter<EstateManagementContext>();
            this.Context = this.GetContext(Guid.NewGuid().ToString("N"));
            var services = new ServiceCollection();
            services.AddTransient<EstateManagementContext>(_ => this.Context);
            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            this.DbContextFactory.Resolve(Arg<String>.Any(), Arg<String>.Any()).Returns(new ResolvedDbContext<EstateManagementContext>(scope));
            this.VoucherDomainService = new VoucherDomainService(AggregateServiceResolver, DbContextFactory.Instance());
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_EstateWithNoOperators_ErrorThrown() {
            
            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new VoucherAggregate()));
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());
            var result = await this.VoucherDomainService.IssueVoucher(TestData.IssueVoucherCommand,
                                                                                 TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_InvalidEstate_ErrorThrown() {
            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new VoucherAggregate()));
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);
            
            Result<IssueVoucherResponse> result = await this.VoucherDomainService.IssueVoucher(TestData.IssueVoucherCommand,
                                                                                 TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }
        
        [Fact]
        public async Task VoucherDomainService_IssueVoucher_OperatorNotSupportedByEstate_ErrorThrown() {
            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new VoucherAggregate()));
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

            Result<IssueVoucherResponse> result = await this.VoucherDomainService.IssueVoucher(TestData.IssueVoucherCommand,
                                                                                 TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_VoucherIssued() {

            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new VoucherAggregate()));
            this.AggregateService.Save(Arg<VoucherAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

            Result<IssueVoucherResponse> result = await this.VoucherDomainService.IssueVoucher(TestData.IssueVoucherCommand,
                                                                                         TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_InvalidEstate_ErrorThrown() {

            AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                      .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

            this.Context.VoucherProjectionStates.Add(new TransactionProcessor.Database.Entities.VoucherProjectionState() {
                                                 VoucherCode = TestData.VoucherCode,
                                                 OperatorIdentifier = TestData.OperatorIdentifier,
                                                 Barcode = TestData.Barcode,
                                                 Timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            });
            await this.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
            
            Result<RedeemVoucherResponse> result = await this.VoucherDomainService.RedeemVoucher(TestData.EstateId,
                                                                                  TestData.VoucherCode,
                                                                                  TestData.RedeemedDateTime,
                                                                                  TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_VoucherRedeemed() {

            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                      .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            this.AggregateService.Save(Arg<VoucherAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.Context.VoucherProjectionStates.Add(new TransactionProcessor.Database.Entities.VoucherProjectionState() {
                                                 VoucherCode = TestData.VoucherCode,
                                                 OperatorIdentifier = TestData.OperatorIdentifier,
                                                 Barcode = TestData.Barcode,
                                                 Timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            });
            await this.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
            
            Result<RedeemVoucherResponse> result = await this.VoucherDomainService.RedeemVoucher(TestData.EstateId,
                                                                                            TestData.VoucherCode,
                                                                                            TestData.RedeemedDateTime,
                                                                                            TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_VoucherNotFound_ErrorThrown() {
            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                      .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            Result<RedeemVoucherResponse> result = await this.VoucherDomainService.RedeemVoucher(TestData.EstateId,
                                                                                  TestData.VoucherCode,
                                                                                  TestData.RedeemedDateTime,
                                                                                  TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_GetVoucherFailed_ErrorThrown() {
            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

            Result<IssueVoucherResponse> result = await this.VoucherDomainService.IssueVoucher(TestData.IssueVoucherCommand,
                                                                                 TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_SaveFailed_ErrorThrown() {
            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new VoucherAggregate()));
            this.AggregateService.Save(Arg<VoucherAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

            Result<IssueVoucherResponse> result = await this.VoucherDomainService.IssueVoucher(TestData.IssueVoucherCommand,
                                                                                 TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_GetVoucherAggregateFailed_ErrorThrown() {
            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.Context.VoucherProjectionStates.Add(new TransactionProcessor.Database.Entities.VoucherProjectionState() {
                VoucherCode = TestData.VoucherCode,
                OperatorIdentifier = TestData.OperatorIdentifier,
                Barcode = TestData.Barcode,
                Timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            });
            await this.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

            Result<RedeemVoucherResponse> result = await this.VoucherDomainService.RedeemVoucher(TestData.EstateId,
                                                                                  TestData.VoucherCode,
                                                                                  TestData.RedeemedDateTime,
                                                                                  TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_SaveFailed_ErrorThrown() {
            this.AggregateService.GetLatest<VoucherAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                                      .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            this.AggregateService.Save(Arg<VoucherAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
            this.AggregateService.Get<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.Context.VoucherProjectionStates.Add(new TransactionProcessor.Database.Entities.VoucherProjectionState() {
                VoucherCode = TestData.VoucherCode,
                OperatorIdentifier = TestData.OperatorIdentifier,
                Barcode = TestData.Barcode,
                Timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            });
            await this.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

            Result<RedeemVoucherResponse> result = await this.VoucherDomainService.RedeemVoucher(TestData.EstateId,
                                                                                            TestData.VoucherCode,
                                                                                            TestData.RedeemedDateTime,
                                                                                            TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }
        
        private EstateManagementContext GetContext(String databaseName)
        {
            EstateManagementContext context = null;
            DbContextOptionsBuilder<EstateManagementContext> builder = new DbContextOptionsBuilder<EstateManagementContext>().UseInMemoryDatabase(databaseName).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            return new EstateManagementContext(builder.Options);
        }

        #endregion
    }
}
