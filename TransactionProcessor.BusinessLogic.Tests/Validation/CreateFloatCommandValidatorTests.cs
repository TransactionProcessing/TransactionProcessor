using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Validation;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Validation;

public sealed class CreateFloatCommandValidatorTests
{
    private readonly CreateFloatCommandValidator Validator = new();

    [Fact]
    public async Task Validate_EmptyOperatorId_ReturnsOperatorIdValidationError()
    {
        FloatCommands.CreateFloatCommand command = new(
            Guid.NewGuid(),
            Guid.Empty,
            DateTimeOffset.UnixEpoch);

        FluentValidation.Results.ValidationResult result =
            await this.Validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorMessage)
            .ShouldContain("Operator Id is required");
    }

    [Fact]
    public async Task Validate_NonEmptyOperatorId_ReturnsValid()
    {
        FloatCommands.CreateFloatCommand command = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UnixEpoch);

        FluentValidation.Results.ValidationResult result =
            await this.Validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeTrue();
    }
}
