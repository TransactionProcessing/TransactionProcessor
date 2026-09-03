using Imposter.Abstractions;
using Shared.EntityFramework;
using TransactionProcessor.Database.Contexts;

[assembly: GenerateImposter(typeof(IDbContextResolver<EstateManagementContext>))]
