namespace TransactionProcessor.Database.ViewEntities
{
    public class SettlementView
    {
        #region Properties

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public Decimal Amount { get; set; }

        public Decimal CalculatedValue { get; set; }
        
        /// <summary>
        /// Gets or sets the day of week.
        /// </summary>
        /// <value>
        /// The day of week.
        /// </value>
        public String DayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; set; }

        public String FeeDescription { get; set; }

        public Boolean IsCompleted { get; set; }

        public Boolean IsSettled { get; set; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; set; }

        public String MerchantName { get; set; }

        /// <summary>
        /// Gets or sets the month.
        /// </summary>
        /// <value>
        /// The month.
        /// </value>
        public String Month { get; set; }

        /// <summary>
        /// Gets or sets the month number.
        /// </summary>
        /// <value>
        /// The month number.
        /// </value>
        public Int32 MonthNumber { get; set; }

        /// <summary>
        /// Gets or sets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        public String OperatorIdentifier { get; set; }

        public DateTime SettlementDate { get; set; }

        public Guid SettlementId { get; set; }

        public Guid TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the week number.
        /// </summary>
        /// <value>
        /// The week number.
        /// </value>
        public Int32 WeekNumber { get; set; }

        /// <summary>
        /// Gets or sets the year number.
        /// </summary>
        /// <value>
        /// The year number.
        /// </value>
        public Int32 YearNumber { get; set; }

        #endregion
    }
}