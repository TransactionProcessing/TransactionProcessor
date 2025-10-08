using Shared.Exceptions;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.BusinessLogic.EventHandling;

using Common;
using MessagingService.Client;
using MessagingService.DataTransferObjects;
using Microsoft.EntityFrameworkCore;
using SecurityService.Client;
using SecurityService.DataTransferObjects.Responses;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EntityFramework;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventHandling;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public class VoucherDomainEventHandler : IDomainEventHandler
{
    #region Fields

    /// <summary>
    /// The file system
    /// </summary>
    private readonly IFileSystem FileSystem;

    /// <summary>
    /// The messaging service client
    /// </summary>
    private readonly IMessagingServiceClient MessagingServiceClient;

    /// <summary>
    /// The security service client
    /// </summary>
    private readonly ISecurityServiceClient SecurityServiceClient;

    private readonly IAggregateService AggregateService;
    private readonly IDbContextResolver<EstateManagementContext> Resolver;

    private TokenResponse TokenResponse;

    private static readonly String EstateManagementDatabaseName = "TransactionProcessorReadModel";
    #endregion

    #region Constructors

    public VoucherDomainEventHandler(ISecurityServiceClient securityServiceClient,
                                     IAggregateService aggregateService,
                                     IDbContextResolver<EstateManagementContext> resolver,
                                     IMessagingServiceClient messagingServiceClient,
                                     IFileSystem fileSystem)
    {
        this.SecurityServiceClient = securityServiceClient;
        this.AggregateService = aggregateService;
        this.Resolver = resolver;
        this.MessagingServiceClient = messagingServiceClient;
        this.FileSystem = fileSystem;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the email voucher message.
    /// </summary>
    /// <param name="voucherModel">The voucher model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<String> GetEmailVoucherMessage(Models.Voucher voucherModel,
                                                     CancellationToken cancellationToken)
    {
        IDirectoryInfo path = this.FileSystem.Directory.GetParent(Assembly.GetExecutingAssembly().Location);

        String fileData = await this.FileSystem.File.ReadAllTextAsync($"{path}/VoucherMessages/VoucherEmail.html", cancellationToken);

        PropertyInfo[] voucherProperties = voucherModel.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Do the replaces for the transaction
        foreach (PropertyInfo propertyInfo in voucherProperties)
        {
            fileData = fileData.Replace($"[{propertyInfo.Name}]", propertyInfo.GetValue(voucherModel)?.ToString());
        }

        String voucherOperator = await this.GetVoucherOperator(voucherModel, cancellationToken);

        fileData = fileData.Replace("[OperatorIdentifier]", voucherOperator);

        return fileData;
    }

    /// <summary>
    /// Handles the specified domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<Result> Handle(IDomainEvent domainEvent,
                                     CancellationToken cancellationToken)
    {
        return await this.HandleSpecificDomainEvent((dynamic)domainEvent, cancellationToken);
    }
    
    /// <summary>
    /// Gets the voucher operator.
    /// </summary>
    /// <param name="voucherModel">The voucher model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    private async Task<String> GetVoucherOperator(Models.Voucher voucherModel,
                                                  CancellationToken cancellationToken)
    {
        using ResolvedDbContext<EstateManagementContext>? resolvedContext = this.Resolver.Resolve(EstateManagementDatabaseName, voucherModel.EstateId.ToString());
        await using EstateManagementContext context = resolvedContext.Context;

        Database.Entities.Transaction transaction = await context.Transactions.SingleOrDefaultAsync(t => t.TransactionId == voucherModel.TransactionId, cancellationToken);
        if (transaction == null)
        {
            throw new NotFoundException($"Transaction not found Transaction Id {voucherModel.TransactionId}");
        }

        Contract contract = await context.Contracts.SingleOrDefaultAsync(c => c.ContractId == transaction.ContractId, cancellationToken);

        return contract.Description;
    }

    /// <summary>
    /// Handles the specific domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task<Result> HandleSpecificDomainEvent(VoucherDomainEvents.VoucherIssuedEvent domainEvent,
                                                         CancellationToken cancellationToken)
    {
        // Get the voucher aggregate
        Result<VoucherAggregate> voucherAggregateResult = await this.AggregateService.Get<VoucherAggregate>(domainEvent.VoucherId, cancellationToken);
        if (voucherAggregateResult.IsFailed)
            return ResultHelpers.CreateFailure(voucherAggregateResult);

        Models.Voucher voucherModel = voucherAggregateResult.Data.GetVoucher();
        Result<TokenResponse> getTokenResult = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
        if (getTokenResult.IsFailed)
            return ResultHelpers.CreateFailure(getTokenResult);
        this.TokenResponse = getTokenResult.Data;
        if (string.IsNullOrEmpty(voucherModel.RecipientEmail) == false)
        {
            String message = await this.GetEmailVoucherMessage(voucherModel, cancellationToken);

            SendEmailRequest request = new SendEmailRequest
                                       {
                                           Body = message,
                                           ConnectionIdentifier = domainEvent.EstateId,
                                           FromAddress = "golfhandicapping@btinternet.com", // TODO: lookup from config
                                           IsHtml = true,
                                           MessageId = domainEvent.EventId,
                                           Subject = "Voucher Issue",
                                           ToAddresses = new List<String>
                                                         {
                                                             voucherModel.RecipientEmail
                                                         }
                                       };

            return await this.MessagingServiceClient.SendEmail(this.TokenResponse.AccessToken, request, cancellationToken);
        }

        if (String.IsNullOrEmpty(voucherModel.RecipientMobile) == false)
        {
            String message = await this.GetSMSVoucherMessage(voucherModel, cancellationToken);

            SendSMSRequest request = new SendSMSRequest
                                     {
                                         ConnectionIdentifier = domainEvent.EstateId,
                                         Destination = domainEvent.RecipientMobile,
                                         Message = message,
                                         MessageId = domainEvent.EventId,
                                         Sender = "Your Voucher"
                                     };

            return await this.MessagingServiceClient.SendSMS(this.TokenResponse.AccessToken, request, cancellationToken);
        }
        // No messages to be sent
        return Result.Success();
    }

    /// <summary>
    /// Gets the SMS voucher message.
    /// </summary>
    /// <param name="voucherModel">The voucher model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    private async Task<String> GetSMSVoucherMessage(Models.Voucher voucherModel,
                                                    CancellationToken cancellationToken)
    {
        IDirectoryInfo path = this.FileSystem.Directory.GetParent(Assembly.GetExecutingAssembly().Location);

        String fileData = await this.FileSystem.File.ReadAllTextAsync($"{path}/VoucherMessages/VoucherSMS.txt", cancellationToken);

        PropertyInfo[] voucherProperties = voucherModel.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Do the replaces for the transaction
        foreach (PropertyInfo propertyInfo in voucherProperties)
        {
            fileData = fileData.Replace($"[{propertyInfo.Name}]", propertyInfo.GetValue(voucherModel)?.ToString());
        }

        String voucherOperator = await this.GetVoucherOperator(voucherModel, cancellationToken);

        fileData = fileData.Replace("[OperatorIdentifier]", voucherOperator);

        return fileData;
    }

    #endregion
}