using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.General;
using SimpleResults;
using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using System;
using EstateManagement.Client;
using SecurityService.Client;
using SecurityService.DataTransferObjects.Responses;
using Shared.Results;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.Factories;
using ContractProduct = TransactionProcessor.DataTransferObjects.Responses.Contract.ContractProduct;
using ContractProductTransactionFee = TransactionProcessor.DataTransferObjects.Responses.Contract.ContractProductTransactionFee;
using ContractResponse = EstateManagement.DataTransferObjects.Responses.Contract.ContractResponse;
using ProductType = TransactionProcessor.DataTransferObjects.Responses.Contract.ProductType;
using Azure;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using EstateManagement.DataTransferObjects.Requests.Contract;
using AddProductToContractRequest = TransactionProcessor.DataTransferObjects.Requests.Contract.AddProductToContractRequest;
using AddTransactionFeeForProductToContractRequest = TransactionProcessor.DataTransferObjects.Requests.Contract.AddTransactionFeeForProductToContractRequest;
using CreateContractRequest = TransactionProcessor.DataTransferObjects.Requests.Contract.CreateContractRequest;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Database.Entities;
using Microsoft.CodeAnalysis.Editing;

namespace TransactionProcessor.Controllers {
    [ExcludeFromCodeCoverage]
    [Route(ControllerRoute)]
    [ApiController]
    [Authorize]
    public class ContractController : ControllerBase {
        private readonly IEstateClient EstateClient;
        private readonly ISecurityServiceClient SecurityServiceClient;

        private ClaimsPrincipal UserOverride;
        private TokenResponse TokenResponse;

        internal void SetContextOverride(HttpContext ctx) {
            UserOverride = ctx.User;
        }

        #region Constructors

        public ContractController(IEstateClient estateClient,
                                  ISecurityServiceClient securityServiceClient) {
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
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

            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            Result<ContractResponse> contractResponse = await this.EstateClient.GetContract(this.TokenResponse.AccessToken, estateId, contractId, cancellationToken);
            if (contractResponse.IsFailed) {
                var x = contractResponse.ToActionResultX();
                return x;
            }

            DataTransferObjects.Responses.Contract.ContractResponse response = ConvertContract(contractResponse.Data);

            var result = Result.Success(response);

            return result.ToActionResultX();
        }

        private TransactionProcessor.DataTransferObjects.Responses.Contract.ContractResponse ConvertContract(ContractResponse contractResponse) {
            TransactionProcessor.DataTransferObjects.Responses.Contract.ContractResponse response = new() {
                ContractId = contractResponse.ContractId,
                ContractReportingId = contractResponse.ContractReportingId,
                Description = contractResponse.Description,
                EstateId = contractResponse.EstateId,
                EstateReportingId = contractResponse.EstateReportingId,
                OperatorId = contractResponse.OperatorId,
                OperatorName = contractResponse.OperatorName,
                Products = new List<ContractProduct>()
            };
            if (contractResponse.Products != null) {
                foreach (EstateManagement.DataTransferObjects.Responses.Contract.ContractProduct contractProduct in contractResponse.Products) {
                    var product = new ContractProduct {
                        Name = contractProduct.Name,
                        ProductId = contractProduct.ProductId,
                        DisplayText = contractProduct.DisplayText,
                        ProductReportingId = contractProduct.ProductReportingId,
                        ProductType = (ProductType)contractProduct.ProductType,
                        Value = contractProduct.Value,
                        TransactionFees = new List<ContractProductTransactionFee>()
                    };

                    if (contractProduct.TransactionFees != null) {
                        foreach (EstateManagement.DataTransferObjects.Responses.Contract.ContractProductTransactionFee contractProductTransactionFee in contractProduct.TransactionFees) {
                            var transactionFee = new ContractProductTransactionFee {
                                Description = contractProductTransactionFee.Description,
                                FeeType = (FeeType)contractProductTransactionFee.FeeType,
                                TransactionFeeId = contractProductTransactionFee.TransactionFeeId,
                                Value = contractProductTransactionFee.Value,
                                CalculationType = (CalculationType)contractProductTransactionFee.CalculationType,
                                TransactionFeeReportingId = contractProductTransactionFee.TransactionFeeReportingId
                            };
                            product.TransactionFees.Add(transactionFee);
                        }
                    }

                    response.Products.Add(product);
                }
            }

            return response;
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

            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            var contractsResponse = await this.EstateClient.GetContracts(this.TokenResponse.AccessToken, estateId, cancellationToken);
            if (contractsResponse.IsFailed) {
                var x = contractsResponse.ToActionResultX();
                return x;
            }

            List<DataTransferObjects.Responses.Contract.ContractResponse> responses = new();
            foreach (ContractResponse contractResponse in contractsResponse.Data) {
                var response = ConvertContract(contractResponse);
                responses.Add(response);
            }

            var result = Result.Success(responses);

            return result.ToActionResultX();

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

            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Contract.AddProductToContractRequest() { ProductType = (EstateManagement.DataTransferObjects.Responses.Contract.ProductType)addProductToContractRequest.ProductType, DisplayText = addProductToContractRequest.DisplayText, Value = addProductToContractRequest.Value, ProductName = addProductToContractRequest.ProductName };

            var result = await this.EstateClient.AddProductToContract(this.TokenResponse.AccessToken, estateId, contractId, estateClientRequest, cancellationToken);
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
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
            var estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Contract.AddTransactionFeeForProductToContractRequest() { Value = addTransactionFeeForProductToContractRequest.Value, CalculationType = (EstateManagement.DataTransferObjects.Responses.Contract.CalculationType)addTransactionFeeForProductToContractRequest.CalculationType, FeeType = (EstateManagement.DataTransferObjects.Responses.Contract.FeeType)addTransactionFeeForProductToContractRequest.FeeType, Description = addTransactionFeeForProductToContractRequest.Description };
            var result = await this.EstateClient.AddTransactionFeeForProductToContract(this.TokenResponse.AccessToken, estateId, contractId, productId, estateClientRequest, cancellationToken);
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
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
            // Route the command
            Result result = await this.EstateClient.DisableTransactionFeeForProduct(this.TokenResponse.AccessToken, estateId, contractId, productId, transactionFeeId, cancellationToken);

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
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
            EstateManagement.DataTransferObjects.Requests.Contract.CreateContractRequest estateClientRequest = new EstateManagement.DataTransferObjects.Requests.Contract.CreateContractRequest() { Description = createContractRequest.Description, OperatorId = createContractRequest.OperatorId, };

            Result result = await this.EstateClient.CreateContract(this.TokenResponse.AccessToken, estateId, estateClientRequest, cancellationToken);

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
