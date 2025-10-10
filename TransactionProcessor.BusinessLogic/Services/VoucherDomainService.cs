using Shared.Results;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
using TransactionProcessor.Models.Estate;

namespace TransactionProcessor.BusinessLogic.Services;

using Microsoft.EntityFrameworkCore;
using Models;
using NetBarcode;
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

    Task<Result<IssueVoucherResponse>> IssueVoucher(VoucherCommands.IssueVoucherCommand command,
                                                        CancellationToken cancellationToken);

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
    
    #region Constructors

    public VoucherDomainService(Func<IAggregateService> aggregateService,
                                IDbContextResolver<EstateManagementContext> resolver)
    {
        this.AggregateService = aggregateService();
        this.Resolver = resolver;
    }
    #endregion

    #region Methods
    
    public async Task<Result<IssueVoucherResponse>> IssueVoucher(VoucherCommands.IssueVoucherCommand command, CancellationToken cancellationToken) {
        try{
            Result<EstateResponse> validateResult = await this.ValidateVoucherIssue(command.EstateId, command.OperatorId, cancellationToken);
            if (validateResult.IsFailed)
                return ResultHelpers.CreateFailure(validateResult);

            Result<VoucherAggregate> voucherResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<VoucherAggregate>(command.VoucherId, ct), command.VoucherId, cancellationToken, false);
            if (voucherResult.IsFailed)
                return ResultHelpers.CreateFailure(voucherResult);

            VoucherAggregate voucherAggregate = voucherResult.Data;

            Result stateResult = voucherAggregate.Generate(command.OperatorId, command.EstateId, command.TransactionId, command.IssuedDateTime, command.Value);
            if (stateResult.IsFailed)
                return ResultHelpers.CreateFailure(stateResult);

            Models.Voucher voucherModel = voucherAggregate.GetVoucher();

            // Generate the barcode
            Barcode barcode = new Barcode(voucherModel.VoucherCode);
            stateResult = voucherAggregate.AddBarcode(barcode.GetBase64Image());
            if (stateResult.IsFailed)
                return ResultHelpers.CreateFailure(stateResult);
            
            stateResult = voucherAggregate.Issue(command.RecipientEmail, command.RecipientMobile, command.IssuedDateTime);
            if (stateResult.IsFailed)
                return ResultHelpers.CreateFailure(stateResult);

            Result saveResult = await this.AggregateService.Save(voucherAggregate, cancellationToken);
            if (saveResult.IsFailed)
                return ResultHelpers.CreateFailure(saveResult);

            return Result.Success(new IssueVoucherResponse
            {
                ExpiryDate = voucherModel.ExpiryDate,
                Message = voucherModel.Message,
                VoucherCode = voucherModel.VoucherCode,
                VoucherId = command.VoucherId
            });
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.GetExceptionMessages());
        }
    }

    public async Task<Result<RedeemVoucherResponse>> RedeemVoucher(Guid estateId,
                                                           String voucherCode,
                                                           DateTime redeemedDateTime,
                                                           CancellationToken cancellationToken)
    {
        try
        {
            // Find the voucher based on the voucher code
            using ResolvedDbContext<EstateManagementContext>? resolvedContext = this.Resolver.Resolve(EstateManagementDatabaseName, estateId.ToString());
            await using EstateManagementContext context = resolvedContext.Context;

            TransactionProcessor.Database.Entities.VoucherProjectionState voucher = await context.VoucherProjectionStates.SingleOrDefaultAsync(v => v.VoucherCode == voucherCode, cancellationToken);

            if (voucher == null)
            {
                return Result.NotFound($"No voucher found with voucher code [{voucherCode}]");
            }

            Result<EstateResponse> validateResult = await this.ValidateVoucherRedemption(estateId, cancellationToken);
            if (validateResult.IsFailed)
                return ResultHelpers.CreateFailure(validateResult);
            
            Result<VoucherAggregate> voucherResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<VoucherAggregate>(voucher.VoucherId, ct), voucher.VoucherId, cancellationToken);
            if (voucherResult.IsFailed)
                return ResultHelpers.CreateFailure(voucherResult);

            VoucherAggregate voucherAggregate = voucherResult.Data;

            // Redeem the voucher
            Result stateResult = voucherAggregate.Redeem(redeemedDateTime);
            if (stateResult.IsFailed)
                return ResultHelpers.CreateFailure(stateResult);

            Result saveResult = await this.AggregateService.Save(voucherAggregate, cancellationToken);
            if (saveResult.IsFailed)
                return ResultHelpers.CreateFailure(saveResult);

            Models.Voucher voucherModel = voucherAggregate.GetVoucher();

            return Result.Success(new RedeemVoucherResponse
            {
                RemainingBalance = voucherModel.Balance,
                ExpiryDate = voucherModel.ExpiryDate,
                VoucherCode = voucherModel.VoucherCode
            });

        }
        catch (Exception ex)
        {
            return Result.Failure(ex.GetExceptionMessages());
        }
    }
    
    private async Task<Result> ValidateVoucherIssue(Guid estateId, Guid operatorId, CancellationToken cancellationToken)
    {
        // Validate the Estate Record is a valid estate
        Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(estateId, ct), estateId, cancellationToken);
        if (estateResult.IsFailed)
            return ResultHelpers.CreateFailure(estateResult);
        EstateAggregate estateAggregate = estateResult.Data;

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
        Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(estateId, ct), estateId, cancellationToken);
        if (estateResult.IsFailed)
            return ResultHelpers.CreateFailure(estateResult);

        return Result.Success();
    }

    #endregion
}