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
namespace TransactionProcessor.IntegrationTests.Features
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "base")]
    [Xunit.TraitAttribute("Category", "shared")]
    public partial class LogonTransactionFeature : object, Xunit.IClassFixture<LogonTransactionFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "shared"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "LogonTransaction.feature"
#line hidden
        
        public LogonTransactionFeature(LogonTransactionFeature.FixtureData fixtureData, TransactionProcessor_IntegrationTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "LogonTransaction", null, ProgrammingLanguage.CSharp, featureTags);
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
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Description"});
            table1.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST Scope",
                        "A scope for Estate Managememt REST"});
            table1.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST  Scope",
                        "A scope for Transaction Processor REST"});
#line 6
 testRunner.Given("I create the following api scopes", ((string)(null)), table1, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceName",
                        "DisplayName",
                        "Secret",
                        "Scopes",
                        "UserClaims"});
            table2.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST",
                        "Secret1",
                        "estateManagement",
                        "MerchantId, EstateId, role"});
            table2.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST",
                        "Secret1",
                        "transactionProcessor",
                        ""});
#line 11
 testRunner.Given("the following api resources exist", ((string)(null)), table2, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "ClientId",
                        "ClientName",
                        "Secret",
                        "AllowedScopes",
                        "AllowedGrantTypes"});
            table3.AddRow(new string[] {
                        "serviceClient",
                        "Service Client",
                        "Secret1",
                        "estateManagement,transactionProcessor",
                        "client_credentials"});
#line 16
 testRunner.Given("the following clients exist", ((string)(null)), table3, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "ClientId"});
            table4.AddRow(new string[] {
                        "serviceClient"});
#line 20
 testRunner.Given("I have a token to access the estate management and transaction processor resource" +
                    "s", ((string)(null)), table4, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName"});
            table5.AddRow(new string[] {
                        "Test Estate 1"});
#line 24
 testRunner.Given("I have created the following estates", ((string)(null)), table5, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "RequireCustomMerchantNumber",
                        "RequireCustomTerminalNumber"});
            table6.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Operator 1",
                        "True",
                        "True"});
#line 28
 testRunner.Given("I have created the following operators", ((string)(null)), table6, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "MerchantName",
                        "AddressLine1",
                        "Town",
                        "Region",
                        "Country",
                        "ContactName",
                        "EmailAddress",
                        "EstateName"});
            table7.AddRow(new string[] {
                        "Test Merchant 1",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 1",
                        "testcontact1@merchant1.co.uk",
                        "Test Estate 1"});
            table7.AddRow(new string[] {
                        "Test Merchant 2",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 2",
                        "testcontact2@merchant2.co.uk",
                        "Test Estate 1"});
            table7.AddRow(new string[] {
                        "Test Merchant 3",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 3",
                        "testcontact3@merchant2.co.uk",
                        "Test Estate 1"});
            table7.AddRow(new string[] {
                        "Test Merchant 4",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 4",
                        "testcontact4@merchant2.co.uk",
                        "Test Estate 1"});
            table7.AddRow(new string[] {
                        "Test Merchant 5",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 5",
                        "testcontact5@merchant2.co.uk",
                        "Test Estate 1"});
            table7.AddRow(new string[] {
                        "Test Merchant 6",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 6",
                        "testcontact6@merchant2.co.uk",
                        "Test Estate 1"});
            table7.AddRow(new string[] {
                        "Test Merchant 7",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 7",
                        "testcontact7@merchant2.co.uk",
                        "Test Estate 1"});
#line 32
 testRunner.Given("I create the following merchants", ((string)(null)), table7, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "OperatorName",
                        "MerchantName",
                        "MerchantNumber",
                        "TerminalNumber",
                        "EstateName"});
            table8.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 1",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table8.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 2",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table8.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 3",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table8.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 4",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table8.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 5",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table8.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 6",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table8.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 7",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
#line 42
 testRunner.Given("I have assigned the following  operator to the merchants", ((string)(null)), table8, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Reference",
                        "Amount",
                        "DateTime",
                        "MerchantName",
                        "EstateName"});
            table9.AddRow(new string[] {
                        "Deposit1",
                        "2000.00",
                        "Today",
                        "Test Merchant 1",
                        "Test Estate 1"});
            table9.AddRow(new string[] {
                        "Deposit1",
                        "1000.00",
                        "Today",
                        "Test Merchant 2",
                        "Test Estate 1"});
            table9.AddRow(new string[] {
                        "Deposit1",
                        "1000.00",
                        "Today",
                        "Test Merchant 3",
                        "Test Estate 1"});
            table9.AddRow(new string[] {
                        "Deposit1",
                        "1000.00",
                        "Today",
                        "Test Merchant 4",
                        "Test Estate 1"});
