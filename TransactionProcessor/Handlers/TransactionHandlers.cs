using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SimpleResults;
using Shared.Results.Web;
using Shared.General;
using Shared.Results;
using TransactionProcessor.DataTransferObjects;
using TransactionProcessor.Factories;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Models;

namespace TransactionProcessor.Handlers
{
    public static class TransactionHandlers
    {
        public static async Task<IResult> PerformTransaction(IMediator mediator, HttpContext ctx, SerialisedMessage transactionRequest, CancellationToken cancellationToken)
        {
            if (transactionRequest == null || string.IsNullOrWhiteSpace(transactionRequest.SerialisedData))
            {
                return Results.BadRequest("Transaction request body is missing.");
            }

            DateTime transactionReceivedDateTime = DateTime.Now;

            DataTransferObject dto;
            try
            {
                dto = JsonConvert.DeserializeObject<DataTransferObject>(transactionRequest.SerialisedData,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            }
            catch (JsonException)
            {
                return Results.BadRequest("Transaction request body is invalid.");
            }

            if (dto == null)
            {
                return Results.BadRequest("Transaction request body is invalid.");
            }

            if (TryGetMetadataGuid(transactionRequest.Metadata, MetadataContants.KeyNameEstateId, out Guid estateId) == false)
            {
                if (dto.EstateId == Guid.Empty)
                {
                    return Results.BadRequest("Transaction request metadata is missing or invalid.");
                }

                estateId = dto.EstateId;
            }

            if (TryGetMetadataGuid(transactionRequest.Metadata, MetadataContants.KeyNameMerchantId, out Guid merchantId) == false)
            {
                if (dto.MerchantId == Guid.Empty)
                {
                    return Results.BadRequest("Transaction request metadata is missing or invalid.");
                }

                merchantId = dto.MerchantId;
            }

            dto.MerchantId = merchantId;
            dto.EstateId = estateId;
            if (dto.TransactionDateTime.Kind == DateTimeKind.Utc)
            {
                dto.TransactionDateTime = new DateTime(dto.TransactionDateTime.Ticks, DateTimeKind.Unspecified);
            }

            Result<SerialisedMessage> transactionResult = dto switch
            {
                LogonTransactionRequest ltr => await ProcessSpecificMessage(mediator, ltr, transactionReceivedDateTime, cancellationToken),
                SaleTransactionRequest str => await ProcessSpecificMessage(mediator, str, transactionReceivedDateTime, cancellationToken),
                ReconciliationRequest rr => await ProcessSpecificMessage(mediator, rr, cancellationToken),
                _ => Result.Invalid($"DTO Type {dto.GetType().Name} not supported)")
            };

            return ResponseFactory.FromResult(transactionResult, message => message);
        }

        public static async Task<IResult> ResendTransactionReceipt(IMediator mediator, HttpContext ctx, Guid estateId, Guid transactionId, CancellationToken cancellationToken)
        {
            TransactionCommands.ResendTransactionReceiptCommand command = new(transactionId, estateId);
            var result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        // Helpers copied from controller logic
        private static async Task<Result<SerialisedMessage>> ProcessSpecificMessage(IMediator mediator, LogonTransactionRequest logon, DateTime transactionReceivedDateTime, CancellationToken cancellationToken)
        {
            Guid transactionId = Guid.NewGuid();

            TransactionCommands.ProcessLogonTransactionCommand command = new(
                transactionId,
                logon.EstateId,
                logon.MerchantId,
                logon.DeviceIdentifier,
                logon.TransactionType,
                logon.TransactionDateTime,
                logon.TransactionNumber,
                transactionReceivedDateTime);

            var result = await mediator.Send(command, cancellationToken);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return ModelFactory.ConvertFrom(result.Data);
        }

        private static async Task<Result<SerialisedMessage>> ProcessSpecificMessage(IMediator mediator, SaleTransactionRequest sale, DateTime transactionReceivedDateTime, CancellationToken cancellationToken)
        {
            Guid transactionId = Guid.NewGuid();

            TransactionCommands.ProcessSaleTransactionCommand command = new(
                transactionId,
                sale.EstateId,
                sale.MerchantId,
                sale.DeviceIdentifier,
                sale.TransactionType,
                sale.TransactionDateTime,
                sale.TransactionNumber,
                sale.OperatorId,
                sale.CustomerEmailAddress,
                sale.AdditionalTransactionMetadata,
                sale.ContractId,
                sale.ProductId,
                // Default to an online sale
                sale.TransactionSource.GetValueOrDefault(1),
                transactionReceivedDateTime);

            var result = await mediator.Send(command, cancellationToken);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return ModelFactory.ConvertFrom(result.Data);
        }

        private static async Task<Result<SerialisedMessage>> ProcessSpecificMessage(IMediator mediator, ReconciliationRequest reconciliation, CancellationToken cancellationToken)
        {
            Guid transactionId = Guid.NewGuid();

            TransactionCommands.ProcessReconciliationCommand command = new(
                transactionId,
                reconciliation.EstateId,
                reconciliation.MerchantId,
                reconciliation.DeviceIdentifier,
                reconciliation.TransactionDateTime,
                reconciliation.TransactionCount,
                reconciliation.TransactionValue);

            Result<ProcessReconciliationTransactionResponse> result = await mediator.Send(command, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return ModelFactory.ConvertFrom(result.Data);
        }

        private static bool TryGetMetadataGuid(IDictionary<string, string> metadata, string key, out Guid value)
        {
            value = Guid.Empty;

            if (metadata == null)
            {
                return false;
            }

            return metadata.TryGetValue(key, out string rawValue) && Guid.TryParse(rawValue, out value);
        }
    }
}
