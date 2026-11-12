using TransactionProcessing.UnionResults.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TransactionProcessor.Handlers;

namespace TransactionProcessor.Endpoints;

public static class DomainEventEndpoints{
    public static IEndpointRouteBuilder MapDomainEventEndpoints(this IEndpointRouteBuilder endpoints) {
        endpoints.MapGroup("/api")
                 .AddUnionResultFilter()
                 .MapPost("/domainevents", DomainEventHandlers.HandleDomainEvent)
                 .WithTags("DomainEvents");
        return endpoints;
    }
}
