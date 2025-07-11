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
    [NUnit.Framework.DescriptionAttribute("SettlementReporting")]
    [NUnit.Framework.FixtureLifeCycleAttribute(NUnit.Framework.LifeCycle.InstancePerTestCase)]
    [NUnit.Framework.CategoryAttribute("base")]
    [NUnit.Framework.CategoryAttribute("shared")]
    public partial class SettlementReportingFeature
    {
        
        private global::Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "shared"};
        
        private static global::Reqnroll.FeatureInfo featureInfo = new global::Reqnroll.FeatureInfo(new global::System.Globalization.CultureInfo("en-US"), "Features", "SettlementReporting", null, global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);
        
#line 1 "SettlementReporting.feature"
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
            global::Reqnroll.Table table213 = new global::Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Description"});
            table213.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST Scope",
                        "A scope for Estate Managememt REST"});
            table213.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST  Scope",
                        "A scope for Transaction Processor REST"});
#line 6
 await testRunner.GivenAsync("I create the following api scopes", ((string)(null)), table213, "Given ");
#line hidden
            global::Reqnroll.Table table214 = new global::Reqnroll.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Secret",
                        "Scopes",
                        "UserClaims"});
            table214.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST",
                        "Secret1",
                        "estateManagement",
                        "MerchantId, EstateId, role"});
            table214.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST",
                        "Secret1",
                        "transactionProcessor",
                        ""});
#line 11
 await testRunner.GivenAsync("the following api resources exist", ((string)(null)), table214, "Given ");
#line hidden
            global::Reqnroll.Table table215 = new global::Reqnroll.Table(new string[] {
                        "ClientId",
                        "ClientName",
                        "Secret",
                        "Scopes",
                        "GrantTypes"});
            table215.AddRow(new string[] {
                        "serviceClient",
                        "Service Client",
                        "Secret1",
                        "estateManagement,transactionProcessor",
                        "client_credentials"});
#line 16
 await testRunner.GivenAsync("the following clients exist", ((string)(null)), table215, "Given ");
#line hidden
            global::Reqnroll.Table table216 = new global::Reqnroll.Table(new string[] {
                        "ClientId"});
            table216.AddRow(new string[] {
                        "serviceClient"});
#line 20
 await testRunner.GivenAsync("I have a token to access the estate management and transaction processor resource" +
                    "s", ((string)(null)), table216, "Given ");
#line hidden
            global::Reqnroll.Table table217 = new global::Reqnroll.Table(new string[] {
                        "EstateName"});
            table217.AddRow(new string[] {
                        "Test Estate 1"});
            table217.AddRow(new string[] {
                        "Test Estate 2"});
#line 24
 await testRunner.GivenAsync("I have created the following estates", ((string)(null)), table217, "Given ");
#line hidden
            global::Reqnroll.Table table218 = new global::Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "RequireCustomMerchantNumber",
                        "RequireCustomTerminalNumber"});
            table218.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "True",
                        "True"});
            table218.AddRow(new string[] {
                        "Test Estate 2",
                        "Safaricom",
                        "True",
                        "True"});
#line 29
 await testRunner.GivenAsync("I have created the following operators", ((string)(null)), table218, "Given ");
#line hidden
            global::Reqnroll.Table table219 = new global::Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName"});
            table219.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom"});
            table219.AddRow(new string[] {
                        "Test Estate 2",
                        "Safaricom"});
#line 34
 await testRunner.AndAsync("I have assigned the following operators to the estates", ((string)(null)), table219, "And ");
#line hidden
            global::Reqnroll.Table table220 = new global::Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription"});
            table220.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract"});
            table220.AddRow(new string[] {
                        "Test Estate 2",
                        "Safaricom",
                        "Safaricom Contract"});
#line 39
 await testRunner.GivenAsync("I create a contract with the following values", ((string)(null)), table220, "Given ");
#line hidden
            global::Reqnroll.Table table221 = new global::Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription",
                        "ProductName",
                        "DisplayText",
                        "Value",
                        "ProductType"});
            table221.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Custom",
                        "",
                        "MobileTopup"});
            table221.AddRow(new string[] {
                        "Test Estate 2",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Custom",
                        "",
                        "MobileTopup"});
