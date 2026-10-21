using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using System;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.Handlers;
using TransactionProcessor.DataTransferObjects.Responses.Settlement;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace TransactionProcessor.Endpoints
{
    [ExcludeFromCodeCoverage]
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
                 .WithName("GetPendingSettlement").WithSummary("Get pending merchant settlement")
                 .WithDescription("Returns settlement counts and completion status for the merchant and settlement date.")
                 .Produces<TransactionProcessor.DataTransferObjects.SettlementResponse>();

            group.MapPost("/{settlementDate:datetime}/merchants/{merchantId:guid}", SettlementHandlers.ProcessSettlement)
                 .WithName("ProcessSettlement").WithSummary("Process a merchant settlement")
                 .WithDescription("Processes eligible fees for the merchant and settlement date, and returns the settlement ID.")
                 .Produces<Guid>()
                 .AddOpenApiOperationTransformer((operation, _, _) =>
                 {
                     if (operation.Responses.TryGetValue("200", out var response))
                     {
                         foreach (var mediaType in response.Content.Values)
                         {
                             mediaType.Example = JsonNode.Parse("\"00000000-0000-0000-0000-000000000888\"");
                         }
                     }

                     return Task.CompletedTask;
                 });

            group.MapGet("/{settlementId:guid}", SettlementHandlers.GetSettlement)
                 .WithName("GetSettlement").WithSummary("Get a settlement")
                 .WithDescription("Returns settlement details and fees. The merchantId query parameter is also required by the handler.")
                 .Produces<SettlementResponse>();

            group.MapGet("/", SettlementHandlers.GetSettlements)
                 .WithName("GetSettlements").WithSummary("List settlements")
                 .WithDescription("Returns settlements for this estate, optionally filtered by merchantId, start_date, and end_date. Date filters are passed as strings to the business layer.")
                 .Produces<List<SettlementResponse>>();

            return endpoints;
        }
    }
}
