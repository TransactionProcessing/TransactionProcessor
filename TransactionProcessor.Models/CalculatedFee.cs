namespace TransactionProcessor.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CalculatedFee
    {
        #region Properties

        /// <summary>
        /// Gets or sets the calculated value.
        /// </summary>
        /// <value>
        /// The calculated value.
        /// </value>
        public Decimal CalculatedValue { get; set; }

        /// <summary>
        /// Gets or sets the type of the fee calculation.
        /// </summary>
        /// <value>
        /// The type of the fee calculation.
        /// </value>
        public CalculationType FeeCalculationType { get; set; }

        /// <summary>
        /// Gets or sets the fee calculated date time.
        /// </summary>
        /// <value>
        /// The fee calculated date time.
        /// </value>
        public DateTime FeeCalculatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the fee identifier.
        /// </summary>
        /// <value>
        /// The fee identifier.
        /// </value>
        public Guid FeeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the fee.
        /// </summary>
        /// <value>
        /// The type of the fee.
        /// </value>
        public FeeType FeeType { get; set; }

        /// <summary>
        /// Gets or sets the fee value.
        /// </summary>
        /// <value>
        /// The fee value.
        /// </value>
        public Decimal FeeValue { get; set; }

        #endregion
    }
}