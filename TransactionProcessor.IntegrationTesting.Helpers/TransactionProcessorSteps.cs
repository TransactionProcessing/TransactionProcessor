using EventStore.Client;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Requests.Operator;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
using TransactionProcessor.DataTransferObjects.Responses.Operator;

namespace TransactionProcessor.IntegrationTesting.Helpers;

using System.Diagnostics.Metrics;
using System.Net;
using System.Text;
using System.Text.Json;
using Client;
using DataTransferObjects;
using Newtonsoft.Json;
using Shared.IntegrationTesting;
using Shouldly;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

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

    public async Task<List<MerchantResponse>> WhenICreateTheFollowingMerchants(string accessToken, List<(EstateDetails estate, CreateMerchantRequest request)> requests)
    {
        List<MerchantResponse> responses = new List<MerchantResponse>();

        List<(Guid, Guid, String)> merchants = new List<(Guid, Guid, String)>();

        foreach ((EstateDetails estate, CreateMerchantRequest request) request in requests)
        {

            Result? result = await this.TransactionProcessorClient
                .CreateMerchant(accessToken, request.estate.EstateId, request.request, CancellationToken.None)
                .ConfigureAwait(false);

            result.IsSuccess.ShouldBeTrue();

            merchants.Add((request.estate.EstateId, request.request.MerchantId.Value, request.request.Name));
        }

        foreach ((Guid, Guid, String) m in merchants)
        {
            await Retry.For(async () => {
                MerchantResponse merchant = await this.TransactionProcessorClient
                    .GetMerchant(accessToken, m.Item1, m.Item2, CancellationToken.None)
                    .ConfigureAwait(false);
                responses.Add(merchant);

                string projectionName = "MerchantBalanceProjection";
                String partitionId = $"MerchantBalance-{m.Item2:N}";

                dynamic gg = await this.ProjectionManagementClient.GetStateAsync<dynamic>(
                    projectionName, partitionId);
                JsonElement x = (JsonElement)gg;

                MerchantBalanceResponse response = await this.TransactionProcessorClient.GetMerchantBalance(accessToken, m.Item1, m.Item2, CancellationToken.None);

                response.ShouldNotBeNull();

                // Force a read model database hit
                MerchantBalanceResponse response2 = await this.TransactionProcessorClient.GetMerchantBalance(accessToken, m.Item1, m.Item2, CancellationToken.None, liveBalance: false);

                response2.ShouldNotBeNull();
            });
        }

        return responses;
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

    public async Task<List<EstateResponse>> WhenICreateTheFollowingEstatesX(String accessToken, List<CreateEstateRequest> requests)
    {
        foreach (CreateEstateRequest createEstateRequest in requests)
        {
            Result? result = await this.TransactionProcessorClient
                .CreateEstate(accessToken, createEstateRequest, CancellationToken.None)
                .ConfigureAwait(false);

            result.IsSuccess.ShouldBeTrue();
        }

        List<EstateResponse> results = new List<EstateResponse>();
        foreach (CreateEstateRequest createEstateRequest in requests)
        {
            EstateResponse estate = null;
            await Retry.For(async () => {
                    List<EstateResponse>? estates = await this.TransactionProcessorClient
                        .GetEstates(accessToken, createEstateRequest.EstateId, CancellationToken.None).ConfigureAwait(false);
                    estates.ShouldNotBeNull();
                    estates.Count.ShouldBe(1);
                    estate = estates.Single();
                },
                retryFor: TimeSpan.FromSeconds(180)).ConfigureAwait(false);

            estate.EstateName.ShouldBe(createEstateRequest.EstateName);
            results.Add(estate);
        }

        return results;
    }

    public async Task<List<(EstateDetails, MerchantOperatorResponse)>> WhenIAssignTheFollowingOperatorToTheMerchants(string accessToken, List<(EstateDetails estate, Guid merchantId, DataTransferObjects.Requests.Merchant.AssignOperatorRequest request)> requests)
    {

        List<(EstateDetails, MerchantOperatorResponse)> result = new();

        List<(EstateDetails estate, Guid merchantId, Guid operatorId)> merchantOperators = new();
        foreach ((EstateDetails estate, Guid merchantId, DataTransferObjects.Requests.Merchant.AssignOperatorRequest request) request in requests)
        {
            await this.TransactionProcessorClient.AssignOperatorToMerchant(accessToken,
                    request.estate.EstateId,
                    request.merchantId,
                    request.request,
                    CancellationToken.None).ConfigureAwait(false);

            merchantOperators.Add((request.estate, request.merchantId, request.request.OperatorId));
        }

        foreach (var m in merchantOperators)
        {
            await Retry.For(async () => {
                MerchantResponse merchant = await this.TransactionProcessorClient
                    .GetMerchant(accessToken, m.estate.EstateId, m.merchantId, CancellationToken.None)
                    .ConfigureAwait(false);

                if (merchant.Operators == null)
                {
                    Console.WriteLine($"Merchant {merchant.MerchantName} has null operators");
                }

                foreach (MerchantOperatorResponse merchantOperatorResponse in merchant.Operators) {
                    Console.WriteLine($"Operator Id {merchantOperatorResponse.OperatorId} Name {merchantOperatorResponse.Name}");
                }

                MerchantOperatorResponse op = merchant.Operators.SingleOrDefault(o => o.OperatorId == m.operatorId);
                op.ShouldNotBeNull();
                result.Add((m.estate, op));
            });
        }

        return result;
    }

    public async Task<List<(Guid, EstateOperatorResponse)>> WhenICreateTheFollowingOperators(String accessToken, List<(EstateDetails estate, CreateOperatorRequest request)> requests)
    {
        List<(Guid, EstateOperatorResponse)> results = new List<(Guid, EstateOperatorResponse)>();
        foreach ((EstateDetails estate, CreateOperatorRequest request) request in requests)
        {
            Result? result = await this.TransactionProcessorClient
                .CreateOperator(accessToken,
                    request.estate.EstateId,
                    request.request,
                    CancellationToken.None).ConfigureAwait(false);

            result.IsSuccess.ShouldBeTrue();
        }

        foreach ((EstateDetails estate, CreateOperatorRequest request) request in requests)
        {
            await Retry.For(async () => {
                Result<List<OperatorResponse>>? operators = await this.TransactionProcessorClient
                    .GetOperators(accessToken, request.estate.EstateId, CancellationToken.None).ConfigureAwait(false);
                operators.IsSuccess.ShouldBeTrue();
                var @operator = operators.Data.SingleOrDefault(o => o.Name == request.request.Name);
                @operator.ShouldNotBeNull();
                request.estate.AddOperator(@operator.OperatorId, request.request.Name);
                results.Add((request.estate.EstateId,
                    new EstateOperatorResponse
                    {
                        OperatorId = @operator.OperatorId,
                        Name = request.request.Name,
                        RequireCustomMerchantNumber = request.request.RequireCustomMerchantNumber.GetValueOrDefault(),
                        RequireCustomTerminalNumber = request.request.RequireCustomTerminalNumber.GetValueOrDefault()
                    }));
            });
        }

        return results;
    }

    public async Task GivenIHaveAssignedTheFollowingOperatorsToTheEstates(String accessToken, List<(EstateDetails estate, DataTransferObjects.Requests.Estate.AssignOperatorRequest request)> requests)
    {
        List<(Guid, EstateOperatorResponse)> results = new List<(Guid, EstateOperatorResponse)>();
        foreach ((EstateDetails estate, DataTransferObjects.Requests.Estate.AssignOperatorRequest request) request in requests)
        {
            await this.TransactionProcessorClient.AssignOperatorToEstate(accessToken, request.estate.EstateId, request.request, CancellationToken.None);
        }

        // verify at the read model
        foreach (var request in requests)
        {
            await Retry.For(async () => {
                    EstateResponse e = await this.TransactionProcessorClient.GetEstate(accessToken,
                        request.estate.EstateId,
                        CancellationToken.None);
                    EstateOperatorResponse operatorResponse = e.Operators.SingleOrDefault(o => o.OperatorId == request.request.OperatorId);
                    operatorResponse.ShouldNotBeNull();
                    results.Add((request.estate.EstateId, operatorResponse));

                    request.estate.AddAssignedOperator(operatorResponse.OperatorId);
                },
                retryFor: TimeSpan.FromSeconds(180)).ConfigureAwait(false);
        }
    }

    public async Task<List<ContractResponse>> GivenICreateAContractWithTheFollowingValues(string accessToken, List<(EstateDetails, CreateContractRequest)> requests)
    {
        List<ContractResponse> contractList = new List<ContractResponse>();

        foreach ((EstateDetails, CreateContractRequest) request in requests)
        {
            Result? result = await this.TransactionProcessorClient.CreateContract(accessToken, request.Item1.EstateId, request.Item2,
                CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
        }

        foreach ((EstateDetails, CreateContractRequest) request in requests)
        {

            await Retry.For(async () => {
                Result<List<ContractResponse>> getContractsResult = await this.TransactionProcessorClient.GetContracts(accessToken, request.Item1.EstateId, CancellationToken.None).ConfigureAwait(false);
                getContractsResult.Data.ShouldNotBeNull();
                ContractResponse contract = getContractsResult.Data.Single(c => c.Description == request.Item2.Description);
                contractList.Add(contract);
            });
        }

        return contractList;
    }

    public async Task WhenICreateTheFollowingProducts(String accessToken, List<(EstateDetails, Contract, AddProductToContractRequest)> requests)
    {
        List<(EstateDetails, Contract, AddProductToContractRequest)> estateContractProducts = new();
        foreach ((EstateDetails, Contract, AddProductToContractRequest) request in requests)
        {
            var result = await this.TransactionProcessorClient.AddProductToContract(accessToken,
                request.Item1.EstateId,
                request.Item2.ContractId,
                request.Item3,
                CancellationToken.None);
            estateContractProducts.Add((request.Item1, request.Item2, request.Item3));
        }

        foreach ((EstateDetails, Contract, AddProductToContractRequest) estateContractProduct in estateContractProducts)
        {

            await Retry.For(async () => {
                ContractResponse contract = await this.TransactionProcessorClient.GetContract(accessToken, estateContractProduct.Item1.EstateId, estateContractProduct.Item2.ContractId, CancellationToken.None).ConfigureAwait(false);
                contract.ShouldNotBeNull();

                ContractProduct product = contract.Products.SingleOrDefault(c => c.Name == estateContractProduct.Item3.ProductName);
                product.ShouldNotBeNull();

                estateContractProduct.Item2.AddProduct(product.ProductId,
                    estateContractProduct.Item3.ProductName,
                    estateContractProduct.Item3.DisplayText,
                    estateContractProduct.Item3.Value);
            });
        }
    }

    public async Task WhenIAddTheFollowingTransactionFees(String accessToken, List<(EstateDetails, Contract, Product, AddTransactionFeeForProductToContractRequest)> requests)
    {
        List<(EstateDetails, Contract, Product, AddTransactionFeeForProductToContractRequest)> estateContractProductsFees = new();
        foreach ((EstateDetails, Contract, Product, AddTransactionFeeForProductToContractRequest) request in requests)
        {
            var result = await this.TransactionProcessorClient.AddTransactionFeeForProductToContract(accessToken,
                                                                          request.Item1.EstateId,
                                                                          request.Item2.ContractId,
                                                                          request.Item3.ProductId,
                                                                          request.Item4,
                                                                          CancellationToken.None);
        }

        foreach ((EstateDetails, Contract, Product, AddTransactionFeeForProductToContractRequest) estateContractProductsFee in estateContractProductsFees)
        {
            await Retry.For(async () => {
                ContractResponse contract = await this.TransactionProcessorClient.GetContract(accessToken, estateContractProductsFee.Item1.EstateId, estateContractProductsFee.Item2.ContractId, CancellationToken.None).ConfigureAwait(false);
                contract.ShouldNotBeNull();

                ContractProduct product = contract.Products.SingleOrDefault(c => c.ProductId == estateContractProductsFee.Item3.ProductId);
                product.ShouldNotBeNull();

                var transactionFee = product.TransactionFees.SingleOrDefault(f => f.Description == estateContractProductsFee.Item4.Description);
                transactionFee.ShouldNotBeNull();

                estateContractProductsFee.Item3.AddTransactionFee(transactionFee.TransactionFeeId,
                                                                  estateContractProductsFee.Item4.CalculationType,
                                                                  estateContractProductsFee.Item4.FeeType,
                                                                  estateContractProductsFee.Item4.Description,
                                                                  estateContractProductsFee.Item4.Value);

                List<ContractProductTransactionFee>? fees = await this.TransactionProcessorClient.GetTransactionFeesForProduct(accessToken,
                                                                                                                 estateContractProductsFee.Item1.EstateId,
                                                                                                                 estateContractProductsFee.Item1.GetMerchants().First().MerchantId,
                                                                                                                 contract.ContractId,
                                                                                                                 product.ProductId,
                                                                                                                 CancellationToken.None);
                var fee = fees.SingleOrDefault(f => f.TransactionFeeId == transactionFee.TransactionFeeId);
                fee.ShouldNotBeNull();
            });
        }
    }

    public async Task<List<(EstateDetails, MerchantResponse, String)>> GivenIHaveAssignedTheFollowingDevicesToTheMerchants(string accessToken, List<(EstateDetails, Guid, AddMerchantDeviceRequest)> requests)
    {
        List<(EstateDetails, MerchantResponse, String)> result = new();
        List<(EstateDetails estate, Guid merchantId, Guid deviceId)> merchantDevices = new();
        foreach ((EstateDetails, Guid, AddMerchantDeviceRequest) request in requests)
        {
            await this.TransactionProcessorClient.AddDeviceToMerchant(accessToken, request.Item1.EstateId, request.Item2, request.Item3, CancellationToken.None).ConfigureAwait(false);
        }

        foreach (var m in merchantDevices)
        {
            await Retry.For(async () => {
                MerchantResponse merchant = await this.TransactionProcessorClient
                    .GetMerchant(accessToken, m.estate.EstateId, m.merchantId, CancellationToken.None)
                    .ConfigureAwait(false);
                var device = merchant.Devices.SingleOrDefault(o => o.Key == m.deviceId);
                device.Value.ShouldNotBeNull();
                result.Add((m.estate, merchant, device.Value));
            });
        }

        return result;
    }

    public async Task WhenIAddTheFollowingContractsToTheFollowingMerchants(String accessToken, List<(EstateDetails, Guid, Guid)> requests)
    {
        foreach ((EstateDetails, Guid, Guid) request in requests)
        {
            AddMerchantContractRequest addMerchantContractRequest = new AddMerchantContractRequest
            {
                ContractId = request.Item3
            };
            await this.TransactionProcessorClient.AddContractToMerchant(accessToken, request.Item1.EstateId, request.Item2, addMerchantContractRequest, CancellationToken.None);
        }
    }

    public async Task GivenIMakeTheFollowingManualMerchantDeposits(String accessToken, (EstateDetails, Guid, MakeMerchantDepositRequest) request)
    {
        var result = await this.TransactionProcessorClient.MakeMerchantDeposit(accessToken, request.Item1.EstateId, request.Item2, request.Item3, CancellationToken.None).ConfigureAwait(false);
        result.IsSuccess.ShouldBeTrue();
    }

    public async Task WhenICreateTheFollowingSecurityUsers(String accessToken, List<CreateNewUserRequest> requests, List<EstateDetails> estateDetailsList)
    {
        foreach (CreateNewUserRequest createNewUserRequest in requests)
        {
            var estateDetails = estateDetailsList.SingleOrDefault(e => e.EstateId == createNewUserRequest.EstateId.GetValueOrDefault());
            estateDetails.ShouldNotBeNull();


            if (createNewUserRequest.UserType == 1)
            {
                CreateEstateUserRequest request = new CreateEstateUserRequest
                {
                    EmailAddress = createNewUserRequest.EmailAddress,
                    FamilyName = createNewUserRequest.FamilyName,
                    GivenName = createNewUserRequest.GivenName,
                    MiddleName = createNewUserRequest.MiddleName,
                    Password = createNewUserRequest.Password
                };
                await this.TransactionProcessorClient.CreateEstateUser(accessToken,
                                                         estateDetails.EstateId,
                                                         request,
                                                         CancellationToken.None);

                estateDetails.SetEstateUser(request.EmailAddress, request.Password);
            }
            else
            {
                // Creating a merchant user
                String token = accessToken;
                if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
                {
                    token = estateDetails.AccessToken;
                }

                CreateMerchantUserRequest createMerchantUserRequest = new CreateMerchantUserRequest
                {
                    EmailAddress = createNewUserRequest.EmailAddress,
                    FamilyName = createNewUserRequest.FamilyName,
                    GivenName = createNewUserRequest.GivenName,
                    MiddleName = createNewUserRequest.MiddleName,
                    Password = createNewUserRequest.Password
                };

                await this.TransactionProcessorClient.CreateMerchantUser(token,
                                                           estateDetails.EstateId,
                                                           createNewUserRequest.MerchantId.Value,
                                                           createMerchantUserRequest,
                                                           CancellationToken.None);

                estateDetails.AddMerchantUser(createNewUserRequest.MerchantName, createMerchantUserRequest.EmailAddress, createMerchantUserRequest.Password);
            }
        }
    }

    public async Task WhenIGetTheEstateTheEstateDetailsAreReturnedAsFollows(String accessToken, String estateName, List<EstateDetails> estateDetailsList, List<String> expectedEstateDetails)
    {
        Guid estateId = Guid.NewGuid();
        EstateDetails estateDetails = estateDetailsList.SingleOrDefault(e => e.EstateName == estateName);
        estateDetails.ShouldNotBeNull();

        String token = accessToken;
        if (estateDetails != null)
        {
            estateId = estateDetails.EstateId;
            if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
            {
                token = estateDetails.AccessToken;
            }
        }

        EstateResponse estate = await this.TransactionProcessorClient.GetEstate(token, estateId, CancellationToken.None).ConfigureAwait(false);
        estate.EstateName.ShouldBe(expectedEstateDetails.Single());
    }

    public async Task WhenIGetAllTheOperatorsTheFollowingDetailsAreReturned(String accessToken, List<(EstateDetails, Guid, OperatorResponse)> expectedOperatorResponses)
    {
        IEnumerable<Guid> distinctEstates = expectedOperatorResponses.Select(e => e.Item1).DistinctBy(e => e.EstateId).Select(e => e.EstateId);

        foreach (Guid estateId in distinctEstates)
        {
            await Retry.For(async () => {
                List<OperatorResponse>? operatorList = await this.TransactionProcessorClient.GetOperators(accessToken, estateId, CancellationToken.None);

                foreach (OperatorResponse operatorResponse in operatorList)
                {
                    OperatorResponse? expectedOperator = expectedOperatorResponses.SingleOrDefault(s => s.Item3.OperatorId == operatorResponse.OperatorId).Item3;
                    expectedOperator.ShouldNotBeNull();
                    operatorResponse.RequireCustomMerchantNumber.ShouldBe(expectedOperator.RequireCustomMerchantNumber);
                    operatorResponse.RequireCustomTerminalNumber.ShouldBe(expectedOperator.RequireCustomTerminalNumber);
                    operatorResponse.Name.ShouldBe(expectedOperator.Name);
                }
            });
        }
    }
    public async Task WhenIGetTheEstateTheEstateOperatorDetailsAreReturnedAsFollows(String accessToken, String estateName, List<EstateDetails> estateDetailsList, List<String> expectedOperators)
    {
        Guid estateId = Guid.NewGuid();
        EstateDetails estateDetails = estateDetailsList.SingleOrDefault(e => e.EstateName == estateName);
        estateDetails.ShouldNotBeNull();

        String token = accessToken;
        if (estateDetails != null)
        {
            estateId = estateDetails.EstateId;
            if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
            {
                token = estateDetails.AccessToken;
            }
        }

        Result<List<EstateResponse>>? getEstatesResult = await this.TransactionProcessorClient.GetEstates(token, estateId, CancellationToken.None).ConfigureAwait(false);
        getEstatesResult.IsSuccess.ShouldBeTrue();
        getEstatesResult.Data.ShouldNotBeEmpty();
        var estate = getEstatesResult.Data.Single();
        foreach (String expectedOperator in expectedOperators)
        {
            EstateOperatorResponse? op = estate.Operators.SingleOrDefault(o => o.Name == expectedOperator);
            op.ShouldNotBeNull();
        }
    }

    public async Task WhenIGetTheEstateTheEstateSecurityUserDetailsAreReturnedAsFollows(String accessToken, String estateName, List<EstateDetails> estateDetailsList, List<String> expectedSecurityUsers)
    {
        Guid estateId = Guid.NewGuid();
        EstateDetails estateDetails = estateDetailsList.SingleOrDefault(e => e.EstateName == estateName);
        estateDetails.ShouldNotBeNull();

        String token = accessToken;
        if (estateDetails != null)
        {
            estateId = estateDetails.EstateId;
            if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
            {
                token = estateDetails.AccessToken;
            }
        }

        EstateResponse? estate = await this.TransactionProcessorClient.GetEstate(token, estateId, CancellationToken.None).ConfigureAwait(false);
        estate.ShouldNotBeNull();
        foreach (String expectedSecurityUser in expectedSecurityUsers)
        {
            var user = estate.SecurityUsers.SingleOrDefault(o => o.EmailAddress == expectedSecurityUser);
            user.ShouldNotBeNull();
        }
    }

    public async Task WhenIGetTheEstateAnErrorIsReturned(String accessToken, String estateName, List<EstateDetails> estateDetailsList)
    {
        Guid estateId = Guid.NewGuid();
        var result = await this.TransactionProcessorClient.GetEstate(accessToken, estateId, CancellationToken.None).ConfigureAwait(false);
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    public async Task WhenIRemoveTheOperatorFromEstateTheOperatorIsRemoved(String accessToken, List<EstateDetails> estateDetailsList, String estateName, String operatorName)
    {
        EstateDetails estateDetails = estateDetailsList.SingleOrDefault(e => e.EstateName == estateName);
        estateDetails.ShouldNotBeNull();
        Guid operatorId = estateDetails.GetOperatorId(operatorName);

        await this.TransactionProcessorClient.RemoveOperatorFromEstate(accessToken, estateDetails.EstateId, operatorId, CancellationToken.None);

        await Retry.For(async () => {
            EstateResponse? estateResponse = await this.TransactionProcessorClient.GetEstate(accessToken, estateDetails.EstateId, CancellationToken.None);
            estateResponse.ShouldNotBeNull();

            EstateOperatorResponse? operatorResponse = estateResponse.Operators.SingleOrDefault(c => c.OperatorId == operatorId);
            operatorResponse.ShouldNotBeNull();
            operatorResponse.IsDeleted.ShouldBeTrue();
        });
    }

    public async Task<List<OperatorResponse>> WhenIUpdateTheOperatorsWithTheFollowingDetails(string accessToken, List<(EstateDetails estate, Guid operatorId, UpdateOperatorRequest request)> requests)
    {
        List<OperatorResponse> responses = new List<OperatorResponse>();

        foreach ((EstateDetails estate, Guid operatorId, UpdateOperatorRequest request) request in requests)
        {
            await this.TransactionProcessorClient.UpdateOperator(accessToken, request.estate.EstateId, request.operatorId, request.request, CancellationToken.None).ConfigureAwait(false);
        }

        foreach ((EstateDetails estate, Guid operatorId, UpdateOperatorRequest request) request in requests)
        {
            await Retry.For(async () => {
                OperatorResponse? operatorResponse = await this.TransactionProcessorClient
                    .GetOperator(accessToken, request.estate.EstateId, request.operatorId, CancellationToken.None)
                    .ConfigureAwait(false);

                operatorResponse.Name.ShouldBe(request.request.Name);
                operatorResponse.RequireCustomTerminalNumber.ShouldBe(request.request.RequireCustomTerminalNumber.Value);
                operatorResponse.RequireCustomMerchantNumber.ShouldBe(request.request.RequireCustomMerchantNumber.Value);
                responses.Add(operatorResponse);
            });
            request.estate.UpdateOperator(request.operatorId, request.request.Name);
        }

        return responses;
    }
}