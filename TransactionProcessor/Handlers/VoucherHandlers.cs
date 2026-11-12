using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using TransactionProcessing.UnionResults;
using TransactionProcessing.UnionResults.AspNetCore;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Factories;
using TransactionProcessor.DataTransferObjects.Requests.Voucher;

namespace TransactionProcessor.Handlers
{
    public static class VoucherHandlers
    {
        public static async Task<IResultOutcome> RedeemVoucher(IMediator mediator, HttpContext ctx, RedeemVoucherRequest redeemVoucherRequest, CancellationToken cancellationToken)
        {
            VoucherCommands.RedeemVoucherCommand command = new(redeemVoucherRequest.EstateId, redeemVoucherRequest.VoucherCode, redeemVoucherRequest);

            Result<TransactionProcessor.Models.RedeemVoucherResponse> result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }

        /// <summary>Looks up a voucher by voucher code or transaction ID.</summary>
        /// <param name="estateId" example="00000000-0000-0000-0000-000000000023">Estate that owns the voucher; supplied as a query parameter.</param>
        /// <param name="voucherCode" example="VOUCHER-2026-001">Voucher code query parameter. Takes precedence when non-empty.</param>
        /// <param name="transactionId" example="00000000-0000-0000-0000-000000000057">Optional transaction ID query parameter used when voucherCode is empty.</param>
        public static async Task<IResultOutcome> GetVoucher(IMediator mediator, HttpContext ctx, Guid estateId, string voucherCode, Guid? transactionId, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(voucherCode) == false)
            {
                VoucherQueries.GetVoucherByVoucherCodeQuery queryByVoucherCode = new(estateId, voucherCode);
                Result<TransactionProcessor.Models.Voucher> getVoucherByCodeResult = await mediator.Send(queryByVoucherCode, cancellationToken);

                return ResponseFactory.FromResult(getVoucherByCodeResult, ModelFactory.ConvertFrom);
            }

            if (transactionId.GetValueOrDefault(Guid.Empty) != Guid.Empty)
            {
                VoucherQueries.GetVoucherByTransactionIdQuery queryByTransactionId = new(estateId, transactionId.Value);
                Result<TransactionProcessor.Models.Voucher> getVoucherByTransactionIdResult = await mediator.Send(queryByTransactionId, cancellationToken);

                return ResponseFactory.FromResult(getVoucherByTransactionIdResult, ModelFactory.ConvertFrom);
            }

            return Result.Fail(Error.Validation("A voucher code or transaction ID must be provided."));
        }
    }
}
