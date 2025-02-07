﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by Reqnroll (https://www.reqnroll.net/).
//      Reqnroll Version:1.0.0.0
//      Reqnroll Generator Version:1.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace TransactionProcessor.IntegrationTests.Features
{
    using Reqnroll;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "1.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("RedeemVoucher")]
    [NUnit.Framework.CategoryAttribute("base")]
    [NUnit.Framework.CategoryAttribute("shared")]
    public partial class RedeemVoucherFeature
    {
        
        private Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "shared"};
        
#line 1 "RedeemVoucher.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual async System.Threading.Tasks.Task FeatureSetupAsync()
        {
            testRunner = Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, NUnit.Framework.TestContext.CurrentContext.WorkerId);
            Reqnroll.FeatureInfo featureInfo = new Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "RedeemVoucher", "\tSimple calculator for adding two numbers", ProgrammingLanguage.CSharp, featureTags);
            await testRunner.OnFeatureStartAsync(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual async System.Threading.Tasks.Task FeatureTearDownAsync()
        {
            await testRunner.OnFeatureEndAsync();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public async System.Threading.Tasks.Task TestInitializeAsync()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public async System.Threading.Tasks.Task TestTearDownAsync()
        {
            await testRunner.OnScenarioEndAsync();
        }
        
        public void ScenarioInitialize(Reqnroll.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public async System.Threading.Tasks.Task ScenarioStartAsync()
        {
            await testRunner.OnScenarioStartAsync();
        }
        
        public async System.Threading.Tasks.Task ScenarioCleanupAsync()
        {
            await testRunner.CollectScenarioErrorsAsync();
        }
        
        public virtual async System.Threading.Tasks.Task FeatureBackgroundAsync()
        {
#line 5
#line hidden
            Reqnroll.Table table131 = new Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Description"});
            table131.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST Scope",
                        "A scope for Estate Managememt REST"});
            table131.AddRow(new string[] {
                        "voucherManagement",
                        "Voucher Management REST  Scope",
                        "A scope for Voucher Management REST"});
#line 7
 await testRunner.GivenAsync("I create the following api scopes", ((string)(null)), table131, "Given ");
#line hidden
            Reqnroll.Table table132 = new Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Secret",
                        "Scopes",
                        "UserClaims"});
            table132.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST",
                        "Secret1",
                        "estateManagement",
                        "MerchantId, EstateId, role"});
            table132.AddRow(new string[] {
                        "voucherManagement",
                        "Voucher Management REST",
                        "Secret1",
                        "voucherManagement",
                        ""});
#line 12
 await testRunner.GivenAsync("the following api resources exist", ((string)(null)), table132, "Given ");
#line hidden
            Reqnroll.Table table133 = new Reqnroll.Table(new string[] {
                        "ClientId",
                        "ClientName",
                        "Secret",
                        "Scopes",
                        "GrantTypes"});
            table133.AddRow(new string[] {
                        "serviceClient",
                        "Service Client",
                        "Secret1",
                        "estateManagement,voucherManagement",
                        "client_credentials"});
#line 17
 await testRunner.GivenAsync("the following clients exist", ((string)(null)), table133, "Given ");
#line hidden
            Reqnroll.Table table134 = new Reqnroll.Table(new string[] {
                        "ClientId"});
            table134.AddRow(new string[] {
                        "serviceClient"});
#line 21
 await testRunner.GivenAsync("I have a token to access the estate management and transaction processor resource" +
                    "s", ((string)(null)), table134, "Given ");
#line hidden
            Reqnroll.Table table135 = new Reqnroll.Table(new string[] {
                        "EstateName"});
            table135.AddRow(new string[] {
                        "Test Estate 1"});
            table135.AddRow(new string[] {
                        "Test Estate 2"});
#line 25
 await testRunner.GivenAsync("I have created the following estates", ((string)(null)), table135, "Given ");
#line hidden
            Reqnroll.Table table136 = new Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "RequireCustomMerchantNumber",
                        "RequireCustomTerminalNumber"});
            table136.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "True",
                        "True"});
#line 30
 await testRunner.GivenAsync("I have created the following operators", ((string)(null)), table136, "Given ");
#line hidden
            Reqnroll.Table table137 = new Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName"});
            table137.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher"});
#line 34
 await testRunner.AndAsync("I have assigned the following operators to the estates", ((string)(null)), table137, "And ");
#line hidden
            Reqnroll.Table table138 = new Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription"});
            table138.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "Hospital 1 Contract"});
