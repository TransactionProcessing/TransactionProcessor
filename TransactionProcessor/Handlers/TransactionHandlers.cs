using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            DateTime transactionReceivedDateTime = DateTime.Now;
            
            Guid estateId = Guid.Parse(transactionRequest.Metadata[MetadataContants.KeyNameEstateId]);
            Guid merchantId = Guid.Parse(transactionRequest.Metadata[MetadataContants.KeyNameMerchantId]);

            Result<DataTransferObject> deserialiseResult = DeserializeTransactionRequest(transactionRequest.SerialisedData);
            if (deserialiseResult.IsFailed || deserialiseResult.Data == null)
            {
                return ResponseFactory.FromResult(Result.Invalid(deserialiseResult.Message));
            }

            DataTransferObject dto = deserialiseResult.Data;

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

        private static Result<DataTransferObject> DeserializeTransactionRequest(String serialisedData)
        {
            try
            {
                JObject jsonObject = JObject.Parse(serialisedData);

                if (IsReconciliationRequest(jsonObject))
                {
                    return Result.Success<DataTransferObject>(jsonObject.ToObject<ReconciliationRequest>());
                }

                if (IsSaleRequest(jsonObject))
                {
                    return Result.Success<DataTransferObject>(jsonObject.ToObject<SaleTransactionRequest>());
                }

                if (IsLogonRequest(jsonObject))
                {
                    return Result.Success<DataTransferObject>(jsonObject.ToObject<LogonTransactionRequest>());
                }

                return Result.Invalid("DTO Type could not be determined");
            }
            catch (JsonException ex)
            {
                return Result.Invalid($"Invalid transaction request payload: {ex.Message}");
            }
        }

        private static Boolean IsLogonRequest(JObject jsonObject)
        {
            String transactionType = jsonObject.GetValue("transaction_type", StringComparison.OrdinalIgnoreCase)?.Value<String>();

            return String.Equals(transactionType, "Logon", StringComparison.OrdinalIgnoreCase) &&
                   HasAllProperties(jsonObject, "device_identifier", "transaction_number");
        }

        private static Boolean IsSaleRequest(JObject jsonObject)
        {
            String transactionType = jsonObject.GetValue("transaction_type", StringComparison.OrdinalIgnoreCase)?.Value<String>();

            return (String.Equals(transactionType, "Sale", StringComparison.OrdinalIgnoreCase) &&
                    HasAllProperties(jsonObject, "device_identifier", "transaction_number")) ||
                   HasAllProperties(jsonObject, "device_identifier", "transaction_number", "operator_id", "contract_id", "product_id");
        }

        private static Boolean IsReconciliationRequest(JObject jsonObject)
        {
            return HasAllProperties(jsonObject, "device_identifier", "transaction_count", "transaction_value");
        }

        private static Boolean HasAllProperties(JObject jsonObject, params String[] propertyNames)
        {
            foreach (String propertyName in propertyNames)
            {
                if (jsonObject.GetValue(propertyName, StringComparison.OrdinalIgnoreCase) == null)
                {
                    return false;
                }
            }

            return true;
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
    }
}
