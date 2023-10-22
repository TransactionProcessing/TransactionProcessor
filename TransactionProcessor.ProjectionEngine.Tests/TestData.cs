namespace TransactionProcessor.ProjectionEngine.Tests
{
    using EstateManagement.Merchant.DomainEvents;
    using Transaction.DomainEvents;

    public class TestData{
        public static Guid MerchantId = Guid.Parse("1FDEF549-4BDA-4DA3-823B-79684CD93F88");

        public static Guid EstateId = Guid.Parse("C81CD4E6-1F3B-431F-AA63-0ACAB7BC0CD3");

        public static String MerchantName = "Test Merchant 1";

        public static DateTime CreatedDateTime = new DateTime(2022, 10, 13, 1, 2, 3);

        public static DateTime ManualDepositDateTime = new DateTime(2022, 10, 13, 2, 2, 3);

        public static Guid ManualDepositId = Guid.Parse("7BDB5FD8-F944-4864-844E-EF12F7B16A4F");

        public static String ManualDepositReference = "Manual Deposit 1";

        public static Decimal ManualDepositAmount = 100.00m;

        public static DateTime AutomaticDepositDateTime = new DateTime(2022, 10, 13, 3, 2, 3);

        public static DateTime WithdrawalDateTime = new DateTime(2022, 10, 13, 3, 2, 3);

        public static Guid AutomaticDepositId = Guid.Parse("520521a1f9504ec1bf1cc5a7fd4fd905");

        public static String AutomaticDepositReference = "Automatic Deposit 1";

        public static Decimal AutomaticDepositAmount = 200.00m;

        public static Decimal WithdrawalAmount = 100.00m;

        public static Guid TransactionId = Guid.Parse("58306666-746C-4984-B264-4ECF15749BF5");

        public static DateTime TransactionDateTime = new DateTime(2022, 10, 13, 7, 30, 0);

        public static String TransactionNumber = "1";

        public static String TransactionType = "Sale";

        public static String TransactionReference = "Test Reference 1";

        public static String DeviceIdentifier = "TestDevice1";

        public static Decimal? TransactionAmount = 25.00m;

        public static Int32 FeeCalculationType = 1;

        public static Guid FeeId = Guid.Parse("BDE02D5A-3489-4A0A-9A69-9DCEA661B9D9");

        public static Decimal FeeValue = 0.25m;

        public static DateTime FeeCalculatedDateTime = new DateTime(2022, 10, 13, 8, 30, 0);

        public static DateTime SettlementDueDate = new DateTime(2022, 10, 13, 8, 31, 0);

        public static DateTime SettledDateTime = new DateTime(2022, 10, 13, 8, 31, 0);

        public static String ResponseCode = "ResponseCode";

        public static String ResponseMessage = "ResponseMessage";

        public static Guid ManualWithdrawalId = Guid.Parse("4DCDA910-53E4-40F8-AF4B-FEEAC2338739");

        public static AddressAddedEvent AddressAddedEvent = new AddressAddedEvent(TestData.MerchantId,
                                                                                  TestData.EstateId,
                                                                                  TestData.AddressId,
                                                                                  TestData.AddressLine1,
                                                                                  TestData.AddressLine2,
                                                                                  TestData.AddressLine3,
                                                                                  TestData.AddressLine4,
                                                                                  TestData.Town,
                                                                                  TestData.Region,
                                                                                  TestData.PostalCode,
                                                                                  TestData.Country);

        public static Guid AddressId = Guid.Parse("A7162F57-EB8E-4E9B-A18E-C38C687DA3C0");

        public static String AddressLine1 = "AddressLine1";

        public static String AddressLine2 = "AddressLine2";

        public static String AddressLine3 = "AddressLine3";

        public static String AddressLine4 = "AddressLine4";

        public static String Town = "Town";

        public static String Region = "Region";

        public static String PostalCode = "PostalCode";

        public static String Country = "Country";

        public static SettledMerchantFeeAddedToTransactionEvent GetSettledMerchantFeeAddedToTransactionEvent(Decimal? calculatedFeeValue = 1.25m) =>
            new SettledMerchantFeeAddedToTransactionEvent(TestData.TransactionId,
                                                          TestData.EstateId,
                                                          TestData.MerchantId,
                                                          calculatedFeeValue.Value,
                                                          TestData.FeeCalculationType,
                                                          TestData.FeeId,
                                                          TestData.FeeValue,
                                                          TestData.FeeCalculatedDateTime,
                                                          TestData.SettledDateTime);
        
        public static TransactionHasBeenCompletedEvent GetTransactionHasBeenCompletedEvent(Boolean? isAuthorised = true,
                                                                                           Decimal? amount = null){
            if (amount == null)
                amount = TestData.TransactionAmount.GetValueOrDefault(0);

            return new TransactionHasBeenCompletedEvent(TestData.TransactionId,
                                                 TestData.EstateId,
                                                 TestData.MerchantId,
                                                 TestData.ResponseCode,
                                                 TestData.ResponseMessage,
                                                 isAuthorised.Value,
                                                 TestData.TransactionDateTime,
                                                 amount);
        }

        public static TransactionHasStartedEvent GetTransactionHasStartedEvent(Decimal? amount = null, String type = null){
                if (amount == null)
                    amount = TestData.TransactionAmount.GetValueOrDefault(0);
                if (type == null)
                    type = TestData.TransactionType;

                return new TransactionHasStartedEvent(TestData.TransactionId,
                                                      TestData.EstateId,
                                                      TestData.MerchantId,
                                                      TestData.TransactionDateTime,
                                                      TestData.TransactionNumber,
                                                      type,
                                                      TestData.TransactionReference,
                                                      TestData.DeviceIdentifier,
                                                      amount);
            }

        public static MerchantCreatedEvent MerchantCreatedEvent =>
            new MerchantCreatedEvent(TestData.MerchantId, TestData.EstateId, TestData.MerchantName, TestData.CreatedDateTime);

        public static ManualDepositMadeEvent ManualDepositMadeEvent =>
            new ManualDepositMadeEvent(TestData.MerchantId, TestData.EstateId, TestData.ManualDepositId, 
                                       TestData.ManualDepositReference, TestData.ManualDepositDateTime,
                                       TestData.ManualDepositAmount);

        public static AutomaticDepositMadeEvent AutomaticDepositMadeEvent =>
            new AutomaticDepositMadeEvent(TestData.MerchantId, TestData.EstateId, TestData.AutomaticDepositId,
                                          TestData.AutomaticDepositReference, TestData.AutomaticDepositDateTime,
                                          TestData.AutomaticDepositAmount);

        public static WithdrawalMadeEvent WithdrawalMadeEvent =>
            new WithdrawalMadeEvent(TestData.MerchantId, TestData.EstateId, TestData.ManualWithdrawalId,
                                    TestData.WithdrawalDateTime,
                                    TestData.WithdrawalAmount);

        public static WithdrawalMadeEvent WithdrawalMadeEvent1 =>
            new WithdrawalMadeEvent(TestData.MerchantId, TestData.EstateId, TestData.ManualWithdrawalId,
                                    TestData.WithdrawalDateTime.AddDays(1),
                                    TestData.WithdrawalAmount);

        public static IReadOnlyDictionary<String, String> DefaultAppSettings =>
            new Dictionary<String, String>
            {
                ["AppSettings:ClientId"] = "clientId",
                ["AppSettings:ClientSecret"] = "clientSecret",
                ["AppSettings:UseConnectionStringConfig"] = "false",
                ["EventStoreSettings:ConnectionString"] = "https://127.0.0.1:2113",
                ["SecurityConfiguration:Authority"] = "https://127.0.0.1",
                ["AppSettings:EstateManagementApi"] = "http://127.0.0.1",
                ["AppSettings:SecurityService"] = "http://127.0.0.1",
                ["ConnectionStrings:TransactionProcessorReadModel"] = "TransactionProcessorReadModel"
            };
    }
}