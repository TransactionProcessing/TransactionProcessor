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

        public DateOnly StatementCreatedDate { get; set; }
               
        public DateOnly StatementGeneratedDate { get; set; }
               
        public DateTimeOffset StatementCreatedDateTime { get; set; }
               
        public DateTimeOffset StatementGeneratedDateTime { get; set; }
    }
}

