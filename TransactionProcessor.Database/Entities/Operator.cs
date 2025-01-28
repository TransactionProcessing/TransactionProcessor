using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    [Table("operator")]
    public class Operator
    {
        #region Properties

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 OperatorReportingId { get; set; }

        public Guid EstateId { get; set; }

        public String Name { get; set; }

        public Guid OperatorId { get; set; }

        public Boolean RequireCustomMerchantNumber { get; set; }

        public Boolean RequireCustomTerminalNumber { get; set; }

        #endregion
    }
}