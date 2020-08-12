namespace TransactionProcessor.BusinessLogic.Manager
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Models;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TransactionFeeToCalculate
    {
        #region Properties

        /// <summary>
        /// Gets or sets the type of the calculation.
        /// </summary>
        /// <value>
        /// The type of the calculation.
        /// </value>
        public CalculationType CalculationType { get; set; }

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
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public Decimal Value { get; set; }

        #endregion
    }
}