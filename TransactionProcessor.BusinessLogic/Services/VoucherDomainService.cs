using Shared.Results;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
using TransactionProcessor.Models.Estate;

namespace TransactionProcessor.BusinessLogic.Services;

using Microsoft.EntityFrameworkCore;
using Models;
using NetBarcode;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EntityFramework;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using SimpleResults;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
    private readonly IDbContextResolver<EstateManagementContext> Resolver;
    private static readonly String EstateManagementDatabaseName = "TransactionProcessorReadModel";

    private readonly IAggregateService AggregateService;

    private const String ConnectionStringIdentifier = "EstateReportingReadModel";

    #region Constructors

    public VoucherDomainService(IAggregateService aggregateService,
                                IDbContextResolver<EstateManagementContext> resolver)
    {
        this.AggregateService = aggregateService;
        this.Resolver = resolver;
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
            Result<VoucherAggregate> getVoucherResult = await this.AggregateService.GetLatest<VoucherAggregate>(voucherId, cancellationToken);
            Result<VoucherAggregate> voucherAggregateResult =
                DomainServiceHelper.HandleGetAggregateResult(getVoucherResult, voucherId, isNotFoundError);

            if (voucherAggregateResult.IsFailed)
                return ResultHelpers.CreateFailure(voucherAggregateResult);

            VoucherAggregate voucherAggregate = voucherAggregateResult.Data;
            Result<T> result = await action(voucherAggregate);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            Result saveResult = await this.AggregateService.Save(voucherAggregate, cancellationToken);
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
        using ResolvedDbContext<EstateManagementContext>? resolvedContext = this.Resolver.Resolve(EstateManagementDatabaseName, estateId.ToString());
        await using EstateManagementContext context = resolvedContext.Context;

        TransactionProcessor.Database.Entities.VoucherProjectionState voucher = await context.VoucherProjectionStates.SingleOrDefaultAsync(v => v.VoucherCode == voucherCode, cancellationToken);

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
            await this.AggregateService.Save(voucherAggregate, cancellationToken);

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

    
    private async Task<Result> ValidateVoucherIssue(Guid estateId, Guid operatorId, CancellationToken cancellationToken)
    {
        // Validate the Estate Record is a valid estate
        Result<EstateAggregate> getEstateResult = await this.AggregateService.Get<EstateAggregate>(estateId, cancellationToken);
        if (getEstateResult.IsFailed)
        {
            return ResultHelpers.CreateFailure(getEstateResult);
        }
        EstateAggregate estateAggregate = getEstateResult.Data;

        Estate estate = estateAggregate.GetEstate();
        if (estate.Operators == null || estate.Operators.Any() == false)
        {
            return Result.NotFound($"Estate {estate.Name} has no operators defined");
        }

        Operator estateOperator = estate.Operators.SingleOrDefault(o => o.OperatorId == operatorId);
        if (estateOperator == null)
        {
            return Result.NotFound($"Operator Identifier [{operatorId}] is not a valid for estate [{estate.Name}]");
        }

        return Result.Success();
    }

    private async Task<Result> ValidateVoucherRedemption(Guid estateId, CancellationToken cancellationToken)
    {
        // Validate the Estate Record is a valid estate
        Result<EstateAggregate> getEstateResult = await this.AggregateService.Get<EstateAggregate>(estateId, cancellationToken);
        if (getEstateResult.IsFailed) {
            return ResultHelpers.CreateFailure(getEstateResult);
        }

        return Result.Success();
    }

    #endregion
}