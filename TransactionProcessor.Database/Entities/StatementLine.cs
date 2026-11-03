using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities;

[Table("statementline")]
public class StatementLine
{
    public Guid StatementId { get; set; }

    public DateTimeOffset ActivityDateTime { get; set; }

    public DateOnly ActivityDate { get; set; }

    public Int32 ActivityType { get; set; }

    public String? ActivityDescription { get; set; }

    public Decimal InAmount { get; set; }
    public Decimal OutAmount { get; set; }

    public Guid TransactionId { get; set; }
}