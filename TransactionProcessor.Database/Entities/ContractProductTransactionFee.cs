using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    [Table("contractproducttransactionfee")]
    public class ContractProductTransactionFee
    {
        #region Properties

        public Guid ContractProductId { get; set; }

        public Int32 CalculationType { get; set; }

        public Int32 FeeType { get; set; }

        public Boolean IsEnabled { get; set; }

        public Guid ContractProductTransactionFeeId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 ContractProductTransactionFeeReportingId { get; set; }

        public String Description { get; set; }

        public Decimal Value { get; set; }

        #endregion
    }
}