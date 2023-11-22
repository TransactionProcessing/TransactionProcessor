﻿namespace TransactionProcessor.IntegrationTesting.Helpers;

using DataTransferObjects;
using EstateManagement.DataTransferObjects.Responses;
using EstateManagement.IntegrationTesting.Helpers;
using Newtonsoft.Json;
using Shared.IntegrationTesting;
using Shouldly;
using TechTalk.SpecFlow;

public static class SpecflowExtensions
{

    private static EstateDetails GetEstateDetails(List<EstateDetails> estateDetailsList, String estateName)
    {
        EstateDetails estateDetails = estateDetailsList.SingleOrDefault(e => e.EstateName == estateName);

        if (estateDetails == null && estateName == "InvalidEstate")
        {
            estateDetails = EstateDetails.Create(Guid.Parse("79902550-64DF-4491-B0C1-4E78943928A3"), estateName, "EstateRef1");
            estateDetails.AddMerchant(new MerchantResponse
                                      {
                                          MerchantId = Guid.Parse("36AA0109-E2E3-4049-9575-F507A887BB1F"),
                                          MerchantName = "Test Merchant 1"
                                      });
            estateDetailsList.Add(estateDetails);
        }
        else
        {
            estateDetails.ShouldNotBeNull();
        }

        return estateDetails;
    }

    public static List<(EstateDetails, Guid, String, SerialisedMessage)> ToSerialisedMessages(this TableRows tableRows, List<EstateDetails> estateDetailsList)
    {

        List<(EstateDetails, Guid, String, SerialisedMessage)> messages = new List<(EstateDetails, Guid, String, SerialisedMessage)>();
        foreach (TableRow tableRow in tableRows)
        {
            String transactionType = SpecflowTableHelper.GetStringRowValue(tableRow, "TransactionType");
            String dateString = SpecflowTableHelper.GetStringRowValue(tableRow, "DateTime");
            DateTime transactionDateTime = SpecflowTableHelper.GetDateForDateString(dateString, DateTime.UtcNow);
            String transactionNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "TransactionNumber");
            String deviceIdentifier = SpecflowTableHelper.GetStringRowValue(tableRow, "DeviceIdentifier");

            String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");

            EstateDetails estateDetails = SpecflowExtensions.GetEstateDetails(estateDetailsList, estateName);

            String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
            Guid merchantId = estateDetails.GetMerchantId(merchantName);

            String serialisedData = null;
            if (transactionType == "Logon")
            {
                LogonTransactionRequest logonTransactionRequest = new LogonTransactionRequest
                                                                  {
                                                                      MerchantId = merchantId,
                                                                      EstateId = estateDetails.EstateId,
                                                                      TransactionDateTime = transactionDateTime,
                                                                      TransactionNumber = transactionNumber,
                                                                      DeviceIdentifier = deviceIdentifier,
                                                                      TransactionType = transactionType
                                                                  };
                serialisedData = JsonConvert.SerializeObject(logonTransactionRequest,
                                                             new JsonSerializerSettings
                                                             {
                                                                 TypeNameHandling = TypeNameHandling.All
                                                             });

            }

            if (transactionType == "Sale")
            {
                // Get specific sale fields
                String operatorName = SpecflowTableHelper.GetStringRowValue(tableRow, "OperatorName");
                Decimal transactionAmount = SpecflowTableHelper.GetDecimalValue(tableRow, "TransactionAmount");
                String customerAccountNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "CustomerAccountNumber");
                String customerEmailAddress = SpecflowTableHelper.GetStringRowValue(tableRow, "CustomerEmailAddress");
                String contractDescription = SpecflowTableHelper.GetStringRowValue(tableRow, "ContractDescription");
                String productName = SpecflowTableHelper.GetStringRowValue(tableRow, "ProductName");
                Int32 transactionSource = SpecflowTableHelper.GetIntValue(tableRow, "TransactionSource");
                String recipientEmail = SpecflowTableHelper.GetStringRowValue(tableRow, "RecipientEmail");
                String recipientMobile = SpecflowTableHelper.GetStringRowValue(tableRow, "RecipientMobile");
                String messageType = SpecflowTableHelper.GetStringRowValue(tableRow, "MessageType");
                String accountNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "AccountNumber");
                String customerName = SpecflowTableHelper.GetStringRowValue(tableRow, "CustomerName");

