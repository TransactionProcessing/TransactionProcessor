using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.Tests.Commands
{
    using BusinessLogic.Commands;
    using Shouldly;
    using Testing;
    using Xunit;

    public class CommandTests
    {
        [Fact]
        public void ProcessLogonTransactionCommand_CanBeCreated_IsCreated()
        {
            ProcessLogonTransactionCommand processLogonTransactionCommand = ProcessLogonTransactionCommand.Create(TestData.EstateId, TestData.MerchantId, TestData.IMEINumber,TestData.TransactionType, TestData.TransactionDateTime,
                                                                                           TestData.TransactionNumber);

            processLogonTransactionCommand.ShouldNotBeNull();
            processLogonTransactionCommand.CommandId.ShouldNotBe(Guid.Empty);
            processLogonTransactionCommand.EstateId.ShouldBe(TestData.EstateId);
            processLogonTransactionCommand.MerchantId.ShouldBe(TestData.MerchantId);
            processLogonTransactionCommand.IMEINumber.ShouldBe(TestData.IMEINumber);
            processLogonTransactionCommand.TransactionType.ShouldBe(TestData.TransactionType);
            processLogonTransactionCommand.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            processLogonTransactionCommand.TransactionNumber.ShouldBe(TestData.TransactionNumber);
        }
    }
}
