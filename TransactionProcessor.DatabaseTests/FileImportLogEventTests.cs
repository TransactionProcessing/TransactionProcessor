using Microsoft.EntityFrameworkCore;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

public class FileImportLogEventTests : BaseTest {
    private async Task CreateFileImportLogAsync()
    {
        Result result = await this.Repository.AddFileImportLog(TestData.DomainEvents.ImportLogCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    private async Task CreateFileAsync()
    {
        await this.CreateFileImportLogAsync();

        Result result = await this.Repository.AddFile(TestData.DomainEvents.FileCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    private async Task CreateFileLineAsync()
    {
        await this.CreateFileAsync();

        Result result = await this.Repository.AddFileLineToFile(TestData.DomainEvents.FileLineAddedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddFileImportLog_FileImportLogIsAdded()
    {
        Result result = await this.Repository.AddFileImportLog(TestData.DomainEvents.ImportLogCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        var fileImportLog = await context.FileImportLogs.SingleOrDefaultAsync(f => f.FileImportLogId == TestData.DomainEvents.ImportLogCreatedEvent.FileImportLogId);
        fileImportLog.ShouldNotBeNull();
        fileImportLog.EstateId.ShouldBe(TestData.DomainEvents.ImportLogCreatedEvent.EstateId);
        fileImportLog.ImportLogDate.ShouldBe(TestData.DomainEvents.ImportLogCreatedEvent.ImportLogDateTime.Date);
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
        fileImportLogFile.FilePath.ShouldBe(TestData.DomainEvents.FileAddedToImportLogEvent.FilePath);
        fileImportLogFile.OriginalFileName.ShouldBe(TestData.DomainEvents.FileAddedToImportLogEvent.OriginalFileName);
    }

    [Fact]
    public async Task AddFileImportLogFile_EventReplayHandled()
    {
        Result result = await this.Repository.AddFileToImportLog(TestData.DomainEvents.FileAddedToImportLogEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.AddFileToImportLog(TestData.DomainEvents.FileAddedToImportLogEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddFile_FileIsAdded()
    {
        await this.CreateFileImportLogAsync();

        Result result = await this.Repository.AddFile(TestData.DomainEvents.FileCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        TransactionProcessor.Database.Entities.File? file = await context.Files.SingleOrDefaultAsync(f => f.FileId == TestData.DomainEvents.FileCreatedEvent.FileId);
        file.ShouldNotBeNull();
        file.EstateId.ShouldBe(TestData.DomainEvents.FileCreatedEvent.EstateId);
        file.MerchantId.ShouldBe(TestData.DomainEvents.FileCreatedEvent.MerchantId);
        file.FileLocation.ShouldBe(TestData.DomainEvents.FileCreatedEvent.FileLocation);
        file.FileReceivedDate.ShouldBe(TestData.DomainEvents.FileCreatedEvent.FileReceivedDateTime.Date);
        file.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    public async Task AddFile_EventReplayHandled()
    {
        await this.CreateFileImportLogAsync();

        Result result = await this.Repository.AddFile(TestData.DomainEvents.FileCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.AddFile(TestData.DomainEvents.FileCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddFileLineToFile_FileLineIsAdded()
    {
        await this.CreateFileAsync();

        Result result = await this.Repository.AddFileLineToFile(TestData.DomainEvents.FileLineAddedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        FileLine? fileLine = await context.FileLines.SingleOrDefaultAsync(f => f.FileId == TestData.DomainEvents.FileLineAddedEvent.FileId && f.LineNumber == TestData.DomainEvents.FileLineAddedEvent.LineNumber);
        fileLine.ShouldNotBeNull();
        fileLine.FileLineData.ShouldBe(TestData.DomainEvents.FileLineAddedEvent.FileLine);
        fileLine.Status.ShouldBe("P");
    }

    [Fact]
    public async Task AddFileLineToFile_EventReplayHandled()
    {
        await this.CreateFileAsync();

        Result result = await this.Repository.AddFileLineToFile(TestData.DomainEvents.FileLineAddedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.AddFileLineToFile(TestData.DomainEvents.FileLineAddedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateFileAsComplete_FileIsCompleted()
    {
        await this.CreateFileAsync();

        Result result = await this.Repository.UpdateFileAsComplete(TestData.DomainEvents.FileProcessingCompletedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        TransactionProcessor.Database.Entities.File? file = await context.Files.SingleOrDefaultAsync(f => f.FileId == TestData.DomainEvents.FileProcessingCompletedEvent.FileId);
        file.ShouldNotBeNull();
        file.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateFileLineSuccessful_FileLineIsUpdated()
    {
        await this.CreateFileLineAsync();

        Result result = await this.Repository.UpdateFileLine(TestData.DomainEvents.FileLineProcessingSuccessfulEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        FileLine? fileLine = await context.FileLines.SingleOrDefaultAsync(f => f.FileId == TestData.DomainEvents.FileLineProcessingSuccessfulEvent.FileId && f.LineNumber == TestData.DomainEvents.FileLineProcessingSuccessfulEvent.LineNumber);
        fileLine.ShouldNotBeNull();
        fileLine.Status.ShouldBe("S");
        fileLine.TransactionId.ShouldBe(TestData.DomainEvents.FileLineProcessingSuccessfulEvent.TransactionId);
    }

    [Fact]
    public async Task UpdateFileLineFailed_FileLineIsUpdated()
    {
        await this.CreateFileLineAsync();

        Result result = await this.Repository.UpdateFileLine(TestData.DomainEvents.FileLineProcessingFailedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        FileLine? fileLine = await context.FileLines.SingleOrDefaultAsync(f => f.FileId == TestData.DomainEvents.FileLineProcessingFailedEvent.FileId && f.LineNumber == TestData.DomainEvents.FileLineProcessingFailedEvent.LineNumber);
        fileLine.ShouldNotBeNull();
        fileLine.Status.ShouldBe("F");
        fileLine.TransactionId.ShouldBe(TestData.DomainEvents.FileLineProcessingFailedEvent.TransactionId);
    }

    [Fact]
    public async Task UpdateFileLineIgnored_FileLineIsUpdated()
    {
        await this.CreateFileLineAsync();

        Result result = await this.Repository.UpdateFileLine(TestData.DomainEvents.FileLineProcessingIgnoredEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        FileLine? fileLine = await context.FileLines.SingleOrDefaultAsync(f => f.FileId == TestData.DomainEvents.FileLineProcessingIgnoredEvent.FileId && f.LineNumber == TestData.DomainEvents.FileLineProcessingIgnoredEvent.LineNumber);
        fileLine.ShouldNotBeNull();
        fileLine.Status.ShouldBe("I");
        fileLine.TransactionId.ShouldBe(Guid.Empty);
    }
}
