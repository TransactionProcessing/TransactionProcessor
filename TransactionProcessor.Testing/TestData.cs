using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Shared.EventStore.ProjectionEngine;
using Shared.ValueObjects;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Events;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Requests.Operator;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Merchant;
using AssignOperatorRequest = TransactionProcessor.DataTransferObjects.Requests.Estate.AssignOperatorRequest;
using Contract = TransactionProcessor.Models.Merchant.Contract;
using Deposit = CallbackHandler.DataTransferObjects.Deposit;
using MerchantDepositSource = TransactionProcessor.DataTransferObjects.Requests.Merchant.MerchantDepositSource;
using SettlementSchedule = TransactionProcessor.DataTransferObjects.Responses.Merchant.SettlementSchedule;
using SettlementScheduleModel = TransactionProcessor.Models.Merchant.SettlementSchedule;

namespace TransactionProcessor.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.OperatorInterfaces.PataPawaPostPay;
    using BusinessLogic.OperatorInterfaces.SafaricomPinless;
    using BusinessLogic.Requests;
    using BusinessLogic.Services;
    using Models;
    using PataPawaPostPay;
    using ProjectionEngine.State;
    using SecurityService.DataTransferObjects.Responses;
    using TransactionProcessor.Aggregates.Models;
    using TransactionProcessor.Database.Entities;
    using TransactionProcessor.Models.Estate;
    using ContractProductTransactionFeeModel = Models.Contract.ContractProductTransactionFee;

    public class TestData
    {
        public static String StatementData = "StatementData";

        #region Fields
        public static Guid TransactionId1 = Guid.Parse("82E1ACE2-EA34-4501-832D-1DB97B8B4294");

        public static DateTime TransactionDateTime1 = new DateTime(2021, 12, 10, 11, 00, 00);

        public static DateTime TransactionDateTime2 = new DateTime(2021, 12, 10, 11, 30, 00);
        public static Decimal? TransactionAmount1 = 100.00m;

        public static Decimal? TransactionAmount2 = 85.00m;

        public static Decimal SettledFeeAmount1 = 1.00m;

        public static Decimal SettledFeeAmount2 = 0.85m;

        public static DateTime SettledFeeDateTime1 = new DateTime(2021, 12, 17, 00, 00, 00);

        public static DateTime SettledFeeDateTime2 = new DateTime(2021, 12, 17, 01, 00, 00);

        public static Guid SettledFeeId1 = Guid.Parse("B4D429AE-756D-4F04-8941-4D41B1A75060");

        public static Guid SettledFeeId2 = Guid.Parse("85C64CF1-6522-408D-93E3-D156B4D5C45B");

        public static SettledFee SettledFee1 => new SettledFee(TestData.SettledFeeId1, TestData.TransactionId1, TestData.SettledFeeDateTime1, TestData.SettledFeeAmount1);
        public static SettledFee SettledFee2 => new SettledFee(TestData.SettledFeeId2, TestData.TransactionId2, TestData.SettledFeeDateTime2, TestData.SettledFeeAmount2);

        public static Guid MessageId = Guid.Parse("353FB307-FDD5-41AE-A2AF-C927D57EADBB");

        public static TransactionProcessor.Aggregates.Models.Transaction Transaction1 => new(TransactionId1, TransactionDateTime1, TransactionAmount1.Value);
        public static TransactionProcessor.Aggregates.Models.Transaction Transaction2 => new(TransactionId2, TransactionDateTime2, TransactionAmount2.Value);

        public static DateTime StatementDate = new DateTime(2021, 12, 10);

        public static DateTime StatementEmailedDate = new DateTime(2021, 12, 12);

        public static DateTime StatementGeneratedDate = new DateTime(2021, 12, 11);
        public static DateTime StatementBuiltDate = new DateTime(2021, 12, 11);
        public static Guid MerchantStatementId = Guid.Parse("C8CC622C-07D9-48E9-B544-F53BD29DE1E6");
        public static Guid MerchantStatementForDateId1 = Guid.Parse("9AE8455F-72A7-4876-8403-D6FBEF6EBDBF");
        public static DateTime ActivityDate1 = new DateTime(2021, 1, 10);
        public static Guid MerchantStatementForDateId2 = Guid.Parse("8014A0E9-9605-4496-995F-B022DDC63A53");
        public static DateTime ActivityDate2 = new DateTime(2021, 1, 11);

        public static Guid EventId1 = Guid.Parse("C8CC622C-07D9-48E9-B544-F53BD29DE1E6");

        public static List<Models.Contract.Contract> MerchantContractsEmptyList => new List<Models.Contract.Contract>();

        public static List<Models.Contract.Contract> MerchantContracts =>
            new List<Models.Contract.Contract>{
                new Models.Contract.Contract{
                    ContractId = TestData.ContractId,
                    Description = TestData.ContractDescription,
                    OperatorId = TestData.OperatorId,
                    OperatorName = TestData.OperatorName,
                    Products = new List<Product>{
                        new Product{
                            ContractProductId = TestData.ContractProductId,
                            Value = TestData.ProductFixedValue,
                            DisplayText = TestData.ProductDisplayText,
                            Name = TestData.ProductName
                        }
                    }
                }
            };

        public static List<ContractProductTransactionFeeModel> ProductTransactionFees =>
            new List<ContractProductTransactionFeeModel>{
                new ContractProductTransactionFeeModel{
                    TransactionFeeId = TestData.TransactionFeeId,
                    Description = TestData.TransactionFeeDescription,
                    Value = TestData.TransactionFeeValue,
                    CalculationType = CalculationType.Fixed
                }
            };

        public static Models.Merchant.Merchant MerchantModelWithNullAddresses =>
            new Models.Merchant.Merchant
            {
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                SettlementSchedule = Models.Merchant.SettlementSchedule.Immediate,
                Addresses = null,
                Contacts = new List<Models.Merchant.Contact>{
                    new Models.Merchant.Contact(Guid.NewGuid(), MerchantContactEmailAddress, MerchantContactName, MerchantContactPhoneNumber)
                },
                Devices = new List<Device>{
                    new Device(DeviceId, DeviceIdentifier, true)
                },
                Operators = new List<Models.Merchant.Operator>{
                    new Models.Merchant.Operator(TestData.OperatorId,TestData.OperatorName,TestData.OperatorMerchantNumber,TestData.OperatorTerminalNumber)
                }
            };

        public static Models.Merchant.Merchant MerchantModelWithNullContacts=>
            new Models.Merchant.Merchant
            {
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                SettlementSchedule = Models.Merchant.SettlementSchedule.Immediate,
                Addresses = new List<Models.Merchant.Address>{
                    new Models.Merchant.Address(Guid.NewGuid(), MerchantAddressLine1,MerchantAddressLine2,
                        MerchantAddressLine3,MerchantAddressLine4, MerchantTown,MerchantRegion,
                        MerchantPostalCode, MerchantCountry)
                },
                Contacts = null,
                Devices = new List<Device>{
                    new Device(DeviceId, DeviceIdentifier, true)
                },
                Operators = new List<Models.Merchant.Operator>{
                    new Models.Merchant.Operator(TestData.OperatorId,TestData.OperatorName,TestData.OperatorMerchantNumber,TestData.OperatorTerminalNumber)
                }
            };

        public static Models.Merchant.Merchant MerchantModelWithNullDevices =>
            new Models.Merchant.Merchant
            {
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                SettlementSchedule = Models.Merchant.SettlementSchedule.Immediate,
                Addresses = new List<Models.Merchant.Address>{
                    new Models.Merchant.Address(Guid.NewGuid(), MerchantAddressLine1,MerchantAddressLine2,
                        MerchantAddressLine3,MerchantAddressLine4, MerchantTown,MerchantRegion,
                        MerchantPostalCode, MerchantCountry)
                },
                Contacts = new List<Models.Merchant.Contact>{
                    new Models.Merchant.Contact(Guid.NewGuid(), MerchantContactEmailAddress, MerchantContactName, MerchantContactPhoneNumber)
                },
                Devices = null,
                Operators = new List<Models.Merchant.Operator>{
                    new Models.Merchant.Operator(TestData.OperatorId,TestData.OperatorName,TestData.OperatorMerchantNumber,TestData.OperatorTerminalNumber)
                }
            };

        public static Models.Merchant.Merchant MerchantModelWithNullOperators =>
            new Models.Merchant.Merchant{
                                            MerchantId = TestData.MerchantId,
                                            MerchantName = TestData.MerchantName,
                                            SettlementSchedule = Models.Merchant.SettlementSchedule.Immediate,
                                            Addresses = new List<Models.Merchant.Address>{
                                                                                             new Models.Merchant.Address(Guid.NewGuid(), MerchantAddressLine1,MerchantAddressLine2,
                                                                                                 MerchantAddressLine3,MerchantAddressLine4, MerchantTown,MerchantRegion,
                                                                                                 MerchantPostalCode, MerchantCountry)
                                                                                         },
                                            Contacts = new List<Models.Merchant.Contact>{
                                                                                            new Models.Merchant.Contact(Guid.NewGuid(), MerchantContactEmailAddress, MerchantContactName, MerchantContactPhoneNumber)
                                                                                        },
                                            Devices = new List<Device>{
                                                                          new Device(DeviceId, DeviceIdentifier, true)
                                                                      },
                                            Operators = null
                                        };

        public static Models.Contract.Contract ContractModelWithProducts =>
            new Models.Contract.Contract
            {
                OperatorId = TestData.OperatorId,
                ContractId = TestData.ContractId,
                Description = TestData.ContractDescription,
                IsCreated = true,
                Products = new List<Product>{
                                                                            new Product{
                                                                                           Value = TestData.ProductFixedValue,
                                                                                           ContractProductId = TestData.ContractProductId,
                                                                                           DisplayText = TestData.ProductDisplayText,
                                                                                           Name = TestData.ProductName,
                                                                                           TransactionFees = null
                                                                                       }
                                                                        }
            };
        
        public static Models.Contract.Contract ContractModel =>
            new Models.Contract.Contract
            {
                OperatorId = TestData.OperatorId,
                ContractId = TestData.ContractId,
                Description = TestData.ContractDescription,
                IsCreated = true,
                Products = null
            };


        public static PositiveMoney WithdrawalAmount = PositiveMoney.Create(Money.Create(1000.00m));

        public static PositiveMoney WithdrawalAmount2 = PositiveMoney.Create(Money.Create(1200.00m));

        public static DateTime WithdrawalDateTime = new DateTime(2019, 11, 16);

        public static DateTime WithdrawalDateTime2 = new DateTime(2019, 11, 16);

        public static PositiveMoney DepositAmount = PositiveMoney.Create(Money.Create(1000.00m));

        public static PositiveMoney DepositAmount2 = PositiveMoney.Create(Money.Create(1200.00m));

        public static DateTime DepositDateTime = new DateTime(2019, 11, 16);

        public static DateTime DepositDateTime2 = new DateTime(2019, 11, 16);
        public static String DepositReference = "Test Deposit 1";

        public static String DepositReference2 = "Test Deposit 2";
        public static MerchantDepositSource MerchantDepositSourceManualDTO = MerchantDepositSource.Manual;
        public static MerchantDepositSource MerchantDepositSourceAutomaticDTO = MerchantDepositSource.Automatic;
        public static Models.Merchant.MerchantDepositSource MerchantDepositSourceManual = Models.Merchant.MerchantDepositSource.Manual;
        public static Models.Merchant.MerchantDepositSource MerchantDepositSourceAutomatic = Models.Merchant.MerchantDepositSource.Automatic;
        public static DataTransferObjects.Responses.Merchant.SettlementSchedule SettlementScheduleDTO = DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly;
        public static CreateMerchantRequest CreateMerchantRequest =>
            new CreateMerchantRequest
            {
                Address = new TransactionProcessor.DataTransferObjects.Requests.Merchant.Address
                {
                    AddressLine1 = TestData.MerchantAddressLine1,
                    AddressLine2 = TestData.MerchantAddressLine2,
                    AddressLine3 = TestData.MerchantAddressLine3,
                    AddressLine4 = TestData.MerchantAddressLine4,
                    Country = TestData.MerchantCountry,
                    PostalCode = TestData.MerchantPostalCode,
                    Region = TestData.MerchantRegion,
                    Town = TestData.MerchantTown
                },
                Contact = new TransactionProcessor.DataTransferObjects.Requests.Merchant.Contact
                {
                    ContactName = TestData.MerchantContactName,
                    EmailAddress = TestData.MerchantContactEmailAddress,
                    PhoneNumber = TestData.MerchantContactPhoneNumber
                },
                CreatedDateTime = TestData.DateMerchantCreated,
                MerchantId = TestData.MerchantId,
                Name = TestData.MerchantName,
                SettlementSchedule = TestData.SettlementScheduleDTO
            };

        public static String MerchantAddressLine1 = "Address Line 1";

        public static String MerchantAddressLine1Update = "Address Line 1 Update";

        public static String MerchantAddressLine2 = "Address Line 2";

        public static String MerchantAddressLine2Update = "Address Line 2 Update";

        public static String MerchantAddressLine3 = "Address Line 3";

        public static String MerchantAddressLine3Update = "Address Line 3 Update";

        public static String MerchantAddressLine4 = "Address Line 4";

        public static String MerchantAddressLine4Update = "Address Line 4 Update";

        public static String MerchantContactEmailAddress = "testcontact@merchant1.co.uk";

        public static String MerchantContactName = "Mr Test Contact";

        public static String MerchantContactPhoneNumber = "1234567890";

        public static String MerchantRegion = "Test Region";

        public static String MerchantRegionUpdate = "Test Region Update";

        public static String MerchantTown = "Test Town";

        public static String MerchantTownUpdate = "Test Town Update";

        public static String MerchantPostalCode = "TE571NG";

        public static String MerchantPostalCodeUpdate = "TE571NGUpdate";

        public static String MerchantCountry = "United Kingdom";

        public static String MerchantCountryUpdate = "United Kingdom Update";

        public static String ContactName = "Test Contact";

        public static String ContactNameUpdate = "Test Contact Update";

        public static String ContactPhone = "123456789";

        public static String ContactPhoneUpdate = "1234567890";

        public static String ContactEmail = "testcontact1@testmerchant1.co.uk";

        public static String ContactEmailUpdate = "testcontact1@testmerchant1.com";

        public static String OperatorMerchantNumber = "00000001";

        public static String OperatorTerminalNumber = "00000001";

        public static String MerchantUserEmailAddress = "testmerchantuser@merchant1.co.uk";

        public static String NewDeviceIdentifier = "EMULATOR78910";

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

        public static Guid EstateId = Guid.Parse("488AAFDE-D1DF-4CE0-A0F7-819E42C4885C");
        public static Guid ContractId = Guid.Parse("97A9ED00-E522-428C-B3C3-5931092DBDCE");
        public static Guid ContractId1 = Guid.Parse("9314DD8B-42A6-4C24-87FE-53CDC70BA48F");

        public static Guid ProductId = Guid.Parse("C6309D4C-3182-4D96-AEEA-E9DBBB9DED8F");
        public static Guid ProductId1 = Guid.Parse("C758C21E-6BB2-4709-9F1D-5DA789FB6182");

        public static String EstateName = "Test Estate 1";

        public static String FailedSafaricomTopup =
            "<COMMAND xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><TYPE>EXRCTRFRESP</TYPE><TXNSTATUS>500</TXNSTATUS><DATE>02-JUL-2018</DATE><EXTREFNUM>100022814</EXTREFNUM><TXNID>20200314231322847</TXNID><MESSAGE>Topup failed</MESSAGE></COMMAND>";

        public static Boolean IsAuthorised = true;

        public static Guid MerchantId = Guid.Parse("833B5AAC-A5C5-46C2-A499-F2B4252B2942");

        public static Int32 TransactionSource = 1;

        public static Guid OperatorId = Guid.Parse("804E9D8D-C6FE-4A46-9E55-6A04EA3E1AE5");
        public static Guid OperatorId2 = Guid.Parse("C2A216E8-345F-4E45-B564-16821FFC524F");

        public static String OperatorName = "Test Operator 1";
        public static String OperatorName2 = "Test Operator Name 2";

        public static String CustomerEmailAddress = "testcustomer1@customer.co.uk";

        public static Boolean RequireCustomMerchantNumber = true;

        public static Boolean RequireCustomMerchantNumberFalse = false;

        public static Boolean RequireCustomMerchantNumberTrue = true;

        public static Boolean RequireCustomTerminalNumber = true;

        public static Boolean RequireCustomTerminalNumberFalse = false;

        public static Boolean RequireCustomTerminalNumberTrue = true;

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

        public static DateTime TransactionReceivedDateTime;

        public static TransactionType TransactionTypeLogon = TransactionType.Logon;

        public static String TransactionReference = "ABCDEFGHI";

        public static TransactionType TransactionTypeSale = TransactionType.Sale;

        public static DateTime DateMerchantCreated = new DateTime(2019, 11, 16);

        public static String MerchantReference = "33224DE8";

        public static String MerchantName = "Test Merchant Name";
        public static String MerchantNameUpdated = "Test Merchant 1 Updated";

        public static String MerchantNumber = "12345678";

        public static String TerminalNumber = "00000001";

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

        public static Models.Operator.Operator OperatorModel =>
            new Models.Operator.Operator()
            {
                OperatorId = TestData.OperatorId,
                RequireCustomTerminalNumber = TestData.RequireCustomTerminalNumber,
                RequireCustomMerchantNumber = TestData.RequireCustomMerchantNumber,
                Name = TestData.OperatorName
            };

        public static Models.Contract.Contract ContractModelWithProductsAndTransactionFees =>
            new Models.Contract.Contract
            {
                OperatorId = TestData.OperatorId,
                ContractId = TestData.ContractId,
                Description = TestData.ContractDescription,
                IsCreated = true,
                Products = new List<Product>{
                                                                            new Product{
                                                                                           Value = TestData.ProductFixedValue,
                                                                                           ContractProductId = TestData.ContractProductId,
                                                                                           DisplayText = TestData.ProductDisplayText,
                                                                                           Name = TestData.ProductName,
                                                                                           TransactionFees = new List<ContractProductTransactionFeeModel>{
                                                                                                                                              new ContractProductTransactionFeeModel(){
                                                                                                                                                                         TransactionFeeId = TestData.TransactionFeeId,
                                                                                                                                                                         Description = TestData.TransactionFeeDescription,
                                                                                                                                                                         Value = TestData.TransactionFeeValue,
                                                                                                                                                                         CalculationType = CalculationType.Fixed
                                                                                                                                                                     }
                                                                                                                                          }
                                                                                       }
                                                                        }
            };

        public static String EmailAddress = "testuser1@testestate1.co.uk";
        public static String ContractDescription = "Test Contract";
        public static Guid ContractProductId = Guid.Parse("C6309D4C-3182-4D96-AEEA-E9DBBB9DED8F");
        public static String ProductName = "Product 1";
        public static String ProductDisplayText = "100 KES";
        public static Decimal ProductFixedValue = 100.00m;
        public static ProductType ProductTypeBillPayment = ProductType.BillPayment;

        public static ProductType ProductTypeMobileTopup = ProductType.MobileTopup;
        public static Guid ContractProductId2 = Guid.Parse("642522E4-05F1-4218-9739-18211930F489");
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
                ["AppSettings:SecurityService"] = "http://127.0.0.1",
                ["AppSettings:ContractProductFeeCacheExpiryInHours"] = "",
                ["AppSettings:ContractProductFeeCacheEnabled"] = "",
                ["AppSettings:DatabaseEngine"] = "SqlServer",
                ["ConnectionStrings:HealthCheck"] = "HealthCheck",
                ["ConnectionStrings:EstateReportingReadModel"] = "",
                ["ConnectionStrings:TransactionProcessorReadModel"] = ""
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

        public static EstateResponse GetEstateResponseWithOperator1Deleted =>
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
                                    RequireCustomTerminalNumber = TestData.RequireCustomTerminalNumber,
                                    IsDeleted = true
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

        public static DataTransferObjects.Responses.Merchant.MerchantResponse GetMerchantResponseWithOperator1 =>
            new DataTransferObjects.Responses.Merchant.MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = new Dictionary<Guid, String>
                          {
                              {TestData.DeviceId, TestData.DeviceIdentifier}
                          },
                Operators = new List<DataTransferObjects.Responses.Merchant.MerchantOperatorResponse>
                            {
                                new DataTransferObjects.Responses.Merchant.MerchantOperatorResponse
                                {
                                    OperatorId = TestData.OperatorId,
                                    MerchantNumber = TestData.MerchantNumber,
                                    TerminalNumber = TestData.TerminalNumber
                                }
                            },
                Contracts = new List<MerchantContractResponse>{
                                                                  new MerchantContractResponse{
                                                                                                  ContractId = TestData.ContractId,
                                                                                                  ContractProducts = new List<Guid>(){
                                                                                                                                         TestData.ProductId
                                                                                                                                     }
                                                                                              }
                                                              },
                SettlementSchedule = TransactionProcessor.DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly
            };

        public static DataTransferObjects.Responses.Merchant.MerchantResponse GetMerchantResponseWithOperator1ImmediateSettlement =>
            new DataTransferObjects.Responses.Merchant.MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = new Dictionary<Guid, String>
                {
                    {TestData.DeviceId, TestData.DeviceIdentifier}
                },
                Operators = new List<DataTransferObjects.Responses.Merchant.MerchantOperatorResponse>
                {
                    new DataTransferObjects.Responses.Merchant.MerchantOperatorResponse
                    {
                        OperatorId = TestData.OperatorId,
                        MerchantNumber = TestData.MerchantNumber,
                        TerminalNumber = TestData.TerminalNumber
                    }
                },
                Contracts = new List<MerchantContractResponse>{
                    new MerchantContractResponse{
                        ContractId = TestData.ContractId,
                        ContractProducts = new List<Guid>(){
                            TestData.ProductId
                        }
                    }
                },
                SettlementSchedule = TransactionProcessor.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate
            };

        public static DataTransferObjects.Responses.Merchant.MerchantResponse GetMerchantResponseWithOperator1AndNullContracts =>
            new DataTransferObjects.Responses.Merchant.MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = new Dictionary<Guid, String>
                          {
                              {TestData.DeviceId, TestData.DeviceIdentifier}
                          },
                Operators = new List<DataTransferObjects.Responses.Merchant.MerchantOperatorResponse>
                            {
                                new DataTransferObjects.Responses.Merchant.MerchantOperatorResponse
                                {
                                    OperatorId = TestData.OperatorId,
                                    MerchantNumber = TestData.MerchantNumber,
                                    TerminalNumber = TestData.TerminalNumber
                                }
                            },
                Contracts = null
            };

        public static DataTransferObjects.Responses.Merchant.MerchantResponse GetMerchantResponseWithOperator1AndEmptyContracts =>
            new DataTransferObjects.Responses.Merchant.MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = new Dictionary<Guid, String>
                          {
                              {TestData.DeviceId, TestData.DeviceIdentifier}
                          },
                Operators = new List<DataTransferObjects.Responses.Merchant.MerchantOperatorResponse>
                            {
                                new DataTransferObjects.Responses.Merchant.MerchantOperatorResponse
                                {
                                    OperatorId = TestData.OperatorId,
                                    MerchantNumber = TestData.MerchantNumber,
                                    TerminalNumber = TestData.TerminalNumber
                                }
                            },
                Contracts = new List<MerchantContractResponse>()
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

        public static MerchantResponse GetMerchantResponseWithOperator1Deleted =>
            new MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = new Dictionary<Guid, String>
                          {
                              {TestData.DeviceId, TestData.DeviceIdentifier}
                          },
                Operators = new List<DataTransferObjects.Responses.Merchant.MerchantOperatorResponse>
                            {
                                new DataTransferObjects.Responses.Merchant.MerchantOperatorResponse
                                {
                                    OperatorId = TestData.OperatorId,
                                    MerchantNumber = TestData.MerchantNumber,
                                    TerminalNumber = TestData.TerminalNumber,
                                    IsDeleted = true

                                }
                            },
                Contracts = new List<MerchantContractResponse>{
                                                                  new MerchantContractResponse{
                                                                                                  ContractId = TestData.ContractId,
                                                                                                  ContractProducts = new List<Guid>(){
                                                                                                                                         TestData.ProductId
                                                                                                                                     }
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

        public static Models.Merchant.Merchant Merchant =>
            new()
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                SettlementSchedule = SettlementScheduleModel.Monthly
            };

        //public static DataTransferObjects.Responses.Merchant.MerchantResponse Merchant =>
        //    new DataTransferObjects.Responses.Merchant.MerchantResponse
        //    {
        //        EstateId = TestData.EstateId,
        //        MerchantId = TestData.MerchantId,
        //        MerchantName = TestData.MerchantName,
        //        SettlementSchedule = SettlementSchedule.Monthly
        //    };

        public static DataTransferObjects.Responses.Merchant.MerchantResponse MerchantWithImmediateSettlement =>
            new DataTransferObjects.Responses.Merchant.MerchantResponse
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                SettlementSchedule = TransactionProcessor.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate
            };

        
        public static ProcessLogonTransactionResponse ProcessLogonTransactionResponseModel =>
            new ProcessLogonTransactionResponse
            {
                ResponseMessage = TestData.ResponseMessage,
                ResponseCode = TestData.ResponseCode
            };

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

        public static FloatActivityAggregate GetEmptyFloatActivityAggregate()
        {
            return FloatActivityAggregate.Create(TestData.FloatAggregateId);
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

        public static TransactionDomainEvents.CustomerEmailReceiptRequestedEvent CustomerEmailReceiptRequestedEvent =
            new TransactionDomainEvents.CustomerEmailReceiptRequestedEvent(TestData.TransactionId, TestData.EstateId, TestData.MerchantId, TestData.CustomerEmailAddress, TestData.TransactionDateTime);

        public static TransactionDomainEvents.CustomerEmailReceiptResendRequestedEvent CustomerEmailReceiptResendRequestedEvent =
            new TransactionDomainEvents.CustomerEmailReceiptResendRequestedEvent(TestData.TransactionId, TestData.EstateId, TestData.MerchantId, TestData.TransactionDateTime);
        
        public static TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent SettledMerchantFeeAddedToTransactionEvent(DateTime settlementDueDate) => new TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent(TestData.SettlementAggregateId,
                                                                                                                                                                                       TestData.EstateId,
                                                                                                                                                                                       TestData.MerchantId,
                                                                                                                                                                                       TestData.CalculatedFeeValue,
                                                                                                                                                                                       (Int32)CalculationType.Fixed,
                                                                                                                                                                                       TestData.TransactionFeeId,
                                                                                                                                                                                       TestData.CalculatedFeeValue,
                                                                                                                                                                                       TestData.TransactionFeeCalculateDateTime,
                                                                                                                                                                                       TestData.SettlementDate,
                                                                                                                                                                                       TestData.SettlementAggregateId,
                                                                                                                                                                                       TestData.TransactionDateTime);

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
        public static String PataPawaPostPaidFailedMessage = "Error logging on with PataPawa Post Paid API, Response is -1";
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

        public static String MerchantStateId = "Merchant1";

        public static ProjectionEngine.State.Merchant MerchantState = new ProjectionEngine.State.Merchant(TestData.MerchantStateId,
                                                            TestData.MerchantName,
                                                            1,
                                                            TestData.AvailableBalance,
                                                            new Deposits(1, 100.00m, DateTime.Now.AddDays(-1)),
                                                            new Withdrawals(0, 0, null),
                                                            new AuthorisedSales(1, 55.0m, DateTime.Now),
                                                            new DeclinedSales(0, 0, null),
                                                            new Fees(0, 0));

        public static ProjectionEngine.State.Merchant MerchantStateNoCredit = new ProjectionEngine.State.Merchant(TestData.MerchantStateId,
                                                            TestData.MerchantName,
                                                            1,
                                                            0,
                                                            new Deposits(1, 100.00m, DateTime.Now.AddDays(-1)),
                                                            new Withdrawals(0, 0, null),
                                                            new AuthorisedSales(1, 55.0m, DateTime.Now),
                                                            new DeclinedSales(0, 0, null),
                                                            new Fees(0, 0));

        public static MerchantBalanceProjectionState1 MerchantBalanceProjectionState => new MerchantBalanceProjectionState1(TestData.MerchantState);

        public static MerchantBalanceProjectionState1 MerchantBalanceProjectionStateNoCredit => new MerchantBalanceProjectionState1(TestData.MerchantStateNoCredit);
        
        public static List<DataTransferObjects.Responses.Contract.ContractProductTransactionFee> ContractProductTransactionFees =>
            new List<DataTransferObjects.Responses.Contract.ContractProductTransactionFee>
            {
                new DataTransferObjects.Responses.Contract.ContractProductTransactionFee
                {
                    FeeType = DataTransferObjects.Responses.Contract.FeeType.ServiceProvider,
                    Value = TestData.TransactionFeeValue,
                    TransactionFeeId = TestData.TransactionFeeId,
                    Description = TestData.TransactionFeeDescription,
                    CalculationType = DataTransferObjects.Responses.Contract.CalculationType.Fixed
                },
                new DataTransferObjects.Responses.Contract.ContractProductTransactionFee
                {
                    FeeType = DataTransferObjects.Responses.Contract.FeeType.Merchant,
                    Value = TestData.TransactionFeeValue,
                    TransactionFeeId = TestData.TransactionFeeId2,
                    Description = TestData.TransactionFeeDescription,
                    CalculationType = DataTransferObjects.Responses.Contract.CalculationType.Fixed
                }
            };

        public static CalculatedFee CalculatedFeeMerchantFee() =>
            new CalculatedFee
            {
                CalculatedValue = TestData.CalculatedFeeValue,
                FeeCalculationType = CalculationType.Fixed,
                FeeId = TestData.TransactionFeeId,
                FeeValue = TestData.TransactionFeeValue,
                FeeType = TransactionProcessor.Models.Contract.FeeType.Merchant,
                FeeCalculatedDateTime = TransactionFeeCalculateDateTime,
                IsSettled = false,
                SettlementDueDate = DateTime.MinValue
            };

        public static CalculatedFee CalculatedFeeMerchantFee(Guid transactionFeeId) =>
            new CalculatedFee
            {
                CalculatedValue = TestData.CalculatedFeeValue,
                FeeCalculationType = CalculationType.Fixed,
                FeeId = transactionFeeId,
                FeeValue = TestData.TransactionFeeValue,
                FeeType = TransactionProcessor.Models.Contract.FeeType.Merchant
            };

        public static CalculatedFee CalculatedFeeMerchantFee2 =>
            new CalculatedFee
            {
                CalculatedValue = TestData.CalculatedFeeValue,
                FeeCalculationType = CalculationType.Fixed,
                FeeId = TestData.TransactionFeeId2,
                FeeValue = TestData.TransactionFeeValue,
                FeeType = TransactionProcessor.Models.Contract.FeeType.Merchant
            };

        public static CalculatedFee CalculatedFeeServiceProviderFee() =>
            new CalculatedFee
            {
                CalculatedValue = TestData.CalculatedFeeValue,
                FeeCalculationType = CalculationType.Fixed,
                FeeId = TestData.TransactionFeeId2,
                FeeValue = TestData.TransactionFeeValue,
                FeeType = TransactionProcessor.Models.Contract.FeeType.ServiceProvider
            };

        public static CalculatedFee CalculatedFeeServiceProviderFee(Guid transactionFeeId) =>
            new CalculatedFee
            {
                CalculatedValue = TestData.CalculatedFeeValue,
                FeeCalculationType = CalculationType.Fixed,
                FeeId = transactionFeeId,
                FeeValue = TestData.TransactionFeeValue,
                FeeType = TransactionProcessor.Models.Contract.FeeType.ServiceProvider
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

        public static List<DataTransferObjects.Responses.Contract.ContractResponse> MerchantContractResponses =>
            new () {
                                             new() {
                                                                      ContractId = TestData.ContractId,
                                                                      Products = new() {
                                                                                                               new () {
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

        public static VoucherCommands.IssueVoucherCommand IssueVoucherCommand => new(TestData.VoucherId,
                                                                                           TestData.OperatorId,
                                                                                           TestData.EstateId,
                                                                                           TestData.TransactionId,
                                                                                           TestData.IssuedDateTime,
                                                                                           TestData.Value,
                                                                                           TestData.RecipientEmail,
                                                                                           TestData.RecipientMobile);

        public static String Barcode = "1234567890";

        public static VoucherDomainEvents.VoucherIssuedEvent VoucherIssuedEvent = new VoucherDomainEvents.VoucherIssuedEvent(TestData.VoucherId,
                                                                                        TestData.EstateId,
                                                                                        TestData.IssuedDateTime,
                                                                                        TestData.RecipientEmail,
                                                                                        TestData.RecipientMobile);

        public static DateTime RedeemedDateTime = new DateTime(2020, 11, 5);

        public static VoucherCommands.RedeemVoucherCommand RedeemVoucherCommand => new(TestData.EstateId, TestData.VoucherCode, TestData.RedeemedDateTime);

        private static Decimal RemainingBalance = 1.00m;

        public static Int32 MerchantReportingId = 1;

        public static Int32 ContractReportingId = 1;

        public static Int32 EstateReportingId = 1;

        public static DateTime SettlementProcessingStartedDateTime = new DateTime(2023,7,17,11,12,20);
        public static DateTime SettlementProcessingStartedDateTimeSecondCall = new DateTime(2023, 7, 17, 11, 12, 40);

        public static DateTime CreditPurchasedDateTime = DateTime.Now;

        public static Guid FloatCreditId = Guid.Parse("97CAFCB1-B9BF-47FA-9438-422ABFCCD790");
        public static Decimal FloatCreditAmount = 100m;

        public static Decimal FloatCreditCostPrice = 90m;

        public static Decimal UnitCostPrice = 0.9m;
        public static Decimal TotalCostPrice = 9.0m;

        public static TransactionDomainEvents.TransactionCostInformationRecordedEvent TransactionCostInformationRecordedEvent =>
            new TransactionDomainEvents.TransactionCostInformationRecordedEvent(TestData.TransactionId,
                                                        TestData.EstateId,
                                                        TestData.MerchantId,
                                                        TestData.UnitCostPrice,
                                                        TestData.TotalCostPrice,
                                                        TestData.TransactionDateTime);

        public static FloatCommands.RecordCreditPurchaseForFloatCommand RecordCreditPurchaseForFloatCommand => new(TestData.EstateId, TestData.FloatAggregateId, TestData.FloatCreditAmount, TestData.FloatCreditCostPrice, TestData.CreditPurchasedDateTime);

        public static FloatCommands.CreateFloatForContractProductCommand CreateFloatForContractProductCommand => new(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);

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

        public static String EstateReference = "C6634DE3";

        public static Guid EstateSecurityUserId = Guid.Parse("CBEE25E6-1B08-4023-B20C-CFE0AD746808");
        public static String EstateUserEmailAddress = "testestateuser@estate1.co.uk";
        public static Guid SecurityUserId = Guid.Parse("45B74A2E-BF92-44E9-A300-08E5CDEACFE3");

        public static CreateEstateRequest CreateEstateRequest =>
            new CreateEstateRequest
            {
                EstateId = TestData.EstateId,
                EstateName = TestData.EstateName,
            };
        public static CreateEstateUserRequest CreateEstateUserRequest =>
            new CreateEstateUserRequest
            {
                EmailAddress = TestData.EstateUserEmailAddress,
                FamilyName = TestData.EstateUserFamilyName,
                GivenName = TestData.EstateUserGivenName,
                MiddleName = TestData.EstateUserMiddleName,
                Password = TestData.EstateUserPassword
            };

        public static AssignOperatorRequest AssignOperatorRequestToEstate =>
            new()
            {
                OperatorId = TestData.OperatorId
            };

        public static String EstateUserFamilyName = "Estate";

        public static String EstateUserGivenName = "Test";

        public static String EstateUserMiddleName = "Middle";

        public static String EstateUserPassword = "123456";

        public static Models.Estate.Estate EstateModel =>
            new Models.Estate.Estate()
            {
                EstateId = TestData.EstateId,
                Name = TestData.EstateName,
                Operators = null,
                SecurityUsers = null
            };

        public static Models.Estate.Estate EstateModelWithOperators =>
            new Models.Estate.Estate()
            {
                EstateId = TestData.EstateId,
                Name = TestData.EstateName,
                Operators = new List<Models.Estate.Operator>{
                                                                          new Models.Estate.Operator{
                                                                                          OperatorId = TestData.OperatorId
                                                                                      }
                                                                      },
                SecurityUsers = null
            };

        public static Models.Estate.Estate EstateModelWithOperatorsAndSecurityUsers =>
            new Models.Estate.Estate()
            {
                EstateId = TestData.EstateId,
                Name = TestData.EstateName,
                Operators = new List<Models.Estate.Operator>{
                                                                          new Models.Estate.Operator{
                                                                                          OperatorId = TestData.OperatorId
                                                                                      }
                                                                      },
                SecurityUsers = new List<SecurityUser>{
                                                                                  new SecurityUser{
                                                                                                      EmailAddress = TestData.EstateUserEmailAddress,
                                                                                                      SecurityUserId = TestData.SecurityUserId
                                                                                                  }
                                                                              }
            };

        public static Models.Estate.Estate EstateModelWithSecurityUsers =>
            new Models.Estate.Estate()
            {
                EstateId = TestData.EstateId,
                Name = TestData.EstateName,
                Operators = null,
                SecurityUsers = new List<SecurityUser>{
                                                                                  new SecurityUser{
                                                                                                      EmailAddress = TestData.EstateUserEmailAddress,
                                                                                                      SecurityUserId = TestData.SecurityUserId
                                                                                                  }
                                                                              }
            };

        public static CreateOperatorRequest CreateOperatorRequest =>
            new CreateOperatorRequest()
            {
                OperatorId = TestData.OperatorId,
                RequireCustomTerminalNumber = TestData.RequireCustomTerminalNumber,
                RequireCustomMerchantNumber = TestData.RequireCustomMerchantNumber,
                Name = TestData.OperatorName
            };

        public static UpdateOperatorRequest UpdateOperatorRequest =>
            new UpdateOperatorRequest()
            {
                RequireCustomTerminalNumber = TestData.RequireCustomTerminalNumber,
                RequireCustomMerchantNumber = TestData.RequireCustomMerchantNumber,
                Name = TestData.OperatorName
            };

        public static DataTransferObjects.Requests.Contract.CreateContractRequest CreateContractRequest =
            new DataTransferObjects.Requests.Contract.CreateContractRequest
            {
                Description = ContractDescription,
                OperatorId = OperatorId
            };

        public static AddProductToContractRequest AddProductToContractRequest_FixedValue =>
            new AddProductToContractRequest
            {
                Value = ProductFixedValue,
                DisplayText = ProductDisplayText,
                ProductName = ProductName,
                ProductType = DataTransferObjects.Responses.Contract.ProductType.MobileTopup
            };



        public static AddProductToContractRequest AddProductToContractRequest_VariableValue =>
            new AddProductToContractRequest
            {
                Value = null,
                DisplayText = ProductDisplayText,
                ProductName = ProductName,
                ProductType = DataTransferObjects.Responses.Contract.ProductType.MobileTopup
            };

        public static DataTransferObjects.Requests.Contract.AddTransactionFeeForProductToContractRequest
            AddTransactionFeeForProductToContractRequest =>
            new()
            {
                Description = TransactionFeeDescription,
                CalculationType = DataTransferObjects.Responses.Contract.CalculationType.Fixed,
                Value = TransactionFeeValue,
                FeeType = DataTransferObjects.Responses.Contract.FeeType.Merchant
            };

        public static Int32 FeeCalculationType = 0;

        public static Int32 FeeType = 0;

        public static Decimal FeeValue = 0.0005m;
        public static DataTransferObjects.Requests.Merchant.AssignOperatorRequest AssignOperatorRequestToMerchant =>
            new DataTransferObjects.Requests.Merchant.AssignOperatorRequest
            {
                MerchantNumber = TestData.OperatorMerchantNumber,
                OperatorId = TestData.OperatorId,
                TerminalNumber = TestData.OperatorTerminalNumber
            };

        public static CreateMerchantUserRequest CreateMerchantUserRequest =>
            new CreateMerchantUserRequest
            {
                EmailAddress = TestData.EmailAddress,
                FamilyName = TestData.MerchantUserFamilyName,
                GivenName = TestData.MerchantUserGivenName,
                MiddleName = TestData.MerchantUserMiddleName,
                Password = TestData.MerchantUserPassword
            };

        public static MakeMerchantDepositRequest MakeMerchantDepositRequest =>
            new MakeMerchantDepositRequest
            {
                DepositDateTime = TestData.DepositDateTime,
                Amount = TestData.DepositAmount.Value,
                Reference = TestData.DepositReference
            };



        public static MakeMerchantWithdrawalRequest MakeMerchantWithdrawalRequest =>
            new MakeMerchantWithdrawalRequest
            {
                WithdrawalDateTime = TestData.WithdrawalDateTime,
                Amount = TestData.WithdrawalAmount.Value,
                Reference = TestData.WithdrawalReference
            };

        public static Guid WithdrawalId = Guid.Parse("C5913183-78AE-41C8-9EAE-903F5748DE9A");

        public static String WithdrawalReference = "Withdraw1";

        public static String MerchantUserFamilyName = "Merchant";

        public static String MerchantUserGivenName = "Test";

        public static String MerchantUserMiddleName = "Middle";

        public static String MerchantUserPassword = "123456";
        public static AddMerchantDeviceRequest AddMerchantDeviceRequest =>
            new AddMerchantDeviceRequest
            {
                DeviceIdentifier = TestData.DeviceIdentifier
            };
        public static SwapMerchantDeviceRequest SwapMerchantDeviceRequest =>
            new SwapMerchantDeviceRequest
            {
                NewDeviceIdentifier = TestData.NewDeviceIdentifier
            };

        public static AddMerchantContractRequest AddMerchantContractRequest =>
            new AddMerchantContractRequest
            {
                ContractId = TestData.ContractId
            };
        public static UpdateMerchantRequest UpdateMerchantRequest =>
            new UpdateMerchantRequest
            {
                Name = TestData.MerchantNameUpdated,
                SettlementSchedule = DataTransferObjects.Responses.Merchant.SettlementSchedule.NotSet
            };

        /// <summary>
        /// The address line1
        /// </summary>
        public static String AddressLine1 = "AddressLine1";

        /// <summary>
        /// The address line2
        /// </summary>
        public static String AddressLine2 = "AddressLine2";

        /// <summary>
        /// The address line3
        /// </summary>
        public static String AddressLine3 = "AddressLine3";

        /// <summary>
        /// The address line4
        /// </summary>
        public static String AddressLine4 = "AddressLine4";
        public static String Country = "Country";
        public static String PostCode = "PostCode";
        public static String Region = "Region";
        public static String Town = "Town";
        public static TransactionProcessor.DataTransferObjects.Requests.Merchant.Address Address =>
            new TransactionProcessor.DataTransferObjects.Requests.Merchant.Address
            {
                AddressLine1 = TestData.AddressLine1,
                AddressLine2 = TestData.AddressLine2,
                AddressLine3 = TestData.AddressLine3,
                AddressLine4 = TestData.AddressLine4,
                Country = TestData.Country,
                PostalCode = TestData.PostCode,
                Region = TestData.Region,
                Town = TestData.Town
            };

        public static TransactionProcessor.DataTransferObjects.Requests.Merchant.Contact Contact =>
            new TransactionProcessor.DataTransferObjects.Requests.Merchant.Contact
            {
                ContactName = TestData.ContactName,
                EmailAddress = TestData.ContactEmail,
                PhoneNumber = TestData.ContactPhone
            };

        public static Guid EventId = Guid.Parse("0F537961-A19C-4A29-80DC-68889474142B");
        
        public static Boolean IsAuthorisedFalse = false;

        public static Boolean IsAuthorisedTrue = true;
        public static DateTime StatementCreateDate = new DateTime(2025, 5, 1);
        public static GenerateMerchantStatementRequest GenerateMerchantStatementRequest =>
            new GenerateMerchantStatementRequest
            {
                MerchantStatementDate = TestData.StatementCreateDate
            };
        #endregion

        public static class Commands {
            public static MerchantCommands.GenerateMerchantStatementCommand GenerateMerchantStatementCommand => new(TestData.EstateId, TestData.MerchantId, TestData.GenerateMerchantStatementRequest);
            public static MerchantStatementCommands.BuildMerchantStatementCommand BuildMerchantStatementCommand => new(EstateId, MerchantId, MerchantStatementId);
            public static MerchantStatementCommands.EmailMerchantStatementCommand EmailMerchantStatementCommand => new(EstateId, MerchantStatementId,StatementData);
            public static MerchantStatementCommands.AddTransactionToMerchantStatementCommand AddTransactionToMerchantStatementCommand => new(EstateId, MerchantId, TransactionDateTime, Money.Create(TransactionAmount), IsAuthorisedTrue, TransactionId);
            public static MerchantStatementCommands.AddTransactionToMerchantStatementCommand AddTransactionNotAuthorisedToMerchantStatementCommand => new(EstateId, MerchantId, TransactionDateTime, Money.Create(TransactionAmount), IsAuthorisedFalse, TransactionId);
            public static MerchantStatementCommands.AddTransactionToMerchantStatementCommand AddTransactionWithNoAmountToMerchantStatementCommand => new(EstateId, MerchantId, TransactionDateTime, null, IsAuthorisedTrue, TransactionId);
            public static MerchantStatementCommands.AddSettledFeeToMerchantStatementCommand AddSettledFeeToMerchantStatementCommand => new(EstateId, MerchantId, TransactionDateTime, PositiveMoney.Create(Money.Create(SettledFeeAmount1)), TransactionId, SettledFeeId1);

            public static MerchantStatementCommands.AddDepositToMerchantStatementCommand AddDepositToMerchantStatementCommand => new(EstateId, MerchantId, DepositId, DepositReference, DepositDateTime, DepositAmount);
            public static MerchantStatementCommands.AddWithdrawalToMerchantStatementCommand AddWithdrawalToMerchantStatementCommand => new(EstateId, MerchantId, WithdrawalId, WithdrawalDateTime, WithdrawalAmount);

            public static MerchantStatementCommands.RecordActivityDateOnMerchantStatementCommand RecordActivityDateOnMerchantStatementCommand => new(EstateId, MerchantId, MerchantStatementId,StatementDate,MerchantStatementForDateId1,ActivityDate1);
            public static TransactionCommands.ResendTransactionReceiptCommand ResendTransactionReceiptCommand => new(TestData.TransactionId, TestData.EstateId);
            public static SettlementCommands.AddMerchantFeePendingSettlementCommand AddMerchantFeePendingSettlementCommand => new(TransactionId, CalculatedFeeValue, TransactionFeeCalculateDateTime, CalculationType.Fixed, TransactionFeeId, CalculatedFeeValue, TransactionFeeSettlementDueDate, MerchantId, EstateId);
            public static SettlementCommands.AddSettledFeeToSettlementCommand AddSettledFeeToSettlementCommand => new(SettlementDate, MerchantId, EstateId, TransactionFeeId, TransactionId);
            public static FloatActivityCommands.RecordCreditPurchaseCommand RecordCreditPurchaseCommand => new(EstateId, FloatAggregateId, CreditPurchasedDateTime, FloatCreditAmount, FloatCreditId);
            public static FloatActivityCommands.RecordTransactionCommand RecordTransactionCommand => new(EstateId, TransactionId);
            public static TransactionCommands.CalculateFeesForTransactionCommand CalculateFeesForTransactionCommand => new(TransactionId, TransactionDateTime, EstateId, MerchantId);
            public static TransactionCommands.AddSettledMerchantFeeCommand AddSettledMerchantFeeCommand => new(TransactionId, CalculatedFeeValue, TransactionFeeCalculateDateTime, CalculationType.Percentage, TransactionFeeId, TransactionFeeValue, SettlementDate, SettlementAggregateId);
            public static TransactionCommands.ProcessSaleTransactionCommand ProcessSaleTransactionCommand =>
                new(TestData.TransactionId, TestData.EstateId,
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
                                                     TestData.TransactionSource,
                                                     TestData.TransactionReceivedDateTime);
            public static TransactionCommands.ProcessReconciliationCommand ProcessReconciliationCommand =>
                new(TestData.TransactionId, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionDateTime, TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            public static TransactionCommands.ProcessLogonTransactionCommand ProcessLogonTransactionCommand =>
                new(TestData.TransactionId,
                    TestData.EstateId,
                    TestData.MerchantId,
                    TestData.DeviceIdentifier,
                    TestData.TransactionTypeLogon.ToString(),
                    TestData.TransactionDateTime,
                    TestData.TransactionNumber,
                    TestData.TransactionReceivedDateTime);

            public static SettlementCommands.ProcessSettlementCommand ProcessSettlementCommand =>
                new(TestData.SettlementDate,
                    TestData.MerchantId,
                    TestData.EstateId);


            public static TransactionCommands.SendCustomerEmailReceiptCommand SendCustomerEmailReceiptCommand => new TransactionCommands.SendCustomerEmailReceiptCommand(EstateId, TransactionId, EventId, CustomerEmailAddress);

            public static MerchantCommands.RemoveMerchantContractCommand RemoveMerchantContractCommand =>
                new MerchantCommands.RemoveMerchantContractCommand(TestData.EstateId,
                    TestData.MerchantId,
                    TestData.ContractId);
            public static MerchantCommands.RemoveOperatorFromMerchantCommand RemoveOperatorFromMerchantCommand => new MerchantCommands.RemoveOperatorFromMerchantCommand(TestData.EstateId, TestData.MerchantId, TestData.OperatorId);

            public static MerchantCommands.UpdateMerchantContactCommand UpdateMerchantContactCommand =>
                new MerchantCommands.UpdateMerchantContactCommand(TestData.EstateId,
                    TestData.MerchantId,
                    Guid.NewGuid(),
                    TestData.Contact);

            public static MerchantCommands.UpdateMerchantAddressCommand UpdateMerchantAddressCommand =>
                new MerchantCommands.UpdateMerchantAddressCommand(TestData.EstateId,
                    TestData.MerchantId,
                    Guid.NewGuid(),
                    TestData.Address);

            public static MerchantCommands.AddMerchantAddressCommand AddMerchantAddressCommand =>
                new MerchantCommands.AddMerchantAddressCommand(TestData.EstateId,
                    TestData.MerchantId,
                    TestData.Address);

            public static MerchantCommands.AddMerchantContactCommand AddMerchantContactCommand =>
                new MerchantCommands.AddMerchantContactCommand(TestData.EstateId,
                    TestData.MerchantId,
                    TestData.Contact);


            public static MerchantCommands.UpdateMerchantCommand UpdateMerchantCommand => new(TestData.EstateId, TestData.MerchantId, TestData.UpdateMerchantRequest);
            public static MerchantCommands.AddMerchantContractCommand AddMerchantContractCommand => new(TestData.EstateId, TestData.MerchantId, TestData.AddMerchantContractRequest);
            public static MerchantCommands.SwapMerchantDeviceCommand SwapMerchantDeviceCommand => new MerchantCommands.SwapMerchantDeviceCommand(TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, TestData.SwapMerchantDeviceRequest);

            public static MerchantCommands.AddMerchantDeviceCommand AddMerchantDeviceCommand => new(TestData.EstateId, TestData.MerchantId, TestData.AddMerchantDeviceRequest);

            public static MerchantCommands.CreateMerchantUserCommand CreateMerchantUserCommand => new(TestData.EstateId, TestData.MerchantId, TestData.CreateMerchantUserRequest);

            public static MerchantCommands.AssignOperatorToMerchantCommand AssignOperatorToMerchantCommand =>
                new MerchantCommands.AssignOperatorToMerchantCommand(TestData.EstateId,
                    TestData.MerchantId,
                    TestData.AssignOperatorRequestToMerchant);

            public static MerchantCommands.CreateMerchantCommand CreateMerchantCommand => new(TestData.EstateId, TestData.CreateMerchantRequest);

            public static ContractCommands.AddTransactionFeeForProductToContractCommand
                AddTransactionFeeForProductToContractCommand(
                    DataTransferObjects.Responses.Contract.CalculationType calculationType,
                    DataTransferObjects.Responses.Contract.FeeType feeType)
            {
                DataTransferObjects.Requests.Contract.AddTransactionFeeForProductToContractRequest x =
                    AddTransactionFeeForProductToContractRequest;
                x.CalculationType = calculationType;
                x.FeeType = feeType;

                ContractCommands.AddTransactionFeeForProductToContractCommand cmd = new(EstateId, ContractId,
                    ContractProductId, TransactionFeeId, x);
                return cmd;
            }

            public static ContractCommands.DisableTransactionFeeForProductCommand DisableTransactionFeeForProductCommand =
                new ContractCommands.DisableTransactionFeeForProductCommand(TestData.EstateId, TestData.ContractId,
                    TestData.ContractProductId, TestData.TransactionFeeId);

            public static ContractCommands.AddProductToContractCommand AddProductToContractCommand_FixedValue =>
                new(EstateId, ContractId, ContractProductId, TestData.AddProductToContractRequest_FixedValue);

            public static ContractCommands.AddProductToContractCommand AddProductToContractCommand_VariableValue =>
                new(EstateId, ContractId, ContractProductId, TestData.AddProductToContractRequest_VariableValue);

            public static ContractCommands.CreateContractCommand CreateContractCommand =>
                new ContractCommands.CreateContractCommand(EstateId, ContractId, CreateContractRequest);
            public static EstateCommands.CreateEstateUserCommand CreateEstateUserCommand => new EstateCommands.CreateEstateUserCommand(TestData.EstateId, TestData.CreateEstateUserRequest);
            public static EstateCommands.AddOperatorToEstateCommand AddOperatorToEstateCommand => new EstateCommands.AddOperatorToEstateCommand(TestData.EstateId, TestData.AssignOperatorRequestToEstate);
            public static EstateCommands.CreateEstateCommand CreateEstateCommand => new EstateCommands.CreateEstateCommand(TestData.CreateEstateRequest);
            public static EstateCommands.RemoveOperatorFromEstateCommand RemoveOperatorFromEstateCommand => new(TestData.EstateId, TestData.OperatorId);

            public static OperatorCommands.CreateOperatorCommand CreateOperatorCommand => new(TestData.EstateId, TestData.CreateOperatorRequest);

            public static OperatorCommands.UpdateOperatorCommand UpdateOperatorCommand => new(TestData.EstateId, TestData.OperatorId, TestData.UpdateOperatorRequest);
            public static MerchantCommands.MakeMerchantWithdrawalCommand MakeMerchantWithdrawalCommand =>
                new(TestData.EstateId,
                    TestData.MerchantId,
                    TestData.MakeMerchantWithdrawalRequest);

            public static MerchantCommands.MakeMerchantDepositCommand MakeMerchantDepositCommand =>
                new(TestData.EstateId,
                    TestData.MerchantId,
                    MerchantDepositSourceManualDTO,
                    TestData.MakeMerchantDepositRequest);
        }
        public static Guid SettlementId = Guid.Parse("7CF02BE4-4BF0-4BB2-93C1-D6E5EC769E56");
        public static String StartDate = "20210104";
        public static String EndDate = "20210105";
        public static class Queries {
            public static SettlementQueries.GetSettlementQuery GetSettlementQuery =>
                new SettlementQueries.GetSettlementQuery(EstateId, MerchantId, SettlementId);

            public static SettlementQueries.GetSettlementsQuery GetSettlementsQuery =>
                new SettlementQueries.GetSettlementsQuery(EstateId, MerchantId, StartDate, EndDate);
            public static VoucherQueries.GetVoucherByVoucherCodeQuery GetVoucherByVoucherCodeQuery => new(EstateId, VoucherCode);
            public static VoucherQueries.GetVoucherByTransactionIdQuery GetVoucherByTransactionIdQuery => new(EstateId, TransactionId);

            public static SettlementQueries.GetPendingSettlementQuery GetPendingSettlementQuery => new(SettlementDate, MerchantId, EstateId);

            public static MerchantQueries.GetMerchantBalanceQuery GetMerchantBalanceQuery => new(EstateId, MerchantId);

            public static MerchantQueries.GetMerchantLiveBalanceQuery GetMerchantLiveBalanceQuery => new(MerchantId);

            public static MerchantQueries.GetMerchantBalanceHistoryQuery GetMerchantBalanceHistoryQuery => new MerchantQueries.GetMerchantBalanceHistoryQuery(EstateId, MerchantId, DateTime.MinValue, DateTime.MaxValue);

            public static MerchantQueries.GetTransactionFeesForProductQuery GetTransactionFeesForProductQuery =>
                new MerchantQueries.GetTransactionFeesForProductQuery(TestData.EstateId,
                    TestData.MerchantId,
                    TestData.ContractId,
                    TestData.ContractProductId);
            public static MerchantQueries.GetMerchantContractsQuery GetMerchantContractsQuery => new MerchantQueries.GetMerchantContractsQuery(TestData.EstateId, TestData.MerchantId);
            public static MerchantQueries.GetMerchantQuery GetMerchantQuery => new MerchantQueries.GetMerchantQuery(TestData.EstateId, TestData.MerchantId);

            public static MerchantQueries.GetMerchantsQuery GetMerchantsQuery => new MerchantQueries.GetMerchantsQuery(TestData.EstateId);
            public static EstateQueries.GetEstateQuery GetEstateQuery => new(TestData.EstateId);
            public static EstateQueries.GetEstatesQuery GetEstatesQuery => new(TestData.EstateId);

            public static OperatorQueries.GetOperatorQuery GetOperatorQuery => new(TestData.EstateId, TestData.OperatorId);
            public static OperatorQueries.GetOperatorsQuery GetOperatorsQuery => new(TestData.EstateId);
            public static ContractQueries.GetContractQuery GetContractQuery => new(EstateId, ContractId);
            public static ContractQueries.GetContractsQuery GetContractsQuery => new(EstateId);
        }

        public static class Aggregates {
            public static MerchantAggregate MerchantAggregateWithAddress()
            {
                MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

                merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
                merchantAggregate.AddAddress(TestData.MerchantAddressLine1,
                    TestData.MerchantAddressLine2,
                    TestData.MerchantAddressLine3,
                    TestData.MerchantAddressLine4,
                    TestData.MerchantTown,
                    TestData.MerchantRegion,
                    TestData.MerchantPostalCode,
                    TestData.MerchantCountry);

                return merchantAggregate;
            }

            public static MerchantAggregate MerchantAggregateWithContact()
            {
                MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

                merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
                merchantAggregate.AddContact(TestData.MerchantContactName,
                    TestData.MerchantContactPhoneNumber,
                    TestData.MerchantContactEmailAddress);

                return merchantAggregate;
            }

            public static MerchantDepositListAggregate EmptyMerchantDepositListAggregate = new MerchantDepositListAggregate();

            public static MerchantDepositListAggregate CreatedMerchantDepositListAggregate()
            {
                MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

                merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

                MerchantDepositListAggregate merchantDepositListAggregate = MerchantDepositListAggregate.Create(TestData.MerchantId);
                merchantDepositListAggregate.Create(merchantAggregate, TestData.DateMerchantCreated);

                return merchantDepositListAggregate;
            }
            public static MerchantAggregate MerchantAggregateWithDevice()
            {
                MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

                merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
                merchantAggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);

                return merchantAggregate;
            }

            public static MerchantAggregate MerchantAggregateWithOperator()
            {
                MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

                merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
                merchantAggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);

                return merchantAggregate;
            }

            public static MerchantAggregate MerchantAggregateWithEverything(Models.Merchant.SettlementSchedule settlementSchedule)
            {
                MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

                merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
                merchantAggregate.AddContact(TestData.MerchantContactName,
                    TestData.MerchantContactPhoneNumber,
                    TestData.MerchantContactEmailAddress);
                merchantAggregate.AddAddress(TestData.MerchantAddressLine1,
                    TestData.MerchantAddressLine2,
                    TestData.MerchantAddressLine3,
                    TestData.MerchantAddressLine4,
                    TestData.MerchantTown,
                    TestData.MerchantRegion,
                    TestData.MerchantPostalCode,
                    TestData.MerchantCountry);
                merchantAggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);
                merchantAggregate.SetSettlementSchedule(settlementSchedule);
                merchantAggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);
                merchantAggregate.AddContract(TestData.Aggregates.CreatedContractAggregateWithAProductAndTransactionFee(CalculationType.Fixed,Models.Contract.FeeType.Merchant));
                return merchantAggregate;
            }

            public static MerchantAggregate MerchantAggregateWithNoContracts(SettlementScheduleModel settlementSchedule)
            {
                MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

                merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
                merchantAggregate.AddContact(TestData.MerchantContactName,
                    TestData.MerchantContactPhoneNumber,
                    TestData.MerchantContactEmailAddress);
                merchantAggregate.AddAddress(TestData.MerchantAddressLine1,
                    TestData.MerchantAddressLine2,
                    TestData.MerchantAddressLine3,
                    TestData.MerchantAddressLine4,
                    TestData.MerchantTown,
                    TestData.MerchantRegion,
                    TestData.MerchantPostalCode,
                    TestData.MerchantCountry);
                merchantAggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);
                merchantAggregate.SetSettlementSchedule(settlementSchedule);
                merchantAggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);
                return merchantAggregate;
            }

            public static MerchantAggregate MerchantAggregateWithDeletedOperator(SettlementScheduleModel settlementSchedule)
            {
                MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

                merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);
                merchantAggregate.AddContact(TestData.MerchantContactName,
                    TestData.MerchantContactPhoneNumber,
                    TestData.MerchantContactEmailAddress);
                merchantAggregate.AddAddress(TestData.MerchantAddressLine1,
                    TestData.MerchantAddressLine2,
                    TestData.MerchantAddressLine3,
                    TestData.MerchantAddressLine4,
                    TestData.MerchantTown,
                    TestData.MerchantRegion,
                    TestData.MerchantPostalCode,
                    TestData.MerchantCountry);
                merchantAggregate.AssignOperator(TestData.OperatorId, TestData.OperatorName, TestData.OperatorMerchantNumber, TestData.OperatorTerminalNumber);
                merchantAggregate.SetSettlementSchedule(settlementSchedule);
                merchantAggregate.AddDevice(TestData.DeviceId, TestData.DeviceIdentifier);
                merchantAggregate.RemoveOperator(TestData.OperatorId);
                return merchantAggregate;
            }

            public static MerchantAggregate CreatedMerchantAggregate()
            {
                MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

                merchantAggregate.Create(TestData.EstateId, TestData.MerchantName, TestData.DateMerchantCreated);

                return merchantAggregate;
            }
            public static MerchantAggregate EmptyMerchantAggregate()
            {
                MerchantAggregate merchantAggregate = MerchantAggregate.Create(TestData.MerchantId);

                return merchantAggregate;
            }
            public static OperatorAggregate EmptyOperatorAggregate()
            {
                OperatorAggregate operatorAggregate = OperatorAggregate.Create(TestData.OperatorId);

                return operatorAggregate;
            }

            public static ContractAggregate EmptyContractAggregate()
            {
                ContractAggregate contractAggregate = ContractAggregate.Create(TestData.ContractId);

                return contractAggregate;
            }
            public static ContractAggregate CreatedContractAggregate()
            {
                ContractAggregate contractAggregate = ContractAggregate.Create(TestData.ContractId);

                contractAggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

                return contractAggregate;
            }

            public static ContractAggregate CreatedContractAggregateWithAProduct()
            {
                ContractAggregate contractAggregate = ContractAggregate.Create(TestData.ContractId);

                contractAggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
                contractAggregate.AddFixedValueProduct(TestData.ContractProductId,
                                                       TestData.ProductName,
                                                       TestData.ProductDisplayText,
                                                       TestData.ProductFixedValue,
                                                       TestData.ProductTypeMobileTopup);

                return contractAggregate;
            }

            public static ContractAggregate CreatedContractAggregateWithAProductAndTransactionFee(CalculationType calculationType, FeeType feeType)
            {
                ContractAggregate contractAggregate = ContractAggregate.Create(TestData.ContractId);

                contractAggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
                contractAggregate.AddFixedValueProduct(TestData.ContractProductId,
                                                       TestData.ProductName,
                                                       TestData.ProductDisplayText,
                                                       TestData.ProductFixedValue,
                                                       TestData.ProductTypeMobileTopup);

                Product product = contractAggregate.GetProducts().Single(p => p.ContractProductId == TestData.ContractProductId);
                contractAggregate.AddTransactionFee(product,
                                                    TestData.TransactionFeeId,
                                                    TestData.TransactionFeeDescription,
                                                    calculationType,
                                                    feeType,
                                                    TestData.TransactionFeeValue);

                return contractAggregate;
            }

            public static OperatorAggregate CreatedOperatorAggregate()
            {
                OperatorAggregate operatorAggregate = OperatorAggregate.Create(TestData.OperatorId);
                operatorAggregate.Create(TestData.EstateId, TestData.OperatorName, TestData.RequireCustomMerchantNumber, TestData.RequireCustomTerminalNumber);
                return operatorAggregate;
            }

            public static EstateAggregate EmptyEstateAggregate = EstateAggregate.Create(TestData.EstateId);

            public static EstateAggregate CreatedEstateAggregate() {
                EstateAggregate estateAggregate = EstateAggregate.Create(TestData.EstateId);

                estateAggregate.Create(TestData.EstateName);

                return estateAggregate;
            }

            public static EstateAggregate EstateAggregateWithOperator() {
                EstateAggregate estateAggregate = EstateAggregate.Create(TestData.EstateId);

                estateAggregate.Create(TestData.EstateName);
                estateAggregate.AddOperator(TestData.OperatorId);

                return estateAggregate;
            }
            public static EstateAggregate EstateAggregateWithOperator2()
            {
                EstateAggregate estateAggregate = EstateAggregate.Create(TestData.EstateId);

                estateAggregate.Create(TestData.EstateName);
                estateAggregate.AddOperator(TestData.OperatorId2);

                return estateAggregate;
            }
            public static EstateAggregate EstateAggregateWithOperatorDeleted()
            {
                EstateAggregate estateAggregate = EstateAggregate.Create(TestData.EstateId);

                estateAggregate.Create(TestData.EstateName);
                estateAggregate.AddOperator(TestData.OperatorId);
                estateAggregate.RemoveOperator(TestData.OperatorId);

                return estateAggregate;
            }

            public static MerchantStatementForDateAggregate EmptyMerchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(MerchantStatementForDateId1);

            public static MerchantStatementForDateAggregate MerchantStatementForDateAggregateWithTransactionAndFee() {
                MerchantStatementForDateAggregate aggregate = MerchantStatementForDateAggregate.Create(MerchantStatementForDateId1);
                aggregate.AddTransactionToStatement(MerchantStatementForDateId1, StatementDate, Guid.NewGuid(), EstateId,MerchantId, Transaction1);
                aggregate.AddSettledFeeToStatement(MerchantStatementForDateId1, StatementDate, Guid.NewGuid(), EstateId, MerchantId, SettledFee1);
                return aggregate;
            }

            public static MerchantStatementAggregate EmptyMerchantStatementAggregate = MerchantStatementAggregate.Create(MerchantStatementId);

            public static MerchantStatementAggregate MerchantStatementAggregateWithActivityDates() {
                MerchantStatementAggregate aggregate = MerchantStatementAggregate.Create(MerchantStatementId);
                aggregate.RecordActivityDateOnStatement(MerchantStatementId, StatementDate, EstateId, MerchantId, MerchantStatementForDateId1, ActivityDate1);
                return aggregate;
            }

            public static MerchantStatementAggregate GeneratedMerchantStatementAggregate()
            {
                MerchantStatementAggregate aggregate = MerchantStatementAggregate.Create(MerchantStatementId);
                aggregate.RecordActivityDateOnStatement(MerchantStatementId, StatementDate, EstateId, MerchantId, MerchantStatementForDateId1, ActivityDate1);
                aggregate.AddDailySummaryRecord(ActivityDate1, 1, 100, 1,0.10m, 1,1000, 1,200);
                aggregate.GenerateStatement(StatementGeneratedDate);
                return aggregate;
            }

            public static MerchantStatementAggregate BuiltMerchantStatementAggregate()
            {
                MerchantStatementAggregate aggregate = MerchantStatementAggregate.Create(MerchantStatementId);
                aggregate.RecordActivityDateOnStatement(MerchantStatementId, StatementDate, EstateId, MerchantId, MerchantStatementForDateId1, ActivityDate1);
                aggregate.AddDailySummaryRecord(ActivityDate1, 1, 100, 1, 0.10m, 1, 1000, 1, 200);
                aggregate.GenerateStatement(StatementGeneratedDate);
                aggregate.BuildStatement(StatementBuiltDate,StatementData);
                return aggregate;
            }
        }
        public static DateTime NextSettlementDate = new DateTime(2021, 8, 30);
        public static SettlementSchedule SettlementSchedule = SettlementSchedule.Immediate;
        public static Guid AddressId = Guid.Parse("B1C68246-F867-43CC-ACA9-37D15D6437C6");
        public static Guid ContactId = Guid.Parse("B1C68246-F867-43CC-ACA9-37D15D6437C6");
        public static Guid MerchantSecurityUserId = Guid.Parse("DFCE7A95-CB6D-442A-928A-F1B41D2AA4A9");

        public static Guid CallbackId = Guid.Parse("ABC603D3-360E-4F58-8BB9-827EE7A1CB03");

        public static String CallbackMessage = "Message1";

        public static Int32 CallbackMessageFormat = 1;

        public static String CallbackReference = "Estate1-Merchant1";

        public static String CallbackTypeString = "CallbackHandler.DataTransferObjects.Deposit";
        public static Guid DepositId = Guid.Parse("A15460B1-9665-4C3E-861D-3B65D0EBEF19");
        public static String DepositAccountNumber = "12345678";
        public static Guid DepositHostIdentifier = Guid.Parse("1D1BD9F0-D953-4B2A-9969-98D3C0CDFA2A");
        public static String DepositSortCode = "112233";

        public static Models.Merchant.Merchant MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts(Models.Merchant.SettlementSchedule settlementSchedule = Models.Merchant.SettlementSchedule.Immediate) =>
            new Models.Merchant.Merchant
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                SettlementSchedule = settlementSchedule,
                Addresses = new List<Models.Merchant.Address>{
                    new Models.Merchant.Address(Guid.NewGuid(), MerchantAddressLine1,MerchantAddressLine2,
                        MerchantAddressLine3,MerchantAddressLine4, MerchantTown,MerchantRegion,
                        MerchantPostalCode, MerchantCountry)
                },
                Contacts = new List<Models.Merchant.Contact>{
                    new Models.Merchant.Contact(Guid.NewGuid(), MerchantContactEmailAddress, MerchantContactName, MerchantContactPhoneNumber)
                },
                Devices = new List<Device>{ new Device(DeviceId, DeviceIdentifier)},
                Operators= new List<Models.Merchant.Operator>() {
                    new Models.Merchant.Operator(OperatorId, OperatorName, OperatorMerchantNumber, OperatorTerminalNumber)
                },
                Contracts = new List<Models.Merchant.Contract>() {
                    new (ContractId) {
                        ContractProducts = new List<Guid> {
                            Guid.Parse("8EF716B9-422D-4FC6-B5A7-22FC4BABDD97")
                        }
                    }
                }
            };

        public static Deposit Deposit =>
            new Deposit
            {
                Reference = TestData.CallbackReference,
                Amount = TestData.DepositAmount.Value,
                DateTime = TestData.DepositDateTime,
                DepositId = TestData.DepositId,
                AccountNumber = TestData.DepositAccountNumber,
                HostIdentifier = TestData.DepositHostIdentifier,
                SortCode = TestData.DepositSortCode
            };

        public static class DomainEvents {

            public static TransactionDomainEvents.TransactionHasBeenCompletedEvent TransactionHasBeenCompletedEvent => new TransactionDomainEvents.TransactionHasBeenCompletedEvent(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.ResponseCode,
                TestData.ResponseMessage,
                TestData.IsAuthorised,
                TestData.TransactionDateTime,
                TestData.TransactionAmount,
                TestData.TransactionDateTime);

            public static MerchantDomainEvents.AutomaticDepositMadeEvent AutomaticDepositMadeEvent => new(MerchantId, EstateId, DepositId, DepositReference, DepositDateTime, DepositAmount.Value);
            public static MerchantDomainEvents.ManualDepositMadeEvent ManualDepositMadeEvent => new(MerchantId, EstateId, DepositId, DepositReference, DepositDateTime, DepositAmount.Value);

            public static MerchantDomainEvents.WithdrawalMadeEvent WithdrawalMadeEvent => new(MerchantId, EstateId, WithdrawalId, WithdrawalDateTime, WithdrawalAmount.Value);
            public static MerchantStatementForDateDomainEvents.StatementCreatedForDateEvent StatementCreatedForDateEvent => new(MerchantStatementForDateId1, ActivityDate1, StatementDate, MerchantStatementId, EstateId, MerchantId);

            public static SettlementDomainEvents.MerchantFeeSettledEvent MerchantFeeSettledEvent => new(SettlementId, EstateId, MerchantId, TransactionId, CalculatedFeeValue, FeeCalculationType, SettledFeeId1, FeeValue, TransactionFeeCalculateDateTime, SettlementDate);

            public static MerchantStatementDomainEvents.StatementCreatedEvent StatementCreatedEvent => new(TestData.MerchantStatementId, TestData.EstateId, TestData.MerchantId, StatementDate);

            public static MerchantStatementDomainEvents.StatementGeneratedEvent StatementGeneratedEvent => new MerchantStatementDomainEvents.StatementGeneratedEvent(TestData.MerchantStatementId, TestData.EstateId, TestData.MerchantId, TestData.StatementGeneratedDate);

            public static MerchantStatementDomainEvents.StatementBuiltEvent StatementBuiltEvent => new MerchantStatementDomainEvents.StatementBuiltEvent(TestData.MerchantStatementId, TestData.EstateId, TestData.MerchantId, TestData.StatementGeneratedDate, Convert.ToBase64String(ASCIIEncoding.Default.GetBytes(StatementData)));

            public static CallbackReceivedEnrichedEvent CallbackReceivedEnrichedEventDeposit =>
                new CallbackReceivedEnrichedEvent(TestData.CallbackId)
                {
                    Reference = TestData.CallbackReference,
                    CallbackMessage = JsonConvert.SerializeObject(TestData.Deposit),
                    EstateId = TestData.EstateId,
                    MessageFormat = TestData.CallbackMessageFormat,
                    TypeString = TestData.CallbackTypeString
                };

            public static CallbackReceivedEnrichedEvent CallbackReceivedEnrichedEventOtherType =>
                new CallbackReceivedEnrichedEvent(TestData.CallbackId)
                {
                    Reference = TestData.CallbackReference,
                    CallbackMessage = JsonConvert.SerializeObject(TestData.Deposit),
                    EstateId = TestData.EstateId,
                    MessageFormat = TestData.CallbackMessageFormat,
                    TypeString = "OtherType"
                };

            public static MerchantDomainEvents.ContractAddedToMerchantEvent ContractAddedToMerchantEvent =>
                new MerchantDomainEvents.ContractAddedToMerchantEvent(TestData.MerchantId,
                    TestData.EstateId,
                    TestData.ContractId);

            public static MerchantDomainEvents.AddressAddedEvent AddressAddedEvent =>
                new MerchantDomainEvents.AddressAddedEvent(TestData.MerchantId,
                    TestData.EstateId,
                    TestData.AddressId,
                    TestData.AddressLine1,
                    TestData.AddressLine2,
                    TestData.AddressLine3,
                    TestData.AddressLine4,
                    TestData.Town,
                    TestData.Region,
                    TestData.PostCode,
                    TestData.Country);

            public static MerchantDomainEvents.ContactAddedEvent ContactAddedEvent =>
                new MerchantDomainEvents.ContactAddedEvent(TestData.MerchantId,
                    TestData.EstateId,
                    TestData.ContactId,
                    TestData.ContactName,
                    TestData.ContactPhone,
                    TestData.ContactEmail);

            public static MerchantDomainEvents.MerchantReferenceAllocatedEvent MerchantReferenceAllocatedEvent => new MerchantDomainEvents.MerchantReferenceAllocatedEvent(TestData.MerchantId, TestData.EstateId, TestData.MerchantReference);

            public static MerchantDomainEvents.DeviceAddedToMerchantEvent DeviceAddedToMerchantEvent => new MerchantDomainEvents.DeviceAddedToMerchantEvent(TestData.MerchantId, TestData.EstateId, TestData.DeviceId, TestData.DeviceIdentifier);
            public static MerchantDomainEvents.MerchantCreatedEvent MerchantCreatedEvent => new MerchantDomainEvents.MerchantCreatedEvent(TestData.MerchantId, TestData.EstateId, TestData.MerchantName, DateTime.Now);

            public static MerchantDomainEvents.OperatorAssignedToMerchantEvent OperatorAssignedToMerchantEvent =>
                new MerchantDomainEvents.OperatorAssignedToMerchantEvent(TestData.MerchantId,
                    TestData.EstateId,
                    TestData.OperatorId,
                    TestData.OperatorName,
                    TestData.MerchantNumber,
                    TestData.TerminalNumber);

            public static MerchantDomainEvents.SecurityUserAddedToMerchantEvent MerchantSecurityUserAddedEvent => new MerchantDomainEvents.SecurityUserAddedToMerchantEvent(TestData.MerchantId, TestData.EstateId, TestData.MerchantSecurityUserId, TestData.EmailAddress);
            public static MerchantDomainEvents.SettlementScheduleChangedEvent SettlementScheduleChangedEvent => new MerchantDomainEvents.SettlementScheduleChangedEvent(TestData.MerchantId, TestData.EstateId, (Int32)TestData.SettlementSchedule, TestData.NextSettlementDate);
            //public static StatementGeneratedEvent StatementGeneratedEvent => new StatementGeneratedEvent(TestData.MerchantStatementId, TestData.EstateId, TestData.MerchantId, TestData.StatementGeneratedDate);
            public static MerchantDomainEvents.MerchantNameUpdatedEvent MerchantNameUpdatedEvent => new MerchantDomainEvents.MerchantNameUpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.MerchantNameUpdated);
            public static MerchantDomainEvents.DeviceSwappedForMerchantEvent DeviceSwappedForMerchantEvent =>
                new MerchantDomainEvents.DeviceSwappedForMerchantEvent(TestData.MerchantId,
                    TestData.EstateId,
                    TestData.DeviceId,
                    TestData.DeviceIdentifier,
                    TestData.NewDeviceIdentifier);

            public static MerchantDomainEvents.OperatorRemovedFromMerchantEvent OperatorRemovedFromMerchantEvent => new MerchantDomainEvents.OperatorRemovedFromMerchantEvent(TestData.MerchantId, TestData.EstateId, TestData.OperatorId);
            public static MerchantDomainEvents.MerchantAddressLine1UpdatedEvent MerchantAddressLine1UpdatedEvent => new MerchantDomainEvents.MerchantAddressLine1UpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.AddressId, TestData.AddressLine1);
            public static MerchantDomainEvents.MerchantAddressLine2UpdatedEvent MerchantAddressLine2UpdatedEvent => new MerchantDomainEvents.MerchantAddressLine2UpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.AddressId, TestData.AddressLine2);
            public static MerchantDomainEvents.MerchantAddressLine3UpdatedEvent MerchantAddressLine3UpdatedEvent => new MerchantDomainEvents.MerchantAddressLine3UpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.AddressId, TestData.AddressLine3);
            public static MerchantDomainEvents.MerchantAddressLine4UpdatedEvent MerchantAddressLine4UpdatedEvent => new MerchantDomainEvents.MerchantAddressLine4UpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.AddressId, TestData.AddressLine4);
            public static MerchantDomainEvents.MerchantContactEmailAddressUpdatedEvent MerchantContactEmailAddressUpdatedEvent => new MerchantDomainEvents.MerchantContactEmailAddressUpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.ContactId, TestData.ContactEmailUpdate);
            public static MerchantDomainEvents.MerchantContactNameUpdatedEvent MerchantContactNameUpdatedEvent => new MerchantDomainEvents.MerchantContactNameUpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.ContactId, TestData.ContactNameUpdate);
            public static MerchantDomainEvents.MerchantContactPhoneNumberUpdatedEvent MerchantContactPhoneNumberUpdatedEvent => new MerchantDomainEvents.MerchantContactPhoneNumberUpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.ContactId, TestData.ContactPhoneUpdate);
            public static MerchantDomainEvents.MerchantCountyUpdatedEvent MerchantCountyUpdatedEvent => new MerchantDomainEvents.MerchantCountyUpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.AddressId, TestData.Country);
            public static MerchantDomainEvents.MerchantPostalCodeUpdatedEvent MerchantPostalCodeUpdatedEvent => new MerchantDomainEvents.MerchantPostalCodeUpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.AddressId, TestData.PostCode);

            public static MerchantDomainEvents.MerchantRegionUpdatedEvent MerchantRegionUpdatedEvent => new MerchantDomainEvents.MerchantRegionUpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.AddressId, TestData.Region);
            public static MerchantDomainEvents.MerchantTownUpdatedEvent MerchantTownUpdatedEvent => new MerchantDomainEvents.MerchantTownUpdatedEvent(TestData.MerchantId, TestData.EstateId, TestData.AddressId, TestData.Town);

            public static ContractDomainEvents.TransactionFeeForProductAddedToContractEvent TransactionFeeForProductAddedToContractEvent =>
                new ContractDomainEvents.TransactionFeeForProductAddedToContractEvent(TestData.ContractId,
                    TestData.EstateId,
                    TestData.ContractProductId,
                    TestData.TransactionFeeId,
                    TestData.TransactionFeeDescription,
                    TestData.FeeCalculationType,
                    TestData.FeeType,
                    TestData.FeeValue);

            public static ContractDomainEvents.TransactionFeeForProductDisabledEvent TransactionFeeForProductDisabledEvent => new ContractDomainEvents.TransactionFeeForProductDisabledEvent(TestData.ContractId, TestData.EstateId, TestData.ContractProductId, TestData.TransactionFeeId);
            public static ContractDomainEvents.VariableValueProductAddedToContractEvent VariableValueProductAddedToContractEvent => new ContractDomainEvents.VariableValueProductAddedToContractEvent(TestData.ContractId, TestData.EstateId, TestData.ContractProductId, TestData.ProductName, TestData.ProductDisplayText, (Int32)TestData.ProductTypeMobileTopup);
            public static ContractDomainEvents.FixedValueProductAddedToContractEvent FixedValueProductAddedToContractEvent =>
                new ContractDomainEvents.FixedValueProductAddedToContractEvent(TestData.ContractId,
                    TestData.EstateId,
                    TestData.ContractProductId,
                    TestData.ProductName,
                    TestData.ProductDisplayText,
                    TestData.ProductFixedValue,
                    (Int32)TestData.ProductTypeMobileTopup);
            public static ContractDomainEvents.ContractCreatedEvent ContractCreatedEvent => new ContractDomainEvents.ContractCreatedEvent(TestData.ContractId, TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            public static EstateDomainEvents.EstateCreatedEvent EstateCreatedEvent => new EstateDomainEvents.EstateCreatedEvent(TestData.EstateId, TestData.EstateName);
            public static EstateDomainEvents.EstateReferenceAllocatedEvent EstateReferenceAllocatedEvent => new EstateDomainEvents.EstateReferenceAllocatedEvent(TestData.EstateId, TestData.EstateReference);

            public static EstateDomainEvents.SecurityUserAddedToEstateEvent EstateSecurityUserAddedEvent =
                new EstateDomainEvents.SecurityUserAddedToEstateEvent(TestData.EstateId, TestData.EstateSecurityUserId, TestData.EmailAddress);

            public static OperatorDomainEvents.OperatorCreatedEvent OperatorCreatedEvent = new OperatorDomainEvents.OperatorCreatedEvent(TestData.OperatorId,
                TestData.EstateId,
                TestData.OperatorName,
                TestData.RequireCustomMerchantNumber,
                TestData.RequireCustomTerminalNumber);

            public static OperatorDomainEvents.OperatorNameUpdatedEvent OperatorNameUpdatedEvent => new(TestData.OperatorId, TestData.EstateId, TestData.OperatorName2);
            public static OperatorDomainEvents.OperatorRequireCustomMerchantNumberChangedEvent OperatorRequireCustomMerchantNumberChangedEvent => new(TestData.OperatorId, TestData.EstateId, TestData.RequireCustomMerchantNumberFalse);
            public static OperatorDomainEvents.OperatorRequireCustomTerminalNumberChangedEvent OperatorRequireCustomTerminalNumberChangedEvent => new(TestData.OperatorId, TestData.EstateId, TestData.RequireCustomTerminalNumberFalse);
        }
    }


}