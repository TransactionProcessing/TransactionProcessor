using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.Handlers;
using TransactionProcessor.DataTransferObjects.Responses.Operator;
using System.Collections.Generic;

namespace TransactionProcessor.Endpoints
{
    [ExcludeFromCodeCoverage]
    public static class OperatorEndpoints
    {
        private const string BaseRoute = "/api/estates/{estateId:guid}/operators";

        public static IEndpointRouteBuilder MapOperatorEndpoints(this IEndpointRouteBuilder endpoints)
        {
            RouteGroupBuilder operatorGroup = endpoints
                .MapGroup(BaseRoute)
                .WithTags("Operators")
                .RequireAuthorization()
                .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy);


            // Create
            operatorGroup.MapPost("/", OperatorHandlers.CreateOperator)
                .WithName("CreateOperator").WithSummary("Create an operator")
                .WithDescription("Creates an operator in the specified estate. The operator ID is supplied in the request body.");

            // Update 
            operatorGroup.MapPost("/{operatorId:guid}", OperatorHandlers.UpdateOperator)
                .WithName("UpdateOperator").WithSummary("Update an operator")
                .WithDescription("Updates the selected operator's name and custom merchant or terminal number requirements.");

            // Read endpoints 
            operatorGroup.MapGet("/{operatorId:guid}", OperatorHandlers.GetOperator)
                .WithName("GetOperator").WithSummary("Get an operator")
                .WithDescription("Returns the selected operator's configuration.").Produces<OperatorResponse>();
            operatorGroup.MapGet("/", OperatorHandlers.GetOperators)
                .WithName("GetOperators").WithSummary("List estate operators")
                .WithDescription("Returns operators configured for the specified estate.").Produces<List<OperatorResponse>>();

            return endpoints;
        }
    }
}
