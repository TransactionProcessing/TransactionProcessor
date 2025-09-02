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
using TransactionProcessor.BusinessLogic.Common;
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
                
        public EstateDomainService(Func<IAggregateService> aggregateService,
                                   ISecurityServiceClient securityServiceClient) {
            this.AggregateService = aggregateService();
            this.SecurityServiceClient = securityServiceClient;
        }

        #endregion

        #region Methods
        
        public async Task<Result> CreateEstate(EstateCommands.CreateEstateCommand command,
                                               CancellationToken cancellationToken) {
            try {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<EstateAggregate>(command.RequestDto.EstateId, ct), command.RequestDto.EstateId, cancellationToken, false);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                EstateAggregate estateAggregate = estateResult.Data;

                Result stateResult = estateAggregate.Create(command.RequestDto.EstateName);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

                Result saveResult = await this.AggregateService.Save(estateAggregate, cancellationToken);
                if (saveResult.IsFailed)
                    return ResultHelpers.CreateFailure(saveResult);

                return Result.Success();
            }
            catch (Exception ex) {
                return Result.Failure(ex.GetExceptionMessages());
            }
        }

        public async Task<Result> AddOperatorToEstate(EstateCommands.AddOperatorToEstateCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<OperatorAggregate> operatorResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.Get<OperatorAggregate>(command.RequestDto.OperatorId, ct), command.RequestDto.OperatorId, cancellationToken);
                if (operatorResult.IsFailed)
                    return ResultHelpers.CreateFailure(operatorResult);

                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                EstateAggregate estateAggregate = estateResult.Data;

                Result stateResult =estateAggregate.AddOperator(command.RequestDto.OperatorId);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

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

        public async Task<Result> CreateEstateUser(EstateCommands.CreateEstateUserCommand command, CancellationToken cancellationToken)
        {
            try
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

                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                EstateAggregate estateAggregate = estateResult.Data;

                Result stateResult = estateAggregate.AddSecurityUser(user.UserId, command.RequestDto.EmailAddress);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

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

        public async Task<Result> RemoveOperatorFromEstate(EstateCommands.RemoveOperatorFromEstateCommand command, CancellationToken cancellationToken)
        {
            try
            {
                Result<EstateAggregate> estateResult = await DomainServiceHelper.GetAggregateOrFailure(ct => this.AggregateService.GetLatest<EstateAggregate>(command.EstateId, ct), command.EstateId, cancellationToken);
                if (estateResult.IsFailed)
                    return ResultHelpers.CreateFailure(estateResult);

                EstateAggregate estateAggregate = estateResult.Data;

                Result stateResult = estateAggregate.RemoveOperator(command.OperatorId);
                if (stateResult.IsFailed)
                    return ResultHelpers.CreateFailure(stateResult);

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

        #endregion
    }
}
