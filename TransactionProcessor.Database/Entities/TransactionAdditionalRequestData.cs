using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("transactionadditionalrequestdata")]
    public class TransactionAdditionalRequestData
    {
        public String? Amount { get; set; }


        public String? CustomerAccountNumber { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid TransactionId { get; set; }

        public String? Metadata { get; set; }
    }
}