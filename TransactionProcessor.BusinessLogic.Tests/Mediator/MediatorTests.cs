using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using Shared.EventStore.EventStore;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
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
            this.Requests.Add(TestData.Commands.ProcessLogonTransactionCommand);
            this.Requests.Add(TestData.Commands.ProcessReconciliationCommand);
            this.Requests.Add(TestData.Commands.ProcessSaleTransactionCommand);
            this.Requests.Add(TestData.Commands.ProcessSettlementCommand);
            this.Requests.Add(TestData.Queries.GetMerchantBalanceQuery);
            this.Requests.Add(TestData.Queries.GetMerchantLiveBalanceQuery);
            this.Requests.Add(TestData.Queries.GetMerchantBalanceHistoryQuery);
            this.Requests.Add(TestData.Commands.AddMerchantFeePendingSettlementCommand);
            this.Requests.Add(TestData.Commands.AddSettledFeeToSettlementCommand);
            this.Requests.Add(TestData.Commands.RecordCreditPurchaseCommand);
            this.Requests.Add(TestData.Commands.CalculateFeesForTransactionCommand);
            this.Requests.Add(TestData.Commands.AddSettledMerchantFeeCommand);
            this.Requests.Add(TestData.Commands.RecordTransactionCommand);
            this.Requests.Add(TestData.Commands.SendCustomerEmailReceiptCommand);
            this.Requests.Add(TestData.Commands.ResendTransactionReceiptCommand);
            // TODO: this needs the query handling function refactoring to use a repository not the context direct
            //this.Requests.Add(TestData.GetVoucherByVoucherCodeQuery);
            //this.Requests.Add(TestData.GetVoucherByTransactionIdQuery);

            // Estate Commands and Queries
            this.Requests.Add(TestData.Commands.CreateEstateCommand);
            this.Requests.Add(TestData.Commands.CreateEstateUserCommand);
            this.Requests.Add(TestData.Commands.AddOperatorToEstateCommand);
            this.Requests.Add(TestData.Commands.RemoveOperatorFromEstateCommand);
            this.Requests.Add(TestData.Queries.GetEstateQuery);
            this.Requests.Add(TestData.Queries.GetEstatesQuery);

            // Operator Commands and Queries
            this.Requests.Add(TestData.Commands.CreateOperatorCommand);
            this.Requests.Add(TestData.Commands.UpdateOperatorCommand);
            this.Requests.Add(TestData.Queries.GetOperatorQuery);
            this.Requests.Add(TestData.Queries.GetOperatorsQuery);

            // Contract Commands and Queries
            this.Requests.Add(TestData.Commands.CreateContractCommand);
            this.Requests.Add(TestData.Commands.AddProductToContractCommand_VariableValue);
            this.Requests.Add(TestData.Commands.AddProductToContractCommand_FixedValue);
            this.Requests.Add(TestData.Commands.AddTransactionFeeForProductToContractCommand(CalculationType.Fixed, FeeType.Merchant));
            this.Requests.Add(TestData.Commands.DisableTransactionFeeForProductCommand);
            this.Requests.Add(TestData.Queries.GetContractQuery);
            this.Requests.Add(TestData.Queries.GetContractsQuery);

            // Merchant Commands and Queries
            this.Requests.Add(TestData.Commands.AddMerchantDeviceCommand);
            this.Requests.Add(TestData.Commands.CreateMerchantCommand);
            this.Requests.Add(TestData.Commands.AssignOperatorToMerchantCommand);
            this.Requests.Add(TestData.Commands.AddMerchantContractCommand);
            this.Requests.Add(TestData.Commands.CreateMerchantUserCommand);
            this.Requests.Add(TestData.Commands.MakeMerchantDepositCommand);
            this.Requests.Add(TestData.Commands.MakeMerchantWithdrawalCommand);
            this.Requests.Add(TestData.Commands.SwapMerchantDeviceCommand);
            this.Requests.Add(TestData.Commands.UpdateMerchantCommand);
            this.Requests.Add(TestData.Commands.AddMerchantAddressCommand);
            this.Requests.Add(TestData.Commands.UpdateMerchantAddressCommand);
            this.Requests.Add(TestData.Commands.AddMerchantContactCommand);
            this.Requests.Add(TestData.Commands.UpdateMerchantContactCommand);
            this.Requests.Add(TestData.Commands.RemoveOperatorFromMerchantCommand);
            this.Requests.Add(TestData.Commands.RemoveMerchantContractCommand);
            this.Requests.Add(TestData.Queries.GetMerchantsQuery);
            this.Requests.Add(TestData.Queries.GetMerchantQuery);
            this.Requests.Add(TestData.Queries.GetMerchantContractsQuery);
            this.Requests.Add(TestData.Queries.GetTransactionFeesForProductQuery);

            // Merchant Statement Commands
            this.Requests.Add(TestData.Commands.GenerateMerchantStatementCommand);
            this.Requests.Add(TestData.Commands.AddTransactionToMerchantStatementCommand);
            //this.Requests.Add(TestData.Commands.EmailMerchantStatementCommand);
            this.Requests.Add(TestData.Commands.AddSettledFeeToMerchantStatementCommand);
            this.Requests.Add(TestData.Commands.RecordActivityDateOnMerchantStatementCommand);

            // Settlement Commands and Queries
            this.Requests.Add(TestData.Queries.GetSettlementQuery);
            this.Requests.Add(TestData.Queries.GetSettlementsQuery);
            this.Requests.Add(TestData.Queries.GetPendingSettlementQuery);

            // Voucher Queries
            this.Requests.Add(TestData.Queries.GetVoucherByVoucherCodeQuery);
            this.Requests.Add(TestData.Queries.GetVoucherByTransactionIdQuery);

            // Merchant Balance Commands
            this.Requests.Add(TestData.Commands.RecordDepositCommand);
            this.Requests.Add(TestData.Commands.RecordWithdrawalCommand);
            this.Requests.Add(TestData.Commands.RecordAuthorisedSaleCommand);
            this.Requests.Add(TestData.Commands.RecordDeclinedSaleCommand);
            this.Requests.Add(TestData.Commands.RecordSettledFeeCommand);
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
                                          s.AddSingleton<IEstateDomainService, DummyEstateDomainService>();
                                          s.AddSingleton<ITransactionProcessorManager, DummyTransactionProcessorManager>();
                                          s.AddSingleton<IOperatorDomainService, DummyOperatorDomainService>();
                                          s.AddSingleton<IVoucherManagementManager, DummyVoucherManagementManager>();
                s.AddSingleton<IEventStoreContext, DummyEventStoreContext>();
            });
        }
    }
}
