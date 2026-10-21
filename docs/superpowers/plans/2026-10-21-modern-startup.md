# Modern Startup Configuration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate TransactionProcessor to .NET 10 minimal hosting and remove its `Startup`-based bootstrapping.

**Architecture:** `Program` owns builder, registrations, pipeline, endpoints, and lifecycle. Configuration/environment are injected, Lamar is the single host provider, and hosted services resolve dependencies normally.

**Tech Stack:** .NET 10, ASP.NET Core minimal hosting, Lamar, xUnit, `WebApplicationFactory`.

**Spec:** `docs/superpowers/specs/2026-10-21-modern-startup-design.md`

## Global Constraints

- Preserve existing configuration sources, order, reload flags, and version override.
- Preserve Windows service, NLog, Sentry, Kestrel, middleware, endpoint, health, metrics, and prewarm behavior.
- Use one DI container and eliminate app runtime dependencies on static `Startup` state.
- Keep IntegrationTest and in-memory database selection working.

## Review Focus

- Configuration source precedence and reload behavior: add a host-level configuration assertion for overlapping values and reload-enabled JSON sources.
- IntegrationTest environment database choice: assert the registry selects the in-memory database for that environment.
- Development Sentry decision: assert Sentry remains disabled in Development and configured otherwise when a section exists.
- Named event-handler resolution: assert subscription ID chooses the matching named resolver.
- Hosted worker dependency resolution and cancellation: update focused worker tests to construct/host the worker with its dependencies.

---

### Task 1: Host configuration and service registration

**Files:**
- Modify: `TransactionProcessor/Program.cs`
- Modify: `TransactionProcessor/Bootstrapper/RepositoryRegistry.cs`
- Modify: `TransactionProcessor/Bootstrapper/MiddlewareRegistry.cs`
- Modify: `TransactionProcessor/Bootstrapper/DomainEventHandlerRegistry.cs`
- Modify: `TransactionProcessor/Bootstrapper/OperatorRegistry.cs`
- Modify tests that construct the registries directly.

**Interfaces:**
- `Program.BuildApplication(string[] args)` returns a configured `WebApplication` for test hosting.
- Registries receive `IConfiguration` and `IWebHostEnvironment` as needed; no registry reads `Startup` globals.

- [ ] Configure `WebApplication.CreateBuilder`, source ordering, host settings, logging, Sentry, Lamar, registrations, and hosted workers.
- [ ] Move all registry configuration to injected host configuration/environment and remove manual `BuildServiceProvider`.
- [ ] Update focused registry tests to provide explicit host context.

### Task 2: Application pipeline and runtime dependencies

**Files:**
- Modify: `TransactionProcessor/Program.cs`
- Modify: `TransactionProcessor/Extensions.cs`
- Modify: `TransactionProcessor/Controllers/DomainEventController.cs`
- Modify: `TransactionProcessor/AutoLogonWorkerService.cs` or its current source file.
- Modify: dependent tests.

**Interfaces:**
- Prewarm resolves registrations from the app's provider.
- `AutoLogonWorkerService` receives operator resolution through constructor injection.
- Controller receives `IServiceProvider` for named resolver selection.

- [ ] Move logging/serialization/type initialization, middleware, route mapping, prewarm, and run lifecycle to the WebApplication pipeline.
- [ ] Replace static runtime container/config/service-provider access with injected services.
- [ ] Preserve middleware order, route mappings, worker lifecycle, and cancellation semantics.

### Task 3: Test host and legacy bootstrap removal

**Files:**
- Modify: `TransactionProcessor.Tests/Common/TransactionProcessorWebFactory.cs`
- Modify: controller and bootstrapper tests under `TransactionProcessor.Tests` and `TransactionProcessor.BusinessLogic.Tests`.
- Modify: `TransactionProcessor/Startup.cs` (remove) and generated XML documentation if maintained.

- [ ] Switch integration test factory to `WebApplicationFactory<Program>` and preserve mocked mediator/controller behavior.
- [ ] Replace direct `Startup` construction and static global setup in unit tests.
- [ ] Remove `Startup` and all stale references/documentation.
