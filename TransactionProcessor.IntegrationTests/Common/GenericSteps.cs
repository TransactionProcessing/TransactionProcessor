using Ductus.FluentDocker.Commands;
using SimpleResults;
using System;
using Ductus.FluentDocker.Model;
using Ductus.FluentDocker.Model.Containers;
using Newtonsoft.Json;

namespace TransactionProcessor.IntegrationTests.Common
{
    using Ductus.FluentDocker.Services;
    using global::Shared.IntegrationTesting;
    using global::Shared.Logger;
    using Microsoft.Extensions.Hosting;
    using NLog;
    using Reqnroll;
    using System.Threading.Tasks;

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

        public static String GetDockerEnginePlatform(NlogLogger logger) {
            try {


                IHostService docker = BaseDockerHelper.GetDockerHost();

                if (docker == null) {
                    logger.LogInformation("Docker Host is null");
                    return "Unknown";
                }
                if (docker.Host == null) {
                    logger.LogInformation("Docker Host Host is null");
                    return "Unknown";
                }

                CommandResponse<DockerInfoBase> version = docker.Host.Version(null);

                if (version == null) {
                    logger.LogInformation("Docker Host Version is null");
                    return "Unknown";
                }

                var x = JsonConvert.SerializeObject(version);
                logger.LogInformation(x);

                return version.Data.ServerOs.ToLower();
            }
            catch (Exception ex) {
                logger.LogInformation(ex.Message);
                return $"Unknown";
            }
        }

        [BeforeScenario]
        public async Task StartSystem()
        {
            // Initialise a logger
            String scenarioName = this.ScenarioContext.ScenarioInfo.Title.Replace(" ", "");
            NlogLogger logger = new NlogLogger();
            logger.Initialise(LogManager.GetLogger(scenarioName), scenarioName);
            LogManager.AddHiddenAssembly(typeof(NlogLogger).Assembly);

            DockerServices dockerServices = DockerServices.CallbackHandler | DockerServices.EventStore |
                                            DockerServices.FileProcessor | DockerServices.MessagingService | DockerServices.SecurityService |
                                            DockerServices.SqlServer | DockerServices.TestHost | DockerServices.TransactionProcessor |
                                            DockerServices.TransactionProcessorAcl;

            
            this.TestingContext.DockerHelper = new DockerHelper();
            this.TestingContext.DockerHelper.Logger = logger;
            this.TestingContext.Logger = logger;
            this.TestingContext.DockerHelper.RequiredDockerServices = dockerServices;
            this.TestingContext.Logger.LogInformation($"About to Start Global Setup [{GetDockerEnginePlatform(this.TestingContext.Logger)}]");

            this.TestingContext.DockerHelper.SetImageDetails(ContainerType.SqlServer, ("mcr.microsoft.com/mssql/server:2019-latest", true));

            await Setup.GlobalSetup(this.TestingContext.DockerHelper);

            this.TestingContext.DockerHelper.SqlServerContainer = Setup.DatabaseServerContainer;
            this.TestingContext.DockerHelper.SqlServerNetwork = Setup.DatabaseServerNetwork;
            this.TestingContext.DockerHelper.DockerCredentials = Setup.DockerCredentials;
            this.TestingContext.DockerHelper.SqlCredentials = Setup.SqlCredentials;
            this.TestingContext.DockerHelper.SqlServerContainerName = "sharedsqlserver";

            this.TestingContext.DockerHelper.SetImageDetails(ContainerType.TransactionProcessor, ("transactionprocessor", false));

            this.TestingContext.Logger = logger;
            this.TestingContext.Logger.LogInformation("About to Start Containers for Scenario Run");
            await this.TestingContext.DockerHelper.StartContainersForScenarioRun(scenarioName, dockerServices).ConfigureAwait(false);
            this.TestingContext.Logger.LogInformation("Containers for Scenario Run Started");
        }

        [AfterScenario]
        public async Task StopSystem(){

            DockerServices sharedDockerServices = DockerServices.SqlServer;

            this.TestingContext.Logger.LogInformation("About to Stop Containers for Scenario Run");
            await this.TestingContext.DockerHelper.StopContainersForScenarioRun(sharedDockerServices).ConfigureAwait(false);
            this.TestingContext.Logger.LogInformation("Containers for Scenario Run Stopped");
        }
    }
}
