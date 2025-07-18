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
using Reqnroll;
namespace TransactionProcessor.IntegrationTests.Features
{
    
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "2.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Contract")]
    [NUnit.Framework.FixtureLifeCycleAttribute(NUnit.Framework.LifeCycle.InstancePerTestCase)]
    [NUnit.Framework.CategoryAttribute("base")]
    [NUnit.Framework.CategoryAttribute("shared")]
    public partial class ContractFeature
    {
        
        private global::Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "shared"};
        
        private static global::Reqnroll.FeatureInfo featureInfo = new global::Reqnroll.FeatureInfo(new global::System.Globalization.CultureInfo("en-US"), "Features", "Contract", null, global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);
        
#line 1 "Contract.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public static async global::System.Threading.Tasks.Task FeatureSetupAsync()
        {
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public static async global::System.Threading.Tasks.Task FeatureTearDownAsync()
        {
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public async global::System.Threading.Tasks.Task TestInitializeAsync()
        {
            testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(featureHint: featureInfo);
            try
            {
                if (((testRunner.FeatureContext != null) 
                            && (testRunner.FeatureContext.FeatureInfo.Equals(featureInfo) == false)))
                {
                    await testRunner.OnFeatureEndAsync();
                }
            }
            finally
            {
                if (((testRunner.FeatureContext != null) 
                            && testRunner.FeatureContext.BeforeFeatureHookFailed))
                {
                    throw new global::Reqnroll.ReqnrollException("Scenario skipped because of previous before feature hook error");
                }
                if ((testRunner.FeatureContext == null))
                {
                    await testRunner.OnFeatureStartAsync(featureInfo);
                }
            }
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public async global::System.Threading.Tasks.Task TestTearDownAsync()
        {
            if ((testRunner == null))
            {
                return;
            }
            try
            {
                await testRunner.OnScenarioEndAsync();
            }
            finally
            {
                global::Reqnroll.TestRunnerManager.ReleaseTestRunner(testRunner);
                testRunner = null;
            }
        }
        
        public void ScenarioInitialize(global::Reqnroll.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public async global::System.Threading.Tasks.Task ScenarioStartAsync()
        {
            await testRunner.OnScenarioStartAsync();
        }
        
        public async global::System.Threading.Tasks.Task ScenarioCleanupAsync()
        {
            await testRunner.CollectScenarioErrorsAsync();
        }
        
        public virtual async global::System.Threading.Tasks.Task FeatureBackgroundAsync()
        {
#line 4
#line hidden
            global::Reqnroll.Table table1 = new global::Reqnroll.Table(new string[] {
                        "Role Name"});
            table1.AddRow(new string[] {
                        "Estate"});
#line 5
 await testRunner.GivenAsync("the following security roles exist", ((string)(null)), table1, "Given ");
#line hidden
            global::Reqnroll.Table table2 = new global::Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Description"});
            table2.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST Scope",
                        "A scope for Estate Managememt REST"});
#line 9
 await testRunner.GivenAsync("I create the following api scopes", ((string)(null)), table2, "Given ");
#line hidden
            global::Reqnroll.Table table3 = new global::Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Secret",
                        "Scopes",
                        "UserClaims"});
            table3.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST",
                        "Secret1",
                        "estateManagement",
                        "merchantId, estateId, role"});
#line 13
 await testRunner.GivenAsync("the following api resources exist", ((string)(null)), table3, "Given ");
#line hidden
            global::Reqnroll.Table table4 = new global::Reqnroll.Table(new string[] {
                        "ClientId",
                        "ClientName",
                        "Secret",
                        "Scopes",
                        "GrantTypes"});
            table4.AddRow(new string[] {
                        "serviceClient",
                        "Service Client",
                        "Secret1",
                        "estateManagement",
                        "client_credentials"});
            table4.AddRow(new string[] {
                        "estateClient",
                        "Estate Client",
                        "Secret1",
                        "estateManagement",
                        "password"});
#line 17
 await testRunner.GivenAsync("the following clients exist", ((string)(null)), table4, "Given ");
#line hidden
            global::Reqnroll.Table table5 = new global::Reqnroll.Table(new string[] {
                        "ClientId"});
            table5.AddRow(new string[] {
                        "serviceClient"});
