using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("transactionadditionalresponsedata")]
    public class TransactionAdditionalResponseData
    {
        #region Properties
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int32 TransactionReportingId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid TransactionId { get; set; }

        #endregion
    }
}