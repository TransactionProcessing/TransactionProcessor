using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.IntegrationTests.Common
{
    using System.Collections.Immutable;
    using System.Data;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using Microsoft.Data.SqlClient;
    using Shouldly;
    using TechTalk.SpecFlow;

    [Binding]
    public class Setup
    {
        public static IContainerService DatabaseServerContainer;
        private static String DbConnectionStringWithNoDatabase;
        public static INetworkService DatabaseServerNetwork;

        [BeforeTestRun]
        protected static void GlobalSetup()
        {
            (String, String, String) dockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

            // Setup a network for the DB Server
            DatabaseServerNetwork = global::Shared.IntegrationTesting.DockerHelper.SetupTestNetwork("sharednetwork", true);

            // Start the Database Server here
            DbConnectionStringWithNoDatabase = global::Shared.IntegrationTesting.DockerHelper.StartSqlContainerWithOpenConnection("shareddatabasesqlserver",
                                                                                                                                  null,
                                                                                                                                  "stuartferguson/subscriptionservicedatabasesqlserver",
                                                                                                                                  Setup.DatabaseServerNetwork,
                                                                                                                                  "",
                                                                                                                                  dockerCredentials);
        }

        public static String GetConnectionString(String databaseName)
        {
            return $"{DbConnectionStringWithNoDatabase} database={databaseName};";
        }
    }
}
