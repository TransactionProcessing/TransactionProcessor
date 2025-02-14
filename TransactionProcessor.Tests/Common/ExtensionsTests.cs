using System;
using System.Collections.Generic;

namespace TransactionProcessor.Tests.Common
{
    using BusinessLogic.Common;
    using Shouldly;
    using Xunit;

    public class ExtensionsTests
    {
        [Fact]
        public void ExtractFieldFromMetadata_Test() {
            Dictionary<String, String> metaData = new Dictionary<String, String>();
            metaData.Add("Amount","0.00");

            Decimal? g = metaData.ExtractFieldFromMetadata<Decimal?>("Amount");
            g.HasValue.ShouldBeTrue();
            g.Value.ShouldBe(0.00m);
        }

        [Fact]
        public void ExtractFieldFromMetadata_Test1()
        {
            Dictionary<String, String> metaData = new Dictionary<String, String>();
            

            Decimal? g = metaData.ExtractFieldFromMetadata<Decimal?>("Amount");
            g.HasValue.ShouldBeFalse();
        }
    }
}
