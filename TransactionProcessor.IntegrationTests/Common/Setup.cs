using System;

namespace TransactionProcessor.IntegrationTests.Common
{
    using System.Threading.Tasks;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using Reqnroll;
    using Shouldly;

    [Binding]
    public class Setup
    {
        public static async Task GlobalSetup(DockerHelper dockerHelper)
        {
            ShouldlyConfiguration.DefaultTaskTimeout = TimeSpan.FromMinutes(1);
        }
    }
}
