using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    [Table("contract")]
    public class Contract
    {
        #region Properties
        
        public Guid ContractId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 ContractReportingId { get; set; }

        public Guid EstateId { get; set; }

        public Guid OperatorId { get; set; }

        public String Description { get; set; }
        #endregion
    }
}