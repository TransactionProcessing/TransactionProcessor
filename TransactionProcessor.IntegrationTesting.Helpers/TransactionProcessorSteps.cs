﻿using EventStore.Client;

namespace TransactionProcessor.IntegrationTesting.Helpers;

using System.Diagnostics.Metrics;
using System.Net;
using System.Text;
using System.Text.Json;
using Client;
using DataTransferObjects;
using EstateManagement.IntegrationTesting.Helpers;
using Newtonsoft.Json;
using Shared.IntegrationTesting;
using Shouldly;

public class TransactionProcessorSteps
{
    private readonly ITransactionProcessorClient TransactionProcessorClient;

    private readonly HttpClient TestHostHttpClient;
    private readonly EventStoreProjectionManagementClient ProjectionManagementClient;

    public TransactionProcessorSteps(ITransactionProcessorClient transactionProcessorClient, HttpClient testHostHttpClient, EventStoreProjectionManagementClient projectionManagementClient)
    {
        this.TransactionProcessorClient = transactionProcessorClient;
        this.TestHostHttpClient = testHostHttpClient;
        this.ProjectionManagementClient = projectionManagementClient;
    }

    public async Task WhenIPerformTheFollowingTransactions(String accessToken, List<(EstateDetails, Guid, String, SerialisedMessage)> serialisedMessages)
    {
        List<(EstateDetails, Guid, String, SerialisedMessage)> responseMessages = new List<(EstateDetails, Guid, String, SerialisedMessage)>();
        foreach ((EstateDetails, Guid, String, SerialisedMessage) serialisedMessage in serialisedMessages)
        {
            SerialisedMessage responseSerialisedMessage =
                await this.TransactionProcessorClient.PerformTransaction(accessToken, serialisedMessage.Item4, CancellationToken.None);
            var message = JsonConvert.SerializeObject(responseSerialisedMessage);
            serialisedMessage.Item1.AddTransactionResponse(serialisedMessage.Item2, serialisedMessage.Item3, message);
        }
    }

    public async Task WhenICreateTheFollowingMerchants(String accessToken, Guid estateId, Guid merchantId){
        await Retry.For(async () => {
                string projectionName = "MerchantBalanceProjection";
                String partitionId = $"MerchantBalance-{merchantId:N}";

            dynamic gg = await this.ProjectionManagementClient.GetStateAsync<dynamic>(
                projectionName, partitionId);
            JsonElement x = (JsonElement)gg;

            MerchantBalanceResponse response = await this.TransactionProcessorClient.GetMerchantBalance(accessToken, estateId, merchantId, CancellationToken.None);

                            response.ShouldNotBeNull();
                            
                            // Force a read model database hit
                            MerchantBalanceResponse response2 = await this.TransactionProcessorClient.GetMerchantBalance(accessToken, estateId, merchantId, CancellationToken.None, liveBalance:false);

                            response2.ShouldNotBeNull();
        }, 
                        TimeSpan.FromMinutes(2), 
                        TimeSpan.FromSeconds(30));
    }

    public void ValidateTransactions(List<(SerialisedMessage, String, String, String)> transactions)
    {
        foreach ((SerialisedMessage, String, String, String) transaction in transactions)
        {
            Object transactionResponse = JsonConvert.DeserializeObject(transaction.Item1.SerialisedData,
                                                                       new JsonSerializerSettings
                                                                       {
                                                                           TypeNameHandling = TypeNameHandling.All
                                                                       });

            this.ValidateTransactionResponse((dynamic)transactionResponse, transaction.Item2, transaction.Item3, transaction.Item4);
        }
    }

    private void ValidateTransactionResponse(LogonTransactionResponse logonTransactionResponse, String transactionNumber, String expectedResponseCode, String expectedResponseMessage)
    {

        logonTransactionResponse.ResponseCode.ShouldBe(expectedResponseCode, $"Transaction Number {transactionNumber} verification failed");
        logonTransactionResponse.ResponseMessage.ShouldBe(expectedResponseMessage, $"Transaction Number {transactionNumber} verification failed");
    }

    private void ValidateTransactionResponse(SaleTransactionResponse saleTransactionResponse, String transactionNumber, String expectedResponseCode, String expectedResponseMessage)
    {
        saleTransactionResponse.ResponseCode.ShouldBe(expectedResponseCode, $"Transaction Number {transactionNumber} verification failed");
        saleTransactionResponse.ResponseMessage.ShouldBe(expectedResponseMessage, $"Transaction Number {transactionNumber} verification failed");
    }

