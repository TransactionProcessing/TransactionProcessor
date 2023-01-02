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
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Mediator
{
    using System.Threading;
    using BusinessLogic.Services;
    using Lamar;
    using Microsoft.Extensions.DependencyInjection;
    using Models;
    using Testing;

    public class MediatorTests
    {
        private List<IBaseRequest> Requests = new List<IBaseRequest>();

        public MediatorTests()
        {
            this.Requests.Add(TestData.ProcessLogonTransactionRequest);
            this.Requests.Add(TestData.ProcessReconciliationRequest);
            this.Requests.Add(TestData.ProcessSaleTransactionRequest);
            this.Requests.Add(TestData.ProcessSettlementRequest);
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
                    errors.Add(ex.Message);
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
            });
        }
    }

    public class DummySettlementDomainService : ISettlementDomainService
    {
        public async Task<ProcessSettlementResponse> ProcessSettlement(DateTime pendingSettlementDate,
                                                                       Guid estateId,
                                                                       CancellationToken cancellationToken) {
            return new ProcessSettlementResponse();
        }
    }

    public class DummyVoucherDomainService : IVoucherDomainService
    {
        public async Task<IssueVoucherResponse> IssueVoucher(Guid voucherId,
                                                             String operatorId,
                                                             Guid estateId,
                                                             Guid transactionId,
                                                             DateTime issuedDateTime,
                                                             Decimal value,
                                                             String recipientEmail,
                                                             String recipientMobile,
                                                             CancellationToken cancellationToken) {
            return new IssueVoucherResponse();
        }

        public async Task<RedeemVoucherResponse> RedeemVoucher(Guid estateId,
                                                               String voucherCode,
                                                               DateTime redeemedDateTime,
                                                               CancellationToken cancellationToken) {
            return new RedeemVoucherResponse();
        }
    }

    public class DummyTransactionDomainService : ITransactionDomainService
    {
        public async Task<ProcessLogonTransactionResponse> ProcessLogonTransaction(Guid transactionId,
                                                                                   Guid estateId,
                                                                                   Guid merchantId,
                                                                                   DateTime transactionDateTime,
                                                                                   String transactionNumber,
                                                                                   String deviceIdentifier,
                                                                                   CancellationToken cancellationToken) {
            return new ProcessLogonTransactionResponse();
        }

        public async Task<ProcessSaleTransactionResponse> ProcessSaleTransaction(Guid transactionId,
                                                                                 Guid estateId,
                                                                                 Guid merchantId,
                                                                                 DateTime transactionDateTime,
                                                                                 String transactionNumber,
                                                                                 String deviceIdentifier,
                                                                                 String operatorId,
                                                                                 String customerEmailAddress,
                                                                                 Dictionary<String, String> additionalTransactionMetadata,
                                                                                 Guid contractId,
                                                                                 Guid productId,
                                                                                 Int32 transactionSource,
                                                                                 CancellationToken cancellationToken) {
            return new ProcessSaleTransactionResponse();
        }

        public async Task<ProcessReconciliationTransactionResponse> ProcessReconciliationTransaction(Guid transactionId,
                                                                                                     Guid estateId,
                                                                                                     Guid merchantId,
                                                                                                     String deviceIdentifier,
                                                                                                     DateTime transactionDateTime,
                                                                                                     Int32 transactionCount,
                                                                                                     Decimal transactionValue,
                                                                                                     CancellationToken cancellationToken) {
            return new ProcessReconciliationTransactionResponse();
        }

        public async Task ResendTransactionReceipt(Guid transactionId,
                                                   Guid estateId,
                                                   CancellationToken cancellationToken) {
            
        }
    }
}
