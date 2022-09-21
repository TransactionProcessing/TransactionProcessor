namespace TransactionProcessor.Bootstrapper
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.OperatorInterfaces.PataPawaPostPay;
    using BusinessLogic.OperatorInterfaces.SafaricomPinless;
    using BusinessLogic.OperatorInterfaces.VoucherManagement;
    using Lamar;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Org.BouncyCastle.Security;
    using PataPawaPostPay;
    using VoucherManagement.Client;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    [ExcludeFromCodeCoverage]
    public class OperatorRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorRegistry"/> class.
        /// </summary>
        public OperatorRegistry()
        {
            IConfigurationSection section = Startup.Configuration.GetSection("AppSettings:OperatorConfiguration");

            this.ConfigureOperator<SafaricomConfiguration>("Safaricom", section);
            this.ConfigureOperator<PataPawaPostPaidConfiguration>("PataPawaPostPay", section);
            this.AddSingleton(new PataPawaPostPayServiceClient(PataPawaPostPayServiceClient.EndpointConfiguration.BasicHttpBinding_IPataPawaPostPayService));

            this.For<IOperatorProxy>().Add<SafaricomPinlessProxy>().Named("Safaricom").Singleton();
            this.For<IOperatorProxy>().Add<PataPawaPostPayProxy>().Named("PataPawaPostPay").Singleton();
            this.For<IOperatorProxy>().Add<VoucherManagementProxy>().Named("Voucher").Singleton();

            this.AddTransient<Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService>>(context => (client,
                                                                                                               url) => {
                                                                                                                  IPataPawaPostPayService channel =
                                                                                                                      client.ChannelFactory.CreateChannel(new EndpointAddress(url));
                                                                                                                  return channel;
                                                                                                              });
            

            this.AddTransient<Func<String, IOperatorProxy>>(context => operatorIdentifier => {
                                                                           //return context.GetInstance<IOperatorProxy>(operatorIdentifier);
                                                                           return Startup.Container.GetInstance<IOperatorProxy>(operatorIdentifier);
                                                                           //if (string.Compare(operatorIdentifier.Replace(" ", ""),
                                                                           //                   "Safaricom",
                                                                           //                   StringComparison.CurrentCultureIgnoreCase) == 0)
                                                                           //{
                                                                           //    SafaricomConfiguration
                                                                           //        configuration = context.GetRequiredService<SafaricomConfiguration>();
                                                                           //    HttpClient client = context.GetRequiredService<HttpClient>();
                                                                           //    return new SafaricomPinlessProxy(configuration, client);
                                                                           //}

                                                                           //if (string.Compare(operatorIdentifier.Replace(" ", ""),
                                                                           //                   "PataPawaPostPay",
                                                                           //                   StringComparison.CurrentCultureIgnoreCase) == 0)
                                                                           //{
                                                                           //    PataPawaPostPayServiceClient client = context.GetRequiredService<PataPawaPostPayServiceClient>();
                                                                           //    PataPawaPostPaidConfiguration
                                                                           //        configuration = context.GetRequiredService<PataPawaPostPaidConfiguration>();
                                                                           //    IPataPawaPostPayService channel = client.ChannelFactory.CreateChannel(new EndpointAddress(configuration.Url));
                                                                           //    IMemoryCache memoryCache = context.GetRequiredService<IMemoryCache>();
                                                                           //    return new PataPawaPostPayProxy(channel, configuration, memoryCache);
                                                                           //}

                                                                           //// Voucher
                                                                           //IVoucherManagementClient voucherManagementClient =
                                                                           //    context.GetRequiredService<IVoucherManagementClient>();
                                                                           //return new VoucherManagementProxy(voucherManagementClient);
                                                                       });

        }

        private void ConfigureOperator<T>(String operatorId, IConfigurationSection operatorConfigurationSection) where T : class {
            T configObject = (T)Activator.CreateInstance(typeof(T));

            Startup.Configuration.GetSection($"OperatorConfiguration:{operatorId}").Bind(configObject);

            this.AddSingleton(configObject);

            BaseOperatorConfiguration cfg = configObject as BaseOperatorConfiguration;

            if (cfg.ApiLogonRequired) {
                Startup.AutoApiLogonOperators.Add(operatorId);
            }
        }

        #endregion
    }
}