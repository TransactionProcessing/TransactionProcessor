using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.IntegrationTests.Common
{
    using System.Threading.Tasks;
    using Ductus.FluentDocker.Executors;
    using Ductus.FluentDocker.Extensions;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using TechTalk.SpecFlow;

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
            String scenarioName = this.ScenarioContext.ScenarioInfo.Title.Replace(" ", "");
            this.TestingContext.DockerHelper = new DockerHelper();
            await this.TestingContext.DockerHelper.StartContainersForScenarioRun(scenarioName).ConfigureAwait(false);
        }

        [AfterScenario]
        public async Task StopSystem()
        {
            Console.Out.WriteLine("In After Scenario");
            Console.Out.WriteLine(this.ScenarioContext.TestError != null);

            if (this.ScenarioContext.TestError != null)
            {
                Exception currentEx = this.ScenarioContext.TestError;
                Console.Out.WriteLine(currentEx.Message);
                while (currentEx.InnerException != null)
                {
                    currentEx = currentEx.InnerException;
                    Console.Out.WriteLine(currentEx.Message);
                }

                // The test has failed, grab the logs from all the containers
                List<IContainerService> containers = new List<IContainerService>();
                containers.Add(this.TestingContext.DockerHelper.EstateManagementContainer);
                containers.Add(this.TestingContext.DockerHelper.TransactionProcessorContainer);

                foreach (IContainerService containerService in containers)
                {
                    ConsoleStream<String> logStream = containerService.Logs();
                    IList<String> logData = logStream.ReadToEnd();

                    foreach (String s in logData)
                    {
                        Console.Out.WriteLine(s);
                    }
                }
            }
            else
            {
                Console.Out.WriteLine("No Error :|");
            }

            //await this.TestingContext.DockerHelper.StopContainersForScenarioRun().ConfigureAwait(false);
        }
    }
}
