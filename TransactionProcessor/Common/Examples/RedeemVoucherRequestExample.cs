namespace TransactionProcessor.Common.Examples;

using DataTransferObjects;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class RedeemVoucherRequestExample : IExamplesProvider<RedeemVoucherRequest>
{
    /// <summary>
    /// Gets the examples.
    /// </summary>
    /// <returns></returns>
    public RedeemVoucherRequest GetExamples()
    {
        return new RedeemVoucherRequest
               {
                   EstateId = ExampleData.EstateId,
                   RedeemedDateTime = ExampleData.RedeemedDateTime,
                   VoucherCode = ExampleData.VoucherCode
               };
    }
}