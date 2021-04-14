namespace TransactionProcessor.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Requests;
    using Common;
    using Common.Examples;
    using DataTransferObjects;
    using Factories;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.CommandHandling;
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

        /// <summary>
        /// The mediator
        /// </summary>
        private readonly IMediator Mediator;

        /// <summary>
        /// The model factory
        /// </summary>
        private readonly IModelFactory ModelFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionController" /> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="modelFactory">The model factory.</param>
        public TransactionController(IMediator mediator,
                                     IModelFactory modelFactory)
        {
            this.Mediator = mediator;
            this.ModelFactory = modelFactory;
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
        [ProducesResponseType(typeof(SerialisedMessage), 201)]
        public async Task<IActionResult> PerformTransaction([FromBody] SerialisedMessage transactionRequest,
                                                            CancellationToken cancellationToken)
        {
            // Reject password tokens
            if (ClaimsHelper.IsPasswordToken(this.User))
            {
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

            SerialisedMessage transactionResponse = await this.ProcessSpecificMessage((dynamic)dto, cancellationToken);

            // TODO: Populate the GET route
            return this.Created("", transactionResponse);
        }

        /// <summary>
        /// Processes the specific message.
        /// </summary>
        /// <param name="logonTransactionRequest">The logon transaction request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<SerialisedMessage> ProcessSpecificMessage(LogonTransactionRequest logonTransactionRequest,
                                                                     CancellationToken cancellationToken)
        {
            Guid transactionId = Guid.NewGuid();

            ProcessLogonTransactionRequest request = ProcessLogonTransactionRequest.Create(transactionId,
                                                                                                       logonTransactionRequest.EstateId,
                                                                                                       logonTransactionRequest.MerchantId,
                                                                                                       logonTransactionRequest.DeviceIdentifier,
                                                                                                       logonTransactionRequest.TransactionType,
                                                                                                       logonTransactionRequest.TransactionDateTime,
                                                                                                       logonTransactionRequest.TransactionNumber);

            ProcessLogonTransactionResponse response = await this.Mediator.Send(request, cancellationToken);

            return this.ModelFactory.ConvertFrom(response);
        }

        /// <summary>
        /// Processes the specific message.
        /// </summary>
        /// <param name="saleTransactionRequest">The sale transaction request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<SerialisedMessage> ProcessSpecificMessage(SaleTransactionRequest saleTransactionRequest,
                                                                     CancellationToken cancellationToken)
        {
            Guid transactionId = Guid.NewGuid();

            ProcessSaleTransactionRequest request = ProcessSaleTransactionRequest.Create(transactionId,
                                                                                         saleTransactionRequest.EstateId,
                                                                                         saleTransactionRequest.MerchantId,
                                                                                         saleTransactionRequest.DeviceIdentifier,
                                                                                         saleTransactionRequest.TransactionType,
                                                                                         saleTransactionRequest.TransactionDateTime,
                                                                                         saleTransactionRequest.TransactionNumber,
                                                                                         saleTransactionRequest.OperatorIdentifier,
                                                                                         saleTransactionRequest.CustomerEmailAddress,
                                                                                         saleTransactionRequest.AdditionalTransactionMetadata,
                                                                                         saleTransactionRequest.ContractId,
                                                                                         saleTransactionRequest.ProductId);

            ProcessSaleTransactionResponse response = await this.Mediator.Send(request, cancellationToken);

            return this.ModelFactory.ConvertFrom(response);
        }

        /// <summary>
        /// Processes the specific message.
        /// </summary>
        /// <param name="reconciliationRequest">The reconciliation request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<SerialisedMessage> ProcessSpecificMessage(ReconciliationRequest reconciliationRequest,
                                                                     CancellationToken cancellationToken)
        {
            Guid transactionId = Guid.NewGuid();

            ProcessReconciliationRequest request = ProcessReconciliationRequest.Create(transactionId,
                                                                                       reconciliationRequest.EstateId,
                                                                                       reconciliationRequest.MerchantId,
                                                                                       reconciliationRequest.DeviceIdentifier,
                                                                                       reconciliationRequest.TransactionDateTime,
                                                                                       reconciliationRequest.TransactionCount,
                                                                                       reconciliationRequest.TransactionValue);

            ProcessReconciliationTransactionResponse response = await this.Mediator.Send(request, cancellationToken);

            return this.ModelFactory.ConvertFrom(response);
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