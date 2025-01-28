using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EventHandling;
using MessagingService.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using SecurityService.Client;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using Shared.Logger;
using Testing;
using Xunit;

public enum TestDatabaseType
{
    InMemory = 0,
    SqliteInMemory = 1
}

public class VoucherDomainEventHandlerTests
{
    private Mock<Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext>> GetMockDbContextFactory()
    {
        return new Mock<Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext>>();
    }

    private async Task<EstateManagementGenericContext> GetContext(String databaseName, TestDatabaseType databaseType = TestDatabaseType.InMemory)
    {
        EstateManagementGenericContext context = null;
        if (databaseType == TestDatabaseType.InMemory)
        {
            DbContextOptionsBuilder<EstateManagementGenericContext> builder = new DbContextOptionsBuilder<EstateManagementGenericContext>()
                                                                              .UseInMemoryDatabase(databaseName)
                                                                              .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            context = new EstateManagementSqlServerContext(builder.Options);
        }
        else
        {
            throw new NotSupportedException($"Database type [{databaseType}] not supported");
        }

        return context;
    }

    [Fact]
    public async Task VoucherDomainEventHandler_VoucherIssuedEvent_WithEmailAddress_IsHandled()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);

        Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
        securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
        voucherAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientEmail()));

        EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);
        context.Transactions.Add(new Database.Entities.Transaction()
        {
                                     TransactionId = TestData.TransactionId,
                                     MerchantId = TestData.MerchantId,
                                     ContractId = TestData.ContractId
                                 });
        context.Contracts.Add(new Contract
                              {
                                  ContractId = TestData.ContractId,
                                  EstateId = TestData.EstateId,
                                  Description = TestData.OperatorIdentifier
                              });
        await context.SaveChangesAsync(CancellationToken.None);

        var dbContextFactory = this.GetMockDbContextFactory();
        dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(),It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

        Mock<IMessagingServiceClient> messagingServiceClient = new Mock<IMessagingServiceClient>();

        DirectoryInfo path = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
        MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                                                       {
                                                           { $"{path}/VoucherMessages/VoucherEmail.html", new MockFileData("Transaction Number: [TransactionNumber]") }
                                                       });

        VoucherDomainEventHandler voucherDomainEventHandler = new VoucherDomainEventHandler(securityServiceClient.Object,
                                                                                            voucherAggregateRepository.Object,
                                                                                            dbContextFactory.Object,
                                                                                            messagingServiceClient.Object,
                                                                                            fileSystem);

        await voucherDomainEventHandler.Handle(TestData.VoucherIssuedEvent, CancellationToken.None);
    }

    [Fact]
    public async Task VoucherDomainEventHandler_VoucherIssuedEvent_WithRecipientMobile_IsHandled()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);

        Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
        securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
        voucherAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(Result.Success(TestData.GetVoucherAggregateWithRecipientMobile()));

        EstateManagementGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);
        context.Transactions.Add(new Database.Entities.Transaction()
                                 {
                                     TransactionId = TestData.TransactionId,
                                     MerchantId = TestData.MerchantId,
                                     ContractId = TestData.ContractId
        });
        context.Contracts.Add(new Contract
                              {
                                  ContractId = TestData.ContractId,
                                  EstateId = TestData.EstateId,
                                  Description = TestData.OperatorIdentifier
                              });
        await context.SaveChangesAsync(CancellationToken.None);

        Mock<Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext>> dbContextFactory = this.GetMockDbContextFactory();
        dbContextFactory.Setup(d => d.GetContext(It.IsAny<Guid>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);

        Mock<IMessagingServiceClient> messagingServiceClient = new Mock<IMessagingServiceClient>();

        DirectoryInfo path = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
        MockFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                                                       {
                                                           { $"{path}/VoucherMessages/VoucherSMS.txt", new MockFileData("Transaction Number: [TransactionNumber]") }
                                                       });

        VoucherDomainEventHandler voucherDomainEventHandler = new VoucherDomainEventHandler(securityServiceClient.Object,
                                                                                            voucherAggregateRepository.Object,
                                                                                            dbContextFactory.Object,
                                                                                            messagingServiceClient.Object,
                                                                                            fileSystem);

        await voucherDomainEventHandler.Handle(TestData.VoucherIssuedEvent, CancellationToken.None);
    }
}