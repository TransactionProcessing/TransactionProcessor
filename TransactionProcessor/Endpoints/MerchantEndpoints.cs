using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using TransactionProcessor.Handlers;

namespace TransactionProcessor.Endpoints;

public static class MerchantEndpoints
{
    private const string BaseRoute = "/api/estates/{estateId:guid}/merchants";

    public static IEndpointRouteBuilder MapMerchantEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Apply client-credentials-only policy to the entire merchant group so all endpoints require client auth.
        RouteGroupBuilder merchantGroup = endpoints
            .MapGroup(BaseRoute)
            .WithTags("Merchants")
            .RequireAuthorization()
            .RequireAuthorization(AuthorizationExtensions.PolicyNames.ClientCredentialsOnlyPolicy);

        // Read endpoints (now also protected by client-credentials-only policy via the group)
        merchantGroup.MapGet("/{merchantId:guid}/balance", MerchantHandlers.GetMerchantBalance).WithName("GetMerchantBalance");
        merchantGroup.MapGet("/{merchantId:guid}/livebalance", MerchantHandlers.GetMerchantBalanceLive).WithName("GetMerchantBalanceLive");
        merchantGroup.MapGet("/{merchantId:guid}/balancehistory", MerchantHandlers.GetMerchantBalanceHistory).WithName("GetMerchantBalanceHistory");
        merchantGroup.MapGet("/", MerchantHandlers.GetMerchants).WithName("GetMerchants");
        merchantGroup.MapGet("/{merchantId:guid}", MerchantHandlers.GetMerchant).WithName("GetMerchant");
        merchantGroup.MapGet("/{merchantId:guid}/contracts", MerchantHandlers.GetMerchantContracts).WithName("GetMerchantContracts");
        merchantGroup.MapGet("/{merchantId:guid}/contracts/{contractId:guid}/products/{productId:guid}/transactionFees",
            MerchantHandlers.GetTransactionFeesForProduct).WithName("GetTransactionFeesForProduct");

        // Write/modify endpoints
        merchantGroup.MapPost("/", MerchantHandlers.CreateMerchant).WithName("CreateMerchant");

        merchantGroup.MapPatch("/{merchantId:guid}/operators", MerchantHandlers.AssignOperator).WithName("AssignMerchantOperator");
        merchantGroup.MapDelete("/{merchantId:guid}/operators/{operatorId:guid}", MerchantHandlers.RemoveOperator).WithName("RemoveMerchantOperator");

        merchantGroup.MapPatch("/{merchantId:guid}/devices",MerchantHandlers.AddDevice).WithName("AddDevice");
        merchantGroup.MapPatch("/{merchantId:guid}/devices/{deviceIdentifier}", MerchantHandlers.SwapMerchantDevice).WithName("SwapMerchantDevice");

        merchantGroup.MapPatch("/{merchantId:guid}/contracts", MerchantHandlers.AddContract).WithName("AddContract");
        merchantGroup.MapDelete("/{merchantId:guid}/contracts/{contractId:guid}", MerchantHandlers.RemoveContract).WithName("RemoveContract");

        merchantGroup.MapPatch("/{merchantId:guid}/users", MerchantHandlers.CreateMerchantUser).WithName("CreateMerchantUser");

        merchantGroup.MapPost("/{merchantId:guid}/deposits", MerchantHandlers.MakeDeposit).WithName("MakeDeposit");
        merchantGroup.MapPost("/{merchantId:guid}/withdrawals", MerchantHandlers.MakeWithdrawal).WithName("MakeWithdrawal");

        merchantGroup.MapPatch("/{merchantId:guid}/addresses", MerchantHandlers.AddMerchantAddress).WithName("AddMerchantAddress");
        merchantGroup.MapPatch("/{merchantId:guid}/addresses/{addressId:guid}", MerchantHandlers.UpdateMerchantAddress).WithName("UpdateMerchantAddress");

        merchantGroup.MapPatch("/{merchantId:guid}/contacts", MerchantHandlers.AddMerchantContact).WithName("AddMerchantContact");
        merchantGroup.MapPatch("/{merchantId:guid}/contacts/{contactId:guid}", MerchantHandlers.UpdateMerchantContact).WithName("UpdateMerchantContact");

        merchantGroup.MapPost("/{merchantId:guid}/statements", MerchantHandlers.GenerateMerchantStatement).WithName("GenerateMerchantStatement");

        merchantGroup.MapPatch("/{merchantId:guid}", MerchantHandlers.UpdateMerchant).WithName("UpdateMerchant");

        return endpoints;
    }
}