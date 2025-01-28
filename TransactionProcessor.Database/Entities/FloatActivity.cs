namespace TransactionProcessor.Database.Entities;

public class FloatActivity{
    public Guid FloatId { get; set; }
    public Guid EventId { get; set; }
    public DateTime ActivityDate { get; set; }
    public DateTime ActivityDateTime { get; set; }
    public String CreditOrDebit { get; set; }
    public Decimal Amount { get; set; }
    public Decimal CostPrice { get; set; }
}