using Shared.Middleware;

namespace TransactionProcessor.Bootstrapper
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.OperatorInterfaces.PataPawaPostPay;
    using BusinessLogic.OperatorInterfaces.PataPawaPrePay;
    using BusinessLogic.OperatorInterfaces.SafaricomPinless;
    using BusinessLogic.OperatorInterfaces.VoucherManagement;
    using Common;
    using Lamar;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    //using Org.BouncyCastle.Security;
    using PataPawaPostPay;

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
            this.ConfigureOperator<PataPawaPrePaidConfiguration>("PataPawaPrePay", section);
            this.AddSingleton(new PataPawaPostPayServiceClient(PataPawaPostPayServiceClient.EndpointConfiguration.BasicHttpBinding_IPataPawaPostPayService));

            this.For<IOperatorProxy>().Add<SafaricomPinlessProxy>().Named("Safaricom").Singleton();
            this.For<IOperatorProxy>().Add<PataPawaPostPayProxy>().Named("PataPawaPostPay").Singleton();
            this.For<IOperatorProxy>().Add<PataPawaPrePayProxy>().Named("PataPawaPrePay").Singleton();
            this.For<IOperatorProxy>().Add<VoucherManagementProxy>().Named("Voucher").Singleton();


            this.AddTransient<Func<PataPawaPostPayServiceClient, String, String, IPataPawaPostPayService>>(context =>
                (client, clientName,
                    url) =>
                {
                    var loggingConfig = Startup.Container.GetInstance<RequestResponseMiddlewareLoggingConfig>();
                    client.Endpoint.SetTraceLogging(clientName, loggingConfig);
                    IPataPawaPostPayService channel =
                        client.ChannelFactory.CreateChannel(new EndpointAddress(url));
                    return channel;
                });
            

            this.AddTransient<Func<String, IOperatorProxy>>(context => operatorIdentifier => {
                                                                           return Startup.Container.GetInstance<IOperatorProxy>(operatorIdentifier);
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