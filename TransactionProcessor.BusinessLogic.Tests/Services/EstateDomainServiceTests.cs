using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SecurityService.Client;
using SecurityService.DataTransferObjects;
using SecurityService.DataTransferObjects.Responses;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shouldly;
using SimpleResults;
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
        private Mock<IAggregateService> AggregateService;
        private Mock<ISecurityServiceClient> SecurityServiceClient;
        public EstateDomainServiceTests() {
            this.AggregateService= new Mock<IAggregateService>();
            this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
            IAggregateService AggregateServiceResolver() => this.AggregateService.Object;
            this.DomainService = new EstateDomainService(AggregateServiceResolver, this.SecurityServiceClient.Object);
        }

        [Fact]
        public async Task EstateDomainService_CreateEstate_EstateIsCreated() {
            
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(new EstateAggregate()));
            this.AggregateService
                .Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success());
            
            Result result = await this.DomainService.CreateEstate(TestData.Commands.CreateEstateCommand, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        
        [Fact]
        public async Task EstateDomainService_AddOperatorEstate_OperatorIsAdded()
        {
            this.AggregateService.Setup(m => m.Get<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));

            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(SimpleResults.Result.Success());

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_OperatorIsRemoved()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_EstateUserIsCreated() {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success());

            this.SecurityServiceClient
                .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.SecurityServiceClient
                .Setup(s => s.GetUsers(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new List<UserDetails>() {
                    new UserDetails {
                        UserId = Guid.Parse("FA077CE3-B915-4048-88E3-9B500699317F")
                    }
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_UserCreateFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success());

            this.SecurityServiceClient
                .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure);
            this.SecurityServiceClient
                .Setup(s => s.GetUsers(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new List<UserDetails>() {
                    new UserDetails {
                        UserId = Guid.Parse("FA077CE3-B915-4048-88E3-9B500699317F")
                    }
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_GetUsersFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success());

            this.SecurityServiceClient
                .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.SecurityServiceClient
                .Setup(s => s.GetUsers(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_NullUserReturned_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success());

            this.SecurityServiceClient
                .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.SecurityServiceClient
                .Setup(s => s.GetUsers(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new List<UserDetails>() {
                    null
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_GetEstateFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            this.SecurityServiceClient
                .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.SecurityServiceClient
                .Setup(s => s.GetUsers(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new List<UserDetails>() {
                    new UserDetails {
                        UserId = Guid.Parse("FA077CE3-B915-4048-88E3-9B500699317F")
                    }
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_EstateNotCreated_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));
            this.AggregateService.Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success());

            this.SecurityServiceClient
                .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.SecurityServiceClient
                .Setup(s => s.GetUsers(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new List<UserDetails>() {
                    new UserDetails {
                        UserId = Guid.Parse("FA077CE3-B915-4048-88E3-9B500699317F")
                    }
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure);

            this.SecurityServiceClient
                .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success);
            this.SecurityServiceClient
                .Setup(s => s.GetUsers(It.IsAny<String>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new List<UserDetails>() {
                    new UserDetails {
                        UserId = Guid.Parse("FA077CE3-B915-4048-88E3-9B500699317F")
                    }
                }));

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstate_GetEstateFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            Result result = await this.DomainService.CreateEstate(TestData.Commands.CreateEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstate_EstateNameEmpty_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(new EstateAggregate()));

            EstateCommands.CreateEstateCommand emptyNameCommand = new EstateCommands.CreateEstateCommand(
                new CreateEstateRequest { EstateId = TestData.EstateId, EstateName = String.Empty });

            Result result = await this.DomainService.CreateEstate(emptyNameCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstate_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(new EstateAggregate()));
            this.AggregateService
                .Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure);

            Result result = await this.DomainService.CreateEstate(TestData.Commands.CreateEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_AddOperatorToEstate_GetOperatorFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.Get<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_AddOperatorToEstate_GetEstateFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.Get<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_AddOperatorToEstate_EstateNotCreated_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.Get<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.EmptyEstateAggregate));

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_AddOperatorToEstate_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.Get<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedOperatorAggregate()));
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.AggregateService.Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure);

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_GetEstateFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure());

            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_EstateNotCreated_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.EmptyEstateAggregate));

            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_OperatorNotAdded_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));

            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_SaveFailed_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.AggregateService.Setup(m => m.Save(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure);

            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstate_ExceptionThrown_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            Result result = await this.DomainService.CreateEstate(TestData.Commands.CreateEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_AddOperatorToEstate_ExceptionThrown_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.Get<OperatorAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_ExceptionThrown_ResultIsFailed()
        {
            this.SecurityServiceClient
                .Setup(s => s.CreateUser(It.IsAny<CreateUserRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            Result result = await this.DomainService.CreateEstateUser(TestData.Commands.CreateEstateUserCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_ExceptionThrown_ResultIsFailed()
        {
            this.AggregateService.Setup(m => m.GetLatest<EstateAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
        }
    }
}
