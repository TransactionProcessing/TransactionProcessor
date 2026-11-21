#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace TransactionProcessing.UnionResults;

internal static class ResultHelpers {
    internal static ErrorList CreateFailure(Result result) => CreateFailure(result.GetErrors());

    internal static ErrorList CreateFailure<T>(Result<T> result) => CreateFailure(result.GetErrors());

    internal static ErrorList CreateFailure(IEnumerable<string> messages) =>
        new(messages.Select(message => new Error(ErrorType.Failure, message)));

    private static ErrorList CreateFailure(IReadOnlyList<Error> errors) => errors switch {
        ErrorList list => list,
        { Count: > 0 } => new ErrorList(errors),
        _ => throw new InvalidOperationException("Only a failed result can be propagated as a failure.")
    };

    extension<T>(Result<T> result) {
        public T Data => result.Value switch {
                             Success<T> success => success.Value,

                             _ => throw new InvalidOperationException(
                                 "Cannot retrieve Data from an unsuccessful result.")
                         };
    }
}
