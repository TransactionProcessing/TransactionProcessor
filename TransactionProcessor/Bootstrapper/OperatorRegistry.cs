using Shared.Middleware;
using TransactionProcessor.BusinessLogic.OperatorInterfaces.AgencyBanking;

namespace TransactionProcessor.Bootstrapper
{
    using System;
    using System.Collections.Generic;
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
        public OperatorRegistry(IConfiguration configuration)
        {
            List<String> autoLogonOperators = new();
            this.ConfigureOperator<SafaricomConfiguration>(configuration, autoLogonOperators, "Safaricom");
            this.ConfigureOperator<PataPawaPostPaidConfiguration>(configuration, autoLogonOperators, "PataPawaPostPay");
            this.ConfigureOperator<PataPawaPrePaidConfiguration>(configuration, autoLogonOperators, "PataPawaPrePay");
            this.ConfigureOperator<AgencyBankingConfiguration>(configuration, autoLogonOperators, "AgencyBanking");
            this.AddSingleton(new AutoLogonOperatorConfiguration(autoLogonOperators));
            this.AddSingleton(new PataPawaPostPayServiceClient(PataPawaPostPayServiceClient.EndpointConfiguration.BasicHttpBinding_IPataPawaPostPayService));

            this.For<IOperatorProxy>().Add<SafaricomPinlessProxy>().Named("Safaricom").Singleton();
            this.For<IOperatorProxy>().Add<PataPawaPostPayProxy>().Named("PataPawaPostPay").Singleton();
            this.For<IOperatorProxy>().Add<PataPawaPrePayProxy>().Named("PataPawaPrePay").Singleton();
            this.For<IOperatorProxy>().Add<VoucherManagementProxy>().Named("Voucher").Singleton();
            this.For<IOperatorProxy>().Add<AgencyBankingProxy>().Named("AgencyBanking").Singleton();


            this.AddTransient<Func<PataPawaPostPayServiceClient, String, String, IPataPawaPostPayService>>(context =>
                (client, clientName,
                    url) =>
                {
                    var loggingConfig = context.GetService<IContainer>().GetInstance<RequestResponseMiddlewareLoggingConfig>();
                    client.Endpoint.SetTraceLogging(clientName, loggingConfig);
                    IPataPawaPostPayService channel =
                        client.ChannelFactory.CreateChannel(new EndpointAddress(url));
                    return channel;
                });
            

            this.AddTransient<Func<String, IOperatorProxy>>(context => operatorIdentifier => {
                                                                           return context.GetService<IContainer>().GetInstance<IOperatorProxy>(operatorIdentifier);
                                                                       });
        }

        private void ConfigureOperator<T>(IConfiguration configuration, List<String> autoLogonOperators, String operatorId) where T : class {
            T configObject = (T)Activator.CreateInstance(typeof(T));

            configuration.GetSection($"OperatorConfiguration:{operatorId}").Bind(configObject);

            this.AddSingleton(configObject);

            BaseOperatorConfiguration cfg = configObject as BaseOperatorConfiguration;

            if (cfg.ApiLogonRequired) {
                autoLogonOperators.Add(operatorId);
            }
        }

        #endregion
    }
}
