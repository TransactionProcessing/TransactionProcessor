using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

using EventHandling;
using MessagingService.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SecurityService.Client;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EntityFramework;
using Shared.EventStore.Aggregate;
using Shared.General;
using Shared.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Testing;
using Xunit;

public enum TestDatabaseType
{
    InMemory = 0,
    SqliteInMemory = 1
}

public class VoucherDomainEventHandlerTests
{
    private readonly Mock<IAggregateService> AggregateService;
    private readonly Mock<IDbContextResolver<EstateManagementContext>> DbContextFactory;
    private readonly EstateManagementContext Context;
    private readonly Mock<ISecurityServiceClient> SecurityServiceClient;
    private readonly Mock<IMessagingServiceClient> MessagingServiceClient;
    private readonly VoucherDomainEventHandler VoucherDomainEventHandler;

    private EstateManagementContext GetContext(String databaseName)
    {
        EstateManagementContext context = null;
        DbContextOptionsBuilder<EstateManagementContext> builder = new DbContextOptionsBuilder<EstateManagementContext>().UseInMemoryDatabase(databaseName).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        return new EstateManagementContext(builder.Options);
    }

    public VoucherDomainEventHandlerTests() {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);

        SecurityServiceClient = new Mock<ISecurityServiceClient>();
        MessagingServiceClient = new Mock<IMessagingServiceClient>();

        DirectoryInfo path = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
        MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { $"{path}/VoucherMessages/VoucherEmail.html", new MockFileData("Transaction Number: [TransactionNumber]") },
            { $"{path}/VoucherMessages/VoucherSMS.txt", new MockFileData("Transaction Number: [TransactionNumber]") }
        });

        this.AggregateService = new Mock<IAggregateService>();
        this.DbContextFactory = new Mock<IDbContextResolver<EstateManagementContext>>();
        this.Context = this.GetContext(Guid.NewGuid().ToString("N"));
        var services = new ServiceCollection();
        services.AddTransient<EstateManagementContext>(_ => this.Context);
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        this.DbContextFactory.Setup(d => d.Resolve(It.IsAny<String>(), It.IsAny<String>())).Returns(new ResolvedDbContext<EstateManagementContext>(scope));

        VoucherDomainEventHandler = new VoucherDomainEventHandler(this.SecurityServiceClient.Object,
                                                                                            this.AggregateService.Object,
                                                                                            this.DbContextFactory.Object,
                                                                                            this.MessagingServiceClient.Object,
                                                                                            fileSystem);

    }

    [Fact]
    public async Task VoucherDomainEventHandler_VoucherIssuedEvent_WithEmailAddress_IsHandled()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.AggregateService.Setup(t => t.Get<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientEmail()));

        this.Context.Transactions.Add(new Database.Entities.Transaction()
        {
                                     TransactionId = TestData.TransactionId,
                                     MerchantId = TestData.MerchantId,
                                     ContractId = TestData.ContractId
                                 });
        this.Context.Contracts.Add(new Contract
                              {
                                  ContractId = TestData.ContractId,
                                  EstateId = TestData.EstateId,
                                  Description = TestData.OperatorIdentifier
                              });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        await this.VoucherDomainEventHandler.Handle(TestData.VoucherIssuedEvent, CancellationToken.None);
    }
    
    [Fact]
    public async Task VoucherDomainEventHandler_VoucherIssuedEvent_WithRecipientMobile_IsHandled()
    {
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.AggregateService.Setup(t => t.Get<VoucherAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));

        this.Context.Transactions.Add(new Database.Entities.Transaction()
                                 {
                                     TransactionId = TestData.TransactionId,
                                     MerchantId = TestData.MerchantId,
                                     ContractId = TestData.ContractId
        });
        this.Context.Contracts.Add(new Contract
                              {
                                  ContractId = TestData.ContractId,
                                  EstateId = TestData.EstateId,
                                  Description = TestData.OperatorIdentifier
                              });
        await this.Context.SaveChangesAsync(CancellationToken.None);

        await VoucherDomainEventHandler.Handle(TestData.VoucherIssuedEvent, CancellationToken.None);
    }
}