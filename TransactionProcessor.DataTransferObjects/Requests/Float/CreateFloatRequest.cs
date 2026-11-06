using System;
using System.ComponentModel.DataAnnotations;

namespace TransactionProcessor.DataTransferObjects.Requests.Float;

/// <summary>
/// Float identifier and creation timestamp.
/// </summary>
/// <example>{"operatorId":"00000000-0000-0000-0000-000000000166","createDateTime":"2026-01-15T10:30:00Z"}</example>
public class CreateFloatRequest{

    /// <summary>
    /// Identifier of the operator.
    /// </summary>
    [Required]
    public Guid OperatorId { get; set; }

    /// <summary>
    /// Date and time the float was created.
    /// </summary>
    public DateTimeOffset CreateDateTime { get; set; }
}