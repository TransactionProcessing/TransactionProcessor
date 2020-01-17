namespace TransactionProcessor.BusinessLogic.CommandHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Models;
    using Services;
    using Shared.DomainDrivenDesign.CommandHandling;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.DomainDrivenDesign.CommandHandling.ICommandHandler" />
    public class TransactionCommandHandler : ICommandHandler
    {
        #region Fields

        /// <summary>
        /// The transaction domain service
        /// </summary>
        private readonly ITransactionDomainService TransactionDomainService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionCommandHandler"/> class.
        /// </summary>
        /// <param name="transactionDomainService">The transaction domain service.</param>
        public TransactionCommandHandler(ITransactionDomainService transactionDomainService)
        {
            this.TransactionDomainService = transactionDomainService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task Handle(ICommand command,
                                 CancellationToken cancellationToken)
        {
            await this.HandleCommand((dynamic)command, cancellationToken);
        }

        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task HandleCommand(ProcessLogonTransactionCommand command,
                                         CancellationToken cancellationToken)
        {
            ProcessLogonTransactionResponse logonResponse = await this.TransactionDomainService.ProcessLogonTransaction(command.TransactionId, command.EstateId,
                command.MerchantId, command.TransactionDateTime, command.TransactionNumber, command.DeviceIdentifier, cancellationToken);

            command.Response = logonResponse;
        }

        #endregion
    }
}