#line 44
 await testRunner.WhenAsync("I create the following Products", ((string)(null)), table221, "When ");
#line hidden
            global::Reqnroll.Table table222 = new global::Reqnroll.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription",
                        "ProductName",
                        "CalculationType",
                        "FeeDescription",
                        "Value",
                        "FeeType"});
            table222.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Percentage",
                        "Merchant Commission",
                        "0.50",
                        "Merchant"});
            table222.AddRow(new string[] {
                        "Test Estate 2",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Percentage",
                        "Merchant Commission",
                        "0.85",
                        "Merchant"});
#line 49
 await testRunner.WhenAsync("I add the following Transaction Fees", ((string)(null)), table222, "When ");
#line hidden
            global::Reqnroll.Table table223 = new global::Reqnroll.Table(new string[] {
                        "MerchantName",
                        "AddressLine1",
                        "Town",
                        "Region",
                        "Country",
                        "ContactName",
                        "EmailAddress",
                        "EstateName",
                        "SettlementSchedule"});
            table223.AddRow(new string[] {
                        "Test Merchant 1",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 1",
                        "testcontact1@merchant1.co.uk",
                        "Test Estate 1",
                        "Weekly"});
            table223.AddRow(new string[] {
                        "Test Merchant 2",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 2",
                        "testcontact2@merchant2.co.uk",
                        "Test Estate 1",
                        "Weekly"});
            table223.AddRow(new string[] {
                        "Test Merchant 3",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 3",
                        "testcontact3@merchant2.co.uk",
                        "Test Estate 2",
                        "Monthly"});
#line 54
 await testRunner.GivenAsync("I create the following merchants", ((string)(null)), table223, "Given ");
#line hidden
            global::Reqnroll.Table table224 = new global::Reqnroll.Table(new string[] {
                        "OperatorName",
                        "MerchantName",
                        "MerchantNumber",
                        "TerminalNumber",
                        "EstateName"});
            table224.AddRow(new string[] {
                        "Safaricom",
                        "Test Merchant 1",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table224.AddRow(new string[] {
                        "Safaricom",
                        "Test Merchant 2",
                        "00000002",
                        "10000002",
                        "Test Estate 1"});
            table224.AddRow(new string[] {
                        "Safaricom",
                        "Test Merchant 3",
                        "00000003",
                        "10000003",
                        "Test Estate 2"});
#line 60
 await testRunner.GivenAsync("I have assigned the following operator to the merchants", ((string)(null)), table224, "Given ");
#line hidden
            global::Reqnroll.Table table225 = new global::Reqnroll.Table(new string[] {
                        "DeviceIdentifier",
                        "MerchantName",
                        "EstateName"});
            table225.AddRow(new string[] {
                        "123456780",
                        "Test Merchant 1",
                        "Test Estate 1"});
            table225.AddRow(new string[] {
                        "123456781",
                        "Test Merchant 2",
                        "Test Estate 1"});
            table225.AddRow(new string[] {
                        "123456782",
                        "Test Merchant 3",
                        "Test Estate 2"});
#line 66
 await testRunner.GivenAsync("I have assigned the following devices to the merchants", ((string)(null)), table225, "Given ");
#line hidden
            global::Reqnroll.Table table226 = new global::Reqnroll.Table(new string[] {
                        "EstateName",
                        "MerchantName",
                        "ContractDescription"});
            table226.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 1",
                        "Safaricom Contract"});
            table226.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 2",
                        "Safaricom Contract"});
            table226.AddRow(new string[] {
                        "Test Estate 2",
                        "Test Merchant 3",
                        "Safaricom Contract"});
#line 72
 await testRunner.WhenAsync("I add the following contracts to the following merchants", ((string)(null)), table226, "When ");
