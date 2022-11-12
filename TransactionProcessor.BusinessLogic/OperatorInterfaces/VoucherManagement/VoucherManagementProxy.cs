using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.VoucherManagement
{
    using System.Security.Policy;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EstateManagement.DataTransferObjects.Responses;
    using MediatR;
    //using global::VoucherManagement.Client;
    //using global::VoucherManagement.DataTransferObjects;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
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

        public async Task<OperatorResponse> ProcessLogonMessage(String accessToken,
                                                                CancellationToken cancellationToken)
        {
            return null;
        }

        /// <summary>
        /// Processes the sale message.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="merchant">The merchant.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="additionalTransactionMetadata">The additional transaction metadata.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Transaction Amount is not a valid decimal value
        /// or
        /// Recipient details (either email or mobile) is a required field for this transaction type
        /// </exception>
        public async Task<OperatorResponse> ProcessSaleMessage(String accessToken,
                                                               Guid transactionId,
                                                               String operatorIdentifier,
                                                               MerchantResponse merchant,
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
                throw new Exception("Transaction Amount is not a valid decimal value");
            }

            if (String.IsNullOrEmpty(recipientEmail) && String.IsNullOrEmpty(recipientMobile))
            {
                throw new Exception("Recipient details (either email or mobile) is a required field for this transaction type");
            }

            IssueVoucherRequest request = IssueVoucherRequest.Create(Guid.NewGuid(),
                                                                     operatorIdentifier,
                                                                     merchant.EstateId,
                                                                     transactionId,
                                                                     DateTime.Now,
                                                                     amountAsDecimal,
                                                                     recipientEmail,
                                                                     recipientMobile);
            IssueVoucherResponse response = await this.Mediator.Send(request, cancellationToken);
            
            if (response != null) {
                // Build the response metadata
                Dictionary<String, String> additionalTransactionResponseMetadata = new Dictionary<String, String>();
                additionalTransactionResponseMetadata.Add("VoucherCode", response.VoucherCode);
                additionalTransactionResponseMetadata.Add("VoucherMessage", response.Message);
                additionalTransactionResponseMetadata.Add("VoucherExpiryDate", response.ExpiryDate.ToString("yyyy-MM-dd"));

                return new OperatorResponse
                {
                    TransactionId = transactionId.ToString("N"),
                    ResponseCode = "0000",
                    ResponseMessage = "SUCCESS",
                    // This may contain the voucher details to be logged with the transaction, and for possible receipt email/print
                    AdditionalTransactionResponseMetadata = additionalTransactionResponseMetadata,
                    AuthorisationCode = "ABCD1234",
                    IsSuccessful = true
                };
            }

            // TODO: handle a failed issue case
            return null;
        }
    }
}
