namespace TransactionProcessor.DataTransferObjects.Requests.Merchant;

/// <summary>
/// Opening and closing times for each day of the merchant week.
/// </summary>
/// <example>{"sunday":{"opening":"example","closing":"example"},"monday":{"opening":"example","closing":"example"},"tuesday":{"opening":"example","closing":"example"},"wednesday":{"opening":"example","closing":"example"},"thursday":{"opening":"example","closing":"example"},"friday":{"opening":"example","closing":"example"},"saturday":{"opening":"example","closing":"example"}}</example>
public class MerchantOpeningRequest {
    /// <summary>
    /// Opening and closing times for Sunday.
    /// </summary>
    public OpeningHours Sunday { get; set; }
    /// <summary>
    /// Opening and closing times for Monday.
    /// </summary>
    public OpeningHours Monday { get; set; }
    /// <summary>
    /// Opening and closing times for Tuesday.
    /// </summary>
    public OpeningHours Tuesday { get; set; }
    /// <summary>
    /// Opening and closing times for Wednesday.
    /// </summary>
    public OpeningHours Wednesday { get; set; }
    /// <summary>
    /// Opening and closing times for Thursday.
    /// </summary>
    public OpeningHours Thursday { get; set; }
    /// <summary>
    /// Opening and closing times for Friday.
    /// </summary>
    public OpeningHours Friday { get; set; }
    /// <summary>
    /// Opening and closing times for Saturday.
    /// </summary>
    public OpeningHours Saturday { get; set; }
}