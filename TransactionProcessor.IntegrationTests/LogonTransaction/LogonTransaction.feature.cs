// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:3.1.0.0
//      SpecFlow Generator Version:3.1.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace TransactionProcessor.IntegrationTests.LogonTransaction
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "base")]
    [Xunit.TraitAttribute("Category", "shared")]
    public partial class LogonTransactionFeature : Xunit.IClassFixture<LogonTransactionFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = new string[] {
                "base",
                "shared"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "LogonTransaction.feature"
#line hidden
        
        public LogonTransactionFeature(LogonTransactionFeature.FixtureData fixtureData, InternalSpecFlow.XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "LogonTransaction", null, ProgrammingLanguage.CSharp, new string[] {
                        "base",
                        "shared"});
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 4
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName"});
            table1.AddRow(new string[] {
                        "Test Estate 1"});
            table1.AddRow(new string[] {
                        "Test Estate 2"});
#line 5
 testRunner.Given("I have created the following estates", ((string)(null)), table1, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "MerchantName",
                        "AddressLine1",
                        "Town",
                        "Region",
                        "Country",
                        "ContactName",
                        "EmailAddress",
                        "EstateName"});
            table2.AddRow(new string[] {
                        "Test Merchant 1",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 1",
                        "testcontact1@merchant1.co.uk",
                        "Test Estate 1"});
            table2.AddRow(new string[] {
                        "Test Merchant 2",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 2",
                        "testcontact2@merchant2.co.uk",
                        "Test Estate 1"});
            table2.AddRow(new string[] {
                        "Test Merchant 3",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 3",
                        "testcontact3@merchant2.co.uk",
                        "Test Estate 2"});
#line 10
 testRunner.Given("I create the following merchants", ((string)(null)), table2, "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Logon Transaction")]
        [Xunit.TraitAttribute("FeatureTitle", "LogonTransaction")]
        [Xunit.TraitAttribute("Description", "Logon Transaction")]
        [Xunit.TraitAttribute("Category", "PRTest")]
        public virtual void LogonTransaction()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Logon Transaction", null, new string[] {
                        "PRTest"});
#line 17
this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                            "DateTime",
                            "TransactionNumber",
                            "TransactionType",
                            "MerchantName",
                            "IMEINumber"});
                table3.AddRow(new string[] {
                            "Today",
                            "1",
                            "Logon",
                            "Test Merchant 1",
                            "123456789"});
                table3.AddRow(new string[] {
                            "Today",
                            "2",
                            "Logon",
                            "Test Merchant 2",
                            "123456789"});
                table3.AddRow(new string[] {
                            "Today",
                            "3",
                            "Logon",
                            "Test Merchant 3",
                            "123456789"});
#line 18
 testRunner.When("I perform the following transactions", ((string)(null)), table3, "When ");
#line hidden
                TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                            "TransactionNumber",
                            "ResponseCode",
                            "ResponseMessage"});
                table4.AddRow(new string[] {
                            "1",
                            "0000",
                            "SUCCESS"});
                table4.AddRow(new string[] {
                            "2",
                            "0000",
                            "SUCCESS"});
                table4.AddRow(new string[] {
                            "3",
                            "0000",
                            "SUCCESS"});
#line 24
 testRunner.Then("transaction response should contain the following information", ((string)(null)), table4, "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.1.0.0")]
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
