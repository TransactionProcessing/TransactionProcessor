namespace TransactionProcessor.Bootstrapper
{
    using ClientProxyBase;
    using Lamar;
    using MessagingService.Client;
    using Microsoft.Extensions.DependencyInjection;
    using SecurityService.Client;
    using Shared.General;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    [ExcludeFromCodeCoverage]
    public class ClientRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRegistry"/> class.
        /// </summary>
        public ClientRegistry()
        {
            this.AddHttpContextAccessor();
            this.RegisterHttpClient<ISecurityServiceClient, SecurityServiceClient>();
            this.RegisterHttpClient<IMessagingServiceClient, MessagingServiceClient>();

            this.AddSingleton<Func<String, String>>(serviceName => ConfigurationReader.GetBaseServerUri(serviceName).OriginalString);
        }

        #endregion
    }
}
