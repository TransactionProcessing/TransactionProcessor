using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("fileimportlog")]
    public class FileImportLog
    {
        public Guid EstateId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 FileImportLogReportingId { get; set; }

        public Guid FileImportLogId { get; set; }

        public DateTime ImportLogDateTime { get; set; }

        public DateTime ImportLogDate { get; set; }
    }
}
