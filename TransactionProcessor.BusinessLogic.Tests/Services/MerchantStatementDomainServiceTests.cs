using System;
using System.Threading;
using System.Threading.Tasks;
using MessagingService.Client;
using MessagingService.DataTransferObjects;
using Microsoft.Extensions.Configuration;
using Moq;
using SecurityService.Client;
using Shared.EventStore.Aggregate;
using Shared.General;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services;

public class MerchantStatementDomainServiceTests {

    private readonly Mock<IAggregateService> AggregateService;
    private readonly Mock<IStatementBuilder> StatementBuilder;
    private readonly Mock<IMessagingServiceClient> MessagingServiceClient;
    private readonly Mock<ISecurityServiceClient> SecurityServiceClient;
    private readonly MerchantStatementDomainService DomainService;
    public MerchantStatementDomainServiceTests() {
        this.AggregateService = new Mock<IAggregateService>();
        this.StatementBuilder = new Mock<IStatementBuilder>();
        this.MessagingServiceClient = new Mock<IMessagingServiceClient>();
        this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
        IAggregateService AggregateServiceResolver() => this.AggregateService.Object;
        this.DomainService = new MerchantStatementDomainService(AggregateServiceResolver, this.StatementBuilder.Object, this.MessagingServiceClient.Object, this.SecurityServiceClient.Object);

        IConfigurationRoot configurationRoot =
            new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(new NullLogger());
    }
    
    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_TransactionAddedToStatement() {

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionToMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_TransactionNotAuthorised_TransactionNotAddedToStatement()
    {

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionNotAuthorisedToMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_TransactionHasNotAmount_TransactionNotAddedToStatement()
    {

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionWithNoAmountToMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_SaveFailed_TransactionNotAddedToStatement()
    {

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionToMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddSettledFeeToStatement_SettledFeeAddedToStatement()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddSettledFeeToStatement(TestData.Commands.AddSettledFeeToMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddSettledFeeToStatement_SaveFailed_SettledFeeNotAddedToStatement()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);
        Result result = await this.DomainService.AddSettledFeeToStatement(TestData.Commands.AddSettledFeeToMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddDepositToStatement_DepositAddedToStatement()
    {

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddDepositToStatement(TestData.Commands.AddDepositToMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddWithdrawalToStatement_WithdrawalAddedToStatement()
    {

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementForDateAggregate>(It.IsAny<MerchantStatementForDateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddWithdrawalToStatement(TestData.Commands.AddWithdrawalToMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_RecordActivityDateOnMerchantStatement_SaveFailed_ActivityDateNotRecorded() {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
        Result result = await this.DomainService.RecordActivityDateOnMerchantStatement(TestData.Commands.RecordActivityDateOnMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_RecordActivityDateOnMerchantStatement_ActivityDateRecorded()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementAggregate));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);
        Result result = await this.DomainService.RecordActivityDateOnMerchantStatement(TestData.Commands.RecordActivityDateOnMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_GenerateStatement_StatementIsGenerated()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementAggregateWithActivityDates()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        Result result = await this.DomainService.GenerateStatement(TestData.Commands.GenerateMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_GenerateStatement_GetStatementForDateFailed_StatementIsNotGenerated()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementAggregateWithActivityDates()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

        Result result = await this.DomainService.GenerateStatement(TestData.Commands.GenerateMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_GenerateStatement_SaveFailed_StatementIsNotGenerated()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementAggregateWithActivityDates()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        Result result = await this.DomainService.GenerateStatement(TestData.Commands.GenerateMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_StatementIsBuilt()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.StatementBuilder.Setup(s => s.GetStatementHtml(It.IsAny<MerchantStatementAggregate>(), It.IsAny<Merchant>(), It.IsAny<CancellationToken>())).ReturnsAsync("<html></html>");

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_GetMerchantFailed_StatementIsNotBuilt()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

        this.StatementBuilder.Setup(s => s.GetStatementHtml(It.IsAny<MerchantStatementAggregate>(), It.IsAny<Merchant>(), It.IsAny<CancellationToken>())).ReturnsAsync("<html></html>");

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_MerchantNotCreated_StatementIsNotBuilt()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));

        this.StatementBuilder.Setup(s => s.GetStatementHtml(It.IsAny<MerchantStatementAggregate>(), It.IsAny<Merchant>(), It.IsAny<CancellationToken>())).ReturnsAsync("<html></html>");

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_SaveFailed_StatementIsNotBuilt()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementForDateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.StatementBuilder.Setup(s => s.GetStatementHtml(It.IsAny<MerchantStatementAggregate>(), It.IsAny<Merchant>(), It.IsAny<CancellationToken>())).ReturnsAsync("<html></html>");

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_StatementIsEmailed()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.BuiltMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(), It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.SecurityServiceClient.Setup(m => m.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_MerchantNotFound_StatementIsNotEmailed()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.BuiltMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

        this.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(), It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.SecurityServiceClient.Setup(m => m.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_GetTokenFailed_StatementIsNotEmailed()
    {
        this.AggregateService.Setup(a => a.GetLatest<MerchantStatementAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.BuiltMerchantStatementAggregate()));
        this.AggregateService.Setup(a => a.Save<MerchantStatementAggregate>(It.IsAny<MerchantStatementAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.AggregateService.Setup(a => a.Get<MerchantAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.MessagingServiceClient.Setup(m => m.SendEmail(It.IsAny<String>(), It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        this.SecurityServiceClient.Setup(m => m.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }


}