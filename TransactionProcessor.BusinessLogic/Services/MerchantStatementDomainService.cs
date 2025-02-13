using MessagingService.Client;
using MessagingService.DataTransferObjects;
using SecurityService.Client;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shared.General;
using Shared.Results;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecurityService.DataTransferObjects.Responses;
using Shared.Logger;
using TransactionProcessor.Aggregates.Models;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Repository;
using Transaction = TransactionProcessor.Aggregates.Models.Transaction;

namespace TransactionProcessor.BusinessLogic.Services
{
    public interface IMerchantStatementDomainService
    {
        #region Methods

        Task<Result> AddTransactionToStatement(MerchantStatementCommands.AddTransactionToMerchantStatementCommand command, CancellationToken cancellationToken);

        Task<Result> AddSettledFeeToStatement(MerchantStatementCommands.AddSettledFeeToMerchantStatementCommand command, CancellationToken cancellationToken);

        Task<Result> GenerateStatement(MerchantCommands.GenerateMerchantStatementCommand command, CancellationToken cancellationToken);

        Task<Result> EmailStatement(MerchantStatementCommands.EmailMerchantStatementCommand command, CancellationToken cancellationToken);

        #endregion
    }

    public class MerchantStatementDomainService : IMerchantStatementDomainService
    {
        #region Fields

        /// <summary>
        /// The merchant aggregate repository
        /// </summary>
        private readonly IAggregateRepository<MerchantAggregate, DomainEvent> MerchantAggregateRepository;

        /// <summary>
        /// The merchant statement aggregate repository
        /// </summary>
        private readonly IAggregateRepository<MerchantStatementAggregate, DomainEvent> MerchantStatementAggregateRepository;

        private readonly IAggregateRepository<EstateAggregate, DomainEvent> EstateAggregateRepository;

        private readonly ITransactionProcessorReadModelRepository EstateManagementRepository;

        //private readonly IStatementBuilder StatementBuilder;

        private readonly IMessagingServiceClient MessagingServiceClient;

        private readonly ISecurityServiceClient SecurityServiceClient;

        private readonly IFileSystem FileSystem;

        //private readonly IPDFGenerator PdfGenerator;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MerchantStatementDomainService" /> class.
        /// </summary>
        /// <param name="merchantAggregateRepository">The merchant aggregate repository.</param>
        /// <param name="merchantStatementAggregateRepository">The merchant statement aggregate repository.</param>
        /// <param name="estateManagementRepository">The estate management repository.</param>
        /// <param name="statementBuilder">The statement builder.</param>
        /// <param name="messagingServiceClient">The messaging service client.</param>
        /// <param name="securityServiceClient">The security service client.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="pdfGenerator">The PDF generator.</param>
        public MerchantStatementDomainService(IAggregateRepository<MerchantAggregate, DomainEvent> merchantAggregateRepository,
                                              IAggregateRepository<MerchantStatementAggregate, DomainEvent> merchantStatementAggregateRepository,
                                              ITransactionProcessorReadModelRepository estateManagementRepository,
                                              //IStatementBuilder statementBuilder,
                                              IMessagingServiceClient messagingServiceClient,
                                              ISecurityServiceClient securityServiceClient,
                                              IFileSystem fileSystem)//,
                                              //IPDFGenerator pdfGenerator)
        {
            this.MerchantAggregateRepository = merchantAggregateRepository;
            this.MerchantStatementAggregateRepository = merchantStatementAggregateRepository;
            this.EstateManagementRepository = estateManagementRepository;
            //this.StatementBuilder = statementBuilder;
            this.MessagingServiceClient = messagingServiceClient;
            this.SecurityServiceClient = securityServiceClient;
            this.FileSystem = fileSystem;
            //this.PdfGenerator = pdfGenerator;
        }

        #endregion

        #region Methods

