using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using TransactionProcessor.Handlers;

namespace TransactionProcessor.Endpoints
{
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
                 .WithName("RedeemVoucher");

            // GET with query parameters estateId, voucherCode, transactionId
            group.MapGet("/", VoucherHandlers.GetVoucher)
                 .WithName("GetVoucher");

            return endpoints;
        }
    }
}