#line hidden
            global::Reqnroll.Table table227 = new global::Reqnroll.Table(new string[] {
                        "Reference",
                        "Amount",
                        "DateTime",
                        "MerchantName",
                        "EstateName"});
            table227.AddRow(new string[] {
                        "Deposit1",
                        "50000.00",
                        "Today",
                        "Test Merchant 1",
                        "Test Estate 1"});
            table227.AddRow(new string[] {
                        "Deposit1",
                        "50000.00",
                        "Today",
                        "Test Merchant 2",
                        "Test Estate 1"});
            table227.AddRow(new string[] {
                        "Deposit1",
                        "50000.00",
                        "Today",
                        "Test Merchant 3",
                        "Test Estate 2"});
#line 78
 await testRunner.GivenAsync("I make the following manual merchant deposits", ((string)(null)), table227, "Given ");
#line hidden
            global::Reqnroll.Table table228 = new global::Reqnroll.Table(new string[] {
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
                        "ProductName"});
            table228.AddRow(new string[] {
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
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "2",
                        "Sale",
                        "1",
                        "Test Merchant 1",
                        "123456780",
                        "Test Estate 1",
                        "Safaricom",
                        "5.00",
                        "123456789",
                        "",
                        "Safaricom Contract",
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "3",
                        "Sale",
                        "1",
                        "Test Merchant 1",
                        "123456780",
                        "Test Estate 1",
                        "Safaricom",
                        "25.00",
                        "123456789",
                        "",
                        "Safaricom Contract",
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "4",
                        "Sale",
                        "1",
                        "Test Merchant 1",
                        "123456780",
                        "Test Estate 1",
                        "Safaricom",
                        "150.00",
                        "123456789",
                        "",
                        "Safaricom Contract",
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "5",
                        "Sale",
                        "1",
                        "Test Merchant 1",
                        "123456780",
                        "Test Estate 1",
                        "Safaricom",
                        "3.00",
                        "123456789",
                        "",
                        "Safaricom Contract",
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "6",
                        "Sale",
                        "1",
                        "Test Merchant 1",
                        "123456780",
                        "Test Estate 1",
                        "Safaricom",
                        "40.00",
                        "123456789",
                        "",
                        "Safaricom Contract",
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "7",
                        "Sale",
                        "1",
                        "Test Merchant 1",
                        "123456780",
                        "Test Estate 1",
                        "Safaricom",
                        "60.00",
                        "123456789",
                        "",
                        "Safaricom Contract",
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "8",
                        "Sale",
                        "1",
                        "Test Merchant 1",
                        "123456780",
                        "Test Estate 1",
                        "Safaricom",
                        "101.00",
                        "123456789",
                        "",
                        "Safaricom Contract",
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "1",
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
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "2",
                        "Sale",
                        "1",
                        "Test Merchant 2",
                        "123456781",
                        "Test Estate 1",
                        "Safaricom",
                        "5.00",
                        "123456789",
                        "",
                        "Safaricom Contract",
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "3",
                        "Sale",
                        "1",
                        "Test Merchant 2",
                        "123456781",
                        "Test Estate 1",
                        "Safaricom",
                        "25.00",
                        "123456789",
                        "",
                        "Safaricom Contract",
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "4",
                        "Sale",
                        "1",
                        "Test Merchant 2",
                        "123456781",
                        "Test Estate 1",
                        "Safaricom",
                        "15.00",
                        "123456789",
                        "",
                        "Safaricom Contract",
                        "Variable Topup"});
            table228.AddRow(new string[] {
                        "2022-01-06",
                        "1",
                        "Sale",
                        "1",
                        "Test Merchant 3",
                        "123456782",
                        "Test Estate 2",
                        "Safaricom",
                        "100.00",
                        "123456789",
                        "",
                        "Safaricom Contract",
                        "Variable Topup"});
#line 84
 await testRunner.WhenAsync("I perform the following transactions", ((string)(null)), table228, "When ");
