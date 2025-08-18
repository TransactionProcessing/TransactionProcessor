using Google.Protobuf.Reflection;
using MessagingService.Client;
using MessagingService.DataTransferObjects;
using SecurityService.Client;
using SecurityService.DataTransferObjects.Responses;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shared.General;
using Shared.Logger;
using Shared.Results;
using Shared.ValueObjects;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        Task<Result> BuildStatement(MerchantStatementCommands.BuildMerchantStatementCommand command, CancellationToken cancellationToken);
        Task<Result> RecordActivityDateOnMerchantStatement(MerchantStatementCommands.RecordActivityDateOnMerchantStatementCommand command, CancellationToken cancellationToken);

        Task<Result> AddDepositToStatement(MerchantStatementCommands.AddDepositToMerchantStatementCommand command, CancellationToken cancellationToken);
        Task<Result> AddWithdrawalToStatement(MerchantStatementCommands.AddWithdrawalToMerchantStatementCommand command, CancellationToken cancellationToken);

        #endregion
    }

    public class MerchantStatementDomainService : IMerchantStatementDomainService
    {
        private readonly IAggregateService AggregateService;
        private readonly IStatementBuilder StatementBuilder;
        private readonly IMessagingServiceClient MessagingServiceClient;
        private readonly ISecurityServiceClient SecurityServiceClient;

        #region Constructors

        public MerchantStatementDomainService(Func<IAggregateService> aggregateService, IStatementBuilder statementBuilder,
                                              IMessagingServiceClient messagingServiceClient, ISecurityServiceClient securityServiceClient) {
            this.AggregateService = aggregateService();
            this.StatementBuilder = statementBuilder;
            this.MessagingServiceClient = messagingServiceClient;
            this.SecurityServiceClient = securityServiceClient;
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

            Guid merchantStatementId = IdGenerationService.GenerateMerchantStatementAggregateId(command.EstateId, command.MerchantId, nextStatementDate);
            Guid merchantStatementForDateId = IdGenerationService.GenerateMerchantStatementForDateAggregateId(command.EstateId, command.MerchantId, nextStatementDate, command.SettledDateTime);
            Guid settlementFeeId = GuidCalculator.Combine(command.TransactionId, command.SettledFeeId);

            Result result = await ApplyUpdates(
                async (MerchantStatementForDateAggregate merchantStatementForDateAggregate) => {

                    SettledFee settledFee = new SettledFee(settlementFeeId, command.TransactionId, command.SettledDateTime, command.SettledAmount.Value);

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
                        var depositsResult = merchantStatementForDateAggregate.GetStatementLines()
                            .Where(sl => sl.LineType == 3)
                            .Aggregate(new { Count = 0, TotalAmount = 0m },
                                (acc, sl) => new { Count = acc.Count + 1, TotalAmount = acc.TotalAmount + sl.Amount });
                        var withdrawalsResult = merchantStatementForDateAggregate.GetStatementLines()
                            .Where(sl => sl.LineType == 4)
                            .Aggregate(new { Count = 0, TotalAmount = 0m },
                                (acc, sl) => new { Count = acc.Count + 1, TotalAmount = acc.TotalAmount + sl.Amount });
                        merchantStatementAggregate.AddDailySummaryRecord(merchantStatementForDateAggregate.ActivityDate, 
                            transactionsResult.Count, transactionsResult.TotalAmount, 
                            settledFeesResult.Count, settledFeesResult.TotalAmount,
                            depositsResult.Count,depositsResult.TotalAmount,
                            withdrawalsResult.Count, withdrawalsResult.TotalAmount);
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
                                                 CancellationToken cancellationToken) {
            Result result = await ApplyUpdates(async (MerchantStatementAggregate merchantStatementAggregate) => {
                MerchantStatement statement = merchantStatementAggregate.GetStatement();
                // Get the merchant
                Result<MerchantAggregate> getMerchantResult = await this.AggregateService.Get<MerchantAggregate>(statement.MerchantId, cancellationToken);
                if (getMerchantResult.IsFailed)
                    return ResultHelpers.CreateFailure(getMerchantResult);
                Merchant merchantModel = getMerchantResult.Data.GetMerchant();
                List<String> emailAddresses = merchantModel.Contacts.Select(c => c.ContactEmailAddress).ToList();
                
                SendEmailRequest sendEmailRequest = new SendEmailRequest
                {
                    Body = "<html><body>Please find attached this months statement.</body></html>",
                    ConnectionIdentifier = command.EstateId,
                    FromAddress = "golfhandicapping@btinternet.com", // TODO: lookup from config
                    IsHtml = true,
                    Subject = $"Merchant Statement for {statement.StatementDate}",
                    ToAddresses = emailAddresses,
                    EmailAttachments = new List<EmailAttachment>
                    {
                        new EmailAttachment
                        {
                            FileData = command.pdfData,
                            FileType = FileType.PDF,
                            Filename = $"merchantstatement{statement.StatementDate}.pdf"
                        }
                    }
                };

                Guid messageId = IdGenerationService.GenerateEventId(new
                {
                    command.MerchantStatementId,
                    DateTime.Now
                });

                sendEmailRequest.MessageId = messageId;

                var getTokenResult = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
                if (getTokenResult.IsFailed)
                    return ResultHelpers.CreateFailure(getTokenResult);
                this.TokenResponse = getTokenResult.Data;

                var sendEmailResponseResult = await this.MessagingServiceClient.SendEmail(this.TokenResponse.AccessToken, sendEmailRequest, cancellationToken);
                //if (sendEmailResponseResult.IsFailed) {
                //    // TODO: record a failed event??
                //}

                merchantStatementAggregate.EmailStatement(DateTime.Now, messageId);

                return Result.Success();
            }, command.MerchantStatementId, cancellationToken, false);

            return result;
        }
        private TokenResponse TokenResponse;

        [ExcludeFromCodeCoverage]
        static String EncodeTo64(String toEncode)
        {
            Byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(toEncode);
            String returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public async Task<Result> BuildStatement(MerchantStatementCommands.BuildMerchantStatementCommand command,
                                                 CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async (MerchantStatementAggregate merchantStatementAggregate) => {

                    MerchantStatement statement = merchantStatementAggregate.GetStatement();
                    Result<MerchantAggregate> getMerchantResult = await this.AggregateService.Get<MerchantAggregate>(statement.MerchantId, cancellationToken);

                    if (getMerchantResult.IsFailed)
                        return ResultHelpers.CreateFailure(getMerchantResult);
                    MerchantAggregate merchantAggregate = getMerchantResult.Data;
                    Merchant m = merchantAggregate.GetMerchant();

                    String html = await this.StatementBuilder.GetStatementHtml(merchantStatementAggregate, m, cancellationToken);
                    // TODO: Record the html to the statement aggregate so we can use it later if needed

                    String base64 = EncodeTo64(html);

                    merchantStatementAggregate.BuildStatement(DateTime.Now, base64);

                    return Result.Success();
             },
                command.MerchantStatementId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

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

        public async Task<Result> AddDepositToStatement(MerchantStatementCommands.AddDepositToMerchantStatementCommand command,
                                                        CancellationToken cancellationToken) {
            // Work out the next statement date
            DateTime nextStatementDate = CalculateStatementDate(command.DepositDateTime.Date);

            Guid merchantStatementId = IdGenerationService.GenerateMerchantStatementAggregateId(command.EstateId, command.MerchantId, nextStatementDate);
            Guid merchantStatementForDateId = IdGenerationService.GenerateMerchantStatementForDateAggregateId(command.EstateId, command.MerchantId, nextStatementDate, command.DepositDateTime.Date);
            
            Result result = await ApplyUpdates(
                async (MerchantStatementForDateAggregate merchantStatementForDateAggregate) => {

                    Deposit deposit = new Deposit { DepositId = command.DepositId, Reference = command.Reference, DepositDateTime = command.DepositDateTime, Amount = command.Amount.Value };
                    
                    Guid eventId = IdGenerationService.GenerateEventId(new
                    {
                        command.DepositId,
                        deposit,
                        command.DepositDateTime,
                    });

                    merchantStatementForDateAggregate.AddDepositToStatement(merchantStatementId, nextStatementDate, eventId, command.EstateId, command.MerchantId, deposit);

                    return Result.Success();
                }, merchantStatementForDateId, cancellationToken, false);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> AddWithdrawalToStatement(MerchantStatementCommands.AddWithdrawalToMerchantStatementCommand command,
                                                           CancellationToken cancellationToken) {
            // Work out the next statement date
            DateTime nextStatementDate = CalculateStatementDate(command.WithdrawalDateTime.Date);

            Guid merchantStatementId = IdGenerationService.GenerateMerchantStatementAggregateId(command.EstateId, command.MerchantId, nextStatementDate);
            Guid merchantStatementForDateId = IdGenerationService.GenerateMerchantStatementForDateAggregateId(command.EstateId, command.MerchantId, nextStatementDate, command.WithdrawalDateTime.Date);

            Result result = await ApplyUpdates(
                async (MerchantStatementForDateAggregate merchantStatementForDateAggregate) => {

                    Withdrawal withdrawal = new Withdrawal() { WithdrawalId = command.WithdrawalId, WithdrawalDateTime = command.WithdrawalDateTime, Amount = command.Amount.Value };

                    Guid eventId = IdGenerationService.GenerateEventId(new
                    {
                        command.WithdrawalId,
                        withdrawal,
                        command.WithdrawalDateTime,
                    });

                    merchantStatementForDateAggregate.AddWithdrawalToStatement(merchantStatementId, nextStatementDate, eventId, command.EstateId, command.MerchantId, withdrawal);

                    return Result.Success();
                }, merchantStatementForDateId, cancellationToken, false);

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
            if (command.TransactionAmount == null)
                return Result.Success();

            // Work out the next statement date
            DateTime nextStatementDate = CalculateStatementDate(command.TransactionDateTime);
            
            Guid merchantStatementId = IdGenerationService.GenerateMerchantStatementAggregateId(command.EstateId, command.MerchantId, nextStatementDate);
            Guid merchantStatementForDateId = IdGenerationService.GenerateMerchantStatementForDateAggregateId(command.EstateId, command.MerchantId, nextStatementDate, command.TransactionDateTime);

            Result result = await ApplyUpdates(
                async (MerchantStatementForDateAggregate merchantStatementForDateAggregate) => {

                    // Add transaction to statement
                    Transaction transaction = new(command.TransactionId, command.TransactionDateTime, command.TransactionAmount.Value);

                    Guid eventId = IdGenerationService.GenerateEventId(new
                    {
                        command.TransactionId,
                        TransactionAmount = command.TransactionAmount.Value,
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
