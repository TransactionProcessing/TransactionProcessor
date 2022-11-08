using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.ProjectionEngine.Database.ViewEntities
{
    public class MerchantBalanceHistoryViewEntry
    {
        public Guid OriginalEventId { get; set; }

        public Decimal ChangeAmount { get; set; }

        public DateTime EntryDateTime { get; set; }

        public String Reference { get; set; }

        public String DebitOrCredit { get; set; }

        public Guid MerchantId { get; set; }

        public Decimal Balance { get; set; }
    }
}
