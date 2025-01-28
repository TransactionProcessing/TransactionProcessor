using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("fileline")]
    public class FileLine
    {
        public string FileLineData { get; set; }

        public int LineNumber { get; set; }

        public string Status { get; set; } // Success/Failed/Ignored (maybe first char?)

        public Guid TransactionId { get; set; }

        public Guid FileId { get; set; }
    }
}