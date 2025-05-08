using MessagingService.Client;
using SecurityService.Client;
using SecurityService.DataTransferObjects.Responses;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shared.General;
using Shared.Logger;
using Shared.Results;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Reflection;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Aggregates.Models;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Models.Merchant;
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
        Task<Result> RecordActivityDateOnMerchantStatement(MerchantStatementCommands.RecordActivityDateOnMerchantStatementCommand command, CancellationToken cancellationToken);

        #endregion
    }

    public class MerchantStatementDomainService : IMerchantStatementDomainService
    {
        private readonly IAggregateService AggregateService;
        
        #region Constructors

        public MerchantStatementDomainService(IAggregateService aggregateService)
        {
            this.AggregateService = aggregateService;
        }

        #endregion

        #region Methods

        private async Task<Result> ApplyUpdates(Func<MerchantStatementAggregate, Task<Result>> action, Guid statementId, CancellationToken cancellationToken, Boolean isNotFoundError = true)
        {
            try
            {
                Result<MerchantStatementAggregate> getMerchantStatementResult = await this.AggregateService.GetLatest<MerchantStatementAggregate>(statementId, cancellationToken);

                Result<MerchantStatementAggregate> merchantStatementAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getMerchantStatementResult, statementId, isNotFoundError);
                if (merchantStatementAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantStatementAggregateResult);

                MerchantStatementAggregate merchantStatementAggregate = merchantStatementAggregateResult.Data;

                Result result = await action(merchantStatementAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.AggregateService.Save(merchantStatementAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        private async Task<Result> ApplyUpdates(Func<MerchantStatementForDateAggregate, Task<Result>> action, Guid statementId, CancellationToken cancellationToken, Boolean isNotFoundError = true)
        {
            try
            {
                Result<MerchantStatementForDateAggregate> getMerchantStatementResult = await this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(statementId, cancellationToken);

                Result<MerchantStatementForDateAggregate> merchantStatementAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getMerchantStatementResult, statementId, isNotFoundError);
                if (merchantStatementAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantStatementAggregateResult);

                MerchantStatementForDateAggregate merchantStatementAggregate = merchantStatementAggregateResult.Data;

                Result result = await action(merchantStatementAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.AggregateService.Save(merchantStatementAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddSettledFeeToStatement(MerchantStatementCommands.AddSettledFeeToMerchantStatementCommand command,
                                                           CancellationToken cancellationToken)
        {
            // Work out the next statement date
            DateTime nextStatementDate = CalculateStatementDate(command.SettledDateTime);

            if (nextStatementDate.Year == 1) {
                return Result.CriticalError($"Error in statement date generation Generated date is {nextStatementDate}");
            }

            Guid merchantStatementId = IdGenerationService.GenerateMerchantStatementAggregateId(command.EstateId, command.MerchantId, nextStatementDate);
            Guid merchantStatementForDateId = IdGenerationService.GenerateMerchantStatementForDateAggregateId(command.EstateId, command.MerchantId, nextStatementDate, command.SettledDateTime);
            Guid settlementFeeId = GuidCalculator.Combine(command.TransactionId, command.SettledFeeId);

            Result result = await ApplyUpdates(
                async (MerchantStatementForDateAggregate merchantStatementForDateAggregate) => {

                    SettledFee settledFee = new SettledFee(settlementFeeId, command.TransactionId, command.SettledDateTime, command.SettledAmount);

                    Guid eventId = IdGenerationService.GenerateEventId(new
                    {
                        command.TransactionId,
                        settlementFeeId,
                        settledFee,
                        command.SettledDateTime,
                    });

                    merchantStatementForDateAggregate.AddSettledFeeToStatement(merchantStatementId, nextStatementDate, eventId, command.EstateId, command.MerchantId, settledFee);

                    return Result.Success();
                }, merchantStatementForDateId, cancellationToken, false);

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
            DateTime calculatedDateTime = eventDateTime.Date.AddMonths(1);

            return new DateTime(calculatedDateTime.Year, calculatedDateTime.Month, 1);
        }

        public async Task<Result> GenerateStatement(MerchantCommands.GenerateMerchantStatementCommand command, CancellationToken cancellationToken)
        {
            // Need to rebuild the date time from the command as the Kind is Utc which is different from the date time used to generate the statement id
            DateTime dt = new DateTime(command.RequestDto.MerchantStatementDate.Year, command.RequestDto.MerchantStatementDate.Month, command.RequestDto.MerchantStatementDate.Day);
            Guid merchantStatementId = IdGenerationService.GenerateMerchantStatementAggregateId(command.EstateId, command.MerchantId, dt);
            
            Result result = await ApplyUpdates(
                async (MerchantStatementAggregate merchantStatementAggregate) =>
                {
                    MerchantStatement statement = merchantStatementAggregate.GetStatement();
                    List<(Guid merchantStatementForDateId, DateTime activityDate)> activityDates = statement.GetActivityDates();

                    List<MerchantStatementForDate> statementForDateAggregates = new();
                    foreach ((Guid merchantStatementForDateId, DateTime activityDate) activityDate in activityDates)
                    {
                        Result<MerchantStatementForDateAggregate> statementForDateResult = await this.AggregateService.GetLatest<MerchantStatementForDateAggregate>(activityDate.merchantStatementForDateId, cancellationToken);
                        if (statementForDateResult.IsFailed)
                            return ResultHelpers.CreateFailure(statementForDateResult);
                        MerchantStatementForDate dailyStatement = statementForDateResult.Data.GetStatement(true);
                        statementForDateAggregates.Add(dailyStatement);
                    }

                    // Ok so now we have the daily statements we need to add a summary line to the statement aggregate
                    foreach (MerchantStatementForDate merchantStatementForDateAggregate in statementForDateAggregates)
                    {
                        // Build the summary event
                        var transactionsResult = merchantStatementForDateAggregate.GetStatementLines()
                            .Where(sl => sl.LineType == 1)
                            .Aggregate(new { Count = 0, TotalAmount = 0m },
                                (acc, sl) => new { Count = acc.Count + 1, TotalAmount = acc.TotalAmount + sl.Amount });
                        var settledFeesResult = merchantStatementForDateAggregate.GetStatementLines()
                            .Where(sl => sl.LineType == 2)
                            .Aggregate(new { Count = 0, TotalAmount = 0m },
                                (acc, sl) => new { Count = acc.Count + 1, TotalAmount = acc.TotalAmount + sl.Amount });
                        merchantStatementAggregate.AddDailySummaryRecord(merchantStatementForDateAggregate.ActivityDate, transactionsResult.Count, transactionsResult.TotalAmount, settledFeesResult.Count,
                            settledFeesResult.TotalAmount);
                    }

                    merchantStatementAggregate.GenerateStatement(DateTime.Now);

                    return Result.Success();
                },
                merchantStatementId, cancellationToken, false);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> EmailStatement(MerchantStatementCommands.EmailMerchantStatementCommand command,
                                                 CancellationToken cancellationToken)
        {
            //Result result = await ApplyUpdates(
            //    async (MerchantStatementAggregate merchantStatementAggregate) => {

            //        //StatementHeader statementHeader = await this.EstateManagementRepository.GetStatement(command.EstateId, command.MerchantStatementId, cancellationToken);

            //        //String html = await this.StatementBuilder.GetStatementHtml(statementHeader, cancellationToken);

            //        //String base64 = await this.PdfGenerator.CreatePDF(html, cancellationToken);

            //        //SendEmailRequest sendEmailRequest = new SendEmailRequest
            //        //{
            //        //    Body = "<html><body>Please find attached this months statement.</body></html>",
            //        //    ConnectionIdentifier = command.EstateId,
            //        //    FromAddress = "golfhandicapping@btinternet.com", // TODO: lookup from config
            //        //    IsHtml = true,
            //        //    Subject = $"Merchant Statement for {statementHeader.StatementDate}",
            //        //    // MessageId = command.MerchantStatementId,
            //        //    ToAddresses = new List<String>
            //        //    {
            //        //        statementHeader.MerchantEmail
            //        //    },
            //        //    EmailAttachments = new List<EmailAttachment>
            //        //    {
            //        //        new EmailAttachment
            //        //        {
            //        //            FileData = base64,
            //        //            FileType = FileType.PDF,
            //        //            Filename = $"merchantstatement{statementHeader.StatementDate}.pdf"
            //        //        }
            //        //    }
            //        //};

            //        //Guid messageId = IdGenerationService.GenerateEventId(new
            //        //{
            //        //    command.MerchantStatementId,
            //        //    DateTime.Now
            //        //});

            //        //sendEmailRequest.MessageId = messageId;

            //        //this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            //        //var sendEmailResponseResult = await this.MessagingServiceClient.SendEmail(this.TokenResponse.AccessToken, sendEmailRequest, cancellationToken);
            //        ////if (sendEmailResponseResult.IsFailed) {
            //        ////    // TODO: record a failed event??
            //        ////}
            //        //merchantStatementAggregate.EmailStatement(DateTime.Now, messageId);

            //        return Result.Success();
            //    },
            //    command.EstateId, command.MerchantStatementId, cancellationToken);

            //if (result.IsFailed)
            //    return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> RecordActivityDateOnMerchantStatement(MerchantStatementCommands.RecordActivityDateOnMerchantStatementCommand command,
                                                                        CancellationToken cancellationToken) {
            Result result = await ApplyUpdates(
                async (MerchantStatementAggregate merchantStatementAggregate) => {

                    merchantStatementAggregate.RecordActivityDateOnStatement(command.MerchantStatementId, command.StatementDate,
                        command.EstateId, command.MerchantId,
                        command.MerchantStatementForDateId, command.StatementActivityDate);
                    
                    return Result.Success();
                }, command.MerchantStatementId, cancellationToken, false);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

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

            if (nextStatementDate.Year == 1)
            {
                return Result.CriticalError($"Error in statement date generation Generated date is {nextStatementDate}");
            }

            Guid merchantStatementId = IdGenerationService.GenerateMerchantStatementAggregateId(command.EstateId, command.MerchantId, nextStatementDate);
            Guid merchantStatementForDateId = IdGenerationService.GenerateMerchantStatementForDateAggregateId(command.EstateId, command.MerchantId, nextStatementDate, command.TransactionDateTime);

            Result result = await ApplyUpdates(
                async (MerchantStatementForDateAggregate merchantStatementForDateAggregate) => {

                    // Add transaction to statement
                    Transaction transaction = new(command.TransactionId, command.TransactionDateTime, command.TransactionAmount.GetValueOrDefault(0));

                    Guid eventId = IdGenerationService.GenerateEventId(new
                    {
                        command.TransactionId,
                        TransactionAmount = command.TransactionAmount.GetValueOrDefault(0),
                        command.TransactionDateTime,
                    });

                    merchantStatementForDateAggregate.AddTransactionToStatement(merchantStatementId, nextStatementDate, eventId, command.EstateId, command.MerchantId, transaction);

                    return Result.Success();
                }, merchantStatementForDateId, cancellationToken, false);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        #endregion
    }
}
