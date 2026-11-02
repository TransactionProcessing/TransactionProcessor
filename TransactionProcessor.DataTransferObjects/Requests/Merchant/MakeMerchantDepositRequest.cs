using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// Host timestamp, deposit amount, effective date and time, and reference.
    /// </summary>
    /// <example>{"hostDateTime":"2026-01-15T10:30:00Z","amount":42.5,"depositDateTime":"2026-01-15T10:30:00Z","reference":"REF-2026-001"}</example>
    [ExcludeFromCodeCoverage]
    public class MakeMerchantDepositRequest : RequestWithHostDateTime {

        /// <summary>
        /// Monetary amount of the deposit or withdrawal.
        /// </summary>
        public Decimal Amount { get; set; }

        /// <summary>
        /// Date and time the deposit was made.
        /// </summary>
        public DateTimeOffset DepositDateTime { get; set; }

        /// <summary>
        /// External or user-supplied reference for the deposit or withdrawal.
        /// </summary>
        public String Reference { get; set; }
    }
}
