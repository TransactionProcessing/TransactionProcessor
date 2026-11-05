using FluentValidation;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Validation;

public sealed class CreateEstateCommandValidator : AbstractValidator<EstateCommands.CreateEstateCommand>
{
    public CreateEstateCommandValidator()
    {
        RuleFor(command => command.RequestDto)
            .NotNull()
            .WithMessage("Request DTO must not be null");

        When(command => command.RequestDto is not null, () =>
        {
            RuleFor(command => command.RequestDto.EstateName)
                .NotEmpty()
                .WithMessage("Estate name is required");
        });
    }
}

public sealed class CreateMerchantCommandValidator : AbstractValidator<MerchantCommands.CreateMerchantCommand> {
    public CreateMerchantCommandValidator() {
        RuleFor(command => command.RequestDto)
            .NotNull()
            .WithMessage("Request DTO must not be null");

        When(command => command.RequestDto is not null, () => {
            RuleFor(command => command.RequestDto.Name)
                .NotEmpty()
                .WithMessage("Merchant name is required");
        });
    }
}
