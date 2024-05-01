namespace TransactionProcessor.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Metadata;
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.OperatorInterfaces.PataPawaPostPay;
    using BusinessLogic.OperatorInterfaces.SafaricomPinless;
    using BusinessLogic.Requests;
    using BusinessLogic.Services;
    using EstateManagement.DataTransferObjects;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.DataTransferObjects.Responses.Contract;
    using EstateManagement.DataTransferObjects.Responses.Estate;
    using FloatAggregate;
    using Models;
    using PataPawaPostPay;
    using ProjectionEngine.State;
    using ReconciliationAggregate;
    using SecurityService.DataTransferObjects.Responses;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Transaction.DomainEvents;
    using TransactionAggregate;
    using TransactionProcessor.Voucher.DomainEvents;
    using VoucherAggregate;
    using CalculationType = Models.CalculationType;
    using FeeType = Models.FeeType;

    public class TestData
    {
        #region Fields

        public static Dictionary<Int32, Guid> FeeIds = new Dictionary<Int32, Guid>(){
                                                                                        { 1, Guid.Parse("A30C47A4-C5D1-4225-B8D4-334365A606F7") },
                                                                                        { 2, Guid.Parse("A53B7325-BB47-43DE-9166-B1F822D2FDAE") },
                                                                                        { 3, Guid.Parse("8E7B7C48-B4B1-47A2-A832-184E04CB227F") },
                                                                                        { 4, Guid.Parse("36DF32D6-CE7E-43F2-B122-F25406071476") },
                                                                                        { 5, Guid.Parse("4E80428F-F052-4765-8084-9BF8A3A5A2A7") },
                                                                                        { 6, Guid.Parse("FDC9C5BA-408C-4853-8BD8-82A89ACC8326") },
                                                                                        { 7, Guid.Parse("12AFB4F6-E242-4C84-9146-F5A2CD39EB9E") },
                                                                                        { 8, Guid.Parse("B0CBF15F-845C-4597-BCE9-7DE66A4E35D5") },
                                                                                        { 9, Guid.Parse("A886ECB9-BE80-4AA9-BFD7-77768200B346") },
                                                                                        { 10, Guid.Parse("2AAB29CE-5D17-4164-90B2-0AACBF2BA47A") },
                                                                                    };
    
        public static String AuthorisationCode = "ABCD1234";

        public static String DeclinedResponseCode = "0001";

        public static String DeclinedResponseMessage = "DeclinedResponseMessage";

        public static Guid DeviceId = Guid.Parse("840F32FF-8B74-467C-8078-F5D9297FED56");

        public static String DeviceIdentifier = "1234567890";

        public static String DeviceIdentifier1 = "1234567891";

        public static Guid EstateId = Guid.Parse("A522FA27-F9D0-470A-A88D-325DED3B62EE");

        public static Guid ContractId = Guid.Parse("97A9ED00-E522-428C-B3C3-5931092DBDCE");
        public static Guid ContractId1 = Guid.Parse("9314DD8B-42A6-4C24-87FE-53CDC70BA48F");

        public static Guid ProductId = Guid.Parse("ABA0E536-4E43-4E26-8362-7FB549DDA534");
        public static Guid ProductId1 = Guid.Parse("C758C21E-6BB2-4709-9F1D-5DA789FB6182");

        public static String EstateName = "Test Estate 1";

        public static String FailedSafaricomTopup =
            "<COMMAND xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><TYPE>EXRCTRFRESP</TYPE><TXNSTATUS>500</TXNSTATUS><DATE>02-JUL-2018</DATE><EXTREFNUM>100022814</EXTREFNUM><TXNID>20200314231322847</TXNID><MESSAGE>Topup failed</MESSAGE></COMMAND>";

        public static Boolean IsAuthorised = true;

        public static Guid MerchantId = Guid.Parse("833B5AAC-A5C5-46C2-A499-F2B4252B2942");

        public static Int32 TransactionSource = 1;

        public static Guid OperatorId = Guid.Parse("804E9D8D-C6FE-4A46-9E55-6A04EA3E1AE5");
        public static Guid OperatorId2 = Guid.Parse("C2A216E8-345F-4E45-B564-16821FFC524F");

        public static String CustomerEmailAddress = "testcustomer1@customer.co.uk";

        //public static Guid OperatorIdentifier1 = Guid.Parse("590C07CF-500C-4F85-A5D4-A90A8021D0A1");

        //public static String OperatorIdentifier2 = "NotSupported";

        public static Boolean RequireCustomMerchantNumber = true;

        public static Boolean RequireCustomTerminalNumber = true;

        public static String ResponseCode = "0000";

        public static String ResponseMessage = "SUCCESS";

        public static String SuccessfulSafaricomTopup =
            "<COMMAND xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><TYPE>EXRCTRFRESP</TYPE><TXNSTATUS>200</TXNSTATUS><DATE>02-JUL-2018</DATE><EXTREFNUM>100022814</EXTREFNUM><TXNID>20200314231322847</TXNID><MESSAGE>Topup Successful</MESSAGE></COMMAND>";

        public static DateTime TransactionDateTime = DateTime.Now;
        public static DateTime FloatCreatedDateTime = DateTime.Now;

        public static Guid TransactionId = Guid.Parse("AE89B2F6-307B-46F4-A8E7-CEF27097D766");

        public static Guid TransactionId2 = Guid.Parse("760E702C-682E-41B1-A582-3D4ECA0F38C3");

        public static Guid SettlementAggregateId = Guid.Parse("BAEBA232-CD7F-46F5-AE2E-3204FE69A441");

        public static Guid FloatAggregateId = Guid.Parse("ffaaea3f-2f39-857a-3da6-61cc7793e03b");

        public static String TransactionNumber = "0001";

        public static TransactionType TransactionTypeLogon = TransactionType.Logon;

        public static String TransactionReference = "ABCDEFGHI";

        public static TransactionType TransactionTypeSale = TransactionType.Sale;

        private static readonly String MerchantName = "Test Merchant Name";

        private static String MerchantNumber = "12345678";

        private static String TerminalNumber = "00000001";

        public static String OperatorAuthorisationCode = "OP1234";

        public static String OperatorResponseCode = "200";

        public static String OperatorResponseMessage = "Topup Successful";

        public static String OperatorTransactionId = "SF12345";

        public static String DeclinedOperatorResponseCode = "400";

        public static String DeclinedOperatorResponseMessage = "Topup Failed";

        public static TransactionResponseCode TransactionResponseCodeSuccess = TransactionResponseCode.Success;

        public static TransactionResponseCode TransactionResponseCodeDeclinedByOperator = TransactionResponseCode.TransactionDeclinedByOperator;

        public static Decimal TransactionAmount = 1000.00m;

        public static OperatorResponse OperatorResponse =>
            new OperatorResponse
            {
                ResponseMessage = TestData.OperatorResponseMessage,
                AdditionalTransactionResponseMetadata = TestData.AdditionalTransactionMetaDataForMobileTopup(),
                TransactionId = TestData.OperatorTransactionId,
                IsSuccessful = true,
                AuthorisationCode = TestData.OperatorAuthorisationCode,
                ResponseCode = TestData.OperatorResponseCode
            };

        #endregion

        #region Properties

        /// <summary>
        /// Gets the null additional transaction meta data.
        /// </summary>
        /// <value>
        /// The null additional transaction meta data.
        /// </value>
        public static Dictionary<String, String> NullAdditionalTransactionMetaData => null;

        /// <summary>
        /// Gets the empty additional transaction meta data.
        /// </summary>
        /// <value>
        /// The empty additional transaction meta data.
        /// </value>
        public static Dictionary<String, String> EmptyAdditionalTransactionMetaData => new Dictionary<String, String>();

        /// <summary>
        /// Gets the additional transaction meta data.
        /// </summary>
        /// <value>
        /// The additional transaction meta data.
        /// </value>
        public static Dictionary<String, String> AdditionalTransactionMetaDataForMobileTopup(String amountName = "Amount", String customerAccountNumberName = "CustomerAccountNumber", String amount="1000.00", String customerAccountNumber = "123456789") =>
            new Dictionary<String, String>
            {
                {amountName, amount},
                {customerAccountNumberName, customerAccountNumber }
            };

        public static Dictionary<String, String> AdditionalTransactionMetaDataForVoucher(String amountName = "Amount", String recipientEmailName = "RecipientEmail", String amount = "100.00", String recipientEmail = "test@testvoucher.co.uk") =>
            new Dictionary<String, String>
            {
                {amountName, amount},
                {recipientEmailName, recipientEmail }

            };

        public static Dictionary<String, String> AdditionalTransactionMetaDataForPataPawaVerifyAccount(String customerAccountNumberName = "CustomerAccountNumber", String pataPawaPostPaidMessageTypeName = "PataPawaPostPaidMessageType", String customerAccountNumber = "123456789", String pataPawaPostPaidMessageType = "VerifyAccount") =>
            new Dictionary<String, String>
            {
                {pataPawaPostPaidMessageTypeName, pataPawaPostPaidMessageType},
                {customerAccountNumberName, customerAccountNumber }
            };
        
        public static Dictionary<String, String> AdditionalTransactionMetaDataForPataPawaVerifyAccount_NoMessageType(String customerAccountNumberName = "CustomerAccountNumber", String customerAccountNumber = "123456789") =>
            new Dictionary<String, String>
            {
                {customerAccountNumberName, customerAccountNumber }
            };

        public static Dictionary<String, String> AdditionalTransactionMetaDataForPataPawaVerifyAccount_NoCustomerAccountNumber(String pataPawaPostPaidMessageTypeName = "PataPawaPostPaidMessageType", String pataPawaPostPaidMessageType = "VerifyAccount") =>
            new Dictionary<String, String>
            {
                {pataPawaPostPaidMessageTypeName, pataPawaPostPaidMessageType},
            };
        
        public static Dictionary<String, String> AdditionalTransactionMetaDataForPataPawaProcessBill(String customerAccountNumberName = "CustomerAccountNumber", 
                                                                                                     String pataPawaPostPaidMessageTypeName = "PataPawaPostPaidMessageType",
                                                                                                     String pataPawaPostPaidMobileNumberName = "MobileNumber",
                                                                                                     String pataPawaPostPaidCustomerNameName = "CustomerName",
                                                                                                     String pataPawaPostPaidAmountName = "Amount",
                                                                                                     String customerAccountNumber = "123456789", 
                                                                                                     String pataPawaPostPaidMessageType = "ProcessBill",
                                                                                                     String pataPawaPostPaidMobileNumber = "123455",
                                                                                                     String pataPawaPostPaidCustomerName = "Customer 1",
                                                                                                     String pataPawaPostPaidAmount = "100.00") =>
            new Dictionary<String, String>
            {
                {pataPawaPostPaidMessageTypeName, pataPawaPostPaidMessageType},
                {customerAccountNumberName, customerAccountNumber },
                {pataPawaPostPaidMobileNumberName, pataPawaPostPaidMobileNumber },
                {pataPawaPostPaidCustomerNameName, pataPawaPostPaidCustomerName },
                {pataPawaPostPaidAmountName, pataPawaPostPaidAmount }
            };

        public static Dictionary<String, String> AdditionalTransactionMetaDataForPataPawaProcessBill_NoMessageType(String customerAccountNumberName = "CustomerAccountNumber",
                                                                                                     String pataPawaPostPaidMessageTypeName = "PataPawaPostPaidMessageType",
                                                                                                     String pataPawaPostPaidMobileNumberName = "MobileNumber",
                                                                                                     String pataPawaPostPaidCustomerNameName = "CustomerName",
                                                                                                     String pataPawaPostPaidAmountName = "Amount",
                                                                                                     String customerAccountNumber = "123456789",
                                                                                                     String pataPawaPostPaidMessageType = "ProcessBill",
                                                                                                     String pataPawaPostPaidMobileNumber = "123455",
                                                                                                     String pataPawaPostPaidCustomerName = "Customer 1",
                                                                                                     String pataPawaPostPaidAmount = "100.00") =>
            new Dictionary<String, String>
            {
                {customerAccountNumberName, customerAccountNumber },
                {pataPawaPostPaidMobileNumberName, pataPawaPostPaidMobileNumber },
                {pataPawaPostPaidCustomerNameName, pataPawaPostPaidCustomerName },
                {pataPawaPostPaidAmountName, pataPawaPostPaidAmount }
            };

        public static Dictionary<String, String> AdditionalTransactionMetaDataForPataPawaProcessBill_NoCustomerAccountNumber(String customerAccountNumberName = "CustomerAccountNumber",
                                                                                                     String pataPawaPostPaidMessageTypeName = "PataPawaPostPaidMessageType",
                                                                                                     String pataPawaPostPaidMobileNumberName = "MobileNumber",
                                                                                                     String pataPawaPostPaidCustomerNameName = "CustomerName",
                                                                                                     String pataPawaPostPaidAmountName = "Amount",
                                                                                                     String customerAccountNumber = "123456789",
                                                                                                     String pataPawaPostPaidMessageType = "ProcessBill",
                                                                                                     String pataPawaPostPaidMobileNumber = "123455",
                                                                                                     String pataPawaPostPaidCustomerName = "Customer 1",
                                                                                                     String pataPawaPostPaidAmount = "100.00") =>
            new Dictionary<String, String>
            {
                {pataPawaPostPaidMessageTypeName, pataPawaPostPaidMessageType},
                {pataPawaPostPaidMobileNumberName, pataPawaPostPaidMobileNumber },
                {pataPawaPostPaidCustomerNameName, pataPawaPostPaidCustomerName },
                {pataPawaPostPaidAmountName, pataPawaPostPaidAmount }
            };

        public static Dictionary<String, String> AdditionalTransactionMetaDataForPataPawaProcessBill_NoMobileNumber(String customerAccountNumberName = "CustomerAccountNumber",
                                                                                                     String pataPawaPostPaidMessageTypeName = "PataPawaPostPaidMessageType",
                                                                                                     String pataPawaPostPaidMobileNumberName = "MobileNumber",
                                                                                                     String pataPawaPostPaidCustomerNameName = "CustomerName",
                                                                                                     String pataPawaPostPaidAmountName = "Amount",
                                                                                                     String customerAccountNumber = "123456789",
                                                                                                     String pataPawaPostPaidMessageType = "ProcessBill",
                                                                                                     String pataPawaPostPaidMobileNumber = "123455",
                                                                                                     String pataPawaPostPaidCustomerName = "Customer 1",
                                                                                                     String pataPawaPostPaidAmount = "100.00") =>
            new Dictionary<String, String>
            {
                {pataPawaPostPaidMessageTypeName, pataPawaPostPaidMessageType},
                {customerAccountNumberName, customerAccountNumber },
                {pataPawaPostPaidCustomerNameName, pataPawaPostPaidCustomerName },
                {pataPawaPostPaidAmountName, pataPawaPostPaidAmount }
            };

        public static Dictionary<String, String> AdditionalTransactionMetaDataForPataPawaProcessBill_NoCustomerName(String customerAccountNumberName = "CustomerAccountNumber",
                                                                                                     String pataPawaPostPaidMessageTypeName = "PataPawaPostPaidMessageType",
                                                                                                     String pataPawaPostPaidMobileNumberName = "MobileNumber",
                                                                                                     String pataPawaPostPaidCustomerNameName = "CustomerName",
                                                                                                     String pataPawaPostPaidAmountName = "Amount",
                                                                                                     String customerAccountNumber = "123456789",
                                                                                                     String pataPawaPostPaidMessageType = "ProcessBill",
                                                                                                     String pataPawaPostPaidMobileNumber = "123455",
                                                                                                     String pataPawaPostPaidCustomerName = "Customer 1",
                                                                                                     String pataPawaPostPaidAmount = "100.00") =>
            new Dictionary<String, String>
            {
                {pataPawaPostPaidMessageTypeName, pataPawaPostPaidMessageType},
                {customerAccountNumberName, customerAccountNumber },
                {pataPawaPostPaidMobileNumberName, pataPawaPostPaidMobileNumber },
                {pataPawaPostPaidAmountName, pataPawaPostPaidAmount }
            };

        public static Dictionary<String, String> AdditionalTransactionMetaDataForPataPawaProcessBill_NoAmount(String customerAccountNumberName = "CustomerAccountNumber",
                                                                                                     String pataPawaPostPaidMessageTypeName = "PataPawaPostPaidMessageType",
                                                                                                     String pataPawaPostPaidMobileNumberName = "MobileNumber",
                                                                                                     String pataPawaPostPaidCustomerNameName = "CustomerName",
                                                                                                     String pataPawaPostPaidAmountName = "Amount",
                                                                                                     String customerAccountNumber = "123456789",
                                                                                                     String pataPawaPostPaidMessageType = "ProcessBill",
                                                                                                     String pataPawaPostPaidMobileNumber = "123455",
                                                                                                     String pataPawaPostPaidCustomerName = "Customer 1",
                                                                                                     String pataPawaPostPaidAmount = "100.00") =>
            new Dictionary<String, String>
            {
                {pataPawaPostPaidMessageTypeName, pataPawaPostPaidMessageType},
                {customerAccountNumberName, customerAccountNumber },
                {pataPawaPostPaidMobileNumberName, pataPawaPostPaidMobileNumber },
                {pataPawaPostPaidCustomerNameName, pataPawaPostPaidCustomerName }
            };

        public static IReadOnlyDictionary<String, String> DefaultAppSettings =>
            new Dictionary<String, String>
            {
                ["AppSettings:ClientId"] = "clientId",
                ["AppSettings:ClientSecret"] = "clientSecret",
                ["AppSettings:UseConnectionStringConfig"] = "false",
                ["EventStoreSettings:ConnectionString"] = "esdb://127.0.0.1:2113",
                ["SecurityConfiguration:Authority"] = "https://127.0.0.1",
                ["AppSettings:EstateManagementApi"] = "http://127.0.0.1",
                ["AppSettings:SecurityService"] = "http://127.0.0.1"
            };

        public static EstateResponse GetEmptyEstateResponse =>
            new EstateResponse
            {
                EstateName = null,
                EstateId = TestData.EstateId
            };

        public static MerchantResponse GetEmptyMerchantResponse =>
            new MerchantResponse
            {
                MerchantId = TestData.MerchantId,
                MerchantName = null
            };

        public static Decimal AvailableBalance = 1000.00m;
        public static Decimal ZeroAvailableBalance = 0.00m;

        public static EstateResponse GetEstateResponseWithOperator1 =>
            new EstateResponse
            {
                EstateName = TestData.EstateName,
                EstateId = TestData.EstateId,
                Operators = new List<EstateOperatorResponse>
                            {
                                new EstateOperatorResponse
                                {
                                    Name = "Safaricom",
                                    OperatorId = TestData.OperatorId,
                                    RequireCustomMerchantNumber = TestData.RequireCustomMerchantNumber,
                                    RequireCustomTerminalNumber = TestData.RequireCustomTerminalNumber
                                }
                            }
            };

        public static EstateResponse GetEstateResponseWithOperator2 =>
            new EstateResponse
            {
                EstateName = TestData.EstateName,
                EstateId = TestData.EstateId,
                Operators = new List<EstateOperatorResponse>
                            {
                                new EstateOperatorResponse
                                {
                                    OperatorId = TestData.OperatorId2,
                                    RequireCustomMerchantNumber = TestData.RequireCustomMerchantNumber,
                                    RequireCustomTerminalNumber = TestData.RequireCustomTerminalNumber
                                }
                            }
            };

        public static EstateResponse GetEstateResponseWithNullOperators =>
            new EstateResponse
            {
                EstateName = TestData.EstateName,
                EstateId = TestData.EstateId,
                Operators = null
            };

        public static EstateResponse GetEstateResponseWithEmptyOperators =>
            new EstateResponse
            {
                EstateName = TestData.EstateName,
                EstateId = TestData.EstateId,
                Operators = new List<EstateOperatorResponse>()
            };

        public static EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse GetMerchantResponseWithOperator1 =>
            new EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = new Dictionary<Guid, String>
                          {
                              {TestData.DeviceId, TestData.DeviceIdentifier}
                          },
                Operators = new List<EstateManagement.DataTransferObjects.Responses.Merchant.MerchantOperatorResponse>
                            {
                                new EstateManagement.DataTransferObjects.Responses.Merchant.MerchantOperatorResponse
                                {
                                    OperatorId = TestData.OperatorId,
                                    MerchantNumber = TestData.MerchantNumber,
                                    TerminalNumber = TestData.TerminalNumber
                                }
                            }
            };


        public static MerchantResponse GetMerchantResponseWithOperator2 =>
            new MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = new Dictionary<Guid, String>
                          {
                              {TestData.DeviceId, TestData.DeviceIdentifier}
                          },
                Operators = new List<MerchantOperatorResponse>
                            {
                                new MerchantOperatorResponse
                                {
                                    OperatorId = TestData.OperatorId2,
                                    MerchantNumber = TestData.MerchantNumber,
                                    TerminalNumber = TestData.TerminalNumber
                                }
                            }
            };

        public static MerchantResponse GetMerchantResponseWithNoDevices =>
            new MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = new Dictionary<Guid, String>(),
                Operators = new List<MerchantOperatorResponse>
                            {
                                new MerchantOperatorResponse
                                {
                                    OperatorId = TestData.OperatorId,
                                    MerchantNumber = TestData.MerchantNumber,
                                    TerminalNumber = TestData.TerminalNumber
                                }
                            }
            };

        public static MerchantResponse GetMerchantResponseWithNullDevices =>
            new MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = null,
                Operators = new List<MerchantOperatorResponse>
                            {
                                new MerchantOperatorResponse
                                {
                                    OperatorId = TestData.OperatorId,
                                    MerchantNumber = TestData.MerchantNumber,
                                    TerminalNumber = TestData.TerminalNumber
                                }
                            }
            };

        public static MerchantResponse GetMerchantResponseWithEmptyOperators =>
            new MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = new Dictionary<Guid, String>
                          {
                              {TestData.DeviceId, TestData.DeviceIdentifier}
                          },
                Operators = new List<MerchantOperatorResponse>()
            };

        public static MerchantResponse GetMerchantResponseWithNullOperators =>
            new MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = new Dictionary<Guid, String>
                          {
                              {TestData.DeviceId, TestData.DeviceIdentifier}
                          },
                Operators = null
            };

        public static EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse Merchant =>
            new EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName
            };

        public static ProcessLogonTransactionRequest ProcessLogonTransactionRequest =>
            ProcessLogonTransactionRequest.Create(TestData.TransactionId,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionTypeLogon.ToString(),
                                                  TestData.TransactionDateTime,
                                                  TestData.TransactionNumber);

        public static ProcessSettlementRequest ProcessSettlementRequest =>
            ProcessSettlementRequest.Create(TestData.SettlementDate,
                                            TestData.MerchantId,
                                            TestData.EstateId);

        public static ProcessLogonTransactionResponse ProcessLogonTransactionResponseModel =>
            new ProcessLogonTransactionResponse
            {
                ResponseMessage = TestData.ResponseMessage,
                ResponseCode = TestData.ResponseCode
            };

        public static ProcessSaleTransactionRequest ProcessSaleTransactionRequest =>
            ProcessSaleTransactionRequest.Create(TestData.TransactionId,
                                                 TestData.EstateId,
                                                 TestData.MerchantId,
                                                 TestData.DeviceIdentifier,
                                                 TestData.TransactionTypeLogon.ToString(),
                                                 TestData.TransactionDateTime,
                                                 TestData.TransactionNumber,
                                                 TestData.OperatorId,
                                                 TestData.CustomerEmailAddress,
                                                 TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                 TestData.ContractId,
                                                 TestData.ProductId,
                                                 TestData.TransactionSource);

        public static ProcessSaleTransactionResponse ProcessSaleTransactionResponseModel =>
            new ProcessSaleTransactionResponse
            {
                ResponseMessage = TestData.ResponseMessage,
                ResponseCode = TestData.ResponseCode,
                AdditionalTransactionMetadata = new Dictionary<String, String>
                                                {
                                                    {"OperatorResponseCode", "1000"}
                                                }
            };

        public static SafaricomConfiguration SafaricomConfiguration =>
            new SafaricomConfiguration
            {
                ExtCode = "ExtCode1",
                LoginId = "LoginId",
                MSISDN = "123456789",
                Password = "Password",
                Url = "http://localhost",
                Pin = "1234"
            };

        public static ProcessReconciliationRequest ProcessReconciliationRequest =>
            ProcessReconciliationRequest.Create(TestData.TransactionId, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime, TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

        public static ProcessReconciliationTransactionResponse ProcessReconciliationTransactionResponseModel =>
            new ProcessReconciliationTransactionResponse
            {
                ResponseMessage = TestData.ResponseMessage,
                ResponseCode = TestData.ResponseCode
            };

        #endregion

        #region Methods

        public static TransactionAggregate GetCompletedLogonTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeLogon,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            transactionAggregate.CompleteTransaction();

            return transactionAggregate;
        }

        public static TransactionAggregate GetCompletedAuthorisedSaleTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeSale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);

            transactionAggregate.AuthoriseTransaction(TestData.OperatorId,
                                                      TestData.AuthorisationCode,
                                                      TestData.OperatorResponseCode,
                                                      TestData.OperatorResponseMessage,
                                                      TestData.OperatorTransactionId,
                                                      TestData.ResponseCode,
                                                      TestData.ResponseMessage);

            transactionAggregate.CompleteTransaction();

            return transactionAggregate;
        }

        public static TransactionAggregate GetCompletedAuthorisedSaleTransactionAggregateWithPendingFee(Guid feeId)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeSale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);

            transactionAggregate.AuthoriseTransaction(TestData.OperatorId,
                                                      TestData.AuthorisationCode,
                                                      TestData.OperatorResponseCode,
                                                      TestData.OperatorResponseMessage,
                                                      TestData.OperatorTransactionId,
                                                      TestData.ResponseCode,
                                                      TestData.ResponseMessage);

            transactionAggregate.CompleteTransaction();
            var calculatedFee = TestData.CalculatedFeeMerchantFee(feeId);
            transactionAggregate.AddFeePendingSettlement(calculatedFee, TestData.TransactionFeeSettlementDueDate);
            return transactionAggregate;
        }

        public static TransactionAggregate GetCompletedAuthorisedSaleTransactionWithReceiptRequestedAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeSale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);

            transactionAggregate.AuthoriseTransaction(TestData.OperatorId,
                                                      TestData.AuthorisationCode,
                                                      TestData.OperatorResponseCode,
                                                      TestData.OperatorResponseMessage,
                                                      TestData.OperatorTransactionId,
                                                      TestData.ResponseCode,
                                                      TestData.ResponseMessage);

            transactionAggregate.CompleteTransaction();

            transactionAggregate.RequestEmailReceipt(TestData.CustomerEmailAddress);

            return transactionAggregate;
        }

        public static TransactionAggregate GetCompletedDeclinedSaleTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeSale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);

            transactionAggregate.DeclineTransaction(TestData.OperatorId,
                                                      TestData.OperatorResponseCode,
                                                      TestData.OperatorResponseMessage,
                                                      TestData.ResponseCode,
                                                      TestData.ResponseMessage);

            transactionAggregate.CompleteTransaction();

            return transactionAggregate;
        }

        public static TransactionAggregate GetIncompleteAuthorisedSaleTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeSale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);

            transactionAggregate.DeclineTransaction(TestData.OperatorId,
                                                    TestData.OperatorResponseCode,
                                                    TestData.OperatorResponseMessage,
                                                    TestData.ResponseCode,
                                                    TestData.ResponseMessage);
            
            return transactionAggregate;
        }

        public static TransactionAggregate GetCompletedAuthorisedLogonTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeLogon,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode,
                                                             TestData.ResponseCode,
                                                             TestData.ResponseMessage);

            transactionAggregate.CompleteTransaction();

            return transactionAggregate;
        }

        public static TransactionAggregate GetCompletedAuthorisedSaleWithNoProductDetailsTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeSale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId,
                                                      TestData.AuthorisationCode,
                                                      TestData.OperatorResponseCode,
                                                      TestData.OperatorResponseMessage,
                                                      TestData.OperatorTransactionId,
                                                      TestData.ResponseCode,
                                                      TestData.ResponseMessage);

            transactionAggregate.CompleteTransaction();

            return transactionAggregate;
        }

        public static FloatAggregate GetEmptyFloatAggregate()
        {
            return FloatAggregate.Create(TestData.FloatAggregateId);
        }

        public static FloatAggregate GetFloatAggregateWithCostValues(){
            
            FloatAggregate floatAggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            floatAggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            floatAggregate.RecordCreditPurchase(DateTime.Now, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice);
            return floatAggregate;
        }

        public static TransactionAggregate GetEmptyTransactionAggregate()
        {
            return TransactionAggregate.Create(TestData.TransactionId);
        }
        
        public static TransactionAggregate GetLocallyAuthorisedTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeLogon,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            return transactionAggregate;
        }

        public static TransactionAggregate GetLocallyDeclinedTransactionAggregate(TransactionResponseCode transactionResponseCode)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeLogon,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.DeclineTransactionLocally(TestData.GetResponseCodeAsString(transactionResponseCode),
                                                           TestData.GetResponseCodeMessage(transactionResponseCode));

            return transactionAggregate;
        }

        public static TransactionAggregate GetDeclinedTransactionAggregate(TransactionResponseCode transactionResponseCode)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeLogon,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.DeclineTransaction(TestData.OperatorId, 
                                                    TestData.DeclinedOperatorResponseCode,
                                                    TestData.DeclinedOperatorResponseMessage,
                                                    TestData.GetResponseCodeAsString(transactionResponseCode),
                                                    TestData.GetResponseCodeMessage(transactionResponseCode));

            return transactionAggregate;
        }

        public static String GetResponseCodeAsString(TransactionResponseCode transactionResponseCode)
        {
            return ((Int32)transactionResponseCode).ToString().PadLeft(4, '0');
        }

        public static String GetResponseCodeMessage(TransactionResponseCode transactionResponseCode)
        {
            return transactionResponseCode.ToString();
        }

        public static TransactionAggregate GetStartedTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TestData.TransactionTypeLogon,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            return transactionAggregate;
        }

        public static TokenResponse TokenResponse()
        {
            return SecurityService.DataTransferObjects.Responses.TokenResponse.Create("AccessToken", string.Empty, 100);
        }

        public static CustomerEmailReceiptRequestedEvent CustomerEmailReceiptRequestedEvent =
            new CustomerEmailReceiptRequestedEvent(TestData.TransactionId, TestData.EstateId, TestData.MerchantId, TestData.CustomerEmailAddress);

        public static CustomerEmailReceiptResendRequestedEvent CustomerEmailReceiptResendRequestedEvent =
            new CustomerEmailReceiptResendRequestedEvent(TestData.TransactionId, TestData.EstateId, TestData.MerchantId);
        
        public static SettledMerchantFeeAddedToTransactionEvent SettledMerchantFeeAddedToTransactionEvent(DateTime settlementDueDate) => new SettledMerchantFeeAddedToTransactionEvent(TestData.SettlementAggregateId,
                                                                                                                                                                                       TestData.EstateId,
                                                                                                                                                                                       TestData.MerchantId,
                                                                                                                                                                                       TestData.CalculatedFeeValue,
                                                                                                                                                                                       (Int32)CalculationType.Fixed,
                                                                                                                                                                                       TestData.TransactionFeeId,
                                                                                                                                                                                       TestData.CalculatedFeeValue,
                                                                                                                                                                                       TestData.TransactionFeeCalculateDateTime,
                                                                                                                                                                                       TestData.SettlementDate,
                                                                                                                                                                                       TestData.SettlementAggregateId);

        public static TransactionHasBeenCompletedEvent TransactionHasBeenCompletedEvent = new TransactionHasBeenCompletedEvent(TestData.TransactionId,
                                                                                                                                  TestData.EstateId,
                                                                                                                                  TestData.MerchantId,
                                                                                                                                  TestData.ResponseCode,
                                                                                                                                  TestData.ResponseMessage,
                                                                                                                                  TestData.IsAuthorised,
                                                                                                                                  TestData.TransactionDateTime,
                                                                                                                                  TestData.TransactionAmount);

        public static Guid TransactionFeeId = Guid.Parse("B83FCCCE-0D45-4FC2-8952-ED277A124BDB");

        public static Guid TransactionFeeId2 = Guid.Parse("CA2D5119-1232-41D6-B6FD-9D84B9B5460C");
        public static Guid TransactionFeeId3 = Guid.Parse("791EF321-24C7-4B90-86D3-2D8ECE3D629F");

        public static String TransactionFeeDescription = "Commission for Merchant";

        public static Decimal TransactionFeeValue = 0.5m;

        public static DateTime TransactionFeeCalculateDateTime = new DateTime(2021, 3, 18);

        public static DateTime TransactionFeeSettlementDueDate = new DateTime(2021, 3, 19);

        public static DateTime TransactionFeeSettledDateTime = new DateTime(2021, 3, 19,1,2,3);

        public static Decimal CalculatedFeeValue = 0.5m;

        public static Int32 ReconciliationTransactionCount = 1;

        public static Decimal ReconciliationTransactionValue = 100;

        public static IssueVoucherResponse IssueVoucherResponse =>
            new IssueVoucherResponse
            {
                ExpiryDate = TestData.VoucherExpiryDate,
                Message = TestData.VoucherMessage,
                VoucherCode = TestData.VoucherCode,
                VoucherId = TestData.VoucherId
            };

        public static DateTime VoucherExpiryDate = new DateTime(2021,1,8);
        public static String VoucherMessage = String.Empty;

        public static String VoucherCode = "ABCDE1234";
        public static Guid VoucherId = Guid.Parse("ED744C18-1F45-47E7-A9FC-7AAC1D9E9D8A");

        public static DateTime SettlementDate = new DateTime(2021,9,22, 1,2,3);

        public static PataPawaPostPaidConfiguration PataPawaPostPaidConfiguration =>
            new PataPawaPostPaidConfiguration {
                                                  ApiLogonRequired = true,
                                                  Password = "TestPassword",
                                                  Url = "http://localhost",
                                                  Username = "TestUserName"
                                              };

        public static String PataPawaPostPaidApiKey = "PataPawaApiKey1";
        public static Decimal PataPawaPostPaidBalance = 100.00m;
        public static String PataPawaPostPaidSuccessMessage = "Successful Logon";
        public static String PataPawaPostPaidFailedMessage = "Error logging on with PataPawa Post Paid API";
        public static Int32 PataPawaPostPaidSuccessStatus = 0;
        public static Int32 PataPawaPostPaidFailedStatus = -1;

        public static login PataPawaPostPaidSuccessfulLoginResponse = new login {
                                                                                    api_key = TestData.PataPawaPostPaidApiKey,
                                                                                    balance = PataPawaPostPaidBalance,
                                                                                    message = PataPawaPostPaidSuccessMessage,
                                                                                    status = PataPawaPostPaidSuccessStatus
                                                                                };

        public static login PataPawaPostPaidFailedLoginResponse = new login {
                                                                                message = PataPawaPostPaidFailedMessage,
                                                                                status = PataPawaPostPaidFailedStatus
                                                                            };

        public static Decimal PataPawaPostPaidAccountBalance = 250.00m;

        public static String PataPawaPostPaidAccountName = "Mr Test Account";
        public static String PataPawaPostPaidAccountNumber = "001122-abc";

        public static DateTime PataPawaPostPaidAccountDueDate = new DateTime(2022, 9, 13);

        public static verify PataPawaPostPaidSuccessfulVerifyAccountResponse = new verify {
                                                                                              account_balance = TestData.PataPawaPostPaidAccountBalance,
                                                                                              account_name = PataPawaPostPaidAccountName,
                                                                                              due_date = PataPawaPostPaidAccountDueDate,
                                                                                              account_no = PataPawaPostPaidAccountNumber
                                                                                          };

        public static paybill PataPawaPostPaidSuccessfulProcessBillResponse = new paybill
                                                                               {
                                                                                   status = 0,
                                                                                   msg = "",
                                                                                   receipt_no = "1",
                                                                                   rescode = "0",
                                                                                   agent_id = "PataPawa",
                                                                                   sms_id = "12345"
                                                                               };

        public static paybill PataPawaPostPaidFailedProcessBillResponse = new paybill
                                                                              {
                                                                                  status = 1,
                                                                                  msg = "",
                                                                                  receipt_no = "1",
                                                                                  rescode = "0",
                                                                                  agent_id = "PataPawa",
                                                                                  sms_id = "12345"
                                                                              };

        public static verify PataPawaPostPaidFailedVerifyAccountResponse = new verify
                                                                               {
                                                                                   account_name = String.Empty,
                                                                                   
                                                                               };

        public static OperatorResponse PataPawaPostPaidSuccessfulLoginOperatorResponse = new OperatorResponse {
                                                                                                                 IsSuccessful = true,
                                                                                                                 AdditionalTransactionResponseMetadata =
                                                                                                                     new Dictionary<String, String> {
                                                                                                                         {
                                                                                                                             "PataPawaPostPaidAPIKey",
                                                                                                                             PataPawaPostPaidApiKey
                                                                                                                         }
                                                                                                                     }
                                                                                                             };

        public static OperatorResponse PataPawaPostPaidFailedLoginOperatorResponse = new OperatorResponse {
                                                                                                              IsSuccessful = false,
                                                                                                              AdditionalTransactionResponseMetadata =
                                                                                                                  new Dictionary<String, String>()
                                                                                                          };

        public static MerchantBalanceState MerchantBalanceProjectionState =>
            new MerchantBalanceState {
                                         AvailableBalance = TestData.AvailableBalance,
                                     };
        public static MerchantBalanceState MerchantBalanceProjectionStateNoCredit =>
            new MerchantBalanceState
            {
                AvailableBalance = 0,
            };

        public static ResendTransactionReceiptRequest ResendTransactionReceiptRequest => ResendTransactionReceiptRequest.Create(TestData.TransactionId,
                                                                                                                                TestData.EstateId);

        public static List<ContractProductTransactionFee> ContractProductTransactionFees =>
            new List<ContractProductTransactionFee>
            {
                new ContractProductTransactionFee
                {
                    FeeType = EstateManagement.DataTransferObjects.Responses.Contract.FeeType.ServiceProvider,
                    Value = TestData.TransactionFeeValue,
                    TransactionFeeId = TestData.TransactionFeeId,
                    Description = TestData.TransactionFeeDescription,
                    CalculationType = (EstateManagement.DataTransferObjects.Responses.Contract.CalculationType)CalculationType.Fixed
                },
                new ContractProductTransactionFee
                {
                    FeeType = EstateManagement.DataTransferObjects.Responses.Contract.FeeType.Merchant,
                    Value = TestData.TransactionFeeValue,
                    TransactionFeeId = TestData.TransactionFeeId2,
                    Description = TestData.TransactionFeeDescription,
                    CalculationType = (EstateManagement.DataTransferObjects.Responses.Contract.CalculationType)CalculationType.Fixed
                }
            };

        public static CalculatedFee CalculatedFeeMerchantFee() =>
            new CalculatedFee
            {
                CalculatedValue = TestData.CalculatedFeeValue,
                FeeCalculationType = CalculationType.Fixed,
                FeeId = TestData.TransactionFeeId,
                FeeValue = TestData.TransactionFeeValue,
                FeeType = FeeType.Merchant
            };

        public static CalculatedFee CalculatedFeeMerchantFee(Guid transactionFeeId) =>
            new CalculatedFee
            {
                CalculatedValue = TestData.CalculatedFeeValue,
                FeeCalculationType = CalculationType.Fixed,
                FeeId = transactionFeeId,
                FeeValue = TestData.TransactionFeeValue,
                FeeType = FeeType.Merchant
            };

        public static CalculatedFee CalculatedFeeMerchantFee2 =>
            new CalculatedFee
            {
                CalculatedValue = TestData.CalculatedFeeValue,
                FeeCalculationType = CalculationType.Fixed,
                FeeId = TestData.TransactionFeeId2,
                FeeValue = TestData.TransactionFeeValue,
                FeeType = FeeType.Merchant
            };

        public static CalculatedFee CalculatedFeeServiceProviderFee() =>
            new CalculatedFee
            {
                CalculatedValue = TestData.CalculatedFeeValue,
                FeeCalculationType = CalculationType.Fixed,
                FeeId = TestData.TransactionFeeId2,
                FeeValue = TestData.TransactionFeeValue,
                FeeType = FeeType.ServiceProvider
            };

        public static CalculatedFee CalculatedFeeServiceProviderFee(Guid transactionFeeId) =>
            new CalculatedFee
            {
                CalculatedValue = TestData.CalculatedFeeValue,
                FeeCalculationType = CalculationType.Fixed,
                FeeId = transactionFeeId,
                FeeValue = TestData.TransactionFeeValue,
                FeeType = FeeType.ServiceProvider
            };

        public static CalculatedFee CalculatedFeeUnsupportedFee =>
            new CalculatedFee
            {
                CalculatedValue = TestData.CalculatedFeeValue,
                FeeCalculationType = CalculationType.Fixed,
                FeeId = TestData.TransactionFeeId3,
                FeeValue = TestData.TransactionFeeValue,
                FeeType = (FeeType)99
            };

        public static List<CalculatedFee> CalculatedMerchantFees =>
            new List<CalculatedFee>
            {
                TestData.CalculatedFeeMerchantFee()
            };

        public static List<CalculatedFee> CalculatedServiceProviderFees =>
            new List<CalculatedFee>
            {
                TestData.CalculatedFeeServiceProviderFee()
            };

        public static SettlementAggregate GetEmptySettlementAggregate()
        {
            return SettlementAggregate.Create(TestData.SettlementAggregateId);
        }

        public static SettlementAggregate GetCreatedSettlementAggregate()
        {
            var aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            return aggregate;
        }

        public static SettlementAggregate GetSettlementAggregateWithPendingMerchantFees(Int32 numberOfFees)
        {
            var aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);

            for (int i = 0; i < numberOfFees; i++)
            {
                aggregate.AddFee(TestData.MerchantId, Guid.NewGuid(), CalculatedFeeMerchantFee(TestData.FeeIds.GetValueOrDefault(i)));
            }

            return aggregate;
        }

        public static SettlementAggregate GetSettlementAggregateWithAllFeesSettled(Int32 numberOfFees)
        {
            var aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);

            for (int i = 0; i < numberOfFees; i++)
            {
                Guid transactionId = Guid.NewGuid();
                Guid transactionFeeId = Guid.NewGuid();
                aggregate.AddFee(TestData.MerchantId, transactionId, CalculatedFeeMerchantFee(transactionFeeId));
                aggregate.MarkFeeAsSettled(TestData.MerchantId, transactionId, transactionFeeId, TestData.SettlementDate);
            }

            return aggregate;
        }

        public static SettlementAggregate GetSettlementAggregateWithNotAllFeesSettled(Int32 numberOfFees)
        {
            var aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);

            for (int i = 0; i <= numberOfFees; i++)
            {
                Guid transactionId = Guid.NewGuid();
                Guid transactionFeeId = Guid.NewGuid();
                aggregate.AddFee(TestData.MerchantId, transactionId, CalculatedFeeMerchantFee(transactionFeeId));
                if (i < numberOfFees)
                {
                    aggregate.MarkFeeAsSettled(TestData.MerchantId, transactionId, transactionFeeId, TestData.SettlementDate);
                }
            }

            return aggregate;
        }

        public static List<ContractResponse> MerchantContractResponses =>
            new List<ContractResponse>() {
                                             new ContractResponse {
                                                                      ContractId = TestData.ContractId,
                                                                      Products = new List<ContractProduct> {
                                                                                                               new ContractProduct {
                                                                                                                   ProductId = TestData.ProductId,
                                                                                                               }
                                                                                                           }
                                                                  }
                                         };
        
        public static String Message = "Test Message";

        public static DateTime ExpiryDate = new DateTime(2020, 12, 5);
        
        public static String OperatorIdentifier = "Operator 1";
        
        public static Decimal Value = 10.00m;

        public static String RecipientEmail = "testrecipient@hotmail.co.uk";

        public static String RecipientMobile = "123456789";

        public static DateTime GeneratedDateTime = new DateTime(2020, 11, 5);

        public static DateTime IssuedDateTime = new DateTime(2020, 11, 5);

        public static IssueVoucherRequest IssueVoucherRequest = IssueVoucherRequest.Create(TestData.VoucherId,
                                                                                           TestData.OperatorId,
                                                                                           TestData.EstateId,
                                                                                           TestData.TransactionId,
                                                                                           TestData.IssuedDateTime,
                                                                                           TestData.Value,
                                                                                           TestData.RecipientEmail,
                                                                                           TestData.RecipientMobile);

        public static String Barcode = "1234567890";

        public static VoucherIssuedEvent VoucherIssuedEvent = new VoucherIssuedEvent(TestData.VoucherId,
                                                                                        TestData.EstateId,
                                                                                        TestData.IssuedDateTime,
                                                                                        TestData.RecipientEmail,
                                                                                        TestData.RecipientMobile);

        public static DateTime RedeemedDateTime = new DateTime(2020, 11, 5);

        public static RedeemVoucherRequest RedeemVoucherRequest = RedeemVoucherRequest.Create(TestData.EstateId, TestData.VoucherCode, TestData.RedeemedDateTime);

        private static Decimal RemainingBalance = 1.00m;

        public static Int32 MerchantReportingId = 1;

        public static Int32 ContractReportingId = 1;

        public static Int32 EstateReportingId = 1;

        public static DateTime SettlementProcessingStartedDateTime = new DateTime(2023,7,17,11,12,20);
        public static DateTime SettlementProcessingStartedDateTimeSecondCall = new DateTime(2023, 7, 17, 11, 12, 40);

        public static DateTime CreditPurchasedDateTime = DateTime.Now;

        public static Decimal FloatCreditAmount = 100m;

        public static Decimal FloatCreditCostPrice = 90m;

        public static Decimal UnitCostPrice = 0.9m;
        public static Decimal TotalCostPrice = 9.0m;

        public static TransactionCostInformationRecordedEvent TransactionCostInformationRecordedEvent =>
            new TransactionCostInformationRecordedEvent(TestData.TransactionId,
                                                        TestData.EstateId,
                                                        TestData.MerchantId,
                                                        TestData.UnitCostPrice,
                                                        TestData.TotalCostPrice);

        public static RecordCreditPurchaseForFloatRequest RecordCreditPurchaseForFloatRequest => RecordCreditPurchaseForFloatRequest.Create(TestData.EstateId, TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice, TestData.CreditPurchasedDateTime);

        public static CreateFloatForContractProductRequest CreateFloatForContractProductRequest => CreateFloatForContractProductRequest.Create(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);

        public static RedeemVoucherResponse RedeemVoucherResponse =>
            new RedeemVoucherResponse
            {
                ExpiryDate = TestData.ExpiryDate,
                VoucherCode = TestData.VoucherCode,
                RemainingBalance = TestData.RemainingBalance
            };

        public static VoucherAggregate GetVoucherAggregateWithRecipientEmail()
        {
            VoucherAggregate aggregate = VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
            aggregate.AddBarcode(TestData.Barcode);
            aggregate.Issue(TestData.RecipientEmail, null, TestData.IssuedDateTime);

            return aggregate;
        }

        public static VoucherAggregate GetVoucherAggregateWithRecipientMobile()
        {
            VoucherAggregate aggregate = VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
            aggregate.AddBarcode(TestData.Barcode);
            aggregate.Issue(null, TestData.RecipientMobile, TestData.IssuedDateTime);

            return aggregate;
        }
        
        #endregion
    }
}