                Guid contractId = Guid.Empty;
                Guid productId = Guid.Empty;
                EstateManagement.IntegrationTesting.Helpers.Contract contract = null;
                EstateManagement.IntegrationTesting.Helpers.Product product = null;
                try
                {
                    contract = estateDetails.GetContract(contractDescription);
                }
                catch (Exception ex)
                {

                }

                if (contract != null)
                {
                    contractId = contract.ContractId;
                    product = contract.GetProduct(productName);
                    if (product != null)
                    {
                        productId = product.ProductId;
                    }
                    else
                    {
                        productId = productName switch
                        {
                            "EmptyProduct" => Guid.Empty,
                            "InvalidProduct" => Guid.Parse("934d8164-f36a-448e-b27b-4d671d41d180"),
                            _ => Guid.NewGuid(),
                        };
                    }
                }
                else
                {
                    // This is a nasty hack atm...
                    contractId = contractDescription switch
                    {
                        "EmptyContract" => Guid.Empty,
                        "InvalidContract" => Guid.Parse("934d8164-f36a-448e-b27b-4d671d41d180"),
                        _ => Guid.NewGuid(),
                    };


                }

                SaleTransactionRequest saleTransactionRequest = new SaleTransactionRequest
                                                                {
                                                                    MerchantId = merchantId,
                                                                    EstateId = estateDetails.EstateId,
                                                                    TransactionDateTime = transactionDateTime,
                                                                    TransactionNumber = transactionNumber,
                                                                    DeviceIdentifier = deviceIdentifier,
                                                                    TransactionType = transactionType,
                                                                    OperatorIdentifier = operatorName,
                                                                    CustomerEmailAddress = customerEmailAddress,
                                                                    ProductId = productId,
                                                                    ContractId = contractId,
                                                                    TransactionSource = transactionSource
                                                                };

