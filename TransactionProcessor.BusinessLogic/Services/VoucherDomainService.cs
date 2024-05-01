namespace TransactionProcessor.BusinessLogic.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using EstateManagement.Client;
using EstateManagement.Database.Contexts;
using EstateManagement.DataTransferObjects.Responses;
using Microsoft.EntityFrameworkCore;
using Models;
using NetBarcode;
using SecurityService.Client;
using SecurityService.DataTransferObjects.Responses;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shared.General;
using Shared.Logger;
using VoucherAggregate;

public class VoucherDomainService : IVoucherDomainService
{
    /// <summary>
    /// The voucher aggregate repository
    /// </summary>
    private readonly IAggregateRepository<VoucherAggregate, DomainEvent> VoucherAggregateRepository;

    /// <summary>
    /// The security service client
    /// </summary>
    private readonly ISecurityServiceClient SecurityServiceClient;

    /// <summary>
    /// The estate client
    /// </summary>
    private readonly IEstateClient EstateClient;

    /// <summary>
    /// The database context factory
    /// </summary>
    private readonly Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext> DbContextFactory;

    private const String ConnectionStringIdentifier = "EstateReportingReadModel";

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="VoucherDomainService"/> class.
    /// </summary>
    /// <param name="voucherAggregateRepository">The voucher aggregate repository.</param>
    /// <param name="securityServiceClient">The security service client.</param>
    /// <param name="estateClient">The estate client.</param>
    /// <param name="dbContextFactory">The database context factory.</param>
    public VoucherDomainService(IAggregateRepository<VoucherAggregate, DomainEvent> voucherAggregateRepository,
                                ISecurityServiceClient securityServiceClient,
                                IEstateClient estateClient,
                                Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext> dbContextFactory)
    {
        this.VoucherAggregateRepository = voucherAggregateRepository;
        this.SecurityServiceClient = securityServiceClient;
        this.EstateClient = estateClient;
        this.DbContextFactory = dbContextFactory;
    }
    #endregion

    #region Methods

