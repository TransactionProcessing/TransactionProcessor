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
    [NUnit.Framework.DescriptionAttribute("Reconciliation")]
    [NUnit.Framework.CategoryAttribute("base")]
    [NUnit.Framework.CategoryAttribute("shared")]
    public partial class ReconciliationFeature
    {
        
        private Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "shared"};
        
#line 1 "ReconciliationFeature.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual async System.Threading.Tasks.Task FeatureSetupAsync()
        {
            testRunner = Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(null, NUnit.Framework.TestContext.CurrentContext.WorkerId);
            Reqnroll.FeatureInfo featureInfo = new Reqnroll.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "Reconciliation", null, ProgrammingLanguage.CSharp, featureTags);
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
#line 4
#line hidden
            Reqnroll.Table table21 = new Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Description"});
            table21.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST Scope",
                        "A scope for Estate Managememt REST"});
            table21.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST  Scope",
                        "A scope for Transaction Processor REST"});
#line 6
 await testRunner.GivenAsync("I create the following api scopes", ((string)(null)), table21, "Given ");
#line hidden
            Reqnroll.Table table22 = new Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Secret",
                        "Scopes",
                        "UserClaims"});
            table22.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST",
                        "Secret1",
                        "estateManagement",
                        "MerchantId, EstateId, role"});
            table22.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST",
                        "Secret1",
                        "transactionProcessor",
                        ""});
#line 11
 await testRunner.GivenAsync("the following api resources exist", ((string)(null)), table22, "Given ");
#line hidden
            Reqnroll.Table table23 = new Reqnroll.Table(new string[] {
                        "ClientId",
                        "ClientName",
                        "Secret",
                        "Scopes",
                        "GrantTypes"});
            table23.AddRow(new string[] {
                        "serviceClient",
                        "Service Client",
                        "Secret1",
                        "estateManagement,transactionProcessor",
                        "client_credentials"});
#line 16
 await testRunner.GivenAsync("the following clients exist", ((string)(null)), table23, "Given ");
#line hidden
            Reqnroll.Table table24 = new Reqnroll.Table(new string[] {
                        "ClientId"});
            table24.AddRow(new string[] {
                        "serviceClient"});
#line 20
 await testRunner.GivenAsync("I have a token to access the estate management and transaction processor resource" +
                    "s", ((string)(null)), table24, "Given ");
#line hidden
            Reqnroll.Table table25 = new Reqnroll.Table(new string[] {
                        "EstateName"});
            table25.AddRow(new string[] {
                        "Test Estate 1"});
            table25.AddRow(new string[] {
                        "Test Estate 2"});
#line 24
 await testRunner.GivenAsync("I have created the following estates", ((string)(null)), table25, "Given ");
#line hidden
            Reqnroll.Table table26 = new Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "RequireCustomMerchantNumber",
                        "RequireCustomTerminalNumber"});
            table26.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "True",
                        "True"});
            table26.AddRow(new string[] {
                        "Test Estate 2",
                        "Safaricom",
                        "True",
                        "True"});
#line 29
 await testRunner.GivenAsync("I have created the following operators", ((string)(null)), table26, "Given ");
#line hidden
            Reqnroll.Table table27 = new Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName"});
            table27.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom"});
            table27.AddRow(new string[] {
                        "Test Estate 2",
                        "Safaricom"});
#line 34
 await testRunner.AndAsync("I have assigned the following operators to the estates", ((string)(null)), table27, "And ");
#line hidden
            Reqnroll.Table table28 = new Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription"});
            table28.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract"});
            table28.AddRow(new string[] {
                        "Test Estate 2",
                        "Safaricom",
                        "Safaricom Contract"});
#line 39
 await testRunner.GivenAsync("I create a contract with the following values", ((string)(null)), table28, "Given ");
#line hidden
            Reqnroll.Table table29 = new Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription",
                        "ProductName",
                        "DisplayText",
                        "Value",
                        "ProductType"});
            table29.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Custom",
                        "",
                        "MobileTopup"});
            table29.AddRow(new string[] {
                        "Test Estate 2",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Custom",
                        "",
                        "MobileTopup"});
#line 44
 await testRunner.WhenAsync("I create the following Products", ((string)(null)), table29, "When ");
#line hidden
            Reqnroll.Table table30 = new Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription",
                        "ProductName",
                        "CalculationType",
                        "FeeDescription",
                        "Value"});
            table30.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Fixed",
                        "Merchant Commission",
                        "2.50"});
            table30.AddRow(new string[] {
                        "Test Estate 2",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Percentage",
                        "Merchant Commission",
                        "0.85"});
#line 49
 await testRunner.WhenAsync("I add the following Transaction Fees", ((string)(null)), table30, "When ");
