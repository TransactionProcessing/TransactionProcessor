using System;
using System.Text;
using Shared.Serialisation;

namespace TransactionProcessor.Tests.Common
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Imposter.Abstractions;
    using Xunit;

    public class TransactionProcessorWebFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Setup my mocks in here
            IMediatorImposter mediatorMock = this.CreateMediatorMock();

            builder.ConfigureServices((builderContext, services) =>
            {
                if (mediatorMock != null)
                {
                    services.AddSingleton<IMediator>(mediatorMock.Instance());
                }

                services.AddMvc(options =>
                {
                    options.Filters.Add(new AllowAnonymousFilter());
                })
                        .AddApplicationPart(typeof(Program).Assembly);
            });
            ;
        }

        private IMediatorImposter CreateMediatorMock()
        {
            IMediatorImposter mediatorMock = new();

            mediatorMock.Send<String>(Arg<IRequest<String>>.Any(), Arg<CancellationToken>.Any()).Returns(Task.FromResult("Hello"));

            return mediatorMock;
        }

    }

    /// <summary>
    /// </summary>
    /// <seealso cref="Program" />
    [CollectionDefinition("TestCollection")]
    public class TestCollection : ICollectionFixture<TransactionProcessorWebFactory<Program>>
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
            return new StringContent(StringSerialiser.Serialise(requestObject), Encoding.UTF8, "application/json");
        }

        #endregion
    }
}
