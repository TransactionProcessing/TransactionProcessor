using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecurityService.Client;
using SecurityService.DataTransferObjects;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventStore;
using Shared.Exceptions;
using Shared.Results;
using Shared.ValueObjects;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Models.Estate;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.ProjectionEngine.State;

namespace TransactionProcessor.BusinessLogic.Services
{
    public interface IMerchantDomainService
    {
        #region Methods
        Task<Result> CreateMerchant(MerchantCommands.CreateMerchantCommand command, CancellationToken cancellationToken);
        Task<Result> AssignOperatorToMerchant(MerchantCommands.AssignOperatorToMerchantCommand command, CancellationToken cancellationToken);
        Task<Result> CreateMerchantUser(MerchantCommands.CreateMerchantUserCommand command, CancellationToken cancellationToken);
        Task<Result> AddDeviceToMerchant(MerchantCommands.AddMerchantDeviceCommand command, CancellationToken cancellationToken);
        Task<Result> SwapMerchantDevice(MerchantCommands.SwapMerchantDeviceCommand command, CancellationToken cancellationToken);
        Task<Result> MakeMerchantDeposit(MerchantCommands.MakeMerchantDepositCommand command, CancellationToken cancellationToken);
        Task<Result> MakeMerchantWithdrawal(MerchantCommands.MakeMerchantWithdrawalCommand command, CancellationToken cancellationToken);
        Task<Result> AddContractToMerchant(MerchantCommands.AddMerchantContractCommand command, CancellationToken cancellationToken);
        Task<Result> UpdateMerchant(MerchantCommands.UpdateMerchantCommand command, CancellationToken cancellationToken);
        Task<Result> AddMerchantAddress(MerchantCommands.AddMerchantAddressCommand command, CancellationToken cancellationToken);
        Task<Result> UpdateMerchantAddress(MerchantCommands.UpdateMerchantAddressCommand command, CancellationToken cancellationToken);
        Task<Result> AddMerchantContact(MerchantCommands.AddMerchantContactCommand command, CancellationToken cancellationToken);
        Task<Result> UpdateMerchantContact(MerchantCommands.UpdateMerchantContactCommand command, CancellationToken cancellationToken);
        Task<Result> RemoveOperatorFromMerchant(MerchantCommands.RemoveOperatorFromMerchantCommand command, CancellationToken cancellationToken);
        Task<Result> RemoveContractFromMerchant(MerchantCommands.RemoveMerchantContractCommand command, CancellationToken cancellationToken);

        #endregion
    }

    public class MerchantDomainService : IMerchantDomainService
    {
        #region Fields
        
        private readonly IAggregateService AggregateService;
        private readonly ISecurityServiceClient SecurityServiceClient;
        private readonly IEventStoreContext EventStoreContext;

        #endregion

        #region Constructors

        public MerchantDomainService(IAggregateService aggregateService,
                                     ISecurityServiceClient securityServiceClient,
                                     IEventStoreContext eventStoreContext)
        {
            this.AggregateService = aggregateService;
            this.SecurityServiceClient = securityServiceClient;
            this.EventStoreContext = eventStoreContext;
        }

        #endregion

        #region Methods

