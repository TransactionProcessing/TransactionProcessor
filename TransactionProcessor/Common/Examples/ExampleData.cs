namespace TransactionProcessor.Common.Examples
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class ExampleData
    {
        #region Fields

        internal static Guid ContractId = Guid.Parse("64CB4CFC-F7DF-411C-9FB3-A631478B503C");
        
        internal static String CustomerEmailAddress = "exmaplecustomer@email.com";

        internal static String DeviceIdentifier = "exampledeviceidentifier1";

        internal static Guid EstateId = Guid.Parse("9B3D0726-D1FC-45CE-BBB4-18F83EB93F07");

        internal static String EstateIdMetadataName = "EstateId";

        internal static String LogonResponseCode = "0000";

        internal static String LogonResponseMessage = "SUCCESS";

        internal static Guid MerchantId = Guid.Parse("9B3D0726-D1FC-45CE-BBB4-18F83EB93F07");

        internal static String MerchantIdMetadataName = "MerchantId";

        internal static Guid OperatorId = Guid.Parse("70881EB5-2892-47FE-BE0B-B23E004C342F");

        internal static Guid ProductId = Guid.Parse("C0AEC683-587E-4F6A-BB20-B36312D9F712");

        internal static Int32 OnlineSaleTransactionSource = 1;

        internal static Int32 FileBasedSaleTransactionSource = 2;

        internal static String ReconciliationResponseCode = "0000";

        internal static String ReconciliationResponseMessage = "SUCCESS";

        internal static String SaleResponseCode = "0000";

        internal static String SaleResponseMessage = "SUCCESS";

        internal static Int32 TransactionCount = 1;

        internal static DateTime TransactionDateTime = DateTime.Now;

        internal static String TransactionNumber = "1";

        internal static String TransactionTypeLogon = "Logon";

        internal static String TransactionTypeSale = "Sale";

        internal static Decimal TransactionValue = 10.00m;

        internal static Guid TransactionId = Guid.Parse("612970B8-FDF1-4CAA-998A-D84632BD4DE0");

        internal static Boolean IsGenerated = true;
        
        internal static Boolean IsIssued = true;
        
        internal static Boolean IsRedeemed = true;
        
        internal static DateTime? IssuedDateTime = new DateTime(2021, 3, 6);
        
        internal static DateTime? GeneratedDateTime = new DateTime(2021, 3, 7);
        
        internal static DateTime? RedeemedDateTime = new DateTime(2021, 3, 7);

        internal static String RecipientMobile = "07777777777";
        
        internal static String RecipientEmail = "recipient@myvoucheremail.co.uk";
        
        internal static Decimal VoucherValue = 10.00m;
        
        internal static Decimal RemainingBalance = 0;
        
        internal static DateTime ExpiryDate = new DateTime(2021, 4, 6);
        
        internal static String Message = String.Empty;
        
        internal static String VoucherCode = "1234567890";
        
        internal static Guid VoucherId = Guid.Parse("AD3297AB-5484-4D5E-BBC2-B91815708920");

        #endregion
    }
}