    /// <summary>
    /// Issues the voucher.
    /// </summary>
    /// <param name="voucherId">The voucher identifier.</param>
    /// <param name="operatorId">The operator identifier.</param>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <param name="issuedDateTime">The issued date time.</param>
    /// <param name="value">The value.</param>
    /// <param name="recipientEmail">The recipient email.</param>
    /// <param name="recipientMobile">The recipient mobile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<IssueVoucherResponse> IssueVoucher(Guid voucherId, Guid operatorId, Guid estateId,
                                                         Guid transactionId,
                                                         DateTime issuedDateTime,
                                                         Decimal value,
                                                         String recipientEmail, String recipientMobile, CancellationToken cancellationToken)
    {
        await this.ValidateVoucherIssue(estateId, operatorId, cancellationToken);

        VoucherAggregate voucher = await this.VoucherAggregateRepository.GetLatestVersion(voucherId, cancellationToken);

        voucher.Generate(operatorId, estateId, transactionId, issuedDateTime, value);

        Voucher voucherModel = voucher.GetVoucher();

        // Generate the barcode
        Barcode barcode = new Barcode(voucherModel.VoucherCode);
        voucher.AddBarcode(barcode.GetBase64Image());
        voucher.Issue(recipientEmail, recipientMobile, issuedDateTime);

        await this.VoucherAggregateRepository.SaveChanges(voucher, cancellationToken);

        return new IssueVoucherResponse
               {
                   ExpiryDate = voucherModel.ExpiryDate,
                   Message = voucherModel.Message,
                   VoucherCode = voucherModel.VoucherCode,
                   VoucherId = voucherId
               };
    }

    /// <summary>
    /// Redeems the voucher.
    /// </summary>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="voucherCode">The voucher code.</param>
    /// <param name="redeemedDateTime">The redeemed date time.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">No voucher found with voucher code [{voucherCode}]</exception>
    public async Task<RedeemVoucherResponse> RedeemVoucher(Guid estateId,
                                                           String voucherCode,
                                                           DateTime redeemedDateTime,
                                                           CancellationToken cancellationToken)
    {
        await this.ValidateVoucherRedemption(estateId, cancellationToken);

        // Find the voucher based on the voucher code
        EstateManagementGenericContext context = await this.DbContextFactory.GetContext(estateId, VoucherDomainService.ConnectionStringIdentifier, cancellationToken);

        EstateManagement.Database.Entities.Voucher voucher = await context.Vouchers.SingleOrDefaultAsync(v => v.VoucherCode == voucherCode, cancellationToken);

        if (voucher == null)
        {
            throw new NotFoundException($"No voucher found with voucher code [{voucherCode}]");
        }

        // Now get the aggregate
        VoucherAggregate voucherAggregate = await this.VoucherAggregateRepository.GetLatestVersion(voucher.VoucherId, cancellationToken);

        // Redeem the voucher
        voucherAggregate.Redeem(redeemedDateTime);

        // Save the changes
        await this.VoucherAggregateRepository.SaveChanges(voucherAggregate, cancellationToken);

        Voucher voucherModel = voucherAggregate.GetVoucher();
        
        return new RedeemVoucherResponse
               {
                   RemainingBalance = voucherModel.Balance,
                   ExpiryDate = voucherModel.ExpiryDate,
                   VoucherCode = voucherModel.VoucherCode
               };
    }

    /// <summary>
    /// Validates the voucher issue.
    /// </summary>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="operatorIdentifier">The operator identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">
    /// Estate Id [{estateId}] is not a valid estate
    /// or
    /// Estate {estate.EstateName} has no operators defined
    /// or
    /// Operator Identifier [{operatorIdentifier}] is not a valid for estate [{estate.EstateName}]
    /// </exception>
    /// <exception cref="System.Exception">Estate Id [{estateId}] is not a valid estate
    /// or
    /// Operator Identifier [{operatorIdentifier}] is not a valid for estate [{estate.EstateName}]</exception>
    private async Task<EstateResponse> ValidateVoucherIssue(Guid estateId, Guid operatorId, CancellationToken cancellationToken)
    {
        EstateResponse estate = null;

        // Validate the Estate Record is a valid estate
        try
        {
            estate = await this.GetEstate(estateId, cancellationToken);
        }
        catch (Exception ex) when (ex.InnerException != null && ex.InnerException.GetType() == typeof(KeyNotFoundException))
        {
            throw new NotFoundException($"Estate Id [{estateId}] is not a valid estate");
        }

        if (estate.Operators == null || estate.Operators.Any() == false)
        {
            throw new NotFoundException($"Estate {estate.EstateName} has no operators defined");
        }

        EstateOperatorResponse estateOperator = estate.Operators.SingleOrDefault(o => o.OperatorId == operatorId);
        if (estateOperator == null)
        {
            throw new NotFoundException($"Operator Identifier [{operatorId}] is not a valid for estate [{estate.EstateName}]");
        }

        return estate;
    }

    /// <summary>
    /// Validates the voucher redemption.
    /// </summary>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">Estate Id [{estateId}] is not a valid estate</exception>
    private async Task<EstateResponse> ValidateVoucherRedemption(Guid estateId, CancellationToken cancellationToken)
    {
        EstateResponse estate = null;

        // Validate the Estate Record is a valid estate
        try
        {
            estate = await this.GetEstate(estateId, cancellationToken);
        }
        catch (Exception ex) when (ex.InnerException != null && ex.InnerException.GetType() == typeof(KeyNotFoundException))
        {
            throw new NotFoundException($"Estate Id [{estateId}] is not a valid estate");
        }

        return estate;
    }

    /// <summary>
    /// The token response
    /// </summary>
    private TokenResponse TokenResponse;

    /// <summary>
    /// Gets the estate.
    /// </summary>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    private async Task<EstateResponse> GetEstate(Guid estateId,
                                                 CancellationToken cancellationToken)
    {
        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        EstateResponse estate = await this.EstateClient.GetEstate(this.TokenResponse.AccessToken, estateId, cancellationToken);

        return estate;
    }
    
    #endregion
}