#line 52
 testRunner.Given("I make the following manual merchant deposits", ((string)(null)), table9, "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Logon Transactions")]
        [Xunit.TraitAttribute("FeatureTitle", "LogonTransaction")]
        [Xunit.TraitAttribute("Description", "Logon Transactions")]
        [Xunit.TraitAttribute("Category", "PRTest")]
        public void LogonTransactions()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Logon Transactions", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 60
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
                TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table10.AddRow(new string[] {
                            "Today",
                            "1",
                            "Logon",
                            "Test Merchant 1",
                            "123456780",
                            "Test Estate 1"});
                table10.AddRow(new string[] {
                            "Today",
                            "2",
                            "Logon",
                            "Test Merchant 2",
                            "123456781",
                            "Test Estate 1"});
                table10.AddRow(new string[] {
                            "Today",
                            "3",
                            "Logon",
                            "Test Merchant 3",
                            "123456782",
                            "Test Estate 1"});
#line 62
 testRunner.When("I perform the following transactions", ((string)(null)), table10, "When ");
#line hidden
                TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table11.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "1",
                            "0001",
                            "SUCCESS"});
                table11.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 2",
                            "2",
                            "0001",
                            "SUCCESS"});
                table11.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 3",
                            "3",
                            "0001",
                            "SUCCESS"});
#line 68
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table11, "Then ");
#line hidden
                TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                            "DeviceIdentifier",
                            "MerchantName",
                            "MerchantNumber",
                            "EstateName"});
                table12.AddRow(new string[] {
                            "123456783",
                            "Test Merchant 4",
                            "00000001",
                            "Test Estate 1"});
#line 74
 testRunner.Given("I have assigned the following devices to the merchants", ((string)(null)), table12, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table13.AddRow(new string[] {
                            "Today",
                            "4",
                            "Logon",
                            "Test Merchant 4",
                            "123456783",
                            "Test Estate 1"});
#line 78
 testRunner.When("I perform the following transactions", ((string)(null)), table13, "When ");
#line hidden
                TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table14.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 4",
                            "4",
                            "0000",
                            "SUCCESS"});
#line 82
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table14, "Then ");
#line hidden
                TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table15.AddRow(new string[] {
                            "Today",
                            "5",
                            "Logon",
                            "Test Merchant 1",
                            "13579135",
                            "Test Estate 1"});
#line 86
 testRunner.When("I perform the following transactions", ((string)(null)), table15, "When ");
#line hidden
                TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table16.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "5",
                            "1000",
                            "Device Identifier 13579135 not valid for Merchant Test Merchant 1"});
#line 90
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table16, "Then ");
#line hidden
                TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table17.AddRow(new string[] {
                            "Today",
                            "6",
                            "Logon",
                            "Test Merchant 1",
                            "123456785",
                            "InvalidEstate"});
#line 94
 testRunner.When("I perform the following transactions", ((string)(null)), table17, "When ");
#line hidden
                TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table18.AddRow(new string[] {
                            "InvalidEstate",
                            "Test Merchant 1",
                            "6",
                            "1001",
                            "Estate Id [79902550-64df-4491-b0c1-4e78943928a3] is not a valid estate"});
#line 98
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table18, "Then ");
#line hidden
                TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table19.AddRow(new string[] {
                            "Today",
                            "7",
                            "Logon",
                            "InvalidMerchant",
                            "123456786",
                            "Test Estate 1"});
#line 102
 testRunner.When("I perform the following transactions", ((string)(null)), table19, "When ");
#line hidden
                TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table20.AddRow(new string[] {
                            "Test Estate 1",
                            "InvalidMerchant",
                            "7",
                            "1002",
                            "Merchant Id [d59320fa-4c3e-4900-a999-483f6a10c69a] is not a valid merchant for es" +
                                "tate [Test Estate 1]"});
#line 106
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table20, "Then ");
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
                LogonTransactionFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                LogonTransactionFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
