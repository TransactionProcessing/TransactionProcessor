﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by Reqnroll (https://www.reqnroll.net/).
//      Reqnroll Version:2.0.0.0
//      Reqnroll Generator Version:2.0.0.0
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "2.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("LogonTransaction")]
    [NUnit.Framework.FixtureLifeCycleAttribute(NUnit.Framework.LifeCycle.InstancePerTestCase)]
    [NUnit.Framework.CategoryAttribute("base")]
    [NUnit.Framework.CategoryAttribute("shared")]
    public partial class LogonTransactionFeature
    {
        
        private global::Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "shared"};
        
        private static global::Reqnroll.FeatureInfo featureInfo = new global::Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "LogonTransaction", null, global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);
        
#line 1 "LogonTransaction.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public static async System.Threading.Tasks.Task FeatureSetupAsync()
        {
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public static async System.Threading.Tasks.Task FeatureTearDownAsync()
        {
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public async System.Threading.Tasks.Task TestInitializeAsync()
        {
            testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(featureHint: featureInfo);
            if (((testRunner.FeatureContext != null) 
                        && (testRunner.FeatureContext.FeatureInfo.Equals(featureInfo) == false)))
            {
                await testRunner.OnFeatureEndAsync();
            }
            if ((testRunner.FeatureContext == null))
            {
                await testRunner.OnFeatureStartAsync(featureInfo);
            }
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public async System.Threading.Tasks.Task TestTearDownAsync()
        {
            await testRunner.OnScenarioEndAsync();
            global::Reqnroll.TestRunnerManager.ReleaseTestRunner(testRunner);
        }
        
        public void ScenarioInitialize(global::Reqnroll.ScenarioInfo scenarioInfo)
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
#line 4
#line hidden
            global::Reqnroll.Table table44 = new global::Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Description"});
            table44.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST Scope",
                        "A scope for Estate Managememt REST"});
            table44.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST  Scope",
                        "A scope for Transaction Processor REST"});
#line 6
 await testRunner.GivenAsync("I create the following api scopes", ((string)(null)), table44, "Given ");
#line hidden
            global::Reqnroll.Table table45 = new global::Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Secret",
                        "Scopes",
                        "UserClaims"});
            table45.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST",
                        "Secret1",
                        "estateManagement",
                        "MerchantId, EstateId, role"});
            table45.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST",
                        "Secret1",
                        "transactionProcessor",
                        ""});
#line 11
 await testRunner.GivenAsync("the following api resources exist", ((string)(null)), table45, "Given ");
#line hidden
            global::Reqnroll.Table table46 = new global::Reqnroll.Table(new string[] {
                        "ClientId",
                        "ClientName",
                        "Secret",
                        "Scopes",
                        "GrantTypes"});
            table46.AddRow(new string[] {
                        "serviceClient",
                        "Service Client",
                        "Secret1",
                        "estateManagement,transactionProcessor",
                        "client_credentials"});
#line 16
 await testRunner.GivenAsync("the following clients exist", ((string)(null)), table46, "Given ");
#line hidden
            global::Reqnroll.Table table47 = new global::Reqnroll.Table(new string[] {
                        "ClientId"});
            table47.AddRow(new string[] {
                        "serviceClient"});
#line 20
 await testRunner.GivenAsync("I have a token to access the estate management and transaction processor resource" +
                    "s", ((string)(null)), table47, "Given ");
#line hidden
            global::Reqnroll.Table table48 = new global::Reqnroll.Table(new string[] {
                        "EstateName"});
            table48.AddRow(new string[] {
                        "Test Estate 1"});
#line 24
 await testRunner.GivenAsync("I have created the following estates", ((string)(null)), table48, "Given ");
#line hidden
            global::Reqnroll.Table table49 = new global::Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "RequireCustomMerchantNumber",
                        "RequireCustomTerminalNumber"});
            table49.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Operator 1",
                        "True",
                        "True"});
#line 28
 await testRunner.GivenAsync("I have created the following operators", ((string)(null)), table49, "Given ");
#line hidden
            global::Reqnroll.Table table50 = new global::Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName"});
            table50.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Operator 1"});
#line 32
 await testRunner.AndAsync("I have assigned the following operators to the estates", ((string)(null)), table50, "And ");
#line hidden
            global::Reqnroll.Table table51 = new global::Reqnroll.Table(new string[] {
                        "MerchantName",
                        "AddressLine1",
                        "Town",
                        "Region",
                        "Country",
                        "ContactName",
                        "EmailAddress",
                        "EstateName"});
            table51.AddRow(new string[] {
                        "Test Merchant 1",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 1",
                        "testcontact1@merchant1.co.uk",
                        "Test Estate 1"});
            table51.AddRow(new string[] {
                        "Test Merchant 2",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 2",
                        "testcontact2@merchant2.co.uk",
                        "Test Estate 1"});
            table51.AddRow(new string[] {
                        "Test Merchant 3",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 3",
                        "testcontact3@merchant2.co.uk",
                        "Test Estate 1"});
            table51.AddRow(new string[] {
                        "Test Merchant 4",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 4",
                        "testcontact4@merchant2.co.uk",
                        "Test Estate 1"});
            table51.AddRow(new string[] {
                        "Test Merchant 5",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 5",
                        "testcontact5@merchant2.co.uk",
                        "Test Estate 1"});
            table51.AddRow(new string[] {
                        "Test Merchant 6",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 6",
                        "testcontact6@merchant2.co.uk",
                        "Test Estate 1"});
            table51.AddRow(new string[] {
                        "Test Merchant 7",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 7",
                        "testcontact7@merchant2.co.uk",
                        "Test Estate 1"});
