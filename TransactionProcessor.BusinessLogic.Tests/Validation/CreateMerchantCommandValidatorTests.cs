using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Validation;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Validation;

public sealed class CreateMerchantCommandValidatorTests
{
    private readonly CreateMerchantCommandValidator Validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_MissingMerchantName_ReturnsNameValidationError(String merchantName)
    {
        MerchantCommands.CreateMerchantCommand command = new(
            Guid.NewGuid(),
            new CreateMerchantRequest { Name = merchantName });

        FluentValidation.Results.ValidationResult result =
            await this.Validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorMessage)
            .ShouldContain("Merchant name is required");
    }

    [Fact]
    public async Task Validate_NullRequestDto_ReturnsRequestValidationError()
    {
        MerchantCommands.CreateMerchantCommand command = new(Guid.NewGuid(), null);

        FluentValidation.Results.ValidationResult result =
            await this.Validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorMessage)
            .ShouldContain("Request DTO must not be null");
    }

    [Fact]
    public async Task Validate_ValidMerchantName_ReturnsValid()
    {
        MerchantCommands.CreateMerchantCommand command = new(
            Guid.NewGuid(),
            new CreateMerchantRequest { Name = "Example Merchant" });

        FluentValidation.Results.ValidationResult result =
            await this.Validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeTrue();
    }
}
