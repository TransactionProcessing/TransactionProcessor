using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("transaction")]
    public class Transaction
    {
        #region Properties
        
        public String? AuthorisationCode { get; set; }

        //public Int32 ContractReportingId { get; set; }
        public Guid ContractId { get; set; }

        public String? DeviceIdentifier { get; set; }
        
        public Boolean IsAuthorised { get; set; }
        
        public Boolean IsCompleted { get; set; }
        
        public Guid MerchantId { get; set; }

        public Guid OperatorId { get; set; }
        
        public Guid ContractProductId { get; set; }

        public String? ResponseCode { get; set; }
        
        public String? ResponseMessage { get; set; }
        
        public DateTime TransactionDate { get; set; }
        
        public DateTime TransactionDateTime { get; set; }
        
        public Guid TransactionId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 TransactionReportingId { get; set; }

        public String? TransactionNumber { get; set; }
        
        public String? TransactionReference { get; set; }
        
        public TimeSpan TransactionTime { get; set; }
        
        public String? TransactionType { get; set; }

        public Int32 TransactionSource { get; set; }

        public Decimal TransactionAmount { get; set; }

        #endregion
    }
}