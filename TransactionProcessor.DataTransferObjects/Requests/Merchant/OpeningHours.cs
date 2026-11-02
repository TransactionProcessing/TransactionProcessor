namespace TransactionProcessor.DataTransferObjects.Requests.Merchant;

/// <summary>
/// Opening and closing time for one day.
/// </summary>
/// <example>{"opening":"example","closing":"example"}</example>
public class OpeningHours
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