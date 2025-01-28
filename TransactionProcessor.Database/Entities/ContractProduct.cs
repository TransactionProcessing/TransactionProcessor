using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    [Table("contractproduct")]
    public class ContractProduct
    {
        #region Properties

        public Guid ContractId { get; set; }

        public String DisplayText { get; set; }
        
        public Guid ContractProductId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 ContractProductReportingId { get; set; }

        public String ProductName { get; set; }
        
        public Decimal? Value { get; set; }

        public Int32 ProductType { get; set; }

        #endregion
    }
}