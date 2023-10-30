namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Manager;
    using BusinessLogic.Services;
    using Common;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects;
    using EstateManagement.DataTransferObjects.Responses;
    using EventHandling;
    using FloatAggregate;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Microsoft.Extensions.Configuration;
    using Models;
    using Moq;
    using SecurityService.Client;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventStore;
    using Shared.Exceptions;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using TransactionAggregate;
    using Xunit;

    public class TransactionDomainEventHandlerTests
    {
        private Mock<IAggregateRepository<SettlementAggregate, DomainEvent>> SettlementAggregateRepository;
        private Mock<IAggregateRepository<TransactionAggregate, DomainEvent>> TransactionAggregateRepository;

        private Mock<IFeeCalculationManager> FeeCalculationManager;

        private Mock<IEstateClient> EstateClient;

        private Mock<ISecurityServiceClient> SecurityServiceClient;

        private Mock<ITransactionReceiptBuilder> TransactionReceiptBuilder;

        private Mock<IMessagingServiceClient> MessagingServiceClient;

        private Mock<IAggregateRepository<FloatAggregate, DomainEvent>> FloatAggregateRepository;

        private TransactionDomainEventHandler TransactionDomainEventHandler;

        public TransactionDomainEventHandlerTests()
        {
            this.SettlementAggregateRepository = new Mock<IAggregateRepository<SettlementAggregate, DomainEvent>>();
            this.TransactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEvent>>();
            this.FloatAggregateRepository = new Mock<IAggregateRepository<FloatAggregate, DomainEvent>>();
            this.FeeCalculationManager = new Mock<IFeeCalculationManager>();
            this.EstateClient = new Mock<IEstateClient>();
            this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
            this.TransactionReceiptBuilder = new Mock<ITransactionReceiptBuilder>();
            this.MessagingServiceClient = new Mock<IMessagingServiceClient>();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            this.TransactionDomainEventHandler = new TransactionDomainEventHandler(this.TransactionAggregateRepository.Object,
                                                                                   this.FeeCalculationManager.Object,
                                                                                   this.EstateClient.Object,
                                                                                   this.SecurityServiceClient.Object,
                                                                                   this.TransactionReceiptBuilder.Object,
                                                                                   this.MessagingServiceClient.Object,
                                                                                   this.SettlementAggregateRepository.Object,
                                                                                   this.FloatAggregateRepository.Object);
        }
        
        [Theory]
        [InlineData(SettlementSchedule.Immediate)]
        [InlineData(SettlementSchedule.Weekly)]
        [InlineData(SettlementSchedule.Monthly)]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_SuccessfulSale_EventIsHandled(SettlementSchedule settlementSchedule)
        {
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFloatAggregateWithCostValues);

            TransactionAggregate transactionAggregate = TestData.GetCompletedAuthorisedSaleTransactionAggregate();
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionAggregate);

            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(new List<CalculatedFee>
                {
                    TestData.CalculatedFeeMerchantFee(TestData.TransactionFeeId),
                    TestData.CalculatedFeeServiceProviderFee(TestData.TransactionFeeId2)
                });

            this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MerchantResponse
                {
                    SettlementSchedule = settlementSchedule,
                });
            this.EstateClient.Setup(e => e.GetTransactionFeesForProduct(It.IsAny<String>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<CancellationToken>())).ReturnsAsync(TestData.ContractProductTransactionFees);

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);

            CalculatedFee merchantFee = transactionAggregate.GetFees().SingleOrDefault(f => f.FeeId == TestData.TransactionFeeId);
            merchantFee.ShouldNotBeNull();
            if (settlementSchedule == SettlementSchedule.Immediate){
                merchantFee.IsSettled.ShouldBeTrue();
            }
            else{
                merchantFee.IsSettled.ShouldBeFalse();
            }

            var expectedSettlementDate = settlementSchedule switch{
                SettlementSchedule.Monthly => transactionAggregate.TransactionDateTime.Date.AddMonths(1),
                SettlementSchedule.Weekly => transactionAggregate.TransactionDateTime.Date.AddDays(7),
                _ => transactionAggregate.TransactionDateTime.Date
            };
            merchantFee.SettlementDueDate.ShouldBe(expectedSettlementDate);
            
            CalculatedFee nonMerchantFee = transactionAggregate.GetFees().SingleOrDefault(f => f.FeeId == TestData.TransactionFeeId2);
            nonMerchantFee.ShouldNotBeNull();
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_SuccessfulSale_MerchantWithNotSetSettlementSchedule_ErrorThrown()
        {
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFloatAggregateWithCostValues);

            TransactionAggregate transactionAggregate = TestData.GetCompletedAuthorisedSaleTransactionAggregate();
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionAggregate);

            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(new List<CalculatedFee>
                                                                                                                                                                   {
                                                                                                                                                                       TestData.CalculatedFeeMerchantFee(TestData.TransactionFeeId),
                                                                                                                                                                       TestData.CalculatedFeeServiceProviderFee(TestData.TransactionFeeId2)
                                                                                                                                                                   });

            this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MerchantResponse
                              {
                                  SettlementSchedule = SettlementSchedule.NotSet,
                              });
            this.EstateClient.Setup(e => e.GetTransactionFeesForProduct(It.IsAny<String>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<CancellationToken>())).ReturnsAsync(TestData.ContractProductTransactionFees);

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            Should.Throw<NotSupportedException>(async () =>
                                                {
                                                    await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
                                                });
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_UnsuccessfulSale_EventIsHandled()
        {
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFloatAggregateWithCostValues);

            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetCompletedDeclinedSaleTransactionAggregate);

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_IncompleteSale_EventIsHandled()
        {
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFloatAggregateWithCostValues);

            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetIncompleteAuthorisedSaleTransactionAggregate);
            await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_SaleWithNoProductDetails_EventIsHandled()
        {
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFloatAggregateWithCostValues);

            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleWithNoProductDetailsTransactionAggregate);

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_AuthorisedLogon_EventIsHandled(){
            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFloatAggregateWithCostValues);

            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetCompletedAuthorisedLogonTransactionAggregate);

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_CustomerEmailReceiptRequestedEvent_EventIsHandled()
        {
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            await this.TransactionDomainEventHandler.Handle(TestData.CustomerEmailReceiptRequestedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_CustomerEmailReceiptResendRequestedEvent_EventIsHandled()
        {
            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            await this.TransactionDomainEventHandler.Handle(TestData.CustomerEmailReceiptResendRequestedEvent, CancellationToken.None);

            this.MessagingServiceClient.Verify(v => v.ResendEmail(It.IsAny<String>(),
                                                                  It.IsAny<ResendEmailRequest>(),
                                                                  It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionCostInformationRecordedEvent_EventIsHandled(){
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetCompletedAuthorisedSaleTransactionAggregate);

            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFloatAggregateWithCostValues);

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionCostInformationRecordedEvent, CancellationToken.None);

            this.FloatAggregateRepository.Verify(f => f.SaveChanges(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionCostInformationRecordedEvent_TransactionNotAuthorised_EventIsHandled()
        {
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetCompletedDeclinedSaleTransactionAggregate);

            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFloatAggregateWithCostValues);

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionCostInformationRecordedEvent, CancellationToken.None);

            this.FloatAggregateRepository.Verify(f => f.SaveChanges(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionCostInformationRecordedEvent_TransactionNotCompleted_EventIsHandled()
        {
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestData.GetIncompleteAuthorisedSaleTransactionAggregate);

            this.FloatAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFloatAggregateWithCostValues);

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionCostInformationRecordedEvent, CancellationToken.None);

            this.FloatAggregateRepository.Verify(f => f.SaveChanges(It.IsAny<FloatAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        //[Fact]
        //public async Task TransactionDomainEventHandler_Handle_MerchantFeeAddedToTransactionEvent_EventIsHandled()
        //{
        //    this.SettlementAggregateRepository.Setup(s => s.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(TestData.GetSettlementAggregateWithPendingMerchantFees(1));

        //    await this.TransactionDomainEventHandler.Handle(TestData.SettledMerchantFeeAddedToTransactionEvent(TestData.TransactionFeeSettlementDueDate), CancellationToken.None);
        //}

        //[Fact]
        //public async Task TransactionDomainEventHandler_Handle_MerchantFeeAddedToTransactionEvent_EventHasNoSettlementDueDate_EventIsHandled()
        //{
        //    await this.TransactionDomainEventHandler.Handle(TestData.SettledMerchantFeeAddedToTransactionEvent(DateTime.MinValue), CancellationToken.None);
        //}
    }
}
