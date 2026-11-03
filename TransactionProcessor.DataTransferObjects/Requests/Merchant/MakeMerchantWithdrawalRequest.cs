using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// Host timestamp, withdrawal amount, effective date and time, and reference.
    /// </summary>
    /// <example>{"hostDateTime":"2026-01-15T10:30:00Z","amount":42.5,"withdrawalDateTime":"2026-01-15T10:30:00Z","reference":"REF-2026-001"}</example>
    [ExcludeFromCodeCoverage]
    public class MakeMerchantWithdrawalRequest : RequestWithHostDateTime {
        /// <summary>
        /// Monetary amount of the deposit or withdrawal.
        /// </summary>
        public Decimal Amount { get; set; }

        /// <summary>
        /// Date and time the withdrawal was made.
        /// </summary>
        public DateTimeOffset WithdrawalDateTime { get; set; }

        /// <summary>
        /// External or user-supplied reference for the deposit or withdrawal.
        /// </summary>
        public String Reference { get; set; }
    }
}
