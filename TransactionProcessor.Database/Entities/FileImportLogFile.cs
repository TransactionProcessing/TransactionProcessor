using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Table("fileimportlogfile")]
    public class FileImportLogFile
    {
        public Guid FileId { get; set; }

        public Guid FileImportLogId { get; set; }

        public string FilePath { get; set; }

        public Guid FileProfileId { get; set; }

        public DateTime FileUploadedDateTime { get; set; }

        public DateTime FileUploadedDate { get; set; }

        public Guid MerchantId { get; set; }

        public string OriginalFileName { get; set; }
        
        public Guid UserId { get; set; }
    }
}