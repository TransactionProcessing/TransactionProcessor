namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Services;
    using DomainEventHandlers;
    using EstateManagement.Client;
    using EstateManagement.Database.Contexts;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Moq;
    using SecurityService.Client;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using VoucherAggregate;
    using Xunit;

    public class VoucherDomainServiceTests
    {
        #region Methods

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_EstateWithEmptyOperators_ErrorThrown() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VoucherAggregate());
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithEmptyOperators);

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));

            var dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            VoucherDomainService domainService = new VoucherDomainService(voucherAggregateRepository.Object,
                                                                          securityServiceClient.Object,
                                                                          estateClient.Object,
                                                                          dbContextFactory.Object);

            Should.Throw<NotFoundException>(async () => {
                                                await domainService.IssueVoucher(TestData.VoucherId,
                                                                                 TestData.OperatorId,
                                                                                 TestData.EstateId,
                                                                                 TestData.TransactionId,
                                                                                 TestData.IssuedDateTime,
                                                                                 TestData.Value,
                                                                                 TestData.RecipientEmail,
                                                                                 TestData.RecipientMobile,
                                                                                 CancellationToken.None);
                                            });
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_EstateWithNullOperators_ErrorThrown() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VoucherAggregate());
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithNullOperators);

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));

            var dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            VoucherDomainService domainService = new VoucherDomainService(voucherAggregateRepository.Object,
                                                                          securityServiceClient.Object,
                                                                          estateClient.Object,
                                                                          dbContextFactory.Object);

            Should.Throw<NotFoundException>(async () => {
                                                await domainService.IssueVoucher(TestData.VoucherId,
                                                                                 TestData.OperatorId,
                                                                                 TestData.EstateId,
                                                                                 TestData.TransactionId,
                                                                                 TestData.IssuedDateTime,
                                                                                 TestData.Value,
                                                                                 TestData.RecipientEmail,
                                                                                 TestData.RecipientMobile,
                                                                                 CancellationToken.None);
                                            });
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_InvalidEstate_ErrorThrown() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VoucherAggregate());
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));

            var dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            VoucherDomainService domainService = new VoucherDomainService(voucherAggregateRepository.Object,
                                                                          securityServiceClient.Object,
                                                                          estateClient.Object,
                                                                          dbContextFactory.Object);

            Should.Throw<NotFoundException>(async () => {
                                                await domainService.IssueVoucher(TestData.VoucherId,
                                                                                 TestData.OperatorId,
                                                                                 TestData.EstateId,
                                                                                 TestData.TransactionId,
                                                                                 TestData.IssuedDateTime,
                                                                                 TestData.Value,
                                                                                 TestData.RecipientEmail,
                                                                                 TestData.RecipientMobile,
                                                                                 CancellationToken.None);
                                            });
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_OperatorNotSupportedByEstate_ErrorThrown() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VoucherAggregate());
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator2);

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));

            var dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            VoucherDomainService domainService = new VoucherDomainService(voucherAggregateRepository.Object,
                                                                          securityServiceClient.Object,
                                                                          estateClient.Object,
                                                                          dbContextFactory.Object);

            Should.Throw<NotFoundException>(async () => {
                                                await domainService.IssueVoucher(TestData.VoucherId,
                                                                                 TestData.OperatorId,
                                                                                 TestData.EstateId,
                                                                                 TestData.TransactionId,
                                                                                 TestData.IssuedDateTime,
                                                                                 TestData.Value,
                                                                                 TestData.RecipientEmail,
                                                                                 TestData.RecipientMobile,
                                                                                 CancellationToken.None);
                                            });
        }

        [Fact]
        public async Task VoucherDomainService_IssueVoucher_VoucherIssued() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new VoucherAggregate());
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));

            var dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            VoucherDomainService domainService = new VoucherDomainService(voucherAggregateRepository.Object,
                                                                          securityServiceClient.Object,
                                                                          estateClient.Object,
                                                                          dbContextFactory.Object);

            IssueVoucherResponse issueVoucherResponse = await domainService.IssueVoucher(TestData.VoucherId,
                                                                                         TestData.OperatorId,
                                                                                         TestData.EstateId,
                                                                                         TestData.TransactionId,
                                                                                         TestData.IssuedDateTime,
                                                                                         TestData.Value,
                                                                                         TestData.RecipientEmail,
                                                                                         TestData.RecipientMobile,
                                                                                         CancellationToken.None);

            issueVoucherResponse.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_InvalidEstate_ErrorThrown() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(TestData.GetVoucherAggregateWithRecipientMobile);
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception("Exception", new KeyNotFoundException("Invalid Estate")));

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));
            context.Vouchers.Add(new EstateManagement.Database.Entities.Voucher {
                                                 VoucherCode = TestData.VoucherCode,
                                                 OperatorIdentifier = TestData.OperatorIdentifier
                                             });
            await context.SaveChangesAsync();
            var dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            VoucherDomainService domainService = new VoucherDomainService(voucherAggregateRepository.Object,
                                                                          securityServiceClient.Object,
                                                                          estateClient.Object,
                                                                          dbContextFactory.Object);

            Should.Throw<NotFoundException>(async () => {
                                                await domainService.RedeemVoucher(TestData.EstateId,
                                                                                  TestData.VoucherCode,
                                                                                  TestData.RedeemedDateTime,
                                                                                  CancellationToken.None);
                                            });
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_VoucherIssued() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(TestData.GetVoucherAggregateWithRecipientMobile);
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));
            context.Vouchers.Add(new EstateManagement.Database.Entities.Voucher {
                                                 VoucherCode = TestData.VoucherCode,
                                                 OperatorIdentifier = TestData.OperatorIdentifier
                                             });
            await context.SaveChangesAsync();
            var dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            VoucherDomainService domainService = new VoucherDomainService(voucherAggregateRepository.Object,
                                                                          securityServiceClient.Object,
                                                                          estateClient.Object,
                                                                          dbContextFactory.Object);

            RedeemVoucherResponse redeemVoucherResponse = await domainService.RedeemVoucher(TestData.EstateId,
                                                                                            TestData.VoucherCode,
                                                                                            TestData.RedeemedDateTime,
                                                                                            CancellationToken.None);

            redeemVoucherResponse.ShouldNotBeNull();
        }

        [Fact]
        public async Task VoucherDomainService_RedeemVoucher_VoucherNotFound_ErrorThrown() {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            Logger.Initialise(NullLogger.Instance);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
            voucherAggregateRepository.Setup(v => v.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(TestData.GetVoucherAggregateWithRecipientMobile);
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);
            estateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEstateResponseWithOperator1);

            EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"));
            var dbContextFactory = this.GetMockDbContextFactory();
            dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

            VoucherDomainService domainService = new VoucherDomainService(voucherAggregateRepository.Object,
                                                                          securityServiceClient.Object,
                                                                          estateClient.Object,
                                                                          dbContextFactory.Object);

            Should.Throw<NotFoundException>(async () => {
                                                await domainService.RedeemVoucher(TestData.EstateId,
                                                                                  TestData.VoucherCode,
                                                                                  TestData.RedeemedDateTime,
                                                                                  CancellationToken.None);
                                            });
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

        private Mock<Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext>> GetMockDbContextFactory() {
            return new Mock<Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext>>();
        }

        #endregion
    }
}