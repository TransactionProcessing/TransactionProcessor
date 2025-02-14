namespace TransactionProcessor.Common.Examples;

using System.Diagnostics.CodeAnalysis;
using DataTransferObjects;
using Swashbuckle.AspNetCore.Filters;

[ExcludeFromCodeCoverage]
public class GetVoucherResponseExample : IExamplesProvider<GetVoucherResponse>
{
    /// <summary>
    /// Gets the examples.
    /// </summary>
    /// <returns></returns>
    public GetVoucherResponse GetExamples()
    {
        return new GetVoucherResponse
               {
                   VoucherCode = ExampleData.VoucherCode,
                   Value = ExampleData.VoucherValue,
                   ExpiryDate = ExampleData.ExpiryDate,
                   Balance = ExampleData.RemainingBalance,
                   GeneratedDateTime = ExampleData.GeneratedDateTime.Value,
                   IsGenerated = ExampleData.IsGenerated,
                   IsIssued = ExampleData.IsIssued,
                   IsRedeemed = ExampleData.IsRedeemed,
                   IssuedDateTime = ExampleData.IssuedDateTime.Value,
                   RedeemedDateTime = ExampleData.RedeemedDateTime.Value,
                   TransactionId = ExampleData.TransactionId,
                   VoucherId = ExampleData.VoucherId
               };
    }
}