using MessagingService.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using Shared.General;
using System.Security.Claims;
using TransactionProcessor.Handlers;

namespace TransactionProcessor.Endpoints
{
    public static class EstateEndpoints
    {
        private const string BaseRoute = "/api/estates";

        public static IEndpointRouteBuilder MapEstateEndpoints(this IEndpointRouteBuilder endpoints)
        {
            RouteGroupBuilder estateGroup = endpoints
                .MapGroup(BaseRoute)
                .WithTags("Estates")
                .RequireAuthorization();

            estateGroup.MapPost("/", EstateHandlers.CreateEstate)
                .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy)
                .WithName("CreateEstate");

            estateGroup.MapGet("/{estateId:guid}", EstateHandlers.GetEstate)
                .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy)
                .WithName("GetEstate");

            estateGroup.MapGet("/{estateId:guid}/all", EstateHandlers.GetEstates)
                .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy)
                .WithName("GetEstates");

            estateGroup.MapPatch("/{estateId:guid}/users", EstateHandlers.CreateEstateUser)
                .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy)
                .WithName("CreateEstateUser");

            estateGroup.MapPatch("/{estateId:guid}/operators", EstateHandlers.AssignOperator)
                .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy)
                .WithName("AssignEstateOperator");

            estateGroup.MapDelete("/{estateId:guid}/operators/{operatorId:guid}", EstateHandlers.RemoveOperator)
                .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy)
                .WithName("RemoveEstateOperator");

            return endpoints;
        }
    }
}
