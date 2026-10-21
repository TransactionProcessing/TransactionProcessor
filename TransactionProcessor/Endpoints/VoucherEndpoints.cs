using Microsoft.AspNetCore.Builder;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.Handlers;
using TransactionProcessor.DataTransferObjects;

namespace TransactionProcessor.Endpoints
{
    [ExcludeFromCodeCoverage]
    public static class VoucherEndpoints
    {
        private const string BaseRoute = "/api/vouchers";

        public static IEndpointRouteBuilder MapVoucherEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup(BaseRoute)
                                 .WithTags("Vouchers")
                                 .RequireAuthorization()
                                 .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy);

            // Redeem and Get both reject password tokens in controller -> require client credentials
            group.MapPut("/", VoucherHandlers.RedeemVoucher)
                 .WithName("RedeemVoucher").WithSummary("Redeem a voucher")
                 .WithDescription("Redeems the voucher identified by its code. If RedeemedDateTime is omitted, the server uses the current local time.")
                 .Produces<RedeemVoucherResponse>();

            // GET with query parameters estateId, voucherCode, transactionId
            group.MapGet("/", VoucherHandlers.GetVoucher)
                 .WithName("GetVoucher").WithSummary("Get a voucher")
                 .WithDescription("Looks up a voucher by voucherCode when provided; otherwise it uses transactionId. estateId is required. At least one lookup key must be provided.")

                 .Produces<GetVoucherResponse>();

            return endpoints;
        }
    }
}
