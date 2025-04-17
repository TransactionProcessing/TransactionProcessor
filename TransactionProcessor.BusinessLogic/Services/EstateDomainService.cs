using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecurityService.Client;
using SecurityService.DataTransferObjects;
using SecurityService.DataTransferObjects.Responses;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Services
{
    public interface IEstateDomainService
    {
        #region Methods

        Task<Result> CreateEstate(EstateCommands.CreateEstateCommand command, CancellationToken cancellationToken);

        Task<Result> AddOperatorToEstate(EstateCommands.AddOperatorToEstateCommand command, CancellationToken cancellationToken);

        Task<Result> CreateEstateUser(EstateCommands.CreateEstateUserCommand command, CancellationToken cancellationToken);

        Task<Result> RemoveOperatorFromEstate(EstateCommands.RemoveOperatorFromEstateCommand command, CancellationToken cancellationToken);

        #endregion
    }

    public class EstateDomainService : IEstateDomainService
    {
        #region Fields

        private readonly IAggregateService AggregateService;
        private readonly ISecurityServiceClient SecurityServiceClient;

        #endregion

        #region Constructors
                
        public EstateDomainService(IAggregateService aggregateService,
                                   ISecurityServiceClient securityServiceClient) {
            this.AggregateService = aggregateService;
            this.SecurityServiceClient = securityServiceClient;
        }

        #endregion

        #region Methods

        private async Task<Result> ApplyUpdates(Action<EstateAggregate> action, Guid estateId, CancellationToken cancellationToken, Boolean isNotFoundError = true)
        {
            try
            {
                Result<EstateAggregate> getLatestVersionResult = await this.AggregateService.GetLatest<EstateAggregate>(estateId, cancellationToken);
                Result<EstateAggregate> estateAggregateResult =
                    DomainServiceHelper.HandleGetAggregateResult(getLatestVersionResult, estateId, isNotFoundError);
                if (estateAggregateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateAggregateResult);

                EstateAggregate estateAggregate = estateAggregateResult.Data;

                action(estateAggregate);

                Result saveResult = await this.AggregateService.Save(estateAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }


        public async Task<Result> CreateEstate(EstateCommands.CreateEstateCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates((estateAggregate) => {
                estateAggregate.Create(command.RequestDto.EstateName);
                estateAggregate.GenerateReference();
            }, command.RequestDto.EstateId, cancellationToken, false);

            return result;
        }

        public async Task<Result> AddOperatorToEstate(EstateCommands.AddOperatorToEstateCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates((estateAggregate) => {
                estateAggregate.AddOperator(command.RequestDto.OperatorId);
            }, command.EstateId, cancellationToken);

            return result;
        }

        public async Task<Result> CreateEstateUser(EstateCommands.CreateEstateUserCommand command, CancellationToken cancellationToken)
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

            // Check if role has been overridden
            String estateRoleName = Environment.GetEnvironmentVariable("EstateRoleName");
            createUserRequest.Roles.Add(String.IsNullOrEmpty(estateRoleName) ? "Estate" : estateRoleName);
            createUserRequest.Claims.Add("estateId", command.EstateId.ToString());

            Result createUserResult = await this.SecurityServiceClient.CreateUser(createUserRequest, cancellationToken);
            if (createUserResult.IsFailed)
                return ResultHelpers.CreateFailure(createUserResult);

            Result<List<UserDetails>> userDetailsResult = await this.SecurityServiceClient.GetUsers(createUserRequest.EmailAddress, cancellationToken);
            if (userDetailsResult.IsFailed)
                return ResultHelpers.CreateFailure(userDetailsResult);

            UserDetails user = userDetailsResult.Data.SingleOrDefault();
            if (user == null)
                return Result.Failure($"Unable to get user details for username {createUserRequest.EmailAddress}");

            Result result = await ApplyUpdates((estateAggregate) => {
                // Add the user to the aggregate 
                estateAggregate.AddSecurityUser(user.UserId, command.RequestDto.EmailAddress);
            }, command.EstateId, cancellationToken);

            return result;
        }

        public async Task<Result> RemoveOperatorFromEstate(EstateCommands.RemoveOperatorFromEstateCommand command, CancellationToken cancellationToken)
        {
            Result result = await ApplyUpdates((estateAggregate) => {
                estateAggregate.RemoveOperator(command.OperatorId);
            }, command.EstateId, cancellationToken);

            return result;
        }

        #endregion
    }
}
