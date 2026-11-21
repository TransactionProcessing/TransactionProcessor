using TransactionProcessing.UnionResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
using TransactionProcessor.Models.Estate;

namespace TransactionProcessor.BusinessLogic.Services;

using Microsoft.EntityFrameworkCore;
using Models;
using TransactionProcessing.Shared.EntityFramework;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.Exceptions;
using TransactionProcessing.UnionResults;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public interface IVoucherDomainService
{
    #region Methods

    Task<Result<IssueVoucherResponse>> IssueVoucher(VoucherCommands.IssueVoucherCommand command,
                                                        CancellationToken cancellationToken);

    Task<Result<RedeemVoucherResponse>> RedeemVoucher(VoucherCommands.RedeemVoucherCommand command,
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
            Result validateResult = await this.ValidateVoucherIssue(command.EstateId, command.OperatorId, cancellationToken);
            if (validateResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(validateResult);

            Guid voucherId = IdGenerationService.GenerateVoucherId(command.EstateId, command.OperatorId, command.TransactionId);
            
            Result<VoucherAggregate> voucherResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<VoucherAggregate>(voucherId, ct), voucherId, cancellationToken, false);
            if (voucherResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(voucherResult);

            VoucherAggregate voucherAggregate = voucherResult.Data;

            Result stateResult = voucherAggregate.Generate(command.OperatorId, command.EstateId, command.TransactionId, command.IssuedDateTime, command.Value);
            if (stateResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(stateResult);

            Models.Voucher voucherModel = voucherAggregate.GetVoucher();

            if (voucherModel.IsGenerated)
                // return the current voucher as its already generated
                return Results.Ok(new IssueVoucherResponse {
                                                               ExpiryDate = voucherModel.ExpiryDate,
                                                               Message = voucherModel.Message,
                                                               VoucherCode = voucherModel.VoucherCode,
                                                               VoucherId = voucherId
                                                           });

            // Generate the barcode
            String barcode = VoucherBarcodeGenerator.GenerateBase64Png(voucherModel.VoucherCode);
            stateResult = voucherAggregate.AddBarcode(barcode);
            if (stateResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(stateResult);
            
            stateResult = voucherAggregate.Issue(command.RecipientEmail, command.RecipientMobile, command.IssuedDateTime);
            if (stateResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(stateResult);

            Result saveResult = await this.AggregateService.Save(voucherAggregate, cancellationToken);
            if (saveResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(saveResult);

            return Results.Ok(new IssueVoucherResponse
            {
                ExpiryDate = voucherModel.ExpiryDate,
                Message = voucherModel.Message,
                VoucherCode = voucherModel.VoucherCode,
                VoucherId = voucherId
            });
        }
        catch (Exception ex)
        {
            return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
        }
    }

    public async Task<Result<RedeemVoucherResponse>> RedeemVoucher(VoucherCommands.RedeemVoucherCommand command, 
                                                           CancellationToken cancellationToken)
    {
        try
        {
            DateTimeOffset redeemedDateTime = command.RedeemVoucherRequest.RedeemedDateTime.HasValue ? command.RedeemVoucherRequest.RedeemedDateTime.Value : DateTimeOffset.UtcNow;

            // Find the voucher based on the voucher code
            using ResolvedDbContext<EstateManagementContext>? resolvedContext = this.Resolver.Resolve(EstateManagementDatabaseName, command.EstateId.ToString());
            await using EstateManagementContext context = resolvedContext.Context;

            TransactionProcessor.Database.Entities.VoucherProjectionState voucher = await context.VoucherProjectionStates.SingleOrDefaultAsync(v => v.VoucherCode == command.VoucherCode, cancellationToken);

            if (voucher == null)
            {
                return new Error(ErrorType.NotFound, $"No voucher found with voucher code [{command.VoucherCode}]");
            }

            Result validateResult = await this.ValidateVoucherRedemption(command.EstateId, cancellationToken);
            if (validateResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(validateResult);
            
            Result<VoucherAggregate> voucherResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<VoucherAggregate>(voucher.VoucherId, ct), voucher.VoucherId, cancellationToken);
            if (voucherResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(voucherResult);

            VoucherAggregate voucherAggregate = voucherResult.Data;

            // Redeem the voucher
            Result stateResult = voucherAggregate.Redeem(redeemedDateTime, command.RedeemVoucherRequest.HostDateTime);
            if (stateResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(stateResult);

            Result saveResult = await this.AggregateService.Save(voucherAggregate, cancellationToken);
            if (saveResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(saveResult);

            Models.Voucher voucherModel = voucherAggregate.GetVoucher();

            return Results.Ok(new RedeemVoucherResponse
            {
                RemainingBalance = voucherModel.Balance,
                ExpiryDate = voucherModel.ExpiryDate,
                VoucherCode = voucherModel.VoucherCode
            });

        }
        catch (Exception ex)
        {
            return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
        }
    }
    
    private async Task<Result> ValidateVoucherIssue(Guid estateId, Guid operatorId, CancellationToken cancellationToken)
    {
        // Validate the Estate Record is a valid estate
        Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(estateId, ct), estateId, cancellationToken);
        if (estateResult.IsSuccess() == false)
            return ResultHelpers.CreateFailure(estateResult);
        EstateAggregate estateAggregate = estateResult.Data;

        Estate estate = estateAggregate.GetEstate();
        if (estate.Operators == null || estate.Operators.Any() == false)
        {
            return new Error(ErrorType.NotFound, $"Estate {estate.Name} has no operators defined");
        }

        Operator estateOperator = estate.Operators.SingleOrDefault(o => o.OperatorId == operatorId);
        if (estateOperator == null)
        {
            return new Error(ErrorType.NotFound, $"Operator Identifier [{operatorId}] is not a valid for estate [{estate.Name}]");
        }

        return Results.Ok();
    }

    private async Task<Result> ValidateVoucherRedemption(Guid estateId, CancellationToken cancellationToken)
    {
        // Validate the Estate Record is a valid estate
        Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(estateId, ct), estateId, cancellationToken);
        if (estateResult.IsSuccess() == false)
            return ResultHelpers.CreateFailure(estateResult);

        return Results.Ok();
    }

    #endregion
}
