using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using TransactionProcessor.Handlers;

namespace TransactionProcessor.Endpoints
{
    public static class ContractEndpoints
    {
        private const string BaseRoute = "/api/estates/{estateId:guid}/contracts";

        public static IEndpointRouteBuilder MapContractEndpoints(this IEndpointRouteBuilder endpoints)
        {
            RouteGroupBuilder contractGroup = endpoints
                .MapGroup(BaseRoute)
                .WithTags("Contracts")
                .RequireAuthorization()
                .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy);

            // Read
            contractGroup.MapGet("/{contractId:guid}", ContractHandlers.GetContract).WithName("GetContract");
            contractGroup.MapGet("/", ContractHandlers.GetContracts).WithName("GetContracts");

            // Write / Modify - require client credentials only
            contractGroup.MapPatch("/{contractId:guid}/products", ContractHandlers.AddProductToContract)
                .WithName("AddProductToContract");

            contractGroup.MapPatch("/{contractId:guid}/products/{productId:guid}/transactionFees", ContractHandlers.AddTransactionFeeForProductToContract)
                .WithName("AddTransactionFeeForProductToContract");

            contractGroup.MapDelete("/{contractId:guid}/products/{productId:guid}/transactionFees/{transactionFeeId:guid}", ContractHandlers.DisableTransactionFeeForProduct)
                .WithName("DisableTransactionFeeForProduct");

            contractGroup.MapPost("/", ContractHandlers.CreateContract)
                .WithName("CreateContract");

            return endpoints;
        }
    }
}