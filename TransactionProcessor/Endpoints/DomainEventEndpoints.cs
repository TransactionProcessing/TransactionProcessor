using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TransactionProcessor.Handlers;

namespace TransactionProcessor.Endpoints;

public static class DomainEventEndpoints{
    private const string BaseRoute = "/api/domainevents";

    public static IEndpointRouteBuilder MapDomainEventEndpoints(this IEndpointRouteBuilder endpoints) {
        endpoints.MapPost(BaseRoute, DomainEventHandlers.HandleDomainEvent)
                 .WithTags("DomainEvents");
        return endpoints;
    }
}