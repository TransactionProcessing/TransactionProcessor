using System;
using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using TransactionProcessor.DataTransferObjects;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;
using TransactionProcessor.IntegrationTesting.Helpers;

namespace TransactionProcessor.IntegrationTests.Common;

public class SpecflowExtensionsTests
{
    [Test]
    public void ToSettlementFeeDetails_ReturnsEveryFeeRow()
    {
        EstateDetails estate = EstateDetails.Create(Guid.NewGuid(), "Test Estate", "estate-reference");
        estate.AddMerchant(new MerchantResponse
        {
            MerchantId = Guid.NewGuid(),
            MerchantName = "Test Merchant"
        });

        Reqnroll.Table table = new(new[] { "FeeDescription", "IsSettled", "Operator", "CalculatedValue" });
        table.AddRow(new[] { "Commission", "True", "Operator 1", "0.50" });
        table.AddRow(new[] { "Commission", "False", "Operator 2", "0.25" });

        List<ReqnrollExtensions.SettlementFeeDetails> result = table.Rows.ToSettlementFeeDetails(
            "Test Estate",
            "Test Merchant",
            "2026-01-13",
            new List<EstateDetails> { estate });

        result.Count.ShouldBe(2);
        result[0].FeeDescription.ShouldBe("Commission");
        result[0].IsSettled.ShouldBeTrue();
        result[0].CalculatedValue.ShouldBe(0.50m);
        result[1].IsSettled.ShouldBeFalse();
        result[1].CalculatedValue.ShouldBe(0.25m);
    }

    [Test]
    public void ToSettlementDetails_RejectsMultipleRows()
    {
        EstateDetails estate = EstateDetails.Create(Guid.NewGuid(), "Test Estate", "estate-reference");
        estate.AddMerchant(new MerchantResponse
        {
            MerchantId = Guid.NewGuid(),
            MerchantName = "Test Merchant"
        });

        Reqnroll.Table table = new(new[] { "SettlementDate", "NumberOfFeesSettled", "ValueOfFeesSettled", "IsCompleted" });
        table.AddRow(new[] { "2026-01-13", "1", "1.00", "False" });
        table.AddRow(new[] { "2026-01-14", "2", "2.00", "True" });

        Should.Throw<ShouldAssertException>(() => table.Rows.ToSettlementDetails("Test Estate", new List<EstateDetails> { estate }));
        Should.Throw<ShouldAssertException>(() => table.Rows.ToSettlementDetails("Test Estate", "Test Merchant", new List<EstateDetails> { estate }));
    }

    [Test]
    public void TestingContext_AddVoucher_DoesNotThrow()
    {
        TestingContext context = new();

        Should.NotThrow(() => context.AddVoucher((new IssueVoucherRequest(), new IssueVoucherResponse())));
    }

    [Test]
    public void EstateDetails_AddMerchant_RejectsDuplicateMerchantId()
    {
        EstateDetails estate = EstateDetails.Create(Guid.NewGuid(), "Test Estate", "estate-reference");
        Guid merchantId = Guid.NewGuid();

        estate.AddMerchant(new MerchantResponse { MerchantId = merchantId, MerchantName = "Merchant One" });

        Should.Throw<InvalidOperationException>(() => estate.AddMerchant(
            new MerchantResponse { MerchantId = merchantId, MerchantName = "Merchant Two" }));
    }

    [Test]
    public void ToTransactionRequests_UnknownContractName_FailsFast()
    {
        EstateDetails estate = CreateTransactionEstate();
        Reqnroll.Table table = CreateSaleTransactionTable("Unknown Contract", "Known Product");

        Should.Throw<InvalidOperationException>(() => table.Rows.ToTransactionRequests(new List<EstateDetails> { estate }))
            .Message.ShouldContain("Unknown Contract");
    }

    [Test]
    public void ToTransactionRequests_UnknownProductName_FailsFast()
    {
        EstateDetails estate = CreateTransactionEstate();
        Reqnroll.Table table = CreateSaleTransactionTable("Known Contract", "Unknown Product");

        Should.Throw<InvalidOperationException>(() => table.Rows.ToTransactionRequests(new List<EstateDetails> { estate }))
            .Message.ShouldContain("Unknown Product");
    }

    [Test]
    public void ToAddressUpdates_ProcessesEachTableRowOnce()
    {
        EstateDetails estate = EstateDetails.Create(Guid.NewGuid(), "Test Estate", "estate-reference");
        MerchantResponse merchantOne = CreateMerchant("Merchant One");
        MerchantResponse merchantTwo = CreateMerchant("Merchant Two");
        estate.AddMerchant(merchantOne);
        estate.AddMerchant(merchantTwo);

        Reqnroll.Table table = new(new[] { "EstateName", "MerchantName", "AddressLine1" });
        table.AddRow(new[] { "Test Estate", "Merchant One", "First address" });
        table.AddRow(new[] { "Test Estate", "Merchant Two", "Second address" });

        List<(EstateDetails, MerchantResponse, Guid, TransactionProcessor.DataTransferObjects.Requests.Merchant.Address)> result =
            table.Rows.ToAddressUpdates(new List<EstateDetails> { estate });

        result.Count.ShouldBe(2);
        result[0].Item2.MerchantName.ShouldBe("Merchant One");
        result[0].Item4.AddressLine1.ShouldBe("First address");
        result[1].Item2.MerchantName.ShouldBe("Merchant Two");
        result[1].Item4.AddressLine1.ShouldBe("Second address");
    }

