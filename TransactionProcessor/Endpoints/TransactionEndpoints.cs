using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using TransactionProcessor.Handlers;

namespace TransactionProcessor.Endpoints
{
    public static class TransactionEndpoints
    {
        private const string BaseRoute = "/api/transactions";

        public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup(BaseRoute)
                                 .WithTags("Transactions")
                                 .RequireAuthorization()
                                 .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy);

            // POST /api/transactions  - rejects password tokens => require client credentials
            group.MapPost("/", TransactionHandlers.PerformTransaction)
                 .WithName("PerformTransaction");

            // POST /api/{estateId}/transactions/{transactionId}/resendreceipt
            // note: controller used absolute route; map both forms — keep support for the original absolute route
            group.MapPost("/{transactionId:guid}/resendreceipt", TransactionHandlers.ResendTransactionReceipt)
                 .WithName("ResendTransactionReceipt");

            // Also map legacy absolute route (controller had "/api/{estateId}/transactions/{transactionId}/resendreceipt")
            endpoints.MapPost("/api/{estateId:guid}/transactions/{transactionId:guid}/resendreceipt", TransactionHandlers.ResendTransactionReceipt)
                     .WithName("ResendTransactionReceiptLegacy");

            return endpoints;
        }
    }
}