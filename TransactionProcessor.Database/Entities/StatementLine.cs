using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities;

[Table("statementline")]
public class StatementLine
{
    public Guid StatementId { get; set; }

    public DateTime ActivityDateTime { get; set; }

    public DateTime ActivityDate { get; set; }

    public Int32 ActivityType { get; set; }

    public String? ActivityDescription { get; set; }

    public Decimal InAmount { get; set; }
    public Decimal OutAmount { get; set; }

    public Guid TransactionId { get; set; }
}