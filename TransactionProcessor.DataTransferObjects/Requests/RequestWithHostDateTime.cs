using System;

namespace TransactionProcessor.DataTransferObjects.Requests;

/// <summary>
/// Base request fields shared by requests that include the host timestamp.
/// </summary>
public class RequestWithHostDateTime {
    /// <summary>
    /// Date and time on the host when the request was created or sent.
    /// </summary>
    public DateTimeOffset HostDateTime { get; set; } = DateTimeOffset.UtcNow;
}