    private void ValidateTransactionResponse(ReconciliationResponse reconciliationResponse, String transactionNumber, String expectedResponseCode, String expectedResponseMessage)
    {
        reconciliationResponse.ResponseCode.ShouldBe(expectedResponseCode, $"Transaction Number {transactionNumber} verification failed");
        reconciliationResponse.ResponseMessage.ShouldBe(expectedResponseMessage, $"Transaction Number {transactionNumber} verification failed");
    }

    public async Task WhenIRequestTheReceiptIsResent(String accessToken, List<SerialisedMessage> transactions)
    {
        foreach (SerialisedMessage serialisedMessage in transactions)
        {
            SaleTransactionResponse transactionResponse = JsonConvert.DeserializeObject<SaleTransactionResponse>(serialisedMessage.SerialisedData,
                                                                                                                 new JsonSerializerSettings
                                                                                                                 {
                                                                                                                     TypeNameHandling = TypeNameHandling.All
                                                                                                                 });

            await Retry.For(async () => {
                                Should.NotThrow(async () => {
                                                    await this.TransactionProcessorClient.ResendEmailReceipt(accessToken,
                                                                                                             transactionResponse.EstateId,
                                                                                                             transactionResponse.TransactionId,
                                                                                                             CancellationToken.None);
                                                });
                            });

        }
    }

    public async Task ThenTheFollowingEntriesAppearInTheMerchantsBalanceHistoryForEstateAndMerchant(String accessToken, DateTime startDate, DateTime endDate, List<ReqnrollExtensions.BalanceEntry> balanceEntries)
    {

        var merchants = balanceEntries.GroupBy(b => new { b.EstateId, b.MerchantId }).Select(b => new {
                                                                                                          b.Key.EstateId,
                                                                                                          b.Key.MerchantId,
                                                                                                          NumberEntries = b.Count()
                                                                                                      });


        foreach (var m in merchants)
        {
            List<MerchantBalanceChangedEntryResponse> balanceHistory = null;
            List<ReqnrollExtensions.BalanceEntry> merchantEntries = balanceEntries.Where(b => b.EstateId == m.EstateId && b.MerchantId == m.MerchantId).ToList();

            await Retry.For(async () => {
                                balanceHistory =
                                    await this.TransactionProcessorClient.GetMerchantBalanceHistory(accessToken,
                                                                                                    m.EstateId,
                                                                                                    m.MerchantId,
                                                                                                    startDate,
                                                                                                    endDate,
                                                                                                    CancellationToken.None);

                                balanceHistory.ShouldNotBeNull();
                                balanceHistory.ShouldNotBeEmpty();
                                balanceHistory.Count.ShouldBe(m.NumberEntries);
                                foreach (ReqnrollExtensions.BalanceEntry merchantEntry in merchantEntries)
                                {


                                    MerchantBalanceChangedEntryResponse balanceEntry =
                                        balanceHistory.SingleOrDefault(m => m.Reference == merchantEntry.Reference &&
                                                                            m.DateTime.Date == merchantEntry.DateTime &&
                                                                            m.DebitOrCredit == merchantEntry.EntryType &&
                                                                            m.ChangeAmount == merchantEntry.ChangeAmount);

                                    balanceEntry.ShouldNotBeNull($"EntryDateTime [{merchantEntry.DateTime.ToString("yyyy-MM-dd")}] Ref [{merchantEntry.Reference}] DebitOrCredit [{merchantEntry.EntryType}] ChangeAmount [{merchantEntry.ChangeAmount}]");
                                }

                            },
                            TimeSpan.FromMinutes(10),
                            TimeSpan.FromSeconds(30));
        }
    }

