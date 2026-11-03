namespace TransactionProcessor.Database.Entities
{
    public class Float
    {
        public Guid FloatId { get; set; }

        public Guid EstateId { get; set; }
        
        public DateTimeOffset CreatedDateTime { get; set; }
        public DateOnly CreatedDate { get; set; }
    }
}
