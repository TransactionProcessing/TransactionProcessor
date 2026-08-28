# Database Tests Speedup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Cut `TransactionProcessor.DatabaseTests` runtime by reusing the SQL Server container while keeping one isolated database per test.

**Architecture:** Keep the current test assertions and event-handling coverage unchanged. Replace the per-test container startup in `BaseTest` with a shared class fixture that owns SQL Server lifecycle, while each test still generates its own `TestId` and database name. After that, trim the highest-setup-cost test classes so they use the new shared fixture without repeating container work.

**Tech Stack:** .NET 10, xUnit v3, Testcontainers, Entity Framework Core, Moq, Shouldly.

**Spec:** `docs/superpowers/specs/2026-08-28-database-tests-speedup-design.md`

## Global Constraints

- Preserve test isolation.
- Do not share a database between tests.
- Reuse the SQL Server container across tests.
- Keep the existing repository assertions intact.
- Measure the change with the existing xUnit test suite.

---

### Task 1: Introduce a shared SQL Server fixture for database tests

**Files:**
- Create: `TransactionProcessor.DatabaseTests/DatabaseTestFixture.cs`
- Create: `TransactionProcessor.DatabaseTests/DatabaseTestFixtureTests.cs`
- Modify: `TransactionProcessor.DatabaseTests/BaseTest.cs`

**Interfaces:**
- Consumes: `Shared.IntegrationTesting.TestContainers.DockerHelper`, `DockerServices.SqlServer`, `TestContext.Current.CancellationToken`
- Produces: a fixture that exposes the SQL Server host port and a reusable connection-string helper for test databases

- [ ] **Step 1: Write the failing test**

Add `TransactionProcessor.DatabaseTests/DatabaseTestFixtureTests.cs` with a test like this:

```csharp
public sealed class DatabaseTestFixtureTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture fixture;

    public DatabaseTestFixtureTests(DatabaseTestFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task GetLocalConnectionString_UsesUniqueDatabaseNames()
    {
        var connection1 = this.fixture.GetLocalConnectionString($"TransactionProcessorReadModel-{Guid.NewGuid()}");
        var connection2 = this.fixture.GetLocalConnectionString($"TransactionProcessorReadModel-{Guid.NewGuid()}");

        connection1.ShouldNotBe(connection2);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test TransactionProcessor.DatabaseTests/TransactionProcessor.DatabaseTests.csproj --filter FullyQualifiedName~DatabaseTestFixtureTests -v minimal`
Expected: fail because the fixture type and shared connection path do not exist yet.

- [ ] **Step 3: Write minimal implementation**

Implement a shared fixture that:

```csharp
public sealed class DatabaseTestFixture : IAsyncLifetime
{
    public Task InitializeAsync();
    public ValueTask DisposeAsync();
    public string GetLocalConnectionString(string databaseName);
}
```

