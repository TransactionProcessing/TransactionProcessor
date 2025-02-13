using Shared.EventStore.Aggregate;
using Shared.Results;
using SimpleResults;

namespace TransactionProcessor.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Requests;
    using Common;
    using Common.Examples;
    using DataTransferObjects;
    using EstateManagement.DataTransferObjects.Responses;
    using Factories;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;
    using Shared.General;
    using Swashbuckle.AspNetCore.Annotations;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ExcludeFromCodeCoverage]
    [Route(TransactionController.ControllerRoute)]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        #region Fields

        private readonly IMediator Mediator;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionController" /> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="modelFactory">The model factory.</param>
        public TransactionController(IMediator mediator)
        {
            this.Mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs the transaction.
        /// </summary>
        /// <param name="transactionRequest">The transaction request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [SwaggerResponse(201, "Created", typeof(SerialisedMessage))]
        [SwaggerRequestExample(typeof(List<SerialisedMessage>), typeof(TransactionRequestExample))]
        [SwaggerResponseExample(201, typeof(TransactionResponseExample))]
        public async Task<IActionResult> PerformTransaction([FromBody] SerialisedMessage transactionRequest,
                                                            CancellationToken cancellationToken)
        {
            // Reject password tokens
            if (ClaimsHelper.IsPasswordToken(this.User)) {
                return this.Forbid();
            }

            Guid estateId = Guid.Parse(transactionRequest.Metadata[MetadataContants.KeyNameEstateId]);
            Guid merchantId = Guid.Parse(transactionRequest.Metadata[MetadataContants.KeyNameMerchantId]);

            DataTransferObject dto = JsonConvert.DeserializeObject<DataTransferObject>(transactionRequest.SerialisedData,
                                                                                       new JsonSerializerSettings
                                                                                       {
                                                                                           TypeNameHandling = TypeNameHandling.Auto
                                                                                       });
            dto.MerchantId = merchantId;
            dto.EstateId = estateId;

            Result<SerialisedMessage> transactionResult = dto switch {
                LogonTransactionRequest ltr => await this.ProcessSpecificMessage(ltr, cancellationToken),
                SaleTransactionRequest str => await this.ProcessSpecificMessage(str, cancellationToken),
                ReconciliationRequest rr => await this.ProcessSpecificMessage(rr, cancellationToken),
                _ => Result.Invalid($"DTO Type {dto.GetType().Name} not supported)")
            };
            

            return transactionResult.ToActionResultX();
        }

        [HttpPost]
        [Route("/api/{estateId}/transactions/{transactionId}/resendreceipt")]
        [SwaggerResponseExample(201, typeof(TransactionResponseExample))]
        public async Task<IActionResult> ResendTransactionReceipt([FromRoute] Guid estateId, 
                                                                  [FromRoute] Guid transactionId,
                                                                  CancellationToken cancellationToken)
        {
            // Reject password tokens
            if (ClaimsHelper.IsPasswordToken(this.User)) {
                return this.Forbid();
            }

            TransactionCommands.ResendTransactionReceiptCommand command= new(transactionId,estateId);

            var result = await this.Mediator.Send(command, cancellationToken);

            return result.ToActionResultX();
        }

        /// <summary>
        /// Processes the specific message.
        /// </summary>
        /// <param name="logonTransactionRequest">The logon transaction request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<Result<SerialisedMessage>> ProcessSpecificMessage(LogonTransactionRequest logonTransactionRequest,
                                                                             CancellationToken cancellationToken)
        {
            Guid transactionId = Guid.NewGuid();

            TransactionCommands.ProcessLogonTransactionCommand command = new(transactionId,
                                                                                                       logonTransactionRequest.EstateId,
                                                                                                       logonTransactionRequest.MerchantId,
                                                                                                       logonTransactionRequest.DeviceIdentifier,
                                                                                                       logonTransactionRequest.TransactionType,
                                                                                                       logonTransactionRequest.TransactionDateTime,
                                                                                                       logonTransactionRequest.TransactionNumber);

            var result =  await this.Mediator.Send(command, cancellationToken);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return ModelFactory.ConvertFrom(result.Data);
        }

        /// <summary>
        /// Processes the specific message.
        /// </summary>
        /// <param name="saleTransactionRequest">The sale transaction request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<Result<SerialisedMessage>> ProcessSpecificMessage(SaleTransactionRequest saleTransactionRequest,
                                                                             CancellationToken cancellationToken)
        {
            Guid transactionId = Guid.NewGuid();
            
            TransactionCommands.ProcessSaleTransactionCommand command =new(transactionId,
                                                                                         saleTransactionRequest.EstateId,
                                                                                         saleTransactionRequest.MerchantId,
                                                                                         saleTransactionRequest.DeviceIdentifier,
                                                                                         saleTransactionRequest.TransactionType,
                                                                                         saleTransactionRequest.TransactionDateTime,
                                                                                         saleTransactionRequest.TransactionNumber,
                                                                                         saleTransactionRequest.OperatorId,
                                                                                         saleTransactionRequest.CustomerEmailAddress,
                                                                                         saleTransactionRequest.AdditionalTransactionMetadata,
                                                                                         saleTransactionRequest.ContractId,
                                                                                         saleTransactionRequest.ProductId,
                                                                                         // Default to an online sale
                                                                                         saleTransactionRequest.TransactionSource.GetValueOrDefault(1));

            var result= await this.Mediator.Send(command, cancellationToken);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return ModelFactory.ConvertFrom(result.Data);
        }

        private async Task<Result<SerialisedMessage>> ProcessSpecificMessage(ReconciliationRequest reconciliationRequest,
                                                                     CancellationToken cancellationToken)
        {
            Guid transactionId = Guid.NewGuid();

            TransactionCommands.ProcessReconciliationCommand command = new(transactionId,
                                                                                       reconciliationRequest.EstateId,
                                                                                       reconciliationRequest.MerchantId,
                                                                                       reconciliationRequest.DeviceIdentifier,
                                                                                       reconciliationRequest.TransactionDateTime,
                                                                                       reconciliationRequest.TransactionCount,
                                                                                       reconciliationRequest.TransactionValue);

            Result<ProcessReconciliationTransactionResponse> result = await this.Mediator.Send(command, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return ModelFactory.ConvertFrom(result.Data);
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "transactions";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + TransactionController.ControllerName;

        #endregion
    }
}