# Modern Startup Configuration Design

## Goal

Move TransactionProcessor's web host to the .NET 10 minimal hosting model while preserving configuration precedence, service registrations, request behavior, hosted services, and deployment behavior.

## Existing constraints

- Target framework is `net10.0`.
- Configuration files are loaded from the application directory and `/home/txnproc/config`; environment variables and the version override are applied afterward.
- Lamar registries provide application registrations and must share the host's single service provider.
- Sentry is configured from the built configuration and remains disabled in Development.
- Middleware, endpoint order, Windows service support, NLog, Kestrel, and host settings remain compatible.
- Existing static `Startup` state has consumers in registries, extensions, a controller, and tests; replace runtime access with host services or explicit dependencies.

## Design

`Program.Main` will use `WebApplication.CreateBuilder(args)` and configure host services, custom configuration sources, logging, Windows service support, Lamar, and Sentry before `Build()`. Registrations will receive `IConfiguration` and `IWebHostEnvironment` from DI rather than global `Startup` properties. Lamar registry constructors will accept the required host context through constructor injection or explicit registration callbacks, and the host container will be the only provider.

After building, `Program` will initialize shared logging/serialization/type infrastructure, configure middleware in the current order, map controllers and health/custom endpoints, and run prewarming with the application service provider. Hosted workers will receive dependencies through constructors. Existing APIs that need an alternate named domain-event resolver will resolve it from the host provider.

Tests will use `WebApplicationFactory<Program>` and test-host service overrides. Focused unit tests will construct service registrations with explicit configuration and environment rather than assigning `Startup` globals. The obsolete `Startup` class and XML documentation entries will be removed after all references migrate.

## Compatibility checks

- Verify the effective configuration precedence and reload behavior.
- Verify Development and non-Development Sentry decisions.
- Verify Windows service, Kestrel, NLog, and hosted worker wiring.
- Verify route, authentication, authorization, health, metrics, prewarm, and shutdown behavior.
- Verify IntegrationTest/in-memory database selection.
