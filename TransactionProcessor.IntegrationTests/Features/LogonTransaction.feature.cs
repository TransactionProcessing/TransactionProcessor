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
            table5.AddRow(new string[] {
                        "Test Estate 2"});
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
#line 29
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
                        "Test Estate 2"});
#line 33
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
#line 39
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
                        "Test Estate 2"});
#line 43
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
#line 50
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
                            "Test Estate 2"});
#line 52
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
                            "0000",
                            "SUCCESS"});
                table11.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 2",
                            "2",
                            "0000",
                            "SUCCESS"});
                table11.AddRow(new string[] {
                            "Test Estate 2",
                            "Test Merchant 3",
                            "3",
                            "0000",
                            "SUCCESS"});
#line 58
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table11, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Logon Transaction with Existing Device")]
        [Xunit.TraitAttribute("FeatureTitle", "LogonTransaction")]
        [Xunit.TraitAttribute("Description", "Logon Transaction with Existing Device")]
        public void LogonTransactionWithExistingDevice()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Logon Transaction with Existing Device", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 64
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
                TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                            "DeviceIdentifier",
                            "MerchantName",
                            "MerchantNumber",
                            "EstateName"});
                table12.AddRow(new string[] {
                            "123456780",
                            "Test Merchant 1",
                            "00000001",
                            "Test Estate 1"});
#line 66
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
                            "1",
                            "Logon",
                            "Test Merchant 1",
                            "123456780",
                            "Test Estate 1"});
#line 70
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
                            "Test Merchant 1",
                            "1",
                            "0000",
                            "SUCCESS"});
#line 74
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table14, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Logon Transaction with Invalid Device")]
        [Xunit.TraitAttribute("FeatureTitle", "LogonTransaction")]
        [Xunit.TraitAttribute("Description", "Logon Transaction with Invalid Device")]
        public void LogonTransactionWithInvalidDevice()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Logon Transaction with Invalid Device", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 78
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
                TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                            "DeviceIdentifier",
                            "MerchantName",
                            "MerchantNumber",
                            "EstateName"});
                table15.AddRow(new string[] {
                            "123456780",
                            "Test Merchant 1",
                            "00000001",
                            "Test Estate 1"});
#line 80
 testRunner.Given("I have assigned the following devices to the merchants", ((string)(null)), table15, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table16.AddRow(new string[] {
                            "Today",
                            "1",
                            "Logon",
                            "Test Merchant 1",
                            "123456781",
                            "Test Estate 1"});
#line 84
 testRunner.When("I perform the following transactions", ((string)(null)), table16, "When ");
#line hidden
                TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table17.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "1",
                            "1000",
                            "Device Identifier 123456781 not valid for Merchant Test Merchant 1"});
#line 88
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table17, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Logon Transaction with Invalid Estate")]
        [Xunit.TraitAttribute("FeatureTitle", "LogonTransaction")]
        [Xunit.TraitAttribute("Description", "Logon Transaction with Invalid Estate")]
        public void LogonTransactionWithInvalidEstate()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Logon Transaction with Invalid Estate", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 92
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
                TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                            "DeviceIdentifier",
                            "MerchantName",
                            "MerchantNumber",
                            "EstateName"});
                table18.AddRow(new string[] {
                            "123456780",
                            "Test Merchant 1",
                            "00000001",
                            "Test Estate 1"});
#line 94
 testRunner.Given("I have assigned the following devices to the merchants", ((string)(null)), table18, "Given ");
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
                            "1",
                            "Logon",
                            "Test Merchant 1",
                            "123456781",
                            "InvalidEstate"});
#line 98
 testRunner.When("I perform the following transactions", ((string)(null)), table19, "When ");
#line hidden
                TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table20.AddRow(new string[] {
                            "InvalidEstate",
                            "Test Merchant 1",
                            "1",
                            "1001",
                            "Estate Id [79902550-64df-4491-b0c1-4e78943928a3] is not a valid estate"});
#line 102
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table20, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Logon Transaction with Invalid Merchant")]
        [Xunit.TraitAttribute("FeatureTitle", "LogonTransaction")]
        [Xunit.TraitAttribute("Description", "Logon Transaction with Invalid Merchant")]
        public void LogonTransactionWithInvalidMerchant()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Logon Transaction with Invalid Merchant", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 106
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
                TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                            "DeviceIdentifier",
                            "MerchantName",
                            "MerchantNumber",
                            "EstateName"});
                table21.AddRow(new string[] {
                            "123456780",
                            "Test Merchant 1",
                            "00000001",
                            "Test Estate 1"});
#line 108
 testRunner.Given("I have assigned the following devices to the merchants", ((string)(null)), table21, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table22 = new TechTalk.SpecFlow.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table22.AddRow(new string[] {
                            "Today",
                            "1",
                            "Logon",
                            "InvalidMerchant",
                            "123456781",
                            "Test Estate 1"});
#line 112
 testRunner.When("I perform the following transactions", ((string)(null)), table22, "When ");
#line hidden
                TechTalk.SpecFlow.Table table23 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table23.AddRow(new string[] {
                            "Test Estate 1",
                            "InvalidMerchant",
                            "1",
                            "1002",
                            "Merchant Id [d59320fa-4c3e-4900-a999-483f6a10c69a] is not a valid merchant for es" +
                                "tate [Test Estate 1]"});
#line 116
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table23, "Then ");
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
