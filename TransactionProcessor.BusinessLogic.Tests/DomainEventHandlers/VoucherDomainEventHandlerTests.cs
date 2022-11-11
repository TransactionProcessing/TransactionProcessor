﻿namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EstateReporting.Database;
using EstateReporting.Database.Entities;
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
using VoucherAggregate;
using Xunit;
using Transaction = Models.Transaction;

public enum TestDatabaseType
{
    InMemory = 0,
    SqliteInMemory = 1
}

public class VoucherDomainEventHandlerTests
{
    private Mock<Shared.EntityFramework.IDbContextFactory<EstateReportingGenericContext>> GetMockDbContextFactory()
    {
        return new Mock<Shared.EntityFramework.IDbContextFactory<EstateReportingGenericContext>>();
    }

    private async Task<EstateReportingGenericContext> GetContext(String databaseName, TestDatabaseType databaseType = TestDatabaseType.InMemory)
    {
        EstateReportingGenericContext context = null;
        if (databaseType == TestDatabaseType.InMemory)
        {
            DbContextOptionsBuilder<EstateReportingGenericContext> builder = new DbContextOptionsBuilder<EstateReportingGenericContext>()
                                                                             .UseInMemoryDatabase(databaseName)
                                                                             .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            context = new EstateReportingSqlServerContext(builder.Options);
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
        securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
        voucherAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(TestData.GetVoucherAggregateWithRecipientEmail);

        EstateReportingGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);
        context.Transactions.Add(new EstateReporting.Database.Entities.Transaction()
        {
                                     TransactionId = TestData.TransactionId,
                                     EstateId = TestData.EstateId,
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
        securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

        Mock<IAggregateRepository<VoucherAggregate, DomainEvent>> voucherAggregateRepository = new Mock<IAggregateRepository<VoucherAggregate, DomainEvent>>();
        voucherAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(TestData.GetVoucherAggregateWithRecipientMobile);

        EstateReportingGenericContext context = await this.GetContext(Guid.NewGuid().ToString("N"), TestDatabaseType.InMemory);
        context.Transactions.Add(new EstateReporting.Database.Entities.Transaction()
                                 {
                                     TransactionId = TestData.TransactionId,
                                     EstateId = TestData.EstateId,
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