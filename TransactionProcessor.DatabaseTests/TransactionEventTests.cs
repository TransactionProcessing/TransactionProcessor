using Microsoft.EntityFrameworkCore;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Testing;
using System.Collections.Generic;

namespace TransactionProcessor.DatabaseTests;

public class TransactionEventTests : BaseTest {
    public TransactionEventTests(DatabaseTestFixture fixture) : base(fixture) {
    }

    private async Task CreateOperatorAsync()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    private async Task CreateContractAsync()
    {
        await this.CreateOperatorAsync();

        Result result = await this.Repository.AddContract(TestData.DomainEvents.ContractCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    private async Task CreateMerchantAsync()
    {
        Result result = await this.Repository.AddMerchant(TestData.DomainEvents.MerchantCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    private async Task CreateTransactionAsync()
    {
        Result result = await this.Repository.StartTransaction(TestData.DomainEvents.TransactionHasStartedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    private async Task CreateTransactionWithProductDetailsAsync()
    {
        await this.CreateContractAsync();
        await this.CreateTransactionAsync();

        Result result = await this.Repository.AddProductDetailsToTransaction(TestData.DomainEvents.ProductDetailsAddedToTransactionEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task StartTransaction_TransactionIsAdded()
    {
        Result result = await this.Repository.StartTransaction(TestData.DomainEvents.TransactionHasStartedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Transaction? transaction = await context.Transactions.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.TransactionHasStartedEvent.TransactionId, TestContext.Current.CancellationToken);
        transaction.ShouldNotBeNull();
        transaction.MerchantId.ShouldBe(TestData.DomainEvents.TransactionHasStartedEvent.MerchantId);
        transaction.TransactionDate.ShouldBe(TestData.DomainEvents.TransactionHasStartedEvent.TransactionDateTime.Date);
        transaction.TransactionDateTime.ShouldBe(TestData.DomainEvents.TransactionHasStartedEvent.TransactionDateTime);
        transaction.TransactionNumber.ShouldBe(TestData.DomainEvents.TransactionHasStartedEvent.TransactionNumber);
        transaction.TransactionReference.ShouldBe(TestData.DomainEvents.TransactionHasStartedEvent.TransactionReference);
        transaction.TransactionType.ShouldBe(TestData.DomainEvents.TransactionHasStartedEvent.TransactionType);
        transaction.DeviceIdentifier.ShouldBe(TestData.DomainEvents.TransactionHasStartedEvent.DeviceIdentifier);
        transaction.TransactionAmount.ShouldBe(TestData.DomainEvents.TransactionHasStartedEvent.TransactionAmount.GetValueOrDefault());
        transaction.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    public async Task StartTransaction_EventReplayHandled()
    {
        Result result = await this.Repository.StartTransaction(TestData.DomainEvents.TransactionHasStartedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.StartTransaction(TestData.DomainEvents.TransactionHasStartedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task CompleteTransaction_TransactionIsCompleted()
    {
        await this.CreateTransactionAsync();

        Result result = await this.Repository.CompleteTransaction(TestData.DomainEvents.TransactionHasBeenCompletedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Transaction? transaction = await context.Transactions.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.TransactionHasBeenCompletedEvent.TransactionId, TestContext.Current.CancellationToken);
        transaction.ShouldNotBeNull();
        transaction.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public async Task RecordTransactionAdditionalRequestData_RequestDataIsStored()
    {
        Result result = await this.Repository.RecordTransactionAdditionalRequestData(TestData.DomainEvents.AdditionalRequestDataRecordedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        TransactionAdditionalRequestData? requestData = await context.TransactionsAdditionalRequestData.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.AdditionalRequestDataRecordedEvent.TransactionId, TestContext.Current.CancellationToken);
        requestData.ShouldNotBeNull();
        requestData.Amount.ShouldBe("123.45");
        requestData.CustomerAccountNumber.ShouldBe("12345678");
    }

    [Fact]
    public async Task RecordTransactionAdditionalResponseData_ResponseDataIsStored()
    {
        Result result = await this.Repository.RecordTransactionAdditionalResponseData(TestData.DomainEvents.AdditionalResponseDataRecordedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        TransactionAdditionalResponseData? responseData = await context.TransactionsAdditionalResponseData.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.AdditionalResponseDataRecordedEvent.TransactionId, TestContext.Current.CancellationToken);
        responseData.ShouldNotBeNull();
        responseData.TransactionId.ShouldBe(TestData.DomainEvents.AdditionalResponseDataRecordedEvent.TransactionId);
        responseData.TransactionReportingId.ShouldBe(0);
    }

    [Fact]
    public async Task SetTransactionAmount_TransactionAmountIsUpdated()
    {
        await this.CreateTransactionAsync();

        Result result = await this.Repository.SetTransactionAmount(TestData.DomainEvents.AdditionalRequestDataRecordedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Transaction? transaction = await context.Transactions.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.TransactionHasStartedEvent.TransactionId, TestContext.Current.CancellationToken);
        transaction.ShouldNotBeNull();
        transaction.TransactionAmount.ShouldBe(123.45m);
    }

    [Fact]
    public async Task AddProductDetailsToTransaction_TransactionIsUpdated()
    {
        await this.CreateContractAsync();
        await this.CreateTransactionAsync();

        Result result = await this.Repository.AddProductDetailsToTransaction(TestData.DomainEvents.ProductDetailsAddedToTransactionEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Transaction? transaction = await context.Transactions.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.ProductDetailsAddedToTransactionEvent.TransactionId, TestContext.Current.CancellationToken);
        transaction.ShouldNotBeNull();
        transaction.ContractId.ShouldBe(TestData.DomainEvents.ProductDetailsAddedToTransactionEvent.ContractId);
        transaction.ContractProductId.ShouldBe(TestData.DomainEvents.ProductDetailsAddedToTransactionEvent.ProductId);
        transaction.OperatorId.ShouldBe(TestData.OperatorId);
    }

    [Fact]
    public async Task AddSourceDetailsToTransaction_TransactionSourceIsUpdated()
    {
        await this.CreateTransactionAsync();

        Result result = await this.Repository.AddSourceDetailsToTransaction(TestData.DomainEvents.TransactionSourceAddedToTransactionEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Transaction? transaction = await context.Transactions.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.TransactionSourceAddedToTransactionEvent.TransactionId, TestContext.Current.CancellationToken);
        transaction.ShouldNotBeNull();
        transaction.TransactionSource.ShouldBe(TestData.DomainEvents.TransactionSourceAddedToTransactionEvent.TransactionSource);
    }

    [Fact]
    public async Task UpdateTransactionAuthorisation_LocallyAuthorisedTransactionIsUpdated()
    {
        await this.CreateTransactionAsync();

        Result result = await this.Repository.UpdateTransactionAuthorisation(TestData.DomainEvents.TransactionHasBeenLocallyAuthorisedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Transaction? transaction = await context.Transactions.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.TransactionHasBeenLocallyAuthorisedEvent.TransactionId, TestContext.Current.CancellationToken);
        transaction.ShouldNotBeNull();
        transaction.IsAuthorised.ShouldBeTrue();
        transaction.AuthorisationCode.ShouldBe(TestData.AuthorisationCode);
        transaction.ResponseCode.ShouldBe(TestData.ResponseCode.ToCodeString());
        transaction.ResponseMessage.ShouldBe(TestData.ResponseMessage);
    }

    [Fact]
    public async Task UpdateTransactionAuthorisation_LocallyDeclinedTransactionIsUpdated()
    {
        await this.CreateTransactionAsync();

        Result result = await this.Repository.UpdateTransactionAuthorisation(TestData.DomainEvents.TransactionHasBeenLocallyDeclinedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Transaction? transaction = await context.Transactions.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.TransactionHasBeenLocallyDeclinedEvent.TransactionId, TestContext.Current.CancellationToken);
        transaction.ShouldNotBeNull();
        transaction.IsAuthorised.ShouldBeFalse();
        transaction.ResponseCode.ShouldBe(TestData.DeclinedResponseCode.ToCodeString());
        transaction.ResponseMessage.ShouldBe(TestData.DeclinedResponseMessage);
    }

    [Fact]
    public async Task UpdateTransactionAuthorisation_OperatorAuthorisedTransactionIsUpdated()
    {
        await this.CreateTransactionAsync();

        Result result = await this.Repository.UpdateTransactionAuthorisation(TestData.DomainEvents.TransactionAuthorisedByOperatorEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Transaction? transaction = await context.Transactions.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.TransactionAuthorisedByOperatorEvent.TransactionId, TestContext.Current.CancellationToken);
        transaction.ShouldNotBeNull();
        transaction.IsAuthorised.ShouldBeTrue();
        transaction.AuthorisationCode.ShouldBe(TestData.OperatorAuthorisationCode);
        transaction.ResponseCode.ShouldBe(TestData.ResponseCode.ToCodeString());
        transaction.ResponseMessage.ShouldBe(TestData.ResponseMessage);
    }

    [Fact]
    public async Task UpdateTransactionAuthorisation_OperatorDeclinedTransactionIsUpdated()
    {
        await this.CreateTransactionAsync();

        Result result = await this.Repository.UpdateTransactionAuthorisation(TestData.DomainEvents.TransactionDeclinedByOperatorEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Transaction? transaction = await context.Transactions.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.TransactionDeclinedByOperatorEvent.TransactionId, TestContext.Current.CancellationToken);
        transaction.ShouldNotBeNull();
        transaction.IsAuthorised.ShouldBeFalse();
        transaction.ResponseCode.ShouldBe(TestData.DeclinedResponseCode.ToCodeString());
        transaction.ResponseMessage.ShouldBe(TestData.DeclinedResponseMessage);
    }

    [Fact]
    public async Task RecordTransactionTimings_TimingsAreStored()
    {
        Result result = await this.Repository.RecordTransactionTimings(TestData.DomainEvents.TransactionTimingsAddedToTransactionEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        TransactionTimings? timings = await context.TransactionTimings.SingleOrDefaultAsync(t => t.TransactionId == TestData.DomainEvents.TransactionTimingsAddedToTransactionEvent.TransactionId, TestContext.Current.CancellationToken);
        timings.ShouldNotBeNull();
        timings.TransactionStartedDateTime.ShouldBe(TestData.DomainEvents.TransactionTimingsAddedToTransactionEvent.TransactionStartedDateTime);
        timings.OperatorCommunicationsStartedDateTime.ShouldBe(TestData.DomainEvents.TransactionTimingsAddedToTransactionEvent.OperatorCommunicationsStartedEvent);
        timings.OperatorCommunicationsCompletedDateTime.ShouldBe(TestData.DomainEvents.TransactionTimingsAddedToTransactionEvent.OperatorCommunicationsCompletedEvent);
        timings.TransactionCompletedDateTime.ShouldBe(TestData.DomainEvents.TransactionTimingsAddedToTransactionEvent.TransactionCompletedDateTime);
        timings.TotalTransactionInMilliseconds.ShouldBe(300000d);
        timings.OperatorCommunicationsDurationInMilliseconds.ShouldBe(120000d);
        timings.TransactionProcessingDurationInMilliseconds.ShouldBe(180000d);
    }

    [Fact]
    public async Task AddTransactionToStatement_StatementLineIsAdded()
    {
        await this.CreateTransactionWithProductDetailsAsync();

        Result result = await this.Repository.AddTransactionToStatement(TestData.DomainEvents.TransactionAddedToStatementForDateEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        StatementLine? statementLine = await context.StatementLines.SingleOrDefaultAsync(s => s.StatementId == TestData.DomainEvents.TransactionAddedToStatementForDateEvent.MerchantStatementId && s.TransactionId == TestData.DomainEvents.TransactionAddedToStatementForDateEvent.TransactionId, TestContext.Current.CancellationToken);
        statementLine.ShouldNotBeNull();
        statementLine.ActivityDate.ShouldBe(TestData.DomainEvents.TransactionAddedToStatementForDateEvent.TransactionDateTime.Date);
        statementLine.ActivityDateTime.ShouldBe(TestData.DomainEvents.TransactionAddedToStatementForDateEvent.TransactionDateTime);
        statementLine.ActivityDescription.ShouldBe($"{TestData.OperatorName} Transaction");
        statementLine.ActivityType.ShouldBe(1);
        statementLine.OutAmount.ShouldBe(TestData.DomainEvents.TransactionAddedToStatementForDateEvent.TransactionValue);
        statementLine.InAmount.ShouldBe(0);
    }

    [Fact]
    public async Task AddSettledFeeToStatement_StatementLineIsAdded()
    {
        await this.CreateTransactionWithProductDetailsAsync();

        Result result = await this.Repository.AddSettledFeeToStatement(TestData.DomainEvents.SettledFeeAddedToStatementForDateEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        StatementLine? statementLine = await context.StatementLines.SingleOrDefaultAsync(s => s.StatementId == TestData.DomainEvents.SettledFeeAddedToStatementForDateEvent.MerchantStatementId && s.TransactionId == TestData.DomainEvents.SettledFeeAddedToStatementForDateEvent.TransactionId, TestContext.Current.CancellationToken);
        statementLine.ShouldNotBeNull();
        statementLine.ActivityDate.ShouldBe(TestData.DomainEvents.SettledFeeAddedToStatementForDateEvent.SettledDateTime.Date);
        statementLine.ActivityDateTime.ShouldBe(TestData.DomainEvents.SettledFeeAddedToStatementForDateEvent.SettledDateTime);
        statementLine.ActivityDescription.ShouldBe($"{TestData.OperatorName} Transaction Fee");
        statementLine.ActivityType.ShouldBe(2);
        statementLine.InAmount.ShouldBe(TestData.DomainEvents.SettledFeeAddedToStatementForDateEvent.SettledValue);
        statementLine.OutAmount.ShouldBe(0);
    }

    [Fact]
    public async Task UpdateMerchant_TransactionCompletionUpdatesMerchantLastSale()
    {
        await this.CreateMerchantAsync();

        Result result = await this.Repository.UpdateMerchant(TestData.DomainEvents.TransactionHasBeenCompletedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Merchant? merchant = await context.Merchants.SingleOrDefaultAsync(m => m.MerchantId == TestData.DomainEvents.TransactionHasBeenCompletedEvent.MerchantId, TestContext.Current.CancellationToken);
        merchant.ShouldNotBeNull();
        merchant.LastSaleDate.ShouldBe(TestData.DomainEvents.TransactionHasBeenCompletedEvent.CompletedDateTime.Date);
        merchant.LastSaleDateTime.ShouldBe(TestData.DomainEvents.TransactionHasBeenCompletedEvent.CompletedDateTime);
    }
}