Make `BaseTest` accept the fixture through its constructor, remove `StartSqlContainer()`, and move the SQL container lifetime into `DatabaseTestFixture`.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test TransactionProcessor.DatabaseTests/TransactionProcessor.DatabaseTests.csproj --filter FullyQualifiedName~DatabaseFixture -v minimal`
Expected: pass.

- [ ] **Step 5: Commit**

```bash
git add TransactionProcessor.DatabaseTests/DatabaseTestFixture.cs TransactionProcessor.DatabaseTests/BaseTest.cs TransactionProcessor.DatabaseTests/DatabaseTestFixtureTests.cs docs/superpowers/specs/2026-08-28-database-tests-speedup-design.md docs/superpowers/plans/2026-08-28-database-tests-speedup.md
git commit -m "refactor: share database test sql container"
```

### Task 2: Move the existing test classes onto the shared fixture safely

**Files:**
- Modify: `TransactionProcessor.DatabaseTests/ContractEventTests.cs`
- Modify: `TransactionProcessor.DatabaseTests/EstateEventTests.cs`
- Modify: `TransactionProcessor.DatabaseTests/FileImportLogEventTests.cs`
- Modify: `TransactionProcessor.DatabaseTests/FloatEventTests.cs`
- Modify: `TransactionProcessor.DatabaseTests/MerchantEventTests.cs`
- Modify: `TransactionProcessor.DatabaseTests/OperatorEventTests.cs`
- Modify: `TransactionProcessor.DatabaseTests/SettlementEventTests.cs`
- Modify: `TransactionProcessor.DatabaseTests/StatementEventTests.cs`
- Modify: `TransactionProcessor.DatabaseTests/TransactionEventTests.cs`

**Interfaces:**
- Consumes: the new `DatabaseTestFixture`
- Produces: all database tests continue to create unique `TestId` values and isolated databases, but no longer pay container startup per test

- [ ] **Step 1: Write the failing test**

Update `TransactionProcessor.DatabaseTests/TransactionEventTests.cs` with a fixture-aware constructor and a single representative test that still creates data and reads it back:

```csharp
public class TransactionEventTests : BaseTest
{
    public TransactionEventTests(DatabaseTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task StartTransaction_TransactionIsAdded()
    {
        Result result = await this.Repository.StartTransaction(TestData.DomainEvents.TransactionHasStartedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test TransactionProcessor.DatabaseTests/TransactionProcessor.DatabaseTests.csproj --filter FullyQualifiedName~TransactionEventTests.StartTransaction_TransactionIsAdded -v minimal`
Expected: fail until the class constructors and base plumbing are updated.

- [ ] **Step 3: Write minimal implementation**

Update each test class to use the shared fixture via the base type or constructor injection pattern already used in the project.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test TransactionProcessor.DatabaseTests/TransactionProcessor.DatabaseTests.csproj --filter FullyQualifiedName~TransactionEventTests.StartTransaction_TransactionIsAdded -v minimal`
Expected: pass.

- [ ] **Step 5: Commit**

```bash
git add TransactionProcessor.DatabaseTests/*.cs
git commit -m "refactor: use shared sql fixture in database tests"
```

### Task 3: Validate the runtime improvement and trim the heaviest arrange chains

**Files:**
- Modify: `TransactionProcessor.DatabaseTests/MerchantEventTests.cs`
- Modify: `TransactionProcessor.DatabaseTests/TransactionEventTests.cs`

**Interfaces:**
- Consumes: the shared fixture and current event/replay helpers
- Produces: the same assertions with less duplicated arrange work in the two biggest classes

- [ ] **Step 1: Capture the baseline timing**

Run and record the current wall-clock time:

```powershell
Measure-Command {
    dotnet test TransactionProcessor.DatabaseTests/TransactionProcessor.DatabaseTests.csproj --filter FullyQualifiedName~MerchantEventTests -v minimal
}
Measure-Command {
    dotnet test TransactionProcessor.DatabaseTests/TransactionProcessor.DatabaseTests.csproj --filter FullyQualifiedName~TransactionEventTests -v minimal
}
```

- [ ] **Step 2: Remove duplicated arrange chains**

Extract the repeated setup in `MerchantEventTests` and `TransactionEventTests` into private helper methods only where the helper removes repeated container-bound database setup, not just readability noise.

- [ ] **Step 3: Re-run the same timing commands**

Run the same `Measure-Command` blocks again and confirm the wall-clock time fell materially after the fixture change and helper cleanup.

- [ ] **Step 4: Run the full suite**

Run:

```powershell
dotnet test TransactionProcessor.DatabaseTests/TransactionProcessor.DatabaseTests.csproj -v minimal
```

Expected: all tests pass with noticeably lower wall-clock time than the original per-test-container version.

- [ ] **Step 5: Commit**

```bash
git add TransactionProcessor.DatabaseTests/*.cs
git commit -m "test: confirm database test runtime improvement"
```
