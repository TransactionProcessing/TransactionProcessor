using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.Testing
{
    using BusinessLogic.Commands;
    using Models;

    public class TestData
    {
        public static ProcessLogonTransactionResponse ProcessLogonTransactionResponseModel = new ProcessLogonTransactionResponse
                                                                                             {
                                                                                                 ResponseMessage = TestData.ResponseMessage,
                                                                                                 ResponseCode = TestData.ResponseCode
                                                                                             };

        public static String ResponseMessage = "SUCCESS";

        public static Int32 ResponseCode= 0;

        public static Guid EstateId = Guid.Parse("A522FA27-F9D0-470A-A88D-325DED3B62EE");
        public static Guid MerchantId = Guid.Parse("833B5AAC-A5C5-46C2-A499-F2B4252B2942");

        public static ProcessLogonTransactionCommand ProcessLogonTransactionCommand = ProcessLogonTransactionCommand.Create(TestData.EstateId, TestData.MerchantId,
                                                                                                                         TestData.IMEINumber, TestData.TransactionType,
                                                                                                                         TestData.TransactionDateTime,
                                                                                                                         TestData.TransactionNumber);

        public static String IMEINumber = "1234567890";

        public static String TransactionType = "1000";

        public static DateTime TransactionDateTime = DateTime.Now;

        public static String TransactionNumber = "0001";
    }
}