#line 22
 await testRunner.GivenAsync("I have a token to access the estate management resource", ((string)(null)), table5, "Given ");
#line hidden
            global::Reqnroll.Table table6 = new global::Reqnroll.Table(new string[] {
                        "EstateName"});
            table6.AddRow(new string[] {
                        "Test Estate 1"});
            table6.AddRow(new string[] {
                        "Test Estate 2"});
#line 26
 await testRunner.GivenAsync("I have created the following estates", ((string)(null)), table6, "Given ");
#line hidden
            global::Reqnroll.Table table7 = new global::Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "RequireCustomMerchantNumber",
                        "RequireCustomTerminalNumber"});
            table7.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Operator 1",
                        "True",
                        "True"});
            table7.AddRow(new string[] {
                        "Test Estate 2",
                        "Test Operator 1",
                        "True",
                        "True"});
#line 31
 await testRunner.GivenAsync("I have created the following operators", ((string)(null)), table7, "Given ");
#line hidden
            global::Reqnroll.Table table8 = new global::Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName"});
            table8.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Operator 1"});
            table8.AddRow(new string[] {
                        "Test Estate 2",
                        "Test Operator 1"});
#line 36
 await testRunner.AndAsync("I have assigned the following operators to the estates", ((string)(null)), table8, "And ");
#line hidden
            global::Reqnroll.Table table9 = new global::Reqnroll.Table(new string[] {
                        "EmailAddress",
                        "Password",
                        "GivenName",
                        "FamilyName",
                        "EstateName"});
            table9.AddRow(new string[] {
                        "estateuser1@testestate1.co.uk",
                        "123456",
                        "TestEstate",
                        "User1",
                        "Test Estate 1"});
            table9.AddRow(new string[] {
                        "estateuser1@testestate2.co.uk",
                        "123456",
                        "TestEstate",
                        "User1",
                        "Test Estate 2"});
#line 41
 await testRunner.GivenAsync("I have created the following security users", ((string)(null)), table9, "Given ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Get Merchant Contracts")]
        public async global::System.Threading.Tasks.Task GetMerchantContracts()
        {
            string[] tagsOfScenario = ((string[])(null));
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Get Merchant Contracts", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 46
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
                global::Reqnroll.Table table10 = new global::Reqnroll.Table(new string[] {
                            "MerchantName",
                            "AddressLine1",
                            "Town",
                            "Region",
                            "Country",
                            "ContactName",
                            "EmailAddress",
                            "EstateName"});
                table10.AddRow(new string[] {
                            "Test Merchant 1",
                            "Address Line 1",
                            "TestTown",
                            "Test Region",
                            "United Kingdom",
                            "Test Contact 1",
                            "testcontact1@merchant1.co.uk",
                            "Test Estate 1"});
                table10.AddRow(new string[] {
                            "Test Merchant 2",
                            "Address Line 1",
                            "TestTown",
                            "Test Region",
                            "United Kingdom",
                            "Test Contact 1",
                            "testcontact1@merchant2.co.uk",
                            "Test Estate 2"});
#line 48
 await testRunner.GivenAsync("I create the following merchants", ((string)(null)), table10, "Given ");
#line hidden
                global::Reqnroll.Table table11 = new global::Reqnroll.Table(new string[] {
                            "EstateName",
                            "OperatorName",
                            "ContractDescription"});
                table11.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Operator 1",
                            "Operator 1 Contract Estate 1"});
                table11.AddRow(new string[] {
                            "Test Estate 2",
                            "Test Operator 1",
                            "Operator 1 Contract Estate 2"});
#line 53
 await testRunner.GivenAsync("I create a contract with the following values", ((string)(null)), table11, "Given ");
#line hidden
                global::Reqnroll.Table table12 = new global::Reqnroll.Table(new string[] {
                            "EstateName",
                            "OperatorName",
                            "ContractDescription",
                            "ProductName",
                            "DisplayText",
                            "Value",
                            "ProductType"});
                table12.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Operator 1",
                            "Operator 1 Contract Estate 1",
                            "100 KES Topup",
                            "100 KES",
                            "100.00",
                            "MobileTopup"});
                table12.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Operator 1",
                            "Operator 1 Contract Estate 1",
                            "Variable Topup 1",
                            "Custom",
                            "",
                            "MobileTopup"});
                table12.AddRow(new string[] {
                            "Test Estate 2",
                            "Test Operator 1",
                            "Operator 1 Contract Estate 2",
                            "200 KES Topup",
                            "100 KES",
                            "100.00",
                            "MobileTopup"});
                table12.AddRow(new string[] {
                            "Test Estate 2",
                            "Test Operator 1",
                            "Operator 1 Contract Estate 2",
                            "Variable Topup 2",
                            "Custom",
                            "",
                            "MobileTopup"});
