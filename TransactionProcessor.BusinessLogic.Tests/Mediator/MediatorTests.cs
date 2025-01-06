using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.EventStore.EventStore;
using TransactionProcessor.ProjectionEngine.Repository;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Mediator
{
    using BusinessLogic.Services;
    using Lamar;
    using Microsoft.Extensions.DependencyInjection;
    using Testing;
    using TransactionProcessor.ProjectionEngine.State;

    public class MediatorTests
    {
        private List<IBaseRequest> Requests = new List<IBaseRequest>();

        public MediatorTests()
        {
            this.Requests.Add(TestData.ProcessLogonTransactionCommand);
            this.Requests.Add(TestData.ProcessReconciliationCommand);
            this.Requests.Add(TestData.ProcessSaleTransactionCommand);
            this.Requests.Add(TestData.ProcessSettlementCommand);
            this.Requests.Add(TestData.GetMerchantBalanceQuery);
            this.Requests.Add(TestData.GetMerchantLiveBalanceQuery);
            this.Requests.Add(TestData.GetMerchantBalanceHistoryQuery);
            this.Requests.Add(TestData.AddMerchantFeePendingSettlementCommand);
            this.Requests.Add(TestData.AddSettledFeeToSettlementCommand);
            this.Requests.Add(TestData.GetPendingSettlementQuery);
            this.Requests.Add(TestData.RecordCreditPurchaseCommand);
            this.Requests.Add(TestData.CalculateFeesForTransactionCommand);
            this.Requests.Add(TestData.AddSettledMerchantFeeCommand);
            this.Requests.Add(TestData.RecordTransactionCommand);
            // TODO: this needs the query handling function refactoring to use a repository not the context direct
            //this.Requests.Add(TestData.GetVoucherByVoucherCodeQuery);
            //this.Requests.Add(TestData.GetVoucherByTransactionIdQuery);


        }

        [Fact]
        public async Task Mediator_Send_RequestHandled()
        {
            Mock<IWebHostEnvironment> hostingEnvironment = new Mock<IWebHostEnvironment>();
            hostingEnvironment.Setup(he => he.EnvironmentName).Returns("Development");
            hostingEnvironment.Setup(he => he.ContentRootPath).Returns("/home");
            hostingEnvironment.Setup(he => he.ApplicationName).Returns("Test Application");

            ServiceRegistry services = new ServiceRegistry();
            Startup s = new Startup(hostingEnvironment.Object);
            Startup.Configuration = this.SetupMemoryConfiguration();

            this.AddTestRegistrations(services, hostingEnvironment.Object);
            s.ConfigureContainer(services);
            Startup.Container.AssertConfigurationIsValid(AssertMode.Full);

            List<String> errors = new List<String>();
            IMediator mediator = Startup.Container.GetService<IMediator>();
            foreach (IBaseRequest baseRequest in this.Requests)
            {
                try
                {
                    await mediator.Send(baseRequest);
                }
                catch (Exception ex)
                {
                    errors.Add($"Command: {baseRequest.GetType().Name} Exception: {ex.Message}");
                }
            }

            if (errors.Any() == true)
            {
                String errorMessage = String.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
        }

        private IConfigurationRoot SetupMemoryConfiguration()
        {
            Dictionary<String, String> configuration = new Dictionary<String, String>();
            
            IConfigurationBuilder builder = new ConfigurationBuilder();
            
            builder.AddInMemoryCollection(TestData.DefaultAppSettings);

            return builder.Build();
        }

        private void AddTestRegistrations(ServiceRegistry services,
                                          IWebHostEnvironment hostingEnvironment)
        {
            services.AddLogging();
            DiagnosticListener diagnosticSource = new DiagnosticListener(hostingEnvironment.ApplicationName);
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddSingleton<DiagnosticListener>(diagnosticSource);
            services.AddSingleton<IWebHostEnvironment>(hostingEnvironment);
            services.AddSingleton<IHostEnvironment>(hostingEnvironment);
            services.AddSingleton<IConfiguration>(Startup.Configuration);

            services.OverrideServices(s => {
                                          s.AddSingleton<ISettlementDomainService, DummySettlementDomainService>();
                                          s.AddSingleton<IVoucherDomainService, DummyVoucherDomainService>();
                                          s.AddSingleton<ITransactionDomainService, DummyTransactionDomainService>();
                                          s.AddSingleton<IProjectionStateRepository<MerchantBalanceState>, DummyMerchantBalanceStateRepository>();
                                          s.AddSingleton<ITransactionProcessorReadRepository, DummyTransactionProcessorReadRepository>();
                                          s.AddSingleton<IEventStoreContext, DummyEventStoreContext>();
            });
        }
    }
}
