using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Database.Contexts;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Services;
    using DomainEventHandlers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using Xunit;

    public class VoucherDomainServiceTests
    {
        #region Methods

        private Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> VoucherAggregateRepository;
        private Mock<IAggregateRepository<EstateAggregate, DomainEvent>> EstateAggregateRepository;
        private Mock<Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext>> DbContextFactory;
        private VoucherDomainService VoucherDomainService;
        public VoucherDomainServiceTests() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            this.VoucherAggregateRepository  = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            this.EstateAggregateRepository = new Mock<IAggregateRepository<EstateAggregate, DomainEvent>>();
            this.DbContextFactory = new Mock<Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext>>();
            this.VoucherDomainService = new VoucherDomainService(VoucherAggregateRepository.Object,
                DbContextFactory.Object,
                EstateAggregateRepository.Object);
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_EstateWithNoOperators_ErrorThrown() {
            
            VoucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VoucherAggregate());
            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));
            DbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);
            EstateAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            var result = await this.VoucherDomainService.IssueVoucher(TestData.VoucherId,
                                                                                 TestData.OperatorId,
                                                                                 TestData.EstateId,
                                                                                 TestData.TransactionId,
                                                                                 TestData.IssuedDateTime,
                                                                                 TestData.Value,
                                                                                 TestData.RecipientEmail,
                                                                                 TestData.RecipientMobile,
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
            VoucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VoucherAggregate());
            EstateAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));
            DbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            var result = await this.VoucherDomainService.IssueVoucher(TestData.VoucherId,
                                                                                 TestData.OperatorId,
                                                                                 TestData.EstateId,
                                                                                 TestData.TransactionId,
                                                                                 TestData.IssuedDateTime,
                                                                                 TestData.Value,
                                                                                 TestData.RecipientEmail,
                                                                                 TestData.RecipientMobile,
                                                                                 CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_OperatorNotSupportedByEstate_ErrorThrown() {
            VoucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VoucherAggregate());
            EstateAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));
            DbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            var result = await this.VoucherDomainService.IssueVoucher(TestData.VoucherId,
                                                                                 TestData.OperatorId2,
                                                                                 TestData.EstateId,
                                                                                 TestData.TransactionId,
                                                                                 TestData.IssuedDateTime,
                                                                                 TestData.Value,
                                                                                 TestData.RecipientEmail,
                                                                                 TestData.RecipientMobile,
                                                                                 CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_VoucherIssued() {

            VoucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VoucherAggregate());
            VoucherAggregateRepository.Setup(v => v.SaveChanges(It.IsAny<VoucherAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            EstateAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));
            DbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            var result = await this.VoucherDomainService.IssueVoucher(TestData.VoucherId,
                                                                                         TestData.OperatorId,
                                                                                         TestData.EstateId,
                                                                                         TestData.TransactionId,
                                                                                         TestData.IssuedDateTime,
                                                                                         TestData.Value,
                                                                                         TestData.RecipientEmail,
                                                                                         TestData.RecipientMobile,
                                                                                         CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_InvalidEstate_ErrorThrown() {
            
            VoucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            EstateAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));
            context.VoucherProjectionStates.Add(new TransactionProcessor.Database.Entities.VoucherProjectionState() {
                                                 VoucherCode = TestData.VoucherCode,
                                                 OperatorIdentifier = TestData.OperatorIdentifier,
                                                 Barcode = TestData.Barcode,
                                                 Timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            });
            await context.SaveChangesAsync();
            DbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            var result = await this.VoucherDomainService.RedeemVoucher(TestData.EstateId,
                                                                                  TestData.VoucherCode,
                                                                                  TestData.RedeemedDateTime,
                                                                                  CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_VoucherRedeemed() {

            VoucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            VoucherAggregateRepository.Setup(v => v.SaveChanges(It.IsAny<VoucherAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            EstateAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));
            context.VoucherProjectionStates.Add(new TransactionProcessor.Database.Entities.VoucherProjectionState() {
                                                 VoucherCode = TestData.VoucherCode,
                                                 OperatorIdentifier = TestData.OperatorIdentifier,
                                                 Barcode = TestData.Barcode,
                                                 Timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            });
            await context.SaveChangesAsync();
            DbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            var result = await this.VoucherDomainService.RedeemVoucher(TestData.EstateId,
                                                                                            TestData.VoucherCode,
                                                                                            TestData.RedeemedDateTime,
                                                                                            CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_VoucherNotFound_ErrorThrown() {
            VoucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));
            EstateAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));
            DbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            var result = await this.VoucherDomainService.RedeemVoucher(TestData.EstateId,
                                                                                  TestData.VoucherCode,
                                                                                  TestData.RedeemedDateTime,
                                                                                  CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        private async Task<EstateManagementGenericContext> GetContext(String databaseName,
                                                                      TestDatabaseType databaseType = TestDatabaseType.InMemory) {
            EstateManagementGenericContext context = null;
            if (databaseType == TestDatabaseType.InMemory) {
                DbContextOptionsBuilder<EstateManagementGenericContext> builder = new DbContextOptionsBuilder<EstateManagementGenericContext>()
                                                                                  .UseInMemoryDatabase(databaseName)
                                                                                  .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                context = new EstateManagementSqlServerContext(builder.Options);
            }
            else {
                throw new NotSupportedException($"Database type [{databaseType}] not supported");
            }

            return context;
        }


        #endregion
    }
}