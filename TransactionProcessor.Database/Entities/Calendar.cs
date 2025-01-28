using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("calendar")]
    public class Calendar
    {
        #region Properties

        [Key]
        public DateTime Date { get; set; }

        public String DayOfWeek { get; set; }

        public Int32 DayOfWeekNumber { get; set; }

        public String DayOfWeekShort { get; set; }

        public String MonthNameLong { get; set; }

        public String MonthNameShort { get; set; }

        public Int32 MonthNumber { get; set; }

        public Int32? WeekNumber { get; set; }

        public String WeekNumberString { get; set; }

        public Int32 Year { get; set; }

        public String YearWeekNumber { get; set; }

        #endregion
    }
}