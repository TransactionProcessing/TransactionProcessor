﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace TransactionProcessor.IntegrationTests.Settlement
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "base")]
    [Xunit.TraitAttribute("Category", "shared")]
    public partial class SettlementFeature : object, Xunit.IClassFixture<SettlementFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "shared"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "Settlement.feature"
#line hidden
        
        public SettlementFeature(SettlementFeature.FixtureData fixtureData, TransactionProcessor_IntegrationTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Settlement", "Settlement", null, ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public void TestInitialize()
        {
        }
        
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 4
#line hidden
            TechTalk.SpecFlow.Table table66 = new TechTalk.SpecFlow.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Description"});
            table66.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST Scope",
                        "A scope for Estate Managememt REST"});
            table66.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST  Scope",
                        "A scope for Transaction Processor REST"});
            table66.AddRow(new string[] {
                        "voucherManagement",
                        "Voucher Management REST  Scope",
                        "A scope for Voucher Management REST"});
#line 6
 testRunner.Given("I create the following api scopes", ((string)(null)), table66, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table67 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceName",
                        "DisplayName",
                        "Secret",
                        "Scopes",
                        "UserClaims"});
            table67.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST",
                        "Secret1",
                        "estateManagement",
                        "MerchantId, EstateId, role"});
            table67.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST",
                        "Secret1",
                        "transactionProcessor",
                        ""});
            table67.AddRow(new string[] {
                        "voucherManagement",
                        "Voucher Management REST",
                        "Secret1",
                        "voucherManagement",
                        ""});
#line 12
 testRunner.Given("the following api resources exist", ((string)(null)), table67, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table68 = new TechTalk.SpecFlow.Table(new string[] {
                        "ClientId",
                        "ClientName",
                        "Secret",
                        "AllowedScopes",
                        "AllowedGrantTypes"});
            table68.AddRow(new string[] {
                        "serviceClient",
                        "Service Client",
                        "Secret1",
                        "estateManagement,transactionProcessor,voucherManagement",
                        "client_credentials"});
#line 18
 testRunner.Given("the following clients exist", ((string)(null)), table68, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table69 = new TechTalk.SpecFlow.Table(new string[] {
                        "ClientId"});
            table69.AddRow(new string[] {
                        "serviceClient"});
#line 22
 testRunner.Given("I have a token to access the estate management and transaction processor resource" +
                    "s", ((string)(null)), table69, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table70 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName"});
            table70.AddRow(new string[] {
                        "Test Estate 1"});
#line 26
 testRunner.Given("I have created the following estates", ((string)(null)), table70, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table71 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "RequireCustomMerchantNumber",
                        "RequireCustomTerminalNumber"});
            table71.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "True",
                        "True"});
            table71.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "True",
                        "True"});
#line 30
 testRunner.Given("I have created the following operators", ((string)(null)), table71, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table72 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription"});
            table72.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract"});
            table72.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "Hospital 1 Contract"});
#line 35
 testRunner.Given("I create a contract with the following values", ((string)(null)), table72, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table73 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription",
                        "ProductName",
                        "DisplayText",
                        "Value"});
            table73.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Custom",
                        ""});
            table73.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "Hospital 1 Contract",
                        "10 KES",
                        "10 KES",
                        ""});
#line 40
 testRunner.When("I create the following Products", ((string)(null)), table73, "When ");
#line hidden
            TechTalk.SpecFlow.Table table74 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription",
                        "ProductName",
                        "CalculationType",
                        "FeeDescription",
                        "Value"});
            table74.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Fixed",
                        "Merchant Commission",
                        "2.50"});
#line 45
 testRunner.When("I add the following Transaction Fees", ((string)(null)), table74, "When ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get Pending Settlement")]
        [Xunit.TraitAttribute("FeatureTitle", "Settlement")]
        [Xunit.TraitAttribute("Description", "Get Pending Settlement")]
        public void GetPendingSettlement()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get Pending Settlement", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 49
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table75 = new TechTalk.SpecFlow.Table(new string[] {
                            "MerchantName",
                            "AddressLine1",
                            "Town",
                            "Region",
                            "Country",
                            "ContactName",
                            "EmailAddress",
                            "EstateName",
                            "SettlementSchedule"});
                table75.AddRow(new string[] {
                            "Test Merchant 1",
                            "Address Line 1",
                            "TestTown",
                            "Test Region",
                            "United Kingdom",
                            "Test Contact 1",
                            "testcontact1@merchant1.co.uk",
                            "Test Estate 1",
                            "Immediate"});
                table75.AddRow(new string[] {
                            "Test Merchant 2",
                            "Address Line 1",
                            "TestTown",
                            "Test Region",
                            "United Kingdom",
                            "Test Contact 2",
                            "testcontact2@merchant2.co.uk",
                            "Test Estate 1",
                            "Weekly"});
                table75.AddRow(new string[] {
                            "Test Merchant 3",
                            "Address Line 1",
                            "TestTown",
                            "Test Region",
                            "United Kingdom",
                            "Test Contact 3",
                            "testcontact3@merchant2.co.uk",
                            "Test Estate 1",
                            "Monthly"});
