namespace TransactionProcessor.Common.Examples;

using System.Diagnostics.CodeAnalysis;
using DataTransferObjects;
using Swashbuckle.AspNetCore.Filters;

[ExcludeFromCodeCoverage]
public class RedeemVoucherResponseExample : IExamplesProvider<RedeemVoucherResponse>
{
    /// <summary>
    /// Gets the examples.
    /// </summary>
    /// <returns></returns>
    public RedeemVoucherResponse GetExamples()
    {
        return new RedeemVoucherResponse
               {
                   VoucherCode = ExampleData.VoucherCode,
                   ExpiryDate = ExampleData.ExpiryDate,
                   RemainingBalance = ExampleData.RemainingBalance
               };
    }
}