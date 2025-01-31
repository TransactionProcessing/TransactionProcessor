using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.General;
using SimpleResults;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using System;
using Shared.Results;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.Factories;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.BusinessLogic.Requests;
using MediatR;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.Models.Contract;
using CalculationType = TransactionProcessor.DataTransferObjects.Responses.Contract.CalculationType;
using ContractProductTransactionFee = TransactionProcessor.DataTransferObjects.Responses.Contract.ContractProductTransactionFee;
using FeeType = TransactionProcessor.DataTransferObjects.Responses.Contract.FeeType;
using ProductType = TransactionProcessor.DataTransferObjects.Responses.Contract.ProductType;

namespace TransactionProcessor.Controllers {
    [ExcludeFromCodeCoverage]
    [Route(ControllerRoute)]
    [ApiController]
    [Authorize]
    public class ContractController : ControllerBase {
        private readonly IMediator Mediator;

        private ClaimsPrincipal UserOverride;
        
        internal void SetContextOverride(HttpContext ctx) {
            UserOverride = ctx.User;
        }

        #region Constructors

        public ContractController(IMediator mediator) {
            this.Mediator = mediator;
        }

        #endregion

        internal ClaimsPrincipal GetUser() {
            return UserOverride switch {
                null => HttpContext.User,
                _ => UserOverride
            };
        }

        #region Methods

        private Result StandardSecurityChecks(Guid estateId) {
            // Get the Estate Id claim from the user
            Claim estateIdClaim = ClaimsHelper.GetUserClaim(GetUser(), "EstateId", estateId.ToString());

            string estateRoleName = Environment.GetEnvironmentVariable("EstateRoleName");
            if (ClaimsHelper.IsUserRolesValid(GetUser(), new[] { string.IsNullOrEmpty(estateRoleName) ? "Estate" : estateRoleName }) == false) {
                return Result.Forbidden("User role is not valid");
            }

            if (ClaimsHelper.ValidateRouteParameter(estateId, estateIdClaim) == false) {
                return Result.Forbidden("User estate id claim is not valid");
            }

            return Result.Success();
        }

        /// <summary>
        /// Gets the contract.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="contractId">The contract identifier.</param>
        /// <param name="includeProducts">if set to <c>true</c> [include products].</param>
        /// <param name="includeProductsWithFees">if set to <c>true</c> [include products with fees].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{contractId}")]
        public async Task<IActionResult> GetContract([FromRoute] Guid estateId,
                                                     [FromRoute] Guid contractId,
                                                     CancellationToken cancellationToken) {
            Result securityChecksResult = StandardSecurityChecks(estateId);
            if (securityChecksResult.IsFailed)
                return securityChecksResult.ToActionResultX();

            ContractQueries.GetContractQuery query = new ContractQueries.GetContractQuery(estateId, contractId);
            Result<Contract> result = await Mediator.Send(query, cancellationToken);
            if (result.IsFailed)
            {
                var x = result.ToActionResultX();
                return x;
            }

            return ModelFactory.ConvertFrom(result.Data).ToActionResultX();
        }

        /// <summary>
        /// Gets the contracts.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetContracts([FromRoute] Guid estateId,
                                                      CancellationToken cancellationToken) {
            Result securityChecksResult = StandardSecurityChecks(estateId);
            if (securityChecksResult.IsFailed)
                return securityChecksResult.ToActionResultX();

            ContractQueries.GetContractsQuery query = new ContractQueries.GetContractsQuery(estateId);

            Result<List<Contract>> result = await Mediator.Send(query, cancellationToken);
            return ModelFactory.ConvertFrom(result.Data).ToActionResultX();

        }

        /// <summary>
        /// Adds the product to contract.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="contractId">The contract identifier.</param>
        /// <param name="addProductToContractRequest">The add product to contract request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{contractId}/products")]
        public async Task<IActionResult> AddProductToContract([FromRoute] Guid estateId,
                                                              [FromRoute] Guid contractId,
                                                              [FromBody] AddProductToContractRequest addProductToContractRequest,
                                                              CancellationToken cancellationToken) {
            Result securityChecksResult = StandardSecurityChecks(estateId);
            if (securityChecksResult.IsFailed)
                return securityChecksResult.ToActionResultX();

            Guid productId = Guid.NewGuid();

            // Create the command
            ContractCommands.AddProductToContractCommand command =
                new(estateId, contractId, productId,
                    addProductToContractRequest);

            // Route the command
            Result result = await Mediator.Send(command, cancellationToken);
            return result.ToActionResultX();
        }


        /// <summary>
        /// Adds the transaction fee for product to contract.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="contractId">The contract identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <param name="addTransactionFeeForProductToContractRequest">The add transaction fee for product to contract request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{contractId}/products/{productId}/transactionFees")]
        public async Task<IActionResult> AddTransactionFeeForProductToContract([FromRoute] Guid estateId,
                                                                               [FromRoute] Guid contractId,
                                                                               [FromRoute] Guid productId,
                                                                               [FromBody] AddTransactionFeeForProductToContractRequest addTransactionFeeForProductToContractRequest,
                                                                               CancellationToken cancellationToken) {
            Result securityChecksResult = StandardSecurityChecks(estateId);
            if (securityChecksResult.IsFailed)
                return securityChecksResult.ToActionResultX();
            Guid transactionFeeId = Guid.NewGuid();

            // Create the command
            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                new(estateId, contractId, productId, transactionFeeId, addTransactionFeeForProductToContractRequest);

            // Route the command
            Result result = await Mediator.Send(command, cancellationToken);
            return result.ToActionResultX();
        }

        /// <summary>
        /// Disables the transaction fee for product.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="contractId">The contract identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <param name="transactionFeeId">The transaction fee identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{contractId}/products/{productId}/transactionFees/{transactionFeeId}")]
        public async Task<IActionResult> DisableTransactionFeeForProduct([FromRoute] Guid estateId,
                                                                         [FromRoute] Guid contractId,
                                                                         [FromRoute] Guid productId,
                                                                         [FromRoute] Guid transactionFeeId,
                                                                         CancellationToken cancellationToken) {
            Result securityChecksResult = StandardSecurityChecks(estateId);
            if (securityChecksResult.IsFailed)
                return securityChecksResult.ToActionResultX();
            // Create the command
            ContractCommands.DisableTransactionFeeForProductCommand command = new(contractId, estateId, productId, transactionFeeId);

            // Route the command
            Result result = await Mediator.Send(command, cancellationToken);

            // return the result
            return result.ToActionResultX();
        }


        /// <summary>
        /// Creates the contract.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="createContractRequest">The create contract request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateContract([FromRoute] Guid estateId,
                                                        [FromBody] CreateContractRequest createContractRequest,
                                                        CancellationToken cancellationToken) {
            Result securityChecksResult = StandardSecurityChecks(estateId);
            if (securityChecksResult.IsFailed)
                return securityChecksResult.ToActionResultX();
            Guid contractId = Guid.NewGuid();

            // Create the command
            ContractCommands.CreateContractCommand command = new(estateId, contractId, createContractRequest);

            // Route the command
            Result result = await Mediator.Send(command, cancellationToken);

            // return the result
            return result.ToActionResultX();
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const string ControllerName = "contracts";

        /// <summary>
        /// The controller route
        /// </summary>
        private const string ControllerRoute = "api/estates/{estateid}/" + ControllerName;

        #endregion
    }
}
