using Microsoft.EntityFrameworkCore;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

public class FileImportLogEventTests : BaseTest {
    [Fact]
    public async Task AddFileImportLog_FileImportLogIsAdded()
    {
        Result result = await this.Repository.AddFileImportLog(TestData.DomainEvents.ImportLogCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        var fileImportLog = await context.FileImportLogs.SingleOrDefaultAsync(f => f.FileImportLogId == TestData.DomainEvents.ImportLogCreatedEvent.FileImportLogId);
        fileImportLog.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddFileImportLog_EventReplayHandled()
    {
        Result result = await this.Repository.AddFileImportLog(TestData.DomainEvents.ImportLogCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.AddFileImportLog(TestData.DomainEvents.ImportLogCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddFileImportLogFile_FileImportLogIsAdded()
    {
        Result result = await this.Repository.AddFileToImportLog(TestData.DomainEvents.FileAddedToImportLogEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        var fileImportLogFile = await context.FileImportLogFiles.SingleOrDefaultAsync(f => f.FileImportLogId == TestData.DomainEvents.FileAddedToImportLogEvent.FileImportLogId && f.FileId == TestData.DomainEvents.FileAddedToImportLogEvent.FileId);
        fileImportLogFile.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddFileImportLogFile_EventReplayHandled()
    {
        Result result = await this.Repository.AddFileToImportLog(TestData.DomainEvents.FileAddedToImportLogEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.AddFileToImportLog(TestData.DomainEvents.FileAddedToImportLogEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }
}