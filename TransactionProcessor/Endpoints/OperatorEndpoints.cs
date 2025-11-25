using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using TransactionProcessor.Handlers;

namespace TransactionProcessor.Endpoints
{
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
                .WithName("CreateOperator");

            // Update 
            operatorGroup.MapPost("/{operatorId:guid}", OperatorHandlers.UpdateOperator)
                .WithName("UpdateOperator");

            // Read endpoints 
            operatorGroup.MapGet("/{operatorId:guid}", OperatorHandlers.GetOperator).WithName("GetOperator");
            operatorGroup.MapGet("/", OperatorHandlers.GetOperators).WithName("GetOperators");

            return endpoints;
        }
    }
}