using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using SimpleResults;
using Shared.Results.Web;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.Factories;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.Handlers
{
    public static class ContractHandlers
    {
        public static async Task<IResult> GetContract(IMediator mediator, HttpContext ctx, Guid estateId, Guid contractId, CancellationToken cancellationToken)
        {
            ContractQueries.GetContractQuery query = new(estateId, contractId);
            Result<Contract> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }

        public static async Task<IResult> GetContracts(IMediator mediator, HttpContext ctx, Guid estateId, CancellationToken cancellationToken)
        {
            ContractQueries.GetContractsQuery query = new(estateId);
            Result<List<Contract>> result = await mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }

        public static async Task<IResult> AddProductToContract(IMediator mediator, HttpContext ctx, Guid estateId, Guid contractId, AddProductToContractRequest addProductToContractRequest, CancellationToken cancellationToken)
        {
            Guid productId = Guid.NewGuid();

            ContractCommands.AddProductToContractCommand command =
                new(estateId, contractId, productId, addProductToContractRequest);

            Result result = await mediator.Send(command, cancellationToken);
            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResult> AddTransactionFeeForProductToContract(IMediator mediator, HttpContext ctx, Guid estateId, Guid contractId, Guid productId, AddTransactionFeeForProductToContractRequest addTransactionFeeForProductToContractRequest, CancellationToken cancellationToken)
        {
            Guid transactionFeeId = Guid.NewGuid();

            ContractCommands.AddTransactionFeeForProductToContractCommand command =
                new(estateId, contractId, productId, transactionFeeId, addTransactionFeeForProductToContractRequest);

            Result result = await mediator.Send(command, cancellationToken);
            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResult> DisableTransactionFeeForProduct(IMediator mediator, HttpContext ctx, Guid estateId, Guid contractId, Guid productId, Guid transactionFeeId, CancellationToken cancellationToken)
        {
            ContractCommands.DisableTransactionFeeForProductCommand command = new(estateId, contractId, productId, transactionFeeId);

            Result result = await mediator.Send(command, cancellationToken);
            return ResponseFactory.FromResult(result);
        }

        public static async Task<IResult> CreateContract(IMediator mediator, HttpContext ctx, Guid estateId, CreateContractRequest createContractRequest, CancellationToken cancellationToken)
        {
            Guid contractId = Guid.NewGuid();

            ContractCommands.CreateContractCommand command = new(estateId, contractId, createContractRequest);

            Result result = await mediator.Send(command, cancellationToken);
            return ResponseFactory.FromResult(result);
        }
    }
}