using MessagingService.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using Shared.General;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using TransactionProcessor.Handlers;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
using System.Collections.Generic;

namespace TransactionProcessor.Endpoints
{
    [ExcludeFromCodeCoverage]
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
                .WithName("CreateEstate").WithSummary("Create an estate")
                .WithDescription("Creates an estate using the supplied estate ID and display name.");

            estateGroup.MapGet("/{estateId:guid}", EstateHandlers.GetEstate)
                .WithName("GetEstate").WithSummary("Get an estate")
                .WithDescription("Returns estate details, assigned operators, and security users.").Produces<EstateResponse>();

            estateGroup.MapGet("/{estateId:guid}/all", EstateHandlers.GetEstates)
                .WithName("GetEstates").WithSummary("List estate details")
                .WithDescription("Returns the estate collection result for the supplied estate ID.").Produces<List<EstateResponse>>();

            estateGroup.MapPatch("/{estateId:guid}/users", EstateHandlers.CreateEstateUser)
                .WithName("CreateEstateUser").WithSummary("Create an estate user")
                .WithDescription("Creates a security user in the specified estate from the supplied identity details.");

            estateGroup.MapPatch("/{estateId:guid}/operators", EstateHandlers.AssignOperator)
                .WithName("AssignEstateOperator").WithSummary("Assign an operator to an estate")
                .WithDescription("Associates the operator ID in the request body with the specified estate.");

            estateGroup.MapDelete("/{estateId:guid}/operators/{operatorId:guid}", EstateHandlers.RemoveOperator)
                .WithName("RemoveEstateOperator").WithSummary("Remove an operator from an estate")
                .WithDescription("Removes the specified operator association from the estate.");

            return endpoints;
        }
    }
}
