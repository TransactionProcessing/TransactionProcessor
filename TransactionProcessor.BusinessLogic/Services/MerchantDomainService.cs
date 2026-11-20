using SecurityService.Client;
using SecurityService.DataTransferObjects;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.EventStore.EventStore;
using TransactionProcessing.Shared.Exceptions;
using TransactionProcessing.UnionResults;
using TransactionProcessing.Shared.ValueObjects;
using TransactionProcessing.UnionResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KurrentDB.Client;
using TransactionProcessing.Shared.Serialisation;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Models.Estate;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Models.MerchantSchedule;
using TransactionProcessor.ProjectionEngine.State;
using Estate = TransactionProcessor.Models.Estate.Estate;
using MerchantScheduleMonth = TransactionProcessor.Models.MerchantSchedule.MerchantScheduleMonth;

namespace TransactionProcessor.BusinessLogic.Services
{
    public interface IMerchantDomainService
    {
        Task<Result<Guid>> CreateMerchant(MerchantCommands.CreateMerchantCommand command, CancellationToken cancellationToken);
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
        Task<Result> UpdateMerchantOpeningHours(MerchantCommands.UpdateMerchantOpeningHoursCommand command, CancellationToken cancellationToken);
        Task<Result> CreateMerchantSchedule(MerchantCommands.CreateMerchantScheduleCommand command, CancellationToken cancellationToken);
        Task<Result> UpdateMerchantSchedule(MerchantCommands.UpdateMerchantScheduleCommand command, CancellationToken cancellationToken);
    }

    public class MerchantDomainService : IMerchantDomainService
    {
        private readonly IAggregateService AggregateService;
        private readonly ISecurityServiceClient SecurityServiceClient;
        private readonly IEventStoreContext EventStoreContext;
        private TokenResponse TokenResponse;

        public MerchantDomainService(Func<IAggregateService> aggregateService,
                                     ISecurityServiceClient securityServiceClient,
                                     IEventStoreContext eventStoreContext)
        {
            this.AggregateService = aggregateService();
            this.SecurityServiceClient = securityServiceClient;
            this.EventStoreContext = eventStoreContext;
        }

