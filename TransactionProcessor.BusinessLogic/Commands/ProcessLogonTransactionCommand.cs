namespace TransactionProcessor.BusinessLogic.Commands
{
    using System;
    using Models;
    using Shared.DomainDrivenDesign.CommandHandling;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.DomainDrivenDesign.CommandHandling.Command{TransactionProcessor.Models.ProcessLogonTransactionResponse}" />
    public class ProcessLogonTransactionCommand : Command<ProcessLogonTransactionResponse>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessLogonTransactionCommand" /> class.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="imeiNumber">The imei number.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="commandId">The command identifier.</param>
        private ProcessLogonTransactionCommand(Guid estateId,
                                               Guid merchantId,
                                               String imeiNumber,
                                               String transactionType,
                                               DateTime transactionDateTime,
                                               String transactionNumber,
                                               Guid commandId) : base(commandId)
        {
            this.EstateId = estateId;
            this.IMEINumber = imeiNumber;
            this.MerchantId = merchantId;
            this.TransactionDateTime = transactionDateTime;
            this.TransactionNumber = transactionNumber;
            this.TransactionType = transactionType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; }

        /// <summary>
        /// Gets the imei number.
        /// </summary>
        /// <value>
        /// The imei number.
        /// </value>
        public String IMEINumber { get; }

        /// <summary>
        /// Gets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; }

        /// <summary>
        /// Gets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        public DateTime TransactionDateTime { get; }

        /// <summary>
        /// Gets the transaction number.
        /// </summary>
        /// <value>
        /// The transaction number.
        /// </value>
        public String TransactionNumber { get; }

        /// <summary>
        /// Gets the type of the transaction.
        /// </summary>
        /// <value>
        /// The type of the transaction.
        /// </value>
        public String TransactionType { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified estate identifier.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="imeiNumber">The imei number.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <returns></returns>
        public static ProcessLogonTransactionCommand Create(Guid estateId,
                                                            Guid merchantId,
                                                            String imeiNumber,
                                                            String transactionType,
                                                            DateTime transactionDateTime,
                                                            String transactionNumber)
        {
            return new ProcessLogonTransactionCommand(estateId, merchantId, imeiNumber, transactionType, transactionDateTime, transactionNumber, Guid.NewGuid());
        }

        #endregion
    }
}