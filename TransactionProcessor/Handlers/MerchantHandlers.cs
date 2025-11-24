using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Results.Web;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.Factories;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.ProjectionEngine.Models;
using TransactionProcessor.ProjectionEngine.State;

namespace TransactionProcessor.Handlers;

public static class MerchantHandlers {
    public static async Task<IResult> GetMerchantBalance(IMediator mediator,
                                                         HttpContext ctx,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         CancellationToken cancellationToken) {
        MerchantQueries.GetMerchantBalanceQuery query = new(estateId, merchantId);
        Result<MerchantBalanceState> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, r => new MerchantBalanceResponse { Balance = r.Balance, MerchantId = r.MerchantId, AvailableBalance = r.AvailableBalance, EstateId = r.EstateId });
    }

    public static async Task<IResult> GetMerchantBalanceLive(IMediator mediator,
                                                             HttpContext ctx,
                                                             Guid estateId,
                                                             Guid merchantId,
                                                             CancellationToken cancellationToken) {
        MerchantQueries.GetMerchantLiveBalanceQuery query = new(merchantId);
        Result<MerchantBalanceProjectionState1> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, r => new MerchantBalanceResponse { Balance = r.merchant.balance, MerchantId = Guid.Parse(r.merchant.Id), AvailableBalance = r.merchant.balance, EstateId = estateId });
    }

    public static async Task<IResult> GetMerchantBalanceHistory(IMediator mediator,
                                                                HttpContext ctx,
                                                                Guid estateId,
                                                                Guid merchantId,
                                                                DateTime startDate,
                                                                DateTime endDate,
                                                                CancellationToken cancellationToken) {
        MerchantQueries.GetMerchantBalanceHistoryQuery query = new(estateId, merchantId, startDate, endDate);
        Result<List<MerchantBalanceChangedEntry>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, r => {
            List<TransactionProcessor.DataTransferObjects.MerchantBalanceChangedEntryResponse> response = new();
            r.ForEach(h => response.Add(new TransactionProcessor.DataTransferObjects.MerchantBalanceChangedEntryResponse {
                Balance = h.Balance,
                MerchantId = h.MerchantId,
                EstateId = h.EstateId,
                DateTime = h.DateTime,
                ChangeAmount = h.ChangeAmount,
                DebitOrCredit = h.DebitOrCredit,
                OriginalEventId = h.OriginalEventId,
                Reference = h.Reference,
            }));
            return response;
        });
    }

    public static async Task<IResult> CreateMerchant(IMediator mediator,
                                                     HttpContext ctx,
                                                     Guid estateId,
                                                     CreateMerchantRequest createMerchantRequest,
                                                     CancellationToken cancellationToken) {
        MerchantCommands.CreateMerchantCommand command = new(estateId, createMerchantRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> AssignOperator(IMediator mediator,
                                                     HttpContext ctx,
                                                     Guid estateId,
                                                     Guid merchantId,
                                                     AssignOperatorRequest assignOperatorRequest,
                                                     CancellationToken cancellationToken) {
        MerchantCommands.AssignOperatorToMerchantCommand command = new(estateId, merchantId, assignOperatorRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> RemoveOperator(IMediator mediator,
                                                     HttpContext ctx,
                                                     Guid estateId,
                                                     Guid merchantId,
                                                     Guid operatorId,
                                                     CancellationToken cancellationToken) {
        MerchantCommands.RemoveOperatorFromMerchantCommand command = new(estateId, merchantId, operatorId);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> AddDevice(IMediator mediator,
                                                HttpContext ctx,
                                                Guid estateId,
                                                Guid merchantId,
                                                AddMerchantDeviceRequest addMerchantDeviceRequest,
                                                CancellationToken cancellationToken) {
        MerchantCommands.AddMerchantDeviceCommand command = new(estateId, merchantId, addMerchantDeviceRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> SwapMerchantDevice(IMediator mediator,
                                                         HttpContext ctx,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         string deviceIdentifier,
                                                         SwapMerchantDeviceRequest swapMerchantDeviceRequest,
                                                         CancellationToken cancellationToken) {
        MerchantCommands.SwapMerchantDeviceCommand command = new(estateId, merchantId, deviceIdentifier, swapMerchantDeviceRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> AddContract(IMediator mediator,
                                                  HttpContext ctx,
                                                  Guid estateId,
                                                  Guid merchantId,
                                                  AddMerchantContractRequest addMerchantContractRequest,
                                                  CancellationToken cancellationToken) {
        MerchantCommands.AddMerchantContractCommand command = new(estateId, merchantId, addMerchantContractRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> RemoveContract(IMediator mediator,
                                                     HttpContext ctx,
                                                     Guid estateId,
                                                     Guid merchantId,
                                                     Guid contractId,
                                                     CancellationToken cancellationToken) {
        MerchantCommands.RemoveMerchantContractCommand command = new(estateId, merchantId, contractId);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> CreateMerchantUser(IMediator mediator,
                                                         HttpContext ctx,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         CreateMerchantUserRequest createMerchantUserRequest,
                                                         CancellationToken cancellationToken) {
        MerchantCommands.CreateMerchantUserCommand command = new(estateId, merchantId, createMerchantUserRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> MakeDeposit(IMediator mediator,
                                                  HttpContext ctx,
                                                  Guid estateId,
                                                  Guid merchantId,
                                                  MakeMerchantDepositRequest makeMerchantDepositRequest,
                                                  CancellationToken cancellationToken) {
        MerchantCommands.MakeMerchantDepositCommand command = new(estateId, merchantId, DataTransferObjects.Requests.Merchant.MerchantDepositSource.Manual, makeMerchantDepositRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> MakeWithdrawal(IMediator mediator,
                                                     HttpContext ctx,
                                                     Guid estateId,
                                                     Guid merchantId,
                                                     MakeMerchantWithdrawalRequest makeMerchantWithdrawalRequest,
                                                     CancellationToken cancellationToken) {
        MerchantCommands.MakeMerchantWithdrawalCommand command = new(estateId, merchantId, makeMerchantWithdrawalRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GetMerchant(IMediator mediator,
                                                  HttpContext ctx,
                                                  Guid estateId,
                                                  Guid merchantId,
                                                  CancellationToken cancellationToken) {
        MerchantQueries.GetMerchantQuery query = new(estateId, merchantId);
        Result<Models.Merchant.Merchant> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetMerchantContracts(IMediator mediator,
                                                           HttpContext ctx,
                                                           Guid estateId,
                                                           Guid merchantId,
                                                           CancellationToken cancellationToken) {
        MerchantQueries.GetMerchantContractsQuery query = new(estateId, merchantId);
        Result<List<Models.Contract.Contract>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetMerchants(IMediator mediator,
                                                   HttpContext ctx,
                                                   Guid estateId,
                                                   CancellationToken cancellationToken) {
        MerchantQueries.GetMerchantsQuery query = new(estateId);
        Result<List<Models.Merchant.Merchant>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetTransactionFeesForProduct(IMediator mediator,
                                                                   HttpContext ctx,
                                                                   Guid estateId,
                                                                   Guid merchantId,
                                                                   Guid contractId,
                                                                   Guid productId,
                                                                   CancellationToken cancellationToken) {
        MerchantQueries.GetTransactionFeesForProductQuery query = new(estateId, merchantId, contractId, productId);
        Result<List<ContractProductTransactionFee>> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> UpdateMerchant(IMediator mediator,
                                                     HttpContext ctx,
                                                     Guid estateId,
                                                     Guid merchantId,
                                                     UpdateMerchantRequest updateMerchantRequest,
                                                     CancellationToken cancellationToken) {
        MerchantCommands.UpdateMerchantCommand command = new(estateId, merchantId, updateMerchantRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> AddMerchantAddress(IMediator mediator,
                                                         HttpContext ctx,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         TransactionProcessor.DataTransferObjects.Requests.Merchant.Address addAddressRequest,
                                                         CancellationToken cancellationToken) {
        MerchantCommands.AddMerchantAddressCommand command = new(estateId, merchantId, addAddressRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> UpdateMerchantAddress(IMediator mediator,
                                                            HttpContext ctx,
                                                            Guid estateId,
                                                            Guid merchantId,
                                                            Guid addressId,
                                                            TransactionProcessor.DataTransferObjects.Requests.Merchant.Address updateAddressRequest,
                                                            CancellationToken cancellationToken) {
        MerchantCommands.UpdateMerchantAddressCommand command = new(estateId, merchantId, addressId, updateAddressRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> AddMerchantContact(IMediator mediator,
                                                         HttpContext ctx,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         TransactionProcessor.DataTransferObjects.Requests.Merchant.Contact addContactRequest,
                                                         CancellationToken cancellationToken) {
        MerchantCommands.AddMerchantContactCommand command = new(estateId, merchantId, addContactRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> UpdateMerchantContact(IMediator mediator,
                                                            HttpContext ctx,
                                                            Guid estateId,
                                                            Guid merchantId,
                                                            Guid contactId,
                                                            TransactionProcessor.DataTransferObjects.Requests.Merchant.Contact updateContactRequest,
                                                            CancellationToken cancellationToken) {
        MerchantCommands.UpdateMerchantContactCommand command = new(estateId, merchantId, contactId, updateContactRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }

    public static async Task<IResult> GenerateMerchantStatement(IMediator mediator,
                                                                HttpContext ctx,
                                                                Guid estateId,
                                                                Guid merchantId,
                                                                GenerateMerchantStatementRequest generateMerchantStatementRequest,
                                                                CancellationToken cancellationToken) {
        MerchantCommands.GenerateMerchantStatementCommand command = new(estateId, merchantId, generateMerchantStatementRequest);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }
}