#line hidden
            Reqnroll.Table table31 = new Reqnroll.Table(new string[] {
                        "MerchantName",
                        "AddressLine1",
                        "Town",
                        "Region",
                        "Country",
                        "ContactName",
                        "EmailAddress",
                        "EstateName"});
            table31.AddRow(new string[] {
                        "Test Merchant 1",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 1",
                        "testcontact1@merchant1.co.uk",
                        "Test Estate 1"});
            table31.AddRow(new string[] {
                        "Test Merchant 2",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 2",
                        "testcontact2@merchant2.co.uk",
                        "Test Estate 1"});
            table31.AddRow(new string[] {
                        "Test Merchant 3",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 3",
                        "testcontact3@merchant2.co.uk",
                        "Test Estate 2"});
#line 54
 await testRunner.GivenAsync("I create the following merchants", ((string)(null)), table31, "Given ");
#line hidden
            Reqnroll.Table table32 = new Reqnroll.Table(new string[] {
                        "OperatorName",
                        "MerchantName",
                        "MerchantNumber",
                        "TerminalNumber",
                        "EstateName"});
            table32.AddRow(new string[] {
                        "Safaricom",
                        "Test Merchant 1",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table32.AddRow(new string[] {
                        "Safaricom",
                        "Test Merchant 2",
                        "00000002",
                        "10000002",
                        "Test Estate 1"});
            table32.AddRow(new string[] {
                        "Safaricom",
                        "Test Merchant 3",
                        "00000003",
                        "10000003",
                        "Test Estate 2"});
#line 60
 await testRunner.GivenAsync("I have assigned the following  operator to the merchants", ((string)(null)), table32, "Given ");
#line hidden
            Reqnroll.Table table33 = new Reqnroll.Table(new string[] {
                        "DeviceIdentifier",
                        "MerchantName",
                        "EstateName"});
            table33.AddRow(new string[] {
                        "123456780",
                        "Test Merchant 1",
                        "Test Estate 1"});
            table33.AddRow(new string[] {
                        "123456781",
                        "Test Merchant 2",
                        "Test Estate 1"});
            table33.AddRow(new string[] {
                        "123456782",
                        "Test Merchant 3",
                        "Test Estate 2"});
#line 66
 await testRunner.GivenAsync("I have assigned the following devices to the merchants", ((string)(null)), table33, "Given ");
#line hidden
            Reqnroll.Table table34 = new Reqnroll.Table(new string[] {
                        "Reference",
                        "Amount",
                        "DateTime",
                        "MerchantName",
                        "EstateName"});
            table34.AddRow(new string[] {
                        "Deposit1",
                        "200.00",
                        "Today",
                        "Test Merchant 1",
                        "Test Estate 1"});
            table34.AddRow(new string[] {
                        "Deposit1",
                        "100.00",
                        "Today",
                        "Test Merchant 2",
                        "Test Estate 1"});
            table34.AddRow(new string[] {
                        "Deposit1",
                        "100.00",
                        "Today",
                        "Test Merchant 3",
                        "Test Estate 2"});
#line 72
 await testRunner.GivenAsync("I make the following manual merchant deposits", ((string)(null)), table34, "Given ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Reconciliation Transactions")]
        [NUnit.Framework.CategoryAttribute("PRTest")]
        public async System.Threading.Tasks.Task ReconciliationTransactions()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            Reqnroll.ScenarioInfo scenarioInfo = new Reqnroll.ScenarioInfo("Reconciliation Transactions", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 79
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 4
await this.FeatureBackgroundAsync();
#line hidden
                Reqnroll.Table table35 = new Reqnroll.Table(new string[] {
                            "DateTime",
                            "MerchantName",
                            "DeviceIdentifier",
                            "EstateName",
                            "TransactionCount",
                            "TransactionValue",
                            "TransactionType"});
                table35.AddRow(new string[] {
                            "Today",
                            "Test Merchant 1",
                            "123456780",
                            "Test Estate 1",
                            "1",
                            "100.00",
                            "Reconciliation"});
                table35.AddRow(new string[] {
                            "Today",
                            "Test Merchant 2",
                            "123456781",
                            "Test Estate 1",
                            "2",
                            "200.00",
                            "Reconciliation"});
                table35.AddRow(new string[] {
                            "Today",
                            "Test Merchant 3",
                            "123456782",
                            "Test Estate 2",
                            "3",
                            "300.00",
                            "Reconciliation"});
#line 81
 await testRunner.WhenAsync("I perform the following reconciliations", ((string)(null)), table35, "When ");
#line hidden
                Reqnroll.Table table36 = new Reqnroll.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "ResponseCode",
                            "ResponseMessage"});
                table36.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "0000",
                            "SUCCESS"});
                table36.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 2",
                            "0000",
                            "SUCCESS"});
                table36.AddRow(new string[] {
                            "Test Estate 2",
                            "Test Merchant 3",
                            "0000",
                            "SUCCESS"});
#line 87
 await testRunner.ThenAsync("reconciliation response should contain the following information", ((string)(null)), table36, "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
    }
}
#pragma warning restore
#endregion
