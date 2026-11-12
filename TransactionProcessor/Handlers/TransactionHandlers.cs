using MediatR;
using Microsoft.AspNetCore.Http;
using TransactionProcessing.UnionResults.AspNetCore;
using TransactionProcessing.UnionResults;
using System;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects.Requests.Float;
using TransactionProcessor.DataTransferObjects.Requests.Transaction;
using TransactionProcessor.DataTransferObjects.Responses.Transaction;
using TransactionProcessor.Factories;
using TransactionProcessor.Models;

namespace TransactionProcessor.Handlers
{
    public static class TransactionHandlers
    {
        public static async Task<IResultOutcome> PerformSaleTransaction(IMediator mediator, HttpContext ctx, SaleTransactionRequest transactionRequest, CancellationToken cancellationToken)
        {
            Result<SaleTransactionResponse> transactionResult = await ProcessSpecificMessage(mediator, transactionRequest, cancellationToken);

            return ResponseFactory.FromResult(transactionResult, message => message);
        }

        public static async Task<IResultOutcome> PerformLogonTransaction(IMediator mediator, HttpContext ctx, LogonTransactionRequest transactionRequest, CancellationToken cancellationToken)
        {
            Result<LogonTransactionResponse> transactionResult = await ProcessSpecificMessage(mediator, transactionRequest, cancellationToken);

            return ResponseFactory.FromResult(transactionResult, message => message);
        }

        public static async Task<IResultOutcome> PerformReconciliationTransaction(IMediator mediator, HttpContext ctx, ReconciliationRequest transactionRequest, CancellationToken cancellationToken)
        {
            Result<ReconciliationResponse> transactionResult = await ProcessSpecificMessage(mediator, transactionRequest, cancellationToken);

            return ResponseFactory.FromResult(transactionResult, message => message);
        }
        
        /// <summary>Resends the receipt for a transaction.</summary>
        /// <param name="estateId" example="00000000-0000-0000-0000-000000000023">Estate owning the transaction. On the short route this is bound from the query string.</param>
        /// <param name="transactionId">Transaction identifier from the route.</param>
        public static async Task<IResultOutcome> ResendTransactionReceipt(IMediator mediator, HttpContext ctx, Guid estateId, Guid transactionId, CancellationToken cancellationToken)
        {
            TransactionCommands.ResendTransactionReceiptCommand command = new(transactionId, estateId);
            var result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        // Helpers copied from controller logic
        private static async Task<Result<LogonTransactionResponse>> ProcessSpecificMessage(IMediator mediator, LogonTransactionRequest logon,  CancellationToken cancellationToken)
        {
            TransactionCommands.ProcessLogonTransactionCommand command = new(
                logon.EstateId,
                logon.MerchantId,
                logon.DeviceIdentifier,
                logon.TransactionType,
                logon.TransactionDateTime,
                logon.TransactionNumber,
                logon.HostDateTime);

            Result<ProcessLogonTransactionResponse> result = await mediator.Send(command, cancellationToken);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return ModelFactory.ConvertFrom(result.GetValueOrThrow());
        }

        private static async Task<Result<SaleTransactionResponse>> ProcessSpecificMessage(IMediator mediator, SaleTransactionRequest sale, CancellationToken cancellationToken)
        {
            TransactionCommands.ProcessSaleTransactionCommand command = new(
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
                sale.HostDateTime);

            Result<ProcessSaleTransactionResponse> result = await mediator.Send(command, cancellationToken);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return ModelFactory.ConvertFrom(result.GetValueOrThrow());
        }

        private static async Task<Result<ReconciliationResponse>> ProcessSpecificMessage(IMediator mediator, ReconciliationRequest reconciliation, CancellationToken cancellationToken)
        {
            TransactionCommands.ProcessReconciliationCommand command = new(
                reconciliation.EstateId,
                reconciliation.MerchantId,
                reconciliation.DeviceIdentifier,
                reconciliation.TransactionDateTime,
                reconciliation.TransactionCount,
                reconciliation.TransactionValue,
                reconciliation.HostDateTime);

            Result<ProcessReconciliationTransactionResponse> result = await mediator.Send(command, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return ModelFactory.ConvertFrom(result.GetValueOrThrow());
        }
    }
}
