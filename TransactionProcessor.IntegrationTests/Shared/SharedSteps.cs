using System;
using System.Collections.Generic;
using SecurityService.DataTransferObjects.Responses;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Requests.Operator;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Operator;

namespace TransactionProcessor.IntegrationTests.Shared
{
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using DataTransferObjects;
    using IntegrationTesting.Helpers;
    using Newtonsoft.Json.Linq;
    using Reqnroll;
    using SecurityService.DataTransferObjects.Requests;
    using SecurityService.IntegrationTesting.Helpers;
    using Shouldly;
    using AssignOperatorRequest = DataTransferObjects.Requests.Estate.AssignOperatorRequest;
    using ClientDetails = Common.ClientDetails;
    using ReqnrollExtensions = IntegrationTesting.Helpers.ReqnrollExtensions;
    
    [Scope(Tag = "shared")]
    public partial class SharedSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        private readonly SecurityServiceSteps SecurityServiceSteps;

        private readonly TransactionProcessorSteps TransactionProcessorSteps;

        public SharedSteps(ScenarioContext scenarioContext,
                           TestingContext testingContext) {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(testingContext.DockerHelper.SecurityServiceClient);
            this.TransactionProcessorSteps = new TransactionProcessorSteps(testingContext.DockerHelper.TransactionProcessorClient, testingContext.DockerHelper.TestHostHttpClient,
                testingContext.DockerHelper.ProjectionManagementClient);
        }
        
        //[Given(@"I have a token to access the estate management and transaction processor resources")]
        //public async Task GivenIHaveATokenToAccessTheEstateManagementAndTransactionProcessorResources(DataTable table)
        //{
        //    DataTableRow firstRow = table.Rows.First();
        //    String clientId = ReqnrollTableHelper.GetStringRowValue(firstRow, "ClientId");
        //    ClientDetails clientDetails = this.TestingContext.GetClientDetails(clientId);

