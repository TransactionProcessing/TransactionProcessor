using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    [Table("statementheader")]
    public class StatementHeader
    {
        public Guid StatementId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 StatementReportingId { get; set; }

        public Guid MerchantId { get; set; }

        public DateTime StatementCreatedDate { get; set; }

        public DateTime StatementGeneratedDate { get; set; }

        public DateTime StatementCreatedDateTime { get; set; }

        public DateTime StatementGeneratedDateTime { get; set; }
    }
}