#line 50
 testRunner.Given("I create the following merchants", ((string)(null)), table75, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table76 = new TechTalk.SpecFlow.Table(new string[] {
                            "OperatorName",
                            "MerchantName",
                            "MerchantNumber",
                            "TerminalNumber",
                            "EstateName"});
                table76.AddRow(new string[] {
                            "Safaricom",
                            "Test Merchant 1",
                            "00000001",
                            "10000001",
                            "Test Estate 1"});
                table76.AddRow(new string[] {
                            "Voucher",
                            "Test Merchant 1",
                            "00000001",
                            "10000001",
                            "Test Estate 1"});
                table76.AddRow(new string[] {
                            "Safaricom",
                            "Test Merchant 2",
                            "00000002",
                            "10000002",
                            "Test Estate 1"});
                table76.AddRow(new string[] {
                            "Voucher",
                            "Test Merchant 2",
                            "00000002",
                            "10000002",
                            "Test Estate 1"});
                table76.AddRow(new string[] {
                            "Safaricom",
                            "Test Merchant 3",
                            "00000003",
                            "10000003",
                            "Test Estate 1"});
                table76.AddRow(new string[] {
                            "Voucher",
                            "Test Merchant 3",
                            "00000003",
                            "10000003",
                            "Test Estate 1"});
#line 56
 testRunner.Given("I have assigned the following  operator to the merchants", ((string)(null)), table76, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table77 = new TechTalk.SpecFlow.Table(new string[] {
                            "DeviceIdentifier",
                            "MerchantName",
                            "EstateName"});
                table77.AddRow(new string[] {
                            "123456780",
                            "Test Merchant 1",
                            "Test Estate 1"});
                table77.AddRow(new string[] {
                            "123456781",
                            "Test Merchant 2",
                            "Test Estate 1"});
                table77.AddRow(new string[] {
                            "123456782",
                            "Test Merchant 3",
                            "Test Estate 1"});
#line 65
 testRunner.Given("I have assigned the following devices to the merchants", ((string)(null)), table77, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table78 = new TechTalk.SpecFlow.Table(new string[] {
                            "Reference",
                            "Amount",
                            "DateTime",
                            "MerchantName",
                            "EstateName"});
                table78.AddRow(new string[] {
                            "Deposit1",
                            "210.00",
                            "Today",
                            "Test Merchant 1",
                            "Test Estate 1"});
                table78.AddRow(new string[] {
                            "Deposit1",
                            "110.00",
                            "Today",
                            "Test Merchant 2",
                            "Test Estate 1"});
                table78.AddRow(new string[] {
                            "Deposit1",
                            "120.00",
                            "Today",
                            "Test Merchant 3",
                            "Test Estate 1"});
#line 71
 testRunner.Given("I make the following manual merchant deposits", ((string)(null)), table78, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table79 = new TechTalk.SpecFlow.Table(new string[] {
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
                            "RecipientMobile"});
                table79.AddRow(new string[] {
                            "2022-01-06",
                            "1",
                            "Sale",
                            "1",
                            "Test Merchant 1",
                            "123456780",
                            "Test Estate 1",
                            "Safaricom",
                            "100.00",
                            "123456789",
                            "",
                            "Safaricom Contract",
                            "Variable Topup",
                            "",
                            ""});
                table79.AddRow(new string[] {
                            "2022-01-06",
                            "2",
                            "Sale",
                            "1",
                            "Test Merchant 2",
                            "123456781",
                            "Test Estate 1",
                            "Safaricom",
                            "100.00",
                            "123456789",
                            "",
                            "Safaricom Contract",
                            "Variable Topup",
                            "",
                            ""});
                table79.AddRow(new string[] {
                            "2022-01-06",
                            "3",
                            "Sale",
                            "2",
                            "Test Merchant 3",
                            "123456782",
                            "Test Estate 1",
                            "Safaricom",
                            "100.00",
                            "123456789",
                            "",
                            "Safaricom Contract",
                            "Variable Topup",
                            "",
                            ""});
                table79.AddRow(new string[] {
                            "2022-01-06",
                            "4",
                            "Sale",
                            "1",
                            "Test Merchant 1",
                            "123456780",
                            "Test Estate 1",
                            "Safaricom",
                            "100.00",
                            "123456789",
                            "testcustomer@customer.co.uk",
                            "Safaricom Contract",
                            "Variable Topup",
                            "",
                            ""});
                table79.AddRow(new string[] {
                            "2022-01-06",
                            "5",
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
                            ""});
                table79.AddRow(new string[] {
                            "2022-01-06",
                            "6",
                            "Sale",
                            "1",
                            "Test Merchant 2",
                            "123456781",
                            "Test Estate 1",
                            "Voucher",
                            "10.00",
                            "",
                            "",
                            "Hospital 1 Contract",
                            "10 KES",
                            "",
                            "123456789"});
                table79.AddRow(new string[] {
                            "2022-01-06",
                            "7",
                            "Sale",
                            "2",
                            "Test Merchant 3",
                            "123456782",
                            "Test Estate 1",
                            "Voucher",
                            "10.00",
                            "",
                            "",
                            "Hospital 1 Contract",
                            "10 KES",
                            "test@recipient.co.uk",
                            ""});
                table79.AddRow(new string[] {
                            "2022-01-06",
                            "8",
                            "Sale",
                            "1",
                            "Test Merchant 3",
                            "123456782",
                            "Test Estate 1",
                            "Voucher",
                            "10.00",
                            "",
                            "",
                            "Hospital 1 Contract",
                            "10 KES",
                            "test@recipient.co.uk",
                            ""});
#line 77
 testRunner.When("I perform the following transactions", ((string)(null)), table79, "When ");
#line hidden
                TechTalk.SpecFlow.Table table80 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table80.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "1",
                            "0000",
                            "SUCCESS"});
                table80.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 2",
                            "2",
                            "0000",
                            "SUCCESS"});
                table80.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 3",
                            "3",
                            "0000",
                            "SUCCESS"});
                table80.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "4",
                            "0000",
                            "SUCCESS"});
                table80.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "5",
                            "0000",
                            "SUCCESS"});
                table80.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 2",
                            "6",
                            "0000",
                            "SUCCESS"});
                table80.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 3",
                            "7",
                            "0000",
                            "SUCCESS"});
                table80.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 3",
                            "8",
                            "0000",
                            "SUCCESS"});
