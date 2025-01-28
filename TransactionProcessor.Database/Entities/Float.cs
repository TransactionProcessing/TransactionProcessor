namespace TransactionProcessor.Database.Entities
{
    public class Float
    {
        public Guid FloatId { get; set; }

        public Guid EstateId { get; set; }

        public Guid ContractId { get; set; }

        public Guid ProductId { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
