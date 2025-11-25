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
    public static class VoucherHandlers
    {
        public static async Task<IResult> RedeemVoucher(IMediator mediator, HttpContext ctx, RedeemVoucherRequest redeemVoucherRequest, CancellationToken cancellationToken)
        {
            DateTime redeemedDateTime = redeemVoucherRequest.RedeemedDateTime.HasValue ? redeemVoucherRequest.RedeemedDateTime.Value : DateTime.Now;
            VoucherCommands.RedeemVoucherCommand command = new(redeemVoucherRequest.EstateId, redeemVoucherRequest.VoucherCode, redeemedDateTime);

            Result<TransactionProcessor.Models.RedeemVoucherResponse> result = await mediator.Send(command, cancellationToken);

            return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
        }

        public static async Task<IResult> GetVoucher(IMediator mediator, HttpContext ctx, Guid estateId, string voucherCode, Guid? transactionId, CancellationToken cancellationToken)
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

            return ResponseFactory.FromResult(Result.Invalid());
        }
    }
}