using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Validation;

public sealed class ValidationBehavior<TRequest>(IValidator<TRequest> validator) : IPipelineBehavior<TRequest, Result>
    where TRequest : notnull
{
    public async Task<Result> Handle(TRequest request,
                                     RequestHandlerDelegate<Result> next,
                                     CancellationToken cancellationToken)
    {
        FluentValidation.Results.ValidationResult validationResult =
            await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false)
            return Result.Invalid(validationResult.Errors.Select(error => error.ErrorMessage));

        return await next(cancellationToken);
    }
}

public sealed class ValidationBehavior<TRequest, TValue>(IValidator<TRequest> validator) : IPipelineBehavior<TRequest, Result<TValue>>
    where TRequest : notnull
{
    public async Task<Result<TValue>> Handle(TRequest request,
                                             RequestHandlerDelegate<Result<TValue>> next,
                                             CancellationToken cancellationToken)
    {
        FluentValidation.Results.ValidationResult validationResult =
            await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false)
            return Result.Invalid(validationResult.Errors.Select(error => error.ErrorMessage));

        return await next(cancellationToken);
    }
}
