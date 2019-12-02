﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.Testing
{
    using BusinessLogic.Commands;
    using Models;
    using TransactionAggregate;

    public class TestData
    {
        public static ProcessLogonTransactionResponse ProcessLogonTransactionResponseModel = new ProcessLogonTransactionResponse
                                                                                             {
                                                                                                 ResponseMessage = TestData.ResponseMessage,
                                                                                                 ResponseCode = TestData.ResponseCode
                                                                                             };

        public static String ResponseMessage = "SUCCESS";

        public static String ResponseCode= "0000";

        public static Guid EstateId = Guid.Parse("A522FA27-F9D0-470A-A88D-325DED3B62EE");
        public static Guid MerchantId = Guid.Parse("833B5AAC-A5C5-46C2-A499-F2B4252B2942");
        public static Guid TransactionId = Guid.Parse("AE89B2F6-307B-46F4-A8E7-CEF27097D766");

        public static ProcessLogonTransactionCommand ProcessLogonTransactionCommand = ProcessLogonTransactionCommand.Create( TestData.TransactionId, TestData.EstateId, TestData.MerchantId,
                                                                                                                         TestData.IMEINumber, TestData.TransactionType,
                                                                                                                         TestData.TransactionDateTime,
                                                                                                                         TestData.TransactionNumber);

        public static String IMEINumber = "1234567890";

        public static String TransactionType = "Logon";

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

            transactionAggregate.StartTransaction(TestData.TransactionDateTime,TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId,
                                                  TestData.IMEINumber);

            return transactionAggregate;
        }

        public static TransactionAggregate GetLocallyAuthorisedTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId,
                                                  TestData.IMEINumber);

            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            return transactionAggregate;
        }

        public static TransactionAggregate GetCompletedTransactionAggregate()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId,
                                                  TestData.IMEINumber);

            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            transactionAggregate.CompleteTransaction();

            return transactionAggregate;
        }
    }
}
