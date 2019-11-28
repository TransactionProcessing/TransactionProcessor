using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.Tests.Common
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Commands;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Models;
    using Moq;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.CommandHandling;
    using Xunit;

    public class TransactionProcessorWebFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Setup my mocks in here
            Mock<ICommandRouter> commandRouterMock = this.CreateCommandRouterMock();

            builder.ConfigureServices((builderContext, services) =>
            {
                if (commandRouterMock != null)
                {
                    services.AddSingleton<ICommandRouter>(commandRouterMock.Object);
                }

                services.AddMvc(options =>
                {
                    options.Filters.Add(new AllowAnonymousFilter());
                })
                        .AddApplicationPart(typeof(Startup).Assembly);
            });
            ;
        }

        private Mock<ICommandRouter> CreateCommandRouterMock()
        {
            Mock<ICommandRouter> commandRouterMock = new Mock<ICommandRouter>(MockBehavior.Strict);

            commandRouterMock.Setup(c => c.Route(It.IsAny<ProcessLogonTransactionCommand>(), It.IsAny<CancellationToken>())).Returns<ProcessLogonTransactionCommand, CancellationToken>((ProcessLogonTransactionCommand command, CancellationToken cancellationToken) =>
                                                                                                                                                                     {
                                                                                                                                                                         command.Response = new ProcessLogonTransactionResponse
                                                                                                                                                                                            {
                                                                                                                                                                                                ResponseMessage = "SUCCESS",
                                                                                                                                                                                                ResponseCode = 0
                                                                                                                                                                                            };
                                                                                                                                                                         return Task.CompletedTask;
                                                                                                                                                                     });

            return commandRouterMock;
        }

    }

    /// <summary>
    /// </summary>
    /// <seealso cref="Startup" />
    [CollectionDefinition("TestCollection")]
    public class TestCollection : ICollectionFixture<TransactionProcessorWebFactory<Startup>>
    {
        // A class with no code, only used to define the collection
    }

    public static class Helpers
    {
        #region Methods

        /// <summary>
        /// Creates the content of the string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestObject">The request object.</param>
        /// <returns></returns>
        public static StringContent CreateStringContent<T>(T requestObject)
        {
            return new StringContent(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json");
        }

        #endregion
    }
}
