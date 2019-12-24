using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.IntegrationTests.Common
{
    using System.Threading;
    using System.Threading.Tasks;
    using Ductus.FluentDocker.Executors;
    using Ductus.FluentDocker.Extensions;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using global::Shared.Logger;
    using NLog;
    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Plugins;

    [Binding]
    [Scope(Tag = "base")]
    public class GenericSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        public GenericSteps(ScenarioContext scenarioContext,
                            TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
        }
        
        [BeforeScenario]
        public async Task StartSystem()
        {
            // Initialise a logger
            String scenarioName = this.ScenarioContext.ScenarioInfo.Title.Replace(" ", "");
            NlogLogger logger = new NlogLogger();
            logger.Initialise(LogManager.GetLogger(scenarioName), scenarioName);
            LogManager.AddHiddenAssembly(typeof(NlogLogger).Assembly);

            this.TestingContext.DockerHelper = new DockerHelper(logger);
            this.TestingContext.Logger = logger;
            this.TestingContext.Logger.LogInformation("About to Start Containers for Scenario Run");
            await this.TestingContext.DockerHelper.StartContainersForScenarioRun(scenarioName).ConfigureAwait(false);
            this.TestingContext.Logger.LogInformation("Containers for Scenario Run Started");

            Thread.Sleep(20000);
        }

        [AfterScenario]
        public async Task StopSystem()
        {
            if (this.ScenarioContext.TestError != null)
            {
                //Exception currentEx = this.ScenarioContext.TestError;
                //Console.Out.WriteLine(currentEx.Message);
                //while (currentEx.InnerException != null)
                //{
                //    currentEx = currentEx.InnerException;
                //    Console.Out.WriteLine(currentEx.Message);
                //}

                //// The test has failed, grab the logs from all the containers
                //List<IContainerService> containers = new List<IContainerService>();
                //containers.Add(this.TestingContext.DockerHelper.EstateManagementContainer);
                //containers.Add(this.TestingContext.DockerHelper.TransactionProcessorContainer);

                //foreach (IContainerService containerService in containers)
                //{
                //    ConsoleStream<String> logStream = containerService.Logs();
                //    IList<String> logData = logStream.ReadToEnd();

                //    foreach (String s in logData)
                //    {
                //        Console.Out.WriteLine(s);
                //    }
                //}
            }

            await this.TestingContext.DockerHelper.StopContainersForScenarioRun().ConfigureAwait(false);
        }
    }
}
