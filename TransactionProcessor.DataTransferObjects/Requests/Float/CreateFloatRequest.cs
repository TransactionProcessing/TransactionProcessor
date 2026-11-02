using System;

namespace TransactionProcessor.DataTransferObjects.Requests.Float;

/// <summary>
/// Float identifier and creation timestamp.
/// </summary>
/// <example>{"floatId":"00000000-0000-0000-0000-000000000166","createDateTime":"2026-01-15T10:30:00Z"}</example>
public class CreateFloatRequest{

    /// <summary>
    /// Identifier of the float.
    /// </summary>
    public Guid FloatId { get; set; }

    /// <summary>
    /// Date and time the float was created.
    /// </summary>
    public DateTimeOffset CreateDateTime { get; set; }
}