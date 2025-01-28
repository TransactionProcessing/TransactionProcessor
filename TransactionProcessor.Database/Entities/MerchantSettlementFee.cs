using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    [Table("merchantsettlementfee")]
    public class MerchantSettlementFee
    {
        #region Properties

        public Decimal CalculatedValue { get; set; }

        public DateTime FeeCalculatedDateTime { get; set; }

        public Guid ContractProductTransactionFeeId { get; set; }

        public Decimal FeeValue { get; set; }

        public Boolean IsSettled { get; set; }

        public Guid MerchantId { get; set; }

        public Guid SettlementId { get; set; }

        public Guid TransactionId { get; set; }

        #endregion
    }
}