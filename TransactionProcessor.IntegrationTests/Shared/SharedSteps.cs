﻿using System;
using System.Collections.Generic;

namespace TransactionProcessor.IntegrationTests.Shared
{
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using DataTransferObjects;
    using EstateManagement.DataTransferObjects.Requests;
    using EstateManagement.DataTransferObjects.Requests.Contract;
    using EstateManagement.DataTransferObjects.Requests.Estate;
    using EstateManagement.DataTransferObjects.Requests.Merchant;
    using EstateManagement.DataTransferObjects.Requests.Operator;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.DataTransferObjects.Responses.Contract;
    using EstateManagement.DataTransferObjects.Responses.Estate;
    using EstateManagement.IntegrationTesting.Helpers;
    using IntegrationTesting.Helpers;
    using Newtonsoft.Json.Linq;
    using Reqnroll;
    using SecurityService.DataTransferObjects.Requests;
    using SecurityService.IntegrationTesting.Helpers;
    using Shouldly;
    using AssignOperatorRequest = EstateManagement.DataTransferObjects.Requests.Estate.AssignOperatorRequest;
    using ClientDetails = Common.ClientDetails;
    using Contract = Common.Contract;
    using MerchantBalanceResponse = DataTransferObjects.MerchantBalanceResponse;
    using MerchantOperatorResponse = EstateManagement.DataTransferObjects.Responses.Merchant.MerchantOperatorResponse;
    using MerchantResponse = EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse;
    using Product = Common.Product;
    using ReqnrollExtensions = IntegrationTesting.Helpers.ReqnrollExtensions;
    
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
            this.EstateManagementSteps = new EstateManagementSteps(testingContext.DockerHelper.EstateClient.EstateClient, testingContext.DockerHelper.TestHostHttpClient);
            this.TransactionProcessorSteps = new TransactionProcessorSteps(testingContext.DockerHelper.TransactionProcessorClient, testingContext.DockerHelper.TestHostHttpClient,
                testingContext.DockerHelper.ProjectionManagementClient);
        }
        
        [Given(@"I have a token to access the estate management and transaction processor resources")]
        public async Task GivenIHaveATokenToAccessTheEstateManagementAndTransactionProcessorResources(DataTable table)
        {
            DataTableRow firstRow = table.Rows.First();
            String clientId = ReqnrollTableHelper.GetStringRowValue(firstRow, "ClientId");
            ClientDetails clientDetails = this.TestingContext.GetClientDetails(clientId);

            this.TestingContext.AccessToken = await this.SecurityServiceSteps.GetClientToken(clientDetails.ClientId, clientDetails.ClientSecret, CancellationToken.None);
        }
        
        
    }

    [Binding]
    [Scope(Tag = "shared")]
    public partial class SharedSteps{

        [When(@"I get the completed settlements the following information should be returned")]
        public async Task WhenIGetTheCompletedSettlementsTheFollowingInformationShouldBeReturned(DataTable table)
        {
            List<(EstateDetails, Guid, DateTime, Int32)> requests = table.Rows.ToCompletedSettlementRequests(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIGetTheCompletedSettlementsTheFollowingInformationShouldBeReturned(this.TestingContext.AccessToken, requests);
        }

        [When(@"I get the pending settlements the following information should be returned")]
        public async Task WhenIGetThePendingSettlementsTheFollowingInformationShouldBeReturned(DataTable table)
        {
            List<(EstateDetails, Guid, DateTime, Int32)> requests = table.Rows.ToPendingSettlementRequests(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIGetThePendingSettlementsTheFollowingInformationShouldBeReturned(this.TestingContext.AccessToken, requests);
        }

        [When(@"I process the settlement for '([^']*)' on Estate '([^']*)' for Merchant '([^']*)' then (.*) fees are marked as settled and the settlement is completed")]
        public async Task WhenIProcessTheSettlementForOnEstateThenFeesAreMarkedAsSettledAndTheSettlementIsCompleted(String dateString, String estateName, String merchantName, Int32 numberOfFeesSettled)
        {
            DateTime settlementDate = ReqnrollTableHelper.GetDateForDateString(dateString, DateTime.UtcNow.Date);

            EstateDetails estateDetails = this.TestingContext.GetEstateDetails(estateName);
            Guid merchantId = estateDetails.GetMerchantId(merchantName);

            ReqnrollExtensions.ProcessSettlementRequest processSettlementRequest = ReqnrollExtensions.ToProcessSettlementRequest(dateString, estateName, merchantName, this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIProcessTheSettlementForOnEstateThenFeesAreMarkedAsSettledAndTheSettlementIsCompleted(this.TestingContext.AccessToken, processSettlementRequest, numberOfFeesSettled);
        }

        [When(@"I redeem the voucher for Estate '([^']*)' and Merchant '([^']*)' transaction number (.*) the voucher balance will be (.*)")]
        public async Task WhenIRedeemTheVoucherForEstateAndMerchantTransactionNumberTheVoucherBalanceWillBe(string estateName, string merchantName, int transactionNumber, int balance)
        {
            (EstateDetails, SaleTransactionResponse) saleResponse = ReqnrollExtensions.GetVoucherByTransactionNumber(estateName, merchantName, transactionNumber, this.TestingContext.Estates);

            GetVoucherResponse voucher = await this.TransactionProcessorSteps.GetVoucherByTransactionNumber(this.TestingContext.AccessToken, saleResponse.Item1, saleResponse.Item2);

            await this.TransactionProcessorSteps.RedeemVoucher(this.TestingContext.AccessToken, saleResponse.Item1, voucher, balance);
        }

        [Given(@"the following bills are available at the PataPawa PostPaid Host")]
        public async Task GivenTheFollowingBillsAreAvailableAtThePataPawaPostPaidHost(DataTable table)
        {
            List<ReqnrollExtensions.PataPawaBill> bills = table.Rows.ToPataPawaBills();
            await this.TransactionProcessorSteps.GivenTheFollowingBillsAreAvailableAtThePataPawaPostPaidHost(bills);
        }

        [Given(@"the following users are available at the PataPawa PrePay Host")]
        public async Task GivenTheFollowingUsersAreAvailableAtThePataPawaPrePayHost(DataTable table)
        {
            List<ReqnrollExtensions.PataPawaUser> users = table.Rows.ToPataPawaUsers();
            await this.TransactionProcessorSteps.GivenTheFollowingUsersAreAvailableAtThePataPawaPrePaidHost(users);
        }

        [Given(@"the following meters are available at the PataPawa PrePay Host")]
        public async Task GivenTheFollowingMetersAreAvailableAtThePataPawaPrePayHost(DataTable table){
            List<ReqnrollExtensions.PataPawaMeter> meters = table.Rows.ToPataPawaMeters();
            await this.TransactionProcessorSteps.GivenTheFollowingMetersAreAvailableAtThePataPawaPrePaidHost(meters);
        }
        
        [Given(@"I have created the following estates")]
        [When(@"I create the following estates")]
        public async Task WhenICreateTheFollowingEstates(DataTable table)
        {
            List<CreateEstateRequest> requests = table.Rows.ToCreateEstateRequests();

            List<EstateResponse> verifiedEstates = await this.EstateManagementSteps.WhenICreateTheFollowingEstates(this.TestingContext.AccessToken, requests);

            foreach (EstateResponse verifiedEstate in verifiedEstates)
            {
                this.TestingContext.AddEstateDetails(verifiedEstate.EstateId, verifiedEstate.EstateName, verifiedEstate.EstateReference);
                this.TestingContext.Logger.LogInformation($"Estate {verifiedEstate.EstateName} created with Id {verifiedEstate.EstateId}");
            }
        }

        [Given(@"I create the following api scopes")]
        public async Task GivenICreateTheFollowingApiScopes(DataTable table)
        {
            List<CreateApiScopeRequest> requests = table.Rows.ToCreateApiScopeRequests();
            await this.SecurityServiceSteps.GivenICreateTheFollowingApiScopes(requests);
        }

        [Given(@"I have assigned the following  operator to the merchants")]
        [When(@"I assign the following  operator to the merchants")]
        public async Task WhenIAssignTheFollowingOperatorToTheMerchants(DataTable table)
        {
            List<(EstateDetails, Guid, EstateManagement.DataTransferObjects.Requests.Merchant.AssignOperatorRequest)> requests = table.Rows.ToAssignOperatorRequests(this.TestingContext.Estates);

            List<(EstateDetails, EstateManagement.DataTransferObjects.Responses.Merchant.MerchantOperatorResponse)> results = await this.EstateManagementSteps.WhenIAssignTheFollowingOperatorToTheMerchants(this.TestingContext.AccessToken, requests);

            foreach ((EstateDetails, MerchantOperatorResponse) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Operator {result.Item2.Name} assigned to Estate {result.Item1.EstateName}");
            }
        }


        [Given(@"I have created the following operators")]
        [When(@"I create the following operators")]
        public async Task WhenICreateTheFollowingOperators(DataTable table)
        {

            List<(EstateDetails estate, CreateOperatorRequest request)> requests = table.Rows.ToCreateOperatorRequests(this.TestingContext.Estates);

            List<(Guid, EstateOperatorResponse)> results = await this.EstateManagementSteps.WhenICreateTheFollowingOperators(this.TestingContext.AccessToken, requests);

            foreach ((Guid, EstateOperatorResponse) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Operator {result.Item2.Name} created with Id {result.Item2.OperatorId} for Estate {result.Item1}");
            }
        }

        [Given("I have assigned the following operators to the estates")]
        public async Task GivenIHaveAssignedTheFollowingOperatorsToTheEstates(DataTable dataTable)
        {
            List<(EstateDetails estate, AssignOperatorRequest request)> requests = dataTable.Rows.ToAssignOperatorToEstateRequests(this.TestingContext.Estates);

            await this.EstateManagementSteps.GivenIHaveAssignedTheFollowingOperatorsToTheEstates(this.TestingContext.AccessToken, requests);

            // TODO Verify
        }

        [Given("I create the following merchants")]
        [When(@"I create the following merchants")]
        public async Task WhenICreateTheFollowingMerchants(DataTable table)
        {
            List<(EstateDetails estate, CreateMerchantRequest)> requests = table.Rows.ToCreateMerchantRequests(this.TestingContext.Estates);

            List<EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse> verifiedMerchants = await this.EstateManagementSteps.WhenICreateTheFollowingMerchants(this.TestingContext.AccessToken, requests);

            foreach (EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse verifiedMerchant in verifiedMerchants){
                await this.TransactionProcessorSteps.WhenICreateTheFollowingMerchants(this.TestingContext.AccessToken, verifiedMerchant.EstateId, verifiedMerchant.MerchantId);

                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(verifiedMerchant.EstateId);
                estateDetails.AddMerchant(verifiedMerchant);
                this.TestingContext.Logger.LogInformation($"Merchant {verifiedMerchant.MerchantName} created with Id {verifiedMerchant.MerchantId} for Estate {estateDetails.EstateName}");
            }
        }

        [When(@"I perform the following transactions")]
        public async Task WhenIPerformTheFollowingTransactions(DataTable table)
        {
            List<(EstateDetails, Guid, String, SerialisedMessage)> serialisedMessages = table.Rows.ToSerialisedMessages(this.TestingContext.Estates);

            await this.TransactionProcessorSteps.WhenIPerformTheFollowingTransactions(this.TestingContext.AccessToken, serialisedMessages);
        }

        [Given(@"I create a contract with the following values")]
        public async Task GivenICreateAContractWithTheFollowingValues(DataTable table)
        {

            List<(EstateDetails, CreateContractRequest)> requests = table.Rows.ToCreateContractRequests(this.TestingContext.Estates);
            List<ContractResponse> responses = await this.EstateManagementSteps.GivenICreateAContractWithTheFollowingValues(this.TestingContext.AccessToken, requests);
            foreach (ContractResponse contractResponse in responses) {
                EstateDetails estate = this.TestingContext.Estates.Single(e => e.EstateId == contractResponse.EstateId);
                estate.AddContract(contractResponse.ContractId, contractResponse.Description, contractResponse.OperatorId);
            }
        }

        [When(@"I create the following Products")]
        public async Task WhenICreateTheFollowingProducts(DataTable table)
        {
            List<(EstateDetails, EstateManagement.IntegrationTesting.Helpers.Contract, AddProductToContractRequest)> requests = table.Rows.ToAddProductToContractRequest(this.TestingContext.Estates);
            await this.EstateManagementSteps.WhenICreateTheFollowingProducts(this.TestingContext.AccessToken, requests);
        }

        [When(@"I add the following Transaction Fees")]
        public async Task WhenIAddTheFollowingTransactionFees(DataTable table)
        {
            List<(EstateDetails, EstateManagement.IntegrationTesting.Helpers.Contract, EstateManagement.IntegrationTesting.Helpers.Product, AddTransactionFeeForProductToContractRequest)> requests = table.Rows.ToAddTransactionFeeForProductToContractRequests(this.TestingContext.Estates);
            await this.EstateManagementSteps.WhenIAddTheFollowingTransactionFees(this.TestingContext.AccessToken, requests);
        }

        [Then(@"transaction response should contain the following information")]
        public void ThenTransactionResponseShouldContainTheFollowingInformation(DataTable table)
        {
            List<(SerialisedMessage, String, String, String)> transactions = table.Rows.GetTransactionDetails(this.TestingContext.Estates);
            this.TransactionProcessorSteps.ValidateTransactions(transactions);
        }

        [When(@"I request the receipt is resent")]
        public async Task WhenIRequestTheReceiptIsResent(DataTable table)
        {
            List<SerialisedMessage> transactions = table.Rows.GetTransactionResendDetails(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIRequestTheReceiptIsResent(this.TestingContext.AccessToken, transactions);
        }

        [Given(@"the following api resources exist")]
        public async Task GivenTheFollowingApiResourcesExist(DataTable table)
        {
            List<CreateApiResourceRequest> requests = table.Rows.ToCreateApiResourceRequests();
            await this.SecurityServiceSteps.GivenTheFollowingApiResourcesExist(requests);
        }

        [Given(@"the following clients exist")]
        public async Task GivenTheFollowingClientsExist(DataTable table)
        {
            List<CreateClientRequest> requests = table.Rows.ToCreateClientRequests();
            List<(String clientId, String secret, List<String> allowedGrantTypes)> clients = await this.SecurityServiceSteps.GivenTheFollowingClientsExist(requests);
            foreach ((String clientId, String secret, List<String> allowedGrantTypes) client in clients)
            {
                this.TestingContext.AddClientDetails(client.clientId, client.secret, String.Join(",", client.allowedGrantTypes));
            }
        }

        [Given(@"I have assigned the following devices to the merchants")]
        public async Task GivenIHaveAssignedTheFollowingDevicesToTheMerchants(DataTable table)
        {

            List<(EstateDetails, Guid, AddMerchantDeviceRequest)> requests = table.Rows.ToAddMerchantDeviceRequests(this.TestingContext.Estates);

            List<(EstateDetails, EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse, String)> results = await this.EstateManagementSteps.GivenIHaveAssignedTheFollowingDevicesToTheMerchants(this.TestingContext.AccessToken, requests);
            foreach ((EstateDetails, MerchantResponse, String) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Device {result.Item3} assigned to Merchant {result.Item2.MerchantName} Estate {result.Item1.EstateName}");
            }
        }

        [When(@"I add the following contracts to the following merchants")]
        public async Task WhenIAddTheFollowingContractsToTheFollowingMerchants(DataTable table)
        {
            List<(EstateDetails, Guid, Guid)> requests = table.Rows.ToAddContractToMerchantRequests(this.TestingContext.Estates);
            await this.EstateManagementSteps.WhenIAddTheFollowingContractsToTheFollowingMerchants(this.TestingContext.AccessToken, requests);
        }

        private async Task<Decimal> GetMerchantBalance(Guid merchantId)
        {
            JsonElement jsonElement = (JsonElement)await this.TestingContext.DockerHelper.ProjectionManagementClient.GetStateAsync<dynamic>("MerchantBalanceProjection", $"MerchantBalance-{merchantId:N}");
            JObject jsonObject = JObject.Parse(jsonElement.GetRawText());
            decimal balanceValue = jsonObject.SelectToken("merchant.balance").Value<decimal>();
            return balanceValue;
        }

        [Given(@"I make the following manual merchant deposits")]
        public async Task GivenIMakeTheFollowingManualMerchantDeposits(DataTable table)
        {
            List<(EstateDetails, Guid, MakeMerchantDepositRequest)> requests = table.Rows.ToMakeMerchantDepositRequest(this.TestingContext.Estates);

            foreach ((EstateDetails, Guid, MakeMerchantDepositRequest) request in requests)
            {
                Decimal previousMerchantBalance = await this.GetMerchantBalance(request.Item2);

                await this.EstateManagementSteps.GivenIMakeTheFollowingManualMerchantDeposits(this.TestingContext.AccessToken, request);

                await Retry.For(async () => {
                                    Decimal currentMerchantBalance = await this.GetMerchantBalance(request.Item2);

                                    currentMerchantBalance.ShouldBe(previousMerchantBalance + request.Item3.Amount);

                    this.TestingContext.Logger.LogInformation($"Deposit Reference {request.Item3.Reference} made for Merchant Id {request.Item2}");
                });
            }
        }

        [When(@"I perform the following reconciliations")]
        public async Task WhenIPerformTheFollowingReconciliations(DataTable table)
        {
            List<(EstateDetails, Guid, String, SerialisedMessage)> serialisedMessages = table.Rows.ToSerialisedMessages(this.TestingContext.Estates);

            await this.TransactionProcessorSteps.WhenIPerformTheFollowingTransactions(this.TestingContext.AccessToken, serialisedMessages);
        }

        [Then(@"reconciliation response should contain the following information")]
        public void ThenReconciliationResponseShouldContainTheFollowingInformation(DataTable table)
        {
            List<(SerialisedMessage, String, String, String)> transactions = table.Rows.GetTransactionDetails(this.TestingContext.Estates);
            this.TransactionProcessorSteps.ValidateTransactions(transactions);
        }

        [Then(@"the following entries appear in the merchants balance history for estate '([^']*)' and merchant '([^']*)'")]
        public async Task ThenTheFollowingEntriesAppearInTheMerchantsBalanceHistoryForEstateAndMerchant(String estateName, String merchantName, DataTable table)
        {
            DateTime startDate = ReqnrollTableHelper.GetDateForDateString("Today", DateTime.UtcNow).AddDays(-1);
            DateTime endDate = ReqnrollTableHelper.GetDateForDateString("Today", DateTime.UtcNow).AddDays(1);
            List<ReqnrollExtensions.BalanceEntry> balanceEntries = table.Rows.ToBalanceEntries(estateName, merchantName, this.TestingContext.Estates);

            await this.TransactionProcessorSteps.ThenTheFollowingEntriesAppearInTheMerchantsBalanceHistoryForEstateAndMerchant(this.TestingContext.AccessToken,
                                                                                                                               startDate,
                                                                                                                               endDate,
                                                                                                                               balanceEntries);
        }
    }
}
