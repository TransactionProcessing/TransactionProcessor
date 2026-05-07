using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using SimpleResults;
using Shared.Results.Web;
using Shared.Results;
using TransactionProcessor.DataTransferObjects;
using TransactionProcessor.Factories;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Models;

namespace TransactionProcessor.Handlers
{
    public static class TransactionHandlers
    {
        public static async Task<IResult> PerformSaleTransaction(IMediator mediator, HttpContext ctx, SaleTransactionRequest transactionRequest, CancellationToken cancellationToken)
        {
            DateTime transactionReceivedDateTime = DateTime.Now;
            
            Result<SaleTransactionResponse> transactionResult = await ProcessSpecificMessage(mediator, transactionRequest, transactionReceivedDateTime, cancellationToken);

            return ResponseFactory.FromResult(transactionResult, message => message);
        }

        public static async Task<IResult> PerformLogonTransaction(IMediator mediator, HttpContext ctx, LogonTransactionRequest transactionRequest, CancellationToken cancellationToken)
        {
            DateTime transactionReceivedDateTime = DateTime.Now;

            Result<LogonTransactionResponse> transactionResult = await ProcessSpecificMessage(mediator, transactionRequest, transactionReceivedDateTime, cancellationToken);

            return ResponseFactory.FromResult(transactionResult, message => message);
        }

        public static async Task<IResult> PerformReconciliationTransaction(IMediator mediator, HttpContext ctx, ReconciliationRequest transactionRequest, CancellationToken cancellationToken)
        {
            Result<ReconciliationResponse> transactionResult = await ProcessSpecificMessage(mediator, transactionRequest, cancellationToken);

            return ResponseFactory.FromResult(transactionResult, message => message);
        }
        
        public static async Task<IResult> ResendTransactionReceipt(IMediator mediator, HttpContext ctx, Guid estateId, Guid transactionId, CancellationToken cancellationToken)
        {
            TransactionCommands.ResendTransactionReceiptCommand command = new(transactionId, estateId);
            var result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        // Helpers copied from controller logic
        private static async Task<Result<LogonTransactionResponse>> ProcessSpecificMessage(IMediator mediator, LogonTransactionRequest logon, DateTime transactionReceivedDateTime, CancellationToken cancellationToken)
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

            Result<ProcessLogonTransactionResponse> result = await mediator.Send(command, cancellationToken);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return ModelFactory.ConvertFrom(result.Data);
        }

        private static async Task<Result<SaleTransactionResponse>> ProcessSpecificMessage(IMediator mediator, SaleTransactionRequest sale, DateTime transactionReceivedDateTime, CancellationToken cancellationToken)
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

        private static async Task<Result<ReconciliationResponse>> ProcessSpecificMessage(IMediator mediator, ReconciliationRequest reconciliation, CancellationToken cancellationToken)
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
