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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecurityService.DataTransferObjects.Responses;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Models.Estate;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.ProjectionEngine.State;
using Estate = TransactionProcessor.Models.Estate.Estate;

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

        public MerchantDomainService(Func<IAggregateService> aggregateService,
                                     ISecurityServiceClient securityServiceClient,
                                     IEventStoreContext eventStoreContext)
        {
            this.AggregateService = aggregateService();
            this.SecurityServiceClient = securityServiceClient;
            this.EventStoreContext = eventStoreContext;
        }

        #endregion

        #region Methods

        public async Task<Result> AddDeviceToMerchant(MerchantCommands.AddMerchantDeviceCommand command, CancellationToken cancellationToken) {
            try {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result result = this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Guid deviceId = Guid.NewGuid();
                Result stateResult = merchantAggregate.AddDevice(deviceId, command.RequestDto.DeviceIdentifier);
                if (stateResult.IsFailed)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AssignOperatorToMerchant(MerchantCommands.AssignOperatorToMerchantCommand command,
                                                                 CancellationToken cancellationToken)
        {
            try {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result result = this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // Is the operator valid for this estate
                Estate estate = estateAggregate.GetEstate();
                Models.Estate.Operator @operator = estate.Operators?.SingleOrDefault(o => o.OperatorId == command.RequestDto.OperatorId);
                if (@operator == null) {
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
                Result stateResult = merchantAggregate.AssignOperator(command.RequestDto.OperatorId, @operator.Name, command.RequestDto.MerchantNumber, command.RequestDto.TerminalNumber);
                if (stateResult.IsFailed)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
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
            try {
                // Check if we have been sent a merchant id to use
                Guid merchantId = command.RequestDto.MerchantId ?? Guid.NewGuid();

                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                EstateAggregate estateAggregate = estateResult.Data;
                // Estate Id is a valid estate
                if (estateAggregate.IsCreated == false)
                {
                    return Result.Invalid($"Estate Id {estateAggregate.AggregateId} has not been created");
                }

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(merchantId, ct), merchantId, cancellationToken, false);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                MerchantAggregate merchantAggregate = merchantResult.Data;

                // Build up the models for the Crete call
                Address address = new Address(Guid.Empty, command.RequestDto.Address.AddressLine1, command.RequestDto.Address.AddressLine2, command.RequestDto.Address.AddressLine3, command.RequestDto.Address.AddressLine4, command.RequestDto.Address.Town, command.RequestDto.Address.Region, command.RequestDto.Address.PostalCode, command.RequestDto.Address.Country);
                Contact contact = new Contact(Guid.Empty, command.RequestDto.Contact.EmailAddress, command.RequestDto.Contact.ContactName, command.RequestDto.Contact.PhoneNumber);
                // Set the settlement schedule
                SettlementSchedule settlementSchedule = ConvertSettlementSchedule(command.RequestDto.SettlementSchedule);

                var stateResult = merchantAggregate.Create(command.EstateId, command.RequestDto.Name, command.RequestDto.CreatedDateTime.GetValueOrDefault(DateTime.Now),
                    address, contact, settlementSchedule);
                if (stateResult.IsFailed)
                    return stateResult;


                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> CreateMerchantUser(MerchantCommands.CreateMerchantUserCommand command, CancellationToken cancellationToken)
        {
            try {
                CreateUserRequest createUserRequest = new() {
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

                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result validateResult = this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (validateResult.IsFailed)
                    return ResultHelpers.CreateFailure(validateResult);

                Result createUserResult = await this.SecurityServiceClient.CreateUser(createUserRequest, cancellationToken);
                if (createUserResult.IsFailed)
                    return ResultHelpers.CreateFailure(createUserResult);

                Result<List<UserDetails>> userDetailsResult = await this.SecurityServiceClient.GetUsers(createUserRequest.EmailAddress, cancellationToken);
                if (userDetailsResult.IsFailed)
                    return ResultHelpers.CreateFailure(userDetailsResult);

                UserDetails user = userDetailsResult.Data.SingleOrDefault();
                if (user == null)
                    return Result.Failure($"Unable to get user details for username {createUserRequest.EmailAddress}");

                // Add the user to the aggregate 
                Result stateResult = merchantAggregate.AddSecurityUser(user.UserId, command.RequestDto.EmailAddress);
                if (stateResult.IsFailed)
                    return stateResult;

                // TODO: add a delete user here in case the aggregate add fails...

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> MakeMerchantDeposit(MerchantCommands.MakeMerchantDepositCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result validateResult =
                    this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (validateResult.IsFailed)
                    return ResultHelpers.CreateFailure(validateResult);

                Result<MerchantDepositListAggregate> getDepositListResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantDepositListAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken, false);
                if (getDepositListResult.IsFailed)
                    return ResultHelpers.CreateFailure(getDepositListResult);

                MerchantDepositListAggregate merchantDepositListAggregate = getDepositListResult.Data;
                if (merchantDepositListAggregate.IsCreated == false)
                {
                    merchantDepositListAggregate.Create(merchantAggregate, command.RequestDto.DepositDateTime);
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

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> MakeMerchantWithdrawal(MerchantCommands.MakeMerchantWithdrawalCommand command,
                                                               CancellationToken cancellationToken)
        {

            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result validateResult =
                    this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (validateResult.IsFailed)
                    return ResultHelpers.CreateFailure(validateResult);

                Result<MerchantDepositListAggregate> getDepositListResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantDepositListAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (getDepositListResult.IsFailed)
                    return ResultHelpers.CreateFailure(getDepositListResult);
                
                MerchantDepositListAggregate merchantDepositListAggregate = getDepositListResult.Data;
                if (merchantDepositListAggregate.IsCreated == false)
                {
                    return Result.Invalid($"Merchant [{command.MerchantId}] has not made any deposits yet");
                }

                // Now we need to check the merchants balance to ensure they have funds to withdraw
                Result<String> getBalanceResult = await this.EventStoreContext.GetPartitionStateFromProjection("MerchantBalanceProjection", $"MerchantBalance-{command.MerchantId:N}", cancellationToken);
                if (getBalanceResult.IsFailed)
                {
                    Result.Invalid($"Failed to get Merchant Balance.");
                }

                MerchantBalanceProjectionState1 projectionState = JsonConvert.DeserializeObject<MerchantBalanceProjectionState1>(getBalanceResult.Data);

                if (command.RequestDto.Amount > projectionState.merchant.balance)
                {
                    return Result.Invalid($"Not enough credit available for withdrawal of [{command.RequestDto.Amount}]. Balance is {projectionState.merchant.balance}");
                }


                // If we are here we have enough credit to withdraw
                PositiveMoney amount = PositiveMoney.Create(Money.Create(command.RequestDto.Amount));

                merchantDepositListAggregate.MakeWithdrawal(command.RequestDto.WithdrawalDateTime, amount);

                Result saveResult = await this.AggregateService.Save(merchantDepositListAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddContractToMerchant(MerchantCommands.AddMerchantContractCommand command, CancellationToken cancellationToken)
        {
            try {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result validateResult = this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (validateResult.IsFailed)
                    return ResultHelpers.CreateFailure(validateResult);

                Result<ContractAggregate> getContractResult = await this.AggregateService.Get<ContractAggregate>(command.RequestDto.ContractId, cancellationToken);
                if (getContractResult.IsFailed) {
                    return ResultHelpers.CreateFailure(getContractResult);
                }

                ContractAggregate contractAggregate = getContractResult.Data;
                if (contractAggregate.IsCreated == false) {
                    return Result.Invalid($"Contract Id {command.RequestDto.ContractId} has not been created");
                }

                Result stateResult = merchantAggregate.AddContract(contractAggregate);
                if (stateResult.IsFailed)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> UpdateMerchant(MerchantCommands.UpdateMerchantCommand command, CancellationToken cancellationToken)
        {
            try {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result result = this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result stateResult = merchantAggregate.UpdateMerchant(command.RequestDto.Name);
                if (stateResult.IsFailed)
                    return stateResult;
                SettlementSchedule settlementSchedule = ConvertSettlementSchedule(command.RequestDto.SettlementSchedule);
                stateResult = merchantAggregate.SetSettlementSchedule(settlementSchedule);
                if (stateResult.IsFailed)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddMerchantAddress(MerchantCommands.AddMerchantAddressCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result result =
                    this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result stateResult = merchantAggregate.AddAddress(command.RequestDto.AddressLine1,
                                                                 command.RequestDto.AddressLine2,
                                                                 command.RequestDto.AddressLine3,
                                                                 command.RequestDto.AddressLine4,
                                                                 command.RequestDto.Town,
                                                                 command.RequestDto.Region,
                                                                 command.RequestDto.PostalCode,
                                                                 command.RequestDto.Country);
                if (stateResult.IsFailed)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> UpdateMerchantAddress(MerchantCommands.UpdateMerchantAddressCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result result =
                    this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result stateResult = merchantAggregate.UpdateAddress(command.AddressId,
                                                                 command.RequestDto.AddressLine1,
                                                                 command.RequestDto.AddressLine2,
                                                                 command.RequestDto.AddressLine3,
                                                                 command.RequestDto.AddressLine4,
                                                                 command.RequestDto.Town,
                                                                 command.RequestDto.Region,
                                                                 command.RequestDto.PostalCode,
                                                                 command.RequestDto.Country);
                if (stateResult.IsFailed)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddMerchantContact(MerchantCommands.AddMerchantContactCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result result =
                    this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result stateResult = merchantAggregate.AddContact(command.RequestDto.ContactName,
                                                                 command.RequestDto.PhoneNumber,
                                                                 command.RequestDto.EmailAddress);
                if (stateResult.IsFailed)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> UpdateMerchantContact(MerchantCommands.UpdateMerchantContactCommand command,
                                                        CancellationToken cancellationToken) {
            try {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result result = this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result stateResult = merchantAggregate.UpdateContact(command.ContactId, command.RequestDto.ContactName, command.RequestDto.EmailAddress, command.RequestDto.PhoneNumber);
                if (stateResult.IsFailed)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> RemoveOperatorFromMerchant(MerchantCommands.RemoveOperatorFromMerchantCommand command, CancellationToken cancellationToken)
        {
            try {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result result = this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result stateResult = merchantAggregate.RemoveOperator(command.OperatorId);
                if (stateResult.IsFailed)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> RemoveContractFromMerchant(MerchantCommands.RemoveMerchantContractCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result result =
                    this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result stateResult = merchantAggregate.RemoveContract(command.ContractId);
                if (stateResult.IsFailed)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        private Result ValidateEstateAndMerchant(EstateAggregate estateAggregate,
                                                 MerchantAggregate merchantAggregate) {

            // Check merchant has been created
            if (merchantAggregate.IsCreated == false) {
                return Result.Invalid($"Merchant Id {merchantAggregate.AggregateId} has not been created");
            }

            // Estate Id is a valid estate
            if (estateAggregate.IsCreated == false) {
                return Result.Invalid($"Estate Id {estateAggregate.AggregateId} has not been created");
            }

            return Result.Success();
        }

        public async Task<Result> SwapMerchantDevice(MerchantCommands.SwapMerchantDeviceCommand command,
                                                     CancellationToken cancellationToken)
        {
            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(merchantResult);

                EstateAggregate estateAggregate = estateResult.Data;
                MerchantAggregate merchantAggregate = merchantResult.Data;

                Result result =
                    this.ValidateEstateAndMerchant(estateAggregate, merchantAggregate);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                Result stateResult = merchantAggregate.SwapDevice(command.DeviceIdentifier, command.RequestDto.NewDeviceIdentifier);
                if (stateResult.IsFailed)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        #endregion
    }
}
