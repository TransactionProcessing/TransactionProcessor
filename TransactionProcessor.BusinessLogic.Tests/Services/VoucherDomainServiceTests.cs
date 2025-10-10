using SimpleResults;
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
    using Moq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EntityFramework;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Testing;
    using Xunit;

    public class VoucherDomainServiceTests
    {
        #region Methods

        private Mock<IAggregateService> AggregateService;
        private VoucherDomainService VoucherDomainService;
        private readonly EstateManagementContext Context;
        private readonly Mock<IDbContextResolver<EstateManagementContext>> DbContextFactory;

        public VoucherDomainServiceTests() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.AggregateService  = new Mock<IAggregateService>();
            IAggregateService AggregateServiceResolver() => this.AggregateService.Object;
            this.DbContextFactory = new Mock<IDbContextResolver<EstateManagementContext>>();
            this.Context = this.GetContext(Guid.NewGuid().ToString("N"));
            var services = new ServiceCollection();
            services.AddTransient<EstateManagementContext>(_ => this.Context);
            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();
            this.DbContextFactory.Setup(d => d.Resolve(It.IsAny<String>(), It.IsAny<String>())).Returns(new ResolvedDbContext<EstateManagementContext>(scope));
            this.VoucherDomainService = new VoucherDomainService(AggregateServiceResolver, DbContextFactory.Object);
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_EstateWithNoOperators_ErrorThrown() {
            
            this.AggregateService.Setup(v => v.GetLatest<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new VoucherAggregate()));
            this.AggregateService.Setup(f => f.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.CreatedEstateAggregate());
            var result = await this.VoucherDomainService.IssueVoucher(TestData.IssueVoucherCommand,
                                                                                 CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        //[Fact]
        //public async Task VoucherDomainService_IssueVoucher_EstateWithNullOperators_ErrorThrown() {
        //    IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        //    ConfigurationReader.Initialise(configurationRoot);

        //    Logger.Initialise(NullLogger.Instance);

        //    Mock<IIntermediateEstateClient> estateClient = new Mock<IIntermediateEstateClient>();
        //    Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
        //    Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
        //    voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VoucherAggregate());
        //    securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
        //    estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //                .ReturnsAsync(TestData.GetEstateResponseWithNullOperators);

        //    EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));

        //    var dbContextFactory = this.GetMockDbContextFactory();
        //    dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

        //    VoucherDomainService domainService = new VoucherDomainService(voucherAggregateRepository.Object,
        //                                                                  securityServiceClient.Object,
        //                                                                  estateClient.Object,
        //                                                                  dbContextFactory.Object);

        //    var result = await domainService.IssueVoucher(TestData.VoucherId,
        //                                                                         TestData.OperatorId,
        //                                                                         TestData.EstateId,
        //                                                                         TestData.TransactionId,
        //                                                                         TestData.IssuedDateTime,
        //                                                                         TestData.Value,
        //                                                                         TestData.RecipientEmail,
        //                                                                         TestData.RecipientMobile,
        //                                                                         CancellationToken.None);
        //    result.IsFailed.ShouldBeTrue();
        //}

        
        [Fact]
        public async Task VoucherDomainService_IssueVoucher_InvalidEstate_ErrorThrown() {
            this.AggregateService.Setup(v => v.GetLatest<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new VoucherAggregate()));
            this.AggregateService.Setup(f => f.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);
            
            Result<IssueVoucherResponse> result = await this.VoucherDomainService.IssueVoucher(TestData.IssueVoucherCommand,
                                                                                 CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }
        
        [Fact]
        public async Task VoucherDomainService_IssueVoucher_OperatorNotSupportedByEstate_ErrorThrown() {
            this.AggregateService.Setup(v => v.GetLatest<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new VoucherAggregate()));
            this.AggregateService.Setup(f => f.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

            Result<IssueVoucherResponse> result = await this.VoucherDomainService.IssueVoucher(TestData.IssueVoucherCommand,
                                                                                 CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_VoucherIssued() {

            this.AggregateService.Setup(v => v.GetLatest<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new VoucherAggregate()));
            this.AggregateService.Setup(v => v.Save(It.IsAny<VoucherAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            this.AggregateService.Setup(f => f.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

            Result<IssueVoucherResponse> result = await this.VoucherDomainService.IssueVoucher(TestData.IssueVoucherCommand,
                                                                                         CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_InvalidEstate_ErrorThrown() {

            AggregateService.Setup(v => v.GetLatest<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            this.AggregateService.Setup(f => f.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Aggregates.EmptyEstateAggregate);

            this.Context.VoucherProjectionStates.Add(new TransactionProcessor.Database.Entities.VoucherProjectionState() {
                                                 VoucherCode = TestData.VoucherCode,
                                                 OperatorIdentifier = TestData.OperatorIdentifier,
                                                 Barcode = TestData.Barcode,
                                                 Timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            });
            await this.Context.SaveChangesAsync();
            
            Result<RedeemVoucherResponse> result = await this.VoucherDomainService.RedeemVoucher(TestData.EstateId,
                                                                                  TestData.VoucherCode,
                                                                                  TestData.RedeemedDateTime,
                                                                                  CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_VoucherRedeemed() {

            this.AggregateService.Setup(v => v.GetLatest<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            this.AggregateService.Setup(v => v.Save(It.IsAny<VoucherAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.AggregateService.Setup(f => f.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            this.Context.VoucherProjectionStates.Add(new TransactionProcessor.Database.Entities.VoucherProjectionState() {
                                                 VoucherCode = TestData.VoucherCode,
                                                 OperatorIdentifier = TestData.OperatorIdentifier,
                                                 Barcode = TestData.Barcode,
                                                 Timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            });
            await this.Context.SaveChangesAsync();
            
            Result<RedeemVoucherResponse> result = await this.VoucherDomainService.RedeemVoucher(TestData.EstateId,
                                                                                            TestData.VoucherCode,
                                                                                            TestData.RedeemedDateTime,
                                                                                            CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_VoucherNotFound_ErrorThrown() {
            this.AggregateService.Setup(v => v.GetLatest<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            this.AggregateService.Setup(f => f.Get<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            Result<RedeemVoucherResponse> result = await this.VoucherDomainService.RedeemVoucher(TestData.EstateId,
                                                                                  TestData.VoucherCode,
                                                                                  TestData.RedeemedDateTime,
                                                                                  CancellationToken.None);
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