using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.Testing
{
    using BusinessLogic.OperatorInterfaces.SafaricomPinless;
    using BusinessLogic.Requests;
    using BusinessLogic.Services;
    using EstateManagement.DataTransferObjects.Responses;
    using Models;
    using SecurityService.DataTransferObjects.Responses;
    using TransactionAggregate;

    public class TestData
    {
        public static ProcessLogonTransactionResponse ProcessLogonTransactionResponseModel = new ProcessLogonTransactionResponse
                                                                                             {
                                                                                                 ResponseMessage = TestData.ResponseMessage,
                                                                                                 ResponseCode = TestData.ResponseCode,
                                                                                             };

        public static String ResponseMessage = "SUCCESS";

        public static String ResponseCode= "0000";

        public static String DeclinedResponseCode = "0001";

        public static String DeclinedResponseMessage = "DeclinedResponseMessage";

        public static Guid EstateId = Guid.Parse("A522FA27-F9D0-470A-A88D-325DED3B62EE");
        public static Guid MerchantId = Guid.Parse("833B5AAC-A5C5-46C2-A499-F2B4252B2942");
        public static Guid TransactionId = Guid.Parse("AE89B2F6-307B-46F4-A8E7-CEF27097D766");

        public static ProcessLogonTransactionRequest ProcessLogonTransactionRequest = ProcessLogonTransactionRequest.Create( TestData.TransactionId, TestData.EstateId, TestData.MerchantId,
                                                                                                                           TestData.DeviceIdentifier, TestData.TransactionTypeLogon.ToString(),
                                                                                                                             TestData.TransactionDateTime,
                                                                                                                             TestData.TransactionNumber);

        public static String DeviceIdentifier = "1234567890";

        public static String DeviceIdentifier1 = "1234567891";

        public static TransactionType TransactionTypeLogon = TransactionType.Logon;
        public static TransactionType TransactionTypeSale = TransactionType.Sale;

        public static DateTime TransactionDateTime = DateTime.Now;

        public static String TransactionNumber = "0001";

        public static String AuthorisationCode = "ABCD1234";

        public static Boolean IsAuthorised = true;

        public static TransactionAggregate GetEmptyTransactionAggregate()
        {
            return TransactionAggregate.Create(TestData.TransactionId);
        }

        public static TransactionAggregate GetStartedTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,TestData.TransactionNumber, TestData.TransactionTypeLogon, TestData.EstateId, TestData.MerchantId,
                                                  TestData.DeviceIdentifier);

            return transactionAggregate;
        }

        public static TransactionAggregate GetLocallyAuthorisedTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionTypeLogon, TestData.EstateId, TestData.MerchantId,
                                                  TestData.DeviceIdentifier);

            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            return transactionAggregate;
        }

        public static TransactionAggregate GetLocallyDeclinedTransactionAggregate(TransactionResponseCode transactionResponseCode)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionTypeLogon, TestData.EstateId, TestData.MerchantId,
                                                  TestData.DeviceIdentifier);

            transactionAggregate.DeclineTransactionLocally(TestData.GetResponseCodeAsString(transactionResponseCode), TestData.GetResponseCodeMessage(transactionResponseCode));

            return transactionAggregate;
        }

        public static TransactionAggregate GetCompletedTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionTypeLogon, TestData.EstateId, TestData.MerchantId,
                                                  TestData.DeviceIdentifier);

            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            transactionAggregate.CompleteTransaction();

            return transactionAggregate;
        }

        public static IReadOnlyDictionary<String, String> DefaultAppSettings { get; } = new Dictionary<String, String>
        {
            ["AppSettings:ClientId"] = "clientId",
            ["AppSettings:ClientSecret"] = "clientSecret"
        };

        public static TokenResponse TokenResponse()
        {
            return SecurityService.DataTransferObjects.Responses.TokenResponse.Create("AccessToken", String.Empty, 100);
        }

        public static Guid DeviceId = Guid.Parse("840F32FF-8B74-467C-8078-F5D9297FED56");

        private static String MerchantName = "Test Merchant Name";

        public static MerchantResponse GetMerchantResponse = new MerchantResponse
                                                             {
                                                                 EstateId = TestData.EstateId,
                                                                 MerchantId = TestData.MerchantId,
                                                                 MerchantName = TestData.MerchantName,
                                                                 Devices = new Dictionary<Guid, String>
                                                                           {
                                                                               {TestData.DeviceId, TestData.DeviceIdentifier}
                                                                           }
                                                             };

        public static MerchantResponse GetMerchantResponseWithNullDevices = new MerchantResponse
                                                                            {
                                                                                EstateId = TestData.EstateId,
                                                                                MerchantId = TestData.MerchantId,
                                                                                MerchantName = TestData.MerchantName,
                                                                                Devices = null
                                                                            };

        public static MerchantResponse GetMerchantResponseWithNoDevices = new MerchantResponse
                                                                          {
                                                                              EstateId = TestData.EstateId,
                                                                              MerchantId = TestData.MerchantId,
                                                                              MerchantName = TestData.MerchantName,
                                                                              Devices = new Dictionary<Guid, String>()
                                                                          };

        public static EstateResponse GetEmptyEstateResponse = new EstateResponse
                                                              {
                                                                  EstateName = null,
                                                                  EstateId = TestData.EstateId
                                                              };
        public static String EstateName = "Test Estate 1";
        public static EstateResponse GetEstateResponse = new EstateResponse
                                                         {
                                                             EstateName = TestData.EstateName,
                                                             EstateId = TestData.EstateId
                                                         };

        public static MerchantResponse GetEmptyMerchantResponse = new MerchantResponse
                                                                  {
                                                                      MerchantId = TestData.MerchantId,
                                                                      MerchantName = null
                                                                  };

        public static ProcessSaleTransactionResponse ProcessSaleTransactionResponseModel = new ProcessSaleTransactionResponse
                                                                                           {
                                                                                               ResponseMessage = TestData.ResponseMessage,
                                                                                               ResponseCode = TestData.ResponseCode,
                                                                                               AdditionalTransactionMetadata = new Dictionary<String, String>
                                                                                                                               {
                                                                                                                                   {"OperatorResponseCode", "1000"}
                                                                                                                               }
                                                                                           };

        public static String OperatorIdentifier = "Safaricom";

        public static Dictionary<String, String> AdditionalTransactionMetaData = new Dictionary<String, String>
                                                                                 {
                                                                                     {"Amount", "100.00"}
                                                                                 };

        public static ProcessSaleTransactionRequest ProcessSaleTransactionRequest = ProcessSaleTransactionRequest.Create(TestData.TransactionId, TestData.EstateId, TestData.MerchantId,
                                                                                                                         TestData.DeviceIdentifier, TestData.TransactionTypeLogon.ToString(),
                                                                                                                         TestData.TransactionDateTime,
                                                                                                                         TestData.TransactionNumber,
                                                                                                                         TestData.OperatorIdentifier,
                                                                                                                         TestData.AdditionalTransactionMetaData);

        public static String GetResponseCodeAsString(TransactionResponseCode transactionResponseCode)
        {
            return ((Int32)transactionResponseCode).ToString().PadLeft(4, '0');
        }

        public static String GetResponseCodeMessage(TransactionResponseCode transactionResponseCode)
        {
            return transactionResponseCode.ToString();
        }

        public static SafaricomConfiguration SafaricomConfiguration = new SafaricomConfiguration
                                                                      {
                                                                          ExtCode = "ExtCode1",
                                                                          LoginId = "LoginId",
                                                                          MSISDN = "123456789",
                                                                          Password = "Password",
                                                                          Url = "http://localhost",
                                                                          Pin = "1234"
                                                                      };

        public static MerchantResponse Merchant = new MerchantResponse
                                                  {
                                                      EstateId = TestData.EstateId,
                                                      MerchantId = TestData.MerchantId,
                                                      MerchantName = TestData.MerchantName
                                                  };

        public static String SuccessfulSafaricomTopup = "<COMMAND xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><TYPE>EXRCTRFRESP</TYPE><TXNSTATUS>200</TXNSTATUS><DATE>02-JUL-2018</DATE><EXTREFNUM>100022814</EXTREFNUM><TXNID>20200314231322847</TXNID><MESSAGE>Topup Successful</MESSAGE></COMMAND>";

        public static String FailedSafaricomTopup = "<COMMAND xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><TYPE>EXRCTRFRESP</TYPE><TXNSTATUS>500</TXNSTATUS><DATE>02-JUL-2018</DATE><EXTREFNUM>100022814</EXTREFNUM><TXNID>20200314231322847</TXNID><MESSAGE>Topup failed</MESSAGE></COMMAND>";
    }
}
