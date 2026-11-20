using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecurityService.Client;
using SecurityService.DataTransferObjects;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.Exceptions;
using TransactionProcessing.UnionResults;
using TransactionProcessing.UnionResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Services
{
    public interface IEstateDomainService
    {
        Task<Result<Guid>> CreateEstate(EstateCommands.CreateEstateCommand command, CancellationToken cancellationToken);

        Task<Result> AddOperatorToEstate(EstateCommands.AddOperatorToEstateCommand command, CancellationToken cancellationToken);

        Task<Result> CreateEstateUser(EstateCommands.CreateEstateUserCommand command, CancellationToken cancellationToken);

        Task<Result> RemoveOperatorFromEstate(EstateCommands.RemoveOperatorFromEstateCommand command, CancellationToken cancellationToken);
    }

    public class EstateDomainService : IEstateDomainService
    {
        private readonly IAggregateService AggregateService;
        private readonly ISecurityServiceClient SecurityServiceClient;

        private TokenResponse TokenResponse;

        public EstateDomainService(Func<IAggregateService> aggregateService,
                                   ISecurityServiceClient securityServiceClient) {
            this.AggregateService = aggregateService();
            this.SecurityServiceClient = securityServiceClient;
        }

        public async Task<Result<Guid>> CreateEstate(EstateCommands.CreateEstateCommand command,
                                               CancellationToken cancellationToken) {
            try {
                Guid estateId = IdGenerationService.GenerateEstateId(command.RequestDto.EstateName);
                
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<EstateAggregate>(estateId, ct), estateId, cancellationToken, false);
                if (estateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(estateResult);

                EstateAggregate estateAggregate = estateResult.Data;

                Result stateResult = estateAggregate.Create(command.RequestDto.EstateName);
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(estateAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return Results.Ok<Guid>(estateId);
            }
            catch (Exception ex) {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddOperatorToEstate(EstateCommands.AddOperatorToEstateCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<OperatorAggregate> operatorResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<OperatorAggregate>(command.RequestDto.OperatorId, ct), command.RequestDto.OperatorId, cancellationToken);
                if (operatorResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(operatorResult);

                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(estateResult);

                EstateAggregate estateAggregate = estateResult.Data;

                Result stateResult =estateAggregate.AddOperator(command.RequestDto.OperatorId);
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(estateAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return Results.Ok();
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> CreateEstateUser(EstateCommands.CreateEstateUserCommand command, CancellationToken cancellationToken)
        {
            try
            {
                CreateUserRequest createUserRequest = this.BuildEstateUserRequest(command);

                Result<UserResponse> getUserResult = await this.CreateEstateSecurityUser(createUserRequest, cancellationToken);
                if (getUserResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(getUserResult);

                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(estateResult);

                EstateAggregate estateAggregate = estateResult.Data;

                Result stateResult = estateAggregate.AddSecurityUser(Guid.Parse(getUserResult.Data.UserId), command.RequestDto.EmailAddress);
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(estateAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return Results.Ok();
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }

        private CreateUserRequest BuildEstateUserRequest(EstateCommands.CreateEstateUserCommand command) {
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

            String estateRoleName = Environment.GetEnvironmentVariable("EstateRoleName");
            createUserRequest.Roles.Add(String.IsNullOrEmpty(estateRoleName) ? "Estate" : estateRoleName);
            createUserRequest.Claims.Add("estateId", command.EstateId.ToString());

            return createUserRequest;
        }

        private async Task<Result<UserResponse>> CreateEstateSecurityUser(CreateUserRequest createUserRequest,
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

        public async Task<Result> RemoveOperatorFromEstate(EstateCommands.RemoveOperatorFromEstateCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(estateResult);

                EstateAggregate estateAggregate = estateResult.Data;

                Result stateResult = estateAggregate.RemoveOperator(command.OperatorId);
                if (stateResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(estateAggregate, cancellationToken);
                if (saveResult.IsSuccess() == false)
                    return ResultHelpers.CreateFailure(saveResult);

                return Results.Ok();
            }
            catch (Exception ex)
            {
                return ResultHelpers.CreateFailure(ex.GetExceptionMessages());
            }
        }
    }
}
