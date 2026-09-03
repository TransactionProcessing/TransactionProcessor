using MessagingService.Client;
using MessagingService.DataTransferObjects;
using Microsoft.Extensions.Configuration;
using Imposter.Abstractions;
using SecurityService.Client;
using Shared.EventStore.Aggregate;
using Shared.General;
using Shared.Logger;
using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services;

public class MerchantStatementDomainServiceTests {

    private readonly IAggregateServiceImposter AggregateService;
    private readonly IStatementBuilderImposter StatementBuilder;
    private readonly IMessagingServiceClientImposter MessagingServiceClient;
    private readonly ISecurityServiceClientImposter SecurityServiceClient;
    private readonly MerchantStatementDomainService DomainService;
    public MerchantStatementDomainServiceTests() {
        StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
        this.AggregateService = new IAggregateServiceImposter();
        this.StatementBuilder = new IStatementBuilderImposter();
        this.MessagingServiceClient = new IMessagingServiceClientImposter();
        this.SecurityServiceClient = new ISecurityServiceClientImposter();
        IAggregateService AggregateServiceResolver() => this.AggregateService.Instance();
        this.DomainService = new MerchantStatementDomainService(AggregateServiceResolver, this.StatementBuilder.Instance(), this.MessagingServiceClient.Instance(), this.SecurityServiceClient.Instance());

        IConfigurationRoot configurationRoot =
            new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(new NullLogger());
    }
    
    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_TransactionAddedToStatement() {

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Save<MerchantStatementForDateAggregate>(Arg<MerchantStatementForDateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_TransactionNotAuthorised_TransactionNotAddedToStatement()
    {

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Save<MerchantStatementForDateAggregate>(Arg<MerchantStatementForDateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionNotAuthorisedToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_TransactionHasNotAmount_TransactionNotAddedToStatement()
    {

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Save<MerchantStatementForDateAggregate>(Arg<MerchantStatementForDateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionWithNoAmountToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_SaveFailed_TransactionNotAddedToStatement()
    {

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Save<MerchantStatementForDateAggregate>(Arg<MerchantStatementForDateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddSettledFeeToStatement_SettledFeeAddedToStatement()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Save<MerchantStatementForDateAggregate>(Arg<MerchantStatementForDateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddSettledFeeToStatement(TestData.Commands.AddSettledFeeToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddSettledFeeToStatement_SaveFailed_SettledFeeNotAddedToStatement()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Save<MerchantStatementForDateAggregate>(Arg<MerchantStatementForDateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.AddSettledFeeToStatement(TestData.Commands.AddSettledFeeToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddDepositToStatement_DepositAddedToStatement()
    {

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Save<MerchantStatementForDateAggregate>(Arg<MerchantStatementForDateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddDepositToStatement(TestData.Commands.AddDepositToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddWithdrawalToStatement_WithdrawalAddedToStatement()
    {

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Save<MerchantStatementForDateAggregate>(Arg<MerchantStatementForDateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.AddWithdrawalToStatement(TestData.Commands.AddWithdrawalToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_RecordActivityDateOnMerchantStatement_SaveFailed_ActivityDateNotRecorded() {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementAggregate));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
        Result result = await this.DomainService.RecordActivityDateOnMerchantStatement(TestData.Commands.RecordActivityDateOnMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_RecordActivityDateOnMerchantStatement_ActivityDateRecorded()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementAggregate));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.RecordActivityDateOnMerchantStatement(TestData.Commands.RecordActivityDateOnMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_GenerateStatement_StatementIsGenerated()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementAggregateWithActivityDates()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        Result result = await this.DomainService.GenerateStatement(TestData.Commands.GenerateMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_GenerateStatement_GetStatementForDateFailed_StatementIsNotGenerated()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementAggregateWithActivityDates()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

        Result result = await this.DomainService.GenerateStatement(TestData.Commands.GenerateMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_GenerateStatement_SaveFailed_StatementIsNotGenerated()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementAggregateWithActivityDates()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        Result result = await this.DomainService.GenerateStatement(TestData.Commands.GenerateMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_StatementIsBuilt()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.StatementBuilder.GetStatementHtml(Arg<MerchantStatementAggregate>.Any(), Arg<Merchant>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync("<html></html>");

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_GetMerchantFailed_StatementIsNotBuilt()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

        this.StatementBuilder.GetStatementHtml(Arg<MerchantStatementAggregate>.Any(), Arg<Merchant>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync("<html></html>");

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_MerchantNotCreated_StatementIsNotBuilt()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantAggregate()));

        this.StatementBuilder.GetStatementHtml(Arg<MerchantStatementAggregate>.Any(), Arg<Merchant>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync("<html></html>");

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_SaveFailed_StatementIsNotBuilt()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantStatementForDateAggregateWithTransactionAndFee()));

        this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.StatementBuilder.GetStatementHtml(Arg<MerchantStatementAggregate>.Any(), Arg<Merchant>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync("<html></html>");

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_StatementIsEmailed()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.BuiltMerchantStatementAggregate()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.MessagingServiceClient.SendEmail(Arg<String>.Any(), Arg<SendEmailRequest>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));

        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_MerchantNotFound_StatementIsNotEmailed()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.BuiltMerchantStatementAggregate()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

        this.MessagingServiceClient.SendEmail(Arg<String>.Any(), Arg<SendEmailRequest>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));

        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_GetTokenFailed_StatementIsNotEmailed()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.BuiltMerchantStatementAggregate()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.MessagingServiceClient.SendEmail(Arg<String>.Any(), Arg<SendEmailRequest>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_GetStatementForDateFailed_TransactionNotAddedToStatement()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddSettledFeeToStatement_GetStatementForDateFailed_SettledFeeNotAddedToStatement()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.AddSettledFeeToStatement(TestData.Commands.AddSettledFeeToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddDepositToStatement_SaveFailed_DepositNotAddedToStatement()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Save<MerchantStatementForDateAggregate>(Arg<MerchantStatementForDateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.AddDepositToStatement(TestData.Commands.AddDepositToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddDepositToStatement_GetStatementForDateFailed_DepositNotAddedToStatement()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.AddDepositToStatement(TestData.Commands.AddDepositToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddWithdrawalToStatement_SaveFailed_WithdrawalNotAddedToStatement()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EmptyMerchantStatementForDateAggregate));
        this.AggregateService.Save<MerchantStatementForDateAggregate>(Arg<MerchantStatementForDateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.AddWithdrawalToStatement(TestData.Commands.AddWithdrawalToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddWithdrawalToStatement_GetStatementForDateFailed_WithdrawalNotAddedToStatement()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.AddWithdrawalToStatement(TestData.Commands.AddWithdrawalToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_RecordActivityDateOnMerchantStatement_GetStatementFailed_ActivityDateNotRecorded()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.RecordActivityDateOnMerchantStatement(TestData.Commands.RecordActivityDateOnMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_GenerateStatement_GetStatementFailed_StatementIsNotGenerated()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.GenerateStatement(TestData.Commands.GenerateMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_GetStatementFailed_StatementIsNotBuilt()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_GetStatementHtmlFailed_StatementIsNotBuilt()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.GeneratedMerchantStatementAggregate()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.StatementBuilder.GetStatementHtml(Arg<MerchantStatementAggregate>.Any(), Arg<Merchant>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_GetStatementFailed_StatementIsNotEmailed()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());
        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_SaveFailed_StatementIsNotEmailed()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.BuiltMerchantStatementAggregate()));
        this.AggregateService.Save<MerchantStatementAggregate>(Arg<MerchantStatementAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

        this.AggregateService.Get<MerchantAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.MerchantAggregateWithEverything(SettlementSchedule.Immediate)));

        this.MessagingServiceClient.SendEmail(Arg<String>.Any(), Arg<SendEmailRequest>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetTokenResponse()));

        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddTransactionToStatement_ExceptionThrown_ResultIsFailed()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception("Test exception"));
        Result result = await this.DomainService.AddTransactionToStatement(TestData.Commands.AddTransactionToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddSettledFeeToStatement_ExceptionThrown_ResultIsFailed()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception("Test exception"));
        Result result = await this.DomainService.AddSettledFeeToStatement(TestData.Commands.AddSettledFeeToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddDepositToStatement_ExceptionThrown_ResultIsFailed()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception("Test exception"));
        Result result = await this.DomainService.AddDepositToStatement(TestData.Commands.AddDepositToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_AddWithdrawalToStatement_ExceptionThrown_ResultIsFailed()
    {
        this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception("Test exception"));
        Result result = await this.DomainService.AddWithdrawalToStatement(TestData.Commands.AddWithdrawalToMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_RecordActivityDateOnMerchantStatement_ExceptionThrown_ResultIsFailed()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception("Test exception"));
        Result result = await this.DomainService.RecordActivityDateOnMerchantStatement(TestData.Commands.RecordActivityDateOnMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_GenerateStatement_ExceptionThrown_ResultIsFailed()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception("Test exception"));
        Result result = await this.DomainService.GenerateStatement(TestData.Commands.GenerateMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_BuildStatement_ExceptionThrown_ResultIsFailed()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception("Test exception"));
        Result result = await this.DomainService.BuildStatement(TestData.Commands.BuildMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainService_EmailStatement_ExceptionThrown_ResultIsFailed()
    {
        this.AggregateService.GetLatest<MerchantStatementAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception("Test exception"));
        Result result = await this.DomainService.EmailStatement(TestData.Commands.EmailMerchantStatementCommand, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }


}