#line 58
 await testRunner.WhenAsync("I create the following Products", ((string)(null)), table12, "When ");
#line hidden
                global::Reqnroll.Table table13 = new global::Reqnroll.Table(new string[] {
                            "EstateName",
                            "OperatorName",
                            "ContractDescription",
                            "ProductName",
                            "CalculationType",
                            "FeeDescription",
                            "Value",
                            "FeeType"});
                table13.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Operator 1",
                            "Operator 1 Contract Estate 1",
                            "100 KES Topup",
                            "Fixed",
                            "Merchant Commission",
                            "1.00",
                            "Merchant"});
                table13.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Operator 1",
                            "Operator 1 Contract Estate 1",
                            "100 KES Topup",
                            "Percentage",
                            "Merchant Commission",
                            "0.015",
                            "Merchant"});
                table13.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Operator 1",
                            "Operator 1 Contract Estate 1",
                            "Variable Topup 1",
                            "Fixed",
                            "Merchant Commission",
                            "1.50",
                            "Merchant"});
                table13.AddRow(new string[] {
                            "Test Estate 2",
                            "Test Operator 1",
                            "Operator 1 Contract Estate 2",
                            "200 KES Topup",
                            "Percentage",
                            "Merchant Commission",
                            "0.25",
                            "Merchant"});
                table13.AddRow(new string[] {
                            "Test Estate 2",
                            "Test Operator 1",
                            "Operator 1 Contract Estate 2",
                            "Variable Topup 2",
                            "Percentage",
                            "Merchant Commission",
                            "2.25",
                            "Merchant"});
#line 65
 await testRunner.WhenAsync("I add the following Transaction Fees", ((string)(null)), table13, "When ");
#line hidden
                global::Reqnroll.Table table14 = new global::Reqnroll.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "ContractDescription"});
                table14.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "Operator 1 Contract Estate 1"});
                table14.AddRow(new string[] {
                            "Test Estate 2",
                            "Test Merchant 2",
                            "Operator 1 Contract Estate 2"});
#line 73
 await testRunner.WhenAsync("I add the following contracts to the following merchants", ((string)(null)), table14, "When ");
#line hidden
                global::Reqnroll.Table table15 = new global::Reqnroll.Table(new string[] {
                            "ContractDescription",
                            "ProductName"});
                table15.AddRow(new string[] {
                            "Operator 1 Contract Estate 1",
                            "100 KES Topup"});
                table15.AddRow(new string[] {
                            "Operator 1 Contract Estate 1",
                            "Variable Topup 1"});
#line 78
 await testRunner.ThenAsync("I get the Contracts for \'Test Estate 1\' the following contract details are return" +
                        "ed", ((string)(null)), table15, "Then ");
#line hidden
                global::Reqnroll.Table table16 = new global::Reqnroll.Table(new string[] {
                            "ContractDescription",
                            "ProductName"});
                table16.AddRow(new string[] {
                            "Operator 1 Contract Estate 2",
                            "200 KES Topup"});
                table16.AddRow(new string[] {
                            "Operator 1 Contract Estate 2",
                            "Variable Topup 2"});
#line 83
 await testRunner.ThenAsync("I get the Contracts for \'Test Estate 2\' the following contract details are return" +
                        "ed", ((string)(null)), table16, "Then ");
#line hidden
                global::Reqnroll.Table table17 = new global::Reqnroll.Table(new string[] {
                            "ContractDescription",
                            "ProductName"});
                table17.AddRow(new string[] {
                            "Operator 1 Contract Estate 1",
                            "100 KES Topup"});
                table17.AddRow(new string[] {
                            "Operator 1 Contract Estate 1",
                            "Variable Topup 1"});
#line 88
 await testRunner.ThenAsync("I get the Merchant Contracts for \'Test Merchant 1\' for \'Test Estate 1\' the follow" +
                        "ing contract details are returned", ((string)(null)), table17, "Then ");
