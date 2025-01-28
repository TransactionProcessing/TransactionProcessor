using Shared.Results;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Database.Contexts;

namespace TransactionProcessor.BusinessLogic.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using EstateManagement.Client;
using EstateManagement.DataTransferObjects.Responses;
using EstateManagement.DataTransferObjects.Responses.Estate;
using Google.Protobuf.WellKnownTypes;
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
using SimpleResults;

public interface IVoucherDomainService
{
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
    Task<Result<IssueVoucherResponse>> IssueVoucher(Guid voucherId,
                                                    Guid operatorId,
                                                    Guid estateId,
                                                    Guid transactionId,
                                                    DateTime issuedDateTime,
                                                    Decimal value,
                                                    String recipientEmail,
                                                    String recipientMobile,
                                                    CancellationToken cancellationToken);

    /// <summary>
    /// Redeems the voucher.
    /// </summary>
    /// <param name="estateId">The estate identifier.</param>
    /// <param name="voucherCode">The voucher code.</param>
    /// <param name="redeemedDateTime">The redeemed date time.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<Result<RedeemVoucherResponse>> RedeemVoucher(Guid estateId,
                                              String voucherCode,
                                              DateTime redeemedDateTime,
                                              CancellationToken cancellationToken);

    #endregion
}

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
    private readonly IIntermediateEstateClient EstateClient;

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
                                IIntermediateEstateClient estateClient,
                                Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext> dbContextFactory)
    {
        this.VoucherAggregateRepository = voucherAggregateRepository;
        this.SecurityServiceClient = securityServiceClient;
        this.EstateClient = estateClient;
        this.DbContextFactory = dbContextFactory;
    }
    #endregion

    #region Methods

    private async Task<Result<T>> ApplyUpdates<T>(Func<VoucherAggregate, Task<Result<T>>> action,
                                                  Guid voucherId,
                                                  CancellationToken cancellationToken,
                                                  Boolean isNotFoundError = true)
    {
        try
        {
            Result<VoucherAggregate> getVoucherResult = await this.VoucherAggregateRepository.GetLatestVersion(voucherId, cancellationToken);
            Result<VoucherAggregate> voucherAggregateResult =
                DomainServiceHelper.HandleGetAggregateResult(getVoucherResult, voucherId, isNotFoundError);

            if (voucherAggregateResult.IsFailed)
                return ResultHelpers.CreateFailure(voucherAggregateResult);

            VoucherAggregate voucherAggregate = voucherAggregateResult.Data;
            Result<T> result = await action(voucherAggregate);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            Result saveResult = await this.VoucherAggregateRepository.SaveChanges(voucherAggregate, cancellationToken);
            if (saveResult.IsFailed)
                return ResultHelpers.CreateFailure(saveResult);
            return Result.Success(result.Data);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.GetExceptionMessages());
        }
    }

    public async Task<Result<IssueVoucherResponse>> IssueVoucher(Guid voucherId, Guid operatorId, Guid estateId,
                                                         Guid transactionId,
                                                         DateTime issuedDateTime,
                                                         Decimal value,
                                                         String recipientEmail, String recipientMobile, CancellationToken cancellationToken) {
        Result<IssueVoucherResponse> result = await ApplyUpdates<IssueVoucherResponse>(
            async (VoucherAggregate voucherAggregate) => {

                Result<EstateResponse> validateResult = await this.ValidateVoucherIssue(estateId, operatorId, cancellationToken);
                if (validateResult.IsFailed)
                    return ResultHelpers.CreateFailure(validateResult);

                voucherAggregate.Generate(operatorId, estateId, transactionId, issuedDateTime, value);

                Models.Voucher voucherModel = voucherAggregate.GetVoucher();

                // Generate the barcode
                Barcode barcode = new Barcode(voucherModel.VoucherCode);
                voucherAggregate.AddBarcode(barcode.GetBase64Image());
                voucherAggregate.Issue(recipientEmail, recipientMobile, issuedDateTime);

                return Result.Success(new IssueVoucherResponse
                {
                    ExpiryDate = voucherModel.ExpiryDate,
                    Message = voucherModel.Message,
                    VoucherCode = voucherModel.VoucherCode,
                    VoucherId = voucherId
                });

            }, voucherId, cancellationToken, false);

        return result;
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
    public async Task<Result<RedeemVoucherResponse>> RedeemVoucher(Guid estateId,
                                                           String voucherCode,
                                                           DateTime redeemedDateTime,
                                                           CancellationToken cancellationToken)
    {
        // Find the voucher based on the voucher code
        EstateManagementGenericContext context = await this.DbContextFactory.GetContext(estateId, VoucherDomainService.ConnectionStringIdentifier, cancellationToken);

        TransactionProcessor.Database.Entities.Voucher voucher = await context.Vouchers.SingleOrDefaultAsync(v => v.VoucherCode == voucherCode, cancellationToken);

        if (voucher == null)
        {
            return Result.NotFound($"No voucher found with voucher code [{voucherCode}]");
        }

        Result<RedeemVoucherResponse> result = await ApplyUpdates<RedeemVoucherResponse>(
        async (VoucherAggregate voucherAggregate) => {
            Result<EstateResponse> validateResult = await this.ValidateVoucherRedemption(estateId, cancellationToken);
            if (validateResult.IsFailed)
                return ResultHelpers.CreateFailure(validateResult);
            
            // Redeem the voucher
            voucherAggregate.Redeem(redeemedDateTime);

            // Save the changes
            await this.VoucherAggregateRepository.SaveChanges(voucherAggregate, cancellationToken);

            Models.Voucher voucherModel = voucherAggregate.GetVoucher();

            return Result.Success(new RedeemVoucherResponse
            {
                RemainingBalance = voucherModel.Balance,
                ExpiryDate = voucherModel.ExpiryDate,
                VoucherCode = voucherModel.VoucherCode
            });

        }, voucher.VoucherId, cancellationToken);

        return result;
    }

    
    private async Task<Result<EstateResponse>> ValidateVoucherIssue(Guid estateId, Guid operatorId, CancellationToken cancellationToken)
    {
        EstateResponse estate = null;

        // Validate the Estate Record is a valid estate
        Result<EstateResponse> getEstateResult = await this.GetEstate(estateId, cancellationToken);
        if (getEstateResult.IsFailed)
            return ResultHelpers.CreateFailure(getEstateResult);

        estate = getEstateResult.Data;
        if (estate.Operators == null || estate.Operators.Any() == false)
        {
            return Result.NotFound($"Estate {estate.EstateName} has no operators defined");
        }

        EstateOperatorResponse estateOperator = estate.Operators.SingleOrDefault(o => o.OperatorId == operatorId);
        if (estateOperator == null)
        {
            return Result.NotFound($"Operator Identifier [{operatorId}] is not a valid for estate [{estate.EstateName}]");
        }

        return getEstateResult;
    }

    private async Task<Result<EstateResponse>> ValidateVoucherRedemption(Guid estateId, CancellationToken cancellationToken)
    {
        // Validate the Estate Record is a valid estate
        Result<EstateResponse> getEstateResult = await this.GetEstate(estateId, cancellationToken);
        if (getEstateResult.IsFailed)
            return ResultHelpers.CreateFailure(getEstateResult);

        return getEstateResult;
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
    private async Task<Result<EstateResponse>> GetEstate(Guid estateId,
                                                 CancellationToken cancellationToken)
    {
        this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

        return await this.EstateClient.GetEstate(this.TokenResponse.AccessToken, estateId, cancellationToken);
    }
    
    #endregion
}