using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json;
using Shared.EventStore.EventStore;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.ProjectionEngine.Models;
using TransactionProcessor.ProjectionEngine.Repository;
using TransactionProcessor.ProjectionEngine.State;
using Merchant = TransactionProcessor.Models.Merchant.Merchant;

namespace TransactionProcessor.BusinessLogic.RequestHandlers;

public class MerchantBalanceRequestHandler :
    IRequestHandler<MerchantBalanceCommands.RecordDepositCommand, Result>,
    IRequestHandler<MerchantBalanceCommands.RecordWithdrawalCommand, Result>,
    IRequestHandler<MerchantBalanceCommands.RecordAuthorisedSaleCommand, Result>,
    IRequestHandler<MerchantBalanceCommands.RecordDeclinedSaleCommand, Result>,
    IRequestHandler<MerchantBalanceCommands.RecordSettledFeeCommand, Result>
{
    private readonly IMerchantDomainService MerchantDomainService;

    public MerchantBalanceRequestHandler(IMerchantDomainService merchantDomainService) {
        this.MerchantDomainService = merchantDomainService;
    }
    
    public async Task<Result> Handle(MerchantBalanceCommands.RecordDepositCommand command,
                                     CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.RecordDepositAgainstBalance(command, cancellationToken);
    }
    
    public async Task<Result> Handle(MerchantBalanceCommands.RecordWithdrawalCommand command,
                                     CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.RecordWithdrawalAgainstBalance(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantBalanceCommands.RecordAuthorisedSaleCommand request,
                                     CancellationToken cancellationToken) {
        return await this.MerchantDomainService.RecordTransactionAgainstBalance(request, cancellationToken);
    }

    public async Task<Result> Handle(MerchantBalanceCommands.RecordDeclinedSaleCommand request,
                                     CancellationToken cancellationToken) {
        return await this.MerchantDomainService.RecordTransactionAgainstBalance(request, cancellationToken);
    }

    public async Task<Result> Handle(MerchantBalanceCommands.RecordSettledFeeCommand request,
                                     CancellationToken cancellationToken) {
        return await this.MerchantDomainService.RecordSettledFeeAgainstBalance(request, cancellationToken);
    }
}