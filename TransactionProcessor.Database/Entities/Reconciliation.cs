using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    [Table("reconciliation")]
    public class Reconciliation
    {
        #region Properties

        public String? DeviceIdentifier { get; set; }
        
        public Boolean IsAuthorised { get; set; }

        public Boolean IsCompleted { get; set; }

        public Guid MerchantId { get; set; }

        public String? ResponseCode { get; set; }

        public String? ResponseMessage { get; set; }

        public Int32 TransactionCount { get; set; }

        public DateTime TransactionDate { get; set; }

        public DateTime TransactionDateTime { get; set; }

        public Guid TransactionId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 TransactionReportingId { get; set; }

        public TimeSpan TransactionTime { get; set; }

        public Decimal TransactionValue { get; set; }

        #endregion
    }
}