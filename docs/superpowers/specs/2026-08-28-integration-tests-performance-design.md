# Integration Tests Performance Design

**Goal:** Reduce the wall-clock time of `TransactionProcessor.IntegrationTests` without introducing cross-scenario contamination.

**Architecture:** Treat performance work as a ranked set of changes. First remove avoidable per-scenario overhead that is deterministic and low-risk, then tighten retry and polling behavior, and only then consider broader lifetime changes for the Docker-backed stack if cleanup can be proven complete. Preserve the existing scenario assertions and keep state reset explicit rather than implicit.

**Tech Stack:** .NET 10, NUnit, Reqnroll, Testcontainers/Ductus.FluentDocker, EventStore, SQL Server, NLog, Shouldly.

## Current Constraints

- Preserve scenario isolation unless a reset mechanism is proven deterministic.
- Keep the PR path reliable on GitHub Actions.
- Prefer changes that shorten the common path for all scenarios.
- Do not trade speed for hidden state reuse.

## Evidence From Current Suite

- `TransactionProcessor.IntegrationTests/Common/GenericSteps.cs` starts and stops the full Docker environment per scenario.
- `TransactionProcessor.IntegrationTests/Common/DockerHelper.cs` configures clients, test-bank callbacks, and projections on every scenario run.
- `TransactionProcessor.IntegrationTests/Common/Retry.cs` defaults to a 90-second retry window with a 15-second polling interval.
- The suite contains 16 scenarios across 10 feature files, with `Merchant.feature` being the largest feature file.

## Ranked Improvement Options

1. Low risk, medium impact: reduce retry/polling overhead in known-fast readiness checks.
2. Low risk, medium impact: move deterministic setup out of `[BeforeScenario]` where it can be shared safely.
3. Medium risk, high impact: share the Docker-backed stack longer than a scenario, but only if cleanup is explicit and proven.
4. Low risk, medium impact: trim redundant setup in the biggest feature backgrounds.
5. High risk, limited impact: increase parallelism without first removing shared infrastructure bottlenecks.

