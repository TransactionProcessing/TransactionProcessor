using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.ProjectionEngine.Database.Database.Entities
{
    public class VoucherProjectionState{
        [Timestamp]
        public Byte[] Timestamp{ get; set; }
        public Guid EstateId{ get; set; }

        public Guid VoucherId{ get; set; }
        public String VoucherCode{ get; set; }
        public Guid TransactionId{ get; set; }
        public String Barcode{ get; set; }
    }
}
