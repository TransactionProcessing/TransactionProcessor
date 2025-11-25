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
                .RequireAuthorization()
                .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy);

            estateGroup.MapPost("/", EstateHandlers.CreateEstate)
                .WithName("CreateEstate");

            estateGroup.MapGet("/{estateId:guid}", EstateHandlers.GetEstate)
                .WithName("GetEstate");

            estateGroup.MapGet("/{estateId:guid}/all", EstateHandlers.GetEstates)
                .WithName("GetEstates");

            estateGroup.MapPatch("/{estateId:guid}/users", EstateHandlers.CreateEstateUser)
                .WithName("CreateEstateUser");

            estateGroup.MapPatch("/{estateId:guid}/operators", EstateHandlers.AssignOperator)
                .WithName("AssignEstateOperator");

            estateGroup.MapDelete("/{estateId:guid}/operators/{operatorId:guid}", EstateHandlers.RemoveOperator)
                .WithName("RemoveEstateOperator");

            return endpoints;
        }
    }
}
