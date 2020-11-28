using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.Requests
{
    using System.Diagnostics.CodeAnalysis;
    using MediatR;
    using Models;

    public class ProcessReconciliationRequest : IRequest<ProcessReconciliationTransactionResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessReconciliationRequest"/> class.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionCount">The transaction count.</param>
        /// <param name="transactionValue">The transaction value.</param>
        private ProcessReconciliationRequest(Guid transactionId,
                                             Guid estateId,
                                             Guid merchantId,
                                             String deviceIdentifier,
                                             DateTime transactionDateTime,
                                             Int32 transactionCount,
                                             Decimal transactionValue)
        {
            this.TransactionId = transactionId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.DeviceIdentifier = deviceIdentifier;
            this.TransactionDateTime = transactionDateTime;
            this.TransactionCount = transactionCount;
            this.TransactionValue = transactionValue;
        }

        /// <summary>
        /// Gets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public Guid TransactionId { get; }

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; }

        /// <summary>
        /// Gets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; }

        public String DeviceIdentifier { get; }

        public DateTime TransactionDateTime { get; }

        public Int32 TransactionCount { get; }

        public Decimal TransactionValue { get; }
        
        public static ProcessReconciliationRequest Create(Guid transactionId,
                                                          Guid estateId,
                                                          Guid merchantId,
                                                          String deviceIdentifier,
                                                          DateTime transactionDateTime,
                                                          Int32 transactionCount,
                                                          Decimal transactionValue)
        {
            return new ProcessReconciliationRequest(transactionId, estateId, merchantId, deviceIdentifier, transactionDateTime, transactionCount, transactionValue);
        }
    }
}
