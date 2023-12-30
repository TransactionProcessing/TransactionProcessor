using System;
using System.Collections.Generic;

namespace TransactionProcessor.IntegrationTests.Shared
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using DataTransferObjects;
    using EstateManagement.DataTransferObjects.Requests;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.IntegrationTesting.Helpers;
    using IntegrationTesting.Helpers;
    using SecurityService.DataTransferObjects.Requests;
    using SecurityService.IntegrationTesting.Helpers;
    using Shouldly;
    using TechTalk.SpecFlow;
    using ClientDetails = Common.ClientDetails;
    using Contract = Common.Contract;
    using MerchantBalanceResponse = DataTransferObjects.MerchantBalanceResponse;
    using Product = Common.Product;
    using SpecflowExtensions = IntegrationTesting.Helpers.SpecflowExtensions;
    using Table = TechTalk.SpecFlow.Table;

    [Binding]
    [Scope(Tag = "shared")]
    public partial class SharedSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        private readonly SecurityServiceSteps SecurityServiceSteps;

        private readonly EstateManagementSteps EstateManagementSteps;
        private readonly TransactionProcessorSteps TransactionProcessorSteps;

        public SharedSteps(ScenarioContext scenarioContext,
                           TestingContext testingContext) {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(testingContext.DockerHelper.SecurityServiceClient);
            this.EstateManagementSteps = new EstateManagementSteps(testingContext.DockerHelper.EstateClient, testingContext.DockerHelper.TestHostHttpClient);
            this.TransactionProcessorSteps = new TransactionProcessorSteps(testingContext.DockerHelper.TransactionProcessorClient, testingContext.DockerHelper.TestHostHttpClient);
        }
        
        [Given(@"I have a token to access the estate management and transaction processor resources")]
        public async Task GivenIHaveATokenToAccessTheEstateManagementAndTransactionProcessorResources(Table table)
        {
            TableRow firstRow = table.Rows.First();
            String clientId = SpecflowTableHelper.GetStringRowValue(firstRow, "ClientId");
            ClientDetails clientDetails = this.TestingContext.GetClientDetails(clientId);

            this.TestingContext.AccessToken = await this.SecurityServiceSteps.GetClientToken(clientDetails.ClientId, clientDetails.ClientSecret, CancellationToken.None);
        }
        
        
    }

    //[Binding]
    [Scope(Tag = "shared")]
    public partial class SharedSteps{

        [When(@"I get the completed settlements the following information should be returned")]
        public async Task WhenIGetTheCompletedSettlementsTheFollowingInformationShouldBeReturned(Table table)
        {
            List<(EstateDetails, Guid, DateTime, Int32)> requests = table.Rows.ToCompletedSettlementRequests(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIGetTheCompletedSettlementsTheFollowingInformationShouldBeReturned(this.TestingContext.AccessToken, requests);
        }

        [When(@"I get the pending settlements the following information should be returned")]
        public async Task WhenIGetThePendingSettlementsTheFollowingInformationShouldBeReturned(Table table)
        {
            List<(EstateDetails, Guid, DateTime, Int32)> requests = table.Rows.ToPendingSettlementRequests(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIGetThePendingSettlementsTheFollowingInformationShouldBeReturned(this.TestingContext.AccessToken, requests);
        }

        [When(@"I process the settlement for '([^']*)' on Estate '([^']*)' for Merchant '([^']*)' then (.*) fees are marked as settled and the settlement is completed")]
        public async Task WhenIProcessTheSettlementForOnEstateThenFeesAreMarkedAsSettledAndTheSettlementIsCompleted(String dateString, String estateName, String merchantName, Int32 numberOfFeesSettled)
        {
            DateTime settlementDate = SpecflowTableHelper.GetDateForDateString(dateString, DateTime.UtcNow.Date);

            EstateDetails estateDetails = this.TestingContext.GetEstateDetails(estateName);
            Guid merchantId = estateDetails.GetMerchantId(merchantName);

            SpecflowExtensions.ProcessSettlementRequest processSettlementRequest = SpecflowExtensions.ToProcessSettlementRequest(dateString, estateName, merchantName, this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIProcessTheSettlementForOnEstateThenFeesAreMarkedAsSettledAndTheSettlementIsCompleted(this.TestingContext.AccessToken, processSettlementRequest, numberOfFeesSettled);
        }

        [When(@"I redeem the voucher for Estate '([^']*)' and Merchant '([^']*)' transaction number (.*) the voucher balance will be (.*)")]
        public async Task WhenIRedeemTheVoucherForEstateAndMerchantTransactionNumberTheVoucherBalanceWillBe(string estateName, string merchantName, int transactionNumber, int balance)
        {
            (EstateDetails, SaleTransactionResponse) saleResponse = SpecflowExtensions.GetVoucherByTransactionNumber(estateName, merchantName, transactionNumber, this.TestingContext.Estates);

            GetVoucherResponse voucher = await this.TransactionProcessorSteps.GetVoucherByTransactionNumber(this.TestingContext.AccessToken, saleResponse.Item1, saleResponse.Item2);

            await this.TransactionProcessorSteps.RedeemVoucher(this.TestingContext.AccessToken, saleResponse.Item1, voucher, balance);
        }

        [Given(@"the following bills are available at the PataPawa PostPaid Host")]
        public async Task GivenTheFollowingBillsAreAvailableAtThePataPawaPostPaidHost(Table table)
        {
            List<SpecflowExtensions.PataPawaBill> bills = table.Rows.ToPataPawaBills();
            await this.TransactionProcessorSteps.GivenTheFollowingBillsAreAvailableAtThePataPawaPostPaidHost(bills);
        }

        [Given(@"I have created the following estates")]
        [When(@"I create the following estates")]
        public async Task WhenICreateTheFollowingEstates(Table table)
        {
            List<CreateEstateRequest> requests = table.Rows.ToCreateEstateRequests();

            foreach (CreateEstateRequest request in requests)
            {
                // Setup the subscriptions for the estate
                await Retry.For(async () => {
                    await this.TestingContext.DockerHelper
                              .CreateEstateSubscriptions(request.EstateName)
                              .ConfigureAwait(false);
                },
                                retryFor: TimeSpan.FromMinutes(2),
                                retryInterval: TimeSpan.FromSeconds(30));
            }

            List<EstateResponse> verifiedEstates = await this.EstateManagementSteps.WhenICreateTheFollowingEstates(this.TestingContext.AccessToken, requests);

            foreach (EstateResponse verifiedEstate in verifiedEstates)
            {
                this.TestingContext.AddEstateDetails(verifiedEstate.EstateId, verifiedEstate.EstateName, verifiedEstate.EstateReference);
                this.TestingContext.Logger.LogInformation($"Estate {verifiedEstate.EstateName} created with Id {verifiedEstate.EstateId}");


            }
        }

        [Given(@"I create the following api scopes")]
        public async Task GivenICreateTheFollowingApiScopes(Table table)
        {
            List<CreateApiScopeRequest> requests = table.Rows.ToCreateApiScopeRequests();
            await this.SecurityServiceSteps.GivenICreateTheFollowingApiScopes(requests);
        }

        [Given(@"I have assigned the following  operator to the merchants")]
        [When(@"I assign the following  operator to the merchants")]
        public async Task WhenIAssignTheFollowingOperatorToTheMerchants(Table table)
        {
            List<(EstateDetails, Guid, AssignOperatorRequest)> requests = table.Rows.ToAssignOperatorRequests(this.TestingContext.Estates);

            List<(EstateDetails, MerchantOperatorResponse)> results = await this.EstateManagementSteps.WhenIAssignTheFollowingOperatorToTheMerchants(this.TestingContext.AccessToken, requests);

            foreach ((EstateDetails, MerchantOperatorResponse) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Operator {result.Item2.Name} assigned to Estate {result.Item1.EstateName}");
            }
        }


        [Given(@"I have created the following operators")]
        [When(@"I create the following operators")]
        public async Task WhenICreateTheFollowingOperators(Table table)
        {

            List<(EstateDetails estate, CreateOperatorRequest request)> requests = table.Rows.ToCreateOperatorRequests(this.TestingContext.Estates);

            List<(Guid, EstateOperatorResponse)> results = await this.EstateManagementSteps.WhenICreateTheFollowingOperators(this.TestingContext.AccessToken, requests);

            foreach ((Guid, EstateOperatorResponse) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Operator {result.Item2.Name} created with Id {result.Item2.OperatorId} for Estate {result.Item1}");
            }
        }

        [Given("I create the following merchants")]
        [When(@"I create the following merchants")]
        public async Task WhenICreateTheFollowingMerchants(Table table)
        {
            List<(EstateDetails estate, CreateMerchantRequest)> requests = table.Rows.ToCreateMerchantRequests(this.TestingContext.Estates);

            List<MerchantResponse> verifiedMerchants = await this.EstateManagementSteps.WhenICreateTheFollowingMerchants(this.TestingContext.AccessToken, requests);

            foreach (MerchantResponse verifiedMerchant in verifiedMerchants)
            {
                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(verifiedMerchant.EstateId);
                estateDetails.AddMerchant(verifiedMerchant);
                this.TestingContext.Logger.LogInformation($"Merchant {verifiedMerchant.MerchantName} created with Id {verifiedMerchant.MerchantId} for Estate {estateDetails.EstateName}");
            }
        }

        [When(@"I perform the following transactions")]
        public async Task WhenIPerformTheFollowingTransactions(Table table)
        {
            List<(EstateDetails, Guid, String, SerialisedMessage)> serialisedMessages = table.Rows.ToSerialisedMessages(this.TestingContext.Estates);

            await this.TransactionProcessorSteps.WhenIPerformTheFollowingTransactions(this.TestingContext.AccessToken, serialisedMessages);
        }

        [Given(@"I create a contract with the following values")]
        public async Task GivenICreateAContractWithTheFollowingValues(Table table)
        {

            List<(EstateDetails, CreateContractRequest)> requests = table.Rows.ToCreateContractRequests(this.TestingContext.Estates);
            List<ContractResponse> responses = await this.EstateManagementSteps.GivenICreateAContractWithTheFollowingValues(this.TestingContext.AccessToken, requests);
        }

        [When(@"I create the following Products")]
        public async Task WhenICreateTheFollowingProducts(Table table)
        {
            List<(EstateDetails, EstateManagement.IntegrationTesting.Helpers.Contract, AddProductToContractRequest)> requests = table.Rows.ToAddProductToContractRequest(this.TestingContext.Estates);
            await this.EstateManagementSteps.WhenICreateTheFollowingProducts(this.TestingContext.AccessToken, requests);
        }

        [When(@"I add the following Transaction Fees")]
        public async Task WhenIAddTheFollowingTransactionFees(Table table)
        {
            List<(EstateDetails, EstateManagement.IntegrationTesting.Helpers.Contract, EstateManagement.IntegrationTesting.Helpers.Product, AddTransactionFeeForProductToContractRequest)> requests = table.Rows.ToAddTransactionFeeForProductToContractRequests(this.TestingContext.Estates);
            await this.EstateManagementSteps.WhenIAddTheFollowingTransactionFees(this.TestingContext.AccessToken, requests);
        }

        [Then(@"transaction response should contain the following information")]
        public void ThenTransactionResponseShouldContainTheFollowingInformation(Table table)
        {
            List<(SerialisedMessage, String, String, String)> transactions = table.Rows.GetTransactionDetails(this.TestingContext.Estates);
            this.TransactionProcessorSteps.ValidateTransactions(transactions);
        }

        [When(@"I request the receipt is resent")]
        public async Task WhenIRequestTheReceiptIsResent(Table table)
        {
            List<SerialisedMessage> transactions = table.Rows.GetTransactionResendDetails(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIRequestTheReceiptIsResent(this.TestingContext.AccessToken, transactions);
        }

        [Given(@"the following api resources exist")]
        public async Task GivenTheFollowingApiResourcesExist(Table table)
        {
            List<CreateApiResourceRequest> requests = table.Rows.ToCreateApiResourceRequests();
            await this.SecurityServiceSteps.GivenTheFollowingApiResourcesExist(requests);
        }

        [Given(@"the following clients exist")]
        public async Task GivenTheFollowingClientsExist(Table table)
        {
            List<CreateClientRequest> requests = table.Rows.ToCreateClientRequests();
            List<(String clientId, String secret, List<String> allowedGrantTypes)> clients = await this.SecurityServiceSteps.GivenTheFollowingClientsExist(requests);
            foreach ((String clientId, String secret, List<String> allowedGrantTypes) client in clients)
            {
                this.TestingContext.AddClientDetails(client.clientId, client.secret, String.Join(",", client.allowedGrantTypes));
            }
        }

        [Given(@"I have assigned the following devices to the merchants")]
        public async Task GivenIHaveAssignedTheFollowingDevicesToTheMerchants(Table table)
        {

            List<(EstateDetails, Guid, AddMerchantDeviceRequest)> requests = table.Rows.ToAddMerchantDeviceRequests(this.TestingContext.Estates);

            List<(EstateDetails, MerchantResponse, String)> results = await this.EstateManagementSteps.GivenIHaveAssignedTheFollowingDevicesToTheMerchants(this.TestingContext.AccessToken, requests);
            foreach ((EstateDetails, MerchantResponse, String) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Device {result.Item3} assigned to Merchant {result.Item2.MerchantName} Estate {result.Item1.EstateName}");
            }
        }

        [When(@"I add the following contracts to the following merchants")]
        public async Task WhenIAddTheFollowingContractsToTheFollowingMerchants(Table table)
        {
            List<(EstateDetails, Guid, Guid)> requests = table.Rows.ToAddContractToMerchantRequests(this.TestingContext.Estates);
            await this.EstateManagementSteps.WhenIAddTheFollowingContractsToTheFollowingMerchants(this.TestingContext.AccessToken, requests);
        }

        [Given(@"I make the following manual merchant deposits")]
        public async Task GivenIMakeTheFollowingManualMerchantDeposits(Table table)
        {
            List<(EstateDetails, Guid, MakeMerchantDepositRequest)> requests = table.Rows.ToMakeMerchantDepositRequest(this.TestingContext.Estates);

            foreach ((EstateDetails, Guid, MakeMerchantDepositRequest) request in requests)
            {
                MerchantBalanceResponse previousMerchantBalance = await this.TestingContext.DockerHelper.TransactionProcessorClient.GetMerchantBalance(this.TestingContext.AccessToken,
                                                                                                                                                       request.Item1.EstateId, request.Item2, CancellationToken.None);

                await this.EstateManagementSteps.GivenIMakeTheFollowingManualMerchantDeposits(this.TestingContext.AccessToken, request);

                await Retry.For(async () => {
                    MerchantBalanceResponse currentMerchantBalance = await this.TestingContext.DockerHelper.TransactionProcessorClient.GetMerchantBalance(this.TestingContext.AccessToken, request.Item1.EstateId, request.Item2, CancellationToken.None);
                    currentMerchantBalance.AvailableBalance.ShouldBe(previousMerchantBalance.AvailableBalance + request.Item3.Amount);

                    this.TestingContext.Logger.LogInformation($"Deposit Reference {request.Item3.Reference} made for Merchant Id {request.Item2}");
                });
            }
        }

        [When(@"I perform the following reconciliations")]
        public async Task WhenIPerformTheFollowingReconciliations(Table table)
        {
            List<(EstateDetails, Guid, String, SerialisedMessage)> serialisedMessages = table.Rows.ToSerialisedMessages(this.TestingContext.Estates);

            await this.TransactionProcessorSteps.WhenIPerformTheFollowingTransactions(this.TestingContext.AccessToken, serialisedMessages);
        }

        [Then(@"reconciliation response should contain the following information")]
        public void ThenReconciliationResponseShouldContainTheFollowingInformation(Table table)
        {
            List<(SerialisedMessage, String, String, String)> transactions = table.Rows.GetTransactionDetails(this.TestingContext.Estates);
            this.TransactionProcessorSteps.ValidateTransactions(transactions);
        }

        [Then(@"the following entries appear in the merchants balance history for estate '([^']*)' and merchant '([^']*)'")]
        public async Task ThenTheFollowingEntriesAppearInTheMerchantsBalanceHistoryForEstateAndMerchant(String estateName, String merchantName, Table table)
        {
            DateTime startDate = SpecflowTableHelper.GetDateForDateString("Today", DateTime.UtcNow).AddDays(-1);
            DateTime endDate = SpecflowTableHelper.GetDateForDateString("Today", DateTime.UtcNow).AddDays(1);
            List<SpecflowExtensions.BalanceEntry> balanceEntries = table.Rows.ToBalanceEntries(estateName, merchantName, this.TestingContext.Estates);

            await this.TransactionProcessorSteps.ThenTheFollowingEntriesAppearInTheMerchantsBalanceHistoryForEstateAndMerchant(this.TestingContext.AccessToken,
                                                                                                                               startDate,
                                                                                                                               endDate,
                                                                                                                               balanceEntries);
        }
    }
}
