using Imposter.Abstractions;
using SecurityService.Client;
using SecurityService.DataTransferObjects;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    public class EstateDomainServiceTests {
        private EstateDomainService DomainService;
        private IAggregateServiceImposter AggregateService;
        private ISecurityServiceClientImposter SecurityServiceClient;
        public EstateDomainServiceTests() {
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
            this.AggregateService= new IAggregateServiceImposter();
            this.SecurityServiceClient = new ISecurityServiceClientImposter();
            IAggregateService AggregateServiceResolver() => this.AggregateService.Instance();
            this.DomainService = new EstateDomainService(AggregateServiceResolver, this.SecurityServiceClient.Instance());
        }

        [Fact]
        public async Task EstateDomainService_CreateEstate_EstateIsCreated() {
            
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success(new EstateAggregate()));
            this.AggregateService
                .Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success());
            
            Result result = await this.DomainService.CreateEstate(TestData.Commands.CreateEstateCommand, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        
        [Fact]
        public async Task EstateDomainService_AddOperatorEstate_OperatorIsAdded()
        {
            this.AggregateService.Get<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(SimpleResults.Result.Success());

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_OperatorIsRemoved()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_EstateUserIsCreated() {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success());

            this.SecurityServiceClient
                .CreateUser(Arg<CreateUserRequest>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.SecurityServiceClient
                .GetUsers(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(new List<UserResponse>() {
                    new UserResponse {
                        UserId = "FA077CE3-B915-4048-88E3-9B500699317F"
                    }
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_EstateRoleOverrideIsUsedInCreateUserRequest() {
            String originalEstateRoleName = Environment.GetEnvironmentVariable("EstateRoleName");
            CreateUserRequest capturedRequest = null;

            try {
                Environment.SetEnvironmentVariable("EstateRoleName", "CustomEstateRole");

                this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                    .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
                this.AggregateService.Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any())
                    .ReturnsAsync(SimpleResults.Result.Success());

                this.SecurityServiceClient
                    .CreateUser(Arg<CreateUserRequest>.Any(), Arg<CancellationToken>.Any())
                    .ReturnsAsync(Result.Success())
                    .Callback((request, _) => { capturedRequest = request; return Task.CompletedTask; });
                this.SecurityServiceClient
                    .GetUsers(Arg<String>.Any(), Arg<CancellationToken>.Any())
                    .ReturnsAsync(Result.Success(new List<UserResponse>() {
                        new UserResponse {
                            UserId = "FA077CE3-B915-4048-88E3-9B500699317F"
                        }
                    }));

                Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, TestContext.Current.CancellationToken);

                result.IsSuccess.ShouldBeTrue();
                capturedRequest.ShouldNotBeNull();
                capturedRequest.Roles.ShouldContain("CustomEstateRole");
                capturedRequest.Claims["estateId"].ShouldBe(TestData.EstateId.ToString());
            }
            finally {
                Environment.SetEnvironmentVariable("EstateRoleName", originalEstateRoleName);
            }
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_UserCreateFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success());

            this.SecurityServiceClient
                .CreateUser(Arg<CreateUserRequest>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());
            this.SecurityServiceClient
                .GetUsers(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(new List<UserResponse>() {
                    new UserResponse {
                        UserId = "FA077CE3-B915-4048-88E3-9B500699317F"
                    }
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_GetUsersFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success());

            this.SecurityServiceClient
                .CreateUser(Arg<CreateUserRequest>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.SecurityServiceClient
                .GetUsers(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_NullUserReturned_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success());

            this.SecurityServiceClient
                .CreateUser(Arg<CreateUserRequest>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.SecurityServiceClient
                .GetUsers(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(new List<UserResponse>() {
                    null
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_GetEstateFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            this.SecurityServiceClient
                .CreateUser(Arg<CreateUserRequest>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.SecurityServiceClient
                .GetUsers(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(new List<UserResponse>() {
                    new UserResponse {
                        UserId = "FA077CE3-B915-4048-88E3-9B500699317F"
                    }
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_EstateNotCreated_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.AggregateService.Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success());

            this.SecurityServiceClient
                .CreateUser(Arg<CreateUserRequest>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.SecurityServiceClient
                .GetUsers(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(new List<UserResponse>() {
                    new UserResponse {
                        UserId = "FA077CE3-B915-4048-88E3-9B500699317F"
                    }
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            this.SecurityServiceClient
                .CreateUser(Arg<CreateUserRequest>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());
            this.SecurityServiceClient
                .GetUsers(Arg<String>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(new List<UserResponse>() {
                    new UserResponse {
                        UserId = "FA077CE3-B915-4048-88E3-9B500699317F"
                    }
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstate_GetEstateFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            Result result = await this.DomainService.CreateEstate(TestData.Commands.CreateEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstate_EstateNameEmpty_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success(new EstateAggregate()));

            EstateCommands.CreateEstateCommand emptyNameCommand = new EstateCommands.CreateEstateCommand(
                new CreateEstateRequest { EstateId = TestData.EstateId, EstateName = String.Empty });

            Result result = await this.DomainService.CreateEstate(emptyNameCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstate_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success(new EstateAggregate()));
            this.AggregateService
                .Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            Result result = await this.DomainService.CreateEstate(TestData.Commands.CreateEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_AddOperatorToEstate_GetOperatorFailed_ResultIsFailed()
        {
            this.AggregateService.Get<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_AddOperatorToEstate_GetEstateFailed_ResultIsFailed()
        {
            this.AggregateService.Get<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_AddOperatorToEstate_EstateNotCreated_ResultIsFailed()
        {
            this.AggregateService.Get<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyEstateAggregate));

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_AddOperatorToEstate_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.Get<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_GetEstateFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_EstateNotCreated_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_OperatorNotAdded_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Save(Arg<EstateAggregate>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Failure());

            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstate_ExceptionThrown_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ThrowsAsync(new Exception());

            Result result = await this.DomainService.CreateEstate(TestData.Commands.CreateEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_AddOperatorToEstate_ExceptionThrown_ResultIsFailed()
        {
            this.AggregateService.Get<OperatorAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ThrowsAsync(new Exception());

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_ExceptionThrown_ResultIsFailed()
        {
            this.SecurityServiceClient
                .CreateUser(Arg<CreateUserRequest>.Any(), Arg<CancellationToken>.Any())
                .ThrowsAsync(new Exception());

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_ExceptionThrown_ResultIsFailed()
        {
            this.AggregateService.GetLatest<EstateAggregate>(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                .ThrowsAsync(new Exception());

            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
        }
    }
}

