using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities;

[Table("transactiontimings")]
public class TransactionTimings {
    public Guid TransactionId { get; set; }
    public DateTimeOffset TransactionStartedDateTime { get; set; }
    public DateTimeOffset? OperatorCommunicationsStartedDateTime { get; set; }
    public DateTimeOffset? OperatorCommunicationsCompletedDateTime { get; set; }
    public DateTimeOffset TransactionCompletedDateTime { get; set; }
    public Double TotalTransactionInMilliseconds { get; set; }
    public Double OperatorCommunicationsDurationInMilliseconds { get; set; }
    public Double TransactionProcessingDurationInMilliseconds { get; set; }
}