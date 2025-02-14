using System;
using System.Collections.Generic;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.VoucherManagement
{
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using MediatR;
    using Models;
    using Requests;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.BusinessLogic.OperatorInterfaces.IOperatorProxy" />
    public class VoucherManagementProxy : IOperatorProxy
    {
        private readonly IMediator Mediator;
        
        public VoucherManagementProxy(IMediator mediator) {
            this.Mediator = mediator;
        }

        public async Task<Result<OperatorResponse>> ProcessLogonMessage(CancellationToken cancellationToken)
        {
            return Result.Success();
        }

        public async Task<Result<OperatorResponse>> ProcessSaleMessage(Guid transactionId,
                                                                       Guid operatorId,
                                                                       Models.Merchant.Merchant merchant,
                                                                       DateTime transactionDateTime,
                                                                       String transactionReference,
                                                                       Dictionary<String, String> additionalTransactionMetadata,
                                                                       CancellationToken cancellationToken)
        {
            // Extract the required fields
            String recipientEmail = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("RecipientEmail");
            String recipientMobile = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("RecipientMobile");
            String transactionAmount = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("Amount");

            // Covert the transaction amount to Decimal and remove decimal places
            if (Decimal.TryParse(transactionAmount, out Decimal amountAsDecimal) == false)
            {
                return Result.Invalid("Transaction Amount is not a valid decimal value");
            }

            if (String.IsNullOrEmpty(recipientEmail) && String.IsNullOrEmpty(recipientMobile))
            {
                return Result.Invalid("Recipient details (either email or mobile) is a required field for this transaction type");
            }

            VoucherCommands.IssueVoucherCommand command = new(Guid.NewGuid(),
                                                                     operatorId,
                                                                     merchant.EstateId,
                                                                     transactionId,
                                                                     DateTime.Now,
                                                                     amountAsDecimal,
                                                                     recipientEmail,
                                                                     recipientMobile);

            Result<IssueVoucherResponse> result= await this.Mediator.Send(command, cancellationToken);
            
            if (result.IsSuccess) {
                // Build the response metadata
                Dictionary<String, String> additionalTransactionResponseMetadata = new Dictionary<String, String>();
                additionalTransactionResponseMetadata.Add("VoucherCode", result.Data.VoucherCode);
                additionalTransactionResponseMetadata.Add("VoucherMessage", result.Data.Message);
                additionalTransactionResponseMetadata.Add("VoucherExpiryDate", result.Data.ExpiryDate.ToString("yyyy-MM-dd"));

                return Result.Success(new OperatorResponse
                {
                    TransactionId = transactionId.ToString("N"),
                    ResponseCode = "0000",
                    ResponseMessage = "SUCCESS",
                    // This may contain the voucher details to be logged with the transaction, and for possible receipt email/print
                    AdditionalTransactionResponseMetadata = additionalTransactionResponseMetadata,
                    AuthorisationCode = "ABCD1234",
                    IsSuccessful = true
                });
            }

            // TODO: handle a failed issue case
            return null;
        }
    }
}
