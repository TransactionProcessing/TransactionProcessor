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
        public Guid TransactionId { get; private set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessLogonTransactionCommand" /> class.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="commandId">The command identifier.</param>
        private ProcessLogonTransactionCommand(Guid transactionId, 
                                               Guid estateId,
                                               Guid merchantId,
                                               String deviceIdentifier,
                                               String transactionType,
                                               DateTime transactionDateTime,
                                               String transactionNumber,
                                               Guid commandId) : base(commandId)
        {
            this.TransactionId = transactionId;
            this.EstateId = estateId;
            this.DeviceIdentifier = deviceIdentifier;
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
        /// Gets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        public String DeviceIdentifier { get; }

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
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <returns></returns>
        public static ProcessLogonTransactionCommand Create(Guid transactionId, 
                                                            Guid estateId,
                                                            Guid merchantId,
                                                            String deviceIdentifier,
                                                            String transactionType,
                                                            DateTime transactionDateTime,
                                                            String transactionNumber)
        {
            return new ProcessLogonTransactionCommand(transactionId, estateId, merchantId, deviceIdentifier, transactionType, transactionDateTime, transactionNumber, Guid.NewGuid());
        }

        #endregion
    }
}