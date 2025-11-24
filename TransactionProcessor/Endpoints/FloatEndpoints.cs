using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using TransactionProcessor.Handlers;

namespace TransactionProcessor.Endpoints
{
    public static class FloatEndpoints
    {
        private const string BaseRoute = "/api/estates/{estateId:guid}/floats";

        public static IEndpointRouteBuilder MapFloatEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup(BaseRoute)
                                 .WithTags("Floats")
                                 .RequireAuthorization()
                                 .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy);

            group.MapPost("/", FloatHandlers.CreateFloatForContractProduct)
                 .WithName("CreateFloatForContractProduct");

            group.MapPut("/", FloatHandlers.RecordFloatCreditPurchase)
                 .WithName("RecordFloatCreditPurchase");

            return endpoints;
        }
    }
}