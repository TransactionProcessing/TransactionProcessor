using FluentValidation;
using System;
using System.Linq.Expressions;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.DataTransferObjects.Requests.Float;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Requests.Operator;

namespace TransactionProcessor.BusinessLogic.Validation;


public abstract class RequestDtoValidator<TCommand, TRequestDto>
    : AbstractValidator<TCommand>
    where TRequestDto : class {
    protected RequestDtoValidator(
        Expression<Func<TCommand, TRequestDto>> requestDtoSelector) {
        RuleFor(requestDtoSelector)
            .NotNull()
            .WithMessage("Request DTO must not be null");
    }
}

public sealed class CreateEstateCommandValidator : RequestDtoValidator<EstateCommands.CreateEstateCommand, CreateEstateRequest>
{
    public CreateEstateCommandValidator() : base(command => command.RequestDto) {

        When(command => command.RequestDto is not null, () =>
        {
            RuleFor(command => command.RequestDto.EstateName)
                .NotEmpty()
                .WithMessage("Estate name is required");
        });
    }
}

public sealed class CreateMerchantCommandValidator : RequestDtoValidator<MerchantCommands.CreateMerchantCommand, CreateMerchantRequest> {
    public CreateMerchantCommandValidator() : base(command => command.RequestDto) {
        
        When(command => command.RequestDto is not null, () => {
            RuleFor(command => command.RequestDto.Name)
                .NotEmpty()
                .WithMessage("Merchant name is required");
        });
    }
}

public sealed class CreateOperatorCommandValidator : RequestDtoValidator<OperatorCommands.CreateOperatorCommand, CreateOperatorRequest> {
    public CreateOperatorCommandValidator() : base(command => command.RequestDto) {

        When(command => command.RequestDto is not null, () => {
            RuleFor(command => command.RequestDto.Name)
                .NotEmpty()
                .WithMessage("Operator name is required");
        });
    }
}

public sealed class CreateFloatCommandValidator : AbstractValidator<FloatCommands.CreateFloatCommand> {
    public CreateFloatCommandValidator() {

        RuleFor(command => command.OperatorId)
            .NotEqual(Guid.Empty)
            .WithMessage("Operator Id is required");
    }
}
