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

        internal static String OperatorIdentifier = "Safaricom";

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

        #endregion
    }
}