#line 38
 await testRunner.GivenAsync("I create a contract with the following values", ((string)(null)), table138, "Given ");
#line hidden
            Reqnroll.Table table139 = new Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription",
                        "ProductName",
                        "DisplayText",
                        "Value",
                        "ProductType"});
            table139.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "Hospital 1 Contract",
                        "10 KES",
                        "10 KES",
                        "",
                        "Voucher"});
#line 42
 await testRunner.WhenAsync("I create the following Products", ((string)(null)), table139, "When ");
#line hidden
            Reqnroll.Table table140 = new Reqnroll.Table(new string[] {
                        "MerchantName",
                        "AddressLine1",
                        "Town",
                        "Region",
                        "Country",
                        "ContactName",
                        "EmailAddress",
                        "EstateName"});
            table140.AddRow(new string[] {
                        "Test Merchant 1",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 1",
                        "testcontact1@merchant1.co.uk",
                        "Test Estate 1"});
#line 46
 await testRunner.GivenAsync("I create the following merchants", ((string)(null)), table140, "Given ");
#line hidden
            Reqnroll.Table table141 = new Reqnroll.Table(new string[] {
                        "OperatorName",
                        "MerchantName",
                        "MerchantNumber",
                        "TerminalNumber",
                        "EstateName"});
            table141.AddRow(new string[] {
                        "Voucher",
                        "Test Merchant 1",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
#line 50
 await testRunner.GivenAsync("I have assigned the following operator to the merchants", ((string)(null)), table141, "Given ");
#line hidden
            Reqnroll.Table table142 = new Reqnroll.Table(new string[] {
                        "DeviceIdentifier",
                        "MerchantName",
                        "EstateName"});
            table142.AddRow(new string[] {
                        "123456780",
                        "Test Merchant 1",
                        "Test Estate 1"});
#line 54
 await testRunner.GivenAsync("I have assigned the following devices to the merchants", ((string)(null)), table142, "Given ");
#line hidden
            Reqnroll.Table table143 = new Reqnroll.Table(new string[] {
                        "Reference",
                        "Amount",
                        "DateTime",
                        "MerchantName",
                        "EstateName"});
            table143.AddRow(new string[] {
                        "Deposit1",
                        "20.00",
                        "Today",
                        "Test Merchant 1",
                        "Test Estate 1"});
#line 58
 await testRunner.GivenAsync("I make the following manual merchant deposits", ((string)(null)), table143, "Given ");
#line hidden
            Reqnroll.Table table144 = new Reqnroll.Table(new string[] {
                        "EstateName",
                        "MerchantName",
                        "ContractDescription"});
            table144.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 1",
                        "Hospital 1 Contract"});
#line 62
 await testRunner.WhenAsync("I add the following contracts to the following merchants", ((string)(null)), table144, "When ");
#line hidden
            Reqnroll.Table table145 = new Reqnroll.Table(new string[] {
                        "DateTime",
                        "TransactionNumber",
                        "TransactionType",
                        "TransactionSource",
                        "MerchantName",
                        "DeviceIdentifier",
                        "EstateName",
                        "OperatorName",
                        "TransactionAmount",
                        "CustomerAccountNumber",
                        "CustomerEmailAddress",
                        "ContractDescription",
                        "ProductName",
                        "RecipientEmail",
                        "RecipientMobile",
                        "MessageType",
                        "AccountNumber",
                        "CustomerName"});
            table145.AddRow(new string[] {
                        "Today",
                        "1",
                        "Sale",
                        "1",
                        "Test Merchant 1",
                        "123456780",
                        "Test Estate 1",
                        "Voucher",
                        "10.00",
                        "",
                        "",
                        "Hospital 1 Contract",
                        "10 KES",
                        "test@recipient.co.uk",
                        "",
                        "",
                        "",
                        ""});
#line 66
 await testRunner.WhenAsync("I perform the following transactions", ((string)(null)), table145, "When ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Redeem Vouchers")]
        [NUnit.Framework.CategoryAttribute("PRTest")]
        public async System.Threading.Tasks.Task RedeemVouchers()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Redeem Vouchers", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 72
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 5
await this.FeatureBackgroundAsync();
#line hidden
#line 73
 await testRunner.WhenAsync("I redeem the voucher for Estate \'Test Estate 1\' and Merchant \'Test Merchant 1\' tr" +
                        "ansaction number 1 the voucher balance will be 0", ((string)(null)), ((Reqnroll.Table)(null)), "When ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
    }
}
#pragma warning restore
#endregion