        public async Task<Result> AddDeviceToMerchant(MerchantCommands.AddMerchantDeviceCommand command, CancellationToken cancellationToken) {
            try {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Guid deviceId = Guid.NewGuid();
                Result stateResult = merchantAggregate.AddDevice(deviceId, command.RequestDto.DeviceIdentifier);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AssignOperatorToMerchant(MerchantCommands.AssignOperatorToMerchantCommand command,
                                                                  CancellationToken cancellationToken)
        {
            try {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                EstateAggregate estateAggregate = contextResult.Data.EstateAggregate;
                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                (Result validationResult, String operatorName) = await this.GetOperatorAssignmentDetails(command, estateAggregate, cancellationToken);
                if (validationResult.IsSuccess() == false)
                    return validationResult;

                // Assign the operator
                Result stateResult = merchantAggregate.AssignOperator(command.RequestDto.OperatorId, operatorName, command.RequestDto.MerchantNumber, command.RequestDto.TerminalNumber);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        private async Task<(Result ValidationResult, String OperatorName)> GetOperatorAssignmentDetails(MerchantCommands.AssignOperatorToMerchantCommand command,
                                                                                                        EstateAggregate estateAggregate,
                                                                                                        CancellationToken cancellationToken)
        {
            Estate estate = estateAggregate.GetEstate();
            Models.Estate.Operator @operator = estate.Operators?.SingleOrDefault(o => o.OperatorId == command.RequestDto.OperatorId);
            if (@operator == null) {
                return (new Error(ErrorType.Validation, $"Operator Id {command.RequestDto.OperatorId} is not supported on Estate [{estate.Name}]"), String.Empty);
            }

            Result<OperatorAggregate> operatorResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<OperatorAggregate>(command.RequestDto.OperatorId, ct), command.RequestDto.OperatorId, cancellationToken);
            if (operatorResult.IsSuccess() == false)
                return (ResultHelpers.CreateFailure(operatorResult), String.Empty);

            Result validationResult = this.ValidateOperatorAssignment(command.RequestDto.OperatorId,
                command.RequestDto.MerchantNumber,
                command.RequestDto.TerminalNumber,
                operatorResult.Data);

            return validationResult.IsSuccess() == false
                ? (validationResult, String.Empty)
                : (Results.Ok(), @operator.Name);
        }

        private Result ValidateOperatorAssignment(Guid operatorId,
                                                  String merchantNumber,
                                                  String terminalNumber,
                                                  OperatorAggregate operatorAggregate)
        {
            if (operatorAggregate.RequireCustomMerchantNumber && String.IsNullOrEmpty(merchantNumber)) {
                return new Error(ErrorType.Validation, $"Operator Id {operatorId} requires that a merchant number is provided");
            }

            if (operatorAggregate.RequireCustomTerminalNumber && String.IsNullOrEmpty(terminalNumber)) {
                return new Error(ErrorType.Validation, $"Operator Id {operatorId} requires that a terminal number is provided");
            }

            return Results.Ok();
        }

        private SettlementSchedule ConvertSettlementSchedule(DataTransferObjects.Responses.Merchant.SettlementSchedule settlementSchedule) =>
            settlementSchedule switch
            {
                DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate => SettlementSchedule.Immediate,
                DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly => SettlementSchedule.Monthly,
                DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly => SettlementSchedule.Weekly,
                _ => SettlementSchedule.NotSet
            };

        private MerchantDepositSource ConvertDepositSource(DataTransferObjects.Requests.Merchant.MerchantDepositSource depositSource) =>
            depositSource switch
            {
                DataTransferObjects.Requests.Merchant.MerchantDepositSource.Manual => MerchantDepositSource.Manual,
                _ => MerchantDepositSource.Automatic,
            };

        private Result EnsureMerchantDepositListCreated(MerchantDepositListAggregate merchantDepositListAggregate,
                                                        MerchantAggregate merchantAggregate,
                                                        DateTimeOffset depositDateTime) {
            if (merchantDepositListAggregate.IsCreated) {
                return Results.Ok();
            }

            return merchantDepositListAggregate.Create(merchantAggregate, depositDateTime);
        }

        public async Task<Result<Guid>> CreateMerchant(MerchantCommands.CreateMerchantCommand command, CancellationToken cancellationToken)
        {
            try {
                if (command.RequestDto == null)
                    return new Error(ErrorType.Validation, "Request DTO must not be null");

                if (String.IsNullOrEmpty(command.RequestDto.Name))
                    return new Error(ErrorType.Validation, "Merchant name is a required value");

                // Check if we have been sent a merchant id to use
                Guid merchantId = IdGenerationService.GenerateMerchantId(command.EstateId, command.RequestDto.Name);

                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(estateResult);

                EstateAggregate estateAggregate = estateResult.Data;
                // Estate Id is a valid estate
                if (estateAggregate.IsCreated == false)
                {
                    return new Error(ErrorType.Validation, $"Estate Id {estateAggregate.AggregateId} has not been created");
                }

                Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantAggregate>(merchantId, ct), merchantId, cancellationToken, false);
                if (merchantResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(merchantResult);

                MerchantAggregate merchantAggregate = merchantResult.Data;
                // Build up the models for the Crete call
                Address address = Address.Create(Guid.Empty, new AddressLines(command.RequestDto.Address.AddressLine1, command.RequestDto.Address.AddressLine2, command.RequestDto.Address.AddressLine3, command.RequestDto.Address.AddressLine4), command.RequestDto.Address.Town, command.RequestDto.Address.Region, command.RequestDto.Address.PostalCode, command.RequestDto.Address.Country);
                Contact contact = new Contact(Guid.Empty, command.RequestDto.Contact.EmailAddress, command.RequestDto.Contact.ContactName, command.RequestDto.Contact.PhoneNumber);
                // Set the settlement schedule
                SettlementSchedule settlementSchedule = ConvertSettlementSchedule(command.RequestDto.SettlementSchedule);

                Result stateResult = merchantAggregate.Create(command.EstateId, command.RequestDto.Name, command.RequestDto.CreatedDateTime.GetValueOrDefault(DateTimeOffset.UtcNow),
                    address, contact, settlementSchedule);
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);


                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return Results.Ok(merchantId);
            }
            catch (Exception ex) {
                return new Result<Guid>(new Error(ErrorType.Failure, String.Join(Environment.NewLine, ex.GetExceptionMessages())));
            }
        }

        public async Task<Result> CreateMerchantUser(MerchantCommands.CreateMerchantUserCommand command, CancellationToken cancellationToken)
        {
            try {
                CreateUserRequest createUserRequest = this.BuildMerchantUserRequest(command);

                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Result<UserResponse> getUserResult = await this.CreateMerchantSecurityUser(createUserRequest, cancellationToken);
                if (getUserResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getUserResult);

                // Add the user to the aggregate 
                Result stateResult = merchantAggregate.AddSecurityUser(Guid.Parse(getUserResult.Data.UserId), command.RequestDto.EmailAddress);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        private CreateUserRequest BuildMerchantUserRequest(MerchantCommands.CreateMerchantUserCommand command) {
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

            return createUserRequest;
        }

        private async Task<Result<UserResponse>> CreateMerchantSecurityUser(CreateUserRequest createUserRequest,
                                                                            CancellationToken cancellationToken) {
            Result<TokenResponse> getTokenResult = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);
            if (getTokenResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(getTokenResult);
            this.TokenResponse = getTokenResult.Data;

            var createUserResult = await this.SecurityServiceClient.CreateUser(this.TokenResponse.AccessToken, createUserRequest, cancellationToken);
            if (createUserResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(createUserResult);

            var userDetailsResult = await this.SecurityServiceClient.GetUsers(this.TokenResponse.AccessToken, createUserRequest.EmailAddress, cancellationToken);
            if (userDetailsResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(userDetailsResult);

            UserResponse user = userDetailsResult.Data.SingleOrDefault();
            if (user == null)
                return new Error(ErrorType.Failure, $"Unable to get user details for username {createUserRequest.EmailAddress}");

            return Results.Ok(user);
        }

        public async Task<Result> MakeMerchantDeposit(MerchantCommands.MakeMerchantDepositCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken, false);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Result<MerchantDepositListAggregate> depositListResult = await this.GetOrCreateMerchantDepositList(command, merchantAggregate, cancellationToken);
                if (depositListResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(depositListResult);

                PositiveMoney amount = PositiveMoney.Create(Money.Create(command.RequestDto.Amount));
                MerchantDepositSource depositSource = this.ConvertDepositSource(command.DepositSource);
                Result stateResult = depositListResult.Data.MakeDeposit(depositSource, command.RequestDto.Reference, command.RequestDto.DepositDateTime, amount, command.RequestDto.HostDateTime);
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(depositListResult.Data, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> MakeMerchantWithdrawal(MerchantCommands.MakeMerchantWithdrawalCommand command,
                                                               CancellationToken cancellationToken)
        {

            try
            {
                Result<MerchantDepositListAggregate> getDepositListResult = await this.GetMerchantDepositListForWithdrawal(command, cancellationToken);
                if (getDepositListResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getDepositListResult);

                Result validateBalanceResult = await this.ValidateWithdrawalBalance(command, cancellationToken);
                if (validateBalanceResult.IsSuccess() == false)
                    return validateBalanceResult;

                // If we are here we have enough credit to withdraw
                PositiveMoney amount = PositiveMoney.Create(Money.Create(command.RequestDto.Amount));
                MerchantDepositListAggregate merchantDepositListAggregate = getDepositListResult.Data;

                Result stateResult = merchantDepositListAggregate.MakeWithdrawal(command.RequestDto.WithdrawalDateTime, amount, command.RequestDto.HostDateTime);
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(merchantDepositListAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddContractToMerchant(MerchantCommands.AddMerchantContractCommand command, CancellationToken cancellationToken)
        {
            try {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Result<ContractAggregate> contractResult = await this.GetCreatedContract(command.RequestDto.ContractId, cancellationToken);
                if (contractResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contractResult);

                Result stateResult = merchantAggregate.AddContract(contractResult.Data);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> UpdateMerchant(MerchantCommands.UpdateMerchantCommand command, CancellationToken cancellationToken)
        {
            try {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Result stateResult = merchantAggregate.UpdateMerchant(command.RequestDto.Name);
                if (stateResult.IsSuccess() == false)
                    return stateResult;
                SettlementSchedule settlementSchedule = ConvertSettlementSchedule(command.RequestDto.SettlementSchedule);
                stateResult = merchantAggregate.SetSettlementSchedule(settlementSchedule);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddMerchantAddress(MerchantCommands.AddMerchantAddressCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Address address = Address.Create(Guid.Empty, new AddressLines(command.RequestDto.AddressLine1, command.RequestDto.AddressLine2, command.RequestDto.AddressLine3, command.RequestDto.AddressLine4), command.RequestDto.Town, command.RequestDto.Region, command.RequestDto.PostalCode, command.RequestDto.Country);

                Result stateResult = merchantAggregate.AddAddress(address);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> UpdateMerchantAddress(MerchantCommands.UpdateMerchantAddressCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Address address = Address.Create(command.AddressId, new AddressLines(command.RequestDto.AddressLine1, command.RequestDto.AddressLine2, command.RequestDto.AddressLine3, command.RequestDto.AddressLine4), command.RequestDto.Town, command.RequestDto.Region, command.RequestDto.PostalCode, command.RequestDto.Country);

                Result stateResult = merchantAggregate.UpdateAddress(address);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddMerchantContact(MerchantCommands.AddMerchantContactCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Result stateResult = merchantAggregate.AddContact(command.RequestDto.ContactName,
                                                                 command.RequestDto.PhoneNumber,
                                                                 command.RequestDto.EmailAddress);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> UpdateMerchantContact(MerchantCommands.UpdateMerchantContactCommand command,
                                                        CancellationToken cancellationToken) {
            try {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Result stateResult = merchantAggregate.UpdateContact(command.ContactId, command.RequestDto.ContactName, command.RequestDto.EmailAddress, command.RequestDto.PhoneNumber);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> RemoveOperatorFromMerchant(MerchantCommands.RemoveOperatorFromMerchantCommand command, CancellationToken cancellationToken)
        {
            try {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Result stateResult = merchantAggregate.RemoveOperator(command.OperatorId);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> RemoveContractFromMerchant(MerchantCommands.RemoveMerchantContractCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Result stateResult = merchantAggregate.RemoveContract(command.ContractId);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> UpdateMerchantOpeningHours(MerchantCommands.UpdateMerchantOpeningHoursCommand command,
                                                              CancellationToken cancellationToken) {
            try
            {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;
                var dayMappings = new List<(DayOfWeek Day, TransactionProcessor.DataTransferObjects.Requests.Merchant.OpeningHours Hours)>() {
                    (DayOfWeek.Monday, command.RequestDto.Monday),
                    (DayOfWeek.Tuesday, command.RequestDto.Tuesday),
                    (DayOfWeek.Wednesday, command.RequestDto.Wednesday),
                    (DayOfWeek.Thursday, command.RequestDto.Thursday),
                    (DayOfWeek.Friday, command.RequestDto.Friday),
                    (DayOfWeek.Saturday, command.RequestDto.Saturday),
                    (DayOfWeek.Sunday, command.RequestDto.Sunday),
                };

                foreach (var mapping in dayMappings)
                {
                    Result stateResult = ApplyOpeningHoursIfPresent(merchantAggregate, mapping.Day, mapping.Hours);
                    if (stateResult.IsSuccess() == false)
                        return stateResult;
                }

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> CreateMerchantSchedule(MerchantCommands.CreateMerchantScheduleCommand command,
                                                         CancellationToken cancellationToken) {
            try {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                Guid merchantScheduleId = IdGenerationService.GenerateMerchantScheduleAggregateId(command.EstateId, command.MerchantId, command.RequestDto.Year);
                Result<MerchantScheduleAggregate> merchantScheduleResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantScheduleAggregate>(merchantScheduleId, ct), merchantScheduleId, cancellationToken, false);
                if (merchantScheduleResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(merchantScheduleResult);

                MerchantScheduleAggregate merchantScheduleAggregate = merchantScheduleResult.Data;
                Result stateResult = merchantScheduleAggregate.Create(command.EstateId, command.MerchantId, command.RequestDto.Year, ConvertScheduleMonths(command.RequestDto.Months));
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantScheduleAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> UpdateMerchantSchedule(MerchantCommands.UpdateMerchantScheduleCommand command,
                                                         CancellationToken cancellationToken) {
            try {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                Guid merchantScheduleId = IdGenerationService.GenerateMerchantScheduleAggregateId(command.EstateId, command.MerchantId, command.Year);
                Result<MerchantScheduleAggregate> merchantScheduleResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantScheduleAggregate>(merchantScheduleId, ct), merchantScheduleId, cancellationToken, false);
                if (merchantScheduleResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(merchantScheduleResult);

                MerchantScheduleAggregate merchantScheduleAggregate = merchantScheduleResult.Data;

                Result stateResult = merchantScheduleAggregate.UpdateSchedule(ConvertScheduleMonths(command.RequestDto.Months));
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantScheduleAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        private Result ApplyOpeningHoursIfPresent(MerchantAggregate merchantAggregate, DayOfWeek day, TransactionProcessor.DataTransferObjects.Requests.Merchant.OpeningHours hours)
        {
            if (hours == null)
                return Results.Ok();

            return merchantAggregate.SetDayOpeningHours(day, hours.Opening, hours.Closing);
        }

        private sealed record ValidatedMerchantContext(EstateAggregate EstateAggregate,
                                                       MerchantAggregate MerchantAggregate);

        private async Task<Result<ValidatedMerchantContext>> GetValidatedMerchantContext(Guid estateId,
                                                                                         Guid merchantId,
                                                                                         CancellationToken cancellationToken,
                                                                                         Boolean useLatestMerchant = true) {
            Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<EstateAggregate>(estateId, ct), estateId, cancellationToken);
            if (estateResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(estateResult);

            Func<CancellationToken, Task<Result<MerchantAggregate>>> getMerchant = useLatestMerchant
                ? ct => this.AggregateService.GetLatest<MerchantAggregate>(merchantId, ct)
                : ct => this.AggregateService.Get<MerchantAggregate>(merchantId, ct);

            Result<MerchantAggregate> merchantResult = await DomainServiceHelper.GetAggregateOrFailure(getMerchant, merchantId, cancellationToken);
            if (merchantResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(merchantResult);

            Result validateResult = this.ValidateEstateAndMerchant(estateResult.Data, merchantResult.Data);
            if (validateResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(validateResult);

            return Results.Ok(new ValidatedMerchantContext(estateResult.Data, merchantResult.Data));
        }

        private Result ValidateEstateAndMerchant(EstateAggregate estateAggregate,
                                                 MerchantAggregate merchantAggregate) {

            // Check merchant has been created
            if (merchantAggregate.IsCreated == false) {
                return new Error(ErrorType.Validation, $"Merchant Id {merchantAggregate.AggregateId} has not been created");
            }

            // Estate Id is a valid estate
            if (estateAggregate.IsCreated == false) {
                return new Error(ErrorType.Validation, $"Estate Id {estateAggregate.AggregateId} has not been created");
            }

            return Results.Ok();
        }

        private static List<MerchantScheduleMonth> ConvertScheduleMonths(IEnumerable<TransactionProcessor.DataTransferObjects.Requests.MerchantSchedule.MerchantScheduleMonthRequest> months) =>
            months?.Select(month => new MerchantScheduleMonth
            {
                Month = month.Month,
                ClosedDays = month.ClosedDays ?? []
            }).ToList() ?? [];

        private async Task<Result<ContractAggregate>> GetCreatedContract(Guid contractId,
                                                                          CancellationToken cancellationToken)
        {
            Result<ContractAggregate> contractResult = await this.AggregateService.Get<ContractAggregate>(contractId, cancellationToken);
            if (contractResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(contractResult);

            ContractAggregate contractAggregate = contractResult.Data;
            if (contractAggregate.IsCreated == false)
                return new Error(ErrorType.Validation, $"Contract Id {contractId} has not been created");

            return Results.Ok(contractAggregate);
        }

        private async Task<Result<MerchantDepositListAggregate>> GetOrCreateMerchantDepositList(MerchantCommands.MakeMerchantDepositCommand command,
                                                                                                MerchantAggregate merchantAggregate,
                                                                                                CancellationToken cancellationToken)
        {
            Result<MerchantDepositListAggregate> getDepositListResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantDepositListAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken, false);
            if (getDepositListResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(getDepositListResult);

            MerchantDepositListAggregate merchantDepositListAggregate = getDepositListResult.Data;
            Result createResult = this.EnsureMerchantDepositListCreated(merchantDepositListAggregate, merchantAggregate, command.RequestDto.DepositDateTime);
            if (createResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(createResult);

            return Results.Ok(merchantDepositListAggregate);
        }

        private async Task<Result<MerchantDepositListAggregate>> GetMerchantDepositListForWithdrawal(MerchantCommands.MakeMerchantWithdrawalCommand command,
                                                                                                      CancellationToken cancellationToken)
        {
            Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken, false);
            if (contextResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(contextResult);

            Result<MerchantDepositListAggregate> getDepositListResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<MerchantDepositListAggregate>(command.MerchantId, ct), command.MerchantId, cancellationToken);
            if (getDepositListResult.IsSuccess() == false)
                return ResultHelpers.CreateFailure(getDepositListResult);

            MerchantDepositListAggregate merchantDepositListAggregate = getDepositListResult.Data;
            if (merchantDepositListAggregate.IsCreated == false)
            {
                return new Error(ErrorType.Validation, $"Merchant [{command.MerchantId}] has not made any deposits yet");
            }

            return Results.Ok(merchantDepositListAggregate);
        }

        private async Task<Result> ValidateWithdrawalBalance(MerchantCommands.MakeMerchantWithdrawalCommand command,
                                                             CancellationToken cancellationToken)
        {
            Result<String> getBalanceResult = await this.EventStoreContext.GetPartitionStateFromProjection("MerchantBalanceProjection", $"MerchantBalance-{command.MerchantId:N}", cancellationToken);
            if (getBalanceResult.IsSuccess() == false)
            {
                return new Error(ErrorType.Validation, $"Failed to get Merchant Balance.");
            }

            MerchantBalanceProjectionState1 projectionState = StringSerialiser.Deserialise<MerchantBalanceProjectionState1>(getBalanceResult.Data);
            if (projectionState?.merchant == null)
            {
                return new Error(ErrorType.Validation, "Merchant Balance data is missing or invalid.");
            }

            if (command.RequestDto.Amount > projectionState.merchant.balance)
            {
                return new Error(ErrorType.Validation, $"Not enough credit available for withdrawal of [{command.RequestDto.Amount}]. Balance is {projectionState.merchant.balance}");
            }

            return Results.Ok();
        }

        public async Task<Result> SwapMerchantDevice(MerchantCommands.SwapMerchantDeviceCommand command,
                                                     CancellationToken cancellationToken)
        {
            try
            {
                Result<ValidatedMerchantContext> contextResult = await this.GetValidatedMerchantContext(command.EstateId, command.MerchantId, cancellationToken);
                if (contextResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(contextResult);

                MerchantAggregate merchantAggregate = contextResult.Data.MerchantAggregate;

                Result stateResult = merchantAggregate.SwapDevice(command.DeviceIdentifier, command.RequestDto.NewDeviceIdentifier);
                if (stateResult.IsSuccess() == false)
                    return stateResult;

                Result saveResult = await this.AggregateService.Save(merchantAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return saveResult;
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }
    }
}
