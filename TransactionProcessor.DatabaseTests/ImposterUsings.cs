using Imposter.Abstractions;
using TransactionProcessing.Shared.EntityFramework;
using TransactionProcessor.Database.Contexts;

[assembly: GenerateImposter(typeof(IDbContextResolver<EstateManagementContext>))]
