using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Tests.Extensions
{
    using Common;
    using Shared.Exceptions;
    using Shouldly;
    using Xunit;

    public class ExtensionsTests
    {
        [Fact]
        public void GuidExtensions_ToDateTime_DateTimeIsReturned()
        {
            Guid guid = Guid.Parse("c150c000-7c92-08d9-0000-000000000000");
            DateTime dateTime = guid.ToDateTime();
            dateTime.ShouldBe(new DateTime(2021, 9, 21));
        }

        [Fact]
        public void DateExtensions_ToGuid_GuidIsReturned()
        {
            DateTime dateTime = new DateTime(2021, 9, 21);
            Guid guid = dateTime.ToGuid();
            guid.ShouldBe(Guid.Parse("c150c000-7c92-08d9-0000-000000000000"));
        }
    }
}
