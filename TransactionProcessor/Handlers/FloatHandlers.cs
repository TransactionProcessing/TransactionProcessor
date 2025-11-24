using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using SimpleResults;
using Shared.Results.Web;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects;
using TransactionProcessor.Factories;

namespace TransactionProcessor.Handlers
{
    public static class FloatHandlers
    {
        public static async Task<IResult> CreateFloatForContractProduct(IMediator mediator, HttpContext ctx, Guid estateId, CreateFloatForContractProductRequest createFloatRequest, CancellationToken cancellationToken)
        {
            FloatCommands.CreateFloatForContractProductCommand command = new(estateId, createFloatRequest.ContractId, createFloatRequest.ProductId, createFloatRequest.CreateDateTime);
            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResult> RecordFloatCreditPurchase(IMediator mediator, HttpContext ctx, Guid estateId, RecordFloatCreditPurchaseRequest recordFloatCreditPurchaseRequest, CancellationToken cancellationToken)
        {
            FloatCommands.RecordCreditPurchaseForFloatCommand command = new(estateId, recordFloatCreditPurchaseRequest.FloatId, recordFloatCreditPurchaseRequest.CreditAmount, recordFloatCreditPurchaseRequest.CostPrice, recordFloatCreditPurchaseRequest.PurchaseDateTime);
            Result result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result);
        }
    }
}