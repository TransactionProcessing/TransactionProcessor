﻿using MediatR;
using SimpleResults;

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
    using EstateManagement.DataTransferObjects.Responses.Contract;
    using EstateManagement.DataTransferObjects.Responses.Merchant;
    using EventHandling;
    using FloatAggregate;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Microsoft.Extensions.Caching.Memory;
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
    /*
    public class TransactionDomainEventHandlerTests
    {
        private Mock<IAggregateRepository<SettlementAggregate, DomainEvent>> SettlementAggregateRepository;
        private Mock<IAggregateRepository<TransactionAggregate, DomainEvent>> TransactionAggregateRepository;

        private Mock<IFeeCalculationManager> FeeCalculationManager;

        private Mock<IEstateClient> EstateClient;

        private Mock<ISecurityServiceClient> SecurityServiceClient;

        private Mock<ITransactionReceiptBuilder> TransactionReceiptBuilder;

        private Mock<IMessagingServiceClient> MessagingServiceClient;

        private Mock<IAggregateRepository<FloatActivityAggregate, DomainEvent>> FloatActivityAggregateRepository;

        private Mock<IMemoryCacheWrapper> MemoryCache;

        private TransactionDomainEventHandler TransactionDomainEventHandler;
        private Mock<IMediator> Mediator;

        public TransactionDomainEventHandlerTests()
        {
            this.SettlementAggregateRepository = new Mock<IAggregateRepository<SettlementAggregate, DomainEvent>>();
            this.TransactionAggregateRepository = new Mock<IAggregateRepository<TransactionAggregate, DomainEvent>>();
            this.FloatActivityAggregateRepository = new Mock<IAggregateRepository<FloatActivityAggregate, DomainEvent>>();
            this.FeeCalculationManager = new Mock<IFeeCalculationManager>();
            this.EstateClient = new Mock<IEstateClient>();
            this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
            this.TransactionReceiptBuilder = new Mock<ITransactionReceiptBuilder>();
            this.MessagingServiceClient = new Mock<IMessagingServiceClient>();
            this.MemoryCache = new Mock<IMemoryCacheWrapper>();
            this.Mediator= new Mock<IMediator>();

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
                                                                                   this.FloatActivityAggregateRepository.Object,
                                                                                   this.MemoryCache.Object,
                                                                                   this.Mediator.Object);
        }
        
        [Theory]
        [InlineData(EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate)]
        [InlineData(EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly)]
        [InlineData(EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly)]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_SuccessfulSale_EventIsHandled(EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule settlementSchedule)
        {
            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersionFromLastEvent(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatActivityAggregate()));

            TransactionAggregate transactionAggregate = TestData.GetCompletedAuthorisedSaleTransactionAggregate();
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionAggregate);

            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(new List<CalculatedFee>
                {
                    TestData.CalculatedFeeMerchantFee(TestData.TransactionFeeId),
                    TestData.CalculatedFeeServiceProviderFee(TestData.TransactionFeeId2)
                });

            this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse
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

            this.MemoryCache.Setup(m => m.Set(It.IsAny<Object>(), It.IsAny<Object>(), It.IsAny<MemoryCacheEntryOptions>()));

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);

            CalculatedFee merchantFee = transactionAggregate.GetFees().SingleOrDefault(f => f.FeeId == TestData.TransactionFeeId);
            merchantFee.ShouldNotBeNull();
            if (settlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate){
                merchantFee.IsSettled.ShouldBeTrue();
            }
            else{
                merchantFee.IsSettled.ShouldBeFalse();
            }

            DateTime expectedSettlementDate = settlementSchedule switch{
                EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly => transactionAggregate.TransactionDateTime.Date.AddMonths(1),
                EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly => transactionAggregate.TransactionDateTime.Date.AddDays(7),
                _ => transactionAggregate.TransactionDateTime.Date
            };
            merchantFee.SettlementDueDate.ShouldBe(expectedSettlementDate);
            
            CalculatedFee nonMerchantFee = transactionAggregate.GetFees().SingleOrDefault(f => f.FeeId == TestData.TransactionFeeId2);
            nonMerchantFee.ShouldNotBeNull();
        }

        [Theory(Skip = "investigation on caching atm")]
        [InlineData(EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate)]
        [InlineData(EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly)]
        [InlineData(EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly)]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_SuccessfulSale_FeesAlreadyCached_EventIsHandled(EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule settlementSchedule)
        {
            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersionFromLastEvent(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatActivityAggregate()));

            TransactionAggregate transactionAggregate = TestData.GetCompletedAuthorisedSaleTransactionAggregate();
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionAggregate);

            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(new List<CalculatedFee>
                {
                    TestData.CalculatedFeeMerchantFee(TestData.TransactionFeeId),
                    TestData.CalculatedFeeServiceProviderFee(TestData.TransactionFeeId2)
                });

            this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse
                {
                    SettlementSchedule = settlementSchedule,
                });

            this.MemoryCache.Setup(m => m.TryGetValue(It.IsAny<Object>(), out It.Ref<List<ContractProductTransactionFee>>.IsAny))
                .Returns((Object key, out List<ContractProductTransactionFee> value) =>
                         {
                             value = TestData.ContractProductTransactionFees; // Set the out parameter
                             return true; // Return value indicating success
                         });

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.MemoryCache.Setup(m => m.Set(It.IsAny<Object>(), It.IsAny<Object>(), It.IsAny<MemoryCacheEntryOptions>()));

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);

            CalculatedFee merchantFee = transactionAggregate.GetFees().SingleOrDefault(f => f.FeeId == TestData.TransactionFeeId);
            merchantFee.ShouldNotBeNull();
            if (settlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate)
            {
                merchantFee.IsSettled.ShouldBeTrue();
            }
            else
            {
                merchantFee.IsSettled.ShouldBeFalse();
            }

            DateTime expectedSettlementDate = settlementSchedule switch
            {
                EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly => transactionAggregate.TransactionDateTime.Date.AddMonths(1),
                EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly => transactionAggregate.TransactionDateTime.Date.AddDays(7),
                _ => transactionAggregate.TransactionDateTime.Date
            };
            merchantFee.SettlementDueDate.ShouldBe(expectedSettlementDate);

            CalculatedFee nonMerchantFee = transactionAggregate.GetFees().SingleOrDefault(f => f.FeeId == TestData.TransactionFeeId2);
            nonMerchantFee.ShouldNotBeNull();
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_SuccessfulSale_MerchantWithNotSetSettlementSchedule_ErrorThrown()
        {
            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersionFromLastEvent(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatActivityAggregate()));

            TransactionAggregate transactionAggregate = TestData.GetCompletedAuthorisedSaleTransactionAggregate();
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionAggregate);

            this.FeeCalculationManager.Setup(f => f.CalculateFees(It.IsAny<List<TransactionFeeToCalculate>>(), It.IsAny<Decimal>(), It.IsAny<DateTime>())).Returns(new List<CalculatedFee>
                                                                                                                                                                   {
                                                                                                                                                                       TestData.CalculatedFeeMerchantFee(TestData.TransactionFeeId),
                                                                                                                                                                       TestData.CalculatedFeeServiceProviderFee(TestData.TransactionFeeId2)
                                                                                                                                                                   });

            this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse
                              {
                                  SettlementSchedule = EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.NotSet,
                              });
            this.EstateClient.Setup(e => e.GetTransactionFeesForProduct(It.IsAny<String>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<Guid>(),
                                                                        It.IsAny<CancellationToken>())).ReturnsAsync(TestData.ContractProductTransactionFees);

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.MemoryCache.Setup(m => m.Set(It.IsAny<Object>(), It.IsAny<Object>(), It.IsAny<MemoryCacheEntryOptions>()));

            var result = await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_UnsuccessfulSale_EventIsHandled()
        {
            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersionFromLastEvent(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatActivityAggregate()));

            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCompletedDeclinedSaleTransactionAggregate()));

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_IncompleteSale_EventIsHandled()
        {
            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersionFromLastEvent(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatActivityAggregate()));

            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetIncompleteAuthorisedSaleTransactionAggregate()));
            await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_SaleWithNoProductDetails_EventIsHandled()
        {
            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersionFromLastEvent(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatActivityAggregate()));

            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleWithNoProductDetailsTransactionAggregate()));

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_AuthorisedLogon_EventIsHandled(){
            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersionFromLastEvent(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatActivityAggregate()));

            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedLogonTransactionAggregate()));

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_CustomerEmailReceiptRequestedEvent_EventIsHandled()
        {
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.TransactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            this.EstateClient.Setup(e => e.GetEstate(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEstateResponseWithOperator1);
            this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetMerchantResponseWithOperator1);

            this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse);

            this.MessagingServiceClient
                .Setup(m => m.SendEmail(It.IsAny<String>(), It.IsAny<SendEmailRequest>(),
                    It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

            var result = await this.TransactionDomainEventHandler.Handle(TestData.CustomerEmailReceiptRequestedEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
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
                .ReturnsAsync(Result.Success(TestData.GetCompletedAuthorisedSaleTransactionAggregate()));
            this.TransactionAggregateRepository.Setup(t => t.SaveChanges(It.IsAny<TransactionAggregate>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersionFromLastEvent(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatActivityAggregate()));

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionCostInformationRecordedEvent, CancellationToken.None);

            this.FloatActivityAggregateRepository.Verify(f => f.SaveChanges(It.IsAny<FloatActivityAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionCostInformationRecordedEvent_TransactionNotAuthorised_EventIsHandled()
        {
            this.TransactionAggregateRepository.Setup(t => t.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(TestData.GetCompletedDeclinedSaleTransactionAggregate()));

            this.FloatActivityAggregateRepository.Setup(f => f.GetLatestVersionFromLastEvent(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFloatActivityAggregate()));

            await this.TransactionDomainEventHandler.Handle(TestData.TransactionCostInformationRecordedEvent, CancellationToken.None);

            this.FloatActivityAggregateRepository.Verify(f => f.SaveChanges(It.IsAny<FloatActivityAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task TransactionDomainEventHandler_Handle_TransactionCostInformationRecordedEvent_TransactionNotCompleted_EventIsHandled()
        {

            var result = await this.TransactionDomainEventHandler.Handle(TestData.TransactionCostInformationRecordedEvent, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
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

        [Fact]
        public async Task TransactionDomainEventHandler_RequireFeeCalculation_IsNotAuthorised_ReturnsFalse(){
            
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Sale, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.DeclineTransaction(TestData.OperatorId, "111", "SUCCESS", "0000", "SUCCESS");

            var result = TransactionDomainEventHandler.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainEventHandler_RequireFeeCalculation_IsNotCompelted_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Sale, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, "111", "111", "SUCCESS", "1234", "0000", "SUCCESS");

            var result = TransactionDomainEventHandler.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainEventHandler_RequireFeeCalculation_IsALogon_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Logon, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransactionLocally("111", "0001", "SUCCESS");
            transactionAggregate.CompleteTransaction();


            var result = TransactionDomainEventHandler.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainEventHandler_RequireFeeCalculation_NoContractId_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Sale, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, "111", "111", "SUCCESS", "1234", "0000", "SUCCESS");
            transactionAggregate.CompleteTransaction();


            var result = TransactionDomainEventHandler.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainEventHandler_RequireFeeCalculation_NullAmount_ReturnsFalse()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Sale, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  null);
            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, "111", "111", "SUCCESS", "1234", "0000", "SUCCESS");
            transactionAggregate.CompleteTransaction();


            var result = TransactionDomainEventHandler.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task TransactionDomainEventHandler_RequireFeeCalculation_ReturnsTrue()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber,
                                                  TransactionType.Sale, TestData.TransactionReference,
                                                  TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, "111", "111", "SUCCESS", "1234", "0000", "SUCCESS");
            transactionAggregate.CompleteTransaction();


            var result = TransactionDomainEventHandler.RequireFeeCalculation(transactionAggregate);
            result.ShouldBeTrue();
        }

        [Theory]
        [InlineData(SettlementSchedule.Immediate, "2024-05-01", "2024-05-01")]
        [InlineData(SettlementSchedule.NotSet, "2024-05-01", "2024-05-01")]
        [InlineData(SettlementSchedule.Weekly, "2024-05-01", "2024-05-08")]
        [InlineData(SettlementSchedule.Monthly, "2024-05-01", "2024-06-01")]
        public async Task TransactionDomainEventHandler_CalculateSettlementDate_CorrectDateReturned(SettlementSchedule settlementSchedule, String completedDateString, String expectedDateString){

            DateTime completedDate = DateTime.ParseExact(completedDateString, "yyyy-MM-dd", null);
            DateTime expectedDate = DateTime.ParseExact(expectedDateString, "yyyy-MM-dd", null);
            DateTime result = TransactionDomainEventHandler.CalculateSettlementDate(settlementSchedule, completedDate);
            result.Date.ShouldBe(expectedDate.Date);
        }
    }*/
}


