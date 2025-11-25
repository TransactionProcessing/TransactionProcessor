using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using TransactionProcessor.Handlers;

namespace TransactionProcessor.Endpoints
{
    public static class SettlementEndpoints
    {
        private const string BaseRoute = "/api/estates/{estateId:guid}/settlements";

        public static IEndpointRouteBuilder MapSettlementEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup(BaseRoute)
                                 .WithTags("Settlements")
                                 .RequireAuthorization()
                                 .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy);

            group.MapGet("/{settlementDate:datetime}/merchants/{merchantId:guid}/pending", SettlementHandlers.GetPendingSettlement)
                 .WithName("GetPendingSettlement");

            group.MapPost("/{settlementDate:datetime}/merchants/{merchantId:guid}", SettlementHandlers.ProcessSettlement)
                 .WithName("ProcessSettlement");

            group.MapGet("/{settlementId:guid}", SettlementHandlers.GetSettlement)
                 .WithName("GetSettlement");

            group.MapGet("/", SettlementHandlers.GetSettlements)
                 .WithName("GetSettlements");

            return endpoints;
        }
    }
}