#line 88
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table80, "Then ");
#line hidden
                TechTalk.SpecFlow.Table table81 = new TechTalk.SpecFlow.Table(new string[] {
                            "SettlementDate",
                            "EstateName",
                            "NumberOfFees"});
                table81.AddRow(new string[] {
                            "2022-01-13",
                            "Test Estate 1",
                            "1"});
                table81.AddRow(new string[] {
                            "2022-02-06",
                            "Test Estate 1",
                            "1"});
#line 99
 testRunner.When("I get the pending settlements the following information should be returned", ((string)(null)), table81, "When ");
#line hidden
                TechTalk.SpecFlow.Table table82 = new TechTalk.SpecFlow.Table(new string[] {
                            "SettlementDate",
                            "EstateName",
                            "NumberOfFees"});
                table82.AddRow(new string[] {
                            "2022-01-06",
                            "Test Estate 1",
                            "2"});
#line 104
 testRunner.When("I get the completed settlements the following information should be returned", ((string)(null)), table82, "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Process Settlement")]
        [Xunit.TraitAttribute("FeatureTitle", "Settlement")]
        [Xunit.TraitAttribute("Description", "Process Settlement")]
        [Xunit.TraitAttribute("Category", "PRTest")]
        public void ProcessSettlement()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Process Settlement", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 109
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table83 = new TechTalk.SpecFlow.Table(new string[] {
                            "MerchantName",
                            "AddressLine1",
                            "Town",
                            "Region",
                            "Country",
                            "ContactName",
                            "EmailAddress",
                            "EstateName",
                            "SettlementSchedule"});
                table83.AddRow(new string[] {
                            "Test Merchant 1",
                            "Address Line 1",
                            "TestTown",
                            "Test Region",
                            "United Kingdom",
                            "Test Contact 1",
                            "testcontact1@merchant1.co.uk",
                            "Test Estate 1",
                            "Immediate"});
                table83.AddRow(new string[] {
                            "Test Merchant 2",
                            "Address Line 1",
                            "TestTown",
                            "Test Region",
                            "United Kingdom",
                            "Test Contact 2",
                            "testcontact2@merchant2.co.uk",
                            "Test Estate 1",
                            "Weekly"});
