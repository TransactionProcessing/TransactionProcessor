using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Exceptions;
using Shared.General;
using Shared.Results;
using Shared.Results.Web;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.Factories;

namespace TransactionProcessor.Controllers
{
    [ExcludeFromCodeCoverage]
    [Route(ControllerRoute)]
    [ApiController]
    [Authorize]
    public class EstateController : ControllerBase
    {
        #region Fields

        /// <summary>
        /// The mediator
        /// </summary>
        private readonly IMediator Mediator;

        #endregion

        #region Constructors

        public EstateController(IMediator mediator)
        {
            Mediator = mediator;
        }

        private ClaimsPrincipal UserOverride;
        internal void SetContextOverride(HttpContext ctx)
        {
            UserOverride = ctx.User;
        }

        internal ClaimsPrincipal GetUser()
        {
            return UserOverride switch
            {
                null => HttpContext.User,
                _ => UserOverride
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the estate.
        /// </summary>
        /// <param name="createEstateRequest">The create estate request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IResult> CreateEstate([FromBody] CreateEstateRequest createEstateRequest,
                                                      CancellationToken cancellationToken)
        {
            // Reject password tokens
            if (ClaimsHelper.IsPasswordToken(GetUser()))
            {
                return ResponseFactory.FromResult(Result.Forbidden());
            }

            // Create the command
            EstateCommands.CreateEstateCommand command = new(createEstateRequest);

            // Route the command
            Result result = await Mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        /// <summary>
        /// Gets the estate.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">Estate not found with estate Id {estateId}</exception>
        [HttpGet]
        [Route("{estateId}")]
        public async Task<IResult> GetEstate([FromRoute] Guid estateId,
                                                   CancellationToken cancellationToken)
        {
            // Get the Estate Id claim from the user
            Result<Claim> estateIdClaimResult = ClaimsHelper.GetUserClaim(GetUser(), "EstateId", estateId.ToString());
            if (estateIdClaimResult.IsFailed)
                return ResponseFactory.FromResult(Result.Forbidden("User estate id claim is not valid"));
            Claim estateIdClaim = estateIdClaimResult.Data;

            string estateRoleName = Environment.GetEnvironmentVariable("EstateRoleName");
            if (ClaimsHelper.IsUserRolesValid(GetUser(), new[] { string.IsNullOrEmpty(estateRoleName) ? "Estate" : estateRoleName }) == false)
            {
                return ResponseFactory.FromResult(Result.Forbidden());
            }

            if (ClaimsHelper.ValidateRouteParameter(estateId, estateIdClaim) == false)
            {
                return ResponseFactory.FromResult(Result.Forbidden());
            }

            EstateQueries.GetEstateQuery query = new(estateId);

            Result<Models.Estate.Estate> result = await Mediator.Send(query, cancellationToken);
            
            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }

        [HttpGet]
        [Route("{estateId}/all")]
        public async Task<IResult> GetEstates([FromRoute] Guid estateId,
                                                    CancellationToken cancellationToken)
        {
            // Get the Estate Id claim from the user
            Result<Claim> estateIdClaimResult = ClaimsHelper.GetUserClaim(GetUser(), "EstateId", estateId.ToString());
            if (estateIdClaimResult.IsFailed)
                return ResponseFactory.FromResult(Result.Forbidden("User estate id claim is not valid"));
            Claim estateIdClaim = estateIdClaimResult.Data;

            string estateRoleName = Environment.GetEnvironmentVariable("EstateRoleName");
            if (ClaimsHelper.IsUserRolesValid(GetUser(), new[] { string.IsNullOrEmpty(estateRoleName) ? "Estate" : estateRoleName }) == false)
            {
                return ResponseFactory.FromResult(Result.Forbidden());
            }

            if (ClaimsHelper.ValidateRouteParameter(estateId, estateIdClaim) == false)
            {
                return ResponseFactory.FromResult(Result.Forbidden());
            }

            EstateQueries.GetEstatesQuery query = new(estateId);

            Result<List<Models.Estate.Estate>> result = await Mediator.Send(query, cancellationToken);
            
            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }

        /// <summary>
        /// Creates the estate user.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="createEstateUserRequest">The create estate user request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>/// <returns></returns>
        [HttpPatch]
        [Route("{estateId}/users")]
        public async Task<IResult> CreateEstateUser([FromRoute] Guid estateId,
                                                          [FromBody] CreateEstateUserRequest createEstateUserRequest,
                                                          CancellationToken cancellationToken)
        {
            // Reject password tokens
            if (ClaimsHelper.IsPasswordToken(GetUser()))
            {
                return ResponseFactory.FromResult(Result.Forbidden());
            }

            // Create the command
            EstateCommands.CreateEstateUserCommand command = new(estateId, createEstateUserRequest);

            // Route the command
            Result result = await Mediator.Send(command, cancellationToken);

            // return the result
            return ResponseFactory.FromResult(result);
        }

        [HttpPatch]
        [Route("{estateId}/operators")]
        public async Task<IResult> AssignOperator([FromRoute] Guid estateId, [FromBody] AssignOperatorRequest assignOperatorRequest, CancellationToken cancellationToken)
        {
            // Reject password tokens
            if (ClaimsHelper.IsPasswordToken(GetUser()))
            {
                return ResponseFactory.FromResult(Result.Forbidden());
            }

            // Create the command
            EstateCommands.AddOperatorToEstateCommand command = new(estateId, assignOperatorRequest);

            // Route the command
            Result result = await Mediator.Send(command, cancellationToken);

            // return the result
            return ResponseFactory.FromResult(result);
        }

        [HttpDelete]
        [Route("{estateId}/operators/{operatorId}")]
        public async Task<IResult> RemoveOperator([FromRoute] Guid estateId,
                                                        [FromRoute] Guid operatorId,
                                                        CancellationToken cancellationToken)
        {
            // Reject password tokens
            if (ClaimsHelper.IsPasswordToken(GetUser()))
            {
                return ResponseFactory.FromResult(Result.Forbidden());
            }

            EstateCommands.RemoveOperatorFromEstateCommand command = new(estateId, operatorId);

            // Route the command
            Result result = await Mediator.Send(command, cancellationToken);

            // return the result
            return ResponseFactory.FromResult(result);
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        private const string ControllerName = "estates";

        /// <summary>
        /// The controller route
        /// </summary>
        private const string ControllerRoute = "api/" + ControllerName;

        #endregion
    }
}
