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
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    public class EstateDomainServiceTests {
        private EstateDomainService DomainService;
        private Mock<IAggregateRepository<EstateAggregate, DomainEvent>> EstateAggregateRepository;
        private Mock<ISecurityServiceClient> SecurityServiceClient;
        public EstateDomainServiceTests() {
            this.EstateAggregateRepository = new Mock<IAggregateRepository<EstateAggregate, DomainEvent>>();
            this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
            this.DomainService = new EstateDomainService(this.EstateAggregateRepository.Object, this.SecurityServiceClient.Object);
        }

        [Fact]
        public async Task EstateDomainService_CreateEstate_EstateIsCreated() {
            
            this.EstateAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success(new EstateAggregate()));
            this.EstateAggregateRepository
                .Setup(m => m.SaveChanges(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SimpleResults.Result.Success());
            
            Result result = await this.DomainService.CreateEstate(TestData.Commands.CreateEstateCommand, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        
        [Fact]
        public async Task EstateDomainService_AddOperatorEstate_OperatorIsAdded()
        {
            this.EstateAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.EstateAggregateRepository.Setup(m => m.SaveChanges(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(SimpleResults.Result.Success());

            Result result = await this.DomainService.AddOperatorToEstate(TestData.Commands.AddOperatorToEstateCommand, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Fact]
        public async Task EstateDomainService_RemoveOperatorFromEstate_OperatorIsRemoved()
        {
            this.EstateAggregateRepository.Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.Aggregates.EstateAggregateWithOperator()));
            this.EstateAggregateRepository.Setup(m => m.SaveChanges(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);
            Result result = await this.DomainService.RemoveOperatorFromEstate(TestData.Commands.RemoveOperatorFromEstateCommand, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task EstateDomainService_CreateEstateUser_EstateUserIsCreated() {
            this.EstateAggregateRepository
                .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.EstateAggregateRepository
                .Setup(m => m.SaveChanges(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
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
            this.EstateAggregateRepository
                .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.EstateAggregateRepository
                .Setup(m => m.SaveChanges(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
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
            this.EstateAggregateRepository
                .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.EstateAggregateRepository
                .Setup(m => m.SaveChanges(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
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
            this.EstateAggregateRepository
                .Setup(m => m.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.Aggregates.CreatedEstateAggregate()));
            this.EstateAggregateRepository
                .Setup(m => m.SaveChanges(It.IsAny<EstateAggregate>(), It.IsAny<CancellationToken>()))
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

        // TODO: EstateDomainServiceTests - CreateEstateUser - failed creating user test
        // TODO: EstateDomainServiceTests - Estate Not Created tests missing
        // TODO: EstateDomainServiceTests - Save Changes failed
    }
}
