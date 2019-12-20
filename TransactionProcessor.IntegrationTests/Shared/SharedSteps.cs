using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.IntegrationTests.Shared
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using DataTransferObjects;
    using EstateManagement.DataTransferObjects.Requests;
    using EstateManagement.DataTransferObjects.Responses;
    using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Shouldly;
    using TechTalk.SpecFlow;
    using Xunit;

    [Binding]
    [Scope(Tag = "shared")]
    public class SharedSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        public SharedSteps(ScenarioContext scenarioContext,
                         TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
        }

        [Given(@"I have created the following estates")]
        [When(@"I create the following estates")]
        public async Task WhenICreateTheFollowingEstates(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");

                CreateEstateRequest createEstateRequest = new CreateEstateRequest
                {
                    EstateId = Guid.NewGuid(),
                    EstateName = estateName
                };
                CreateEstateResponse response = null;
                try
                {
response = await this.TestingContext.DockerHelper.EstateClient.CreateEstate(String.Empty, createEstateRequest, CancellationToken.None).ConfigureAwait(false);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                

                response.ShouldNotBeNull();
                response.EstateId.ShouldNotBe(Guid.Empty);

                // Cache the estate id
                this.TestingContext.Estates.Add(estateName, response.EstateId);
            }

            foreach (TableRow tableRow in table.Rows)
            {
                String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");

                KeyValuePair<String, Guid> estateItem = this.TestingContext.Estates.SingleOrDefault(e => e.Key == estateName);

                estateItem.Key.ShouldNotBeNullOrEmpty();
                estateItem.Value.ShouldNotBe(Guid.Empty);

                EstateResponse estate = await this.TestingContext.DockerHelper.EstateClient.GetEstate(String.Empty, estateItem.Value, CancellationToken.None).ConfigureAwait(false);

                estate.EstateName.ShouldBe(estateName);
            }
        }

        [Given(@"I have created the following operators")]
        [When(@"I create the following operators")]
        public async Task WhenICreateTheFollowingOperators(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                String operatorName = SpecflowTableHelper.GetStringRowValue(tableRow, "OperatorName");
                Boolean requireCustomMerchantNumber = SpecflowTableHelper.GetBooleanValue(tableRow, "RequireCustomMerchantNumber");
                Boolean requireCustomTerminalNumber = SpecflowTableHelper.GetBooleanValue(tableRow, "RequireCustomTerminalNumber");

                CreateOperatorRequest createOperatorRequest = new CreateOperatorRequest
                {
                    Name = operatorName,
                    RequireCustomMerchantNumber = requireCustomMerchantNumber,
                    RequireCustomTerminalNumber = requireCustomTerminalNumber
                };

                // lookup the estate id based on the name in the table
                String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");
                Guid estateId = this.TestingContext.Estates.Single(e => e.Key == estateName).Value;

                CreateOperatorResponse response = await this.TestingContext.DockerHelper.EstateClient.CreateOperator(String.Empty, estateId, createOperatorRequest, CancellationToken.None).ConfigureAwait(false);

                response.ShouldNotBeNull();
                response.EstateId.ShouldNotBe(Guid.Empty);
                response.OperatorId.ShouldNotBe(Guid.Empty);

                // Cache the estate id
                this.TestingContext.Operators.Add(operatorName, response.OperatorId);
            }
        }

        [Given("I create the following merchants")]
        [When(@"I create the following merchants")]
        public async Task WhenICreateTheFollowingMerchants(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
                CreateMerchantRequest createMerchantRequest = new CreateMerchantRequest
                {
                    Name = merchantName,
                    Contact = new Contact
                    {
                        ContactName = SpecflowTableHelper.GetStringRowValue(tableRow, "ContactName"),
                        EmailAddress = SpecflowTableHelper.GetStringRowValue(tableRow, "EmailAddress")
                    },
                    Address = new Address
                    {
                        AddressLine1 = SpecflowTableHelper.GetStringRowValue(tableRow, "AddressLine1"),
                        Town = SpecflowTableHelper.GetStringRowValue(tableRow, "Town"),
                        Region = SpecflowTableHelper.GetStringRowValue(tableRow, "Region"),
                        Country = SpecflowTableHelper.GetStringRowValue(tableRow, "Country")
                    }
                };

                // lookup the estate id based on the name in the table
                String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");
                Guid estateId = this.TestingContext.Estates.Single(e => e.Key == estateName).Value;

                CreateMerchantResponse response = await this.TestingContext.DockerHelper.EstateClient
                                                            .CreateMerchant(String.Empty, estateId, createMerchantRequest, CancellationToken.None).ConfigureAwait(false);

                response.ShouldNotBeNull();
                response.EstateId.ShouldBe(estateId);
                response.MerchantId.ShouldNotBe(Guid.Empty);

                // Cache the merchant id
                this.TestingContext.Merchants.Add(merchantName, response.MerchantId);
                if (this.TestingContext.EstateMerchants.ContainsKey(estateId))
                {
                    List<Guid> merchantIdList = this.TestingContext.EstateMerchants[estateId];
                    merchantIdList.Add(response.MerchantId);
                }
                else
                {
                    this.TestingContext.EstateMerchants.Add(estateId, new List<Guid> { response.MerchantId });
                }
            }

            foreach (TableRow tableRow in table.Rows)
            {
                String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");

                KeyValuePair<String, Guid> estateItem = this.TestingContext.Estates.SingleOrDefault(e => e.Key == estateName);

                estateItem.Key.ShouldNotBeNullOrEmpty();
                estateItem.Value.ShouldNotBe(Guid.Empty);

                String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");

                KeyValuePair<String, Guid> merchantItem = this.TestingContext.Merchants.SingleOrDefault(m => m.Key == merchantName);

                merchantItem.Key.ShouldNotBeNullOrEmpty();
                merchantItem.Value.ShouldNotBe(Guid.Empty);

                MerchantResponse merchant = await this.TestingContext.DockerHelper.EstateClient.GetMerchant(String.Empty, estateItem.Value, merchantItem.Value, CancellationToken.None).ConfigureAwait(false);

                merchant.MerchantName.ShouldBe(merchantName);
            }
        }

        [When(@"I assign the following  operator to the merchants")]
        public async Task WhenIAssignTheFollowingOperatorToTheMerchants(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                // Lookup the merchant id
                String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
                Guid merchantId = this.TestingContext.Merchants[merchantName];

                // Lookup the operator id
                String operatorName = SpecflowTableHelper.GetStringRowValue(tableRow, "OperatorName");
                Guid operatorId = this.TestingContext.Operators[operatorName];

                // Now find the estate Id
                Guid estateId = this.TestingContext.EstateMerchants.Where(e => e.Value.Contains(merchantId)).Single().Key;

                AssignOperatorRequest assignOperatorRequest = new AssignOperatorRequest
                {
                    OperatorId = operatorId,
                    MerchantNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantNumber"),
                    TerminalNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "TerminalNumber"),
                };

                AssignOperatorResponse assignOperatorResponse = await this.TestingContext.DockerHelper.EstateClient.AssignOperatorToMerchant(String.Empty, estateId, merchantId, assignOperatorRequest, CancellationToken.None).ConfigureAwait(false);

                assignOperatorResponse.EstateId.ShouldBe(estateId);
                assignOperatorResponse.MerchantId.ShouldBe(merchantId);
                assignOperatorResponse.OperatorId.ShouldBe(operatorId);
            }
        }

        [When(@"I perform the following transactions")]
        public async Task WhenIPerformTheFollowingTransactions(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
                String dateString = SpecflowTableHelper.GetStringRowValue(tableRow, "DateTime");
                DateTime transactionDateTime = SpecflowTableHelper.GetDateForDateString(dateString, DateTime.Today);
                String transactionNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "TransactionNumber");
                String transactionType = SpecflowTableHelper.GetStringRowValue(tableRow, "TransactionType");
                String imeiNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "IMEINumber");

                Guid merchantId = this.TestingContext.Merchants[merchantName];
                Guid estateId = this.TestingContext.EstateMerchants.Single(e => e.Value.Contains(merchantId)).Key;
                
                switch (transactionType)
                {
                    case "Logon":
                        await this.PerformLogonTransaction(estateId,
                                                           merchantId,
                                                           transactionDateTime,
                                                           transactionType,
                                                           transactionNumber,
                                                           imeiNumber,
                                                           CancellationToken.None);
                        break;
                        
                }
            }
        }

        private async Task PerformLogonTransaction(Guid estateId, Guid merchantId, DateTime transactionDateTime, String transactionType, String transactionNumber, String imeiNumber, CancellationToken cancellationToken)
        {
            LogonTransactionRequest logonTransactionRequest = new LogonTransactionRequest
                                                              {
                                                                  MerchantId = merchantId,
                                                                  EstateId = estateId,
                                                                  TransactionDateTime = transactionDateTime,
                                                                  TransactionNumber = transactionNumber,
                                                                  IMEINumber = imeiNumber,
                                                                  TransactionType = transactionType
                                                              };

            SerialisedMessage serialisedMessage = new SerialisedMessage();
            serialisedMessage.Metadata.Add(MetadataContants.KeyNameEstateId, estateId.ToString());
            serialisedMessage.Metadata.Add(MetadataContants.KeyNameMerchantId, merchantId.ToString());
            serialisedMessage.SerialisedData = JsonConvert.SerializeObject(logonTransactionRequest, new JsonSerializerSettings
                                                                                                    {
                                                                                                        TypeNameHandling = TypeNameHandling.All
                                                                                                    });

            SerialisedMessage responseSerialisedMessage = await this.TestingContext.DockerHelper.TransactionProcessorClient.PerformTransaction(serialisedMessage, cancellationToken);
            
            this.TestingContext.TransactionResponses.Add(transactionNumber, responseSerialisedMessage);
        }

        [Then(@"transaction response should contain the following information")]
        public void ThenTransactionResponseShouldContainTheFollowingInformation(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                String transactionNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "TransactionNumber");
                SerialisedMessage serialisedMessage = this.TestingContext.TransactionResponses[transactionNumber];
                Object transactionResponse = JsonConvert.DeserializeObject(serialisedMessage.SerialisedData,
                                                      new JsonSerializerSettings
                                                      {
                                                          TypeNameHandling = TypeNameHandling.All
                                                      });
                this.ValidateTransactionResponse((dynamic)transactionResponse, tableRow);
            }
        }

        private void ValidateTransactionResponse(LogonTransactionResponse logonTransactionResponse,
                                           TableRow tableRow)
        {
            String expectedResponseCode = SpecflowTableHelper.GetStringRowValue(tableRow, "ResponseCode");
            String expectedResponseMessage = SpecflowTableHelper.GetStringRowValue(tableRow, "ResponseMessage");

            logonTransactionResponse.ResponseCode.ShouldBe(expectedResponseCode);
            logonTransactionResponse.ResponseMessage.ShouldBe(expectedResponseMessage);
        }


    }

}
