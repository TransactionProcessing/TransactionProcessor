# Integration Tests Performance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Cut `TransactionProcessor.IntegrationTests` runtime by removing avoidable per-scenario overhead first, then improving suite-wide reuse only where isolation can still be proved.

**Architecture:** Start by measuring the existing bottlenecks so the work is driven by timing data instead of intuition. Next, reduce the retry and setup overhead that is paid on every scenario. Finally, evaluate whether a longer-lived test environment is safe enough to justify a broader lifetime change; if not, stop after the low-risk wins and keep the suite fully isolated.

**Tech Stack:** .NET 10, NUnit, Reqnroll, Testcontainers/Ductus.FluentDocker, EventStore, SQL Server, NLog, Shouldly.

**Spec:** `docs/superpowers/specs/2026-08-28-integration-tests-performance-design.md`

## Global Constraints

- Preserve scenario isolation unless a reset mechanism is proven deterministic.
- Keep the PR path reliable on GitHub Actions.
- Prefer changes that shorten the common path for all scenarios.
- Do not trade speed for hidden state reuse.

---

### Task 1: Measure where the suite actually spends time

**Files:**
- Modify: `TransactionProcessor.IntegrationTests/Common/Retry.cs`
- Modify: `TransactionProcessor.IntegrationTests/Common/GenericSteps.cs`
- Modify: `.github/workflows/pullrequest.yml`
- Add: `docs/superpowers/plans/2026-08-28-integration-tests-performance-notes.md`

**Interfaces:**
- Consumes: existing scenario hooks, existing GitHub Actions workflow, existing retry helper
- Produces: a timing baseline per scenario and per setup phase

- [ ] **Step 1: Add a lightweight timing baseline**

Record elapsed time around:

```csharp
StartSystem();
Setup.GlobalSetup(...);
StartContainersForScenarioRun(...);
StopContainersForScenarioRun(...);
```

Keep the output minimal and scenario-scoped so it is usable in CI logs.

- [ ] **Step 2: Run the PR suite once and capture baseline numbers**

Run the integration suite in the same way the PR workflow runs it and record the slowest scenario names, setup phases, and any retry-heavy paths.

- [ ] **Step 3: Persist the notes**

Write the baseline summary into `docs/superpowers/plans/2026-08-28-integration-tests-performance-notes.md` so later changes can be compared against the same numbers.

- [ ] **Step 4: Remove the timing probes if they are not needed long term**

Keep the instrumentation only if it is genuinely useful; otherwise remove it after the baseline is captured.

- [ ] **Step 5: Commit**

```bash
git add TransactionProcessor.IntegrationTests/Common/Retry.cs TransactionProcessor.IntegrationTests/Common/GenericSteps.cs .github/workflows/pullrequest.yml docs/superpowers/plans/2026-08-28-integration-tests-performance-notes.md
git commit -m "perf: capture integration test baseline timings"
```

### Task 2: Reduce retry and polling overhead

**Files:**
- Modify: `TransactionProcessor.IntegrationTests/Common/Retry.cs`
- Modify: `TransactionProcessor.IntegrationTests/Common/DockerHelper.cs`
- Modify: `TransactionProcessor.IntegrationTests/Common/TestingContext.cs`

**Interfaces:**
- Consumes: the existing `Retry.For(...)` helper and the places that depend on it
- Produces: shorter waits for readiness checks that are normally fast, without removing retries for genuinely eventual-consistent paths

- [ ] **Step 1: Identify the retry call sites**

List the call sites that are waiting for container readiness or immediate host configuration versus the call sites that are waiting for real domain propagation.

- [ ] **Step 2: Tighten the fast-path waits**

Use shorter retry windows and intervals for deterministic startup checks where the service should normally be ready within a few seconds.

- [ ] **Step 3: Keep the long-path waits where they are needed**

Do not shorten waits that cover real asynchronous domain work, event projection lag, or downstream propagation.

- [ ] **Step 4: Re-run the impacted scenarios**

Verify the change against the scenarios that previously paid the retry cost and confirm there is no new flakiness.

- [ ] **Step 5: Commit**

```bash
git add TransactionProcessor.IntegrationTests/Common/Retry.cs TransactionProcessor.IntegrationTests/Common/DockerHelper.cs TransactionProcessor.IntegrationTests/Common/TestingContext.cs
git commit -m "perf: tighten integration test retry paths"
```

