using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    [Table("settlement")]
    public class Settlement
    {
        #region Properties

        public Guid EstateId { get; set; }
        public Guid MerchantId { get; set; }

        public Boolean ProcessingStarted { get; set; }
        public DateTime ProcessingStartedDateTIme { get; set; }

        public Boolean IsCompleted { get; set; }

        public DateTime SettlementDate { get; set; }

        public Guid SettlementId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 SettlementReportingId { get; set; }

        #endregion
    }
}