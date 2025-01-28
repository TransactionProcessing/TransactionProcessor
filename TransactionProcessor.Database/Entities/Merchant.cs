using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("merchant")]
    public class Merchant
    {
        #region Properties

        public DateTime CreatedDateTime { get; set; }

        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 MerchantReportingId { get; set; }

        public String Name { get; set; }

        public Int32 SettlementSchedule { get; set; }

        public String? Reference { get; set; }

        public DateTime LastStatementGenerated { get; set; }

        public DateTime LastSaleDate { get; set; }

        public DateTime LastSaleDateTime { get; set; }

        #endregion
    }
}