using Imposter.Abstractions;
using Microsoft.AspNetCore.Hosting;
using MediatR;
using TransactionProcessor.BusinessLogic.OperatorInterfaces;
using Shared.EntityFramework;
using TransactionProcessor.Database.Contexts;
using Shared.EventStore.EventHandling;

[assembly: GenerateImposter(typeof(IMediator))]
[assembly: GenerateImposter(typeof(IWebHostEnvironment))]
[assembly: GenerateImposter(typeof(IOperatorProxy))]
[assembly: GenerateImposter(typeof(IDbContextResolver<EstateManagementContext>))]
[assembly: GenerateImposter(typeof(IDomainEventHandlerResolver))]