#line hidden
            global::Reqnroll.Table table229 = new global::Reqnroll.Table(new string[] {
                        "EstateName",
                        "MerchantName",
                        "TransactionNumber",
                        "ResponseCode",
                        "ResponseMessage"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 1",
                        "1",
                        "0000",
                        "SUCCESS"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 1",
                        "2",
                        "1008",
                        "DECLINED BY OPERATOR"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 1",
                        "3",
                        "0000",
                        "SUCCESS"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 1",
                        "4",
                        "0000",
                        "SUCCESS"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 1",
                        "5",
                        "1008",
                        "DECLINED BY OPERATOR"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 1",
                        "6",
                        "0000",
                        "SUCCESS"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 1",
                        "7",
                        "0000",
                        "SUCCESS"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 1",
                        "8",
                        "0000",
                        "SUCCESS"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 2",
                        "1",
                        "0000",
                        "SUCCESS"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 2",
                        "2",
                        "1008",
                        "DECLINED BY OPERATOR"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 2",
                        "3",
                        "0000",
                        "SUCCESS"});
            table229.AddRow(new string[] {
                        "Test Estate 1",
                        "Test Merchant 2",
                        "4",
                        "0000",
                        "SUCCESS"});
            table229.AddRow(new string[] {
                        "Test Estate 2",
                        "Test Merchant 3",
                        "1",
                        "0000",
                        "SUCCESS"});
#line 102
 await testRunner.ThenAsync("transaction response should contain the following information", ((string)(null)), table229, "Then ");
#line hidden
            global::Reqnroll.Table table230 = new global::Reqnroll.Table(new string[] {
                        "SettlementDate",
                        "EstateName",
                        "MerchantName",
                        "NumberOfFees"});
            table230.AddRow(new string[] {
                        "2022-01-13",
                        "Test Estate 1",
                        "Test Merchant 1",
                        "6"});
            table230.AddRow(new string[] {
                        "2022-01-13",
                        "Test Estate 1",
                        "Test Merchant 2",
                        "3"});
#line 120
 await testRunner.WhenAsync("I get the pending settlements the following information should be returned", ((string)(null)), table230, "When ");
