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

        public DateTimeOffset CreatedDateTime { get; set; }

        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 MerchantReportingId { get; set; }

        public String Name { get; set; }

        public Int32 SettlementSchedule { get; set; }

        public String? Reference { get; set; }

        public DateTimeOffset LastStatementGenerated { get; set; }

        public DateOnly LastSaleDate { get; set; }

        public DateTimeOffset LastSaleDateTime { get; set; }

        #endregion
    }
}