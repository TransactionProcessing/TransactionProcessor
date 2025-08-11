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

public class MerchantRequestHandler :
    IRequestHandler<MerchantQueries.GetMerchantBalanceQuery, Result<MerchantBalanceState>>,
    IRequestHandler<MerchantQueries.GetMerchantLiveBalanceQuery, Result<Decimal>>,
    IRequestHandler<MerchantQueries.GetMerchantBalanceHistoryQuery, Result<List<MerchantBalanceChangedEntry>>>,
IRequestHandler<MerchantCommands.SwapMerchantDeviceCommand, Result>,
                                          IRequestHandler<MerchantCommands.AddMerchantContractCommand, Result>,
                                          IRequestHandler<MerchantCommands.CreateMerchantCommand, Result>,
                                          IRequestHandler<MerchantCommands.AssignOperatorToMerchantCommand, Result>,
                                          IRequestHandler<MerchantCommands.AddMerchantDeviceCommand, Result>,
                                          IRequestHandler<MerchantCommands.CreateMerchantUserCommand, Result>,
                                          IRequestHandler<MerchantCommands.MakeMerchantDepositCommand, Result>,
                                          IRequestHandler<MerchantCommands.MakeMerchantWithdrawalCommand, Result>,
                                          IRequestHandler<MerchantQueries.GetMerchantQuery, Result<Models.Merchant.Merchant>>,
                                          IRequestHandler<MerchantQueries.GetMerchantContractsQuery, Result<List<Models.Contract.Contract>>>,
                                          IRequestHandler<MerchantQueries.GetMerchantsQuery, Result<List<Models.Merchant.Merchant>>>,
                                          IRequestHandler<MerchantQueries.GetTransactionFeesForProductQuery, Result<List<Models.Contract.ContractProductTransactionFee>>>,
                                          IRequestHandler<MerchantCommands.UpdateMerchantCommand, Result>,
                                          IRequestHandler<MerchantCommands.AddMerchantAddressCommand, Result>,
                                          IRequestHandler<MerchantCommands.UpdateMerchantAddressCommand, Result>,
                                          IRequestHandler<MerchantCommands.AddMerchantContactCommand, Result>,
                                          IRequestHandler<MerchantCommands.UpdateMerchantContactCommand, Result>,
                                          IRequestHandler<MerchantCommands.RemoveOperatorFromMerchantCommand, Result>,
                                          IRequestHandler<MerchantCommands.RemoveMerchantContractCommand, Result>
{
    private readonly IProjectionStateRepository<MerchantBalanceState> MerchantBalanceStateRepository;
    private readonly IEventStoreContext EventStoreContext;
    private readonly ITransactionProcessorReadRepository TransactionProcessorReadRepository;
    private readonly IMerchantDomainService MerchantDomainService;
    private readonly ITransactionProcessorManager TransactionProcessorManager;

    public MerchantRequestHandler(IProjectionStateRepository<MerchantBalanceState> merchantBalanceStateRepository,
                                  IEventStoreContext eventStoreContext,
                                  ITransactionProcessorReadRepository transactionProcessorReadRepository,
                                  IMerchantDomainService merchantDomainService,
                                  ITransactionProcessorManager manager) {
        this.MerchantBalanceStateRepository = merchantBalanceStateRepository;
        this.EventStoreContext = eventStoreContext;
        this.TransactionProcessorReadRepository = transactionProcessorReadRepository;
        this.MerchantDomainService = merchantDomainService;
        this.TransactionProcessorManager = manager;
    }

    public async Task<Result<MerchantBalanceState>> Handle(MerchantQueries.GetMerchantBalanceQuery query,
                                                           CancellationToken cancellationToken) {
        return await this.MerchantBalanceStateRepository.Load(query.EstateId, query.MerchantId, cancellationToken);
    }

    public async Task<Result<Decimal>> Handle(MerchantQueries.GetMerchantLiveBalanceQuery query,
                                                                      CancellationToken cancellationToken) {
        return await this.TransactionProcessorManager.GetMerchantLiveBalance(query.EstateId, query.MerchantId, cancellationToken);
        //Result<String> result = await this.EventStoreContext.GetPartitionStateFromProjection("MerchantBalanceProjection", $"MerchantBalance-{query.MerchantId:N}", cancellationToken);
        //if (result.IsFailed)
        //    return Result.NotFound(
        //        $"Merchant Balance not found for Merchant {query.MerchantId} on MerchantBalanceProjection");

        //MerchantBalanceProjectionState1 projectionState = JsonConvert.DeserializeObject<MerchantBalanceProjectionState1>(result.Data);

        //return Result.Success(projectionState);
    }

    public async Task<Result<List<MerchantBalanceChangedEntry>>> Handle(MerchantQueries.GetMerchantBalanceHistoryQuery query,
                                                                        CancellationToken cancellationToken) {
        return await this.TransactionProcessorReadRepository.GetMerchantBalanceHistory(query.EstateId, query.MerchantId, query.StartDate, query.EndDate, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.AssignOperatorToMerchantCommand command,
                                         CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.AssignOperatorToMerchant(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.CreateMerchantUserCommand command,
                                     CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.CreateMerchantUser(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.AddMerchantDeviceCommand command,
                                     CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.AddDeviceToMerchant(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.MakeMerchantDepositCommand command,
                                     CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.MakeMerchantDeposit(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.SwapMerchantDeviceCommand command,
                                     CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.SwapMerchantDevice(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.MakeMerchantWithdrawalCommand command,
                                     CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.MakeMerchantWithdrawal(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.AddMerchantContractCommand command, CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.AddContractToMerchant(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.CreateMerchantCommand command, CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.CreateMerchant(command, cancellationToken);
    }

    public async Task<Result<Models.Merchant.Merchant>> Handle(MerchantQueries.GetMerchantQuery query, CancellationToken cancellationToken)
    {
        return await this.TransactionProcessorManager.GetMerchant(query.EstateId, query.MerchantId, cancellationToken);
    }

    public async Task<Result<List<Models.Contract.Contract>>> Handle(MerchantQueries.GetMerchantContractsQuery query, CancellationToken cancellationToken)
    {
        return await this.TransactionProcessorManager.GetMerchantContracts(query.EstateId, query.MerchantId, cancellationToken);
    }

    public async Task<Result<List<Merchant>>> Handle(MerchantQueries.GetMerchantsQuery query, CancellationToken cancellationToken)
    {
        return await this.TransactionProcessorManager.GetMerchants(query.EstateId, cancellationToken);
    }

    public async Task<Result<List<ContractProductTransactionFee>>> Handle(MerchantQueries.GetTransactionFeesForProductQuery query, CancellationToken cancellationToken)
    {
        return await this.TransactionProcessorManager.GetTransactionFeesForProduct(query.EstateId, query.MerchantId, query.ContractId, query.ProductId, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.UpdateMerchantCommand command, CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.UpdateMerchant(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.AddMerchantAddressCommand command, CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.AddMerchantAddress(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.UpdateMerchantAddressCommand command, CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.UpdateMerchantAddress(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.AddMerchantContactCommand command, CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.AddMerchantContact(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.UpdateMerchantContactCommand command, CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.UpdateMerchantContact(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.RemoveOperatorFromMerchantCommand command, CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.RemoveOperatorFromMerchant(command, cancellationToken);
    }

    public async Task<Result> Handle(MerchantCommands.RemoveMerchantContractCommand command, CancellationToken cancellationToken)
    {
        return await this.MerchantDomainService.RemoveContractFromMerchant(command, cancellationToken);
    }
}