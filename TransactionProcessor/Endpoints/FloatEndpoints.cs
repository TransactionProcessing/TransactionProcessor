using TransactionProcessing.UnionResults.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TransactionProcessing.Shared.Authorisation;
using System;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.Handlers;

namespace TransactionProcessor.Endpoints
{
    [ExcludeFromCodeCoverage]
    public static class FloatEndpoints
    {
        private const string BaseRoute = "/api/estates/{estateId:guid}/floats";

        public static IEndpointRouteBuilder MapFloatEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup(BaseRoute).AddUnionResultFilter()
                                 .WithTags("Floats")
                                 .RequireAuthorization()
                                 .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy);

            group.MapPost("/", FloatHandlers.CreateFloat)
                 .WithName("CreateFloat").WithSummary("Create an estate float")
                 .WithDescription("Creates a float for the operator and date in the request body and returns the new float ID.")
                 .ProducesUnionResult<Guid>();

            group.MapPut("/", FloatHandlers.RecordFloatCreditPurchase)
                 .WithName("RecordFloatCreditPurchase").WithSummary("Record a float credit purchase")
                 .WithDescription("Records a credit purchase, amount, cost price, and purchase time against the specified estate float.");

            return endpoints;
        }
    }
}