#line 36
 await testRunner.GivenAsync("I create the following merchants", ((string)(null)), table51, "Given ");
#line hidden
            global::Reqnroll.Table table52 = new global::Reqnroll.Table(new string[] {
                        "OperatorName",
                        "MerchantName",
                        "MerchantNumber",
                        "TerminalNumber",
                        "EstateName"});
            table52.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 1",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table52.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 2",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table52.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 3",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table52.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 4",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table52.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 5",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table52.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 6",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table52.AddRow(new string[] {
                        "Test Operator 1",
                        "Test Merchant 7",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
#line 46
 await testRunner.GivenAsync("I have assigned the following operator to the merchants", ((string)(null)), table52, "Given ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Logon Transactions")]
        [NUnit.Framework.CategoryAttribute("PRTest")]
        public async System.Threading.Tasks.Task LogonTransactions()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Logon Transactions", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 57
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 4
await this.FeatureBackgroundAsync();
#line hidden
                global::Reqnroll.Table table53 = new global::Reqnroll.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table53.AddRow(new string[] {
                            "Today",
                            "1",
                            "Logon",
                            "Test Merchant 1",
                            "123456780",
                            "Test Estate 1"});
                table53.AddRow(new string[] {
                            "Today",
                            "2",
                            "Logon",
                            "Test Merchant 2",
                            "123456781",
                            "Test Estate 1"});
                table53.AddRow(new string[] {
                            "Today",
                            "3",
                            "Logon",
                            "Test Merchant 3",
                            "123456782",
                            "Test Estate 1"});
#line 59
 await testRunner.WhenAsync("I perform the following transactions", ((string)(null)), table53, "When ");
#line hidden
                global::Reqnroll.Table table54 = new global::Reqnroll.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table54.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "1",
                            "0001",
                            "SUCCESS"});
                table54.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 2",
                            "2",
                            "0001",
                            "SUCCESS"});
                table54.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 3",
                            "3",
                            "0001",
                            "SUCCESS"});
#line 65
 await testRunner.ThenAsync("transaction response should contain the following information", ((string)(null)), table54, "Then ");
#line hidden
                global::Reqnroll.Table table55 = new global::Reqnroll.Table(new string[] {
                            "DeviceIdentifier",
                            "MerchantName",
                            "MerchantNumber",
                            "EstateName"});
                table55.AddRow(new string[] {
                            "123456783",
                            "Test Merchant 4",
                            "00000001",
                            "Test Estate 1"});
#line 71
 await testRunner.GivenAsync("I have assigned the following devices to the merchants", ((string)(null)), table55, "Given ");
#line hidden
                global::Reqnroll.Table table56 = new global::Reqnroll.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table56.AddRow(new string[] {
                            "Today",
                            "4",
                            "Logon",
                            "Test Merchant 4",
                            "123456783",
                            "Test Estate 1"});
#line 75
 await testRunner.WhenAsync("I perform the following transactions", ((string)(null)), table56, "When ");
#line hidden
                global::Reqnroll.Table table57 = new global::Reqnroll.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table57.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 4",
                            "4",
                            "0000",
                            "SUCCESS"});
#line 79
 await testRunner.ThenAsync("transaction response should contain the following information", ((string)(null)), table57, "Then ");
#line hidden
                global::Reqnroll.Table table58 = new global::Reqnroll.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table58.AddRow(new string[] {
                            "Today",
                            "5",
                            "Logon",
                            "Test Merchant 1",
                            "13579135",
                            "Test Estate 1"});
#line 83
 await testRunner.WhenAsync("I perform the following transactions", ((string)(null)), table58, "When ");
#line hidden
                global::Reqnroll.Table table59 = new global::Reqnroll.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table59.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "5",
                            "1000",
                            "Device Identifier 13579135 not valid for Merchant Test Merchant 1"});
#line 87
 await testRunner.ThenAsync("transaction response should contain the following information", ((string)(null)), table59, "Then ");
#line hidden
                global::Reqnroll.Table table60 = new global::Reqnroll.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table60.AddRow(new string[] {
                            "Today",
                            "6",
                            "Logon",
                            "Test Merchant 1",
                            "123456785",
                            "InvalidEstate"});
#line 91
 await testRunner.WhenAsync("I perform the following transactions", ((string)(null)), table60, "When ");
#line hidden
                global::Reqnroll.Table table61 = new global::Reqnroll.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table61.AddRow(new string[] {
                            "InvalidEstate",
                            "Test Merchant 1",
                            "6",
                            "1001",
                            "Estate Id [79902550-64df-4491-b0c1-4e78943928a3] is not a valid estate"});
#line 95
 await testRunner.ThenAsync("transaction response should contain the following information", ((string)(null)), table61, "Then ");
#line hidden
                global::Reqnroll.Table table62 = new global::Reqnroll.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName"});
                table62.AddRow(new string[] {
                            "Today",
                            "7",
                            "Logon",
                            "InvalidMerchant",
                            "123456786",
                            "Test Estate 1"});
#line 99
 await testRunner.WhenAsync("I perform the following transactions", ((string)(null)), table62, "When ");
#line hidden
                global::Reqnroll.Table table63 = new global::Reqnroll.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table63.AddRow(new string[] {
                            "Test Estate 1",
                            "InvalidMerchant",
                            "7",
                            "1002",
                            "Merchant Id [d59320fa-4c3e-4900-a999-483f6a10c69a] is not a valid merchant for es" +
                                "tate [Test Estate 1]"});
#line 103
 await testRunner.ThenAsync("transaction response should contain the following information", ((string)(null)), table63, "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
    }
}
#pragma warning restore
#endregion