        private async Task<Result> ApplyUpdates(Func<(EstateAggregate estateAggregate, MerchantAggregate merchantAggregate), Task<Result>> action, Guid estateId, Guid merchantId, CancellationToken cancellationToken, Boolean isNotFoundError = true)
        {
            try
            {
                Result<EstateAggregate> getEstateResult = await this.AggregateService.Get<EstateAggregate>(estateId, cancellationToken);
                if (getEstateResult.IsFailed) {
                    return ResultHelpers.CreateFailure(getEstateResult);
                }
                EstateAggregate estateAggregate = getEstateResult.Data;
                Result<MerchantAggregate> getMerchantResult = await this.AggregateService.GetLatest<MerchantAggregate>(merchantId, cancellationToken);
                Result<MerchantAggregate> merchantAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getMerchantResult, merchantId, isNotFoundError);
                if (merchantAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantAggregateResult);

                MerchantAggregate merchantAggregate = merchantAggregateResult.Data;

                Result result = await action((estateAggregate, merchantAggregate));
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddDeviceToMerchant(MerchantCommands.AddMerchantDeviceCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    Guid deviceId = Guid.NewGuid();
                    aggregates.merchantAggregate.AddDevice(deviceId, command.RequestDto.DeviceIdentifier);

                    return Result.Success();
                },
                command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> AssignOperatorToMerchant(MerchantCommands.AssignOperatorToMerchantCommand command,
                                                                 CancellationToken cancellationToken)
        {

            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    // Is the operator valid for this estate
                    Estate estate = aggregates.estateAggregate.GetEstate();
                    Models.Estate.Operator @operator = estate.Operators?.SingleOrDefault(o => o.OperatorId == command.RequestDto.OperatorId);
                    if (@operator == null)
                    {
                        return Result.Invalid($"Operator Id {command.RequestDto.OperatorId} is not supported on Estate [{estate.Name}]");
                    }

                    // TODO: Reintroduce when we have an Operator Aggregate
                    // https://github.com/TransactionProcessing/EstateManagement/issues/558
                    // Operator has been validated, now check the rules of the operator against the passed in data
                    //if (@operator.RequireCustomMerchantNumber) {
                    //    // requested addition must have a merchant number supplied
                    //    if (String.IsNullOrEmpty(command.RequestDto.MerchantNumber)) {
                    //        throw new InvalidOperationException($"Operator Id {command.RequestDto.OperatorId} requires that a merchant number is provided");
                    //    }
                    //}

                    //if (@operator.RequireCustomTerminalNumber) {
                    //    // requested addition must have a terminal number supplied
                    //    if (String.IsNullOrEmpty(command.RequestDto.TerminalNumber)) {
                    //        throw new InvalidOperationException($"Operator Id {command.RequestDto.OperatorId} requires that a terminal number is provided");
                    //    }
                    //}

                    // Assign the operator
                    // TODO: Swap second parameter to name
                    aggregates.merchantAggregate.AssignOperator(command.RequestDto.OperatorId, @operator.Name, command.RequestDto.MerchantNumber, command.RequestDto.TerminalNumber);

                    return Result.Success();
                },
                command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        private SettlementSchedule ConvertSettlementSchedule(DataTransferObjects.Responses.Merchant.SettlementSchedule settlementSchedule) =>
            settlementSchedule switch
            {
                DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate => SettlementSchedule.Immediate,
                DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly => SettlementSchedule.Monthly,
                DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly => SettlementSchedule.Weekly,
                _ => SettlementSchedule.NotSet
            };

        public async Task<Result> CreateMerchant(MerchantCommands.CreateMerchantCommand command, CancellationToken cancellationToken)
        {
            // Check if we have been sent a merchant id to use
            Guid merchantId = command.RequestDto.MerchantId ?? Guid.NewGuid();

            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {
                    if (aggregates.estateAggregate.IsCreated == false)
                    {
                        return Result.Forbidden($"Estate Id {command.EstateId} has not been created");
                    }

                    if (aggregates.merchantAggregate.IsCreated)
                    {
                        aggregates.merchantAggregate.Create(command.EstateId, command.RequestDto.Name, aggregates.merchantAggregate.DateCreated);
                    }
                    else
                    {
                        aggregates.merchantAggregate.Create(command.EstateId, command.RequestDto.Name, command.RequestDto.CreatedDateTime.GetValueOrDefault(DateTime.Now));
                        aggregates.merchantAggregate.GenerateReference();

                        // Add the address 
                        aggregates.merchantAggregate.AddAddress(command.RequestDto.Address.AddressLine1, command.RequestDto.Address.AddressLine2, command.RequestDto.Address.AddressLine3,
                                                         command.RequestDto.Address.AddressLine4, command.RequestDto.Address.Town, command.RequestDto.Address.Region,
                                                         command.RequestDto.Address.PostalCode, command.RequestDto.Address.Country);

                        // Add the contact
                        aggregates.merchantAggregate.AddContact(command.RequestDto.Contact.ContactName, command.RequestDto.Contact.PhoneNumber, command.RequestDto.Contact.EmailAddress);

                        // Set the settlement schedule
                        SettlementSchedule settlementSchedule = ConvertSettlementSchedule(command.RequestDto.SettlementSchedule);
                        aggregates.merchantAggregate.SetSettlementSchedule(settlementSchedule);
                    }
                    return Result.Success();
                },
                command.EstateId, merchantId, cancellationToken, false);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> CreateMerchantUser(MerchantCommands.CreateMerchantUserCommand command, CancellationToken cancellationToken)
        {

            CreateUserRequest createUserRequest = new CreateUserRequest
            {
                EmailAddress = command.RequestDto.EmailAddress,
                FamilyName = command.RequestDto.FamilyName,
                GivenName = command.RequestDto.GivenName,
                MiddleName = command.RequestDto.MiddleName,
                Password = command.RequestDto.Password,
                PhoneNumber = "123456", // Is this really needed :|
                Roles = new List<String>(),
                Claims = new Dictionary<String, String>()
            };

            String merchantRoleName = Environment.GetEnvironmentVariable("MerchantRoleName");
            createUserRequest.Roles.Add(String.IsNullOrEmpty(merchantRoleName) ? "Merchant" : merchantRoleName);
            createUserRequest.Claims.Add("estateId", command.EstateId.ToString());
            createUserRequest.Claims.Add("merchantId", command.MerchantId.ToString());

            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    Result createUserResult = await this.SecurityServiceClient.CreateUser(createUserRequest, cancellationToken);
                    if (createUserResult.IsFailed)
                        return ResultHelpers.CreateFailure(createUserResult);

                    var userDetailsResult = await this.SecurityServiceClient.GetUsers(createUserRequest.EmailAddress, cancellationToken);
                    if (userDetailsResult.IsFailed)
                        return ResultHelpers.CreateFailure(userDetailsResult);

                    var user = userDetailsResult.Data.SingleOrDefault();
                    if (user == null)
                        return Result.Failure($"Unable to get user details for username {createUserRequest.EmailAddress}");

                    // Add the user to the aggregate 
                    aggregates.merchantAggregate.AddSecurityUser(user.UserId,
                        command.RequestDto.EmailAddress);

                    // TODO: add a delete user here in case the aggregate add fails...

                    return Result.Success();
                }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> MakeMerchantDeposit(MerchantCommands.MakeMerchantDepositCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    Result<MerchantDepositListAggregate> getDepositListResult = await this.AggregateService.GetLatest<MerchantDepositListAggregate>(command.MerchantId, cancellationToken);
                    Result<MerchantDepositListAggregate> merchantDepositListAggregateResult =
                        DomainServiceHelper.HandleGetAggregateResult(getDepositListResult, command.MerchantId, false);
                    if (merchantDepositListAggregateResult.IsFailed)
                        return ResultHelpers.CreateFailure(merchantDepositListAggregateResult);

                    MerchantDepositListAggregate merchantDepositListAggregate = merchantDepositListAggregateResult.Data;
                    if (merchantDepositListAggregate.IsCreated == false)
                    {
                        merchantDepositListAggregate.Create(aggregates.merchantAggregate, command.RequestDto.DepositDateTime);
                    }

                    PositiveMoney amount = PositiveMoney.Create(Money.Create(command.RequestDto.Amount));
                    MerchantDepositSource depositSource = command.DepositSource switch
                    {
                        DataTransferObjects.Requests.Merchant.MerchantDepositSource.Manual => Models.Merchant.MerchantDepositSource.Manual,
                        _ => Models.Merchant.MerchantDepositSource.Automatic,
                    };
                    merchantDepositListAggregate.MakeDeposit(depositSource, command.RequestDto.Reference, command.RequestDto.DepositDateTime, amount);

                    Result saveResult = await this.AggregateService.Save(merchantDepositListAggregate, cancellationToken);
                    if (saveResult.IsFailed)
                        return ResultHelpers.CreateFailure(saveResult);

                    return Result.Success();
                }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> MakeMerchantWithdrawal(MerchantCommands.MakeMerchantWithdrawalCommand command,
                                                               CancellationToken cancellationToken)
        {

            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) =>
                {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    Result<MerchantDepositListAggregate> getDepositListResult = await this.AggregateService.GetLatest<MerchantDepositListAggregate>(command.MerchantId, cancellationToken);
                    Result<MerchantDepositListAggregate> merchantDepositListAggregateResult =
                        DomainServiceHelper.HandleGetAggregateResult(getDepositListResult, command.MerchantId, false);
                    if (merchantDepositListAggregateResult.IsFailed)
                        return ResultHelpers.CreateFailure(merchantDepositListAggregateResult);

                    MerchantDepositListAggregate merchantDepositListAggregate = merchantDepositListAggregateResult.Data;
                    if (merchantDepositListAggregate.IsCreated == false)
                    {
                        return Result.Invalid($"Merchant [{command.MerchantId}] has not made any deposits yet");
                    }
                    
                    // TODO:Convert to use the new MerchantBalanceAggregate
                    //// Now we need to check the merchants balance to ensure they have funds to withdraw
                    //Result<String> getBalanceResult = await this.EventStoreContext.GetPartitionStateFromProjection("MerchantBalanceProjection", $"MerchantBalance-{command.MerchantId:N}", cancellationToken);
                    //if (getBalanceResult.IsFailed)
                    //{
                    //    Result.Invalid($"Failed to get Merchant Balance.");
                    //}

                    //MerchantBalanceProjectionState1 projectionState = JsonConvert.DeserializeObject<MerchantBalanceProjectionState1>(getBalanceResult.Data);

                    //if (command.RequestDto.Amount > projectionState.merchant.balance)
                    //{
                    //    return Result.Invalid($"Not enough credit available for withdrawal of [{command.RequestDto.Amount}]. Balance is {projectionState.merchant.balance}");
                    //}

                    // If we are here we have enough credit to withdraw
                    PositiveMoney amount = PositiveMoney.Create(Money.Create(command.RequestDto.Amount));

                    merchantDepositListAggregate.MakeWithdrawal(command.RequestDto.WithdrawalDateTime, amount);

                    Result saveResult = await this.AggregateService.Save(merchantDepositListAggregate, cancellationToken);
                    if (saveResult.IsFailed)
                        return ResultHelpers.CreateFailure(saveResult);

                    return Result.Success();
                }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        /// <summary>
        /// The token response
        /// </summary>
        //private TokenResponse TokenResponse;

        public async Task<Result> AddContractToMerchant(MerchantCommands.AddMerchantContractCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    var getContractResult = await this.AggregateService.Get<ContractAggregate>(command.RequestDto.ContractId, cancellationToken);
                    if (getContractResult.IsFailed) {
                        return ResultHelpers.CreateFailure(getContractResult);
                    }

                    ContractAggregate contractAggregate = getContractResult.Data;
                    if (contractAggregate.IsCreated == false)
                    {
                        return Result.Invalid($"Contract Id {command.RequestDto.ContractId} has not been created");
                    }

                    aggregates.merchantAggregate.AddContract(contractAggregate);

                    return Result.Success();
                }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> UpdateMerchant(MerchantCommands.UpdateMerchantCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    aggregates.merchantAggregate.UpdateMerchant(command.RequestDto.Name);

                    SettlementSchedule settlementSchedule = ConvertSettlementSchedule(command.RequestDto.SettlementSchedule);
                    aggregates.merchantAggregate.SetSettlementSchedule(settlementSchedule);

                    return Result.Success();
                }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> AddMerchantAddress(MerchantCommands.AddMerchantAddressCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    aggregates.merchantAggregate.AddAddress(command.RequestDto.AddressLine1,
                                                                 command.RequestDto.AddressLine2,
                                                                 command.RequestDto.AddressLine3,
                                                                 command.RequestDto.AddressLine4,
                                                                 command.RequestDto.Town,
                                                                 command.RequestDto.Region,
                                                                 command.RequestDto.PostalCode,
                                                                 command.RequestDto.Country);

                    return Result.Success();
                }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> UpdateMerchantAddress(MerchantCommands.UpdateMerchantAddressCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    aggregates.merchantAggregate.UpdateAddress(command.AddressId,
                                                                 command.RequestDto.AddressLine1,
                                                                 command.RequestDto.AddressLine2,
                                                                 command.RequestDto.AddressLine3,
                                                                 command.RequestDto.AddressLine4,
                                                                 command.RequestDto.Town,
                                                                 command.RequestDto.Region,
                                                                 command.RequestDto.PostalCode,
                                                                 command.RequestDto.Country);

                    return Result.Success();
                }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> AddMerchantContact(MerchantCommands.AddMerchantContactCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    aggregates.merchantAggregate.AddContact(command.RequestDto.ContactName,
                                                                 command.RequestDto.PhoneNumber,
                                                                 command.RequestDto.EmailAddress);

                    return Result.Success();
                }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> UpdateMerchantContact(MerchantCommands.UpdateMerchantContactCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                 async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                     Result result =
                         this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                     if (result.IsFailed)
                         return ResultHelpers.CreateFailure(result);

                     aggregates.merchantAggregate.UpdateContact(command.ContactId,
                         command.RequestDto.ContactName,
                         command.RequestDto.EmailAddress,
                         command.RequestDto.PhoneNumber
                     ); ;

                     return Result.Success();
                 }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> RemoveOperatorFromMerchant(MerchantCommands.RemoveOperatorFromMerchantCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    aggregates.merchantAggregate.RemoveOperator(command.OperatorId);

                    return Result.Success();
                }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        public async Task<Result> RemoveContractFromMerchant(MerchantCommands.RemoveMerchantContractCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    aggregates.merchantAggregate.RemoveContract(command.ContractId);

                    return Result.Success();
                }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        private Result ValidateEstateAndMerchant(EstateAggregate estateAggregate, MerchantAggregate merchantAggregate)
        {

            // Check merchant has been created
            if (merchantAggregate.IsCreated == false)
            {
                return Result.Invalid($"Merchant Id {merchantAggregate.AggregateId} has not been created");
            }

            // Estate Id is a valid estate
            if (estateAggregate.IsCreated == false)
            {
                return Result.Invalid($"Estate Id {estateAggregate.AggregateId} has not been created");
            }
            return Result.Success();
        }

        public async Task<Result> SwapMerchantDevice(MerchantCommands.SwapMerchantDeviceCommand command,
                                                   CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates(
                async ((EstateAggregate estateAggregate, MerchantAggregate merchantAggregate) aggregates) => {

                    Result result =
                        this.ValidateEstateAndMerchant(aggregates.estateAggregate, aggregates.merchantAggregate);
                    if (result.IsFailed)
                        return ResultHelpers.CreateFailure(result);

                    aggregates.merchantAggregate.SwapDevice(command.DeviceIdentifier, command.RequestDto.NewDeviceIdentifier);

                    return Result.Success();
                }, command.EstateId, command.MerchantId, cancellationToken);

            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            return Result.Success();
        }

        #endregion
    }
}