        private async Task<Result> ApplyUpdates(Func<MerchantStatementAggregate, Task<Result>> action, Guid estateId, Guid statementId, CancellationToken cancellationToken, Boolean isNotFoundError = true)
        {
            try
            {
                Result<MerchantStatementAggregate> getMerchantStatementResult = await this.GetLatestVersion(statementId, cancellationToken);

                Result<MerchantStatementAggregate> merchantStatementAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getMerchantStatementResult, statementId, isNotFoundError);
                if (merchantStatementAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantStatementAggregateResult);

                MerchantStatementAggregate merchantStatementAggregate = merchantStatementAggregateResult.Data;

                Result result = await action(merchantStatementAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.MerchantStatementAggregateRepository.SaveChanges(merchantStatementAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        [ExcludeFromCodeCoverage(Justification = "This will get replaced by metrics soon")]
        private async Task<Result<MerchantStatementAggregate>> GetLatestVersion(Guid statementId, CancellationToken cancellationToken)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Result<MerchantStatementAggregate> merchantStatementAggregate =
                await this.MerchantStatementAggregateRepository.GetLatestVersion(statementId, cancellationToken);

            sw.Stop();
            Int64 elapsedTime = sw.ElapsedMilliseconds;

            if (elapsedTime > 1000)
            {
                Logger.LogWarning($"Rehydration of MerchantStatementAggregate Id [{statementId}] took {elapsedTime} ms");
            }

            return merchantStatementAggregate;
        }

        public async Task<Result> AddSettledFeeToStatement(MerchantStatementCommands.AddSettledFeeToMerchantStatementCommand command,
                                                           CancellationToken cancellationToken)
        {
            // Work out the next statement date
            DateTime nextStatementDate = CalculateStatementDate(command.SettledDateTime);

            Guid statementId = GuidCalculator.Combine(command.MerchantId, nextStatementDate.ToGuid());
            Guid settlementFeeId = GuidCalculator.Combine(command.TransactionId, command.SettledFeeId);

            Result result = await ApplyUpdates(
                async (MerchantStatementAggregate merchantStatementAggregate) => {

                    SettledFee settledFee = new SettledFee(settlementFeeId, command.TransactionId, command.SettledDateTime, command.SettledAmount);

                    Guid eventId = IdGenerationService.GenerateEventId(new
                    {
                        command.TransactionId,
                        settlementFeeId,
                        settledFee,
                        command.SettledDateTime,
                    });

                    merchantStatementAggregate.AddSettledFeeToStatement(statementId, eventId, nextStatementDate, command.EstateId, command.MerchantId, settledFee);

                    return Result.Success();
                },
                command.EstateId, statementId, cancellationToken, false);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventDateTime"></param>
        /// <returns></returns>
        internal static DateTime CalculateStatementDate(DateTime eventDateTime)
        {
            var calculatedDateTime = eventDateTime.Date.AddMonths(1);

            return new DateTime(calculatedDateTime.Year, calculatedDateTime.Month, 1);
        }

        /// <summary>
        /// Generates the statement.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="statementDate">The statement date.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Result> GenerateStatement(MerchantCommands.GenerateMerchantStatementCommand command, CancellationToken cancellationToken)
        {
            Guid statementId = GuidCalculator.Combine(command.MerchantId, command.RequestDto.MerchantStatementDate.ToGuid());

            Result result = await ApplyUpdates(
                async (MerchantStatementAggregate merchantStatementAggregate) => {

                    merchantStatementAggregate.GenerateStatement(DateTime.Now);

                    return Result.Success();
                },
                command.EstateId, statementId, cancellationToken, false);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> EmailStatement(MerchantStatementCommands.EmailMerchantStatementCommand command,
                                                 CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async (MerchantStatementAggregate merchantStatementAggregate) => {

                    //StatementHeader statementHeader = await this.EstateManagementRepository.GetStatement(command.EstateId, command.MerchantStatementId, cancellationToken);

                    //String html = await this.StatementBuilder.GetStatementHtml(statementHeader, cancellationToken);

                    //String base64 = await this.PdfGenerator.CreatePDF(html, cancellationToken);

                    //SendEmailRequest sendEmailRequest = new SendEmailRequest
                    //{
                    //    Body = "<html><body>Please find attached this months statement.</body></html>",
                    //    ConnectionIdentifier = command.EstateId,
                    //    FromAddress = "golfhandicapping@btinternet.com", // TODO: lookup from config
                    //    IsHtml = true,
                    //    Subject = $"Merchant Statement for {statementHeader.StatementDate}",
                    //    // MessageId = command.MerchantStatementId,
                    //    ToAddresses = new List<String>
                    //    {
                    //        statementHeader.MerchantEmail
                    //    },
                    //    EmailAttachments = new List<EmailAttachment>
                    //    {
                    //        new EmailAttachment
                    //        {
                    //            FileData = base64,
                    //            FileType = FileType.PDF,
                    //            Filename = $"merchantstatement{statementHeader.StatementDate}.pdf"
                    //        }
                    //    }
                    //};

                    //Guid messageId = IdGenerationService.GenerateEventId(new
                    //{
                    //    command.MerchantStatementId,
                    //    DateTime.Now
                    //});

                    //sendEmailRequest.MessageId = messageId;

                    //this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

                    //var sendEmailResponseResult = await this.MessagingServiceClient.SendEmail(this.TokenResponse.AccessToken, sendEmailRequest, cancellationToken);
                    ////if (sendEmailResponseResult.IsFailed) {
                    ////    // TODO: record a failed event??
                    ////}
                    //merchantStatementAggregate.EmailStatement(DateTime.Now, messageId);

                    return Result.Success();
                },
                command.EstateId, command.MerchantStatementId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        /// <summary>
        /// The token response
        /// </summary>
        private TokenResponse TokenResponse;

        public async Task<Result> AddTransactionToStatement(MerchantStatementCommands.AddTransactionToMerchantStatementCommand command,
                                                            CancellationToken cancellationToken)
        {
            // Transaction Completed arrives(if this is a logon transaction or failed then return)
            if (command.IsAuthorised == false)
                return Result.Success();
            if (command.TransactionAmount.HasValue == false)
                return Result.Success();

            // Work out the next statement date
            DateTime nextStatementDate = CalculateStatementDate(command.TransactionDateTime);

            Guid statementId = GuidCalculator.Combine(command.MerchantId, nextStatementDate.ToGuid());

            Result result = await ApplyUpdates(
                async (MerchantStatementAggregate merchantStatementAggregate) => {

                    // Add transaction to statement
                    Transaction transaction = new(command.TransactionId, command.TransactionDateTime, command.TransactionAmount.GetValueOrDefault(0));

                    Guid eventId = IdGenerationService.GenerateEventId(new
                    {
                        command.TransactionId,
                        TransactionAmount = command.TransactionAmount.GetValueOrDefault(0),
                        command.TransactionDateTime,
                    });

                    merchantStatementAggregate.AddTransactionToStatement(statementId, eventId, nextStatementDate, command.EstateId, command.MerchantId, transaction);

                    return Result.Success();
                },
                command.EstateId, statementId, cancellationToken, false);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        #endregion
    }
}
