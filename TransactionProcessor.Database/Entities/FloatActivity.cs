namespace TransactionProcessor.Database.Entities;

public class FloatActivity{
    public Guid FloatId { get; set; }
    public Guid EventId { get; set; }
    public DateOnly ActivityDate { get; set; }
    public DateTimeOffset ActivityDateTime { get; set; }
    public String CreditOrDebit { get; set; }
    public Decimal Amount { get; set; }
    public Decimal CostPrice { get; set; }
}