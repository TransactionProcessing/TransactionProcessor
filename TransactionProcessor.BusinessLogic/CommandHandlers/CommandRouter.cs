namespace TransactionProcessor.BusinessLogic.CommandHandlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Services;
    using Shared.DomainDrivenDesign.CommandHandling;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.DomainDrivenDesign.CommandHandling.ICommandRouter" />
    public class CommandRouter : ICommandRouter
    {
        #region Fields

        /// <summary>
        /// The transaction domain service
        /// </summary>
        private readonly ITransactionDomainService TransactionDomainService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandRouter"/> class.
        /// </summary>
        /// <param name="transactionDomainService">The transaction domain service.</param>
        public CommandRouter(ITransactionDomainService transactionDomainService)
        {
            this.TransactionDomainService = transactionDomainService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Routes the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task Route(ICommand command,
                                CancellationToken cancellationToken)
        {
            ICommandHandler commandHandler = CreateHandler((dynamic)command);

            await commandHandler.Handle(command, cancellationToken);
        }

        /// <summary>
        /// Creates the handler.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private ICommandHandler CreateHandler(ProcessLogonTransactionCommand command)
        {
            return new TransactionCommandHandler(this.TransactionDomainService);
        }

        #endregion
    }
}