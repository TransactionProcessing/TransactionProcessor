using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities;

[Table("merchantopeninghours")]
public class MerchantOpeningHours
{
    #region Properties

    public Guid MerchantId { get; set; }
        
    public String? MondayOpening { get; set; }
    public String? MondayClosing { get; set; }
    public String? TuesdayOpening { get; set; }
    public String? TuesdayClosing { get; set; }
    public String? WednesdayOpening { get; set; }
    public String? WednesdayClosing { get; set; }
    public String? ThursdayOpening { get; set; }
    public String? ThursdayClosing { get; set; }
    public String? FridayOpening { get; set; }
    public String? FridayClosing { get; set; }
    public String? SaturdayOpening { get; set; }
    public String? SaturdayClosing { get; set; }
    public String? SundayOpening { get; set; }
    public String? SundayClosing { get; set; }

    #endregion
}