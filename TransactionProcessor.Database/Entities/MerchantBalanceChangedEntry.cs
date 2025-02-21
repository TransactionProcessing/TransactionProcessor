namespace TransactionProcessor.ProjectionEngine.Database.Database.Entities;

public class MerchantBalanceChangedEntry{
    #region Properties

    public Guid AggregateId{ get; set; }
    public Guid CauseOfChangeId{ get; set; }
    public Decimal ChangeAmount{ get; set; }
    public DateTime DateTime{ get; set; }
    public String DebitOrCredit{ get; set; }
    public Guid EstateId{ get; set; }
    public Guid MerchantId{ get; set; }
    public Guid OriginalEventId{ get; set; }
    public String Reference{ get; set; }

    #endregion
}