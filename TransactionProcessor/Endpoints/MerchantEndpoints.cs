using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorisation;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.Handlers;
using TransactionProcessor.DataTransferObjects;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;
using System.Collections.Generic;

namespace TransactionProcessor.Endpoints;

[ExcludeFromCodeCoverage]
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
        merchantGroup.MapGet("/{merchantId:guid}/balance", MerchantHandlers.GetMerchantBalance)
            .WithName("GetMerchantBalance").WithSummary("Get merchant balance")
            .WithDescription("Returns the stored balance and available balance for this estate merchant.").Produces<MerchantBalanceResponse>();
        merchantGroup.MapGet("/{merchantId:guid}/livebalance", MerchantHandlers.GetMerchantBalanceLive)
            .WithName("GetMerchantBalanceLive").WithSummary("Get live merchant balance")
            .WithDescription("Returns the current balance from the live balance projection for this merchant.").Produces<MerchantBalanceResponse>();
        merchantGroup.MapGet("/{merchantId:guid}/balancehistory", MerchantHandlers.GetMerchantBalanceHistory)
            .WithName("GetMerchantBalanceHistory").WithSummary("Get merchant balance history")
            .WithDescription("Returns balance changes for the merchant between the required startDate and endDate query values.").Produces<List<MerchantBalanceChangedEntryResponse>>();
        merchantGroup.MapGet("/", MerchantHandlers.GetMerchants)
            .WithName("GetMerchants").WithSummary("List estate merchants")
            .WithDescription("Returns all merchants belonging to the specified estate.").Produces<List<MerchantResponse>>();
        merchantGroup.MapGet("/{merchantId:guid}", MerchantHandlers.GetMerchant)
            .WithName("GetMerchant").WithSummary("Get a merchant")
            .WithDescription("Returns merchant details, including addresses, contacts, devices, operators, contracts, opening hours, and schedules.").Produces<MerchantResponse>();
        merchantGroup.MapGet("/{merchantId:guid}/contracts", MerchantHandlers.GetMerchantContracts)
            .WithName("GetMerchantContracts").WithSummary("List merchant contracts")
            .WithDescription("Returns contracts associated with the specified merchant.").Produces<List<ContractResponse>>();
        merchantGroup.MapGet("/{merchantId:guid}/schedules/{year:int}", MerchantHandlers.GetMerchantSchedule)
            .WithName("GetMerchantSchedule").WithSummary("Get merchant schedule")
            .WithDescription("Returns the merchant schedule for the requested year from the primary schedule store.").Produces<MerchantScheduleResponse>();
        merchantGroup.MapGet("/{merchantId:guid}/schedules/{year:int}/readmodel", MerchantHandlers.GetMerchantScheduleFromReadModel)
            .WithName("GetMerchantScheduleFromReadModel").WithSummary("Get merchant schedule read model")
            .WithDescription("Returns the requested year's merchant schedule from the read model.").Produces<MerchantScheduleResponse>();
        merchantGroup.MapGet("/{merchantId:guid}/contracts/{contractId:guid}/products/{productId:guid}/transactionFees",
            MerchantHandlers.GetTransactionFeesForProduct).WithName("GetTransactionFeesForProduct")
            .WithSummary("List merchant product transaction fees")
            .WithDescription("Returns transaction fee definitions for the selected product on a merchant contract.").Produces<List<ContractProductTransactionFee>>();

        // Write/modify endpoints
        merchantGroup.MapPost("/", MerchantHandlers.CreateMerchant).WithName("CreateMerchant")
            .WithSummary("Create a merchant")
            .WithDescription("Creates a merchant in the specified estate, including its address, contact, and settlement schedule.");

        merchantGroup.MapPatch("/{merchantId:guid}/operators", MerchantHandlers.AssignOperator).WithName("AssignMerchantOperator")
            .WithSummary("Assign an operator to a merchant").WithDescription("Associates the request's operator and terminal identifiers with the merchant.");
        merchantGroup.MapDelete("/{merchantId:guid}/operators/{operatorId:guid}", MerchantHandlers.RemoveOperator).WithName("RemoveMerchantOperator")
            .WithSummary("Remove an operator from a merchant").WithDescription("Removes the specified operator association from the merchant.");

        merchantGroup.MapPatch("/{merchantId:guid}/devices",MerchantHandlers.AddDevice).WithName("AddDevice")
            .WithSummary("Add a merchant device").WithDescription("Registers the device identifier supplied in the request body for this merchant.");
        merchantGroup.MapPatch("/{merchantId:guid}/devices/{deviceIdentifier}", MerchantHandlers.SwapMerchantDevice).WithName("SwapMerchantDevice")
            .WithSummary("Replace a merchant device").WithDescription("Replaces the device identified by the deviceIdentifier path value with the new identifier in the request body.");

        merchantGroup.MapPatch("/{merchantId:guid}/contracts", MerchantHandlers.AddContract).WithName("AddContract")
            .WithSummary("Associate a contract with a merchant").WithDescription("Adds the contract ID in the request body to the merchant.");
        merchantGroup.MapDelete("/{merchantId:guid}/contracts/{contractId:guid}", MerchantHandlers.RemoveContract).WithName("RemoveContract")
            .WithSummary("Remove a merchant contract").WithDescription("Removes the selected contract association from the merchant.");

        merchantGroup.MapPatch("/{merchantId:guid}/users", MerchantHandlers.CreateMerchantUser).WithName("CreateMerchantUser")
            .WithSummary("Create a merchant user").WithDescription("Creates a security user associated with this merchant.");

        merchantGroup.MapPost("/{merchantId:guid}/deposits", MerchantHandlers.MakeDeposit).WithName("MakeDeposit")
            .WithSummary("Record a merchant deposit").WithDescription("Records a manual deposit with amount, timestamp, and reference.");
        merchantGroup.MapPost("/{merchantId:guid}/withdrawals", MerchantHandlers.MakeWithdrawal).WithName("MakeWithdrawal")
            .WithSummary("Record a merchant withdrawal").WithDescription("Records a withdrawal with amount, timestamp, and reference.");

        merchantGroup.MapPatch("/{merchantId:guid}/addresses", MerchantHandlers.AddMerchantAddress).WithName("AddMerchantAddress")
            .WithSummary("Add a merchant address").WithDescription("Adds the address in the request body to the merchant.");
        merchantGroup.MapPatch("/{merchantId:guid}/addresses/{addressId:guid}", MerchantHandlers.UpdateMerchantAddress).WithName("UpdateMerchantAddress")
            .WithSummary("Update a merchant address").WithDescription("Replaces the selected merchant address with the address in the request body.");

        merchantGroup.MapPatch("/{merchantId:guid}/contacts", MerchantHandlers.AddMerchantContact).WithName("AddMerchantContact")
            .WithSummary("Add a merchant contact").WithDescription("Adds the contact in the request body to the merchant.");
        merchantGroup.MapPatch("/{merchantId:guid}/contacts/{contactId:guid}", MerchantHandlers.UpdateMerchantContact).WithName("UpdateMerchantContact")
            .WithSummary("Update a merchant contact").WithDescription("Replaces the selected merchant contact with the contact in the request body.");

        merchantGroup.MapPost("/{merchantId:guid}/statements", MerchantHandlers.GenerateMerchantStatement).WithName("GenerateMerchantStatement")
            .WithSummary("Generate a merchant statement").WithDescription("Generates a statement for the merchant and statement date in the request body.");

        merchantGroup.MapPatch("/{merchantId:guid}", MerchantHandlers.UpdateMerchant).WithName("UpdateMerchant")
            .WithSummary("Update a merchant").WithDescription("Updates the merchant's name and settlement schedule.");
        merchantGroup.MapPatch("/{merchantId:guid}/opening", MerchantHandlers.UpdateMerchantOpening).WithName("UpdateMerchantOpening")
            .WithSummary("Update merchant opening hours").WithDescription("Sets opening and closing times for each day supplied in the request body.");
        merchantGroup.MapPost("/{merchantId:guid}/schedules", MerchantHandlers.CreateMerchantSchedule).WithName("CreateMerchantSchedule")
            .WithSummary("Create a merchant schedule").WithDescription("Creates the merchant schedule for the year and month closure days in the request body.");
        merchantGroup.MapPatch("/{merchantId:guid}/schedules/{year:int}", MerchantHandlers.UpdateMerchantSchedule).WithName("UpdateMerchantSchedule")
            .WithSummary("Update a merchant schedule").WithDescription("Updates month closure days for the year in the route.");

        return endpoints;
    }
}
