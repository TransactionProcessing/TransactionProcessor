namespace TransactionProcessor.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Commands;
    using DataTransferObjects;
    using Factories;
    using Microsoft.AspNetCore.Mvc;
    using Shared.DomainDrivenDesign.CommandHandling;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ExcludeFromCodeCoverage]
    [Route(TransactionController.ControllerRoute)]
    [ApiController]
    [ApiVersion("1.0")]
    public class TransactionController : ControllerBase
    {
        #region Fields

        /// <summary>
        /// The command router
        /// </summary>
        private readonly ICommandRouter CommandRouter;

        /// <summary>
        /// The model factory
        /// </summary>
        private readonly IModelFactory ModelFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionController"/> class.
        /// </summary>
        /// <param name="commandRouter">The command router.</param>
        /// <param name="modelFactory">The model factory.</param>
        public TransactionController(ICommandRouter commandRouter,
                                     IModelFactory modelFactory)
        {
            this.CommandRouter = commandRouter;
            this.ModelFactory = modelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Logons the transaction.
        /// </summary>
        /// <param name="logonTransactionRequest">The logon transaction request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> LogonTransaction([FromBody] LogonTransactionRequest logonTransactionRequest,
                                                          CancellationToken cancellationToken)
        {
            ProcessLogonTransactionCommand command = new ProcessLogonTransactionCommand(Guid.NewGuid());

            await this.CommandRouter.Route(command, cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(command.Response));
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "transactions";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + TransactionController.ControllerName;

        #endregion
    }
}