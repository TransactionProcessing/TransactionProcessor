# Database Tests Speedup Design

**Goal:** Reduce `TransactionProcessor.DatabaseTests` runtime by removing repeated SQL container startup while preserving strict test isolation.

**Architecture:** Move SQL Server startup out of each test instance and into a shared xUnit class fixture. Keep a unique database name per test via `TestId`, so each test still gets an isolated schema and data store. Leave the repository assertions and EF model checks unchanged, so the only behavioral change is how often the SQL container is created.

**Tech Stack:** .NET 10, xUnit v3, Testcontainers, Entity Framework Core, Moq, Shouldly.

## Requirements

- Preserve test isolation.
- Do not share a database between tests.
- Reuse the SQL Server container across tests.
- Keep the existing repository assertions intact.
- Measure the change with the existing xUnit test suite.