#line 110
 testRunner.Given("I create the following merchants", ((string)(null)), table83, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table84 = new TechTalk.SpecFlow.Table(new string[] {
                            "OperatorName",
                            "MerchantName",
                            "MerchantNumber",
                            "TerminalNumber",
                            "EstateName"});
                table84.AddRow(new string[] {
                            "Safaricom",
                            "Test Merchant 1",
                            "00000001",
                            "10000001",
                            "Test Estate 1"});
                table84.AddRow(new string[] {
                            "Voucher",
                            "Test Merchant 1",
                            "00000001",
                            "10000001",
                            "Test Estate 1"});
                table84.AddRow(new string[] {
                            "Safaricom",
                            "Test Merchant 2",
                            "00000002",
                            "10000002",
                            "Test Estate 1"});
                table84.AddRow(new string[] {
                            "Voucher",
                            "Test Merchant 2",
                            "00000002",
                            "10000002",
                            "Test Estate 1"});
#line 115
 testRunner.Given("I have assigned the following  operator to the merchants", ((string)(null)), table84, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table85 = new TechTalk.SpecFlow.Table(new string[] {
                            "DeviceIdentifier",
                            "MerchantName",
                            "EstateName"});
                table85.AddRow(new string[] {
                            "123456780",
                            "Test Merchant 1",
                            "Test Estate 1"});
                table85.AddRow(new string[] {
                            "123456781",
                            "Test Merchant 2",
                            "Test Estate 1"});
#line 122
 testRunner.Given("I have assigned the following devices to the merchants", ((string)(null)), table85, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table86 = new TechTalk.SpecFlow.Table(new string[] {
                            "Reference",
                            "Amount",
                            "DateTime",
                            "MerchantName",
                            "EstateName"});
                table86.AddRow(new string[] {
                            "Deposit1",
                            "210.00",
                            "Today",
                            "Test Merchant 1",
                            "Test Estate 1"});
                table86.AddRow(new string[] {
                            "Deposit1",
                            "110.00",
                            "Today",
                            "Test Merchant 2",
                            "Test Estate 1"});
#line 127
 testRunner.Given("I make the following manual merchant deposits", ((string)(null)), table86, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table87 = new TechTalk.SpecFlow.Table(new string[] {
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
                            "RecipientMobile"});
                table87.AddRow(new string[] {
                            "2022-01-06",
                            "1",
                            "Sale",
                            "1",
                            "Test Merchant 1",
                            "123456780",
                            "Test Estate 1",
                            "Safaricom",
                            "100.00",
                            "123456789",
                            "",
                            "Safaricom Contract",
                            "Variable Topup",
                            "",
                            ""});
                table87.AddRow(new string[] {
                            "2022-01-06",
                            "2",
                            "Sale",
                            "1",
                            "Test Merchant 2",
                            "123456781",
                            "Test Estate 1",
                            "Safaricom",
                            "100.00",
                            "123456789",
                            "",
                            "Safaricom Contract",
                            "Variable Topup",
                            "",
                            ""});
                table87.AddRow(new string[] {
                            "2022-01-06",
                            "4",
                            "Sale",
                            "1",
                            "Test Merchant 1",
                            "123456780",
                            "Test Estate 1",
                            "Safaricom",
                            "100.00",
                            "123456789",
                            "testcustomer@customer.co.uk",
                            "Safaricom Contract",
                            "Variable Topup",
                            "",
                            ""});
                table87.AddRow(new string[] {
                            "2022-01-06",
                            "5",
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
                            ""});
                table87.AddRow(new string[] {
                            "2022-01-06",
                            "6",
                            "Sale",
                            "1",
                            "Test Merchant 2",
                            "123456781",
                            "Test Estate 1",
                            "Voucher",
                            "10.00",
                            "",
                            "",
                            "Hospital 1 Contract",
                            "10 KES",
                            "",
                            "123456789"});
#line 132
 testRunner.When("I perform the following transactions", ((string)(null)), table87, "When ");
#line hidden
                TechTalk.SpecFlow.Table table88 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table88.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "1",
                            "0000",
                            "SUCCESS"});
                table88.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 2",
                            "2",
                            "0000",
                            "SUCCESS"});
                table88.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "4",
                            "0000",
                            "SUCCESS"});
                table88.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "5",
                            "0000",
                            "SUCCESS"});
                table88.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 2",
                            "6",
                            "0000",
                            "SUCCESS"});
#line 140
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table88, "Then ");
#line hidden
                TechTalk.SpecFlow.Table table89 = new TechTalk.SpecFlow.Table(new string[] {
                            "SettlementDate",
                            "EstateName",
                            "NumberOfFees"});
                table89.AddRow(new string[] {
                            "2022-01-13",
                            "Test Estate 1",
                            "1"});
#line 148
 testRunner.When("I get the pending settlements the following information should be returned", ((string)(null)), table89, "When ");
#line hidden
#line 152
 testRunner.When("I process the settlement for \'2022-01-13\' on Estate \'Test Estate 1\' then 1 fees a" +
                        "re marked as settled and the settlement is completed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                SettlementFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                SettlementFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
