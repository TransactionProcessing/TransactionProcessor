namespace TransactionProcessor.DataTransferObjects.Responses.Merchant;

/// <summary>Data transfer object for OpeningHoursResponse.</summary>
/// <example>{"opening":"example","closing":"example"}</example>

public class OpeningHoursResponse
{
    /// <summary>
    /// Local opening time for the day, represented as a string.
    /// </summary>
    public string Opening { get; set; }
    /// <summary>
    /// Local closing time for the day, represented as a string.
    /// </summary>
    public string Closing { get; set; }
}