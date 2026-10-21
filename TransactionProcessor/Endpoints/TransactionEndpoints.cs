using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.Handlers;
using TransactionProcessor.DataTransferObjects;

namespace TransactionProcessor.Endpoints
{
    [ExcludeFromCodeCoverage]
    public static class TransactionEndpoints
    {
        private const string BaseRoute = "/api/transactions";

        public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder endpoints) {
            var group = endpoints.MapGroup(BaseRoute).WithTags("Transactions")
                                 .RequireAuthorization()
                                 .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy);

            // POST /api/transactions  - rejects password tokens => require client credentials
            group.MapPost("/logon", TransactionHandlers.PerformLogonTransaction)
                 .WithName("PerformLogonTransaction").WithSummary("Process a logon transaction")
                 .WithDescription("Submits a terminal logon transaction and returns the processor response.").Produces<LogonTransactionResponse>();

            group.MapPost("/sale", TransactionHandlers.PerformSaleTransaction)
                .WithName("PerformSaleTransaction").WithSummary("Process a sale transaction")
                .WithDescription("Submits a sale transaction, including merchant, operator, contract, product, and optional metadata.").Produces<SaleTransactionResponse>();
            
            group.MapPost("/reconciliation", TransactionHandlers.PerformReconciliationTransaction)
                .WithName("PerformReconciliationTransaction").WithSummary("Reconcile terminal transactions")
                .WithDescription("Submits the transaction count and value reported by a terminal, optionally broken down by operator and contract.").Produces<ReconciliationResponse>();

            // POST /api/{estateId}/transactions/{transactionId}/resendreceipt
            // note: controller used absolute route; map both forms — keep support for the original absolute route
            group.MapPost("/{transactionId:guid}/resendreceipt", TransactionHandlers.ResendTransactionReceipt)
                 .WithName("ResendTransactionReceipt").WithSummary("Resend a transaction receipt")
                 .WithDescription("Resends the receipt for the transactionId. estateId is currently bound from the query string on this route.");

            // Also map legacy absolute route (controller had "/api/{estateId}/transactions/{transactionId}/resendreceipt")
            endpoints.MapPost("/api/{estateId:guid}/transactions/{transactionId:guid}/resendreceipt", TransactionHandlers.ResendTransactionReceipt)
                     .WithName("ResendTransactionReceiptLegacy").WithSummary("Resend a transaction receipt (legacy route)")
                     .WithDescription("Resends the receipt for the transaction identified by estateId and transactionId in the route.");

            return endpoints;
        }
    }
}