### Task 3: Hoist deterministic setup out of per-scenario startup

**Files:**
- Modify: `TransactionProcessor.IntegrationTests/Common/GenericSteps.cs`
- Modify: `TransactionProcessor.IntegrationTests/Common/DockerHelper.cs`
- Modify: `TransactionProcessor.IntegrationTests/Common/Setup.cs`
- Modify: `TransactionProcessor.IntegrationTests/AssemblyInfo.cs`

**Interfaces:**
- Consumes: existing NUnit hooks and Docker helper behavior
- Produces: setup that is executed once per feature or once per suite where it is provably deterministic

- [ ] **Step 1: Identify safe-to-share setup**

Separate deterministic work such as static configuration, image selection, and client construction from scenario-specific environment creation.

- [ ] **Step 2: Move safe setup to broader scope**

Promote only the deterministic portions to a wider NUnit scope such as feature-level or assembly-level initialization.

- [ ] **Step 3: Keep scenario-specific state reset explicit**

Leave anything that mutates database rows, EventStore streams, or domain state inside scenario setup/cleanup until it can be proven safe elsewhere.

- [ ] **Step 4: Validate on the full feature set**

Run the whole integration suite and confirm the broader-scope setup does not leak state between scenarios.

- [ ] **Step 5: Commit**

```bash
git add TransactionProcessor.IntegrationTests/Common/GenericSteps.cs TransactionProcessor.IntegrationTests/Common/DockerHelper.cs TransactionProcessor.IntegrationTests/Common/Setup.cs TransactionProcessor.IntegrationTests/AssemblyInfo.cs
git commit -m "perf: hoist deterministic integration setup"
```

### Task 4: Decide whether suite-long environment reuse is safe enough

**Files:**
- Modify: `TransactionProcessor.IntegrationTests/Common/GenericSteps.cs`
- Modify: `TransactionProcessor.IntegrationTests/Common/DockerHelper.cs`
- Modify: feature cleanup hooks under `TransactionProcessor.IntegrationTests/Features`

**Interfaces:**
- Consumes: the reset behavior proven in earlier tasks
- Produces: either a safe suite-long environment with explicit reset, or a decision to stop before that change

- [ ] **Step 1: Write the reset checklist**

Document every mutable surface that must be cleaned between scenarios: SQL rows, EventStore streams/subscriptions, projection state, and in-memory scenario collections.

- [ ] **Step 2: Validate reset completeness**

Prove the reset path is complete by running scenarios that would fail if any prior state leaked through.

- [ ] **Step 3: Only then consider suite-long reuse**

If and only if the reset path is deterministic, move the Docker environment lifetime out to the suite scope.

- [ ] **Step 4: Re-run the heaviest features**

Focus on `Merchant.feature`, `Settlement.feature`, and `SaleTransactionFeature.feature` because they are the most likely to benefit.

- [ ] **Step 5: Commit or stop**

```bash
git add TransactionProcessor.IntegrationTests/Common/GenericSteps.cs TransactionProcessor.IntegrationTests/Common/DockerHelper.cs TransactionProcessor.IntegrationTests/Features/*.feature
git commit -m "perf: extend integration environment lifetime"
```

### Task 5: Rebalance the suite after the harness changes

**Files:**
- Modify: `.github/workflows/pullrequest.yml`
- Modify: `TransactionProcessor.IntegrationTests/Features/*.feature`

**Interfaces:**
- Consumes: the measured timings from Tasks 1-4
- Produces: a more intentional PR suite split between fast smoke coverage and slower full-scenario coverage

- [ ] **Step 1: Identify the slowest scenarios**

Use the measured timings to mark the scenarios that are too expensive for every PR if the harness improvements are still not enough.

- [ ] **Step 2: Split smoke from full coverage**

Keep the deterministic, high-signal scenarios in PR and move the largest end-to-end cases to a slower lane if needed.

- [ ] **Step 3: Keep coverage intentional**

Do not drop tests just to improve metrics; only move them to a different execution lane when the PR signal remains strong.

- [ ] **Step 4: Verify the workflow shape**

Run the workflow locally or on CI and confirm the faster lane still exercises the important paths.

- [ ] **Step 5: Commit**

```bash
git add .github/workflows/pullrequest.yml TransactionProcessor.IntegrationTests/Features/*.feature
git commit -m "perf: rebalance integration test coverage"
```

