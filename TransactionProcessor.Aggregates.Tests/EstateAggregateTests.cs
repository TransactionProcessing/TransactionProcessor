using Shouldly;
using TransactionProcessor.Models.Estate;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
    public class EstateAggregateTests
    {
        [Fact]
        public void EstateAggregate_CanBeCreated_IsCreated()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);

            aggregate.AggregateId.ShouldBe(TestData.EstateId);
        }

        [Fact]
        public void EstateAggregate_Create_IsCreated()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);
            aggregate.GenerateReference();

            aggregate.AggregateId.ShouldBe(TestData.EstateId);
            aggregate.EstateName.ShouldBe(TestData.EstateName);
            aggregate.IsCreated.ShouldBeTrue();
            aggregate.EstateReference.ShouldBe(TestData.EstateReference);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void EstateAggregate_Create_InvalidEstateName_ErrorThrown(String estateName)
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            ArgumentNullException exception = Should.Throw<ArgumentNullException>(() =>
                                                {
                                                    aggregate.Create(estateName);
                                                });
            
            exception.Message.ShouldContain("Estate name must be provided when registering a new estate");
        }

        [Fact]
        public void EstateAggregate_Create_EstateAlreadyCreated_NoErrorThrown()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);

            Should.NotThrow(() =>
                            {
                                aggregate.Create(TestData.EstateName);
                            });
        }

        [Fact]
        public void EstateAggregate_GenerateReference_CalledTwice_NoErrorThrown()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);
            aggregate.GenerateReference();

            Should.NotThrow(() =>
                            {
                                aggregate.GenerateReference();
                            });
        }

        [Fact]
        public void EstateAggregate_GetEstate_NoOperators_EstateIsReturned()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);
            aggregate.GenerateReference();
            TransactionProcessor.Models.Estate.Estate model = aggregate.GetEstate();

            model.EstateId.ShouldBe(TestData.EstateId);
            model.Name.ShouldBe(TestData.EstateName);
            model.Reference.ShouldBe(TestData.EstateReference);
            model.Operators.ShouldBeEmpty();
        }

        [Fact]
        public void EstateAggregate_GetEstate_WithAnOperator_EstateIsReturned()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);
            aggregate.GenerateReference();
            aggregate.AddOperator(TestData.OperatorId);

            TransactionProcessor.Models.Estate.Estate model = aggregate.GetEstate();

            model.EstateId.ShouldBe(TestData.EstateId);
            model.Name.ShouldBe(TestData.EstateName);
            model.Reference.ShouldBe(TestData.EstateReference);
            model.Operators.ShouldHaveSingleItem();
            
            Models.Estate.Operator? @operator =model.Operators.Single();
            @operator.OperatorId.ShouldBe(TestData.OperatorId);
        }

        [Fact]
        public void EstateAggregate_GetEstate_NoSecurityUsers_EstateIsReturned()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);
            aggregate.GenerateReference();
            TransactionProcessor.Models.Estate.Estate model = aggregate.GetEstate();

            model.EstateId.ShouldBe(TestData.EstateId);
            model.Name.ShouldBe(TestData.EstateName);
            model.Reference.ShouldBe(TestData.EstateReference);
            model.Operators.ShouldBeEmpty();
            model.SecurityUsers.ShouldBeEmpty();
        }

        [Fact]
        public void EstateAggregate_GetEstate_WithASecurityUser_EstateIsReturned()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);
            aggregate.GenerateReference();
            aggregate.AddSecurityUser(TestData.SecurityUserId,TestData.EstateUserEmailAddress);

            TransactionProcessor.Models.Estate.Estate model = aggregate.GetEstate();

            model.EstateId.ShouldBe(TestData.EstateId);
            model.Name.ShouldBe(TestData.EstateName);
            model.Reference.ShouldBe(TestData.EstateReference);
            model.SecurityUsers.ShouldHaveSingleItem();

            SecurityUser? securityUser = model.SecurityUsers.Single();
            securityUser.SecurityUserId.ShouldBe(TestData.SecurityUserId);
            securityUser.EmailAddress.ShouldBe(TestData.EstateUserEmailAddress);
        }

        [Fact]
        public void EstateAggregate_AddOperatorToEstate_OperatorIsAdded()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);

            aggregate.AddOperator(TestData.OperatorId);

            TransactionProcessor.Models.Estate.Estate estate = aggregate.GetEstate();
            estate.Operators.ShouldHaveSingleItem();
            estate.Operators.Single().OperatorId.ShouldBe(TestData.OperatorId);
            estate.Operators.Single().IsDeleted.ShouldBeFalse();
        }

        [Fact]
        public void EstateAggregate_AddOperatorToEstate_EstateNotCreated_ErrorThrown()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);

            InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
                                                                                  {
                                                                                      aggregate.AddOperator(TestData.OperatorId);
                                                                                  });

            exception.Message.ShouldContain("Estate has not been created");
        }
        
        [Fact]
        public void EstateAggregate_AddOperatorToEstate_OperatorWithIdAlreadyAdded_ErrorThrown()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);
            aggregate.AddOperator(TestData.OperatorId);

            InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
                                                                                  {
                                                                                      aggregate.AddOperator(TestData.OperatorId);
                                                                                  });

            exception.Message.ShouldContain($"Duplicate operator details are not allowed, an operator already exists on this estate with Id [{TestData.OperatorId}]");
        }

        [Fact]
        public void EstateAggregate_AddSecurityUserToEstate_SecurityUserIsAdded()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);
            aggregate.AddSecurityUser(TestData.SecurityUserId, TestData.EstateUserEmailAddress);

            TransactionProcessor.Models.Estate.Estate estate = aggregate.GetEstate();
            estate.SecurityUsers.ShouldHaveSingleItem();
            estate.SecurityUsers.Single().EmailAddress.ShouldBe(TestData.EstateUserEmailAddress);
        }

        [Fact]
        public void EstateAggregate_AddSecurityUserToEstate_EstateNotCreated_ErrorThrown()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);

            InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
                                                                                          {
                                                                                              aggregate.AddSecurityUser(TestData.SecurityUserId, TestData.EstateUserEmailAddress);
                                                                                          });

            exception.Message.ShouldContain("Estate has not been created");
        }

        [Fact]
        public void EstateAggregate_RemoveOperatorFromEstate_OperatorIsAdded()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);
            aggregate.AddOperator(TestData.OperatorId);

            aggregate.RemoveOperator(TestData.OperatorId);

            TransactionProcessor.Models.Estate.Estate estate = aggregate.GetEstate();
            estate.Operators.ShouldHaveSingleItem();
            estate.Operators.Single().IsDeleted.ShouldBeTrue();
        }

        [Fact]
        public void EstateAggregate_RemoveOperatorFromEstate_EstateNotCreated_ErrorThrown()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);

            InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
                                                                                          {
                                                                                              aggregate.RemoveOperator(TestData.OperatorId);
                                                                                          });

            exception.Message.ShouldContain("Estate has not been created");
        }

        [Fact]
        public void EstateAggregate_RemoveOperatorFromEstate_OperatorWithIdNotAlreadyAdded_ErrorThrown()
        {
            EstateAggregate aggregate = EstateAggregate.Create(TestData.EstateId);
            aggregate.Create(TestData.EstateName);

            InvalidOperationException exception = Should.Throw<InvalidOperationException>(() =>
                                                                                          {
                                                                                              aggregate.RemoveOperator(TestData.OperatorId);
                                                                                          });

            exception.Message.ShouldContain($"Operator not added to this Estate with Id [{TestData.OperatorId}]");
        }
    }
}