#line hidden
#line 125
 await testRunner.WhenAsync("I process the settlement for \'2022-01-13\' on Estate \'Test Estate 1\' for Merchant " +
                    "\'Test Merchant 1\' then 6 fees are marked as settled and the settlement is comple" +
                    "ted", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 127
 await testRunner.WhenAsync("I process the settlement for \'2022-01-13\' on Estate \'Test Estate 1\' for Merchant " +
                    "\'Test Merchant 2\' then 3 fees are marked as settled and the settlement is comple" +
                    "ted", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
            global::Reqnroll.Table table231 = new global::Reqnroll.Table(new string[] {
                        "SettlementDate",
                        "EstateName",
                        "MerchantName",
                        "NumberOfFees"});
            table231.AddRow(new string[] {
                        "2022-02-06",
                        "Test Estate 2",
                        "Test Merchant 3",
                        "1"});
#line 129
 await testRunner.WhenAsync("I get the pending settlements the following information should be returned", ((string)(null)), table231, "When ");
#line hidden
#line 133
 await testRunner.WhenAsync("I process the settlement for \'2022-02-06\' on Estate \'Test Estate 2\' for Merchant " +
                    "\'Test Merchant 3\' then 1 fees are marked as settled and the settlement is comple" +
                    "ted", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Get Settlements - Merchant Filter")]
        [NUnit.Framework.CategoryAttribute("settlement")]
        [NUnit.Framework.CategoryAttribute("PRTest")]
        public async global::System.Threading.Tasks.Task GetSettlements_MerchantFilter()
        {
            string[] tagsOfScenario = new string[] {
                    "settlement",
                    "PRTest"};
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Get Settlements - Merchant Filter", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 137
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
                global::Reqnroll.Table table232 = new global::Reqnroll.Table(new string[] {
                            "SettlementDate",
                            "NumberOfFeesSettled",
                            "ValueOfFeesSettled",
                            "IsCompleted"});
                table232.AddRow(new string[] {
                            "2022-01-13",
                            "6",
                            "2.39",
                            "True"});
#line 138
 await testRunner.WhenAsync("I get the Estate Settlement Report for Estate \'Test Estate 1\' for Merchant \'Test " +
                        "Merchant 1\' with the Start Date \'2022-01-13\' and the End Date \'2022-02-06\' the f" +
                        "ollowing data is returned", ((string)(null)), table232, "When ");
#line hidden
                global::Reqnroll.Table table233 = new global::Reqnroll.Table(new string[] {
                            "SettlementDate",
                            "NumberOfFeesSettled",
                            "ValueOfFeesSettled",
                            "IsCompleted"});
                table233.AddRow(new string[] {
                            "2022-01-13",
                            "3",
                            "0.71",
                            "True"});
#line 142
 await testRunner.WhenAsync("I get the Estate Settlement Report for Estate \'Test Estate 1\' for Merchant \'Test " +
                        "Merchant 2\' with the Start Date \'2022-01-13\' and the End Date \'2022-02-06\' the f" +
                        "ollowing data is returned", ((string)(null)), table233, "When ");
#line hidden
                global::Reqnroll.Table table234 = new global::Reqnroll.Table(new string[] {
                            "SettlementDate",
                            "NumberOfFeesSettled",
                            "ValueOfFeesSettled",
                            "IsCompleted"});
                table234.AddRow(new string[] {
                            "2022-02-06",
                            "1",
                            "0.85",
                            "True"});
#line 146
 await testRunner.WhenAsync("I get the Estate Settlement Report for Estate \'Test Estate 2\' for Merchant \'Test " +
                        "Merchant 3\' with the Start Date \'2022-01-13\' and the End Date \'2022-02-06\' the f" +
                        "ollowing data is returned", ((string)(null)), table234, "When ");
#line hidden
                global::Reqnroll.Table table235 = new global::Reqnroll.Table(new string[] {
                            "FeeDescription",
                            "IsSettled",
                            "Operator",
                            "CalculatedValue"});
                table235.AddRow(new string[] {
                            "Merchant Commission",
                            "True",
                            "Safaricom",
                            "0.50"});
                table235.AddRow(new string[] {
                            "Merchant Commission",
                            "True",
                            "Safaricom",
                            "0.13"});
                table235.AddRow(new string[] {
                            "Merchant Commission",
                            "True",
                            "Safaricom",
                            "0.75"});
                table235.AddRow(new string[] {
                            "Merchant Commission",
                            "True",
                            "Safaricom",
                            "0.20"});
                table235.AddRow(new string[] {
                            "Merchant Commission",
                            "True",
                            "Safaricom",
                            "0.30"});
                table235.AddRow(new string[] {
                            "Merchant Commission",
                            "True",
                            "Safaricom",
                            "0.51"});
#line 150
 await testRunner.WhenAsync("I get the Estate Settlement Report for Estate \'Test Estate 1\' for Merchant \'Test " +
                        "Merchant 1\' with the Date \'2022-01-13\' the following fees are settled", ((string)(null)), table235, "When ");
#line hidden
                global::Reqnroll.Table table236 = new global::Reqnroll.Table(new string[] {
                            "FeeDescription",
                            "IsSettled",
                            "Operator",
                            "CalculatedValue"});
                table236.AddRow(new string[] {
                            "Merchant Commission",
                            "True",
                            "Safaricom",
                            "0.50"});
                table236.AddRow(new string[] {
                            "Merchant Commission",
                            "True",
                            "Safaricom",
                            "0.13"});
                table236.AddRow(new string[] {
                            "Merchant Commission",
                            "True",
                            "Safaricom",
                            "0.08"});
#line 159
 await testRunner.WhenAsync("I get the Estate Settlement Report for Estate \'Test Estate 1\' for Merchant \'Test " +
                        "Merchant 2\' with the Date \'2022-01-13\' the following fees are settled", ((string)(null)), table236, "When ");
#line hidden
                global::Reqnroll.Table table237 = new global::Reqnroll.Table(new string[] {
                            "FeeDescription",
                            "IsSettled",
                            "Operator",
                            "CalculatedValue"});
                table237.AddRow(new string[] {
                            "Merchant Commission",
                            "True",
                            "Safaricom",
                            "0.85"});
#line 165
 await testRunner.WhenAsync("I get the Estate Settlement Report for Estate \'Test Estate 2\' for Merchant \'Test " +
                        "Merchant 3\' with the Date \'2022-02-06\' the following fees are settled", ((string)(null)), table237, "When ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
    }
}
#pragma warning restore
#endregion
