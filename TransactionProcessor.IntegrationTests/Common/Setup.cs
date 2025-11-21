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
        public static (String usename, String password) SqlCredentials = ("sa", "thisisalongpassword123!");
        public static (String url, String username, String password) DockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

        
        public static async Task GlobalSetup(DockerHelper dockerHelper)
        {
            ShouldlyConfiguration.DefaultTaskTimeout = TimeSpan.FromMinutes(1);
        }

        //public static String GetConnectionString(String databaseName)
        //{
        //    return $"server={Setup.DatabaseServerContainer.Name};database={databaseName};user id={Setup.SqlCredentials.usename};password={Setup.SqlCredentials.password}";
        //}

        //public static String GetLocalConnectionString(String databaseName)
        //{
        //    Int32 databaseHostPort = Setup.DatabaseServerContainer.GetMappedPublicPort(1433);

        //    return $"server=localhost,{databaseHostPort};database={databaseName};user id={Setup.SqlCredentials.usename};password={Setup.SqlCredentials.password}";
        //}

    }
}
