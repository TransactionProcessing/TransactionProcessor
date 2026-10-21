using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using TransactionProcessor.Handlers;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using System.Collections.Generic;

namespace TransactionProcessor.Endpoints
{
    [ExcludeFromCodeCoverage]
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
            contractGroup.MapGet("/{contractId:guid}", ContractHandlers.GetContract)
                .WithName("GetContract").WithSummary("Get a contract")
                .WithDescription("Returns one contract in the specified estate, including its products and transaction fees.")
                .Produces<ContractResponse>();
            contractGroup.MapGet("/", ContractHandlers.GetContracts)
                .WithName("GetContracts").WithSummary("List estate contracts")
                .WithDescription("Returns all contracts belonging to the specified estate.")
                .Produces<List<ContractResponse>>();

            // Write / Modify - require client credentials only
            contractGroup.MapPatch("/{contractId:guid}/products", ContractHandlers.AddProductToContract)
                .WithName("AddProductToContract").WithSummary("Add a product to a contract")
                .WithDescription("Adds the supplied product definition to the contract. The service generates the product ID.");

            contractGroup.MapPatch("/{contractId:guid}/products/{productId:guid}/transactionFees", ContractHandlers.AddTransactionFeeForProductToContract)
                .WithName("AddTransactionFeeForProductToContract").WithSummary("Add a transaction fee")
                .WithDescription("Adds a fee definition to the specified contract product. The service generates the transaction fee ID.");

            contractGroup.MapDelete("/{contractId:guid}/products/{productId:guid}/transactionFees/{transactionFeeId:guid}", ContractHandlers.DisableTransactionFeeForProduct)
                .WithName("DisableTransactionFeeForProduct").WithSummary("Disable a transaction fee")
                .WithDescription("Disables the selected transaction fee on the specified contract product.");

            contractGroup.MapPost("/", ContractHandlers.CreateContract)
                .WithName("CreateContract").WithSummary("Create a contract")
                .WithDescription("Creates a contract in the specified estate for the supplied operator.");

            return endpoints;
        }
    }
}
