using System;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace TransactionProcessor.IntegrationTests.Common
{
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using global::Shared.IntegrationTesting;
    using Reqnroll;
    using Shouldly;
    using System.Threading.Tasks;

    [Binding]
    public class Setup
    {
        public static IContainer DatabaseServerContainer;
        public static INetwork DatabaseServerNetwork;
        public static (String usename, String password) SqlCredentials = ("sa", "thisisalongpassword123!");
        public static (String url, String username, String password) DockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

        static object padLock = new object(); // Object to lock on

        public static async Task GlobalSetup(DockerHelper dockerHelper)
        {
            ShouldlyConfiguration.DefaultTaskTimeout = TimeSpan.FromMinutes(1);
            dockerHelper.SqlCredentials = Setup.SqlCredentials;
            dockerHelper.DockerCredentials = Setup.DockerCredentials;
            dockerHelper.SqlServerContainerName = "sharedsqlserver";
            dockerHelper.SetImageDetails(ContainerType.SqlServer, ("mcr.microsoft.com/mssql/server:2022-latest", true));
            lock (Setup.padLock)
            {
                Setup.DatabaseServerNetwork = dockerHelper.SetupTestNetwork("sharednetwork").Result;

                dockerHelper.Logger.LogInformation("in start SetupSqlServerContainer");
                Setup.DatabaseServerContainer = dockerHelper.SetupSqlServerContainer(Setup.DatabaseServerNetwork).Result;
            }
        }

        public static String GetConnectionString(String databaseName)
        {
            return $"server={Setup.DatabaseServerContainer.Name};database={databaseName};user id={Setup.SqlCredentials.usename};password={Setup.SqlCredentials.password}";
        }

        public static String GetLocalConnectionString(String databaseName)
        {
            Int32 databaseHostPort = Setup.DatabaseServerContainer.GetMappedPublicPort(1433);

            return $"server=localhost,{databaseHostPort};database={databaseName};user id={Setup.SqlCredentials.usename};password={Setup.SqlCredentials.password}";
        }

    }
}
