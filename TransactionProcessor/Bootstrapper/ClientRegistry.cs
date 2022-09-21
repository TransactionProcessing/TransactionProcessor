namespace TransactionProcessor.Bootstrapper
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using EstateManagement.Client;
    using Lamar;
    using MessagingService.Client;
    using Microsoft.Extensions.DependencyInjection;
    using SecurityService.Client;
    using Shared.General;
    using VoucherManagement.Client;

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
            this.AddSingleton<ISecurityServiceClient, SecurityServiceClient>();
            this.AddSingleton<IMessagingServiceClient, MessagingServiceClient>();
            this.AddSingleton<IEstateClient, EstateClient>();
            this.AddSingleton<IVoucherManagementClient, VoucherManagementClient>();

            this.AddSingleton<Func<String, String>>(container => serviceName => { return ConfigurationReader.GetBaseServerUri(serviceName).OriginalString; });

            var httpMessageHandler = new SocketsHttpHandler
                                     {
                                         SslOptions =
                                         {
                                             RemoteCertificateValidationCallback = (sender,
                                                                                    certificate,
                                                                                    chain,
                                                                                    errors) => true,
                                         }
                                     };
            HttpClient httpClient = new HttpClient(httpMessageHandler);
            this.AddSingleton(httpClient);
        }

        #endregion
    }
}