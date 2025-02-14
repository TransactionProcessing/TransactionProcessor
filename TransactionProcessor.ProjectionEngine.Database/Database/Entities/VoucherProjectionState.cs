using System.ComponentModel.DataAnnotations;

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
