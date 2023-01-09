namespace TransactionProcessor.Common.Examples;

using DataTransferObjects;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

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