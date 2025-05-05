using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities;

[Table("transactiontimings")]
public class TransactionTimings {
    public Guid TransactionId { get; set; }
    public DateTime TransactionStartedDateTime { get; set; }
    public DateTime? OperatorCommunicationsStartedDateTime { get; set; }
    public DateTime? OperatorCommunicationsCompletedDateTime { get; set; }
    public DateTime TransactionCompletedDateTime { get; set; }
    public Double TotalTransactionInMilliseconds { get; set; }
    public Double OperatorCommunicationsDurationInMilliseconds { get; set; }
    public Double TransactionProcessingDurationInMilliseconds { get; set; }
}