    public async Task SendRequestToTestHost<T>(List<T> objects, String url){
        foreach (T o in objects){
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await this.TestHostHttpClient.SendAsync(httpRequestMessage);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    public async Task GivenTheFollowingBillsAreAvailableAtThePataPawaPostPaidHost(List<ReqnrollExtensions.PataPawaBill> bills){
        await this.SendRequestToTestHost<ReqnrollExtensions.PataPawaBill>(bills, "/api/developer/patapawapostpay/createbill");
    }

    public async Task GivenTheFollowingMetersAreAvailableAtThePataPawaPrePaidHost(List<ReqnrollExtensions.PataPawaMeter> meters){
        await this.SendRequestToTestHost<ReqnrollExtensions.PataPawaMeter>(meters, "/api/developer/patapawaprepay/createmeter");
    }

    public async Task GivenTheFollowingUsersAreAvailableAtThePataPawaPrePaidHost(List<ReqnrollExtensions.PataPawaUser> users){
        await this.SendRequestToTestHost<ReqnrollExtensions.PataPawaUser>(users, "/api/developer/patapawaprepay/createuser");
    }

    public async Task<GetVoucherResponse> GetVoucherByTransactionNumber(String accessToken, EstateDetails estate, SaleTransactionResponse transactionResponse)
    {
        GetVoucherResponse voucher = null;
        await Retry.For(async () => {
                            voucher = await this.TransactionProcessorClient.GetVoucherByTransactionId(accessToken,
                                                                                                      estate.EstateId,
                                                                                                      transactionResponse.TransactionId,
                                                                                                      CancellationToken.None);
                        });
        return voucher;
    }

    public async Task RedeemVoucher(String accessToken, EstateDetails estate, GetVoucherResponse voucher, Decimal expectedBalance)
    {
        RedeemVoucherRequest redeemVoucherRequest = new RedeemVoucherRequest
                                                    {
                                                        EstateId = estate.EstateId,
                                                        RedeemedDateTime = DateTime.Now,
                                                        VoucherCode = voucher.VoucherCode
                                                    };
        // Do the redeem
        await Retry.For(async () =>
                        {
                            RedeemVoucherResponse response = await this.TransactionProcessorClient
                                                                       .RedeemVoucher(accessToken, redeemVoucherRequest, CancellationToken.None)
                                                                       .ConfigureAwait(false);
                            response.RemainingBalance.ShouldBe(expectedBalance);
                        });
    }

    public async Task WhenIProcessTheSettlementForOnEstateThenFeesAreMarkedAsSettledAndTheSettlementIsCompleted(String accessToken, ReqnrollExtensions.ProcessSettlementRequest request, Int32 expectedNumberFeesSettled)
    {
        await this.TransactionProcessorClient.ProcessSettlement(accessToken,
                                                                request.SettlementDate,
                                                                request.EstateDetails.EstateId,
                                                                request.MerchantId,
                                                                CancellationToken.None);

        await Retry.For(async () => {
                            TransactionProcessor.DataTransferObjects.SettlementResponse settlement =
                                await this.TransactionProcessorClient.GetSettlementByDate(accessToken,
                                                                                          request.SettlementDate,
                                                                                          request.EstateDetails.EstateId,
                                                                                          request.MerchantId,
                                                                                          CancellationToken.None);

                            settlement.NumberOfFeesPendingSettlement.ShouldBe(0);
                            settlement.NumberOfFeesSettled.ShouldBe(expectedNumberFeesSettled);
                            settlement.SettlementCompleted.ShouldBeTrue();
                        },
                        TimeSpan.FromMinutes(2));
    }

    public async Task WhenIGetTheCompletedSettlementsTheFollowingInformationShouldBeReturned(String accessToken, List<(EstateDetails, Guid, DateTime, Int32)> requests)
    {
        foreach ((EstateDetails, Guid, DateTime, Int32) request in requests)
        {
            await Retry.For(async () => {
                                TransactionProcessor.DataTransferObjects.SettlementResponse settlements =
                                    await this.TransactionProcessorClient.GetSettlementByDate(accessToken,
                                                                                              request.Item3,
                                                                                              request.Item1.EstateId,
                                                                                              request.Item2,
                                                                                              CancellationToken.None);

                                settlements.NumberOfFeesSettled.ShouldBe(request.Item4, $"Settlement date {request.Item3}");
                            },
                            TimeSpan.FromMinutes(3));
        }
    }

    public async Task WhenIGetThePendingSettlementsTheFollowingInformationShouldBeReturned(String accessToken, List<(EstateDetails, Guid, DateTime, Int32)> requests)
    {
        foreach ((EstateDetails, Guid, DateTime, Int32) request in requests)
        {
            await Retry.For(async () => {
                                TransactionProcessor.DataTransferObjects.SettlementResponse settlements =
                                    await this.TransactionProcessorClient.GetSettlementByDate(accessToken,
                                                                                              request.Item3,
                                                                                              request.Item1.EstateId,
                                                                                              request.Item2,
                                                                                              CancellationToken.None);

                                settlements.NumberOfFeesPendingSettlement.ShouldBe(request.Item4, $"Settlement date {request.Item3}");
                            },
                            TimeSpan.FromMinutes(3));
        }
    }
}