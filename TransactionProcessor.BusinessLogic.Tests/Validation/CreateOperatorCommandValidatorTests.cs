using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Validation;
using TransactionProcessor.DataTransferObjects.Requests.Operator;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Validation;

public sealed class CreateOperatorCommandValidatorTests
{
    private readonly CreateOperatorCommandValidator Validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_MissingOperatorName_ReturnsNameValidationError(String operatorName)
    {
        OperatorCommands.CreateOperatorCommand command = new(
            Guid.NewGuid(),
            new CreateOperatorRequest { Name = operatorName });

        FluentValidation.Results.ValidationResult result =
            await this.Validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorMessage)
            .ShouldContain("Operator name is required");
    }

    [Fact]
    public async Task Validate_NullRequestDto_ReturnsRequestValidationError()
    {
        OperatorCommands.CreateOperatorCommand command = new(Guid.NewGuid(), null);

        FluentValidation.Results.ValidationResult result =
            await this.Validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorMessage)
            .ShouldContain("Request DTO must not be null");
    }

    [Fact]
    public async Task Validate_ValidOperatorName_ReturnsValid()
    {
        OperatorCommands.CreateOperatorCommand command = new(
            Guid.NewGuid(),
            new CreateOperatorRequest { Name = "Example Operator" });

        FluentValidation.Results.ValidationResult result =
            await this.Validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeTrue();
    }
}