                saleTransactionRequest.AdditionalTransactionMetadata = operatorName switch
                {
                    "Voucher" => SpecflowExtensions.BuildVoucherTransactionMetaData(recipientEmail, recipientMobile, transactionAmount),
                    "PataPawa PostPay" => SpecflowExtensions.BuildPataPawaMetaData(messageType, accountNumber, recipientMobile, customerName, transactionAmount),
                    _ => SpecflowExtensions.BuildMobileTopupMetaData(transactionAmount, customerAccountNumber)
                };
                serialisedData = JsonConvert.SerializeObject(saleTransactionRequest,
                                                             new JsonSerializerSettings
                                                             {
                                                                 TypeNameHandling = TypeNameHandling.All
                                                             });
            }

            if (transactionType == "Reconciliation")
            {
                Int32 transactionCount = SpecflowTableHelper.GetIntValue(tableRow, "TransactionCount");
                Decimal transactionValue = SpecflowTableHelper.GetDecimalValue(tableRow, "TransactionValue");

                ReconciliationRequest reconciliationRequest = new ReconciliationRequest
                                                              {
                                                                  MerchantId = merchantId,
                                                                  EstateId = estateDetails.EstateId,
                                                                  TransactionDateTime = transactionDateTime,
                                                                  DeviceIdentifier = deviceIdentifier,
                                                                  TransactionValue = transactionValue,
                                                                  TransactionCount = transactionCount,
                                                              };

                serialisedData = JsonConvert.SerializeObject(reconciliationRequest,
                                                             new JsonSerializerSettings
                                                             {
                                                                 TypeNameHandling = TypeNameHandling.All
                                                             });
            }

            SerialisedMessage serialisedMessage = new SerialisedMessage();
            serialisedMessage.Metadata.Add(MetadataContants.KeyNameEstateId, estateDetails.EstateId.ToString());
            serialisedMessage.Metadata.Add(MetadataContants.KeyNameMerchantId, merchantId.ToString());
            serialisedMessage.SerialisedData = serialisedData;
            messages.Add((estateDetails, merchantId, transactionNumber, serialisedMessage));
        }

        return messages;
    }

    public static List<(SerialisedMessage, String, String, String)> GetTransactionDetails(this TableRows tableRows, List<EstateDetails> estateDetailsList)
    {
        List<(SerialisedMessage, String, String, String)> expectedValuesForTransaction = new List<(SerialisedMessage, String, String, String)>();

        foreach (TableRow tableRow in tableRows)
        {
            String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");
            EstateDetails estateDetails = SpecflowExtensions.GetEstateDetails(estateDetailsList, estateName);

            String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
            Guid merchantId = estateDetails.GetMerchantId(merchantName);

            String transactionNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "TransactionNumber");
            String expectedResponseCode = SpecflowTableHelper.GetStringRowValue(tableRow, "ResponseCode");
            String expectedResponseMessage = SpecflowTableHelper.GetStringRowValue(tableRow, "ResponseMessage");

            var message = estateDetails.GetTransactionResponse(merchantId, transactionNumber);
            var serialisedMessage = JsonConvert.DeserializeObject<SerialisedMessage>(message);
            expectedValuesForTransaction.Add((serialisedMessage, transactionNumber, expectedResponseCode, expectedResponseMessage));
        }

        return expectedValuesForTransaction;
    }

    public static List<BalanceEntry> ToBalanceEntries(this TableRows tableRows, String estateName, String merchantName, List<EstateDetails> estateDetailsList)
    {
        List<BalanceEntry> balanceEntries = new List<BalanceEntry>();
        foreach (TableRow tableRow in tableRows)
        {
            EstateDetails estateDetails = SpecflowExtensions.GetEstateDetails(estateDetailsList, estateName);
            Guid merchantId = estateDetails.GetMerchantId(merchantName);

            DateTime entryDateTime = SpecflowTableHelper.GetDateForDateString(tableRow["DateTime"], DateTime.UtcNow);
            String reference = SpecflowTableHelper.GetStringRowValue(tableRow, "Reference");
            String debitOrCredit = SpecflowTableHelper.GetStringRowValue(tableRow, "EntryType");
            Decimal changeAmount = SpecflowTableHelper.GetDecimalValue(tableRow, "ChangeAmount");

            BalanceEntry balanceEntry = new BalanceEntry
                                        {
                                            ChangeAmount = changeAmount,
                                            MerchantId = merchantId,
                                            EntryType = debitOrCredit,
                                            DateTime = entryDateTime,
                                            EstateId = estateDetails.EstateId,
                                            Reference = reference
                                        };
            balanceEntries.Add(balanceEntry);
        }

        return balanceEntries;
    }

    public static List<PataPawaBill> ToPataPawaBills(this TableRows tableRows)
    {
        List<PataPawaBill> bills = new List<PataPawaBill>();
        foreach (TableRow tableRow in tableRows)
        {
            PataPawaBill bill = new PataPawaBill
                                {
                                    due_date = SpecflowTableHelper.GetDateForDateString(tableRow["DueDate"], DateTime.Now),
                                    amount = SpecflowTableHelper.GetDecimalValue(tableRow, "Amount"),
                                    account_number = SpecflowTableHelper.GetStringRowValue(tableRow, "AccountNumber"),
                                    account_name = SpecflowTableHelper.GetStringRowValue(tableRow, "AccountName")
                                };
            bills.Add(bill);
        }
        return bills;
    }

    private static Dictionary<String, String> BuildMobileTopupMetaData(Decimal transactionAmount, String customerAccountNumber)
    {
        return new Dictionary<String, String>
               {
                   {"Amount", transactionAmount.ToString()},
                   {"CustomerAccountNumber", customerAccountNumber}
               };
    }

    private static Dictionary<String, String> BuildPataPawaMetaData(String messageType, String accountNumber, String recipientMobile,
                                                                    String customerName, Decimal transactionAmount)
    {
        return messageType switch
        {
            "VerifyAccount" => SpecflowExtensions.BuildPataPawaMetaDataForVerifyAccount(accountNumber),
            "ProcessBill" => SpecflowExtensions.BuildPataPawaMetaDataForProcessBill(accountNumber, recipientMobile, customerName, transactionAmount),
            _ => throw new Exception($"Unsupported message type [{messageType}]")
        };
    }

    private static Dictionary<String, String> BuildPataPawaMetaDataForVerifyAccount(String accountNumber)
    {
        return new Dictionary<String, String>
               {
                   {"PataPawaPostPaidMessageType", "VerifyAccount"},
                   {"CustomerAccountNumber", accountNumber}
               };
    }

    private static Dictionary<String, String> BuildVoucherTransactionMetaData(String recipientEmail,
                                                                              String recipientMobile,
                                                                              Decimal transactionAmount)
    {
        Dictionary<String, String> additionalTransactionMetadata = new Dictionary<String, String>{
                                                                                                     { "Amount", transactionAmount.ToString() },
                                                                                                 };

        if (String.IsNullOrEmpty(recipientEmail) == false)
        {
            additionalTransactionMetadata.Add("RecipientEmail", recipientEmail);
        }

        if (String.IsNullOrEmpty(recipientMobile) == false)
        {
            additionalTransactionMetadata.Add("RecipientMobile", recipientMobile);
        }

        return additionalTransactionMetadata;
    }
    private static Dictionary<String, String> BuildPataPawaMetaDataForProcessBill(String accountNumber,
                                                                                  String recipientMobile,
                                                                                  String customerName,
                                                                                  Decimal transactionAmount)
    {
        return new Dictionary<String, String>
               {
                   {"PataPawaPostPaidMessageType", "ProcessBill"},
                   {"Amount", transactionAmount.ToString()},
                   {"CustomerAccountNumber", accountNumber},
                   {"MobileNumber", recipientMobile},
                   {"CustomerName", customerName},
               };
    }

    public static List<SerialisedMessage> GetTransactionResendDetails(this TableRows tableRows, List<EstateDetails> estateDetailsList)
    {
        List<SerialisedMessage> serialisedMessages = new List<SerialisedMessage>();

        foreach (TableRow tableRow in tableRows)
        {
            String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");
            EstateDetails estateDetails = estateDetailsList.SingleOrDefault(e => e.EstateName == estateName);
            estateDetails.ShouldNotBeNull();

            String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
            Guid merchantId = estateDetails.GetMerchantId(merchantName);

            String transactionNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "TransactionNumber");
            var message = estateDetails.GetTransactionResponse(merchantId, transactionNumber);
            var serialisedMessage = JsonConvert.DeserializeObject<SerialisedMessage>(message);
            serialisedMessages.Add(serialisedMessage);
        }
        return serialisedMessages;
    }

    public static (EstateDetails, SaleTransactionResponse) GetVoucherByTransactionNumber(String estateName, String merchantName, Int32 transactionNumber, List<EstateDetails> estateDetailsList)
    {
        EstateDetails estateDetails = SpecflowExtensions.GetEstateDetails(estateDetailsList, estateName);

        Guid merchantId = estateDetails.GetMerchantId(merchantName);
        var message = estateDetails.GetTransactionResponse(merchantId, transactionNumber.ToString());
        var serialisedMessage = JsonConvert.DeserializeObject<SerialisedMessage>(message);
        SaleTransactionResponse transactionResponse = JsonConvert.DeserializeObject<SaleTransactionResponse>(serialisedMessage.SerialisedData,
                                                                                                             new JsonSerializerSettings
                                                                                                             {
                                                                                                                 TypeNameHandling = TypeNameHandling.All
                                                                                                             });
        return (estateDetails, transactionResponse);
    }

    public static ProcessSettlementRequest ToProcessSettlementRequest(String dateString, String estateName, String merchantName, List<EstateDetails> estateDetailsList)
    {
        DateTime settlementDate = SpecflowTableHelper.GetDateForDateString(dateString, DateTime.UtcNow.Date);

        EstateDetails estateDetails = SpecflowExtensions.GetEstateDetails(estateDetailsList, estateName);
        Guid merchantId = estateDetails.GetMerchantId(merchantName);

        return new ProcessSettlementRequest
               {
                   MerchantId = merchantId,
                   EstateDetails = estateDetails,
                   SettlementDate = settlementDate,
               };
    }

    public static List<(EstateDetails, Guid, DateTime, Int32)> ToCompletedSettlementRequests(this TableRows tableRows, List<EstateDetails> estateDetailsList)
    {
        List<(EstateDetails, Guid, DateTime, Int32)> results = new List<(EstateDetails, Guid, DateTime, Int32)>();
        foreach (TableRow tableRow in tableRows)
        {
            // Get the merchant name
            String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");
            EstateDetails estateDetails = SpecflowExtensions.GetEstateDetails(estateDetailsList, estateName);

            Guid merchantId = estateDetails.GetMerchantId(SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName"));
            String settlementDateString = SpecflowTableHelper.GetStringRowValue(tableRow, "SettlementDate");
            Int32 numberOfFees = SpecflowTableHelper.GetIntValue(tableRow, "NumberOfFees");
            DateTime settlementDate = SpecflowTableHelper.GetDateForDateString(settlementDateString, DateTime.UtcNow.Date);

            results.Add((estateDetails, merchantId, settlementDate, numberOfFees));
        }

        return results;
    }

    public static List<(EstateDetails, Guid, DateTime, Int32)> ToPendingSettlementRequests(this TableRows tableRows, List<EstateDetails> estateDetailsList)
    {
        List<(EstateDetails, Guid, DateTime, Int32)> results = new List<(EstateDetails, Guid, DateTime, Int32)>();
        foreach (TableRow tableRow in tableRows)
        {
            // Get the merchant name
            String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");
            EstateDetails estateDetails = SpecflowExtensions.GetEstateDetails(estateDetailsList, estateName);

            Guid merchantId = estateDetails.GetMerchantId(SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName"));
            String settlementDateString = SpecflowTableHelper.GetStringRowValue(tableRow, "SettlementDate");
            Int32 numberOfFees = SpecflowTableHelper.GetIntValue(tableRow, "NumberOfFees");
            DateTime settlementDate = SpecflowTableHelper.GetDateForDateString(settlementDateString, DateTime.UtcNow.Date);

            results.Add((estateDetails, merchantId, settlementDate, numberOfFees));
        }

        return results;
    }

    public class BalanceEntry
    {
        public Guid EstateId { get; set; }
        public Guid MerchantId { get; set; }
        public DateTime DateTime { get; set; }
        public String Reference { get; set; }
        public String EntryType { get; set; }
        public Decimal In { get; set; }
        public Decimal Out { get; set; }
        public Decimal ChangeAmount { get; set; }
        public Decimal Balance { get; set; }
    }

    public class PataPawaBill
    {
        public DateTime due_date { get; set; }
        public Decimal amount { get; set; }
        public String account_number { get; set; }
        public String account_name { get; set; }
    }

    public class ProcessSettlementRequest
    {
        public DateTime SettlementDate { get; set; }
        public EstateDetails EstateDetails { get; set; }
        public Guid MerchantId { get; set; }
    }
}