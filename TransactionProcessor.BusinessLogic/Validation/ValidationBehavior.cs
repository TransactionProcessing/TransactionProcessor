using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TransactionProcessing.UnionResults;

namespace TransactionProcessor.BusinessLogic.Validation;

public sealed class ValidationBehavior<TRequest>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, Result>
    where TRequest : notnull, IRequest<Result>
{
    public async Task<Result> Handle(TRequest request,
                                     RequestHandlerDelegate<Result> next,
                                     CancellationToken cancellationToken)
    {
        IValidator<TRequest>? validator = validators.SingleOrDefault();
        if (validator is null)
            return await next(cancellationToken);

        FluentValidation.Results.ValidationResult validationResult =
            await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false)
            return Result.Invalid(validationResult.Errors.Select(error => error.ErrorMessage));

        return await next(cancellationToken);
    }
}

public sealed class ValidationBehavior<TRequest, TValue>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, Result<TValue>>
    where TRequest : notnull, IRequest<Result<TValue>>
{
    public async Task<Result<TValue>> Handle(TRequest request,
                                             RequestHandlerDelegate<Result<TValue>> next,
                                             CancellationToken cancellationToken)
    {
        IValidator<TRequest>? validator = validators.SingleOrDefault();
        if (validator is null)
            return await next(cancellationToken);

        FluentValidation.Results.ValidationResult validationResult =
            await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false)
            return Result.Invalid(validationResult.Errors.Select(error => error.ErrorMessage));

        return await next(cancellationToken);
    }
}
