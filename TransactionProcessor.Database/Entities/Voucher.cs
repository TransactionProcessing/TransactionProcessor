using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("voucherprojectionstate")]
    public class VoucherProjectionState
    {
        #region Properties
        
        public DateTime ExpiryDate { get; set; }
        public DateTime ExpiryDateTime { get; set; }

        public Boolean IsGenerated { get; set; }
        
        public Boolean IsIssued { get; set; }

        public Boolean IsRedeemed { get; set; }

        public String? OperatorIdentifier { get; set; }

        public String? RecipientEmail { get; set; }

        public String? RecipientMobile { get; set; }

        public Decimal Value { get; set; }

        public String VoucherCode { get; set; }

        public Guid VoucherId { get; set; }
        public Guid EstateId { get; set; }
        public Guid TransactionId { get; set; }

        public DateTime GenerateDateTime { get; set; }

        public DateTime IssuedDateTime { get; set; }

        public DateTime RedeemedDateTime { get; set; }

        public DateTime GenerateDate { get; set; }

        public DateTime IssuedDate { get; set; }

        public DateTime RedeemedDate { get; set; }
        [Timestamp]
        public Byte[] Timestamp { get; set; }
        public String Barcode { get; set; }

        #endregion
    }
}