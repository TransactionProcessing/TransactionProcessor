namespace TransactionProcessor.Common.Examples;

using DataTransferObjects;
using Swashbuckle.AspNetCore.Filters;

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