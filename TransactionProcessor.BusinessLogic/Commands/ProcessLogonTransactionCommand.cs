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
        /// Initializes a new instance of the <see cref="ProcessLogonTransactionCommand"/> class.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        public ProcessLogonTransactionCommand(Guid commandId) : base(commandId)
        {
        }

        #endregion
    }
}