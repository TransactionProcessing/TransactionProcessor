using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("transactionadditionalresponsedata")]
    public class TransactionAdditionalResponseData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int32 TransactionReportingId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid TransactionId { get; set; }

        public String? Metadata { get; set; }
    }
}