#line hidden
                global::Reqnroll.Table table18 = new global::Reqnroll.Table(new string[] {
                            "ContractDescription",
                            "ProductName"});
                table18.AddRow(new string[] {
                            "Operator 1 Contract Estate 2",
                            "200 KES Topup"});
                table18.AddRow(new string[] {
                            "Operator 1 Contract Estate 2",
                            "Variable Topup 2"});
#line 93
 await testRunner.ThenAsync("I get the Merchant Contracts for \'Test Merchant 2\' for \'Test Estate 2\' the follow" +
                        "ing contract details are returned", ((string)(null)), table18, "Then ");
#line hidden
                global::Reqnroll.Table table19 = new global::Reqnroll.Table(new string[] {
                            "CalculationType",
                            "FeeDescription",
                            "Value",
                            "FeeType"});
                table19.AddRow(new string[] {
                            "Fixed",
                            "Merchant Commission",
                            "1.00",
                            "Merchant"});
                table19.AddRow(new string[] {
                            "Percentage",
                            "Merchant Commission",
                            "0.015",
                            "Merchant"});
#line 98
 await testRunner.ThenAsync("I get the Transaction Fees for \'100 KES Topup\' on the \'Operator 1 Contract Estate" +
                        " 1\' contract for \'Test Estate 1\' the following fees are returned", ((string)(null)), table19, "Then ");
#line hidden
                global::Reqnroll.Table table20 = new global::Reqnroll.Table(new string[] {
                            "CalculationType",
                            "FeeDescription",
                            "Value",
                            "FeeType"});
                table20.AddRow(new string[] {
                            "Fixed",
                            "Merchant Commission",
                            "1.50",
                            "Merchant"});
#line 103
 await testRunner.ThenAsync("I get the Transaction Fees for \'Variable Topup 1\' on the \'Operator 1 Contract Est" +
                        "ate 1\' contract for \'Test Estate 1\' the following fees are returned", ((string)(null)), table20, "Then ");
#line hidden
                global::Reqnroll.Table table21 = new global::Reqnroll.Table(new string[] {
                            "CalculationType",
                            "FeeDescription",
                            "Value",
                            "FeeType"});
                table21.AddRow(new string[] {
                            "Percentage",
                            "Merchant Commission",
                            "0.25",
                            "Merchant"});
#line 107
 await testRunner.ThenAsync("I get the Transaction Fees for \'200 KES Topup\' on the \'Operator 1 Contract Estate" +
                        " 2\' contract for \'Test Estate 2\' the following fees are returned", ((string)(null)), table21, "Then ");
#line hidden
                global::Reqnroll.Table table22 = new global::Reqnroll.Table(new string[] {
                            "CalculationType",
                            "FeeDescription",
                            "Value",
                            "FeeType"});
                table22.AddRow(new string[] {
                            "Percentage",
                            "Merchant Commission",
                            "2.25",
                            "Merchant"});
#line 111
 await testRunner.ThenAsync("I get the Transaction Fees for \'Variable Topup 2\' on the \'Operator 1 Contract Est" +
                        "ate 2\' contract for \'Test Estate 2\' the following fees are returned", ((string)(null)), table22, "Then ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Prevent Duplicate Contracts")]
        public async global::System.Threading.Tasks.Task PreventDuplicateContracts()
        {
            string[] tagsOfScenario = ((string[])(null));
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Prevent Duplicate Contracts", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 116
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
                global::Reqnroll.Table table23 = new global::Reqnroll.Table(new string[] {
                            "EstateName",
                            "OperatorName",
                            "ContractDescription"});
                table23.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Operator 1",
                            "Operator 1 Contract"});
#line 118
 await testRunner.GivenAsync("I create a contract with the following values", ((string)(null)), table23, "Given ");
#line hidden
                global::Reqnroll.Table table24 = new global::Reqnroll.Table(new string[] {
                            "EstateName",
                            "OperatorName",
                            "ContractDescription"});
                table24.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Operator 1",
                            "Operator 1 Contract"});
#line 122
 await testRunner.WhenAsync("I create another contract with the same values it should be rejected", ((string)(null)), table24, "When ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
    }
}
#pragma warning restore
#endregion