    [Test]
    public void ToContactUpdates_ProcessesEachTableRowOnce()
    {
        EstateDetails estate = EstateDetails.Create(Guid.NewGuid(), "Test Estate", "estate-reference");
        MerchantResponse merchantOne = CreateMerchant("Merchant One");
        MerchantResponse merchantTwo = CreateMerchant("Merchant Two");
        estate.AddMerchant(merchantOne);
        estate.AddMerchant(merchantTwo);

        Reqnroll.Table table = new(new[] { "EstateName", "MerchantName", "ContactName", "EmailAddress", "PhoneNumber" });
        table.AddRow(new[] { "Test Estate", "Merchant One", "First contact", "first@example.com", "111" });
        table.AddRow(new[] { "Test Estate", "Merchant Two", "Second contact", "second@example.com", "222" });

        List<(EstateDetails, MerchantResponse, Guid, TransactionProcessor.DataTransferObjects.Requests.Merchant.Contact)> result =
            table.Rows.ToContactUpdates(new List<EstateDetails> { estate });

        result.Count.ShouldBe(2);
        result[0].Item2.MerchantName.ShouldBe("Merchant One");
        result[0].Item4.ContactName.ShouldBe("First contact");
        result[1].Item2.MerchantName.ShouldBe("Merchant Two");
        result[1].Item4.ContactName.ShouldBe("Second contact");
    }

    [Test]
    public void ToAddressUpdates_RejectsAmbiguousExistingAddresses()
    {
        EstateDetails estate = EstateDetails.Create(Guid.NewGuid(), "Test Estate", "estate-reference");
        MerchantResponse merchant = CreateMerchant("Test Merchant");
        merchant.Addresses.Add(new AddressResponse { AddressId = Guid.NewGuid() });
        estate.AddMerchant(merchant);

        Reqnroll.Table table = new(new[] { "EstateName", "MerchantName", "AddressLine1" });
        table.AddRow(new[] { "Test Estate", "Test Merchant", "Updated address" });

        Should.Throw<ShouldAssertException>(() => table.Rows.ToAddressUpdates(new List<EstateDetails> { estate }));
    }

    [Test]
    public void ToContactUpdates_RejectsAmbiguousExistingContacts()
    {
        EstateDetails estate = EstateDetails.Create(Guid.NewGuid(), "Test Estate", "estate-reference");
        MerchantResponse merchant = CreateMerchant("Test Merchant");
        merchant.Contacts.Add(new ContactResponse { ContactId = Guid.NewGuid() });
        estate.AddMerchant(merchant);

        Reqnroll.Table table = new(new[] { "EstateName", "MerchantName", "ContactName" });
        table.AddRow(new[] { "Test Estate", "Test Merchant", "Updated contact" });

        Should.Throw<ShouldAssertException>(() => table.Rows.ToContactUpdates(new List<EstateDetails> { estate }));
    }

    private static MerchantResponse CreateMerchant(String merchantName)
    {
        return new MerchantResponse
        {
            MerchantId = Guid.NewGuid(),
            MerchantName = merchantName,
            Addresses = new List<AddressResponse> { new() { AddressId = Guid.NewGuid() } },
            Contacts = new List<ContactResponse> { new() { ContactId = Guid.NewGuid() } }
        };
    }

    private static EstateDetails CreateTransactionEstate()
    {
        EstateDetails estate = EstateDetails.Create(Guid.NewGuid(), "Test Estate", "estate-reference");
        estate.AddMerchant(new MerchantResponse
        {
            MerchantId = Guid.NewGuid(),
            MerchantName = "Test Merchant"
        });
        estate.AddOperator(Guid.NewGuid(), "Test Operator");
        estate.AddContract(Guid.NewGuid(), "Known Contract", Guid.NewGuid());

        TransactionProcessor.IntegrationTesting.Helpers.Contract contract = estate.GetContract("Known Contract");
        contract.AddProduct(Guid.NewGuid(), "Known Product", "Known Product");

        return estate;
    }

    private static Reqnroll.Table CreateSaleTransactionTable(String contractName, String productName)
    {
        Reqnroll.Table table = new(new[]
        {
            "TransactionType", "DateTime", "TransactionNumber", "DeviceIdentifier", "EstateName", "MerchantName",
            "OperatorName", "TransactionAmount", "ContractDescription", "ProductName", "TransactionSource"
        });
        table.AddRow(new[]
        {
            "Sale", "2026-01-13", "transaction-1", "device-1", "Test Estate", "Test Merchant", "Test Operator",
            "10.00", contractName, productName, "1"
        });

        return table;
    }
}
