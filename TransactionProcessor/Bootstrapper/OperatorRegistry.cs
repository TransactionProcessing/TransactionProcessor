namespace TransactionProcessor.Bootstrapper
{
    using System;
    using System.Net.Http;
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.OperatorInterfaces.SafaricomPinless;
    using BusinessLogic.OperatorInterfaces.VoucherManagement;
    using Lamar;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using VoucherManagement.Client;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    public class OperatorRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorRegistry"/> class.
        /// </summary>
        public OperatorRegistry()
        {
            SafaricomConfiguration safaricomConfiguration = new SafaricomConfiguration();

            if (Startup.Configuration != null)
            {
                IConfigurationSection section = Startup.Configuration.GetSection("AppSettings:EventHandlerConfiguration");

                if (section != null)
                {
                    Startup.Configuration.GetSection("OperatorConfiguration:Safaricom").Bind(safaricomConfiguration);
                }
            }

            this.AddSingleton(safaricomConfiguration);

            this.AddTransient<Func<String, IOperatorProxy>>(context => operatorIdentifier =>
                                                                       {
                                                                           if (string.Compare(operatorIdentifier,
                                                                                              "Safaricom",
                                                                                              StringComparison.CurrentCultureIgnoreCase) == 0)
                                                                           {
                                                                               SafaricomConfiguration
                                                                                   configuration = context.GetRequiredService<SafaricomConfiguration>();
                                                                               HttpClient client = context.GetRequiredService<HttpClient>();
                                                                               return new SafaricomPinlessProxy(configuration, client);
                                                                           }

                                                                           // Voucher
                                                                           IVoucherManagementClient voucherManagementClient =
                                                                               context.GetRequiredService<IVoucherManagementClient>();
                                                                           return new VoucherManagementProxy(voucherManagementClient);
                                                                       });
        }

        #endregion
    }
}