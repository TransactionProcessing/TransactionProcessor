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
        
        public DateOnly ExpiryDate { get; set; }
        public DateTimeOffset ExpiryDateTime { get; set; }

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

        public DateTimeOffset GenerateDateTime { get; set; }

        public DateTimeOffset IssuedDateTime { get; set; }

        public DateTimeOffset RedeemedDateTime { get; set; }

        public DateOnly GenerateDate { get; set; }

        public DateOnly IssuedDate { get; set; }

        public DateOnly RedeemedDate { get; set; }
        [Timestamp]
        public Byte[] Timestamp { get; set; }
        public String Barcode { get; set; }

        #endregion
    }
}