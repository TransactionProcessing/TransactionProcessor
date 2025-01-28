namespace TransactionProcessor.Database.Entities
{
    public class MerchantContract
    {
        public Guid MerchantId { get; set; }
        public Guid ContractId { get; set; }
        public Boolean IsDeleted{ get; set; }
    }
}