        //    this.TestingContext.AccessToken = await this.SecurityServiceSteps.GetClientToken(clientDetails.ClientId, clientDetails.ClientSecret, CancellationToken.None);
        //}
        
        
    }

    [Binding]
    [Scope(Tag = "shared")]
    public partial class SharedSteps{
        
        [When(@"I get the merchant ""(.*)"" for estate ""(.*)"" an error is returned")]
        public async Task WhenIGetTheMerchantForEstateAnErrorIsReturned(String merchantName,
                                                                        String estateName)
        {
            await this.TransactionProcessorSteps.WhenIGetTheMerchantForEstateAnErrorIsReturned(this.TestingContext.AccessToken, estateName, merchantName, this.TestingContext.Estates);
        }

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
            List<DataTransferObjects.Requests.Estate.CreateEstateRequest> requests = table.Rows.ToCreateEstateRequests();

            List<DataTransferObjects.Responses.Estate.EstateResponse> verifiedEstates = await this.TransactionProcessorSteps.WhenICreateTheFollowingEstatesX(this.TestingContext.AccessToken, requests);

            foreach (DataTransferObjects.Responses.Estate.EstateResponse verifiedEstate in verifiedEstates)
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

        [Given(@"I have assigned the following operator to the merchants")]
        [When(@"I assign the following operator to the merchants")]
        public async Task WhenIAssignTheFollowingOperatorToTheMerchants(DataTable table)
        {
            foreach (EstateDetails testingContextEstate in this.TestingContext.Estates) {
                var operators = testingContextEstate.GetOperators();
                foreach (KeyValuePair<String, Guid> keyValuePair in operators) {
                    this.TestingContext.Logger.LogInformation($"Operator {keyValuePair.Key} {keyValuePair.Value} assigned to Estate {testingContextEstate.EstateName}");
                }
                var merchants = testingContextEstate.GetMerchants();
                foreach (DataTransferObjects.Responses.Merchant.MerchantResponse merchantResponse in merchants) {
                    this.TestingContext.Logger.LogInformation($"Merchant {merchantResponse.MerchantName} {merchantResponse.MerchantId} assigned to Estate {testingContextEstate.EstateName}");
                }
            }



            List<(EstateDetails, Guid, DataTransferObjects.Requests.Merchant.AssignOperatorRequest)> requests = table.Rows.ToAssignOperatorRequests(this.TestingContext.Estates);

            List<(EstateDetails, DataTransferObjects.Responses.Merchant.MerchantOperatorResponse)> results = await this.TransactionProcessorSteps.WhenIAssignTheFollowingOperatorToTheMerchants(this.TestingContext.AccessToken, requests);

            foreach ((EstateDetails, DataTransferObjects.Responses.Merchant.MerchantOperatorResponse) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Operator {result.Item2.Name} assigned to Estate {result.Item1.EstateName}");
            }
        }


        [Given(@"I have created the following operators")]
        [When(@"I create the following operators")]
        public async Task WhenICreateTheFollowingOperators(DataTable table)
        {

            List<(EstateDetails estate, DataTransferObjects.Requests.Operator.CreateOperatorRequest request)> requests = table.Rows.ToCreateOperatorRequests(this.TestingContext.Estates);

            List<(Guid, DataTransferObjects.Responses.Estate.EstateOperatorResponse)> results = await this.TransactionProcessorSteps.WhenICreateTheFollowingOperators(this.TestingContext.AccessToken, requests);

            foreach ((Guid, DataTransferObjects.Responses.Estate.EstateOperatorResponse) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Operator {result.Item2.Name} created with Id {result.Item2.OperatorId} for Estate {result.Item1}");
            }
        }

        [Given("I have assigned the following operators to the estates")]
        public async Task GivenIHaveAssignedTheFollowingOperatorsToTheEstates(DataTable dataTable)
        {
            List<(EstateDetails estate, AssignOperatorRequest request)> requests = dataTable.Rows.ToAssignOperatorToEstateRequests(this.TestingContext.Estates);

            await this.TransactionProcessorSteps.GivenIHaveAssignedTheFollowingOperatorsToTheEstates(this.TestingContext.AccessToken, requests);

            // TODO Verify
        }

        [Given(@"I have assigned the following devices to the merchants")]
        [When(@"I add the following devices to the merchant")]
        public async Task WhenIAddTheFollowingDevicesToTheMerchant(DataTable table)
        {
            List<(EstateDetails, Guid, AddMerchantDeviceRequest)> requests = table.Rows.ToAddMerchantDeviceRequests(this.TestingContext.Estates);

            List<(EstateDetails, DataTransferObjects.Responses.Merchant.MerchantResponse, String)> results = await this.TransactionProcessorSteps.GivenIHaveAssignedTheFollowingDevicesToTheMerchants(this.TestingContext.AccessToken, requests);
            foreach ((EstateDetails, DataTransferObjects.Responses.Merchant.MerchantResponse, String) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Device {result.Item3} assigned to Merchant {result.Item2.MerchantName} Estate {result.Item1.EstateName}");
            }
        }

        [When(@"I make the following merchant withdrawals")]
        public async Task WhenIMakeTheFollowingMerchantWithdrawals(DataTable table)
        {
            List<(EstateDetails, Guid, MakeMerchantWithdrawalRequest)> requests = table.Rows.ToMakeMerchantWithdrawalRequest(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIMakeTheFollowingMerchantWithdrawals(this.TestingContext.AccessToken, requests);
        }

        [When(@"I make the following automatic merchant deposits")]
        public async Task WhenIMakeTheFollowingAutomaticMerchantDeposits(DataTable table)
        {
            var results = table.Rows.ToAutomaticDepositRequests(this.TestingContext.Estates, DockerHelper.TestBankSortCode, DockerHelper.TestBankAccountNumber);
            await this.TransactionProcessorSteps.WhenIMakeTheFollowingAutomaticMerchantDeposits(results);
        }

        [When(@"I make the following manual merchant deposits the deposit is rejected")]
        [When(@"I make the following automatic merchant deposits the deposit is rejected")]
        public async Task WhenIMakeTheFollowingMerchantDepositsTheDepositIsRejected(DataTable table)
        {
            List<(EstateDetails, Guid, MakeMerchantDepositRequest)> requests = table.Rows.ToMakeMerchantDepositRequest(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIMakeTheFollowingMerchantDepositsTheDepositIsRejected(this.TestingContext.AccessToken, requests);
        }

        [When(@"I set the merchants settlement schedule")]
        public async Task WhenISetTheMerchantsSettlementSchedule(DataTable table)
        {
            List<(EstateDetails, Guid, SetSettlementScheduleRequest)> requests = table.Rows.ToSetSettlementScheduleRequests(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenISetTheMerchantsSettlementSchedule(this.TestingContext.AccessToken, requests);
        }

        [Given(@"I make the following manual merchant deposits")]
        [When(@"I make the following manual merchant deposits")]
        public async Task WhenIMakeTheFollowingManualMerchantDeposits(DataTable table)
        {
            List<(EstateDetails, Guid, MakeMerchantDepositRequest)> requests = table.Rows.ToMakeMerchantDepositRequest(this.TestingContext.Estates);

            foreach ((EstateDetails, Guid, MakeMerchantDepositRequest) request in requests)
            {
                Decimal previousMerchantBalance = await this.GetMerchantBalance(request.Item2);

                await this.TransactionProcessorSteps.GivenIMakeTheFollowingManualMerchantDeposits(this.TestingContext.AccessToken, request);

                await Retry.For(async () => {
                    Decimal currentMerchantBalance = await this.GetMerchantBalance(request.Item2);

                    currentMerchantBalance.ShouldBe(previousMerchantBalance + request.Item3.Amount);

                    this.TestingContext.Logger.LogInformation($"Deposit Reference {request.Item3.Reference} made for Merchant Id {request.Item2}");
                });
            }
        }

        [When(@"I swap the merchant device the device is swapped")]
        public async Task WhenISwapTheMerchantDeviceTheDeviceIsSwapped(DataTable table)
        {
            var requests = table.Rows.ToSwapMerchantDeviceRequests(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenISwapTheMerchantDeviceTheDeviceIsSwapped(this.TestingContext.AccessToken, requests);
        }

        [Given("I create the following merchants")]
        [When(@"I create the following merchants")]
        public async Task WhenICreateTheFollowingMerchants(DataTable table)
        {
            List<(EstateDetails estate, CreateMerchantRequest)> requests = table.Rows.ToCreateMerchantRequests(this.TestingContext.Estates);

            List<DataTransferObjects.Responses.Merchant.MerchantResponse> verifiedMerchants = await this.TransactionProcessorSteps.WhenICreateTheFollowingMerchants(this.TestingContext.AccessToken, requests);

            foreach (DataTransferObjects.Responses.Merchant.MerchantResponse verifiedMerchant in verifiedMerchants){
                //await this.TransactionProcessorSteps.WhenICreateTheFollowingMerchants(this.TestingContext.AccessToken, verifiedMerchant.EstateId, verifiedMerchant.MerchantId);

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
            List<ContractResponse> responses = await this.TransactionProcessorSteps.GivenICreateAContractWithTheFollowingValues(this.TestingContext.AccessToken, requests);
            foreach (ContractResponse contractResponse in responses) {
                EstateDetails estate = this.TestingContext.Estates.Single(e => e.EstateId == contractResponse.EstateId);
                estate.AddContract(contractResponse.ContractId, contractResponse.Description, contractResponse.OperatorId);
            }
        }

        [When(@"I create the following Products")]
        public async Task WhenICreateTheFollowingProducts(DataTable table)
        {
            List<(EstateDetails, TransactionProcessor.IntegrationTesting.Helpers.Contract, AddProductToContractRequest)> requests = table.Rows.ToAddProductToContractRequest(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenICreateTheFollowingProducts(this.TestingContext.AccessToken, requests);
        }

        [When(@"I add the following Transaction Fees")]
        public async Task WhenIAddTheFollowingTransactionFees(DataTable table)
        {
            List<(EstateDetails, TransactionProcessor.IntegrationTesting.Helpers.Contract, TransactionProcessor.IntegrationTesting.Helpers.Product, AddTransactionFeeForProductToContractRequest)> requests = table.Rows.ToAddTransactionFeeForProductToContractRequests(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIAddTheFollowingTransactionFees(this.TestingContext.AccessToken, requests);
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
        [When(@"I get the merchants for '(.*)' then (.*) merchants will be returned")]
        public async Task WhenIGetTheMerchantsForThenMerchantsWillBeReturned(String estateName,
                                                                             Int32 expectedMerchantCount)
        {
            await this.TransactionProcessorSteps.WhenIGetTheMerchantsForThenMerchantsWillBeReturned(this.TestingContext.AccessToken,
                estateName,
                this.TestingContext.Estates,
                expectedMerchantCount);
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

        

        [When(@"I add the following contracts to the following merchants")]
        public async Task WhenIAddTheFollowingContractsToTheFollowingMerchants(DataTable table)
        {
            List<(EstateDetails, Guid, Guid)> requests = table.Rows.ToAddContractToMerchantRequests(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIAddTheFollowingContractsToTheFollowingMerchants(this.TestingContext.AccessToken, requests);
        }

        private async Task<Decimal> GetMerchantBalance(Guid merchantId)
        {
            JsonElement jsonElement = (JsonElement)await this.TestingContext.DockerHelper.ProjectionManagementClient.GetStateAsync<dynamic>("MerchantBalanceProjection", $"MerchantBalance-{merchantId:N}");
            JObject jsonObject = JObject.Parse(jsonElement.GetRawText());
            decimal balanceValue = jsonObject.SelectToken("merchant.balance").Value<decimal>();
            return balanceValue;
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

        [Given(@"the following security roles exist")]
        public async Task GivenTheFollowingSecurityRolesExist(DataTable table)
        {
            List<CreateRoleRequest> requests = table.Rows.ToCreateRoleRequests();
            List<(String, Guid)> responses = await this.SecurityServiceSteps.GivenICreateTheFollowingRoles(requests, CancellationToken.None);
        }

        [Given(@"I have a token to access the estate management resource")]
        [Given(@"I have a token to access the estate management and transaction processor resources")]
        public async Task GivenIHaveATokenToAccessTheEstateManagementResource(DataTable table)
        {
            DataTableRow firstRow = table.Rows.First();
            String clientId = ReqnrollTableHelper.GetStringRowValue(firstRow, "ClientId");
            ClientDetails clientDetails = this.TestingContext.GetClientDetails(clientId);

            //this.TestingContext.AccessToken = await this.SecurityServiceSteps.GetClientToken(clientDetails.ClientId, clientDetails.ClientSecret, CancellationToken.None);
            var token = await this.TestingContext.DockerHelper.SecurityServiceClient.GetToken(clientDetails.ClientId, clientDetails.ClientSecret, CancellationToken.None);
            token.IsSuccess.ShouldBeTrue();
            this.TestingContext.AccessToken = token.Data.AccessToken;

        }

        [When(@"I create the following security users")]
        [Given("I have created the following security users")]
        public async Task WhenICreateTheFollowingSecurityUsers(DataTable table)
        {
            List<CreateNewUserRequest> createUserRequests = table.Rows.ToCreateNewUserRequests(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenICreateTheFollowingSecurityUsers(this.TestingContext.AccessToken, createUserRequests, this.TestingContext.Estates);
        }

        [When(@"I get the estate ""(.*)"" the estate details are returned as follows")]
        public async Task WhenIGetTheEstateTheEstateDetailsAreReturnedAsFollows(String estateName,
                                                                                DataTable table)
        {
            List<String> estateDetails = table.Rows.ToEstateDetails();

            await this.TransactionProcessorSteps.WhenIGetTheEstateTheEstateDetailsAreReturnedAsFollows(this.TestingContext.AccessToken, estateName, this.TestingContext.Estates, estateDetails);
        }

        [When(@"I get the estate ""(.*)"" the estate operator details are returned as follows")]
        public async Task WhenIGetTheEstateTheEstateOperatorDetailsAreReturnedAsFollows(String estateName,
                                                                                        DataTable table)
        {
            List<String> operators = table.Rows.ToOperatorDetails();
            await this.TransactionProcessorSteps.WhenIGetTheEstateTheEstateOperatorDetailsAreReturnedAsFollows(this.TestingContext.AccessToken, estateName, this.TestingContext.Estates, operators);
        }

        [When(@"I get the estate ""(.*)"" the estate security user details are returned as follows")]
        public async Task WhenIGetTheEstateTheEstateSecurityUserDetailsAreReturnedAsFollows(String estateName,
                                                                                            DataTable table)
        {

            List<String> securityUsers = table.Rows.ToSecurityUsersDetails();
            await this.TransactionProcessorSteps.WhenIGetTheEstateTheEstateSecurityUserDetailsAreReturnedAsFollows(this.TestingContext.AccessToken, estateName, this.TestingContext.Estates, securityUsers);
        }

        [When(@"I get the estate ""(.*)"" an error is returned")]
        public async Task WhenIGetTheEstateAnErrorIsReturned(String estateName)
        {
            await this.TransactionProcessorSteps.WhenIGetTheEstateAnErrorIsReturned(this.TestingContext.AccessToken, estateName, this.TestingContext.Estates);
        }

        [Given(@"I am logged in as ""(.*)"" with password ""(.*)"" for Estate ""(.*)"" with client ""(.*)""")]
        public async Task GivenIAmLoggedInAsWithPasswordForEstate(String username,
                                                                  String password,
                                                                  String estateName,
                                                                  String clientId)
        {
            EstateDetails estateDetails = this.TestingContext.GetEstateDetails(estateName);
            ClientDetails clientDetails = this.TestingContext.GetClientDetails(clientId);

            //String tokenResponse = await this.SecurityServiceSteps
            //    .GetPasswordToken(clientId, clientDetails.ClientSecret, username, password, CancellationToken.None).ConfigureAwait(false);
            Result<TokenResponse> token = await this.TestingContext.DockerHelper.SecurityServiceClient.GetToken(username, password, clientId, clientDetails.ClientSecret,
                CancellationToken.None);
            token.IsSuccess.ShouldBeTrue();
            this.TestingContext.AccessToken = token.Data.AccessToken;
            estateDetails.SetEstateUserToken(token.Data.AccessToken);
        }

        [When("I remove the operator {string} from estate {string} the operator is removed")]
        public async Task WhenIRemoveTheOperatorFromEstateTheOperatorIsRemoved(string operatorName, string estateName)
        {
            await this.TransactionProcessorSteps.WhenIRemoveTheOperatorFromEstateTheOperatorIsRemoved(this.TestingContext.AccessToken,
                this.TestingContext.Estates,
                estateName,
                operatorName);
        }

        [When("I update the operators with the following details")]
        public async Task WhenIUpdateTheOperatorsWithTheFollowingDetails(DataTable table)
        {
            List<(EstateDetails, Guid, UpdateOperatorRequest)> requests = table.Rows.ToUpdateOperatorRequests(this.TestingContext.Estates);

            List<OperatorResponse> verifiedOperators = await this.TransactionProcessorSteps.WhenIUpdateTheOperatorsWithTheFollowingDetails(this.TestingContext.AccessToken, requests);

            foreach (OperatorResponse verifiedOperator in verifiedOperators)
            {
                this.TestingContext.Logger.LogInformation($"Operator {verifiedOperator.Name} updated");
            }
        }

        [When("I get all the operators the following details are returned")]
        public async Task WhenIGetAllTheOperatorsTheFollowingDetailsAreReturned(DataTable dataTable)
        {
            List<(EstateDetails, Guid, OperatorResponse)> expectedOperatorResponses = dataTable.Rows.ToOperatorResponses(this.TestingContext.Estates);

            await this.TransactionProcessorSteps.WhenIGetAllTheOperatorsTheFollowingDetailsAreReturned(this.TestingContext.AccessToken, expectedOperatorResponses);
        }

        [Then(@"I get the Contracts for '(.*)' the following contract details are returned")]
        public async Task ThenIGetTheContractsForTheFollowingContractDetailsAreReturned(String estateName,
                                                                                        DataTable table)
        {
            List<(String, String)> contractDetails = table.Rows.ToContractDetails();
            await this.TransactionProcessorSteps.ThenIGetTheContractsForTheFollowingContractDetailsAreReturned(this.TestingContext.AccessToken, estateName, this.TestingContext.Estates, contractDetails);
        }

        [Then(@"I get the Merchant Contracts for '(.*)' for '(.*)' the following contract details are returned")]
        public async Task ThenIGetTheMerchantContractsForForTheFollowingContractDetailsAreReturned(String merchantName,
                                                                                                   String estateName,
                                                                                                   DataTable table)
        {
            List<(String, String)> contractDetails = table.Rows.ToContractDetails();
            await this.TransactionProcessorSteps.ThenIGetTheMerchantContractsForForTheFollowingContractDetailsAreReturned(this.TestingContext.AccessToken, estateName, merchantName, this.TestingContext.Estates, contractDetails);
        }

        [Then(@"I get the Transaction Fees for '(.*)' on the '(.*)' contract for '(.*)' the following fees are returned")]
        public async Task ThenIGetTheTransactionFeesForOnTheContractForTheFollowingFeesAreReturned(String productName,
                                                                                                   String contractName,
                                                                                                   String estateName,
                                                                                                   DataTable table)
        {
            List<(CalculationType, String, Decimal?, FeeType)> transactionFees = table.Rows.ToContractTransactionFeeDetails();
            await this.TransactionProcessorSteps.ThenIGetTheTransactionFeesForOnTheContractForTheFollowingFeesAreReturned(this.TestingContext.AccessToken,
                estateName,
                contractName,
                productName,
                this.TestingContext.Estates, transactionFees);


        }

        [When("I create another contract with the same values it should be rejected")]
        public async Task WhenICreateAnotherContractWithTheSameValuesItShouldBeRejected(DataTable table)
        {
            List<(EstateDetails, CreateContractRequest)> requests = table.Rows.ToCreateContractRequests(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenICreateAnotherContractWithTheSameValuesItShouldBeRejected(this.TestingContext.AccessToken, requests);
        }

        [When("I update the merchants with the following details")]
        public async Task WhenIUpdateTheMerchantsWithTheFollowingDetails(DataTable table)
        {
            List<(EstateDetails, Guid, UpdateMerchantRequest)> requests = table.Rows.ToUpdateMerchantRequests(this.TestingContext.Estates);

            List<DataTransferObjects.Responses.Merchant.MerchantResponse> verifiedMerchants = await this.TransactionProcessorSteps.WhenIUpdateTheFollowingMerchants(this.TestingContext.AccessToken, requests);

            foreach (DataTransferObjects.Responses.Merchant.MerchantResponse verifiedMerchant in verifiedMerchants)
            {
                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(verifiedMerchant.EstateId);
                this.TestingContext.Logger.LogInformation($"Merchant {verifiedMerchant.MerchantName} updated for Estate {estateDetails.EstateName}");
            }
        }

        [When("I update the merchants address with the following details")]
        public async Task WhenIUpdateTheMerchantsAddressWithTheFollowingDetails(DataTable dataTable)
        {
            List<(EstateDetails, DataTransferObjects.Responses.Merchant.MerchantResponse, Guid, Address)> addressUpdatesList = dataTable.Rows.ToAddressUpdates(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIUpdateTheMerchantsAddressWithTheFollowingDetails(this.TestingContext.AccessToken, addressUpdatesList);
        }

        [When("I update the merchants contact with the following details")]
        public async Task WhenIUpdateTheMerchantsContactWithTheFollowingDetails(DataTable dataTable)
        {
            List<(EstateDetails, DataTransferObjects.Responses.Merchant.MerchantResponse, Guid, Contact)> contactUpdatesList = dataTable.Rows.ToContactUpdates(this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIUpdateTheMerchantsContactWithTheFollowingDetails(this.TestingContext.AccessToken, contactUpdatesList);
        }

        [When("I remove the contract {string} from merchant {string} on {string} the contract is removed")]
        public async Task WhenIRemoveTheContractFromMerchantOnTheContractIsRemoved(string contractName, string merchantName, string estateName)
        {
            await this.TransactionProcessorSteps.WhenIRemoveTheContractFromMerchantOnTheContractIsRemoved(this.TestingContext.AccessToken,
                this.TestingContext.Estates,
                estateName,
                merchantName,
                contractName);
        }

        [When("I remove the operator {string} from merchant {string} on {string} the operator is removed")]
        public async Task WhenIRemoveTheOperatorFromMerchantOnTheOperatorIsRemoved(string operatorName, string merchantName, string estateName)
        {
            await this.TransactionProcessorSteps.WhenIRemoveTheOperatorFromMerchantOnTheOperatorIsRemoved(this.TestingContext.AccessToken,
                this.TestingContext.Estates,
                estateName,
                merchantName,
                operatorName);
        }

        [When(@"I get the Estate Settlement Report for Estate '([^']*)' with the Start Date '([^']*)' and the End Date '([^']*)' the following data is returned")]
        public async Task WhenIGetTheEstateSettlementReportForEstateWithTheStartDateAndTheEndDateTheFollowingDataIsReturned(string estateName,
                                                                                                                            string startDateString,
                                                                                                                            string endDateString,
                                                                                                                            DataTable table)
        {
            DateTime stateDate = ReqnrollTableHelper.GetDateForDateString(startDateString, DateTime.UtcNow.Date);
            DateTime endDate = ReqnrollTableHelper.GetDateForDateString(endDateString, DateTime.UtcNow.Date);

            ReqnrollExtensions.SettlementDetails settlementDetails = table.Rows.ToSettlementDetails(estateName, this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIGetTheEstateSettlementReportForEstateForMerchantWithTheStartDateAndTheEndDateTheFollowingDataIsReturned(this.TestingContext.AccessToken, stateDate, endDate, settlementDetails);
        }

        [When(@"I get the Estate Settlement Report for Estate '([^']*)' for Merchant '([^']*)' with the Start Date '([^']*)' and the End Date '([^']*)' the following data is returned")]
        public async Task WhenIGetTheEstateSettlementReportForEstateForMerchantWithTheStartDateAndTheEndDateTheFollowingDataIsReturned(string estateName,
            string merchantName,
            string startDateString,
            string endDateString,
            DataTable table)
        {

            DateTime stateDate = ReqnrollTableHelper.GetDateForDateString(startDateString, DateTime.UtcNow.Date);
            DateTime endDate = ReqnrollTableHelper.GetDateForDateString(endDateString, DateTime.UtcNow.Date);
            ReqnrollExtensions.SettlementDetails settlementDetails = table.Rows.ToSettlementDetails(estateName, merchantName, this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIGetTheEstateSettlementReportForEstateForMerchantWithTheStartDateAndTheEndDateTheFollowingDataIsReturned(this.TestingContext.AccessToken,
                                                                                                                                                          stateDate, endDate,
                                                                                                                                                          settlementDetails);
        }

        [When(@"I get the Estate Settlement Report for Estate '([^']*)' for Merchant '([^']*)' with the Date '([^']*)' the following fees are settled")]
        public async Task WhenIGetTheEstateSettlementReportForEstateForMerchantWithTheDateTheFollowingFeesAreSettled(string estateName,
            string merchantName,
            string settlementDateString,
            DataTable table)
        {

            List<ReqnrollExtensions.SettlementFeeDetails> settlementFeeDetailsList = table.Rows.ToSettlementFeeDetails(estateName, merchantName, settlementDateString, this.TestingContext.Estates);
            await this.TransactionProcessorSteps.WhenIGetTheEstateSettlementReportForEstateForMerchantWithTheDateTheFollowingFeesAreSettled(this.TestingContext.AccessToken, settlementFeeDetailsList);
        }
    }
}
