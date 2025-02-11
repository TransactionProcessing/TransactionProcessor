using TransactionProcessor.BusinessLogic.Requests;

namespace EstateManagement.BusinessLogic.RequestHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Services;
    using SimpleResults;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="MediatR.IRequestHandler&lt;EstateManagement.BusinessLogic.Requests.AddTransactionToMerchantStatementRequest&gt;" />
    /// <seealso cref="MediatR.IRequestHandler&lt;EstateManagement.BusinessLogic.Requests.AddSettledFeeToMerchantStatementRequest&gt;" />
    /// <seealso cref="MediatR.IRequestHandler&lt;EstateManagement.BusinessLogic.Requests.AddTransactionToMerchantStatementRequest&gt;" />
    public class MerchantStatementRequestHandler : IRequestHandler<MerchantStatementCommands.AddTransactionToMerchantStatementCommand, Result>, 
                                                   IRequestHandler<MerchantStatementCommands.AddSettledFeeToMerchantStatementCommand,Result>,
                                                   IRequestHandler<MerchantCommands.GenerateMerchantStatementCommand, Result>,
                                                   IRequestHandler<MerchantStatementCommands.EmailMerchantStatementCommand, Result>
    {
        #region Fields

        /// <summary>
        /// The domain service
        /// </summary>
        private readonly IMerchantStatementDomainService MerchantStatementDomainService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MerchantStatementRequestHandler" /> class.
        /// </summary>
        /// <param name="merchantStatementDomainService">The merchant statement domain service.</param>
        public MerchantStatementRequestHandler(IMerchantStatementDomainService merchantStatementDomainService)
        {
            this.MerchantStatementDomainService = merchantStatementDomainService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Result> Handle(MerchantStatementCommands.AddTransactionToMerchantStatementCommand command,
                                         CancellationToken cancellationToken)
        {
            return await this.MerchantStatementDomainService.AddTransactionToStatement(command,
                                                                                cancellationToken);
        }

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Result> Handle(MerchantStatementCommands.AddSettledFeeToMerchantStatementCommand command,
                                         CancellationToken cancellationToken)
        {
            return await this.MerchantStatementDomainService.AddSettledFeeToStatement(command,
                                                                               cancellationToken);
        }

        #endregion

        public async Task<Result> Handle(MerchantCommands.GenerateMerchantStatementCommand command, CancellationToken cancellationToken)
        {
            return await this.MerchantStatementDomainService.GenerateStatement(command, cancellationToken);
        }

        public async Task<Result> Handle(MerchantStatementCommands.EmailMerchantStatementCommand command,
                                         CancellationToken cancellationToken)
        {
            return await this.MerchantStatementDomainService.EmailStatement(command, cancellationToken);
        }
    }
}